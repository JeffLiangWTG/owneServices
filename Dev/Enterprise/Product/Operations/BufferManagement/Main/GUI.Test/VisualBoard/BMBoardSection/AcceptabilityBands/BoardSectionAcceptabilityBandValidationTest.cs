using Enterprise.BufferManagement.Business.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	class BoardSectionAcceptabilityBandValidationTest : BMSTestCaseWithFactory
	{
		public void TestMaximumItems()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "");
			band.BAB_Name = "Defect Backlog Improvement ESRVRG (30 days)";
			band.BAB_SqlText = @"
				SELECT convert(decimal(10,3), 'Nope')  as Value,
				null as Component,
				null as ReleaseGroup";

			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);

			sectionBand.MaximumItems = 0;
			AssertHasError(sectionBand.MaximumItemsInfo, "Please enter a 'Maximum Items to Display' greater than 0.");

			sectionBand.MaximumItems = 1000;
			AssertHasError(sectionBand.MaximumItemsInfo, "Please enter a 'Maximum Items to Display' less than or equal to 999.");

			sectionBand.MaximumItems = 1;
			AssertNoErrors(sectionBand.MaximumItemsInfo);

			sectionBand.MaximumItems = 999;
			AssertNoErrors(sectionBand.MaximumItemsInfo);
		}
	}
}
