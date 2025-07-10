using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceReferenceForm))]
	public class OverrideInvoiceReferenceFormBasherTest : OverrideInvoiceDetailsFormTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new OverrideInvoiceReferenceForm(new OverrideInvoiceReferenceHelper(Factory, Factory.NewWithValidTestData<APInvoice>().PK));
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		public void TestUpperPanelAndRegistryMessage()
		{
			var security = new Security.SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
			security.PayablesModifyInvoiceRemittanceReference.IsAllowed = true;
			security.PayablesModifyInvoiceDateNumOrSupplierCostRef.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var testForm = GetFormToBashCore())
			{
				testForm.Show();

				var formType = (typeof(OverrideInvoiceDetailsForm));
				var upperPanelField = formType.GetField("UpperPanel", BindingFlags.NonPublic | BindingFlags.Instance);

				var upperPanel = (ZPanel)upperPanelField.GetValue(testForm);

				AssertEquals("Upper Panel is hidden", false, upperPanel.Visible);
			}

			security.PayablesModifyInvoiceRemittanceReference.IsAllowed = false;
			security.PayablesModifyInvoiceDateNumOrSupplierCostRef.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var testForm = GetFormToBashCore())
			{
				testForm.Show();

				var formType = (typeof(OverrideInvoiceDetailsForm));
				var upperPanelField = formType.GetField("UpperPanel", BindingFlags.NonPublic | BindingFlags.Instance);
				var upperLabelRegistryMessageField = formType.GetField("UpperLabelRegistryMessage", BindingFlags.NonPublic | BindingFlags.Instance);

				var upperPanel = (ZPanel)upperPanelField.GetValue(testForm);
				var upperLabelRegistryMessage = (ZLabel)upperLabelRegistryMessageField.GetValue(testForm);

				AssertEquals("Upper Panel is visible", true, upperPanel.Visible);
				AssertEquals("Upper Label contains message for Invoice Remittance Reference Registry",
					"To override Invoice Remittance Reference, the following security right is required: Manage > Payables > Payables Transactions > Modify Invoice Remittance Reference",
					upperLabelRegistryMessage.CaptionResourceString.Caption);
			}

			security.PayablesModifyInvoiceRemittanceReference.IsAllowed = true;
			security.PayablesModifyInvoiceDateNumOrSupplierCostRef.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (var testForm = GetFormToBashCore())
			{
				testForm.Show();

				var formType = (typeof(OverrideInvoiceDetailsForm));
				var upperPanelField = formType.GetField("UpperPanel", BindingFlags.NonPublic | BindingFlags.Instance);
				var upperLabelRegistryMessageField = formType.GetField("UpperLabelRegistryMessage", BindingFlags.NonPublic | BindingFlags.Instance);

				var upperPanel = (ZPanel)upperPanelField.GetValue(testForm);
				var upperLabelRegistryMessage = (ZLabel)upperLabelRegistryMessageField.GetValue(testForm);

				AssertEquals("Upper Panel is visible", true, upperPanel.Visible);
				AssertEquals("Upper Label contains message for Invoice Date, Num and Cost. Ref.",
					"To override Invoice Date, Invoice Number and Supplier Cost Reference, the following security right is required: Manage > Payables > Payables Transactions > Modify Invoice Date, Invoice Number and Supplier Cost Reference",
					upperLabelRegistryMessage.CaptionResourceString.Caption);
			}
		}

		protected override void AssertFormDisplayMode(OverrideInvoiceDetailsForm testForm)
		{
			AssertNotEquals("DisplayMode is not savedNew", ODisplayMode.NewSaved, testForm.DisplayMode);
			AssertEquals("DisplayMode is Edit", ODisplayMode.Edit, testForm.DisplayMode);
		}

		protected override void AssertButtonVisibility(ZButton closeButton, ZButton continueButton, ZPostingButtonsUserControl postingUserControl)
		{
			Assert(!closeButton.Visible);
			Assert(!continueButton.Visible);
			Assert(postingUserControl.Visible);
		}

		#endregion

		[TestDate(2022, 11, 15)]
		public void TestCheckAH_PostOrInvoiceDateForCompliance()
		{
			var now = ZDateTime.Now;
			var company = GlbCompany.CurrentCompany;
			var companyPK = company.PK.ToGuid();

			new AccountingPeriodTestHelper().SetupPeriods();

			var regMaster = AccountingMasterFilesRegistry.Instance;
			using (company.TemporarilySetCountry(CountryCodes.Italy))
			using (regMaster.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (regMaster.ComplianceNumberAllocationDate_AP.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				var testObjectCreator = new TestObjectCreator(Factory);

				var subType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = testObjectCreator.SetupComplianceSequence(menuPK, subType, subType + ".20-", 1, 100, 2);
				sequence.XD_StartDate = new ZDate(now.Year, now.Month, 1);
				sequence.XD_ExpiryDate = sequence.XD_StartDate.AddMonths(1).AddDays(-1);
				sequence.XD_IsActive = true;
				Factory.Save();

				var invoice = testObjectCreator.CreateAPInvoice<APInvoice>("INV001", testObjectCreator.EUR, 1, 100, 10, 0, 100, 10, 0, testObjectCreator.AALSHI);
				invoice.AH_GC = sequence.XD_GC_Company;
				invoice.AH_GB = sequence.XD_GB_BranchOwner;
				invoice.AH_ComplianceSubType = sequence.XD_SequenceClass;
				AssertNoRowErrors(invoice);
				Factory.Save();

				var originalComplianceNumber = invoice.AH_TransactionReference;
				AssertNotNullOrEmpty(originalComplianceNumber);

				var newFactory = new BusinessObjectFactory();
				sequence = newFactory.Load<AccComplianceSequence>(sequence.PK);
				newFactory.Save();

				using (var testForm = new OverrideInvoiceReferenceForm(new OverrideInvoiceReferenceHelper(Factory, invoice.PK)))
				{
					testForm.Show();
					invoice.AH_TransactionNum = "INV002";
					var saveResult = testForm.FireSaveButton();
					AssertEquals(ContinueWithSave.Yes, saveResult);

					AssertEquals(originalComplianceNumber, invoice.AH_TransactionReference);
				}
			}
		}
	}
}
