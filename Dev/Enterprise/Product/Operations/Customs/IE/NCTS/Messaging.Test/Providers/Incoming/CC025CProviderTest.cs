using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC025C;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC025CProviderTest : TestCaseWithFactory
	{
		public void TestEmptyValue()
		{
			CombineAssertions(() =>
			{
				var emptyProvider = new CC025CProvider(new Cc025CType());
				AssertEquals("Null value should return empty", ZString.Empty, emptyProvider.ReleaseIndicator);
			});
		}

		public void TestMRN()
		{
			AssertEquals("21IEDUB11A782454R2", provider.MRN);
		}

		public void TestReleaseDate()
		{
			AssertEquals(ZDateTime.BrettsBirthday, provider.ReleaseDate);
		}

		public void TestReleaseIndicator()
		{
			AssertEquals("ReleaseIndicator", "1", provider.ReleaseIndicator);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC025CProvider(InterchangeProcessorTestHelper.GetStandardCC025C("21IEDUB11A782454R2", "1"));
		}
		CC025CProvider provider;
	}
}
