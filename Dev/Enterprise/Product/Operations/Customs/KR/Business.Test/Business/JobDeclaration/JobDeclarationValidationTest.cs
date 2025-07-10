using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	class JobDeclarationValidationTest : BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_MessageType_MessageErrorIfChangedAfterSendingMessage()
		{
			JobDeclarationValidationBaseOnlyTest.AssertErrorsAfterChangingJE_MessageType<JobDeclaration>(Factory, false, true, false);
		}

		public override void TestJE_ShipmentIncoTerm()
		{
			Declaration.JE_ShipmentIncoTerm = "XXX";
			AssertHasMessageErrorContaining(Declaration.JE_ShipmentIncoTermInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_ShipmentIncoTerm = IncotermList.Codes.CarriageAndInsurancePaidTo;
			AssertNoMessageErrors(Declaration.JE_ShipmentIncoTermInfo);
		}

		public override void TestPackagesActualPackageCount_Validation()
		{
			Declaration.DisableDefaultPackingInformation = true;
			Declaration.JE_TotalNoOfPacks = 100;
			Declaration.Validation.ValidatePackagesActualPackageCount();
			AssertNoNotifications(Declaration.PackagesActualPackageCountInfo);
		}

		public override void TestCheckJE_OH_ImporterUsingCustomsRule()
		{
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				base.TestCheckJE_OH_ImporterUsingCustomsRule();
			}
		}

		public virtual void TestJE_LocationOtherInformation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "11111111", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			Declaration.JE_LocationOtherInformation = "XXX";
			AssertHasMessageErrorContaining(Declaration.JE_LocationOtherInformationInfo, ListValidation.InvalidCodeMessageError);

			Declaration.JE_LocationOtherInformation = "11111111";
			AssertNoMessageErrors(Declaration.JE_LocationOtherInformationInfo);
		}

		public void TestCheckJE_MessageType()
		{
			var impError = "This declaration type is being developed. You will not able to save it now.";
			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertNoErrors(Declaration.JE_MessageTypeInfo);

			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertNoErrors(Declaration.JE_MessageTypeInfo);

			Declaration.JE_ApplicationCode = ZString.Empty;
			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertHasError(Declaration.JE_MessageTypeInfo, impError);
			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			Declaration.Validation.ValidateJE_MessageType();
			AssertHasError(Declaration.JE_MessageTypeInfo, impError);
			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			Declaration.Validation.ValidateJE_MessageType();
			AssertNoError(Declaration.JE_MessageTypeInfo, impError);

			KRCustomsRegistry.Instance.EnableToSaveImportDeclaration.SetTemporaryValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			Declaration.JE_ApplicationCode = ZString.Empty;
			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertNoError(Declaration.JE_MessageTypeInfo, impError);

			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.Carnet;
			AssertNoErrorContaining(Declaration.JE_MessageTypeInfo, ListValidation.InvalidCodeError);

			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
			AssertNoErrorContaining(Declaration.JE_MessageTypeInfo, ListValidation.InvalidCodeError);

			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			AssertNoErrorContaining(Declaration.JE_MessageTypeInfo, ListValidation.InvalidCodeError);

			Declaration.JE_MessageType = "XXX";
			AssertHasErrorContaining(Declaration.JE_MessageTypeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckJE_ApplicationCode()
		{
			Declaration.JE_ApplicationCode = ZString.Empty;
			AssertNoErrors(Declaration.JE_ApplicationCodeInfo);
			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertNoErrors(Declaration.JE_ApplicationCodeInfo);
			Declaration.JE_ApplicationCode = "XXX";
			AssertHasWarningContaining(Declaration.JE_ApplicationCodeInfo, ListValidation.InvalidCodeMessage);
		}

		JobDeclaration Declaration => (JobDeclaration)declaration;
	}
}
