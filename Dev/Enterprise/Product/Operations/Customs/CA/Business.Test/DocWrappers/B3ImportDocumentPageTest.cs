using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B3ImportDocumentPage))]
	sealed class B3ImportDocumentPageTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new B3ImportDocumentPage(null);
		}

		public void TestFormattedVendorOnB3ImportDocumentPage()
		{
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
			var helper = new DeclarationTestHelper(factory, true);
			var canada = factory.Load<RefCountry>(Constants.CountryGuids.Canada);

			var declaration = factory.New<JobDeclaration>();
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3.Description, "Note to be printed on B3");
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			declaration.JE_OH_Supplier = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.WarehouseDocAddress.OrganisationPK = helper.CreateOrganisation("WAREHOUSE NAME", "CATOR").PK;
			declaration.WarehouseDocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "12345", canada);
			var importer = helper.CreateOrganisation("IMP", "IMPORTER NAME", "CATOR", "IMPORTER ADDRESS", "IMPORTER CITY", "123 4567");
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = ZString.Empty;
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment = true;
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "3021", canada);
			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "1234", canada);
			declaration.Importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "0987654321", canada);
			declaration.JE_CustomsOffice = "351";
			declaration.CA_UnladingOffice = "423";
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2010, 4, 30, 12, 41, 25);
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "212112345678987654321";
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = ZString.Empty;
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "123456789321566549877";
			declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
			declaration.JE_OH_Forwarder = helper.ExportForwarder.PK;
			declaration.ShippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "2121", canada);
			declaration.Forwarder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1212", canada);

			//Invoice 1
			var supplier1 = helper.CreateOrganisation("SUP", "SUPPLIER NAME", "AUMEL", "SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567").PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 100m);
			invoice.JZ_OH_Supplier = supplier1;
			invoice.ExporterDocumentaryAddress.OrganisationPK = helper.CreateOrganisation("SUP", "EXPORTER NAME", "USCHI", "EXPORTER ADDRESS", "CHICARGO", "IL", "12321", "123 4567").PK;
			invoice.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			invoice.CA_USStateOfExport = USStatesList.Codes.NewYork;
			invoice.CA_TradeZone = "168B";
			invoice.CA_USPortOfExit = "2813";
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			invoice.CA_TimeLimit = 20;
			helper.USD.SetCustomsRate(ZDateTime.Today, ZDateTime.Today, 1.111111);
			invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			invoice.JZ_Weight = 250;
			invoice.JZ_WeightUQ = "LB";

			//Invoice Lines
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 1, 1, 1000, 1000, 666, 333, Constants.Weight.Kilograms, 222);
			JobComInvoiceLineTestHelper.AddTariffRecord(invoiceLine);
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoice.JobComInvoiceLines.AddNew(), 2, 1, 700, 500, 250, 100, Constants.Weight.Tonnes, 180);

			//Invoice 2
			var supplier2 = helper.CreateOrganisation("SUP", "SUPPLIER NAME2", "USNYC", "SUPPLIER ADDRESS2", "NEW YOUK", "NY", "12345", "123 4567").PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice2.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 0.01m);
			invoice2.JZ_OH_Supplier = supplier2;
			invoice2.CA_RN_NKExport = Constants.CountryCodes.NewZealand;
			invoice2.CA_USPortOfExit = "3216";
			invoice2.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoice2.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			invoice2.CA_TimeLimit = 2;
			invoice2.JZ_RX_NKInvoice_Currency = helper.CAD.RX_Code;
			invoice2.CA_TradeZone = "";
			invoice2.JZ_Weight = 50;
			invoice2.JZ_WeightUQ = "LB";

			//Invoice Lines
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine2, 3, 2, 1000, 1000, 666, 333, Constants.Weight.Kilograms);
			invoiceLine2.JI_Tariff = "0301104567";
			invoiceLine2.JI_CustomsQuantity = 1000;
			invoiceLine2.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			JobComInvoiceLineTestHelper.AddTariffRecord(invoiceLine2);

			//TODO: Make this line negative.
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 4, 2, 600, 100, 90, 80, Constants.Weight.Kilograms, 90);
			invoiceLine.JI_Tariff = "0301104567";
			invoiceLine.JI_CustomsQuantity = 100;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Litre;
			declaration.DocAddresses.RemoveAndDeleteAll();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);

			IB3Header b3Header = new B3ImportMessageWrapper(entryHeader);
			var b3PositiveSubHeaders = new List<IB3SubHeader>(b3Header.PositiveB3SubHeaders);
			var subHeaderForTest = new B3SubHeaderForTest(b3PositiveSubHeaders[0]);
			var docPage = new B3ImportDocumentPage(subHeaderForTest);

			AssertEquals("CompanyTruncatedName Address State City Postcode", @"SUPPLIER NAME
VIC 3000", docPage.VendorFormatted);
		}
	}

	class B3SubHeaderForTest : IB3SubHeader
	{
		readonly IB3SubHeader b3SubHeader;

		internal B3SubHeaderForTest(IB3SubHeader header)
		{
			b3SubHeader = header;
		}

		public IDocAddress Vendor
		{
			get
			{
				var docWrapper = new DocAddressWrapper(b3SubHeader.Vendor.E2_CompanyName);
				docWrapper.E2_State = b3SubHeader.Vendor.E2_State;
				docWrapper.E2_Postcode = b3SubHeader.Vendor.E2_Postcode;
				return docWrapper;
			}
		}

		public ZInt B3SubHeaderNumber => b3SubHeader.B3SubHeaderNumber;

		public ZDecimal FreightCharges => b3SubHeader.FreightCharges;

		public IDocAddress Exporter => b3SubHeader.Exporter;

		public ZDateTime DateOfDirectShipment => b3SubHeader.DateOfDirectShipment;

		public ZString CountryOfOrigin => b3SubHeader.CountryOfOrigin;

		public ZString PlaceOfExport => b3SubHeader.PlaceOfExport;

		public ZString USPortOfExit => b3SubHeader.USPortOfExit;

		public ZString TariffTreatmentCode => b3SubHeader.TariffTreatmentCode;

		public ZString TimeLimitUnit => b3SubHeader.TimeLimitUnit;

		public ZInt B3TimeLimits => b3SubHeader.B3TimeLimits;

		public ZString CurrencyCode => b3SubHeader.CurrencyCode;

		public ZString TradeZone => b3SubHeader.TradeZone;

		public ZString InvoiceNumber => b3SubHeader.InvoiceNumber;

		public ZDecimal ExchangeRate => b3SubHeader.ExchangeRate;

		public VendorStateAndZipStruct VendorStateAndZip => b3SubHeader.VendorStateAndZip;
		public IB3Header B3Header => b3SubHeader.B3Header;
	}
}
