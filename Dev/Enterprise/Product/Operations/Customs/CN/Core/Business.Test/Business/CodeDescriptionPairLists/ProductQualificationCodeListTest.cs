using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ProductQualificationCodeListTest : TestCaseWithFactory
	{
		public void TestGetCachedProductQualificationCodeList()
		{
			var testList = ProductQualificationCodeList.GetCachedProductQualificationCodeList(Factory, false);
			var testList2 = ProductQualificationCodeList.GetCachedProductQualificationCodeList(Factory, false);
			Assert("Should have cached.", testList.Equals(testList2));
			AssertEquals(31, testList.Count);
			AssertEquals("103, 203, 401, 404, 417, 425, 426, 429, 518, 522, 526, 527, 528, 529, 530, 602, 614, 615, 616, 617, 618, 619, 620, 621, 622, 623, 624, 626, 627, 628, 630", testList.CodesAsString);
			testList = ProductQualificationCodeList.GetCachedProductQualificationCodeList(Factory, true);
			AssertEquals(60, testList.Count);
			AssertEquals("105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 203, 325, 328, 330, 331, 332, 401, 402, 408, 409, 410, 411, 412, 416, 422, 423, 424, 428, 429, 430, 516, 517, 519, 522, 523, 524, 526, 527, 528, 529, 530, 601, 603, 604, 605, 606, 607, 608, 609, 610, 611, 612, 613, 629, 630, 800, 900", testList.CodesAsString);
		}

		public void TestIsProductQualificationCodeSupportTSD()
		{
			var expectedList = new[] { "330", "331", "332", "410", "411", "429", "430", "526", "527", "528", "529", "530", "606", "612", "629" };
			var unexpectedList = new ProductQualificationCodeList().GetAllCodes().Except(expectedList);
			foreach (var item in expectedList)
			{
				AssertEquals(true, ProductQualificationCodeList.IsProductQualificationCodeSupportTSD(item));
			}
			foreach (var item in unexpectedList)
			{
				AssertEquals(false, ProductQualificationCodeList.IsProductQualificationCodeSupportTSD(item));
			}
		}
	}
}
