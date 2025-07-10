using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCSQuarantineExDocEstablishmentAndTimeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTreatmentCodesAreLoadedFromRefDatabase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("EXE30", "EXDOCS Code Set - E30 Treatment Code", "AU");
			helper.CreateNewOrGetExistingCusCodeList("AU", "EXE30", "ACID", "ACIDIFICATION", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("AU", "EXE30", "BDR", "BATCH DRY RENDERING", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("AU", "EXE30", "CHEMCL", "CHEMICAL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var codePairs = new EXDOCSQuarantineExDocEstablishmentAndTimeLookups(process).TreatmentCode;
			AssertEquals("codePairs", "ACID - ACIDIFICATION\r\nBDR - BATCH DRY RENDERING\r\nCHEMCL - CHEMICAL", codePairs.ElementsAsString);
		}

		QuarantineExDocEstablishmentAndTime process;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			process = quarantineExDocLine.Processes.AddNew();
		}
	}
}
