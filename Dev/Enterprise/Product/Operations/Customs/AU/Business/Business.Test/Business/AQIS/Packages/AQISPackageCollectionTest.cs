using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISPackageCollection))]
	sealed class AQISPackageCollectionTest : AQISCollectionTest<AQISPackageCollection, AQISPackage>
	{
		public void TestLoadingOneAQISPackages()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_AQISPackageType_Hidden = "123/KG";
			AQISPackageCollection collection = new AQISPackageCollection(Factory, invoiceLine.AddInfo);
			collection.SplitAndAddAQISElements(invoiceLine.AddInfo.ZA_AQISPackageType_Hidden);
			AssertEquals("Collection count", 1, collection.Count);
			AssertEquals("Package Number 1", 123, collection[0].Number);
			AssertEquals("Package Number 1", "KG", collection[0].Type);
		}

		public void TestLoadingMultipleAQISPackages()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_AQISPackageType_Hidden = "123/KG,456/BX";
			AQISPackageCollection collection = new AQISPackageCollection(Factory, invoiceLine.AddInfo);
			collection.SplitAndAddAQISElements(invoiceLine.AddInfo.ZA_AQISPackageType_Hidden);
			AssertEquals("Collection count", 2, collection.Count);
			AssertEquals("Package Number 1 not empty", false, collection[0].Number.IsEmpty);
			AssertEquals("Package Number 1 type not empty", false, collection[0].Type.IsEmpty);
			AssertEquals("Package Number 2 not empty", false, collection[1].Number.IsEmpty);
			AssertEquals("Package Number 2 type not empty", false, collection[1].Type.IsEmpty);
		}

		public void TestGetNewAQISPackageString()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AQISPackageCollection collection = new AQISPackageCollection(Factory, invoiceLine.AddInfo);

			AQISPackage package1 = new AQISPackage(Factory);
			package1.Number = 123;
			package1.Type = "KG";
			collection.Add(package1);
			collection.ReBuildAndSaveAQISElements();
			AssertEquals("GetNewAQISPackageString", "123/KG", invoiceLine.AddInfo.ZA_AQISPackageType_Hidden);

			AQISPackage package2 = new AQISPackage(Factory);
			package2.Number = 567;
			package2.Type = "BX";
			collection.Add(package2);
			collection.ReBuildAndSaveAQISElements();
			AssertEquals("GetNewAQISPackageString", true, invoiceLine.AddInfo.ZA_AQISPackageType_Hidden.Contains("123/KG"));
			AssertEquals("GetNewAQISPackageString", true, invoiceLine.AddInfo.ZA_AQISPackageType_Hidden.Contains("567/BX"));
			AssertEquals("GetNewAQISPackageString", false, invoiceLine.AddInfo.ZA_AQISPackageType_Hidden.EndsWith(","));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AQISPackage(Factory);

		protected override AQISPackageCollection GetCollectionToTest() => new AQISPackageCollection(Factory, JobDeclaration.New(Factory).AddInfo);
	}
}
