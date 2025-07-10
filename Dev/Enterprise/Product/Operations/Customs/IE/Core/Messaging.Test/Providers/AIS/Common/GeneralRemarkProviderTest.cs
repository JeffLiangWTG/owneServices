using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(GeneralRemarkProvider))]
	sealed class GeneralRemarkProviderTest : TestCase
	{
		public void TestProperties()
		{
			AssertEquals("Test_General_Remarks", provider.GeneralRemarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new GeneralRemarkProvider("Test_General_Remarks");
		}

		GeneralRemarkProvider provider;
	}
}
