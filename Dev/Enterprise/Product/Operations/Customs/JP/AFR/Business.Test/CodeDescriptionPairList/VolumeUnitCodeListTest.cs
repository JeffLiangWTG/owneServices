using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class VolumeUnitCodeListTest : TestCase
	{
		public void TestList()
		{
			var list = new VolumeUnitCodeList();
			AssertEquals(3, list.Count);
			AssertEquals("Cubic Meter", list.GetDescriptionFromCode("M3"));
			AssertEquals("Cubic Feet", list.GetDescriptionFromCode("CF"));
			AssertEquals("Board Foot Measure (timber)", list.GetDescriptionFromCode("BF"));
		}
	}
}
