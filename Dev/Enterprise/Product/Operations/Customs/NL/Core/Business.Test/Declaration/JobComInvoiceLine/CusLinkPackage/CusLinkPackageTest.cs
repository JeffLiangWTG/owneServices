using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(CusLinkPackage))]
class CusLinkPackageTest : NonPersistentBusinessObjectTestCase
{
	public void TestPackageNumber_Caption()
	{
		var linkPackage = (CusLinkPackage)GetNewBusinessObject();

		AssertCaptionAndFullDescription(linkPackage.PackageNumberInfo, "[UCC 6/11] Pack. No.", "[UCC 6/11] Package Number", "IMP");
		AssertCaptionAndFullDescription(linkPackage.PackageNumberInfo, "[UCC 6/11] Pack. No.", "[UCC 6/11] Package Number", "EXP");
	}

	public void TestPackageQuantity_Caption()
	{
		var linkPackage = (CusLinkPackage)GetNewBusinessObject();

		AssertCaptionAndFullDescription(linkPackage.PackQtyInfo, "[UCC 6/10] Pack. Qty.", "[UCC 6/10] Package Quantity", "IMP");
		AssertCaptionAndFullDescription(linkPackage.PackQtyInfo, "[UCC 6/10] Pack. Qty.", "[UCC 6/10] Package Quantity", "EXP");
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var package = declaration.Packages.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var collection = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<CusLinkPackage>();

		var linkPackage = collection.FirstOrDefault();
		linkPackage.IsLinked = true;

		return linkPackage;
	}

	void AssertCaptionAndFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedFullDescription, string messageType = "", string expectedShortCaption = "")
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(propertyInfo, null, [messageType == "IMP" ? JobDeclaration.CaptionKeyImportUCC6 : messageType == "EXP" ? JobDeclaration.CaptionKeyExportUCC6 : null]);
		CombineAssertions(() =>
		{
			if (!string.IsNullOrEmpty(expectedShortCaption))
			{
				AssertEquals($"{propertyInfo.Name} {messageType} ShortCaption", expectedShortCaption, resourceStringData.ShortCaption);
			}
			AssertEquals($"{propertyInfo.Name} {messageType} Caption", expectedCaption, resourceStringData.Caption);
			AssertEquals($"{propertyInfo.Name} {messageType} FullDescription", expectedFullDescription, resourceStringData.FullDescription);
		});
	}
}
