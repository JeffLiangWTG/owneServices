using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceLineViewCollection))]
	sealed class InvoiceLineCollectionBOTest : InvoiceLineCollectionBOTest<InvoiceLineViewCollection>
	{
		new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

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

			JobComInvoiceHeader invoice2 = jobDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceDisplaySequence = 2;
			var line3 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line3.CA_PageNumber = 3;
			var line4 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line4.CA_PageNumber = 3;

			JobComInvoiceHeader invoice3 = jobDeclaration.Invoices.AddNew();
			invoice3.JZ_InvoiceDisplaySequence = 3;
			var line5 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			line5.CA_PageNumber = 4;
			var line6 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			line6.CA_PageNumber = 5;

			var collection = new InvoiceLineViewCollection(jobDeclaration);
			collection.RemoveAndDelete(line1);
			AssertEquals(1, line2.CA_PageNumber);
			AssertEquals(2, line3.CA_PageNumber);
			AssertEquals(2, line4.CA_PageNumber);
			AssertEquals(3, line5.CA_PageNumber);
			AssertEquals(4, line6.CA_PageNumber);

			collection.RemoveAndDelete(line3);
			AssertEquals(1, line2.CA_PageNumber);
			AssertEquals(2, line4.CA_PageNumber);
			AssertEquals(3, line5.CA_PageNumber);
			AssertEquals(4, line6.CA_PageNumber);

			collection.RemoveAndDelete(line6);
			AssertEquals(1, line2.CA_PageNumber);
			AssertEquals(2, line4.CA_PageNumber);
			AssertEquals(3, line5.CA_PageNumber);
		}

		public void TestTypedIndexer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = new InvoiceLineViewCollection(declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		public void TestSetDefaultsForNewChild()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			var collection = new InvoiceLineViewCollection(JobDeclaration);
			var invoiceLine = collection.AddNew();
			AssertEquals("CA_PageNumber", 1, invoiceLine.CA_PageNumber);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.DeliveredDutyPaid, invoiceLine.CA_CalculationMethod);

			invoiceLine.CA_PageNumber = 3;
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.JI_StateOrRegionOfOrigin = CanadianProvinceList.Codes.Alberta;
			invoiceLine.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_USStateOfExport = USStatesList.Codes.Alabama;
			invoiceLine = collection.AddNew();
			AssertEquals("CA_PageNumber", 1, invoiceLine.CA_PageNumber);
			AssertEquals("CA_TreatmentCode", TariffTreatmentCodes.Codes.UnitedStates, invoiceLine.CA_TreatmentCode);
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.Canada, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", CanadianProvinceList.Codes.Alberta, invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("CA_RN_NKExport", Core.Constants.CountryCodes.UnitedStates, invoiceLine.CA_RN_NKExport);
			AssertEquals("CA_USStateOfExport", USStatesList.Codes.Alabama, invoiceLine.CA_USStateOfExport);

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine = collection.AddNew();
			AssertEquals("CA_TreatmentCode", "02", invoiceLine.CA_TreatmentCode);
			AssertEquals("JI_CountryOfOrigin", string.Empty, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", string.Empty, invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("CA_RN_NKExport", string.Empty, invoiceLine.CA_RN_NKExport);
			AssertEquals("CA_USStateOfExport", string.Empty, invoiceLine.CA_USStateOfExport);
		}

		public void TestCopyLastLineDetailsToNewLinesIfEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoiceLine1 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.CA_CFIAInd = "Y";
			var cfiaPGA = invoiceLine1.CFIAPGAHeader;
			cfiaPGA.CA_AllProgramInd = "Y";
			cfiaPGA.CA_AIRSEndUse = "ENS";

			Factory.Save();

			declaration.FilteredInvoiceLines.CopyLastLineDetailsToNewLines = true;

			var invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			AssertNotNull("CFIAPGAHeader is copied", invoiceLine2.CFIAPGAHeader);
			AssertEquals("Y", invoiceLine2.CFIAPGAHeader.CA_AllProgramInd);
			AssertEquals("ENS", invoiceLine2.CFIAPGAHeader.CA_AIRSEndUse);
		}

		public void TestResetAVSStatusForNewChild()
		{
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA, "0101210000");
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			var invoiceLine1 = (JobComInvoiceLine)JobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0101210000";
			invoiceLine1.CA_OGDStatus = AVSStatusList.Codes.WillBeApproved;
			Factory.Save();

			JobDeclaration.FilteredInvoiceLines.CopyLastLineDetailsToNewLines = true;
			var invoiceLine2 = JobDeclaration.FilteredInvoiceLines.AddNew();
			AssertEquals("JI_Tariff", "0101210000", invoiceLine2.JI_Tariff);
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine2.CA_OGDStatus);
			Assert("Declaration.AVSEventRequired", JobDeclaration.AVSEventRequired);
		}

		public void TestSetIsForInvoiceLineForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "MB123";

			declaration.Packages.RemoveAndDeleteAll();

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = Constants.PkgUnit.Container;

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 5;
			package2.CW_PackType = Constants.PkgUnit.Box;
			package2.CW_CW_Parent = package1.PK;

			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 1;
			package3.CW_PackType = Constants.PkgUnit.Box;
			package3.CW_CW_Parent = package1.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.Codes.Import;

			var invoiceLine1 = declaration.FilteredInvoiceLines.AddNew();
			AssertNotNull(invoiceLine1.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(2, invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Count);
			Assert("Is not marked for invoice line", !invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			Assert("Is not marked for invoice line", !invoiceLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked);

			declaration.Packages.Delete(package3);
			var invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			AssertNotNull(invoiceLine2.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(1, invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Count);
			Assert("Should be marked for invoice line as it is single lowest package", invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			Assert("Parent package should be added to the invoice lines", invoiceLine2.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(p => p.Package.PK == package1.PK));

			declaration.MakeNonPersistent();
			var invoiceLine3 = declaration.FilteredInvoiceLines.AddNew();
			AssertNotNull(invoiceLine3.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(1, invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Count);
			Assert("Should not be marked for invoice line as it has not a persisent declaration", !invoiceLine3.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "MB123";
			package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = Constants.PkgUnit.Container;
			var invoiceLine4 = declaration.FilteredInvoiceLines.AddNew();
			Assert(invoiceLine4.PackagesForInvoiceLinesForBindingOnly == null || invoiceLine4.PackagesForInvoiceLinesForBindingOnly.Count == 0);
		}

		public void TestNotSetIsForInvoiceLineForNewChildWhenPackageIsForInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "MB123";

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = Constants.PkgUnit.Container;

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 5;
			package2.CW_PackType = Constants.PkgUnit.Box;
			package2.CW_CW_Parent = package1.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.ToggleLinkageWithPackage(package2, true);
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();

			AssertNotNull(invoiceLine.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(1, invoiceLine.PackagesForInvoiceLinesForBindingOnly.Count);
			Assert("Should not be marked for invoice line as it is marked for invoice header", !invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
		}

		public void TestNoExceptionThrownWhenRecalculatePageNumberInDeletedInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceDisplaySequence = 1;
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.CA_PageNumber = 1;
			line1.JI_LineNo = 1;
			line1.JI_LinePrice = 100m;
			var line2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line2.CA_PageNumber = 2;
			line2.JI_LineNo = 2;
			line2.JI_LinePrice = 200m;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceDisplaySequence = 2;
			var line3 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line3.CA_PageNumber = 3;
			line3.JI_LineNo = 3;
			line3.JI_LinePrice = 300m;
			var line4 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line4.CA_PageNumber = 4;
			line4.JI_LineNo = 4;
			line4.JI_LinePrice = 400m;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declarationLoaded = newFactory.Load<JobDeclaration>(declaration.PK);
			var invoiceLoaded = newFactory.Load<JobComInvoiceHeader>(invoice.PK);
			var invoice2Loaded = newFactory.Load<JobComInvoiceHeader>(invoice2.PK);
			var collectionLoaded = new InvoiceLineViewCollection(declarationLoaded);

			var collection = new InvoiceLineViewCollection(declaration);
			collection.RemoveAndDelete(line1);
			Factory.Save();

			AssertNoExceptionThrown(() => PageNumberCalculator.RecalculatePageNumber(declarationLoaded, invoiceLoaded.JZ_InvoiceDisplaySequence));
			AssertNoExceptionThrown(() => PageNumberCalculator.RecalculatePageNumber(declarationLoaded, invoice2Loaded.JZ_InvoiceDisplaySequence));
		}

		public void TestCalculateRemainingPackages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_ClusterKey = 1;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "MB123";

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 100;
			package1.CW_PackType = Constants.PkgUnit.Package;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.FilteredInvoiceLines.AddNew();
			AssertNotNull(invoiceLine1.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(1, invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(100, invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].PackQty);

			invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 20;
			var invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(1, invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(80, invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].PackQty);

			invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 28;
			var invoiceLine3 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(1, invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(52, invoiceLine3.PackagesForInvoiceLinesForBindingOnly[0].PackQty);
		}

		protected override InvoiceLineViewCollection GetCollectionToTest()
		{
			return new InvoiceLineViewCollection(JobDeclaration);
		}
	}
}
