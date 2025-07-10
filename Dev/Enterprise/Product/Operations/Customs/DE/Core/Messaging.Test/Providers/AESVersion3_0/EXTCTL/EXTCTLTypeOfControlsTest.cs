using NUnit.Framework;
namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	sealed class EXTCTLTypeOfControlsTest : TestCase
	{
		public void TestConstructorNoExceptionThrown()
		{
			AssertNoExceptionThrown(() => new EXTCTLTypeOfControls(null, null));
		}

		[ExpectNoExceptions]
		public void TestType()
		{
			NUnit.Framework.Assert.That(typeOfControls.Type, Is.EqualTo("10"));
		}

		[ExpectNoExceptions]
		public void TestText()
		{
			NUnit.Framework.Assert.That(typeOfControls.Text, Is.EqualTo("Text"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			typeOfControls = new EXTCTLTypeOfControls("10", "Text");
		}

		EXTCTLTypeOfControls typeOfControls;
	}
}
