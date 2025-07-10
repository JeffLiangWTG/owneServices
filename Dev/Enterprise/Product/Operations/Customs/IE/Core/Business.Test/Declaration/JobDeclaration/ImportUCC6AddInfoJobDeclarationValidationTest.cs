using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ImportUCC6AddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_BorderTransportMeans()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var targetInfo = declaration.ZG_BorderTransportMeansInfo;

			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}
	}
}
