using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.MFI.DocWrappers.Testing
{
	[TestedType(typeof(DocMFIARInvoice))]
	public class DocMFIARInvoiceTestCase : DocumentWrapperTestCase
	{
		public void TestBarcode()
		{
			Assert("Should be empty", InvoiceWrapper.BarcodeForFontPlaceholder.IsEmpty);
			Assert("Should be empty", InvoiceWrapper.BarcodeForPlaceholder.IsEmpty);
			ForwardingShipment shipment = GetLinkedDomesticShipment();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "BLAH";
			Invoice.AH_OH = org.PK;
			Invoice.AH_OSTotalAmount = 203.03M;
			InvoiceWrapper = DocMFIARInvoice.New(Invoice, Factory);
			ZString expectedString = string.Format("{0} | {1} | {2}", org.OH_Code, Invoice.InvoiceNumber, InvoiceWrapper.OSTotalFormatted);
			AssertEquals("Should have proper barcode format", expectedString, InvoiceWrapper.BarcodeForPlaceholder);
			TextBarcode barcode = new TextBarcode(expectedString, true);
			AssertEquals("Should have proper barcode format", barcode.TextAs128sFontString, InvoiceWrapper.BarcodeForFontPlaceholder);
		}

		public void TestGoodsValueFormatted()
		{
			Assert("Should be empty", InvoiceWrapper.GoodsValueFormattedWithCurrencyCode.IsEmpty);
			ForwardingShipment shipment = GetLinkedDomesticShipment();
			InvoiceWrapper = DocMFIARInvoice.New(Invoice, Factory);
			AssertEquals("Should be formatted to include currency code", "100.01 AUD", InvoiceWrapper.GoodsValueFormattedWithCurrencyCode);
		}

		public void TestClientOverride()
		{
			AssertEquals("Client's DocInvoice Override", typeof(DocMFIARInvoice), DocARInvoice.New(Invoice, Factory).GetType());
		}

		public void TestInvoiceLogo()
		{
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(3, 3));
			Image invoiceLetterhead = new Bitmap(1, 1);
			MFIDataRegistry.Instance.InvoiceLetterhead = invoiceLetterhead;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, "EML");
			InvoiceWrapper.SetTemplateConstants(constants);
			Factory.Save();
			AssertEquals("Default logo", SystemDataRegistry.Instance.CompanyLogo.Value.Size, InvoiceWrapper.InvoiceLogo.Size);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			GlbCompany.CurrentCompany.GC_Code = "AKL";
			constants.Clear();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, "PRN");
			InvoiceWrapper.SetTemplateConstants(constants);
			Factory.Save();
			AssertEquals("MFI NZ Specific Invoice logo", invoiceLetterhead.Size, InvoiceWrapper.InvoiceLogo.Size);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			constants.Clear();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, "FAX");
			InvoiceWrapper.SetTemplateConstants(constants);
			Factory.Save();
			AssertEquals("Default logo", SystemDataRegistry.Instance.CompanyLogo.Value.Size, InvoiceWrapper.InvoiceLogo.Size);
		}

		#region Implementation
		DocMFIARInvoice InvoiceWrapper;
		InvoicingBase Invoice;
		ForwardingShipment GetLinkedDomesticShipment()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "USMIA";
			shipment.JS_GoodsValue = 100.01M;
			shipment.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.Australia;
			shipment.JS_InsuranceValue = 222.22M;
			shipment.JS_RX_NKInsuranceCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;
			Invoice.AH_JH = job.PK;
			return shipment;
		}

		protected override void SetUp()
		{
			Invoice = Factory.New<ARInvoice>();
			InvoiceWrapper = DocMFIARInvoice.New(Invoice, Factory);
			base.SetUp();
			Factory.Save();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { InvoiceWrapper };
		}
		#endregion
	}
}
