using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using InvoiceLineDependentCollection = Enterprise.Customs.Business.InvoiceLineDependentCollection;

namespace Enterprise.Customs.BE.Business.Declaration;

public class JobComInvoiceHeader : AutoJobComInvoiceHeader, Integration.Customs.BE.IJobComInvoiceHeader
{
	public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

	public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	public new InvoiceChargeCollection<InvoiceCharge> Charges => (InvoiceChargeCollection<InvoiceCharge>)base.Charges;

	public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	public new AddInfoJobComInvoiceHeader AddInfo => (AddInfoJobComInvoiceHeader)base.AddInfo;

	public new AddInfoJobComInvoiceHeaderLookups AddInfoLookups => (AddInfoJobComInvoiceHeaderLookups)base.AddInfoLookups;

	public new AddInfoJobComInvoiceHeaderValidation AddInfoValidation => (AddInfoJobComInvoiceHeaderValidation)base.AddInfoValidation;

	public override ZGuid JZ_OA_ConsigneeAddress
	{
		get => base.JZ_OA_ConsigneeAddress;
		set
		{
			bool hasChanged = JZ_OA_ConsigneeAddress != value;
			base.JZ_OA_ConsigneeAddress = value;
			if (hasChanged)
			{
				JobComInvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("F309EE03-CF37-4D7D-BE47-A24B7FDA5E11", Caption = "Agreed Place")]
	public override ZString JZ_IncoTermPlace { get => base.JZ_IncoTermPlace; set => base.JZ_IncoTermPlace = value; }

	[ResourceStringData("D3D1A271-20CD-43E3-AEBA-5D736E3EA4AD", Caption = "Commercial Ref.")]
	public override ZString JZ_UCR { get => base.JZ_UCR; set => base.JZ_UCR = value; }

	public void JobDeclarationMessageTypeChanged(ZString newMessageType)
	{
		if (newMessageType != JobMessageTypeList.Codes.Export)
		{
			JZ_UCR = ZString.Empty;
			ZG_TransportChargesMethodOfPayment = ZString.Empty;
		}
	}

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Belgium;

	[ResourceStringData("E20E96DF-5295-486A-9EE6-A96B526346D1", Caption = "[UCC 4/1] Incoterm")]
	public override ZString JZ_IncoTerm { get => base.JZ_IncoTerm; set => base.JZ_IncoTerm = value; }

	[ResourceStringData("B40D00F4-3B08-4393-8A91-751391FD4F4D", Caption = "Incoterm Place", FullDescription = "[UCC 4/1] Incoterm Place")]
	public override ZString ZG_AgreedPlaceCode { get => base.ZG_AgreedPlaceCode; set => base.ZG_AgreedPlaceCode = value; }

	[ResourceStringData("182F6ED6-99E7-42E1-956B-812D515FF548", ShortCaption = "Invoice Amt", Caption = "Invoice Amount", FullDescription = "[UCC 4/10] Invoice Amount", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	[ResourceStringData("448E2031-B6BA-4BD8-8EFC-0DB2F25800DF", ShortCaption = "Invoice Amt", Caption = "Invoice Amount", FullDescription = "[UCC 4/10] Invoice Amount", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZDecimal JZ_InvoiceAmount
	{
		get => base.JZ_InvoiceAmount;
		set => base.JZ_InvoiceAmount = value;
	}

	[ResourceStringData("0760D598-E7D8-4B98-BB7C-8877C4B8E5D1", ShortCaption = "Exch. Rate", Caption = "Exchange Rate", FullDescription = "[UCC 4/15] Exchange Rate")]
	public override ZDecimal JZ_InvoiceCurrExRate
	{
		get => base.JZ_InvoiceCurrExRate;
		set => base.JZ_InvoiceCurrExRate = value;
	}

	[ResourceStringData("0A92BCCA-7B15-4130-8967-F06BDAEE9862", Caption = "[24] Tran. Nature", FullDescription = "[24] Transaction Nature")]
	[ResourceStringData("8B6D0340-8AB7-4C4B-BD13-51020A0E40AC", Caption = "[24] Tran. Nature", FullDescription = "[24] Transaction Nature", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZString JZ_ValuationCode
	{
		get => base.JZ_ValuationCode;
		set => base.JZ_ValuationCode = value;
	}

	protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
	{
		var dec = JobDeclaration;
		return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
	}

	protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
	{
		var collection = new InvoiceLineDependentCollection(this);
		collection.Load();
		return new JobComInvoiceLineViewCollection(this, collection);
	}

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		return result;
	}

	protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges() => new InvoiceChargeCollection<InvoiceCharge>(this);

	protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation() => new JobComInvoiceHeaderValidation(this);

	protected override EU.Business.Declaration.AddInfoJobComInvoiceHeader GetNewAddInfo() => new AddInfoJobComInvoiceHeader(JZ_AddInfoInfo);

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);
}
