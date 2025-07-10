using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.MX.Business.Testing
{
	class JobDeclarationValidationTest : BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_GoodsOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_GoodsOriginInfo, "X", GoodsRegionList.Codes._11);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_GoodsOrigin = ZString.Empty;
			AssertNoNotifications(declaration.JE_GoodsOriginInfo);
		}

		public void TestCheckJE_GoodsDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_GoodsDestinationInfo, "X", GoodsRegionList.Codes._11);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_GoodsDestination = ZString.Empty;
			AssertNoNotifications(declaration.JE_GoodsDestinationInfo);
		}

		public virtual void TestCheckJE_LocationOfGoods()
		{
			ReferenceTestDataHelper.CreateCustomsFacilitiesCodes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_LocationOfGoodsInfo, "CA1", "123");
		}
		public void TestCheckJE_SubLocationOfGoods()
		{
			ReferenceTestDataHelper.CreateCustomsFacilitiesCodes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_SubLocationOfGoodsInfo, "CA1", "123");
		}

		public void TestCheckJE_CustomsProfile()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_CustomsProfile = CustomsRegimeList.Codes.IMD;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CustomsProfileInfo, "X", "IMD");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_CustomsProfile = ZString.Empty;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_CustomsProfile = CustomsRegimeList.Codes.EXD;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CustomsProfileInfo, "Y", "EXD");
		}

		public void TestCheckJE_MessageSubType()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageErrorContaining(declaration.JE_MessageSubTypeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageSubType = MXDeclarationTypeList.Codes.A1;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoMessageErrorContaining(declaration.JE_MessageSubTypeInfo, MandatoryValidation.YouHaveNotEntered);

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_MessageSubTypeInfo, "X", "A1");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = ZString.Empty;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageErrorContaining(declaration.JE_MessageSubTypeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageSubType = MXDeclarationTypeList.Codes.V5;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoMessageErrorContaining(declaration.JE_MessageSubTypeInfo, MandatoryValidation.YouHaveNotEntered);

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_MessageSubTypeInfo, "Y", "V5");
		}
	}
}
