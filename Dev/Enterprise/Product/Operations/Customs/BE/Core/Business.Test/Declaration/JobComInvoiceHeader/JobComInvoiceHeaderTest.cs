using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceHeader))]
sealed class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
{
	public void TestJZ_IncoTermPlace()
	{
		AssertEquals("Agreed Place", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().JZ_IncoTermPlaceInfo).Caption);
	}

	public void TestPreviousDocuments_Load_NoException()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var invoice = declaration.Invoices.AddNew();
		var previousDocument = invoice.PreviousDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		Factory.Save();
		var newFactory = new BusinessObjectFactory();
		var type = CusSupportingInfo.TypeDecider.GetTypeForLoad(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, JobComInvoiceHeaderSchema.Constants.Prefix, invoice.PK, newFactory);
		newFactory.Load(type, previousDocument.PK);
		AssertNoExceptionThrown(() =>
		{
			newFactory.Load<JobComInvoiceLine>(invoiceLine.PK).Validation.ValidateJI_LineNo();
		});
	}

	public void TestGetCusSupportingInfoTypes_PRE()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var invoice = declaration.Invoices.AddNew();
		AssertEquals(typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)invoice).GetCusSupportingInfoTypes()[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
	}

	public void TestSupportingDocuments_Load_NoException()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var invoice = declaration.Invoices.AddNew();
		var supportingDocument = invoice.SupportingDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		Factory.Save();
		var newFactory = new BusinessObjectFactory();
		var type = CusSupportingInfo.TypeDecider.GetTypeForLoad(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, JobComInvoiceHeaderSchema.Constants.Prefix, invoice.PK, newFactory);
		newFactory.Load(type, supportingDocument.PK);
		AssertNoExceptionThrown(() =>
		{
			newFactory.Load<JobComInvoiceLine>(invoiceLine.PK).Validation.ValidateJI_LineNo();
		});
	}

	public void TestGetCusSupportingInfoTypes_SUP()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var invoice = declaration.Invoices.AddNew();
		AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)invoice).GetCusSupportingInfoTypes()[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
	}

	public void TestJZ_UCR()
	{
		AssertEquals("Commercial Ref.", Factory.New<JobComInvoiceHeader>().JZ_UCRInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
	}

	public void TestPreviousDocuments()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		var invoice = declaration.Invoices.AddNew();
		AssertType<PreviousDocumentCollection>(invoice.PreviousDocuments);
	}

	public void TestJobDeclarationMessageTypeChanged()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();

		CombineAssertions(() =>
		{
			invoiceHeader.JZ_UCR = "ucr";
			invoiceHeader.ZG_TransportChargesMethodOfPayment = "C";
			invoiceHeader.JobDeclarationMessageTypeChanged(JobMessageTypeList.Codes.Import);
			AssertEquals("InvoiceHeader JZ_UCR should be empty", ZString.Empty, invoiceHeader.JZ_UCR);
			AssertEquals("InvoiceHeader ZG_TransportChargesMethodOfPayment should be empty", ZString.Empty, invoiceHeader.ZG_TransportChargesMethodOfPayment);

			invoiceHeader.JZ_UCR = "ucr";
			invoiceHeader.ZG_TransportChargesMethodOfPayment = "C";
			invoiceHeader.JobDeclarationMessageTypeChanged(JobMessageTypeList.Codes.Export);
			AssertEquals("InvoiceHeader JZ_UCR should not be cleared", "ucr", invoiceHeader.JZ_UCR);
			AssertEquals("InvoiceHeader ZG_TransportChargesMethodOfPayment should not be cleared", "C", invoiceHeader.ZG_TransportChargesMethodOfPayment);
		});
	}

	public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Belgium;

	protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList() => new BECustomsChargeTypeList();

	protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

	protected override Type ExpectedTypeOfCharges => typeof(InvoiceChargeCollection<InvoiceCharge>);

	public void TestAdditionalInfos()
	{
		var header = GetNewInvoiceHeader();
		AssertEquals(typeof(AdditionalInfoCollection), header.AdditionalInfos.GetType());
	}

	public void TestJZ_IncoTerm_Caption()
	{
		AssertEquals("[UCC 4/1] Incoterm", Factory.New<JobComInvoiceHeader>().JZ_IncoTermInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
	}

	public void TestZG_AgreedPlaceCode_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().ZG_AgreedPlaceCodeInfo, "Incoterm Place", "[UCC 4/1] Incoterm Place");
	}

	public void TestJZ_ValuationCode_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().JZ_ValuationCodeInfo, "[24] Tran. Nature", "[24] Transaction Nature");
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().JZ_ValuationCodeInfo, "[24] Tran. Nature", "[24] Transaction Nature", "IMP");
	}

	public void TestJZ_InvoiceAmount_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().JZ_InvoiceAmountInfo, "Invoice Amount", "[UCC 4/10] Invoice Amount", "EXP", expectedShortCaption: "Invoice Amt");
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().JZ_InvoiceAmountInfo, "Invoice Amount", "[UCC 4/10] Invoice Amount", "IMP", expectedShortCaption: "Invoice Amt");
	}

	public void TestJZ_InvoiceCurrExRate_Caption()
	{
		AssertCaptionAndFullDescription(Factory.New<JobComInvoiceHeader>().JZ_InvoiceCurrExRateInfo, "Exchange Rate", "[UCC 4/15] Exchange Rate", expectedShortCaption: "Exch. Rate");
	}

	public override void TestZGFieldsCaption()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		CombineAssertions("Payment Properties Caption", () =>
		{
			var transportMOPPropertyData = DataBoundResourceStrings.GetDataForProperty(invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo);

			AssertEquals($"{invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo.Name} Caption", "Transport Charges Method of Payment", transportMOPPropertyData.Caption);
			AssertEquals($"{invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo.Name} Short Caption", "MoP", transportMOPPropertyData.ShortCaption);
		});
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

	JobComInvoiceHeader GetNewInvoiceHeader()
	{
		var declaration = (JobDeclaration)GetNewDeclaration();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		return declaration.Invoices.AddNew();
	}
}
