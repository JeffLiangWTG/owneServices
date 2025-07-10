using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class InvoiceLinePackageValidationTest : TestCaseWithFactory
{
	public void TestCheckIsLinked_YouHaveSelectedMoreThanOnePackageType_WhenDeclarationIsNotUCC6()
	{
		var warningMessage = "You have selected more than one package type. Only one will be sent to Customs.";

		using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, nameof(DeclarationConfiguration.IsUCC6) + "Core", false))
		{
			(var lineLinkPackage1, var lineLinkPackage2) = SetUpPackagesPivotWithDeclaration();
			lineLinkPackage1.Package.CW_PackType = "AA";
			lineLinkPackage2.Package.CW_PackType = "BB";
			CombineAssertions("When the invoice line has more than 1 different package type, but they are not linked", () =>
			{
				lineLinkPackage1.IsLinked = false;
				lineLinkPackage2.IsLinked = false;
				AssertNoWarningContaining(nameof(lineLinkPackage1), lineLinkPackage1.IsLinkedInfo, warningMessage);
				AssertNoWarningContaining(nameof(lineLinkPackage2), lineLinkPackage2.IsLinkedInfo, warningMessage);
			});

			CombineAssertions("When the invoice line has more than 1 different package type, but only one is linked", () =>
			{
				lineLinkPackage1.IsLinked = false;
				lineLinkPackage2.IsLinked = true;
				AssertNoWarningContaining(nameof(lineLinkPackage1), lineLinkPackage1.IsLinkedInfo, warningMessage);
				AssertNoWarningContaining(nameof(lineLinkPackage2), lineLinkPackage2.IsLinkedInfo, warningMessage);
			});

			CombineAssertions("When the invoice line has more than 1 different package type, and they are all linked", () =>
			{
				lineLinkPackage1.IsLinked = true;
				lineLinkPackage2.IsLinked = true;
				AssertHasWarningContaining(nameof(lineLinkPackage1), lineLinkPackage1.IsLinkedInfo, warningMessage);
				AssertHasWarningContaining(nameof(lineLinkPackage2), lineLinkPackage2.IsLinkedInfo, warningMessage);
			});

			lineLinkPackage1.Package.CW_PackType = "CC";
			lineLinkPackage2.Package.CW_PackType = "CC";
			CombineAssertions("When the invoice line does not have more than 1 different package type", () =>
			{
				lineLinkPackage1.IsLinked = true;
				lineLinkPackage2.IsLinked = true;
				AssertNoWarningContaining(nameof(lineLinkPackage1), lineLinkPackage1.IsLinkedInfo, warningMessage);
				AssertNoWarningContaining(nameof(lineLinkPackage2), lineLinkPackage2.IsLinkedInfo, warningMessage);
			});
		}
	}

	public void TestCheckIsLinked_YouHaveSelectedMoreThanOnePackageType_WhenDeclarationIsUCC6()
	{
		using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, nameof(DeclarationConfiguration.IsUCC6) + "Core", true))
		{
			(var lineLinkPackage1, var lineLinkPackage2) = SetUpPackagesPivotWithDeclaration();
			lineLinkPackage1.Package.CW_PackType = "AA";
			lineLinkPackage2.Package.CW_PackType = "BB";
			CombineAssertions("When the invoice line has more than 1 different package type, and they are all linked", () =>
			{
				lineLinkPackage1.IsLinked = true;
				lineLinkPackage2.IsLinked = true;
				AssertNoWarnings(nameof(lineLinkPackage1), lineLinkPackage1.IsLinkedInfo);
				AssertNoWarnings(nameof(lineLinkPackage2), lineLinkPackage2.IsLinkedInfo);
			});
		}
	}

	public void TestCheckIsLinked_YouHaveSelectedMoreThan99DifferentPackageType_WhenDeclarationIsUCC6()
	{
		var messageError = "You have selected more than 99 different package types in all the invoice lines linked to the entry line.";

		using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, nameof(DeclarationConfiguration.IsUCC6) + "Core", true))
		{
			var declaration = Factory.New<JobDeclaration>();
			var packingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();
			for (int i = 0; i < 100; i++)
			{
				packingGroup.Packages.AddNew().CW_PackType = i.ToString();
			}
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var packagePivotCollection1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var packagePivotCollection2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly;
			invoiceLine2.JI_CL = entryLine.PK;

			using (new DisposableAction(() => Factory.SuspendValidation(), () => Factory.ResumeValidation()))
			{
				for (int i = 0; i < 99; i++)
				{
					var packagePivotItem = packagePivotCollection1[i];
					packagePivotItem.Package = packingGroup.Packages[i];
					packagePivotItem.IsLinked = true;
				}
			}

			var invoiceLinePackagePivot1 = (BaseCusLinkPackage)packagePivotCollection1.First();
			var invoiceLinePackagePivot2 = (BaseCusLinkPackage)packagePivotCollection2.Last();

			CombineAssertions("When the entry line has up to 99 different package types selected", () =>
			{
				AssertEquals("EntryLine PackagingDetails distinct count", 99, entryLine.PackagingDetails.Select(x => x.Package.CW_PackType).Distinct().Count());
				invoiceLinePackagePivot1.Validation.ValidateIsLinked();
				invoiceLinePackagePivot2.Validation.ValidateIsLinked();
				AssertNoMessageErrorContaining(nameof(invoiceLinePackagePivot1), invoiceLinePackagePivot1.IsLinkedInfo, messageError);
				AssertNoMessageErrorContaining(nameof(invoiceLinePackagePivot2), invoiceLinePackagePivot2.IsLinkedInfo, messageError);
			});

			CombineAssertions("When the entry line has more than 99 different package types selected", () =>
			{
				invoiceLinePackagePivot2.IsLinked = true;
				AssertEquals("EntryLine PackagingDetails distinct count", 100, entryLine.PackagingDetails.Select(x => x.Package.CW_PackType).Distinct().Count());
				invoiceLinePackagePivot1.Validation.ValidateIsLinked();
				invoiceLinePackagePivot2.Validation.ValidateIsLinked();
				AssertHasMessageErrorContaining(nameof(invoiceLinePackagePivot1), invoiceLinePackagePivot1.IsLinkedInfo, messageError);
				AssertHasMessageErrorContaining(nameof(invoiceLinePackagePivot2), invoiceLinePackagePivot2.IsLinkedInfo, messageError);
			});

			CombineAssertions("When the entry line has more than 99 packages selected with the same package type", () =>
			{
				using (new DisposableAction(() => Factory.SuspendValidation(), () => Factory.ResumeValidation()))
				{
					for (int i = 0; i < 100; i++)
					{
						packingGroup.Packages[i].CW_PackType = "AA";
					}
				}
				AssertEquals("EntryLine PackagingDetails distinct count", 1, entryLine.PackagingDetails.Select(x => x.Package.CW_PackType).Distinct().Count());
				invoiceLinePackagePivot1.Validation.ValidateIsLinked();
				invoiceLinePackagePivot2.Validation.ValidateIsLinked();
				AssertNoMessageErrorContaining(nameof(invoiceLinePackagePivot1), invoiceLinePackagePivot1.IsLinkedInfo, messageError);
				AssertNoMessageErrorContaining(nameof(invoiceLinePackagePivot2), invoiceLinePackagePivot2.IsLinkedInfo, messageError);
			});

			CombineAssertions("When invoice line has not a linked entry line", () =>
			{
				invoiceLine1.JI_CL = invoiceLine2.JI_CL = ZGuid.Empty;
				entryLine.RefreshInvoiceLines();
				AssertEquals("EntryLine PackagingDetails distinct count", 0, entryLine.PackagingDetails.Select(x => x.Package.CW_PackType).Distinct().Count());
				invoiceLinePackagePivot1.Validation.ValidateIsLinked();
				invoiceLinePackagePivot2.Validation.ValidateIsLinked();
				AssertNoMessageErrorContaining(nameof(invoiceLinePackagePivot1), invoiceLinePackagePivot1.IsLinkedInfo, messageError);
				AssertNoMessageErrorContaining(nameof(invoiceLinePackagePivot2), invoiceLinePackagePivot2.IsLinkedInfo, messageError);
			});
		}
	}

	#region Implementation

	(BaseCusLinkPackage PackagePivot1, BaseCusLinkPackage PackagePivot2) SetUpPackagesPivotWithDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var packingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();
		var package1 = packingGroup.Packages.AddNew();
		var package2 = packingGroup.Packages.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var linePackageCollection = invoiceLine.PackagesForInvoiceLinesForBindingOnly;
		var lineLinkPackage1 = linePackageCollection[0];
		lineLinkPackage1.Package = package1;
		var lineLinkPackage2 = linePackageCollection[1];
		lineLinkPackage2.Package = package2;
		return (lineLinkPackage1, lineLinkPackage2);
	}

	#endregion
}
