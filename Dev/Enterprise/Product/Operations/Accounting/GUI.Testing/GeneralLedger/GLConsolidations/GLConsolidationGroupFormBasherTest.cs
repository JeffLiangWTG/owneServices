using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLConsolidations.Testing
{
	[TestedType(typeof(GLConsolidationGroupForm))]
	internal sealed class GLConsolidationGroupFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new GLConsolidationGroupForm(Factory.New<AccConsolidationGroup>());
		}

		#endregion

		[TestDate(2025, 01, 01, 12, 00, 00)]
		public void TestCollectExportConsolidationGroupsUsageData()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery());

			var group = Factory.New<AccConsolidationGroup>();
			group.GroupMembers.AddNew().YM_OH_Organisation = org.PK;
			group.GroupMembers.AddNew().YM_GC_Company = company.PK;
			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			var jobRevenueJournalControlAccount = testObjectCreator.CreateAccGLHeader("6555.55.55", "AS", "Job Revenue Journal Control Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid()))
			using (var form = new GLConsolidationGroupForm(group))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var tabControl = (ZTemplateTabControl)form.Controls.Find("TabControl", true)[0];
				tabControl.SelectTab((ZTabPage)tabControl.Controls.Find("GroupDetailsTabPage", true)[0]);
				var button = (ZButton)form.Controls.Find("GenerateExportFilesButton", true)[0];
				button.PerformClick();

				var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
				AssertEquals("EDI message (messageType = USG) records should be 2", 2, ediMessages.Length);

				var jObjects = ediMessages.Select(msg => {
					var jsonObject = JObject.Parse(msg.EM_MessageTextDetail);
					return jsonObject;
				}).Where(jsonObject => jsonObject["FeatureCode"]?.Value<string>() == "CGG").ToList();

				AssertEquals("CGG", jObjects[0].Properties().FirstOrDefault(kp => kp.Name.Equals("FeatureCode", StringComparison.InvariantCulture))?.Value.ToString());
				AssertEquals("Accounting", jObjects[0].Properties().FirstOrDefault(kp => kp.Name.Equals("Module", StringComparison.InvariantCulture))?.Value.ToString());
				AssertEquals("Accounting General Ledger Consolidation Group Generate Export File", jObjects[0].Properties().FirstOrDefault(kp => kp.Name.Equals("FeatureDescription", StringComparison.InvariantCulture))?.Value.ToString());
				AssertEquals("EDIEDIDAT", jObjects.Properties().FirstOrDefault(kp => kp.Name.Equals("ConsolidationGroupClientID", StringComparison.InvariantCulture))?.Value.ToString());
			}
		}

		[TestDate(2025, 01, 01, 12, 00, 00)]
		public void TestCollectCreateEliminationJournalUsageData()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery());

			var group = Factory.New<AccConsolidationGroup>();
			group.GroupMembers.AddNew().YM_OH_Organisation = org.PK;
			group.GroupMembers.AddNew().YM_GC_Company = company.PK;
			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			var jobRevenueJournalControlAccount = testObjectCreator.CreateAccGLHeader("6555.55.55", "AS", "Job Revenue Journal Control Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid()))
			using (var form = new GLConsolidationGroupForm(group))
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var tabControl = (ZTemplateTabControl)form.Controls.Find("TabControl", true)[0];
				tabControl.SelectTab((ZTabPage)tabControl.Controls.Find("GroupDetailsTabPage", true)[0]);
				var button = (ZButton)form.Controls.Find("CreateEliminationJournalButton", true)[0];
				button.PerformClick();

				var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
				AssertEquals("EDI message (messageType = USG) records should be 2", 2, ediMessages.Length);

				var jObjects = ediMessages.Select(msg => {
					var jsonObject = JObject.Parse(msg.EM_MessageTextDetail);
					return jsonObject;
				}).Where(jsonObject => jsonObject["FeatureCode"]?.Value<string>() == "CGC").ToList();

				AssertEquals("CGC", jObjects[0].Properties().FirstOrDefault(kp => kp.Name.Equals("FeatureCode", StringComparison.InvariantCulture))?.Value.ToString());
				AssertEquals("Accounting", jObjects[0].Properties().FirstOrDefault(kp => kp.Name.Equals("Module", StringComparison.InvariantCulture))?.Value.ToString());
				AssertEquals("Accounting General Ledger Consolidation Group Create Elimination Journals", jObjects[0].Properties().FirstOrDefault(kp => kp.Name.Equals("FeatureDescription", StringComparison.InvariantCulture))?.Value.ToString());
				AssertEquals("EDIEDIDAT", jObjects.Properties().FirstOrDefault(kp => kp.Name.Equals("ConsolidationGroupClientID", StringComparison.InvariantCulture))?.Value.ToString());
			}
		}
	}
}
