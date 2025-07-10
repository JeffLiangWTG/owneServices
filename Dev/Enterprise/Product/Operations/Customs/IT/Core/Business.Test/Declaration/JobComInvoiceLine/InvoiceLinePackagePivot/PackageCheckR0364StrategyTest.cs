using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PackageCheckR0364StrategyTest : CommonPackageCheckStrategyTest
{
	public void TestConstructor()
	{
		var r0364CheckablePackageMock = new Mock<IR0364CheckablePackage>();
		r0364CheckablePackageMock.Setup(x => x.UnitCountInfo).Returns((ZPropertyInfo)null);

		AssertExceptionThrown<ArgumentNullException>("Exception when InvoiceLinePackagePivot is null", () => GetNewStrategy(null));
		AssertNoExceptionThrown("No exception when InvoiceLinePackagePivot is valid", () => GetNewStrategy(Factory.New<InvoiceLinePackagePivot>()));

		AssertExceptionThrown<ArgumentNullException>("Exception when UnitCountInfo is null", () => new PackageCheckTransitionPeriodR0364Strategy(r0364CheckablePackageMock.Object));
	}

	public void TestPackageNumberZeroWithPackTypeBulkOrBreakBulk_DifferentEntryLine()
		=> SetupPackagesAndAssertNumberOfPacksZeroWithPackTypeBulkOrBreakBulkAndMergedLines(expectedMergedLines: 2);

	public void TestPackageNumberZeroWithPackTypeBulkOrBreakBulk_SameEntryLine()
	{
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		SetupPackagesAndAssertNumberOfPacksZeroWithPackTypeBulkOrBreakBulkAndMergedLines(expectedMergedLines: 1);
	}

	void SetupPackagesAndAssertNumberOfPacksZeroWithPackTypeBulkOrBreakBulkAndMergedLines(int expectedMergedLines)
	{
		declaration.JE_MessageType = "EXP";
		var expectedMessage = ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenOtherLineWithSameMarksAndBulkPackType;

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT_IND, 0);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT_IND, 0);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
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

			AssertEquals("Entry lines count", expectedMergedLines, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("When no other line with Quantiy greater than zero.", () =>
			{
				AssertHasMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
				AssertHasMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
			});

			invoiceLine2.PackagesPivot.RemoveAndDeleteAll();
			packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT_IND, 4);
			merger.DoMerge();

			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("When same Marks and a line with quanity greater than zero", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
			});

			invoiceLine2.PackagesPivot.RemoveAndDeleteAll();
			packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT_B, 4);
			merger.DoMerge();

			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("When different Marks and a line with quanity greater than zero", () =>
			{
				AssertHasMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		{
			packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT_IND, 0);
			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("When no other line with Quantiy greater than zero.", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
			});
		}
	}

	PackageCheckTransitionPeriodR0364Strategy GetNewStrategy(InvoiceLinePackagePivot currentPackage) => new PackageCheckTransitionPeriodR0364Strategy(currentPackage);
}
