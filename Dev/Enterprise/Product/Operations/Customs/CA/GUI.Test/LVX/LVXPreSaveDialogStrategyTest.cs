using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LVXPreSaveDialogStrategyTest : TestCaseWithFactory
	{
		public void TestRunPreSaveAction()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				CombineAssertions("Null Source", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var tester = new LVXPreSaveDialogStrategy(null);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
					AssertEquals(ContinueWithSave.No, tester.ShowPreSaveDialogs(ContinueWithSave.No));
				});

				CombineAssertions("Remission All", () =>
				{
					ZFormModaliser.LastFormShownDialogForTest = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lvxJob = Factory.New<JobDeclaration>();
					lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
					var invoice = lvxJob.LVXInvoiceHeader;
					invoice.JZ_ValuationDateOverride = ZDate.Today;
					invoice.CA_RN_NKExport = Core.Constants.CountryCodes.China;
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.CA_AuthorityNumber = "AUTHO";
					invoiceLine.CA_99TariffCode = "9960";

					var sima = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
					sima.C1_Override = false;
					sima.C1_Amount = 500m;
					sima.C1_ExemptCode = SIMACodes.Codes.C31;

					var duty = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
					duty.C1_Override = false;
					duty.C1_Amount = 300m;
					duty.C1_Rate = 6.5m;
					duty.C1_UnitOfMeasure = "AG";
					duty.C1_RateType = RateTypes.Codes.AdValorem;

					var tester = new LVXPreSaveDialogStrategy(lvxJob);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));

					var lastMessageBox = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox;
					AssertNotNull("Message box is shown", lastMessageBox);

					var expectedMessage = tester.RemissionAllMessage;
					AssertMultilineASCIIEquals("messageBox.Message", expectedMessage, lastMessageBox.MessageMultilingual);
					Assert(invoiceLine.DutiesAndTaxes.All(x => x.C1_Amount.IsEmpty));
					AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine.CA_RemissionType);
					AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
					AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber1, invoiceLine.CA_AuthorityNumber);
				});

				CombineAssertions("Remission All without message box", () =>
				{
					ZFormModaliser.LastFormShownDialogForTest = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lvxJob = Factory.New<JobDeclaration>();
					lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
					var invoice = lvxJob.LVXInvoiceHeader;
					invoice.JZ_ValuationDateOverride = ZDate.Today;
					invoice.CA_RN_NKExport = Core.Constants.CountryCodes.China;
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.CA_AuthorityNumber = DutyAndTaxManager.TaxRemittedOICNumber1;
					invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
					invoiceLine.CA_RemissionType = RemissionTypeList.Codes.OrderInCouncil;

					var sima = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
					sima.C1_Override = false;
					sima.C1_Amount = 500m;
					sima.C1_ExemptCode = SIMACodes.Codes.C31;

					var duty = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
					duty.C1_Override = false;
					duty.C1_Amount = 300m;
					duty.C1_Rate = 6.5m;
					duty.C1_UnitOfMeasure = "AG";
					duty.C1_RateType = RateTypes.Codes.AdValorem;

					var tester = new LVXPreSaveDialogStrategy(lvxJob);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));

					var lastMessageBox = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox;
					AssertNull("Message box was not shown.", lastMessageBox);
					Assert(invoiceLine.DutiesAndTaxes.All(x => x.C1_Amount.IsEmpty));
					AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine.CA_RemissionType);
					AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
					AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber1, invoiceLine.CA_AuthorityNumber);
				});

				CombineAssertions("Remission Mexico And US Duty And Tax", () =>
				{
					ZFormModaliser.LastFormShownDialogForTest = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lvxJob = Factory.New<JobDeclaration>();
					lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
					var invoice = lvxJob.LVXInvoiceHeader;
					invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
					invoice.JZ_ValuationDateOverride = ZDate.Today;
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.CA_AuthorityNumber = "AUTHO";
					invoiceLine.CA_99TariffCode = "9960";
					invoiceLine.CA_CustomsValue = 40m;

					var sima = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
					sima.C1_Override = false;
					sima.C1_Amount = 500m;
					sima.C1_ExemptCode = SIMACodes.Codes.C31;

					var duty = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
					duty.C1_Override = false;
					duty.C1_Amount = 300m;
					duty.C1_Rate = 6.5m;
					duty.C1_UnitOfMeasure = "AG";
					duty.C1_RateType = RateTypes.Codes.AdValorem;

					var tester = new LVXPreSaveDialogStrategy(lvxJob);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));

					var lastMessageBox = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox;
					AssertNotNull("Message box is shown", lastMessageBox);

					var expectedMessage = tester.RemissionMexicoAndUSDutyAndTaxMessage;
					AssertMultilineASCIIEquals("messageBox.Message", expectedMessage, lastMessageBox.MessageMultilingual);
					Assert(invoiceLine.DutiesAndTaxes.All(x => x.C1_Amount.IsEmpty));
					AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine.CA_RemissionType);
					AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
					AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber2, invoiceLine.CA_AuthorityNumber);
				});

				CombineAssertions("Remission Mexico And US Duty And Tax without message box", () =>
				{
					ZFormModaliser.LastFormShownDialogForTest = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lvxJob = Factory.New<JobDeclaration>();
					lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
					var invoice = lvxJob.LVXInvoiceHeader;
					invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
					invoice.JZ_ValuationDateOverride = ZDate.Today;
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.CA_AuthorityNumber = DutyAndTaxManager.TaxRemittedOICNumber2;
					invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
					invoiceLine.CA_RemissionType = RemissionTypeList.Codes.OrderInCouncil;
					invoiceLine.CA_CustomsValue = 40m;

					var sima = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
					sima.C1_Override = false;
					sima.C1_Amount = 500m;
					sima.C1_ExemptCode = SIMACodes.Codes.C31;

					var duty = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
					duty.C1_Override = false;
					duty.C1_Amount = 300m;
					duty.C1_Rate = 6.5m;
					duty.C1_UnitOfMeasure = "AG";
					duty.C1_RateType = RateTypes.Codes.AdValorem;

					var tester = new LVXPreSaveDialogStrategy(lvxJob);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));

					var lastMessageBox = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox;
					AssertNull("Message box was not shown.", lastMessageBox);
					Assert(invoiceLine.DutiesAndTaxes.All(x => x.C1_Amount.IsEmpty));
					AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine.CA_RemissionType);
					AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
					AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber2, invoiceLine.CA_AuthorityNumber);
				});

				CombineAssertions("Remission Mexico And US Duty Only", () =>
				{
					ZFormModaliser.LastFormShownDialogForTest = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lvxJob = Factory.New<JobDeclaration>();
					lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
					var invoice = lvxJob.LVXInvoiceHeader;
					invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
					invoice.JZ_ValuationDateOverride = ZDate.Today;
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.CA_AuthorityNumber = "AUTHO";
					invoiceLine.CA_99TariffCode = "9960";
					invoiceLine.CA_CustomsValue = 150m;

					var sima = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
					sima.C1_Override = false;
					sima.C1_Amount = 500m;
					sima.C1_ExemptCode = SIMACodes.Codes.C31;

					var duty = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
					duty.C1_Override = false;
					duty.C1_Amount = 300m;
					duty.C1_Rate = 6.5m;
					duty.C1_UnitOfMeasure = "AG";
					duty.C1_RateType = RateTypes.Codes.AdValorem;

					var gst = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
					gst.C1_Override = false;
					gst.C1_Amount = 300m;
					gst.C1_Rate = 6.5m;
					gst.C1_UnitOfMeasure = "AG";
					gst.C1_RateType = RateTypes.Codes.AdValorem;

					var tester = new LVXPreSaveDialogStrategy(lvxJob);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));

					var lastMessageBox = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox;
					AssertNotNull("Message box is shown", lastMessageBox);

					var expectedMessage = tester.RemissionMexicoAndUSDutyOnlyMessage;
					AssertMultilineASCIIEquals("messageBox.Message", expectedMessage, lastMessageBox.MessageMultilingual);
					Assert(invoiceLine.DutiesAndTaxes.Where(x => !x.IsGST).All(x => x.C1_Amount.IsEmpty));
					Assert(invoiceLine.DutiesAndTaxes.Where(x => x.IsGST).All(x => !x.C1_Amount.IsEmpty));
					AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine.CA_RemissionType);
					AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
					AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber3, invoiceLine.CA_AuthorityNumber);
				});

				CombineAssertions("Remission Mexico And US Duty Only without message box", () =>
				{
					ZFormModaliser.LastFormShownDialogForTest = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lvxJob = Factory.New<JobDeclaration>();
					lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
					var invoice = lvxJob.LVXInvoiceHeader;
					invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
					invoice.JZ_ValuationDateOverride = ZDate.Today;
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.CA_AuthorityNumber = DutyAndTaxManager.TaxRemittedOICNumber3;
					invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
					invoiceLine.CA_RemissionType = RemissionTypeList.Codes.OrderInCouncil;
					invoiceLine.CA_CustomsValue = 150m;

					var sima = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
					sima.C1_Override = false;
					sima.C1_Amount = 500m;
					sima.C1_ExemptCode = SIMACodes.Codes.C31;

					var duty = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
					duty.C1_Override = false;
					duty.C1_Amount = 300m;
					duty.C1_Rate = 6.5m;
					duty.C1_UnitOfMeasure = "AG";
					duty.C1_RateType = RateTypes.Codes.AdValorem;

					var gst = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
					gst.C1_Override = false;
					gst.C1_Amount = 300m;
					gst.C1_Rate = 6.5m;
					gst.C1_UnitOfMeasure = "AG";
					gst.C1_RateType = RateTypes.Codes.AdValorem;

					var tester = new LVXPreSaveDialogStrategy(lvxJob);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));

					var lastMessageBox = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox;
					AssertNull("Message box was not shown.", lastMessageBox);
					Assert(invoiceLine.DutiesAndTaxes.Where(x => !x.IsGST).All(x => x.C1_Amount.IsEmpty));
					Assert(invoiceLine.DutiesAndTaxes.Where(x => x.IsGST).All(x => !x.C1_Amount.IsEmpty));
					AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine.CA_RemissionType);
					AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
					AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber3, invoiceLine.CA_AuthorityNumber);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			Factory.Save();
			helper.CreateTaxOrFee("RT1", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 20.00m);
			helper.CreateTaxOrFee("RT2", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 40.00m);
			helper.CreateTaxOrFee("RT3", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 150.00m);
			Factory.Save();
		}
	}
}
