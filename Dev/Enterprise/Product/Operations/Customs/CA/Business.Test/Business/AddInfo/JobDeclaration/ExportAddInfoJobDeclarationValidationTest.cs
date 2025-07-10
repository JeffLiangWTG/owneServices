using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ExportAddInfoJobDeclarationValidationTest : AddInfoJobDeclarationValidationTest
	{
		public void TestCheckCA_RX_DeclaredCurr()
		{
			declaration.CA_RX_DeclaredCurr = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.CA_RX_DeclaredCurrInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrorContaining(declaration.CA_RX_DeclaredCurrInfo, "Enter a valid ");

			declaration.CA_RX_DeclaredCurr = ZGuid.Invalid;
			AssertNoMessageErrorContaining(declaration.CA_RX_DeclaredCurrInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasErrorContaining(declaration.CA_RX_DeclaredCurrInfo, "Enter a valid ");

			declaration.CA_RX_DeclaredCurr = JobDeclaration.GetLocalCurrency().PK;
			AssertNoMessageErrorContaining(declaration.CA_RX_DeclaredCurrInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrorContaining(declaration.CA_RX_DeclaredCurrInfo, "Enter a valid ");
		}

		public void TestCheckCA_PortOfExit()
		{
			var messageError = MandatoryValidation.YouHaveNotEntered + " a Port Of Exit.";
			declaration.CA_PortOfExit = ZString.Empty;
			AssertHasMessageError(declaration.CA_PortOfExitInfo, messageError);
			AssertNoMessageErrorContaining(declaration.CA_PortOfExitInfo, ListValidation.InvalidCodeMessageError);

			declaration.CA_PortOfExit = OfficeCode.ZZD_Code;
			AssertNoMessageError(declaration.CA_PortOfExitInfo, messageError);
			AssertNoMessageErrorContaining(declaration.CA_PortOfExitInfo, ListValidation.InvalidCodeMessageError);

			declaration.CA_PortOfExit = "0000";
			AssertHasMessageErrorContaining(declaration.CA_PortOfExitInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCA_PlaceOfReport()
		{
			var messageError = MandatoryValidation.YouHaveNotEntered + " a Place Of Report.";
			declaration.CA_PlaceOfReport = ZString.Empty;
			AssertHasMessageError(declaration.CA_PlaceOfReportInfo, messageError);
			AssertNoMessageErrorContaining(declaration.CA_PlaceOfReportInfo, ListValidation.InvalidCodeMessageError);

			declaration.CA_PlaceOfReport = OfficeCode.ZZD_Code;
			AssertNoMessageError(declaration.CA_PlaceOfReportInfo, messageError);
			AssertNoMessageErrorContaining(declaration.CA_PlaceOfReportInfo, ListValidation.InvalidCodeMessageError);

			declaration.CA_PlaceOfReport = "0000";
			AssertHasMessageErrorContaining(declaration.CA_PlaceOfReportInfo, ListValidation.InvalidCodeMessageError);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.G7Export;
			declaration.CA_PlaceOfReport = ZString.Empty;
			AssertNoMessageError(declaration.CA_PlaceOfReportInfo, messageError);
			AssertNoMessageErrorContaining(declaration.CA_PlaceOfReportInfo, ListValidation.InvalidCodeMessageError);

			declaration.CA_PlaceOfReport = "0000";
			AssertHasMessageErrorContaining(declaration.CA_PlaceOfReportInfo, ListValidation.InvalidCodeMessageError);

			declaration.CA_PlaceOfReport = OfficeCode.ZZD_Code;
			AssertNoMessageErrorContaining(declaration.CA_PlaceOfReportInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCA_TransportDocumentNumber()
		{
			string messageError = MandatoryValidation.YouHaveNotEntered + " a Transport Document Number.";
			declaration.CA_TransportDocumentNumber = ZString.Empty;
			AssertHasMessageError(declaration.CA_TransportDocumentNumberInfo, messageError);

			declaration.CA_TransportDocumentNumber = "999023423";
			AssertNoMessageError(declaration.CA_TransportDocumentNumberInfo, messageError);

			declaration.CA_TransportDocumentNumber = "9990";
			AssertHasMessageErrorContaining(declaration.CA_TransportDocumentNumberInfo, ExportAddInfoJobDeclarationValidation.invalidCCNMessage);

			declaration.CA_TransportDocumentNumber = "9990A";
			AssertNoMessageErrorContaining(declaration.CA_TransportDocumentNumberInfo, ExportAddInfoJobDeclarationValidation.invalidCCNMessage);
		}

		public void TestCheckCA_ReasonForExportCode()
		{
			declaration.CA_ReasonForExportCode = "ZZ";
			AssertHasMessageErrorContaining(declaration.CA_ReasonForExportCodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.CA_ReasonForExportCode = ReasonForExportList.Codes.ContractorsEquipmentOneYearOrMore;
			AssertNoMessageErrorContaining(declaration.CA_ReasonForExportCodeInfo, ListValidation.InvalidCodeMessageError);

			declaration.CA_ReasonForExportCode = ReasonForExportList.Codes.GoodsSold;
			AssertNoMessageErrorContaining(declaration.CA_ReasonForExportCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestListReasonForExportCodes()
		{
			AssertEquals("03, 01, 02, 04, 05, 07, 06, 00, 08, 09, 10, 11, 26, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25", declaration.AddInfoLookups.ReasonForExportCodes.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		RefCusCodeList OfficeCode =>
			Factory.GetCachedValue("ExportAddInfoJobDeclarationValidationTest|OfficeCode", () =>
				{
					var helper = new UniversalReferenceTestDataHelper(Factory);
					helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
					var officeCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0999", "Customs Office Code for testing", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
					helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "ON");
					Factory.Save();
					return officeCode;
				});
	}
}
