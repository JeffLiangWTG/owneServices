using System;
using System.Collections.Generic;
// using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderImportWizard))]
	public class InvoiceHeaderImportWizardTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportFirstPackage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var package = invoiceHeader.JobDeclaration.Packages.AddNew();
			invoiceLine.PackagesPivot.AddPivotFor(package);
			var cusLinkPackage = new InvoiceHeaderCusLinkPackage(invoiceHeader);
			cusLinkPackage.IsLinked = true;
			cusLinkPackage.Package = package;
			invoiceHeader.PackagesForInvoicesForBindingOnly.Add(cusLinkPackage);
			var collection = new InvoiceHeaderActiveCollection(declaration);
			var wizard = new InvoiceHeaderImportWizardForTest();
			wizard.Mapping[0].AddFileColumnIndex(0);
			wizard.Mapping[1].AddFileColumnIndex(1);
			wizard.fakeFileContent.Add(new[] { "true", "1234" });
			wizard.fakeFileContent.Add(new[] { "false", "5678" });
			wizard.fakeFileContent.Add(new[] { "", "2233" });

			UnitTestUserNotification.Instance.AddYesAnswer();
			UnitTestUserNotification.Instance.AddYesAnswer();
			wizard.ImportIntoCollection(collection);
			Assert(collection[1].FirstPackageIsLinked);
			AssertEquals(1234, collection[1].FirstPackageQty);
			Assert(collection[2].FirstPackageIsLinked);
			AssertEquals(5678, collection[2].FirstPackageQty);
			Assert(collection[3].FirstPackageIsLinked);
			AssertEquals(2233, collection[3].FirstPackageQty);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceHeaderImportWizardForTest();
		}

		public class InvoiceHeaderImportWizardForTest : InvoiceHeaderImportWizard
		{	
			static readonly BusinessObjectFactory factory = new BusinessObjectFactory();
			static readonly JobDeclaration declaration = factory.New<JobDeclaration>();

			public InvoiceHeaderImportWizardForTest() : base(GetCollectionInfo(), GetSettingsStorage(), new FileMapperForTest())
			{
				var factory = new BusinessObjectFactory();
				var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var package = invoiceHeader.JobDeclaration.Packages.AddNew();
				invoiceLine.PackagesPivot.AddPivotFor(package);
				var cusLinkPackage = new InvoiceHeaderCusLinkPackage(invoiceHeader);
				cusLinkPackage.IsLinked = true;
				cusLinkPackage.Package = package;
				invoiceHeader.PackagesForInvoicesForBindingOnly.Add(cusLinkPackage);
			}

			IImportCollectionInfo collectionIfo;
			public new IImportCollectionInfo CollectionInfo => collectionIfo ?? (collectionIfo = GetCollectionInfo());

			public List<string[]> fakeFileContent = new List<string[]>();

			protected override void ImportIntoBizObjCore(Action<BusinessObject, string, object> setValue, IEnumerable<ImportWizardMapping> mappedRecords, BusinessObject bizObj, string[] values)
			{
				var invoiceHeader = bizObj as JobComInvoiceHeader;
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var package = invoiceHeader.JobDeclaration.Packages.AddNew();
				invoiceLine.PackagesPivot.AddPivotFor(package);
				var cusLinkPackage = new InvoiceHeaderCusLinkPackage(invoiceHeader);
				cusLinkPackage.IsLinked = true;
				cusLinkPackage.Package = package;
				invoiceHeader.PackagesForInvoicesForBindingOnly.Add(cusLinkPackage);
				base.ImportIntoBizObjCore(setValue, mappedRecords, bizObj, values);
			}

			public override List<string[]> LoadFile(int startingRow, int maximumRows, bool forceFileLoad = false)
			{
				return fakeFileContent;
			}

			static IImportCollectionInfo GetCollectionInfo()
			{
				return new ImportCollectionInfoImpl(new InvoiceHeaderActiveCollection(declaration))
				{
					new ImportPropertyInfoImpl<JobComInvoiceHeader>(JobComInvoiceHeader.Schema.FirstPackageIsLinked) { HeaderText = "Is For Invoice Header?" },
					new ImportPropertyInfoImpl<JobComInvoiceHeader>(JobComInvoiceHeader.Schema.FirstPackageQty) { HeaderText = "Pack Quantity" },
				};
			}

			static ISettingsStorage GetSettingsStorage()
			{
				var mockery = new MockRepository(MockBehavior.Default);
				var settingsStorageStub = new Mock<ISettingsStorage>();
				settingsStorageStub.Setup(m => m.GetSavedSettings())
					.Returns(
					new string[] {
						JobComInvoiceHeader.Schema.FirstPackageIsLinked,
						JobComInvoiceHeader.Schema.FirstPackageQty,
					});
				return settingsStorageStub.Object;
			}
		}
	}
}
