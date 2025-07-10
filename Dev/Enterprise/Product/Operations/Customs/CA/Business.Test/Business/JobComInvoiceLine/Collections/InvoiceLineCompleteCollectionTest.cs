using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	sealed class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestRecalculatePageNumberWhenDeleteItem()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceDisplaySequence = 1;
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.CA_PageNumber = 1;

			var line2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line2.CA_PageNumber = 2;

			var completeCollection = new InvoiceLineCompleteCollection(jobDeclaration);
			var viewCollection = new InvoiceLineViewCollection(jobDeclaration);
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				((INeedRow)line2).Row.Delete();
				((IBusiness)line1).DeleteForDataRefresh();
				Factory.ForcePublishForDataRefresh(line1);
			});
		}

		public void TestTypedIndexer()
		{
			InvoiceLineCompleteCollection collection = new InvoiceLineCompleteCollection(Declaration);
			JobComInvoiceLine invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		public void TestUpdateAVSEventRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			Assert("Precondition: AVSEventRequired", !declaration.AVSEventRequired);
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);

			declaration.AVSEventRequired = false;
			invoiceLine.Delete();
			Assert("AVSEventRequired", declaration.AVSEventRequired);
		}

		public void TestNotAutoAllocatePackageToInvoiceLineWhenPackageIsForInvoiceHeader()
		{
			CustomsDataRegistry.Instance.AutoAllocatePackageToInvoiceLines.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "MB123";

			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "PK";

			var invoice = declaration.Invoices.AddNew();
			invoice.ToggleLinkageWithPackage(package, true);
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();

			AssertNotNull(invoiceLine.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(1, invoiceLine.PackagesForInvoiceLinesForBindingOnly.Count);
			Assert("Should not be marked for invoice line as it is marked for invoice header", !invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
		}

		[ExpectNoExceptions]
		public void TestShouldAutoAllocatePackageToInvoiceLineReturnFalseWhenInvoiceHeaderIsNull()
		{
			CustomsDataRegistry.Instance.AutoAllocatePackageToInvoiceLines.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "MB123";

			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "PK";

			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.ToggleLinkageWithPackage(package, true);

			var collection = new InvoiceLineCompleteCollectionToTest(declaration);

			Assert(!collection.ExposedShouldAutoAllocatePackageToInvoiceLine(invoiceLine));
		}

		public void TestSetDefaultValueForCFIARegistrationNumber()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.SafeFoodForCanadiansLicense, "54321");
			var declaration = Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			declaration.CA_OGDCFIA = true;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals(1, invoiceLine.CFIARegistrationNumbers.Count);
			AssertNotNull("Code [SafeFoodForCanadiansLicence] has been added automatically", invoiceLine.CFIARegistrationNumbers.OfType<CFIARegistrationNumber>().FirstOrDefault(x => x.CY_Code == RegistrationNumberHelper.SafeFoodForCanadiansLicence && x.CY_Data == "54321"));

			importer.CustomsCodes.RemoveAndDeleteAll();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertEquals(0, invoiceLine2.CFIARegistrationNumbers.Count);
			AssertNull("No CFIA number has been added", invoiceLine2.CFIARegistrationNumbers.OfType<CFIARegistrationNumber>().FirstOrDefault(x => x.CY_Code == RegistrationNumberHelper.SafeFoodForCanadiansLicence && x.CY_Data == "54321"));
		}

		public void TestUsingSubCollectionRebuildSuspender()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collectionType = declaration.InvoiceLines.GetType();
			var suspendRebuildSubCollectionMethod = collectionType.GetMethod("SuspendRebuildSubCollection",BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNotNull(suspendRebuildSubCollectionMethod);
			Assert("Rebuild sub collection should be allowed before using Sub collection rebuilder suspender.",
				declaration.InvoiceLines.AllowRebuildSubCollection);
			using (suspendRebuildSubCollectionMethod.Invoke(declaration.InvoiceLines, null) as IDisposable)
			{
				Assert("Rebuild sub collection should be disallowed during using Sub collection rebuilder suspender.",
					!declaration.InvoiceLines.AllowRebuildSubCollection);
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLineCompleteCollection(Declaration);
		}

		new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		public class InvoiceLineCompleteCollectionToTest : InvoiceLineCompleteCollection
		{
			public InvoiceLineCompleteCollectionToTest(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
			{
			}

			public bool ExposedShouldAutoAllocatePackageToInvoiceLine(BaseJobComInvoiceLine invoiceLine)
			{
				return base.ShouldAutoAllocatePackageToInvoiceLine(invoiceLine);
			}
		}
	}
}
