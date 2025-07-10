using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC060CTypeOfControlProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("TypeOfControlsType missing", () => new CC060CTypeOfControlProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", 1, provider.SequenceNumber);
		}

		public void TestType()
		{
			AssertEquals("Type", "ABC", provider.Type);
		}

		public void TestText()
		{
			AssertEquals("Text", "Some text about the type of control", provider.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC060CTypeOfControlProvider(new TypeOfControlsType
			{
				SequenceNumber = "1",
				Type = "ABC",
				Text = "Some text about the type of control"
			});
		}
		CC060CTypeOfControlProvider provider;
	}
}
