using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ES.Business.Declaration;

public partial class JobComInvoiceLine
{
	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

	public new CusEntryLine CusEntryLine => (CusEntryLine)base.CusEntryLine;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

	public new AddInfoJobComInvoiceLineLookups AddInfoLookups => (AddInfoJobComInvoiceLineLookups)base.AddInfoLookups;

	public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

	protected override EU.Business.Declaration.AddInfoJobComInvoiceLine GetNewAddInfo() => new AddInfoJobComInvoiceLine(JI_AddInfoInfo);

	public new ESCustomsValuationCalculator ValuationCalculator => (ESCustomsValuationCalculator)base.ValuationCalculator;
	protected override ICustomsValuationCalculator GetValuationCalculatorCore() => new ESCustomsValuationCalculator(this);

	protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
	{
		JobComInvoiceLineValidation result;
		if (IsExport)
		{
			result = new ExportJobComInvoiceLineValidation(this);
		}
		else if (IsImport)
		{
			result = new ImportJobComInvoiceLineValidation(this);
		}
		else
		{
			result = new JobComInvoiceLineValidation(this);
		}
		return result;
	}

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	public new EU.Business.Declaration.InvoiceLineChargeCollection<InvoiceLineCharge> Charges => (EU.Business.Declaration.InvoiceLineChargeCollection<InvoiceLineCharge>)base.Charges;

	protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection() => new EU.Business.Declaration.InvoiceLineChargeCollection<InvoiceLineCharge>(this);

	public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

	protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection()
		=> new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

	protected override EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<EU.Business.Declaration.CusSupplyChainActorReference> GetNewCusSupplyChainActorReferenceCollection()
		=> new EU.Business.Declaration.CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);

	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);
}
