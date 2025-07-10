using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class SeaOutturnReportLineInformationAbstractTest : CusOutturnOutturnReportLineInformationAbstractTest
	{
		public void TestSealIntactIndicator()
		{
			Outturn.C5_SealIntactIndicator = false;
			AssertEquals("SealIntactIndicator", false, ReportInfo.SealIntactIndicator);
		}

		public void TestVesselDischargeUnderbondIndicator()
		{
			AssertEquals("VesselDischargeUnderbondIndicator", false, ReportInfo.VesselDischargeUnderbondIndicator);
		}

		public void TestUnpackIndicator()
		{
			Outturn.C5_ReceiptOnlyIndicator = true;
			AssertEquals("UnpackIndicator", false, ReportInfo.UnpackIndicator);
		}

		[TestDate(2005, 04, 04)]
		public void TestDateTimeOfCargoReceiptUnloadUTC()
		{
			Underbond.C4_DateOfArrivalIntoDestinationPremise = new ZDateTime(2005, 6, 21, 11, 22, 0);
			AssertEquals("UTC DateTimeOfCargoReceiptUnload", new ZDateTime(2005, 6, 21, 1, 22, 0), ReportInfo.DateTimeOfCargoReceiptUnload);
		}

		[TestDate(2020, 09, 16, 09, 17, 00)]
		public void TestUTCDateTimesChangeWithPremisesInDifferentTimeZones()
		{
			var perthBranch = CreateBranch("PER", "Perth Branch", "AUPER");
			var sydneyBranch = CreateBranch("SYD", "Sydney Branch", "AUSYD");

			var perthPremise = Factory.NewWithValidTestData<OrgHeader>();
			perthPremise.OH_RL_NKClosestPort = "AUPER";

			var sydneyPremise = Factory.NewWithValidTestData<OrgHeader>();
			sydneyPremise.OH_RL_NKClosestPort = "AUSYD";
			var adelaidePremise = sydneyPremise.Addresses.AddNew();
			adelaidePremise.Address1 = "100 Port Road";
			adelaidePremise.Address2 = "Port Adelaide";
			adelaidePremise.OA_RL_NKRelatedPortCode = "AUADL";

			var header = Factory.New<CusOutturnHeader>();
			header.Underbonds.Add(Underbond);

			Factory.Save();

			var testDate = ZDateTime.Now;
			Underbond.C4_DateOfArrivalIntoDestinationPremise = testDate;
			Underbond.C4_Outurned = testDate;

			// test for fallback branch first:
			var expectedUtcDateForPerth = testDate.AddHours(-8);
			var expectedUtcDateForSydney = testDate.AddHours(-10);

			using (Environment.DisposableEnvironment.ForBranch(perthBranch.PK.ToGuid()))
			{
				AssertEquals("UTC DateTimeOfCargoReceiptUnload for Perth branch", expectedUtcDateForPerth, ReportInfo.DateTimeOfCargoReceiptUnload);
				AssertEquals("UTC DateTimeOfOutturn for Perth branch", expectedUtcDateForPerth, ReportInfo.DateTimeOfOutturn);
			}

			using (Environment.DisposableEnvironment.ForBranch(sydneyBranch.PK.ToGuid()))
			{
				AssertEquals("UTC DateTimeOfCargoReceiptUnload for Sydney branch", expectedUtcDateForSydney, ReportInfo.DateTimeOfCargoReceiptUnload);
				AssertEquals("UTC DateTimeOfOutturn for Sydney branch", expectedUtcDateForSydney, ReportInfo.DateTimeOfOutturn);
			}

			// test for appropriate premise id locode:
			Underbond.Outturns.Add(Outturn);
			Outturn.C5_C6 = header.PK;
			Underbond.Header.C6_OA_OutturningPremise = perthPremise.MainAddress.PK;
			AssertEquals("UTC DateTimeOfCargoReceiptUnload for Premise in Perth", expectedUtcDateForPerth, ReportInfo.DateTimeOfCargoReceiptUnload);
			AssertEquals("UTC DateTimeOfOutturn for Premise in Perth", expectedUtcDateForPerth, ReportInfo.DateTimeOfOutturn);

			Underbond.Header.C6_OA_OutturningPremise = sydneyPremise.MainAddress.PK;
			AssertEquals("UTC DateTimeOfCargoReceiptUnload for Premise in Sydney", expectedUtcDateForSydney, ReportInfo.DateTimeOfCargoReceiptUnload);
			AssertEquals("UTC DateTimeOfOutturn for Premise in Sydney", expectedUtcDateForSydney, ReportInfo.DateTimeOfOutturn);

			// test org address premise id locode:
			using (Environment.DisposableEnvironment.ForBranch(sydneyBranch.PK.ToGuid()))
			{
				var expectedUtcDateForAdelaide = testDate.AddHours(-9).AddMinutes(-30);
				Underbond.Header.C6_OA_OutturningPremise = adelaidePremise.PK;
				AssertEquals("UTC DateTimeOfCargoReceiptUnload for Premise in Adelaide even though user logged in to Sydney branch", expectedUtcDateForAdelaide, ReportInfo.DateTimeOfCargoReceiptUnload);
				AssertEquals("UTC DateTimeOfOutturn for Premise in Adelaide", expectedUtcDateForAdelaide, ReportInfo.DateTimeOfOutturn);
			}

			using (Environment.DisposableEnvironment.ForBranch(sydneyBranch.PK.ToGuid()))
			{
				Underbond.Header.C6_OA_OutturningPremise = sydneyPremise.PK;
				AssertEquals("UTC DateTimeOfCargoReceiptUnload for Sydney branch", expectedUtcDateForSydney, ReportInfo.DateTimeOfCargoReceiptUnload);
				AssertEquals("UTC DateTimeOfOutturn for Sydney branch", expectedUtcDateForSydney, ReportInfo.DateTimeOfOutturn);
			}

			// premise ID should come from the associated port code to the premise id entered in the first instance, with all of the above as fallback values
			var establishmentCode = Factory.New<CMREstablishmentCodes>();
			establishmentCode.EC_EstablishmentCode = "9515C";
			establishmentCode.EC_EstablishmentPortCode = "AUDRW";

			using (Environment.DisposableEnvironment.ForBranch(perthBranch.PK.ToGuid()))
			{
				var expectedUtcDateForDarwin = testDate.AddHours(-9).AddMinutes(-30);
				Underbond.Header.C6_OutturningPremiseID = "9515C";
				AssertEquals("UTC DateTimeOfCargoReceiptUnload for Premise in Darwin even though the user logged in to the Perth branch", expectedUtcDateForDarwin, ReportInfo.DateTimeOfCargoReceiptUnload);
				AssertEquals("UTC DateTimeOfOutturn for Premise in Darwin", expectedUtcDateForDarwin, ReportInfo.DateTimeOfOutturn);
			}
		}

		[TestDate(2005, 04, 04)]
		public void TestDateTimeOfOutturnUTC()
		{
			Underbond.C4_Outurned = new ZDateTime(2005, 6, 21, 11, 22, 0);
			AssertEquals("UTC DateTimeOfOutturn", new ZDateTime(2005, 6, 21, 1, 22, 0), ReportInfo.DateTimeOfOutturn);
		}

		public void TestQuantity()
		{
			AssertEquals("Quantity", 0, ReportInfo.Quantity);
		}

		public void TestQuantityUnit()
		{
			AssertEquals("QuantityUnit", "", ReportInfo.QuantityUnit);
		}

		[TestDate(2005, 11, 1)]
		public void TestDontReturnDateOfOutturnIfReceiptOnly()
		{
			Underbond.C4_Outurned = ZDateTime.Now;
			Outturn.C5_ReceiptOnlyIndicator = false;
			AssertEquals("DateTimeOfOutturn", Outturn.Underbond.C4_Outurned.AddHours(-10), ReportInfo.DateTimeOfOutturn);
			Outturn.C5_ReceiptOnlyIndicator = true;
			AssertEquals("DateTimeOfOutturn", ZDateTime.Empty, ReportInfo.DateTimeOfOutturn);
		}

		CusUnderbond underbond;
		protected CusUnderbond Underbond
		{
			get
			{
				if (underbond == null)
				{
					underbond = Factory.New<CusUnderbond>();
					underbond.Outturns.Add(Outturn);
				}
				return underbond;
			}
		}

		SeaCusOutturnOutturnReportLineInformation ReportInfo => (SeaCusOutturnOutturnReportLineInformation)GetHeaderInfo();

		GlbBranch CreateBranch(string branchCode, string branchName, string unloco)
		{
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			branch.GB_Code = branchCode;
			branch.GB_BranchName = branchName;
			branch.GB_RL_NKHomePort = unloco;

			return branch;
		}
	}
}
