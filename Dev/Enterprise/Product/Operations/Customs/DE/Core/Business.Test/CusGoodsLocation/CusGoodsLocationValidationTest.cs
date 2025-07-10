using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class CusGoodsLocationValidationTest : TestCaseWithFactory
	{
		public void TestCheckLoadingPlace_Mandatory()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				var goodsLocation = instruction.GoodsLocation;
				goodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.Address;
				goodsLocation.Validation.ValidateLoadingPlace();
				AssertHasMessageErrorContaining("Has message error", goodsLocation.LoadingPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				goodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode;
				goodsLocation.Validation.ValidateLoadingPlace();
				AssertNoMessageErrorContaining("No message error", goodsLocation.LoadingPlaceInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCGL_Qualifier_Export_ListValidation()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var goodsLocation = instruction.GoodsLocation;
			ValidationTestHelper.AssertInvalidCodeMessageError(goodsLocation.CGL_QualifierInfo, "X", CusGoodsLocationQualifierList.Codes.UnLocode);
		}

		public void TestCheckCGL_Qualifier_AuthorizationHasLoadingPlaceCode()
		{
			const string message = "If your Authorization contains codes for loading place please use Qualifier Of identification 'Y'.";
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var goodsLocation = instruction.GoodsLocation;
			var targetInfo = goodsLocation.CGL_QualifierInfo;
			CombineAssertions(() =>
			{
				foreach (var s in new[] { ExportDeclarationTypeProcedureList.Codes._001300, ExportDeclarationTypeProcedureList.Codes._111410 })
				{
					instruction.CEI_Style = s;
					goodsLocation.Validation.ValidateCGL_Qualifier();
					AssertNoWarning($"CEI_Style is {s}", targetInfo, message);

					goodsLocation.CGL_Qualifier = ZString.Empty;
					goodsLocation.Validation.ValidateCGL_Qualifier();
					AssertHasWarning($"CEI_Style is {s}", targetInfo, message);
				}
			});
		}

		public void TestCheckCGL_Qualifier_AuthorizationNotHaveLoadingPlaceCode()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var goodsLocation = instruction.GoodsLocation;
			var targetInfo = goodsLocation.CGL_QualifierInfo;
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
			goodsLocation.Validation.ValidateCGL_Qualifier();
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered, "SubStyle is MultipleDeclarationForExport");

				instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, MandatoryValidation.YouHaveNotEntered, "SubStyle is not MultipleDeclarationForExport");
			});
		}

		public void TestCheckCGL_Qualifier_Export_CEI_Style()
		{
			const string message = "Only Code 'V' can be selected.";
			const string message2 = "Only Codes 'U', 'W', 'Y', 'Z' can be selected.";
			const string message3 = "Only Code 'Y' can be selected.";

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var goodsLocation = instruction.GoodsLocation;
			var targetInfo = goodsLocation.CGL_QualifierInfo;
			CombineAssertions(() =>
			{
				instruction.CEI_Style = "000100";
				goodsLocation.Validation.ValidateCGL_Qualifier();
				AssertNoMessageErrorContaining("CGL_Qualifier is empty", targetInfo, message);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				AssertHasMessageErrorContaining("The fourth num is '1', has message error", targetInfo, message);
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				AssertNoMessageErrorContaining("The fourth num is '1', no message error", targetInfo, message);

				instruction.CEI_Style = "000900";
				goodsLocation.Validation.ValidateCGL_Qualifier();
				AssertNoMessageErrorContaining("The fourth num is '9', no message error", targetInfo, message);
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				AssertHasMessageErrorContaining("The fourth num is '9', has message error", targetInfo, message);

				instruction.CEI_Style = "000200";
				goodsLocation.Validation.ValidateCGL_Qualifier();
				AssertNoMessageErrorContaining("The fourth num is '2', CGL_Qualifier is 'U', no message error", targetInfo, message2);
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
				AssertNoMessageErrorContaining("The fourth num is '2', CGL_Qualifier is 'W', no message error", targetInfo, message2);
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				AssertNoMessageErrorContaining("The fourth num is '2', CGL_Qualifier is 'Y', no message error", targetInfo, message2);
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				AssertNoMessageErrorContaining("The fourth num is '2', CGL_Qualifier is 'Z', no message error", targetInfo, message2);
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				AssertHasMessageErrorContaining("The fourth num is '2', CGL_Qualifier is 'V', has message error", targetInfo, message2);

				instruction.CEI_Style = "000300";
				goodsLocation.Validation.ValidateCGL_Qualifier();
				AssertNoMessageErrorContaining("The fourth num is '3', no message error", targetInfo, message3);
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
				AssertHasMessageErrorContaining("The fourth num is '3', has message error", targetInfo, message3);

				instruction.CEI_Style = "000400";
				goodsLocation.Validation.ValidateCGL_Qualifier();
				AssertNoMessageErrorContaining("The fourth num is '4', no message error", targetInfo, message3);
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
				AssertHasMessageErrorContaining("The fourth num is '4', has message error", targetInfo, message3);
			});
		}

		public void TestIncludeRuleCode()
		{
			var goodsLocation = instruction.GoodsLocation;
			AssertEquals("Should be 'false' in DE", false, goodsLocation.Validation.IncludeRuleCode);
		}

		public void TestCheckCGL_AdditionalIdentifier_WhenQualifierIsU()
		{
			const string message = "Additional Identifier required when qualifier is 'U'.";
			var goodsLocation = instruction.GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			goodsLocation.Unlocode = "XYZ";
			var targetInfo = goodsLocation.UnlocodeInfo;
			AssertNoMessageErrorContaining("Should not be empty, has message error", targetInfo, message);

			goodsLocation.Unlocode = ZString.Empty;
			AssertHasMessageErrorContaining("Should not be empty, has message error", targetInfo, message);
		}

		public void TestCheckCGL_CustomsOffice()
		{
			var goodsLocation = instruction.GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertNoNotifications("No message errors if Cutoms Office is empty (always empty for DE)", goodsLocation.CGL_CustomsOfficeInfo);
		}

		public void TestCheckCGL_AdditionalIdentifier_WhenQualifierIsY()
		{
			const string message = "Additional Identifier required when qualifier is 'Y'.";

			var goodsLocation = instruction.GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			var targetInfo = goodsLocation.CGL_AdditionalIdentifierInfo;
			AssertHasMessageError("Should not be empty, has message error", targetInfo, message);

			goodsLocation.CGL_AdditionalIdentifier = "AA08";
			AssertNoMessageError("No message error when filled", targetInfo, message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<Declaration.JobDeclaration>();
			instruction = Factory.New<Declaration.CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
		}

		Declaration.CusEntryInstruction instruction;
		Declaration.JobDeclaration declaration;
	}
}
