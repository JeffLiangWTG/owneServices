using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.NL.Business.Declaration;

[SystemDefinedValues]
public class JobComInvoiceHeader : AutoJobComInvoiceHeader
	, Integration.Customs.NL.IJobComInvoiceHeader
	, Integration.Customs.ICusSupportingInfoTypeSupporter
{
	public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.JobComInvoiceHeader.Schema
	{
		public const string ZG_VDN = "ZG_VDN";
		public const string ZG_ValuationMarkup = "ZG_ValuationMarkup";
		public const int VDNMaxLength = 35;
	}

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

	public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

	public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

	public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	public new AddInfoJobComInvoiceHeader AddInfo => (AddInfoJobComInvoiceHeader)base.AddInfo;

	public new AddInfoJobComInvoiceHeaderLookups AddInfoLookups => (AddInfoJobComInvoiceHeaderLookups)base.AddInfoLookups;

	public override ZGuid JZ_OH_Supplier
	{
		get { return base.JZ_OH_Supplier; }
		set
		{
			if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Supplier))
			{
				var oldValue = JZ_OH_Supplier;
				base.JZ_OH_Supplier = value;
				if (oldValue != JZ_OH_Supplier && !IsCopying)
				{
					PopulateFromSupplierLink();
					PopulateValuesFromSupplierLink();
				}
			}
		}
	}

	public void PopulateValuesFromSupplierLink()
	{
		if (SupplierBuyerLink != null)
		{
			if (!SupplierBuyerLink.OL_ValuationBasisDeterminationNum.IsEmpty && ZG_VDN.IsEmpty)
			{
				ZG_VDN = SupplierBuyerLink.OL_ValuationBasisDeterminationNum;
			}

			if (!SupplierBuyerLink.OL_ValuationBasisMarkupPercent.IsEmpty && ZG_ValuationMarkup.IsEmpty)
			{
				ZG_ValuationMarkup = SupplierBuyerLink.OL_ValuationBasisMarkupPercent;
			}
		}
	}

	[ResourceStringData("JobComInvoiceHeader.ZG_VDN", Caption = "VDN")]
	[MaxLength(Schema.VDNMaxLength)]
	public ZString ZG_VDN
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.ZG_VDN);
		set
		{
			var oldValue = ZG_VDN;
			CheckMaximumLength(ZG_VDNInfo, value);
			this.SetSystemDefinedValue(Schema.ZG_VDN, value);
			ZG_VDNInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo ZG_VDNInfo => GetZPropertyInfo(Schema.ZG_VDN);

	[ResourceStringData("JobComInvoiceHeader.ZG_ValuationMarkup", Caption = "Valuation Markup %")]
	public ZDecimal ZG_ValuationMarkup
	{
		get => this.GetSystemDefinedValue<ZDecimal>(Schema.ZG_ValuationMarkup);
		set
		{
			var oldValue = ZG_ValuationMarkup;
			this.SetSystemDefinedValue(Schema.ZG_ValuationMarkup, value);
			if (!IsCopying && oldValue != ZG_ValuationMarkup)
			{
				cachedInvoiceLineTotal = null;
			}
			ZZG_ValuationMarkupInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo ZZG_ValuationMarkupInfo => GetZPropertyInfo(Schema.ZG_ValuationMarkup);

	[ResourceStringData("182F6ED6-99E7-42E1-956B-812D515FF547", Caption = "Invoice Amount", FullDescription = "[UCC 4/10] Invoice Amount", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	[ResourceStringData("448E2031-B6BA-4BD8-8EFC-0DB2F25800DG", Caption = "Invoice Amount", FullDescription = "[UCC 4/10] Invoice Amount", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZDecimal JZ_InvoiceAmount
	{
		get => base.JZ_InvoiceAmount;
		set
		{
			var oldValue = base.JZ_InvoiceAmount;
			base.JZ_InvoiceAmount = value;
			if (!IsCopying && oldValue != base.JZ_InvoiceAmount)
			{
				cachedInvoiceLineTotal = null;
			}
		}
	}

	public override ZDecimal InvoiceLineTotal
		=> Factory.GetValue(
			ref cachedInvoiceLineTotal,
			() => JZ_InvoiceAmount + (JZ_InvoiceAmount * ZG_ValuationMarkup / 100) - JZ_Calc_ChargesExcludedFromITOT);
	CachedProperty<ZDecimal> cachedInvoiceLineTotal;

	[ResourceStringData("114CDB09-304B-4DCC-939A-EDE9629BD0B4", Caption = "[24] Tran. Nature", FullDescription = "[24] Transaction Nature")]
	public override ZString JZ_ValuationCode
	{
		get => base.JZ_ValuationCode;
		set
		{
			base.JZ_ValuationCode = value;
			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				invoiceLine.AddInfo.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("878C6A00-4C48-412F-9DBE-152440A46FCC", Caption = "Commercial Ref.")]
	public override ZString JZ_UCR { get => base.JZ_UCR; set => base.JZ_UCR = value; }

	[ResourceStringData("E20E96DF-5295-486A-9EE6-A96B526346D9", Caption = "[UCC 4/1] Incoterm")]
	public override ZString JZ_IncoTerm { get => base.JZ_IncoTerm; set => base.JZ_IncoTerm = value; }

	[ResourceStringData("B40D00F4-3B08-4393-8A91-751391FD4F4C", Caption = "Incoterm Place", FullDescription = "[UCC 4/1] Incoterm Place")]
	public override ZString ZG_AgreedPlaceCode { get => base.ZG_AgreedPlaceCode; set => base.ZG_AgreedPlaceCode = value; }

	[ResourceStringData("AE7E3B2C-0971-4D29-B49D-CE111DCD1792", Caption = "Agreed Place", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZString JZ_IncoTermPlace { get => base.JZ_IncoTermPlace; set => base.JZ_IncoTermPlace = value; }

	[ResourceStringData("1E7B8A7B-CBAE-471A-BC79-0A0A553D04DB", Caption = "[UCC 4/11] Curr.")]
	public override ZString JZ_RX_NKInvoice_Currency { get => base.JZ_RX_NKInvoice_Currency; set => base.JZ_RX_NKInvoice_Currency = value; }

	[ResourceStringData("0760D598-E7D8-4B98-BB7C-8877C4B8E5D2", Caption = "Exchange Rate", FullDescription = "[UCC 4/15] Exchange Rate")]
	public override ZDecimal JZ_InvoiceCurrExRate
	{
		get => base.JZ_InvoiceCurrExRate;
		set => base.JZ_InvoiceCurrExRate = value;
	}

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	protected override EU.Business.Declaration.AddInfoJobComInvoiceHeader GetNewAddInfo() => new AddInfoJobComInvoiceHeader(JZ_AddInfoInfo);

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		return result;
	}

	protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
	{
		var collection = new InvoiceLineDependentCollection(this);
		collection.Load();
		return new JobComInvoiceLineViewCollection(this, collection);
	}

	protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
	{
		JobComInvoiceHeaderValidation result;
		if (IsImport)
		{
			result = new ImportJobComInvoiceHeaderValidation(this);
		}
		else if (IsExport)
		{
			result = new ExportJobComInvoiceHeaderValidation(this);
		}
		else
		{
			result = new JobComInvoiceHeaderValidation(this);
		}
		return result;
	}

	protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
	{
		JobComInvoiceHeaderLookups result;
		if (IsImport)
		{
			result = new ImportJobComInvoiceHeaderLookups(this);
		}
		else if (IsExport)
		{
			result = new ExportJobComInvoiceHeaderLookups(this);
		}
		else
		{
			result = new JobComInvoiceHeaderLookups(this);
		}
		return result;
	}

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Netherlands;

	protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
	{
		var dec = JobDeclaration;
		return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
	}

	void PopulateFromSupplierLink()
	{
		foreach (var invoiceLine in InvoiceLines.Cast<JobComInvoiceLine>())
		{
			invoiceLine.PopulateFromSupplierLink();
		}
	}
}
