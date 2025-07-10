using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business
{
	sealed class CSARSFAssessmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProperties()
		{
			AssertEquals(typeof(CustomsAssessmentsCodes), csaRSFAssessment.Lookups.TypeList.GetType());
		}

		public void TestCounts()
		{
			AssertEquals("7 codes", 7, csaRSFAssessment.Lookups.TypeList.Count);
		}

		protected override void SetUp()
		{
			rSF = Factory.New<CusStatementHeader>();
			line = rSF.StatementLines.AddNew();
			line.B3_EntryType = CSARSFAssessmentTypes.Codes.CustomsAssessment;
			csaRSFAssessment = new CSARSFAssessment(line);
		}
		CSARSFAssessment csaRSFAssessment;
		CusStatementHeader rSF;
		CusStatementLine line;
	}
}
