using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PackageCheckRN22StrategyTest : CommonPackageCheckStrategyTest
{
	public void TestConstructor()
	{
		var rN22CheckablePackageMock = new Mock<IRN22CheckablePackage>();
		rN22CheckablePackageMock.Setup(x => x.UnitCountInfo).Returns((ZPropertyInfo)null);

		AssertExceptionThrown<ArgumentNullException>("Exception when InvoiceLinePackagePivot is null", () => GetNewStrategy(null));
		AssertNoExceptionThrown("No exception when InvoiceLinePackagePivot is valid", () => GetNewStrategy(Factory.New<InvoiceLinePackagePivot>()));

		AssertExceptionThrown<ArgumentNullException>("Exception when UnitCountInfo is null", () => new PackageCheckRN22Strategy(rN22CheckablePackageMock.Object));
	}

	public void TestCheckCHC_NumberOfPacks_WhenOtherEntryLineExistsWithSamePackTypeAndMarksAndNosHavingValidNumberOfPacks()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT_IND, 0);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff2", "invoiceLine2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT_IND, 1);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		{
			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("Message errors related to Number of Packages never appear in unmerged declarations", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
				AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
			});

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("Number of Packages error does not appear when at least one Entry line with same PackType and Marks & Nos has it", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
				AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
			});

			invoiceLine2.PackagesPivot.RemoveAndDeleteAll();
			packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT_B, 0);
			merger.DoMerge();

			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("Number of Packages error must appear when no other Entry line with same PackType and Marks & Nos has it", () =>
			{
				AssertHasMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
				AssertHasMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			packagePivot1.Validation.ValidateAll();
			packagePivot2.Validation.ValidateAll();
			CombineAssertions("For Ucc6, Number of Packages error must not appear even when no other Entry line with same PackType and Marks & Nos has it", () =>
			{
				AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
				AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
			});
		}
	}

	PackageCheckRN22Strategy GetNewStrategy(IRN22CheckablePackage currentPackage) => new PackageCheckRN22Strategy(currentPackage);
}
