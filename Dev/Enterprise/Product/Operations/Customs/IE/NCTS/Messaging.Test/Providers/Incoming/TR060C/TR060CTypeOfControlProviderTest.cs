using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	sealed class TR060CTypeOfControlProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("TypeOfControlsType missing", () => new TR060CTypeOfControlProvider(null));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestType()
		{
			AssertEquals("ABC", provider.Type);
		}

		public void TestText()
		{
			AssertEquals("Some text about the type of control", provider.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TR060CTypeOfControlProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.TypeOfControlsType
			{
				SequenceNumber = "1",
				Type = "ABC",
				Text = "Some text about the type of control"
			});
		}
		TR060CTypeOfControlProvider provider;
	}
}
