using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using RepresentationTypeList = Enterprise.Customs.EU.Business.RepresentationTypeList;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusReconDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCRD_CustomsOffice()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.CRD_CustomsOfficeInfo, "Please enter an Authorization");
		}

		public void TestCheckCRD_PeriodFrom_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.CRD_PeriodFromInfo);
		}

		public void TestCheckCRD_PeriodFrom_NotInFuture()
		{
			const string message = "The Period can't be in the future.";
			var propertyInfo = declaration.CRD_PeriodFromInfo;
			CombineAssertions(() =>
			{
				propertyInfo.Value = ZDate.Today;
				AssertNoMessageError("Today", propertyInfo, message);

				propertyInfo.Value = ZDate.Today.AddDays(1);
				AssertHasMessageError("Future", propertyInfo, message);
			});
		}

		public void TestCheckCRD_PeriodTo_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.CRD_PeriodToInfo);
		}

		public void TestCRD_PeriodTo_NotBeforePeriodFrom()
		{
			const string message = "Period To cannot be before Period From.";
			declaration.CRD_PeriodFrom = ZDate.Today;

			CombineAssertions(() =>
			{
				declaration.CRD_PeriodTo = ZDate.Today.AddDays(-1);
				AssertHasMessageError("PeriodTo < PeriodFrom", declaration.CRD_PeriodToInfo, message);

				declaration.CRD_PeriodTo = ZDate.Today;
				AssertNoMessageError("PeriodTo = PeriodFrom", declaration.CRD_PeriodToInfo, message);

				declaration.CRD_PeriodTo = ZDate.Today.AddDays(1);
				AssertNoMessageError("PeriodTo > PeriodFrom", declaration.CRD_PeriodToInfo, message);
			});
		}

		public void TestCheckCRD_CPH_ReconClearanceAuthorisation()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.CRD_CPH_ReconClearanceAuthorisationInfo);
		}

		public void TestCRD_DeclarationType_Mandatory() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.CRD_DeclarationTypeInfo);

		public void TestCRD_DeclarantType_Mandatory() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.CRD_DeclarantTypeInfo);

		public void TestCRD_OA_DeclarantAddress_Mandatory() => ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.CRD_OA_DeclarantAddressInfo);

		public void TestCRD_OA_RepresentativeAddress_Mandatory() => AssertAddressMandatoryDependingOnDeclarantType(declaration.CRD_OA_RepresentativeAddressInfo, RepresentationTypeList.Codes._2Direct, RepresentationTypeList.Codes._3Indirect);

		public void TestCRD_OA_BuyingAgentAddress_Mandatory() => AssertAddressMandatoryDependingOnDeclarantType(declaration.CRD_OA_BuyingAgentAddressInfo, RepresentationTypeList.Codes._3Indirect, RepresentationTypeList.Codes._2Direct);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<CusReconDeclaration>();
		}
		CusReconDeclaration declaration;

		void AssertAddressMandatoryDependingOnDeclarantType(ZPropertyInfo targetInfo, ZString mandatoryCode, ZString notMandatoryCode)
		{
			CombineAssertions(() =>
			{
				declaration.CRD_DeclarantType = mandatoryCode;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

				declaration.CRD_DeclarantType = notMandatoryCode;
				ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo);
			});
		}
	}
}
