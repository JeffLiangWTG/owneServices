using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	public class CusReconEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCharges()
		{
			var reconEntryLine = SetupCusReconEntryLine();
			var fileReader = new TestFileReader(typeof(CusReconEntryLineValidationTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "ImportEntryOrEntryLine_1.xml");
			var snapshot = reconEntryLine.CusReconSnapshots.AddNew();
			snapshot.CRS_SnapshotXml = messageText;

			#region assertions
			reconEntryLine.DutyToRefund = -100m;
			reconEntryLine.DutyPenaltyToRefund = -100m;
			reconEntryLine.LQTToRefund = -100m;
			reconEntryLine.LQTPenaltyToRefund = -100m;
			reconEntryLine.SCTToRefund = -100m;
			reconEntryLine.SCTPenaltyToRefund = -100m;
			reconEntryLine.TRTToRefund = -100m;
			reconEntryLine.TRTPenaltyToRefund = -100m;
			reconEntryLine.EDTToRefund = -100m;
			reconEntryLine.EDTPenaltyToRefund = -100m;
			reconEntryLine.AGTToRefund = -100m;
			reconEntryLine.AGTPenaltyToRefund = -100m;
			reconEntryLine.VATToRefund = -100m;
			reconEntryLine.VATPenaltyToRefund = -100m;
			reconEntryLine.PenaltyLateDecToRefund = -100m;
			reconEntryLine.PenaltyMissedDecToRefund = -100m;
			reconEntryLine.PenaltyLatePaymentToRefund = -100m;
			reconEntryLine.NonDutyTaxRevenueToRefund = -100m;
			AssertHasMessageErrorContaining(reconEntryLine.DutyToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.DutyPenaltyToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.LQTToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.LQTPenaltyToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.SCTToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.SCTPenaltyToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.TRTToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.TRTPenaltyToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.EDTToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.EDTPenaltyToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.AGTToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.AGTPenaltyToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.VATToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.VATPenaltyToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.PenaltyLateDecToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.PenaltyMissedDecToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.PenaltyLatePaymentToRefundInfo, "Please enter an amount greater than or equal to 0.");
			AssertHasMessageErrorContaining(reconEntryLine.NonDutyTaxRevenueToRefundInfo, "Please enter an amount greater than or equal to 0.");

			var paidAmounts = reconEntryLine.FirstSnapShot.ImportEntryOrEntryLine.PaidAmounts;
			paidAmounts.DutyAmount = 1000m;
			paidAmounts.LiquorTaxAmount = 1000m;
			paidAmounts.EducationTaxAmount = 1000m;
			paidAmounts.AgricultureTaxAmount = 1000m;
			paidAmounts.VATAmount = 1000m;
			paidAmounts.LateDeclarationPenalty = 1000m;
			paidAmounts.MissedDeclarationPenalty = 1000m;
			paidAmounts.LatePaymentPenalty = 1000m;
			paidAmounts.NonDutyTaxPayment = 1000m;

			reconEntryLine.DutyToRefund = 1001m;
			reconEntryLine.DutyPenaltyToRefund = 1001m;
			reconEntryLine.LQTToRefund = 1001m;
			reconEntryLine.LQTPenaltyToRefund = 1001m;
			reconEntryLine.SCTToRefund = 1001m;
			reconEntryLine.SCTPenaltyToRefund = 1001m;
			reconEntryLine.TRTToRefund = 1001m;
			reconEntryLine.TRTPenaltyToRefund = 1001m;
			reconEntryLine.EDTToRefund = 1001m;
			reconEntryLine.EDTPenaltyToRefund = 1001m;
			reconEntryLine.AGTToRefund = 1001m;
			reconEntryLine.AGTPenaltyToRefund = 1001m;
			reconEntryLine.VATToRefund = 1001m;
			reconEntryLine.VATPenaltyToRefund = 1001m;
			reconEntryLine.PenaltyLateDecToRefund = 1001m;
			reconEntryLine.PenaltyMissedDecToRefund = 1001m;
			reconEntryLine.PenaltyLatePaymentToRefund = 1001m;
			reconEntryLine.NonDutyTaxRevenueToRefund = 1001m;
			reconEntryLine.Validation.ValidatePenaltyLateDecToRefund();
			reconEntryLine.Validation.ValidatePenaltyMissedDecToRefund();
			reconEntryLine.Validation.ValidatePenaltyLatePaymentToRefund();

			AssertHasMessageErrorContaining(reconEntryLine.DutyToRefundInfo, "The refund amount must be less than or equal to the paid amount.");
			AssertNoMessageErrors(reconEntryLine.DutyPenaltyToRefundInfo);
			AssertHasMessageErrorContaining(reconEntryLine.LQTToRefundInfo, "The refund amount must be less than or equal to the paid amount.");
			AssertNoMessageErrors(reconEntryLine.LQTPenaltyToRefundInfo);
			AssertHasMessageErrorContaining(reconEntryLine.SCTToRefundInfo, "The refund amount must be less than or equal to the paid amount.");
			AssertNoMessageErrors(reconEntryLine.SCTPenaltyToRefundInfo);
			AssertHasMessageErrorContaining(reconEntryLine.TRTToRefundInfo, "The refund amount must be less than or equal to the paid amount.");
			AssertNoMessageErrors(reconEntryLine.TRTPenaltyToRefundInfo);
			AssertHasMessageErrorContaining(reconEntryLine.EDTToRefundInfo, "The refund amount must be less than or equal to the paid amount.");
			AssertNoMessageErrors(reconEntryLine.EDTPenaltyToRefundInfo);
			AssertHasMessageErrorContaining(reconEntryLine.AGTToRefundInfo, "The refund amount must be less than or equal to the paid amount.");
			AssertNoMessageErrors(reconEntryLine.AGTPenaltyToRefundInfo);
			AssertHasMessageErrorContaining(reconEntryLine.VATToRefundInfo, "The refund amount must be less than or equal to the paid amount.");
			AssertNoMessageErrors(reconEntryLine.VATPenaltyToRefundInfo);
			AssertHasMessageErrorContaining(reconEntryLine.PenaltyLateDecToRefundInfo, "The refund total amount of missed, late declaration and late payment penalties has exceeded the total penalty amount paid. Please check.");
			AssertHasMessageErrorContaining(reconEntryLine.PenaltyMissedDecToRefundInfo, "The refund total amount of missed, late declaration and late payment penalties has exceeded the total penalty amount paid. Please check.");
			AssertHasMessageErrorContaining(reconEntryLine.PenaltyLatePaymentToRefundInfo, "The refund total amount of missed, late declaration and late payment penalties has exceeded the total penalty amount paid. Please check.");
			AssertHasMessageErrorContaining(reconEntryLine.NonDutyTaxRevenueToRefundInfo, "The refund total amount of missed, late declaration and late payment penalties has exceeded the total penalty amount paid. Please check.");

			reconEntryLine.DutyToRefund = 500m;
			reconEntryLine.LQTToRefund = 500m;
			reconEntryLine.SCTToRefund = 500m;
			reconEntryLine.TRTToRefund = 500m;
			reconEntryLine.EDTToRefund = 500m;
			reconEntryLine.AGTToRefund = 500m;
			reconEntryLine.VATToRefund = 500m;
			reconEntryLine.PenaltyLateDecToRefund = 996m;
			reconEntryLine.Validation.ValidatePenaltyMissedDecToRefund();
			reconEntryLine.Validation.ValidatePenaltyLatePaymentToRefund();
			reconEntryLine.Validation.ValidateNonDutyTaxRevenueToRefund();

			AssertNoMessageErrors(reconEntryLine.DutyToRefundInfo);
			AssertNoMessageErrors(reconEntryLine.LQTToRefundInfo);
			AssertNoMessageErrors(reconEntryLine.SCTToRefundInfo);
			AssertNoMessageErrors(reconEntryLine.TRTToRefundInfo);
			AssertNoMessageErrors(reconEntryLine.EDTToRefundInfo);
			AssertNoMessageErrors(reconEntryLine.AGTToRefundInfo);
			AssertNoMessageErrors(reconEntryLine.VATToRefundInfo);
			AssertNoMessageErrors(reconEntryLine.PenaltyLateDecToRefundInfo);
			AssertNoMessageErrors(reconEntryLine.PenaltyMissedDecToRefundInfo);
			AssertNoMessageErrors(reconEntryLine.PenaltyLatePaymentToRefundInfo);
			AssertNoMessageErrors(reconEntryLine.NonDutyTaxRevenueToRefundInfo);
			#endregion
		}

		CusReconEntryLine SetupCusReconEntryLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var reconEntry = reconDeclaration.CusReconEntries.AddNew();
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			var reconEntryLine = (CusReconEntryLine)reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_CRE = reconEntry.PK;

			return reconEntryLine;
		}
		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
