using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors.Testing
{
	[TestedType(typeof(EdifactErrorMessageHelper))]
	public class EdifactErrorMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocumentMessageName()
		{
			AssertEquals("DocumentMessageName is empty", ZString.Empty, testErrorHelperResponse.DocumentMessageName);
		}

		public void TestAdmissionDate()
		{
			AssertEquals("AdmissionDate is empty", ZDateTime.Empty, testErrorHelperResponse.AdmissionDate);
		}

		public void TestMessageFunction()
		{
			AssertEquals("MessageFunction is empty", ZString.Empty, testErrorHelperResponse.MessageFunction);
		}

		public void TestFreeTextErrors()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Only one error in FreeTextErrors", 1, testErrorHelperResponse.FreeTextErrors.Count);

				AssertEquals("FreeTextErrors[0].Code", "00338", testErrorHelperResponse.FreeTextErrors[0].Code);
				AssertEquals("FreeTextErrors[0].Location", string.Empty, testErrorHelperResponse.FreeTextErrors[0].Location);
				AssertEquals("FreeTextErrors[0].Description", "Error description", testErrorHelperResponse.FreeTextErrors[0].Description);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EdifactErrorMessageHelper(Factory, "00338", "Error description");
		}

		protected override void SetUp()
		{
			base.SetUp();

			testErrorHelperResponse = new EdifactErrorMessageHelper(Factory, "00338", "Error description");
		}

		ICUSRESMessageProvider testErrorHelperResponse;
	}
}
