using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class OperationMatterListTest : TestCaseWithFactory
	{
		public void TestGetOptionGroup()
		{
			var list = new OperationMatterList();
			var result = list.GetMutuallyExclusiveCodes(OperationMatterList.Codes.ConsolidatedDutyCollection);
			AssertContainsExactElementsInAnyOrder(new[] { OperationMatterList.Codes.AssuredInspectClearance }, result);
			result = list.GetMutuallyExclusiveCodes(OperationMatterList.Codes.AssuredInspectClearance);
			AssertContainsExactElementsInAnyOrder(new[] { OperationMatterList.Codes.ConsolidatedDutyCollection }, result);
			result = list.GetMutuallyExclusiveCodes(OperationMatterList.Codes.PaperlessTaxForm);
			Assert("No mutually exclusive codes", !result.Any());
			Assert("Operation matter list should not be translatable", new OperationMatterList() is UntranslatableCodeDescriptionPairList);
		}
	}
}
