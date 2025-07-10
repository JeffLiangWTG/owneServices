using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceHeader))]
sealed class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
{
	public void TestJobComInvoiceLines()
	{
		var invoiceHeader = this.invoiceHeader;
		CombineAssertions(() =>
		{
			AssertType<JobComInvoiceLineViewCollection>("Type", invoiceHeader.JobComInvoiceLines);
			AssertSame("InvoiceLine same", invoiceHeader.JobComInvoiceLines, invoiceHeader.InvoiceLines);
		});
	}

	public void TestLookups_Import()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobComInvoiceHeaderLookups>(invoiceHeader.Lookups);
	}

	public void TestLookups_Export()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobComInvoiceHeaderLookups>(invoiceHeader.Lookups);
	}

	public void TestLookups_MiscellaneousCustoms()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobComInvoiceHeaderLookups>(invoiceHeader.Lookups);
	}

	public void TestValidation_Import()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobComInvoiceHeaderValidation>(invoiceHeader.Validation);
	}

	public void TestValidation_Export()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobComInvoiceHeaderValidation>(invoiceHeader.Validation);
	}

	public void TestValidation_MiscellaneousCustoms()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobComInvoiceHeaderValidation>(invoiceHeader.Validation);
	}

	public void TestUpliftCalculation()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_ExportDate = new ZDateTime(2004, 10, 30);
		var invoice = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

		invoice.JZ_InvoiceAmount = 1000m;
		invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
		invoice.ZG_ValuationMarkup = 10;
		AssertEquals("InvoiceLineTotal should get the uplift ", 1100m, invoice.InvoiceLineTotal);

		invoice.JZ_InvoiceAmount = 2000m;
		AssertEquals("InvoiceLineTotal should get the uplift ", 2200m, invoice.InvoiceLineTotal);

		invoice.ZG_ValuationMarkup = 0;
		AssertEquals("InvoiceLineTotal should not get the uplift ", 2000m, invoice.InvoiceLineTotal);
	}

	public void TestAdditionalInfos()
	{
		AssertEquals(typeof(AdditionalInfoCollection), ((JobComInvoiceHeader)invoiceHeader).AdditionalInfos.GetType());
	}

	public void TestSupportingDocumentCollectionType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		AssertType<SupportingDocumentCollection>(invoiceHeader.SupportingDocuments);
	}

	public void TestPreviousDocumentCollectionType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		AssertType<PreviousDocumentCollection>(invoiceHeader.PreviousDocuments);
	}

	public void TestJZ_UCR()
	{
		AssertEquals("Commercial Ref.", Factory.New<JobComInvoiceHeader>().JZ_UCRInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
	}

	public void TestJZ_IncoTerm_Caption()
	{
		AssertEquals("[UCC 4/1] Incoterm", Factory.New<JobComInvoiceHeader>().JZ_IncoTermInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
	}

	public void TestZG_AgreedPlaceCode_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().ZG_AgreedPlaceCodeInfo, "Incoterm Place", "[UCC 4/1] Incoterm Place");
	}

	public void TestJZ_InvoiceAmount_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().JZ_InvoiceAmountInfo, "Invoice Amount", "[UCC 4/10] Invoice Amount", "IMP");
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().JZ_InvoiceAmountInfo, "Invoice Amount", "[UCC 4/10] Invoice Amount", "EXP");
	}

	public void TestJZ_InvoiceCurrExRate_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().JZ_InvoiceCurrExRateInfo, "Exchange Rate", "[UCC 4/15] Exchange Rate");
	}

	public void TestJZ_ValuationCode_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().JZ_ValuationCodeInfo, "[24] Tran. Nature", "[24] Transaction Nature");
	}

	public override void TestZGFieldsCaption()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		CombineAssertions("Payment Properties Caption", () =>
		{
			var transportMOPPropertyData = DataBoundResourceStrings.GetDataForProperty(invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo);
			var agreedPlacePropertyData = DataBoundResourceStrings.GetDataForProperty(invoiceHeader.ZG_AgreedPlaceCodeInfo);

			AssertEquals($"{invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo.Name} Caption", "Transport Charges Method of Payment", transportMOPPropertyData.Caption);
			AssertEquals($"{invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo.Name} Short Caption", "MoP", transportMOPPropertyData.ShortCaption);

			AssertEquals($"{invoiceHeader.ZG_AgreedPlaceCodeInfo.Name} Caption", "Incoterm Place", agreedPlacePropertyData.Caption);
			AssertEquals($"{invoiceHeader.ZG_AgreedPlaceCodeInfo.Name} FullDescription", "[UCC 4/1] Incoterm Place", agreedPlacePropertyData.FullDescription);
		});
	}

	public void TestJZ_IncoTermPlace_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().JZ_IncoTermPlaceInfo, "Agreed Place", string.Empty, "IMP");
	}

	public void TestGetCusSupportingInfoTypes() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var supportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)invoiceHeader;
		
		AssertEquals("AdditionalInfo", typeof(AdditionalInfo), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		AssertEquals("SupportingDocument", typeof(SupportingDocument), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		AssertEquals("PreviousDocument", typeof(PreviousDocument), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
	});

	public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Netherlands;

	protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

	protected override bool ShouldBOGetSavedWithDetachedInvoiceHeader(BaseJobComInvoiceHeader invoice, Type invoiceLineAddInfoChildType, BusinessObject bO) => base.ShouldBOGetSavedWithDetachedInvoiceHeader(invoice, invoiceLineAddInfoChildType, bO) || bO is JobDocAddress;

	void AssertCaptionAndFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedFullDescription, string messageType = "")
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(propertyInfo, null, [messageType == "IMP" ? JobDeclaration.CaptionKeyImportUCC6 : messageType == "EXP" ? JobDeclaration.CaptionKeyExportUCC6 : null]);

		CombineAssertions(() =>
		{
			AssertEquals($"{propertyInfo.Name} {messageType} Caption", expectedCaption, resourceStringData.Caption);
			AssertEquals($"{propertyInfo.Name} {messageType} FullDescription", expectedFullDescription, resourceStringData.FullDescription);
		});
	}
}
