using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DutyAndTaxTypesTest : TestCaseWithFactory
	{
		public void TestIsSIMATaxCode()
		{
			AssertEquals(true, DutyAndTaxTypes.IsSIMATaxCode(DutyAndTaxTypes.Codes.ADD));
			AssertEquals(true, DutyAndTaxTypes.IsSIMATaxCode(DutyAndTaxTypes.Codes.CVD));
			AssertEquals(true, DutyAndTaxTypes.IsSIMATaxCode(DutyAndTaxTypes.Codes.SUR));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCode(DutyAndTaxTypes.Codes.CPT));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCode(DutyAndTaxTypes.Codes.CTA));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCode(DutyAndTaxTypes.Codes.GST));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCode(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCode(DutyAndTaxTypes.Codes.ExciseTax));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCode(DutyAndTaxTypes.Codes.SIMADuty));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCode(DutyAndTaxTypes.Codes.SAF));
		}

		public void TestIsSIMATaxCodeIncludingSIMAType()
		{
			AssertEquals(true, DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(DutyAndTaxTypes.Codes.ADD));
			AssertEquals(true, DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(DutyAndTaxTypes.Codes.CVD));
			AssertEquals(true, DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(DutyAndTaxTypes.Codes.SUR));
			AssertEquals(true, DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(DutyAndTaxTypes.Codes.SIMADuty));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(DutyAndTaxTypes.Codes.CPT));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(DutyAndTaxTypes.Codes.CTA));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(DutyAndTaxTypes.Codes.GST));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(DutyAndTaxTypes.Codes.ExciseTax));
			AssertEquals(false, DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(DutyAndTaxTypes.Codes.SAF));
		}

		public void TestIsCustomsDutyOrExciseTax()
		{
			AssertEquals(true, DutyAndTaxTypes.IsCustomsDutyOrExciseTax(DutyAndTaxTypes.Codes.CustomsDuty));
			AssertEquals(true, DutyAndTaxTypes.IsCustomsDutyOrExciseTax(DutyAndTaxTypes.Codes.ExciseTax));
			AssertEquals(false, DutyAndTaxTypes.IsCustomsDutyOrExciseTax(DutyAndTaxTypes.Codes.ADD));
			AssertEquals(false, DutyAndTaxTypes.IsCustomsDutyOrExciseTax(DutyAndTaxTypes.Codes.CVD));
			AssertEquals(false, DutyAndTaxTypes.IsCustomsDutyOrExciseTax(DutyAndTaxTypes.Codes.SUR));
			AssertEquals(false, DutyAndTaxTypes.IsCustomsDutyOrExciseTax(DutyAndTaxTypes.Codes.CPT));
			AssertEquals(false, DutyAndTaxTypes.IsCustomsDutyOrExciseTax(DutyAndTaxTypes.Codes.CTA));
			AssertEquals(false, DutyAndTaxTypes.IsCustomsDutyOrExciseTax(DutyAndTaxTypes.Codes.GST));
			AssertEquals(false, DutyAndTaxTypes.IsCustomsDutyOrExciseTax(DutyAndTaxTypes.Codes.SIMADuty));
			AssertEquals(false, DutyAndTaxTypes.IsCustomsDutyOrExciseTax(DutyAndTaxTypes.Codes.SAF));
		}
	}
}
