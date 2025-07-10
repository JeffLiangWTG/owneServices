using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PackageCheckR0219StrategyTest : CommonPackageCheckStrategyTest
{
	public void TestConstructor()
	{
		var r0219CheckablePackageMock = new Mock<IR0219CheckablePackage>();
		r0219CheckablePackageMock.Setup(x => x.UnitCountInfo).Returns((ZPropertyInfo)null);

		AssertExceptionThrown<ArgumentNullException>("Exception when InvoiceLinePackagePivot is null", () => GetNewStrategy(null));
		AssertNoExceptionThrown("No exception when InvoiceLinePackagePivot is valid", () => GetNewStrategy(Factory.New<InvoiceLinePackagePivot>()));

		AssertExceptionThrown<ArgumentNullException>("Exception when UnitCountInfo is null", () => new PackageCheckR0219Strategy(r0219CheckablePackageMock.Object));
	}

	public void TestCheckEntryLinesPackageNumbersZero_WithSingleInvoiceLine()
	{
		declaration.JE_MessageType = "EXP";
		var expectedMessage = ValidationCaptions.Package.NoOtherPackageHavePackageNumberGreaterThanZero;
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";

			var invoiceLine = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine");
			var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine, packageCT_B, 0);

			var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine, packageCT_IND, 1);

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
			CombineAssertions("When one pack in same entry is Zero quantity and other are not.", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
				AssertHasMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
			});

			packagePivot2.CHC_NumberOfPacks = 0;
			merger.DoMerge();

			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("When all packs in same entry are Zero quantity.", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
			});

			packagePivot1.CHC_NumberOfPacks = 2;
			merger.DoMerge();

			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("When other pack in same entry is Zero quantity and first one is not.", () =>
			{
				AssertHasMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
			});

			packagePivot1.CHC_NumberOfPacks = 3;
			packagePivot2.CHC_NumberOfPacks = 3;
			merger.DoMerge();

			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("When all packs in same entry are Non-zero quantity.", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, expectedMessage);
			});
		}
	}

	public void TestCheckEntryLinesPackageNumbersZero_WithMultipleInvoiceLines()
	{
		declaration.JE_MessageType = "EXP";
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		var expectedMessage = ValidationCaptions.Package.NoOtherPackageHavePackageNumberGreaterThanZero;
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";

			var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine1");
			var packagePivot1A = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT_IND, 1);
			var packagePivot1B = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT_B, 0);

			var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine1");
			var packagePivot2A = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT_IND, 0);
			var packagePivot2B = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT_B, 0);

			packagePivot1A.Validation.ValidateAll();
			packagePivot1B.Validation.ValidateAll();
			packagePivot2A.Validation.ValidateAll();
			packagePivot2B.Validation.ValidateAll();
			CombineAssertions("Message errors related to Number of Packages never appear in unmerged declarations", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1A.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot1B.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2A.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2B.CHC_NumberOfPacksInfo, expectedMessage);
			});

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Entry lines count", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			packagePivot1A.Validation.ValidateAll();
			packagePivot1B.Validation.ValidateAll();
			packagePivot2A.Validation.ValidateAll();
			packagePivot2B.Validation.ValidateAll();
			CombineAssertions("When one pack in same entry is not Zero quantity and other are zero", () =>
			{
				AssertHasMessageErrorContaining(packagePivot1A.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot1B.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2A.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2B.CHC_NumberOfPacksInfo, expectedMessage);
			});

			packagePivot1A.CHC_NumberOfPacks = 0;
			merger.DoMerge();

			packagePivot1A.Validation.ValidateAll();
			packagePivot1B.Validation.ValidateAll();
			packagePivot2A.Validation.ValidateAll();
			packagePivot2B.Validation.ValidateAll();
			CombineAssertions("When all packs in same entry are Zero quantity.", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1A.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot1B.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2A.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2B.CHC_NumberOfPacksInfo, expectedMessage);
			});

			packagePivot1A.CHC_NumberOfPacks = 2;
			packagePivot2B.CHC_NumberOfPacks = 2;
			merger.DoMerge();

			packagePivot1A.Validation.ValidateAll();
			packagePivot1B.Validation.ValidateAll();
			packagePivot2A.Validation.ValidateAll();
			packagePivot2B.Validation.ValidateAll();
			CombineAssertions("When all packs in same entry are not zero quanity", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1A.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot1B.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2A.CHC_NumberOfPacksInfo, expectedMessage);
				AssertNoMessageErrorContaining(packagePivot2B.CHC_NumberOfPacksInfo, expectedMessage);
			});
		}
	}

	PackageCheckR0219Strategy GetNewStrategy(IR0219CheckablePackage currentPackage) => new PackageCheckR0219Strategy(currentPackage);
}
