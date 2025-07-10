using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CargoAttributeListTest : TestCaseWithFactory
	{
		public void TestGetOptionGroup()
		{
			AssertGetOptionGroupResult("11", "11", "12", "13");
			AssertGetOptionGroupResult("12", "11", "12", "13");
			AssertGetOptionGroupResult("13", "11", "12", "13");
			AssertGetOptionGroupResult("14", "14", "15");
			AssertGetOptionGroupResult("15", "14", "15");
			AssertGetOptionGroupResult("16", "16", "17");
			AssertGetOptionGroupResult("17", "16", "17");
			AssertGetOptionGroupResult("18", "18", "19", "20", "21", "22");
			AssertGetOptionGroupResult("19", "18", "19", "20", "21", "22");
			AssertGetOptionGroupResult("20", "18", "19", "20", "21", "22");
			AssertGetOptionGroupResult("21", "18", "19", "20", "21", "22");
			AssertGetOptionGroupResult("22", "18", "19", "20", "21", "22");
			AssertGetOptionGroupResult("23", "23", "24");
			AssertGetOptionGroupResult("24", "23", "24");
			AssertGetOptionGroupResult("25", "25", "26", "27", "28", "29");
			AssertGetOptionGroupResult("26", "25", "26", "27", "28", "29");
			AssertGetOptionGroupResult("27", "25", "26", "27", "28", "29");
			AssertGetOptionGroupResult("28", "25", "26", "27", "28", "29");
			AssertGetOptionGroupResult("29", "25", "26", "27", "28", "29");
			AssertGetOptionGroupResult("30", "30");
			AssertGetOptionGroupResult("31", "31", "32", "33");
			AssertGetOptionGroupResult("32", "31", "32", "33");
			AssertGetOptionGroupResult("33", "31", "32", "33");
			AssertGetOptionGroupResult("34", "34", "35", "36", "37", "38");
			AssertGetOptionGroupResult("35", "34", "35", "36", "37", "38");
			AssertGetOptionGroupResult("36", "34", "35", "36", "37", "38");
			AssertGetOptionGroupResult("37", "34", "35", "36", "37", "38");
			AssertGetOptionGroupResult("38", "34", "35", "36", "37", "38");
			AssertGetOptionGroupResult("39", "39", "40");
			AssertGetOptionGroupResult("40", "39", "40");
			AssertGetOptionGroupResult("41", "41");
			AssertGetOptionGroupResult("42", "42", "43", "44");
			AssertGetOptionGroupResult("43", "42", "43", "44");
			AssertGetOptionGroupResult("44", "42", "43", "44");
			AssertGetOptionGroupResult("46", "46");

			void AssertGetOptionGroupResult(ZString code, params string[] expectedCodes)
			{
				var result = ((IGroupedCodeDescriptionPairList)new CargoAttributeList()).GetMutuallyExclusiveCodes(code).ToArray();
				AssertEquals("Result count for " + code + " should be" + (expectedCodes.Length - 1), expectedCodes.Length - 1, result.Length);
				foreach (var expectedCode in expectedCodes.Where(x => x != code))
				{
					Assert("Result for " + code + " should contain " + expectedCode, result.Any(option => option == expectedCode));
				}
			}
		}

		public void TestIsDangerousGoodsAttribute()
		{
			AssertEquals("31,32,33 true for IsDangerousGoodsAttribute(), others false.", false, CargoAttributeList.IsDangerousGoodsAttribute("XXX"));
			AssertEquals("31,32,33 true for IsDangerousGoodsAttribute(), others false.", false, CargoAttributeList.IsDangerousGoodsAttribute("11"));
			AssertEquals("31,32,33 true for IsDangerousGoodsAttribute(), others false.", true, CargoAttributeList.IsDangerousGoodsAttribute("31"));
			AssertEquals("31,32,33 true for IsDangerousGoodsAttribute(), others false.", true, CargoAttributeList.IsDangerousGoodsAttribute("32"));
			AssertEquals("31,32,33 true for IsDangerousGoodsAttribute(), others false.", true, CargoAttributeList.IsDangerousGoodsAttribute("33"));
		}

		public void TestCanLinkToAttachment()
		{
			AssertEquals("31,32 true for CanLinkToAttachment(), others false.", false, CargoAttributeList.CanLinkToAttachment("XXX"));
			AssertEquals("31,32 true for CanLinkToAttachment(), others false.", false, CargoAttributeList.CanLinkToAttachment("11"));
			AssertEquals("31,32 true for CanLinkToAttachment(), others false.", true, CargoAttributeList.CanLinkToAttachment("31"));
			AssertEquals("31,32 true for CanLinkToAttachment(), others false.", true, CargoAttributeList.CanLinkToAttachment("32"));
			AssertEquals("31,32 true for CanLinkToAttachment(), others false.", false, CargoAttributeList.CanLinkToAttachment("33"));
		}
	}
}
