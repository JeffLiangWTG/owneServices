using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeader))]
	sealed class DocJobComInvoiceHeaderTest : DocBaseJobComInvoiceHeaderAbstractTest<JobComInvoiceHeader, DocJobComInvoiceHeader>
	{
		public void TestOSPartyWorksWithMiscSuppliers()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ZGuid miscOrgPK = declaration.CachedMiscOrgPK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			declaration.JE_OH_Supplier = miscOrgPK;
			declaration.MiscSupplierName = "GROTTEN FOGGARTY";

			OrgHeader normalSupplier = Factory.New<OrgHeader>();
			normalSupplier.OH_FullName = "TWID-WIDDLE FIDDLES";
			normalSupplier.OH_Code = "TWFIDDLE";
			normalSupplier.LocalCustomsSupplierCode = "123457Z";

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			AssertEquals("Precondition: invoiceHeader.SupplierName", "GROTTEN FOGGARTY", invoiceHeader.SupplierName);

			DocJobComInvoiceHeader invoiceWrapper = DocJobComInvoiceHeader.New(invoiceHeader, Factory);

			AssertEquals("invoiceWrapper.OSParty.Code", "MISC", invoiceWrapper.OSParty.Code);
			AssertEquals("invoiceWrapper.OSParty.ToString()", "GROTTEN FOGGARTY", invoiceWrapper.OSParty.ToString());
			AssertEquals("invoiceWrapper.OSParty.LocalCustomsSupplierCode", "", invoiceWrapper.OSParty.LocalCustomsSupplierCode);
			AssertEquals("invoiceWrapper.OSParty.PostalAddress", "GROTTEN FOGGARTY\nNEW ZEALAND", invoiceWrapper.OSParty.PostalAddress);

			invoiceHeader.JZ_OH_Supplier = normalSupplier.PK;

			AssertEquals("invoiceWrapper.OSParty.Code", "TWFIDDLE", invoiceWrapper.OSParty.Code);
			AssertEquals("invoiceWrapper.OSParty.ToString()", "TWID-WIDDLE FIDDLES", invoiceWrapper.OSParty.ToString());
			AssertEquals("invoiceWrapper.OSParty.LocalCustomsSupplierCode", "123457Z", invoiceWrapper.OSParty.LocalCustomsSupplierCode);
			AssertEquals("invoiceWrapper.OSParty.PostalAddress", "TWID-WIDDLE FIDDLES\nNEW ZEALAND", invoiceWrapper.OSParty.PostalAddress);
		}

		public override void TestConversionFactorIsWrapped()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, InvoiceHeaderWrapperInternal.ConversionFactor);
		}

		public override void TestIncoTermDescription()
		{
			InvoiceHeader.JZ_IncoTerm = "EXW";
			AssertEquals("Inco term should be 'Ex Works'", "Ex Works", InvoiceHeaderWrapper.IncoTermDescription);

			InvoiceHeader.JZ_IncoTerm = "CIF";
			AssertEquals("Inco term should be 'Cost, Insurance And Freight'", "Cost Insurance and Freight", InvoiceHeaderWrapper.IncoTermDescription);
		}

		public override void TestCIFCurrency()
		{
			Assert("Not used in NZ", true);
		}

		public override void TestFOBCurrency()
		{
			Assert("Not used in NZ", true);
		}

		public override void TestInvoiceCurr()
		{
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			InvoiceHeaderInternal = GetNewInvoice();
			AssertEquals("Invoice_Currency", InvoiceHeaderWrapperInternal.InvoiceCurr.ToString(), "NZD");
			AssertEquals("Invoice_Currency is of type DocCurrency", typeof(DocCurrency), InvoiceHeaderWrapperInternal.InvoiceCurr.GetType());
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.NewZealand; }
		}

		JobComInvoiceHeader InvoiceHeader;
		DocJobComInvoiceHeader InvoiceHeaderWrapper;

		protected override void SetUp()
		{
			base.SetUp();
			InvoiceHeader = InvoiceHeaderInternal;
			InvoiceHeaderWrapper = InvoiceHeaderWrapperInternal;
		}

		protected override DocJobComInvoiceHeader CreateInvoiceHeaderWrapper(JobComInvoiceHeader invoiceHeaderInternal)
		{
			return DocJobComInvoiceHeader.New(invoiceHeaderInternal, Factory);
		}

		#endregion
	}
}
