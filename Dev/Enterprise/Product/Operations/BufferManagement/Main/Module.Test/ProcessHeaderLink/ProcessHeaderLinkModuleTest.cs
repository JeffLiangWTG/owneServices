using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ProcessHeaderLinkModule))]
	class ProcessHeaderLinkModuleTest : ZModuleBasherTest
	{
		public void TestShouldNotAllowNew()
		{
			using (var module = new ProcessHeaderLinkModule())
			{
				Assert(!module.AllowNew);
			}
		}

		public void TestShouldAllowDelete()
		{
			using (var module = new ProcessHeaderLinkModule())
			{
				Assert(module.AllowDelete);
			}
		}

		public void TestFilterControlColumns()
		{
			using (var module = new DummyProcessHeaderLinkModule())
			using (var processHeaderLinkFilterControl = (ProcessHeaderLinkFilterControl)module.GetNewFilterControl_ForTest())
			{
				var expectedColumnNames = new string[]
				{
				"FP_SystemCreateUser",
				"FP_SystemCreateTimeUtc",
				"FP_SystemLastEditUser",
				"FP_SystemLastEditTimeUtc",
				"StatusOfFromWorkflow",
				"HeaderFrom+ProviderJobDescription",
				"HeaderFrom+ParentJobDescription",
				"FP_FH_HeaderFrom",
				"HeaderTo+ProviderJobDescription",
				"HeaderTo+ParentJobDescription",
				"FP_FH_HeaderTo",
				"FP_LinkType",
				"LinkTypeDescription",
				"FP_TimeDelayFactor",
				"StaggeredReleaseTimeDelay",
				"FP_SynchroniseBufferPenetration",
				};

				var columnStyles = processHeaderLinkFilterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertArrayEqualsByElements(expectedColumnNames, columnStyles.Select(column => column.ColumnName).ToArray());

				var toJobColumnStyle = columnStyles.Single(x => x.ColumnName == "HeaderTo+ParentJobDescription");
				Assert("To Job column should hide", !toJobColumnStyle.IsVisible);

				var toJobDescriptionColumnStyle = columnStyles.Single(x => x.ColumnName == "HeaderTo+ProviderJobDescription");
				Assert("To Job Description column should hide", !toJobDescriptionColumnStyle.IsVisible);

				var toWorkflow = columnStyles.Single(x => x.ColumnName == "FP_FH_HeaderTo");
				Assert("To Workflow column should hide", !toWorkflow.IsVisible);
			}
		}

		[TestDate(2019, 12, 06, 11, 00, 00)]
		public void TestJobDescriptionDisplay_CanHandleNullValues()
		{
			var otherFactory = new BusinessObjectFactory();
			var system = BMSTestHelper.CreateSystem(otherFactory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var template = BMSTestHelper.CreateWorkflowTemplate(otherFactory, "ORG");

			var job = BMSTestHelper.CreateJob<OrgHeader>(otherFactory);
			var jobHeader = BMSTestHelper.CreateJobHeader(job);
			var header1 = MakeHeaderInNewJob(otherFactory, buffer, "Header1");
			var header2 = MakeHeaderInNewJob(otherFactory, buffer, "Header2");

			BMSTestHelper.CreateDependencyLink(template, header1, header2);
			otherFactory.Save();

			var sql = $@"UPDATE dbo.ProcessHeader
SET FH_ParentId = null, FH_ParentTableCode = ''
WHERE FH_PK = '{header2.PK.ToString()}'";
			Db.Connection.ExecuteNonQuery(sql);

			AssertNoExceptionThrown("We should be able to display processheaderlink rows with invalid values nicely", () =>
			{
				using (var module = new DummyProcessHeaderLinkModule())
				using (var form = (ZForm)module.ShowPopup())
				{
					form.Show();
					Application.DoEvents();

					var toolStrip = form.FindAll<ZFilterStripBaseControl>().First();
					toolStrip.Find();

					form.Show();
					Application.DoEvents();
				}
			});
		}

		[TestDate(2019, 12, 06, 11, 00, 00)]
		public void TestDbHits_ShouldNotLoadEveryJobOneAtATime()
		{
			var otherFactory = new BusinessObjectFactory();
			var system = BMSTestHelper.CreateSystem(otherFactory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var template = BMSTestHelper.CreateWorkflowTemplate(otherFactory, "ORG");

			ProcessHeader previousHeader = null;
			for (int i = 0; i < 50; i++)
			{
				var newHeader = MakeHeaderInNewJob(otherFactory, buffer, "Header" + i);

				if (previousHeader == null)
				{
					previousHeader = newHeader;
				}
				else
				{
					BMSTestHelper.CreateDependencyLink(template, previousHeader, newHeader);
				}
			}

			otherFactory.Save();

			var hits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 1 },
#if !WINZOR
				{ StmDataSchema.Constants.TableName, 1 },
#endif
			};

			using (var module = new DummyProcessHeaderLinkModule())
			using (var form = (ZForm)module.ShowPopup())
			{
				using (AssertDbHitsForAllFactories(hits, ignoreHitsFromTablesCachedInUberFactory: true, tablesToCollectQueriesFor: new[] { ProcessHeaderSchema.Constants.TableName }))
				{
					form.Show();
					Application.DoEvents();

					QueryStackTraceRecorder.Instance.Enabled = true;

					var toolStrip = form.FindAll<ZFilterStripBaseControl>().First();
					toolStrip.Find();

					form.Show();
					Application.DoEvents();

					Assert(true);
				}
			}
		}

		ProcessHeader MakeHeaderInNewJob(BusinessObjectFactory factory, BMComponent component, string name)
		{
			var job = BMSTestHelper.CreateJob<OrgHeader>(factory);
			var jobHeader = BMSTestHelper.CreateJobHeader(job);
			return BMSTestHelper.CreateProcessHeader(jobHeader, component, name, ZDateTime.Now);
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ProcessHeaderLink;
		}

		protected override BusinessObject GetBusinessObjectForHyperlinking(ZFilterGridModule module)
		{
			var link = (ProcessHeaderLink)base.GetBusinessObjectForHyperlinking(module);
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			return link;
		}

		#endregion
	}

	#region Dummy ProcessHeaderModule

	class DummyProcessHeaderLinkModule : ProcessHeaderLinkModule
	{
		public IFilterControl GetNewFilterControl_ForTest()
		{
			return this.GetNewFilterControl();
		}
	}

	#endregion
}
