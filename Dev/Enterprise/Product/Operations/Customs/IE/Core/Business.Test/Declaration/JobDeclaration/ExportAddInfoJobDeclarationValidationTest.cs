using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExportAddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_BorderTransportMeans()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var targetInfo = declaration.ZG_BorderTransportMeansInfo;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}
	}
}
