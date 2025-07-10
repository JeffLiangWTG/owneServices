using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing.Reports
{
	[TestedType(typeof(GLReportingBooksReport))]
	public class GLReportingBooksReportTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GLReportingBooksReport;
		}

		public void TestGetNewEmbeddedControl()
		{
			using (var module = new GLReportingBooksReport())
			{
				var creator = new AccountingTestDataCreator();
				creator.CreatePeriods(202201, new ZDateTime(2022,01,01), new ZDateTime(2022, 02, 01), false);

				AddReport(module.BusinessContext, "menu");
				Factory.Save();

				using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
				using (AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2021, 01, 01)))
				using (var testForm = (ZForm)module.ShowPopup())
				{
					Assert("Form should be shown", testForm.Visible);

					var reportUserControl = testForm.Controls.OfType<ReportUserControl>().FirstOrDefault();
					AssertNotNull("Pre-condition: a ReportUserControl was found", reportUserControl);

					var commandCollection = reportUserControl.BindingSource.DataSource as ReportCommandCollection;

					AssertNotNull("There should have been a binding to this control", commandCollection);
					Assert("Rows should be visible", commandCollection.Count > 0);
				}
			}
		}

		public void TestGetNewEmbeddedControl_EmptyPeriod()
		{
			using (var module = new GLReportingBooksReport())
			{
				using (AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 01, 01)))
				using (var label = (ZLabel)module.EmbeddedControl)
				{
					AssertContains("Reporting book reports cannot be generated as journal entries for backlog accounting transactions have not been completely processed. Please check the Journal Entries Last Processed Date registry.", label.Text);
				}
			}
		}

		public void TestGetNewEmbeddedControl_LableText()
		{
			var creator = new AccountingTestDataCreator();
			creator.CreatePeriods(202201, new ZDateTime(2022, 01, 01), new ZDateTime(2022, 02, 01), false);

			using (var module = new GLReportingBooksReport())
			{
				using (var label = (ZLabel)module.EmbeddedControl)
				{
					AssertContains("Reporting book reports cannot be generated as journal entries for backlog accounting transactions have not been completely processed. Please check the Journal Entries Last Processed Date registry.", label.Text);
				}
			}

			using (AccountingUtils.TemporarilySetDataWarehouseServerToNull())
			using (AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2021, 01, 01)))
			using (var module = new GLReportingBooksReport())
			{
				using (var label = (ZLabel)module.EmbeddedControl)
				{
					AssertContains("Reporting book reports cannot be generated as the EET service task has not been run. Please check the Data Warehouse Server via System->BI->Data Warehouse Server.", label.Text);
				}
			}
		}

		void AddReport(string businessContext, string menuName = "")
		{
			var menuItem = Factory.New<ReportCommand>();
			menuItem.SU_BusinessContext = businessContext;
			menuItem.SU_GS_NKStaffCode = "";
			menuItem.SU_MenuName = menuName;
			menuItem.SU_DocumentDirection = "";
		}
	}
}
