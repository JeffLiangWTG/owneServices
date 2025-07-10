using CargoWise.EntityFramework.Testing;
using static Enterprise.Customs.Business.Testing.ValidationTestHelper;

namespace Enterprise.Customs.DE.Business.Testing
{
	class TaxChangeAssessmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEntryStatus()
		{
			var taxChangeAssessment = Factory.New<TaxChangeAssessment>();
			AssertInvalidCodeMessageError(taxChangeAssessment.EntryStatusInfo, "XX", TaxChangeAssessmentStatusCodeList.Codes.PRG);
		}
	}
}
