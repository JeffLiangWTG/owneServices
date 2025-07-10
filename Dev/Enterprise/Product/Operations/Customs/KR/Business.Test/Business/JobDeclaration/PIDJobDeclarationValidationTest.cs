using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PIDJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJE_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Messaging.Constants.ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			declaration.JE_CustomsOffice = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_CustomsOfficeInfo);

			declaration.JE_CustomsOffice = "XXX";
			AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CustomsOffice = "010";
			AssertNoMessageErrors(declaration.JE_CustomsOfficeInfo);
		}

		public void TestJE_CustomsDivision()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Messaging.Constants.ZZ.NKCodeType.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			declaration.JE_CustomsDivision = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_CustomsDivisionInfo);

			declaration.JE_CustomsDivision = "XX";
			AssertHasMessageErrorContaining(declaration.JE_CustomsDivisionInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CustomsDivision = "20";
			AssertNoMessageErrors(declaration.JE_CustomsDivisionInfo);
		}

		public void TestJE_OH_Importer()
		{
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KR1", "COMPANYName");
			declaration.JE_OH_Importer = orgHeader.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJE_RL_NKOrigin()
		{
			declaration.JE_RL_NKOrigin = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_RL_NKOriginInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_RL_NKOrigin = "XXXXX";
			AssertEquals(1, declaration.JE_RL_NKPortOfLoadingInfo.Notifications.Count());
			AssertHasMessageErrorContaining(declaration.JE_RL_NKOriginInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_RL_NKOrigin = "KRSEL";
			AssertNoMessageErrors(declaration.JE_RL_NKOriginInfo);
		}

		public void TestJE_MessageSubType()
		{
			declaration.JE_MessageSubType = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_MessageSubTypeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = "XX";
			AssertHasMessageErrorContaining(declaration.JE_MessageSubTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_MessageSubType = StayPeriodCodeList.Codes._01;
			AssertNoMessageErrors(declaration.JE_MessageSubTypeInfo);
		}

		public void TestJE_RL_NKPortOfLoading()
		{
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_RL_NKPortOfLoading = "XXXXX";
			AssertEquals(1, declaration.JE_RL_NKPortOfLoadingInfo.Notifications.Count());
			AssertHasMessageErrorContaining(declaration.JE_RL_NKPortOfLoadingInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_RL_NKPortOfLoading = "KRSEL";
			AssertNoMessageErrors(declaration.JE_RL_NKPortOfLoadingInfo);
		}

		public void TestPIDFreightAmount()
		{
			var validation = declaration.Validation as PIDJobDeclarationValidation;
			validation.ValidatePIDFreightAmount();
			AssertEquals(0m, declaration.PIDFreightAmount);
			AssertHasMessageErrorContaining(declaration.PIDFreightAmountInfo, MandatoryValidation.ValueCannotBeZero);

			var invoice = declaration.Invoices.AddNew();
			declaration.PIDFreightAmount = -1m;
			validation.ValidatePIDFreightAmount();
			AssertHasMessageErrorContaining(declaration.PIDFreightAmountInfo, MandatoryValidation.ValueCannotBeNegative);

			declaration.PIDFreightAmount = 1m;
			validation.ValidatePIDFreightAmount();
			AssertNoMessageErrors(declaration.PIDFreightAmountInfo);
		}

		public void TestJE_OH_ShippingLine()
		{
			declaration.JE_OH_ShippingLine = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_ShippingLineInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KR1", "COMPANYName");
			declaration.JE_OH_ShippingLine = orgHeader.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_ShippingLineInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestDeliveryOrPickupCartageCoPK()
		{
			var validation = declaration.Validation as PIDJobDeclarationValidation;
			validation.ValidateDeliveryOrPickupCartageCoPK();
			AssertHasMessageErrorContaining(declaration.DeliveryOrPickupCartageCoPKInfo, MandatoryValidation.YouHaveNotEntered);

			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KR1", "COMPANYName");
			declaration.DeliveryOrPickupCartageCoPK = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(declaration.DeliveryOrPickupCartageCoPKInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJE_ContainerMode()
		{
			declaration.JE_ContainerMode = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_ContainerModeInfo);
		}

		public void TestCheckPIDWeapon()
		{
			AssertDecQuestion("PIDWeapon");
		}
		public void TestCheckPIDDrug()
		{
			AssertDecQuestion("PIDDrug");
		}
		public void TestCheckPIDAnimal()
		{
			AssertDecQuestion("PIDAnimal");
		}
		public void TestCheckPIDEndangeredItems()
		{
			AssertDecQuestion("PIDEndangeredItems");
		}
		public void TestCheckPIDCounterfeit()
		{
			AssertDecQuestion("PIDCounterfeit");
		}
		public void TestCheckPIDCommercialUseItems()
		{
			AssertDecQuestion("PIDCommercialUseItems");
		}
		public void TestCheckPIDExcessTimeLimitItems()
		{
			AssertDecQuestion("PIDExcessTimeLimitItems");
		}
		public void TestCheckPIDPornography()
		{
			AssertDecQuestion("PIDPornography");
		}

		void AssertDecQuestion(string obj)
		{
			string infoName = obj + "Info";
			ZPropertyInfo info = (ZPropertyInfo)declaration.GetPropertyValue(infoName);
			declaration.SetPropertyValue(obj, ZString.Empty);
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

			declaration.SetPropertyValue(obj, (ZString)YesNoList.Codes.Yes);
			AssertNoNotifications(info);

			declaration.SetPropertyValue(obj, (ZString)YesNoList.Codes.No);
			AssertNoNotifications(info);

			declaration.SetPropertyValue(obj, (ZString)"A");
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
		}
		JobDeclaration declaration;
	}
}
