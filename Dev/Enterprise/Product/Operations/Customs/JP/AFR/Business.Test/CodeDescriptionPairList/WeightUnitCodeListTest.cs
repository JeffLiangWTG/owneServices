using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class WeightUnitCodeListTest : TestCase
	{
		public void TestList()
		{
			var list = new WeightUnitCodeList();
			AssertEquals(3, list.Count);
			AssertEquals("Kilogram", list.GetDescriptionFromCode("KG"));
			AssertEquals("Pound", list.GetDescriptionFromCode("LB"));
			AssertEquals("Tonne", list.GetDescriptionFromCode("T"));
		}
	}
}
