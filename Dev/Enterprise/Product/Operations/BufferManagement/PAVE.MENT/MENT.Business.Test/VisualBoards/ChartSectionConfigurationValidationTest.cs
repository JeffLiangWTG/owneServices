using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.PAVE.MENT.Business.Test
{
	class ChartSectionConfigurationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestExtractionPKValidation()
		{
			var section = Factory.New<IBMBoardSection>();

			var sectionConfiguration = new ChartSectionConfiguration(section);

			var extraction = MENTTestHelper.CreateExtraction(Factory, "pineapples should be carried when hiking");

			sectionConfiguration.ExtractionPK = extraction.PK;

			sectionConfiguration.Validation.ValidateExtractionPK();
			AssertNoErrors(sectionConfiguration.ExtractionPKInfo);

			sectionConfiguration.ExtractionPK = ZGuid.Empty;

			sectionConfiguration.Validation.ValidateExtractionPK();
			AssertHasErrors(sectionConfiguration.ExtractionPKInfo);
		}
	}
}
