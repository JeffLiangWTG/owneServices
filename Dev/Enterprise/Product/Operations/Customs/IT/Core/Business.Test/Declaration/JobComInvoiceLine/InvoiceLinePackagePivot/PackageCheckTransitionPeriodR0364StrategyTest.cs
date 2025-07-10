using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal;
using Moq;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PackageCheckTransitionPeriodR0364StrategyTest : CommonPackageCheckStrategyTest
{
	public void TestConstructor()
	{
		var r0364CheckablePackageMock = new Mock<IR0364CheckablePackage>();
		r0364CheckablePackageMock.Setup(x => x.UnitCountInfo).Returns((ZPropertyInfo)null);

		AssertExceptionThrown<ArgumentNullException>("Exception when InvoiceLinePackagePivot is null", () => GetNewStrategy(null));
		AssertNoExceptionThrown("No exception when InvoiceLinePackagePivot is valid", () => GetNewStrategy(Factory.New<InvoiceLinePackagePivot>()));

		AssertExceptionThrown<ArgumentNullException>("Exception when UnitCountInfo is null", () => new PackageCheckTransitionPeriodR0364Strategy(r0364CheckablePackageMock.Object));
	}

	public void TestCheckCHC_NumberOfPacks_WhenOtherEntryLineHasSamePackTypeMarksAndQtyDifferentFromZero()
	{
		declaration.JE_MessageType = "EXP";
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT_IND, 0);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff2", "invoiceLine2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT_IND, 0);

		var expectedMessage = ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenOtherEntryLineHasSamePackTypeMarksAndQtyDifferentFromZero;
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
			{
				packagePivot1.Validation.ValidateAll();
				packagePivot2.Validation.ValidateAll();
				CombineAssertions("Message errors related to Number of Packages never appear in unmerged declarations", () =>
				{
					AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
					AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
				});

				var merger = new LineMerger(declaration);
				merger.DoMerge();

				packagePivot1.Validation.ValidateAll();
				packagePivot2.Validation.ValidateAll();
				CombineAssertions("In merged declaration, message errors related to Number of Packages appear regardless line position", () =>
				{
					AssertHasMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
					AssertHasMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
				});
			}

			using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: false))
			{
				packagePivot1.Validation.ValidateAll();
				packagePivot2.Validation.ValidateAll();
				CombineAssertions("In merged declaration, message errors related to Number of Packages will not appear when Tranistion period off", () =>
				{
					AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
					AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
				});
			}
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		{
			using (TemporarilySetAESTransitionPeriod(isInTransitionPeriod: true))
			{
				packagePivot1.Validation.ValidateAll();
				packagePivot2.Validation.ValidateAll();
				CombineAssertions("In merged declaration, message errors related to Number of Packages appear regardless line position", () =>
				{
					AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
					AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
				});
			}
		}
	}

	PackageCheckTransitionPeriodR0364Strategy GetNewStrategy(InvoiceLinePackagePivot currentPackage) => new PackageCheckTransitionPeriodR0364Strategy(currentPackage);

	IDisposable TemporarilySetAESTransitionPeriod(bool isInTransitionPeriod)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod
			, RefDataGroupingCodes.EuropeanUnionEUN
			, ZDate.Today
			, isInTransitionPeriod);
}
