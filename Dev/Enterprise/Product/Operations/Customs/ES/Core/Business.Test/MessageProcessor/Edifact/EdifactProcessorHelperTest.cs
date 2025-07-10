using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	sealed class EdifactProcessorHelperTest : TestCaseWithFactory
	{
		public void TestProcessEdifactErrorResponse()
		{
			var message = Factory.New<TestEdiMessage>();
			message.EM_MessageText = GetEdifactErrorMessageFile();
			var edifactErrorProvider = EdifactProcessorHelper.ProcessEdifactErrorResponse(message);

			CombineAssertions(() =>
			{
				AssertEquals("DocumentMessageName", ZString.Empty, edifactErrorProvider.DocumentMessageName);
				AssertEquals("AdmissionDate", ZDateTime.Empty, edifactErrorProvider.AdmissionDate);
				AssertEquals("MessageFunction", ZString.Empty, edifactErrorProvider.MessageFunction);

				AssertEquals("Only one error in FreeTextErrors", 1, edifactErrorProvider.FreeTextErrors.Count);

				AssertEquals("FreeTextErrors[0].Code", "50052", edifactErrorProvider.FreeTextErrors[0].Code);
				AssertEquals("FreeTextErrors[0].Location", string.Empty, edifactErrorProvider.FreeTextErrors[0].Location);
				AssertEquals("FreeTextErrors[0].Description", "Error Traducción:Segmento (TPL) Mensaje erroneo", edifactErrorProvider.FreeTextErrors[0].Description);
			});
		}

		string GetEdifactErrorMessageFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EdifactTestFilePath, "EdifactErrorMessage.txt");
	}
}
