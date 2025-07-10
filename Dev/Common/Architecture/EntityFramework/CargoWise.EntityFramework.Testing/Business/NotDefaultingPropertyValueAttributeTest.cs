using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class NotDefaultingPropertyValueAttributeTest : TestCaseWithDummy
	{
		public void TestMatchFilterValue()
		{
			NotDefaultingPropertyValueAttribute attrib = new NotDefaultingPropertyValueAttribute();
			AssertEquals("Filter Value is empty", ZString.Empty, attrib.FilterValue);
			AssertEquals("MatchFilterValue should return true as it doesn't specify the filter value. It will be always true", true, attrib.MatchFilterValue("xxxx"));

			attrib = new NotDefaultingPropertyValueAttribute("yyyy");
			AssertEquals("MatchFilterValue should return false as it doesn't match filter value.", false, attrib.MatchFilterValue("xxxx"));
			AssertEquals("MatchFilterValue should return true as it finds a match.", true, attrib.MatchFilterValue("yyyy"));
		}
	}
}
