using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.Testing
{
	sealed class EDIInterchangeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEI_HeaderText()
		{
			var interchange = Factory.New<EDIInterchange>();
			CheckText(interchange, interchange.EI_HeaderTextInfo);
		}

		public void TestCheckEI_BodyText()
		{
			var interchange = Factory.New<EDIInterchange>();
			CheckText(interchange, interchange.EI_BodyTextInfo);
		}

		public void TestCheckEI_FooterText()
		{
			var interchange = Factory.New<EDIInterchange>();
			CheckText(interchange, interchange.EI_FooterTextInfo);
		}

		public void TestSizeCheckValidationIsSuppressed()
		{
			var reallyBigString = new string('*', 5000000);

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_BodyData = ZBlob.FromUTF8(reallyBigString);
			AssertNoErrors(interchange.EI_BodyDataInfo);
		}

		void CheckText(EDIInterchange interchange, ZPropertyInfo info)
		{
			AssertEquals("no errors", false, info.HasNotifications());

			string valid = "This \r is \n is \t a test ?' message'. This" + '\x1f' + "format" + '\x1d' + "I don't" + '\x1c' + " understand?";
			info.SetValueFromString(valid);
			AssertEquals("no errors", false, info.HasNotifications());

			info.SetValueFromString(valid + '\u03C0');
			AssertEquals("invalid", true, info.HasError(EnglishCharactersValidation.GetNotificationMessage(info)));
		}
	}
}
