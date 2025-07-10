using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PackageValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCW_PackType_MandatoryValidation()
	{
		package.CW_PackType = ZString.Empty;
		AssertHasMessageErrorContaining(package.CW_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);

		package.CW_PackType = "XX";
		AssertNoMessageErrorContaining(package.CW_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCW_PackQty_CannotBeZeroForTheSelectedPackType_ExportUCC6()
	{
		const string expectedErrorMessage = "Pack Qty cannot be zero for the selected Pack Type.";

		SetUpRefData();
		Factory.Save();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			package.CW_PackType = "VQ";
			package.CW_PackQty = 0;
			AssertNoMessageErrorContaining("When 'Pack Qty' is zero and 'Pack Type' is Bulk", package.CW_PackQtyInfo, expectedErrorMessage);

			package.CW_PackType = "AA";
			package.Validation.ValidateCW_PackQty();
			AssertHasMessageErrorContaining("When 'Pack Qty' is zero and 'Pack Type' is not Bulk", package.CW_PackQtyInfo, expectedErrorMessage);

			package.CW_PackQty = 1;
			AssertNoMessageErrorContaining("When 'Pack Qty' is not zero and 'Pack Type' is not Bulk", package.CW_PackQtyInfo, expectedErrorMessage);
		}
	}

	public void TestCheckCW_PackQty_CannotBeZeroForTheSelectedPackType_NonExportOrNonUCC6()
	{
		SetUpRefData();
		Factory.Save();

		package.CW_PackType = "AA";
		package.CW_PackQty = 0;

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = "EXP";
			package.Validation.ValidateCW_PackQty();
			AssertNoMessageErrors("When 'Message Type' is export but not UCC6, 'Pack Qty' is zero and 'Pack Type' is not Bulk", package.CW_PackQtyInfo);
		}

		declaration.JE_MessageType = "IMP";
		package.Validation.ValidateCW_PackQty();
		AssertNoMessageErrors("When 'Message Type' is import, 'Pack Qty' is zero and 'Pack Type' is not Bulk", package.CW_PackQtyInfo);
	}

	public void TestCheckCW_PackQty_MustBeZeroForTheSelectedPackType_ExportUCC6()
	{
		const string expectedErrorMessage = "Pack Qty must be zero for the selected Pack Type.";

		SetUpRefData();
		Factory.Save();

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			package.CW_PackType = "AA";
			package.CW_PackQty = 1;
			AssertNoMessageErrorContaining("When 'Pack Qty' is not zero and 'Pack Type' is not Bulk", package.CW_PackQtyInfo, expectedErrorMessage);

			package.CW_PackType = "VQ";
			package.Validation.ValidateCW_PackQty();
			AssertHasMessageErrorContaining("When 'Pack Qty' is not zero and 'Pack Type' is Bulk", package.CW_PackQtyInfo, expectedErrorMessage);

			package.CW_PackQty = 0;
			AssertNoMessageErrorContaining("When 'Pack Qty' is zero and 'Pack Type' is Bulk", package.CW_PackQtyInfo, expectedErrorMessage);
		}
	}

	public void TestCheckCW_PackQty_MustBeZeroForTheSelectedPackType_NonExportOrNonUCC6()
	{
		SetUpRefData();
		Factory.Save();

		package.CW_PackType = "VQ";
		package.CW_PackQty = 1;

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = "EXP";
			package.Validation.ValidateCW_PackQty();
			AssertNoMessageErrors("When 'Message Type' is export but not UCC6, 'Pack Qty' is not zero and 'Pack Type' is Bulk", package.CW_PackQtyInfo);
		}

		declaration.JE_MessageType = "IMP";
		package.Validation.ValidateCW_PackQty();
		AssertNoMessageErrors("When 'Message Type' is import, 'Pack Qty' is not zero and 'Pack Type' is Bulk", package.CW_PackQtyInfo);
	}

	public void TestCheckCW_MarksAndNosValidation()
	{
		const string expectedErrorMessage = "Marks field exceeds the maximum allowed length in the declaration message (42 characters).";
		const int expectedMaxLength512 = 512;
		const int expectedMaxLengthWhenAES42 = 42;

		AssertExceptionThrown("While MarksAndNos value length more than 512", typeof(MaxLengthExceededException), "The maximum length of 'CW_MarksAndNos' has been exceeded", () => package.CW_MarksAndNos = new ZString('0', 513), true);
		ClearExceptionReporter();

		using (TemporarilySetTransitionPeriod(isActive: true))
		{
			declaration.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			package.CW_MarksAndNos = new ZString('0', expectedMaxLength512);
			AssertNoMessageErrorContaining("When Import Declaration and AESTP is On", package.CW_MarksAndNosInfo, expectedErrorMessage);
		}

		using (TemporarilySetTransitionPeriod(isActive: false))
		{
			declaration.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			package.CW_MarksAndNos = new ZString('0', expectedMaxLength512);
			AssertNoMessageErrorContaining("When Import Declaration but AESTP is Off", package.CW_MarksAndNosInfo, expectedErrorMessage);
		}

		CombineAssertions("When Export Declaration is UCC6 ON and AESTP is On", () =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
			using (TemporarilySetTransitionPeriod(isActive: true))
			{
				declaration.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
				package.CW_MarksAndNos = new ZString('0', expectedMaxLengthWhenAES42);
				AssertNoMessageErrorContaining(package.CW_MarksAndNosInfo, expectedErrorMessage);

				package.CW_MarksAndNos += "1";
				AssertHasMessageErrorContaining(package.CW_MarksAndNosInfo, expectedErrorMessage);
			}
		});

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		using (TemporarilySetTransitionPeriod(isActive: false))
		{
			declaration.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
			package.CW_MarksAndNos = new ZString('0', expectedMaxLength512);

			AssertNoMessageErrorContaining("When Export Declaration is UCC6 ON But AESTP is Off", package.CW_MarksAndNosInfo, expectedErrorMessage);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			declaration.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
			package.CW_MarksAndNos = new ZString('0', expectedMaxLength512);

			AssertNoMessageErrorContaining("When Export Declaration is UCC6 Off", package.CW_MarksAndNosInfo, expectedErrorMessage);
		}

		declaration.JE_MessageType = ZString.Empty;
		package.CW_MarksAndNos = new ZString('0', expectedMaxLength512);
		AssertNoMessageErrorContaining("When Declaration without MessageType", package.CW_MarksAndNosInfo, expectedErrorMessage);

		var package2 = Factory.New<Package>();
		package2.CW_MarksAndNos = new ZString('0', expectedMaxLength512);
		AssertNoMessageErrorContaining("When Package without Declaration", package2.CW_MarksAndNosInfo, expectedErrorMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		package = (Package)declaration.Packages.AddNew();
	}

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
	}

	void ClearExceptionReporter() => ExceptionReporterTestListener.Instance.Clear();

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	IDisposable TemporarilySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

	JobDeclaration declaration;

	Package package;
}
