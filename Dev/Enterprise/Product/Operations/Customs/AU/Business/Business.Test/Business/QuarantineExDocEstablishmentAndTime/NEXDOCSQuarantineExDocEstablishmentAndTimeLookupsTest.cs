using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCSQuarantineExDocEstablishmentAndTimeLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2019, 3, 6)]
		public void TestTreatmentCode()
		{
			var treatmentCodes = lookups.TreatmentCode;
			AssertEquals("CORE, D HEAT, FUNGIC, TR4", treatmentCodes.CodesAsString);
			AssertSame(treatmentCodes, lookups.TreatmentCode);
		}

		[TestDate(2019, 3, 6)]
		public void TestEstablishmentIndicatorList()
		{
			var establishmentIndList = lookups.EstablishmentIndicatorList;
			AssertEquals("IN, M, PC, PK, ST", establishmentIndList.CodesAsString);
			AssertSame(establishmentIndList, lookups.EstablishmentIndicatorList);
		}

		public void TestProcessingType()
		{
			var processingTypes = lookups.ProcessingType;
			AssertEquals("AQ, CB, CT, CU, DP, FR, HA, IN, LO, M, PC, PK, SA, SG, SL, ST, TR", processingTypes.CodesAsString);
			AssertSame(processingTypes, lookups.ProcessingType);

			process.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			AssertEquals("AQ, CB, CT, CU, DP, FF, FR, FV, HA, IN, IR, LO, M, PC, PK, SA, SG, SL, ST, TR, TV", lookups.ProcessingType.CodesAsString);
		}

		QuarantineExDocEstablishmentAndTimeLookups lookups;

		protected override void SetUp()
		{
			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("NTRTT", "NEXDOCS Treatment Code");
			helper.CreateNewOrGetExistingCusCodeList("AU", "NTRTT", "D HEAT", "DRY HEAT", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NTRTT", "TR4", "Test treatment", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NTRTT", "CORE", "CORE", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NTRTT", "FUNGIC", "FUNGIC COOL", date1, date2);

			helper.CreateNewOrGetExistingCusCodeType("NESTI", "NEXDOC Establishment Indicators");
			helper.CreateNewOrGetExistingCusCodeList("AU", "NESTI", "M", "Manufacturer", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NESTI", "PC", "Processing", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NESTI", "PK", "Packing", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NESTI", "ST", "Storage", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NESTI", "IN", "Inspection", date1, date2);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			process = quarantineExDocLine.Processes.AddNew();
			lookups = process.Lookups;
			base.SetUp();
		}

		QuarantineExDocEstablishmentAndTime process;
	}
}
