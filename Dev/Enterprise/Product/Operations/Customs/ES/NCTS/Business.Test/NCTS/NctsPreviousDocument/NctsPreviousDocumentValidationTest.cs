using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsPreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var previoucDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS;
			helper.CreateNewOrGetExistingCusCodeType(previoucDocumentType, "Previous Documents Transit NCTS (BOX40)");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, previoucDocumentType, "DUA", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, previoucDocumentType, "SUM", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.SummaryDeclaration;
				previousDocument.CSI_Code = "DUA";
				previousDocument.Validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, MandatoryValidation.DoNotEntered);

				previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures;
				previousDocument.CSI_Code = "SUM";
				previousDocument.Validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, "Please do not enter 'SUM' Type when Class is not 'X'");

				previousDocument.CSI_Code = string.Empty;
				previousDocument.Validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "AAA";
				previousDocument.Validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckCSI_SubType()
		{
			CombineAssertions(() =>
			{
				previousDocument.Validation.ValidateCSI_SubType();
				AssertHasMessageErrorContaining(previousDocument.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				previousDocument.Validation.ValidateCSI_SubType();
				AssertNoMessageErrorContaining(previousDocument.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				previousDocument.CSI_SubType = "AAA";
				previousDocument.Validation.ValidateCSI_SubType();
				AssertHasMessageErrorContaining(previousDocument.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS;
			var dataGrouping = Core.Constants.CountryCodes.Spain;
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping, dataGrouping, eun);
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Previous Documents Transit NCTS (BOX40)");

			var refCusCodeList = helper.CreateCusCodeList(dataGrouping, codeType, "SD1", "SD1 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList.PK, RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item);
			Factory.Save();

			CombineAssertions(() =>
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("No message error when CSI_Code is empty", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "SD1";
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageErrorContaining("message error when CSI_Code is in database", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				previousDocument.CSI_Code = "AAA";
				previousDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageErrorContaining("No message error when CSI_Code is not in database", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_LineNo()
		{
			previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.SummaryDeclaration;
			previousDocument.CSI_ReferenceNumber = "AAAAAAAAAAA";
			previousDocument.CSI_LineNo = ZInt.Zero;
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertHasMessageErrorContaining(previousDocument.CSI_LineNoInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_LineNo = 100000;
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertHasMessageErrorContaining(previousDocument.CSI_LineNoInfo, "Line No. should be between 1 and 99999.");

			previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.SummaryDeclaration;
			previousDocument.CSI_ReferenceNumber = "AAAAAAAAAAA";
			previousDocument.CSI_LineNo = 1;
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertNoMessageErrorContaining(previousDocument.CSI_LineNoInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures;
			previousDocument.CSI_LineNo = 1;
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertHasMessageErrorContaining(previousDocument.CSI_LineNoInfo, MandatoryValidation.DoNotEntered);

			previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.SummaryDeclaration;
			previousDocument.CSI_ReferenceNumber = "AA";
			previousDocument.CSI_LineNo = 1;
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertHasMessageErrorContaining(previousDocument.CSI_LineNoInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestCheckCSI_LineNoPhase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertNoMessageErrors(previousDocument.CSI_LineNoInfo);
		}

		public void TestCSI_CodeDH7()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var previoucDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS;
			helper.CreateNewOrGetExistingCusCodeType(previoucDocumentType, "Previous Documents Transit NCTS (BOX40)");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, previoucDocumentType, "DH7", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();

			previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.SummaryDeclaration;
			previousDocument.CSI_Code = NctsPreviousDocumentTypeCodeList.Codes.MrnOfLowValueDeclaration;
			previousDocument.Validation.ValidateCSI_Code();

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, "Please do not enter 'DH7' Type when Class is not 'Z'");

				previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
				previousDocument.Validation.ValidateCSI_Code();
				AssertNoMessageErrorContaining(previousDocument.CSI_CodeInfo, "Please do not enter 'DH7' Type when Class is not 'Z'");
			});
		}

		public void TestCSI_ReferenceNumberDH7()
		{
			previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			previousDocument.CSI_Code = NctsPreviousDocumentTypeCodeList.Codes.MrnOfLowValueDeclaration;
			previousDocument.CSI_ReferenceNumber = "21ES00";
			previousDocument.Validation.ValidateCSI_ReferenceNumber();

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, "Reference should be a valid MRN");

				previousDocument.CSI_ReferenceNumber = "22ES00999912345678";
				previousDocument.Validation.ValidateCSI_Code();
				AssertNoMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, "Reference should be a valid MRN");
			});
		}

		public void TestCSI_LineNoDH7()
		{
			previousDocument.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			previousDocument.CSI_Code = NctsPreviousDocumentTypeCodeList.Codes.MrnOfLowValueDeclaration;
			previousDocument.Validation.ValidateCSI_LineNo();
			AssertNoMessageErrors(previousDocument.CSI_LineNoInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			previousDocument = Factory.New<NctsPreviousDocument>();
			goodsItem.PreviousDocuments.Add(previousDocument);
		}

		NctsHeader nctsHeader;
		NctsPreviousDocument previousDocument;
	}
}
