using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CalculateTotalForIM2OrB3XB2StrategyTest : TestCaseWithFactory
	{
		public void TestRunPreSaveAction()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				CombineAssertions("Null Source", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var tester = new CalculateTotalForIM2OrB3XB2Strategy(null);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
					AssertEquals(ContinueWithSave.No, tester.ShowPreSaveDialogs(ContinueWithSave.No));
				});

				CombineAssertions("Test B2 Total", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var b2 = Factory.New<JobDeclaration>();
					b2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
					b2.CA_B2Type = B2TypeList.Codes.Specific;
					var header = b2.B2AsAccountedForInvoices.AddNew();
					header.JZ_InvoiceNumber = "INV1";
					header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
					var line = header.AsAccountForFilteredInvoiceLines.AddNew();
					line.CA_OriginalLineNo = "1";
					line.JI_Description = "SOME DESCRIPTION";
					line.CA_AuthorityNumber = "AUTHO";
					line.JI_Tariff = "2402.10.00 10";
					line.CA_99TariffCode = "9960";
					line.JI_CustomsQuantity = 5m;
					line.JI_CustomsUnitQty = "MIL";
					line.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsSimilarGoods;
					line.CA_CVforCurrConv = 1000m;

					var sima = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
					sima.C1_Override = true;
					sima.C1_Amount = 500m;
					sima.C1_ExemptCode = SIMACodes.Codes.C31;

					var excise = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
					excise.C1_Override = true;
					excise.C1_Amount = 50m;
					excise.C1_Rate = 5.0m;
					excise.C1_RateType = RateTypes.Codes.Specific;

					var duty = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
					duty.C1_Override = true;
					duty.C1_Amount = 300m;
					duty.C1_Rate = 6.5m;
					duty.C1_UnitOfMeasure = "AG";
					duty.C1_RateType = RateTypes.Codes.AdValorem;

					var gst = line.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
					gst.C1_Override = true;
					gst.C1_Amount = 60m;
					gst.C1_Rate = 5m;
					gst.C1_RateType = RateTypes.Codes.AdValorem;

					var laimLine = line.CorrespondingAsClaimedForInvoiceLine;
					laimLine.DutiesAndTaxes.DeleteAll();
					var sima2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
					sima2.C1_Override = true;
					sima2.C1_Amount = 400m;
					sima2.C1_ExemptCode = SIMACodes.Codes.C31;

					var excise2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
					excise2.C1_Override = true;
					excise2.C1_Amount = 40m;
					excise2.C1_Rate = 5.0m;
					excise2.C1_RateType = RateTypes.Codes.Specific;

					var duty2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
					duty2.C1_Override = true;
					duty2.C1_Amount = 200m;
					duty2.C1_Rate = 6.5m;
					duty2.C1_UnitOfMeasure = "AG";
					duty2.C1_RateType = RateTypes.Codes.AdValorem;

					var gst2 = laimLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
					gst2.C1_Override = true;
					gst2.C1_Amount = 70m;
					gst2.C1_Rate = 5m;
					gst2.C1_RateType = RateTypes.Codes.AdValorem;

					var tester = new CalculateTotalForIM2OrB3XB2Strategy(b2);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));

					var subrefund = 400m + 40 + 200 - 500 - 50 - 300;
					var gstrefund = 70m - 60;
					AssertEquals(b2.CA_B2Total, subrefund + gstrefund);

					gst2.C1_Amount = 50m;
					tester = new CalculateTotalForIM2OrB3XB2Strategy(b2);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
					AssertEquals(b2.CA_B2Total, subrefund);

					sima2.C1_ExemptCode = SIMACodes.Codes.C32;
					sima.C1_ExemptCode = SIMACodes.Codes.C32;
					tester = new CalculateTotalForIM2OrB3XB2Strategy(b2);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
					subrefund = 40 + 200 - 50 - 300;
					AssertEquals(b2.CA_B2Total, subrefund);

					b2.CA_B2Total = 9999999999999.99m;
					b2.AddInfoValidation.ValidateCA_B2Total();
					AssertNoError(b2.CA_B2TotalInfo, "The number 10,000,000,000,000.00 is too large, the maximum value allowed for selection is 9,999,999,999,999.99.");

					b2.CA_B2Total = 10000000000000.00m;
					b2.AddInfoValidation.ValidateCA_B2Total();
					AssertHasError(b2.CA_B2TotalInfo, "The number 10,000,000,000,000.00 is too large, the maximum value allowed for selection is 9,999,999,999,999.99.");
				});

				CombineAssertions("Test IM2 Total", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.CA_MergeBy = "TRF";
					declaration.CA_B2Type = B2TypeList.Codes.Specific;
					declaration.CA_IsDocAttached = true;
					declaration.CA_JustificationForRequest = "JUSTIFICATION FOR REQUEST";
					declaration.CA_Under = "UNDER";
					declaration.CA_B2Explanation = "EXPLANATION";
					declaration.CA_ClaimedInterestAmount = 11m;
					declaration.CA_AnySightDepositAmount = 12m;
					var company = Factory.New<GlbCompany>();
					company.GC_Name = "MY COMPANY";
					var branch = company.Branches.AddNew();
					branch.GB_BranchName = "MY BRANCH";
					var staff = Factory.New<GlbStaff>();
					staff.GS_Code = "XXX";
					staff.GS_WorkPhone = "MY PHONE";
					staff.GS_FullName = "MY NAME";
					staff.GS_GB_HomeBranch = branch.PK;
					declaration.JE_GS_NKCusAgent = staff.GS_Code;

					var header = declaration.Invoices.AddNew();
					header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
					var line1 = header.JobComInvoiceLines.AddNew();
					line1.JI_LineNo = 1;
					line1.CA_CVforCurrConv = 1000m;
					line1.JI_Tariff = "0000000001";
					var sima = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
					sima.C1_ExemptCode = SIMACodes.Codes.C51;
					sima.C1_Override = true;
					sima.C1_Amount = 500m;
					var duty = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
					duty.C1_Override = true;
					duty.C1_Amount = 50m;
					var excise = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
					excise.C1_Override = true;
					excise.C1_Amount = 300m;
					var gst = line1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
					gst.C1_Override = true;
					gst.C1_Amount = 60m;

					declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
					declaration.DoMerge();
					Factory.Save();

					var im2 = declaration.GetNewCopyToB2Declaration();
					var line2 = im2.InvoiceLines[0];
					line2.JI_Tariff = "0000000002";
					var sima2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
					sima2.C1_Amount = 600;
					var duty2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty);
					duty2.C1_Amount = 60;
					var excise2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
					excise2.C1_Amount = 400m;
					var gst2 = line2.DutiesAndTaxes.First(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.GST);
					gst2.C1_Amount = 70m;
					var tester = new CalculateTotalForIM2OrB3XB2Strategy(im2);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));

					Factory.Save();
					var subrefund = 600m + 60 + 400 - 500 - 50 - 300;
					var gstrefund = 70m - 60;
					AssertEquals(subrefund + gstrefund - declaration.CA_AnySightDepositAmount, im2.CA_B2Total);
					gst2.C1_Amount = 50m;
					tester = new CalculateTotalForIM2OrB3XB2Strategy(im2);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
					AssertEquals(subrefund - declaration.CA_AnySightDepositAmount, im2.CA_B2Total);

					sima.C1_ExemptCode = SIMACodes.Codes.C32;
					sima2.C1_ExemptCode = SIMACodes.Codes.C32;
					tester = new CalculateTotalForIM2OrB3XB2Strategy(im2);
					AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
					subrefund = 60 + 400 - 50 - 300;
					AssertEquals(im2.CA_B2Total, subrefund - declaration.CA_AnySightDepositAmount);

					im2.CA_B2Total = 9999999999999.99m;
					im2.AddInfoValidation.ValidateCA_B2Total();
					AssertNoError(im2.CA_B2TotalInfo, "The number 10,000,000,000,000.00 is too large, the maximum value allowed for selection is 9,999,999,999,999.99.");

					im2.CA_B2Total = 10000000000000.00m;
					im2.AddInfoValidation.ValidateCA_B2Total();
					AssertHasError(im2.CA_B2TotalInfo, "The number 10,000,000,000,000.00 is too large, the maximum value allowed for selection is 9,999,999,999,999.99.");
				});
			}
		}
	}
}
