using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Moq.Protected;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class PackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRuleC0820_ForCW_MarksAndNos()
		{
			var messageError = "[C0820] This field is mandatory for any case besides Additional procedure F15.";

			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var package = GetPackage(true, ZString.Empty, "0000F16");
				AssertHasMessageError("A message error is expected for RuleC0820 for concession other than F15 when CW_MarksAndNos is empty.", package.CW_MarksAndNosInfo, messageError);

				package = GetPackage(true, "Marks", "0000F16");
				AssertNoMessageErrorContaining("No validation should run for RuleC0820 for concession other than F15 when CW_MarksAndNos is set.", package.CW_MarksAndNosInfo, messageError);

				package = GetPackage(false, ZString.Empty, "0000F16");
				AssertNoMessageErrorContaining("No validation should run for RuleC0820 for concession other than F15 when IsLinked is set to false.", package.CW_MarksAndNosInfo, messageError);

				package = GetPackage(true, ZString.Empty, "0000F15");
				AssertNoMessageErrorContaining("No validation should run for RuleC0820 when concession is F15.", package.CW_MarksAndNosInfo, messageError);

				package = GetPackage(true, ZString.Empty, "0000F16", MessageTypeList.Codes.Export);
				AssertNoMessageErrorContaining("No validation should run for RuleC0820 when declaration type is export.", package.CW_MarksAndNosInfo, messageError);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var package = GetPackage(true, ZString.Empty, "0000F15");
				AssertNoMessageErrorContaining("No validation should run for RuleC0820 for concession other than F15 when declaration is non UCC6.", package.CW_MarksAndNosInfo, messageError);
			}

			using (var context = new PackageValidationDeciderTestContext(declaration, isUCC6: true))
			{
				context.DisableRule(x => x.IsRuleC0820ActiveForCW_MarksAndNos);

				var package = GetPackage(true, ZString.Empty, "0000F16");
				AssertNoMessageErrorContaining("No validation is expected for RuleC0820 for concession other than F15 when CW_MarksAndNos is empty and Rule is disabled", package.CW_MarksAndNosInfo, messageError);
			}

			BasePackage GetPackage(bool isLinked, ZString marks, ZString procedure, string messageType = MessageTypeList.Codes.Import)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;
				var invoice = declaration.Invoices.AddNew();
				var line = invoice.InvoiceLines.AddNew();

				var package = declaration.Packages.AddNew();
				package.CW_MarksAndNos = marks;
				var linePackage1 = line.PackagesForInvoiceLinesForBindingOnly[0];
				linePackage1.IsLinked = isLinked;
				linePackage1.Package = package;
				line.JI_FormattedProcedure = procedure;
				package.Validation.ValidateCW_MarksAndNos();
				return package;
			}
		}

		public void TestCheckCW_MarksAndNos_EmptyPackType()
		{
			SetupZZBulkPackType();
			var declaration = Factory.New<JobDeclaration>();
			var package = (Package)declaration.Packages.AddNew();

			CombineAssertions(() =>
			{
				package.Validation.ValidateCW_MarksAndNos();
				AssertHasMessageErrorContaining("Marks and No's empty", package.CW_MarksAndNosInfo, MarksAndNosRequiredMessage);
				package.CW_PackType = RefCusCodeBulkPackageUnitType.BulkGas;
				package.Validation.ValidateCW_MarksAndNos();
				AssertNoMessageErrorContaining("Marks Not required for Bulk Type", package.CW_MarksAndNosInfo, MarksAndNosRequiredMessage);
			});
		}

		public void TestCheckCW_MarksAndNos_EmptyPackageMarksAndNumbersAlwaysRequiredValidationMessageCore()
		{
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();
			var package = (Package)mockDeclaration.Object.Packages.AddNew();

			CombineAssertions(() =>
			{
				package.Validation.ValidateCW_MarksAndNos();
				AssertHasMessageErrorContaining("Marks and No's empty", package.CW_MarksAndNosInfo, MarksAndNosRequiredMessage);
				mockDeclaration.Protected().Setup<ZString>("PackageMarksAndNumbersAlwaysRequiredValidationMessageCore").Returns(ZString.Empty);
				package.Validation.ValidateCW_MarksAndNos();
				AssertNoMessageErrorContaining("Marks and No's not required", package.CW_MarksAndNosInfo, MarksAndNosRequiredMessage);
			});
		}

		public void TestCheckCW_PackType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = (Package)declaration.Packages.AddNew();
			package.CW_PackType = "BB";
			AssertEquals(false, package.CW_PackTypeInfo.HasMessageError(ListValidation.InvalidCodeMessageError.ToString()));
		}

		public void TestCheckCW_PackType_ZZPackType()
		{
			SetupZZBulkPackType();
			var declaration = Factory.New<JobDeclaration>();
			var package = (Package)declaration.Packages.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(package.CW_PackTypeInfo, "BB", RefCusCodeBulkPackageUnitType.BulkGas);
		}

		void SetupZZBulkPackType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				RefCusCodeBulkPackageUnitType.BulkGas,
				"Bulk Gas",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue,
				Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk,
				"");

			Factory.Save();
		}

		const string MarksAndNosRequiredMessage = "marks are required";
	}
}
