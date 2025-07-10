using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.Customs.GB.Chief.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	sealed class GbHeaderTest : TestCaseWithFactory
	{
		public void TestGovernmentContractorTurn()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Test1";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678");

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var gbHeader = new GbChiefExportHeader(entryHeader);
			AssertEquals("", gbHeader.GovernmentContractorTurn);

			declaration.GovernmentContractorDocAddress.OrganisationPK = orgHeader.PK;
			AssertEquals("12345678", gbHeader.GovernmentContractorTurn);
		}

		public void TestRegisteredConsigneeTurn()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cusInstruction.PK;
			var gbHeader = new GbChiefExportHeader(entryHeader);
			AssertEquals("", gbHeader.RegisteredConsigneeTurn);

			var fiscalReference = cusInstruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Reference = "GB1234567890123";
			AssertEquals("1234567890123", gbHeader.RegisteredConsigneeTurn);
		}

		public void TestGBHeaderCheckBox18Information()
		{
			//GbHeader gbHeader;
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var gbHeader = new GbChiefExportHeader(entryHeader);
			declaration.ZG_Box18TransportID = "TR123";
			declaration.ZG_Box18TransportNationality = "KR";

			AssertEquals("KR TR123", gbHeader.TransportIdentityOnDepartureBox18);
			AssertEquals("KR", gbHeader.TransportNationalityOnDepartureBox18);
			AssertEquals("TR123", gbHeader.Box18TransportID);
		}

		public void TestGetConsigneeFromLine()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = consignee.MainAddress;

			var address = consignee.Addresses.AddNew();
			address.FillWithValidTestData();

			AssertNotEquals("Precondition", address.PK, mainAddress.PK);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			declaration.Invoices.DeleteAll();

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			invoice.JZ_OH_Buyer = Factory.NewWithValidTestData<OrgHeader>().PK;

			invoice.InvoiceLines.RemoveAndDeleteAll();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();

			Factory.Save();

			var declarationInOtherFacotry = NewFactory().Load<JobDeclaration>(declaration.PK);
			declarationInOtherFacotry.ImporterDocumentaryAddress.E2_OA_Address = address.PK;

			var docAddress = declarationInOtherFacotry.ImporterDocumentaryAddress;

			var entryHeader = declarationInOtherFacotry.CustomsEntryHeaders.First();
			var gbHeader = new GbChiefExportHeader(entryHeader);
			var gbLine = (GbLine)gbHeader.Lines.First();

			AssertEquals("The consignee pk of declaration is not empty.", declarationInOtherFacotry.JE_OH_Importer, consignee.PK);
			AssertEquals("The address pk is not empty.", address.PK, docAddress.E2_OA_Address);

			AssertNotNull("The address is not null.", docAddress.Address);
			AssertEquals("The address is not empty.", address.Address1, docAddress.E2_Address1);

			AssertExceptionThrown<NotSupportedException>(() =>
			{
				var cne = gbLine.Consignee;
			});

			var newFactory = NewFactory();

			var addressInOtherFactory = newFactory.Load<OrgAddress>(address.PK);
			addressInOtherFactory.Delete();

			newFactory.Save();
			newFactory.ForcePublishForDataRefresh(addressInOtherFactory);

			AssertEquals("The consignee pk of declaration is still not empty.", declarationInOtherFacotry.JE_OH_Importer, consignee.PK);
			AssertEquals("The address pk is still not empty.", address.PK, docAddress.E2_OA_Address);

			AssertNull("The address is null as it is deleted.", docAddress.Address);
			AssertEquals("The address is empty as the consignee is deleted.", ZString.Empty, docAddress.E2_Address1);

			AssertEquals("Should get the consignee from the invoice's buyer.", invoice.Buyer.MainAddress.PK, gbLine.Consignee.PK);
		}

		public void TestGetShipperFromLine()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = supplier.MainAddress;

			var address = supplier.Addresses.AddNew();
			address.FillWithValidTestData();

			AssertNotEquals("Precondition", address.PK, mainAddress.PK);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.SupplierDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			declaration.Invoices.DeleteAll();

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			invoice.JZ_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;

			invoice.InvoiceLines.RemoveAndDeleteAll();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();

			Factory.Save();

			var declarationInOtherFacotry = NewFactory().Load<JobDeclaration>(declaration.PK);
			declarationInOtherFacotry.SupplierDocumentaryAddress.E2_OA_Address = address.PK;

			var docAddress = declarationInOtherFacotry.SupplierDocumentaryAddress;

			var entryHeader = declarationInOtherFacotry.CustomsEntryHeaders.First();
			var gbHeader = new GbChiefExportHeader(entryHeader);
			var gbLine = (GbLine)gbHeader.Lines.First();

			AssertEquals("The supplier pk of declaration is not empty.", declarationInOtherFacotry.JE_OH_Supplier, supplier.PK);
			AssertEquals("The address pk is not empty.", address.PK, docAddress.E2_OA_Address);

			AssertNotNull("The address is not null.", docAddress.Address);
			AssertEquals("The address is not empty.", address.Address1, docAddress.E2_Address1);

			AssertExceptionThrown<NotSupportedException>(() =>
			{
				var shipper = gbLine.Shipper;
			});

			var newFactory = NewFactory();

			var addressInOtherFactory = newFactory.Load<OrgAddress>(address.PK);
			addressInOtherFactory.Delete();

			newFactory.Save();
			newFactory.ForcePublishForDataRefresh(addressInOtherFactory);

			AssertEquals("The supplier pk of declaration is still not empty.", declarationInOtherFacotry.JE_OH_Supplier, supplier.PK);
			AssertEquals("The address pk is still not empty.", address.PK, docAddress.E2_OA_Address);

			AssertNull("The address is null as it is deleted.", docAddress.Address);
			AssertEquals("The address is empty as the supplier is deleted.", ZString.Empty, docAddress.E2_Address1);

			AssertEquals("Should get the shipper from the invoice's supplier.", invoice.Supplier.MainAddress.PK, gbLine.Shipper.PK);
		}

		public void TestPremisesFromDeclaration()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			warehouse.OH_Code = "WHTEST";
			warehouse.OH_FullName = "MyTest";
			var add1 = warehouse.MainAddress;
			add1.FillWithValidTestData();

			var declaration = Factory.New<JobDeclaration>();

			declaration.WarehouseDocAddress.E2_OA_Address = add1.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var gbHeader = new GbChiefExportHeader(entryHeader);

			var org = gbHeader.Premises;
			AssertNotNull(org);
			AssertEquals("MyTest", org?.Name ?? ZString.Empty);
		}
	}
}
