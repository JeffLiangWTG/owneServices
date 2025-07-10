using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class ImportAmendmentDataItemIDListTest : TestCaseWithFactory
	{
		public void TestUpdatedList()
		{
			var importAmendmentDataItemIDs = Factory.GetCachedValue<ImportAmendmentDataItemIDList>();
			Assert(!importAmendmentDataItemIDs.ContainsCode("999D"));
			Assert(!importAmendmentDataItemIDs.ContainsCode("999I"));
			Assert(!importAmendmentDataItemIDs.ContainsCode("A313"));
			Assert(!importAmendmentDataItemIDs.ContainsCode("A906"));
			Assert(!importAmendmentDataItemIDs.ContainsCode("Z101"));
			Assert(!importAmendmentDataItemIDs.ContainsCode("Z102"));
			Assert(!importAmendmentDataItemIDs.ContainsCode("Z103"));

			Assert(importAmendmentDataItemIDs.ContainsCode("A616"));
			Assert(importAmendmentDataItemIDs.ContainsCode("A617"));
			Assert(importAmendmentDataItemIDs.ContainsCode("A618"));
			Assert(importAmendmentDataItemIDs.ContainsCode("A620"));
			Assert(importAmendmentDataItemIDs.ContainsCode("A621"));
			Assert(importAmendmentDataItemIDs.ContainsCode("A622"));
			Assert(importAmendmentDataItemIDs.ContainsCode("A623"));
			Assert(importAmendmentDataItemIDs.ContainsCode("A624"));
			Assert(importAmendmentDataItemIDs.ContainsCode("A625"));
			Assert(importAmendmentDataItemIDs.ContainsCode("A619"));
			Assert(importAmendmentDataItemIDs.ContainsCode("E107"));
			Assert(importAmendmentDataItemIDs.ContainsCode("H101"));
			Assert(importAmendmentDataItemIDs.ContainsCode("H102"));
			Assert(importAmendmentDataItemIDs.ContainsCode("H103"));
			Assert(importAmendmentDataItemIDs.ContainsCode("I101"));
			Assert(importAmendmentDataItemIDs.ContainsCode("I102"));
			Assert(importAmendmentDataItemIDs.ContainsCode("I103"));
			Assert(importAmendmentDataItemIDs.ContainsCode("J101"));
			Assert(importAmendmentDataItemIDs.ContainsCode("J102"));
			Assert(importAmendmentDataItemIDs.ContainsCode("J103"));
		}

		public void TestIsDateTimeField()
		{
			Assert("ArrivalDateAtDischargePort", ImportAmendmentDataItemIDList.IsDateTimeField(ImportAmendmentDataItemIDList.Codes.A604));
			Assert("UnderbondMovementArrivalDate",ImportAmendmentDataItemIDList.IsDateTimeField(ImportAmendmentDataItemIDList.Codes.A605));
			Assert("BondedFactoryUseDate", ImportAmendmentDataItemIDList.IsDateTimeField(ImportAmendmentDataItemIDList.Codes.A704));
			Assert("ApprovalDate", ImportAmendmentDataItemIDList.IsDateTimeField(ImportAmendmentDataItemIDList.Codes.D107));
			Assert("CertificateOfOriginIssueDate", ImportAmendmentDataItemIDList.IsDateTimeField(ImportAmendmentDataItemIDList.Codes.F104));
		}

		public void TestIsDutyTaxItemField()
		{
			Assert(!ImportAmendmentDataItemIDList.IsDutyTaxItemField(ImportAmendmentDataItemIDList.Codes.A101));
			Assert("TotalDutyAmount", ImportAmendmentDataItemIDList.IsDutyTaxItemField(ImportAmendmentDataItemIDList.Codes.A815));
			Assert("TotalSpecialConsumptionTax", ImportAmendmentDataItemIDList.IsDutyTaxItemField(ImportAmendmentDataItemIDList.Codes.A816));
			Assert("TotalTransportationTax", ImportAmendmentDataItemIDList.IsDutyTaxItemField(ImportAmendmentDataItemIDList.Codes.A818));
			Assert("TotalLiquorTax", ImportAmendmentDataItemIDList.IsDutyTaxItemField(ImportAmendmentDataItemIDList.Codes.A817));
			Assert("TotalEducationTax", ImportAmendmentDataItemIDList.IsDutyTaxItemField(ImportAmendmentDataItemIDList.Codes.A820));
			Assert("TotalAgricultureTax", ImportAmendmentDataItemIDList.IsDutyTaxItemField(ImportAmendmentDataItemIDList.Codes.A821));
			Assert("TotalVAT", ImportAmendmentDataItemIDList.IsDutyTaxItemField(ImportAmendmentDataItemIDList.Codes.A819));
			Assert("PenaltyForLateDeclaration", ImportAmendmentDataItemIDList.IsDutyTaxItemField(ImportAmendmentDataItemIDList.Codes.A822));
			Assert("PenaltyForMissedDeclaration", ImportAmendmentDataItemIDList.IsDutyTaxItemField(ImportAmendmentDataItemIDList.Codes.A823));
		}
	}
}
