using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class DEMonthlyClosingEntryLineSnapshotTest : TestCase
	{
		public void TestIsEmpty()
		{
			AssertEquals(true, snapshot.IsEmpty);
		}

		public void TestIsEmpty_AdditionalProcedureNotNull()
		{
			snapshot.AdditionalProcedure = new[] { new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure() };
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_AssessmentNotNull()
		{
			snapshot.Assessment = new DEMonthlyClosingEntryLineSnapshotAssessment();
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_BorderTransportMeansNotNull()
		{
			snapshot.BorderTransportMeans = new DEMonthlyClosingEntryLineSnapshotBorderTransportMeans();
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_CessionManagementFlagNotNull()
		{
			snapshot.CessionManagementFlag = "J";
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_CommodityCodeNotNull()
		{
			snapshot.CommodityCode = "1234";
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_DepartureCountryNotNull()
		{
			snapshot.DepartureCountry = "DE";
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_DocumentNotNull()
		{
			snapshot.Document = new[] { new DEMonthlyClosingEntryLineSnapshotDocument() };
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_ExciseDutyNotNull()
		{
			snapshot.ExciseDuty = new[] { new DEMonthlyClosingEntryLineSnapshotExciseDuty() };
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_ForeignTradeFlagNotNull()
		{
			snapshot.ForeignTradeFlag = "J";
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_ForeignTradeStatisticsNotNull()
		{
			snapshot.ForeignTradeStatistics = new DEMonthlyClosingEntryLineSnapshotForeignTradeStatistics();
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_InwardMovementAmountNotNull()
		{
			snapshot.InwardMovementAmount = new Amount();
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_NetMassMeasureSpecifiedTrue()
		{
			snapshot.NetMassMeasureSpecified = true;
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_OriginCountryNotNull()
		{
			snapshot.OriginCountry = "DE";
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_PreferentialCountryNotNull()
		{
			snapshot.PreferentialCountry = "DE";
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_PreferentialTreatmentNotNull()
		{
			snapshot.PreferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment();
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_SupplementaryCodesNotNull()
		{
			snapshot.SupplementaryCodes = new[] { new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes() };
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_SupplementaryInformationNotNull()
		{
			snapshot.SupplementaryInformation = "Test";
			AssertEquals(false, snapshot.IsEmpty);
		}

		public void TestIsEmpty_TobaccoRevenueStampNumberNotNull()
		{
			snapshot.TobaccoRevenueStampNumber = "X";
			AssertEquals(false, snapshot.IsEmpty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			snapshot = new DEMonthlyClosingEntryLineSnapshot();
		}
		DEMonthlyClosingEntryLineSnapshot snapshot;
	}
}
