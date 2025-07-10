using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class GatepassFeedbackMessageIndicationTypeListTest : TestCaseWithFactory
	{
		public void TestList()
		{
			var list = new GatepassFeedbackMessageIndicationTypeList();
			AssertEquals(11, list.Count);
			AssertEquals("Guaranty Not Suitable", list.GetDescriptionFromCode("1"));
			AssertEquals("Check Open", list.GetDescriptionFromCode("2"));
			AssertEquals("Risk Assesment Delay", list.GetDescriptionFromCode("4"));
			AssertEquals("Manually Delay", list.GetDescriptionFromCode("5"));
			AssertEquals("Initiated Check", list.GetDescriptionFromCode("6"));
			AssertEquals("Import Declaration Exists", list.GetDescriptionFromCode("7"));
			AssertEquals("Cargo Delay", list.GetDescriptionFromCode("8"));
			AssertEquals("Pending Child Cargo", list.GetDescriptionFromCode("9"));
			AssertEquals("Pending Child Cargo Import", list.GetDescriptionFromCode("10"));
			AssertEquals("Pending Ship Status", list.GetDescriptionFromCode("11"));
			AssertEquals("Pending Risk Assesment", list.GetDescriptionFromCode("12"));
		}
	}
}
