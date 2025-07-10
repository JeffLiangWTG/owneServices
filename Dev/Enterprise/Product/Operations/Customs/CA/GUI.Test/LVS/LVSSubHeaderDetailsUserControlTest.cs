using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LVSSubHeaderDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestReadyForConsolidationCheckBoxClickWhenLVSIsNotSaved()
		{
			AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));

			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var lvxInvoice = testDeclaration.LVXInvoiceHeader;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TSTORG";
			lvxInvoice.JZ_OH_Buyer = org.PK;
			var expectedMessage = @"The Job has not yet been saved. Do you want to save and proceed?";

			using (var control = new LVSSubHeaderDetailsUserControl())
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				control.SetDataBinding(testDeclaration, "");
				control.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.IsReadyForConsolidationCheckBox.Checked = true;
				AssertNoExceptionThrown(() =>
				{
					control.IsReadyForConsolidationCheckBox_Click(this, EventArgs.Empty);
				});
				AssertMultilineASCIIEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReadyForConsolidationCheckBoxClick()
		{
			AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			collection.Add(CreateNewSettings(1, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));

			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = ZBool.True;
			org.CompanyData.OB_AROnCreditHold = ZBool.True;

			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var lvxInvoice = testDeclaration.LVXInvoiceHeader;
			lvxInvoice.JZ_OH_Buyer = org.PK;
			testDeclaration.Importer.MiscServ.OM_ARCreditLimit = -1M;
			var expectedMessage = @"CLVS job cannot be added to F-Type job because:
       The Importer, Supplier, Local Client for Billing or any Debtors in associated Shipment
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and proceed?";

			Factory.Save();
			using (var control = new LVSSubHeaderDetailsUserControl())
			using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				control.SetDataBinding(testDeclaration, "");
				control.Show();
				lvxInvoice.Buyer.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.IsReadyForConsolidationCheckBox.Checked = true;
				control.IsReadyForConsolidationCheckBox_Click(this, EventArgs.Empty);
				AssertMultilineASCIIEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!control.IsReadyForConsolidationCheckBox.Checked);

				using (CACustomsDataRegistry.Instance.EnableCreditCheckForCLVS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					lvxInvoice.Buyer.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					control.IsReadyForConsolidationCheckBox.Checked = true;
					control.IsReadyForConsolidationCheckBox_Click(this, EventArgs.Empty);
					AssertMultilineASCIIEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(control.IsReadyForConsolidationCheckBox.Checked);
				}

				using (CACustomsDataRegistry.Instance.EnableCreditCheckForCLVS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					lvxInvoice.Buyer.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					control.IsReadyForConsolidationCheckBox.Checked = true;
					control.IsReadyForConsolidationCheckBox_Click(this, EventArgs.Empty);
					AssertMultilineASCIIEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(control.IsReadyForConsolidationCheckBox.Checked);
				}
			}
		}

		public void TestIsReadyForConsolidationCheckBoxVisibility()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice = testDeclaration.Invoices.AddNew();
			using (var control = new LVSSubHeaderDetailsUserControl())
			{
				control.SetDataBinding(testDeclaration, "");
				control.Show();
				Assert("Invisible for LVS", !control.IsReadyForConsolidationCheckBox.Visible);
			}

			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			using (var control = new LVSSubHeaderDetailsUserControl())
			{
				control.SetDataBinding(testDeclaration, "");
				control.Show();
				Assert("Visible for LVX", control.IsReadyForConsolidationCheckBox.Visible);
			}
		}

		AmountOrPercentageBasedThreeLevelAuthorisationRequirement CreateNewSettings(ZDecimal amount, ZDecimal percentage, ZString range, ZString auth)
		{
			var settings = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;

			return settings;
		}
	}
}
