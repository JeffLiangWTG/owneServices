using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	internal class AccApportionmentTemplateLinesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBranchDepartmentCombinationValidation_AccApportionmentTemplateLinesValidation()
		{
			var bizObj = Factory.NewWithValidTestData<AccApportionmentTemplateLines>();

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { bizObj.Y0_GB = branch; bizObj.Y0_GE = department; }, bizObj.Y0_GEInfo);
		}
	}
}