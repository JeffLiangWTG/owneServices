using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	public class CDSAddInfoJobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestCheckZG_Gateway()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = Enterprise.Customs.GB.Business.CodeDescriptionPairLists.ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.JE_MessageSubType = "IM";
			declaration.ZG_Gateway = "";
			AssertHasMessageError(declaration.ZG_GatewayInfo, CDSJobDeclarationValidation.CSPZG_GatewayValidationError.ToString());

			declaration.ZG_Gateway = Enterprise.Customs.GB.Registry.GatewayList.Codes.CDS;
			AssertNoMessageError(declaration.ZG_GatewayInfo, CDSJobDeclarationValidation.CSPZG_GatewayValidationError.ToString());
		}

		public void TestCheckZG_CTStatusID()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.ZG_CTStatusID = "XXXX";
			AssertHasMessageErrorContaining(declaration.ZG_CTStatusIDInfo, "The code you have selected is not in the list.");

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.ZG_CTStatusID = ImportCommunityTransitStatusList.Codes.T;
			AssertNoMessageErrors(declaration.ZG_CTStatusIDInfo);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.ZG_CTStatusID = ExportCommunityTransitStatusList.Codes.X;
			AssertNoMessageErrors(declaration.ZG_CTStatusIDInfo);

			declaration.ZG_CTStatusID = "";
			AssertNoMessageErrors(declaration.ZG_CTStatusIDInfo);
		}

		public void TestCheckZG_Box18TransportID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertHasMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertHasMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertHasMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertHasMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
