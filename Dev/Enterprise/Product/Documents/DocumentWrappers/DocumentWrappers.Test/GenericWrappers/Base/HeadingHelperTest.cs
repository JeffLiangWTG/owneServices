using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	sealed class HeadingHelperTest : TestCase
	{
		public void TestGetHouseBillHeading()
		{
			AssertEquals("HAWB", HeadingHelper.GetHouseBillHeading(Core.Constants.TransportModes.Air));
			AssertEquals("House Bill", HeadingHelper.GetHouseBillHeading(null));
			AssertEquals("House Bill", HeadingHelper.GetHouseBillHeading(string.Empty));
			AssertEquals("House Bill", HeadingHelper.GetHouseBillHeading("TEST"));
		}

		public void TestGetMasterBillHeading()
		{
			AssertEquals("MAWB", HeadingHelper.GetMasterBillHeading(Core.Constants.TransportModes.Air));
			AssertEquals("Master Bill", HeadingHelper.GetMasterBillHeading(null));
			AssertEquals("Master Bill", HeadingHelper.GetMasterBillHeading(string.Empty));
			AssertEquals("Master Bill", HeadingHelper.GetMasterBillHeading("TEST"));
		}
	}
}
