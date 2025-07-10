using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class JobComInvoiceLine
{
	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
	public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public new CommonJobComInvoiceLineValidation Validation => (CommonJobComInvoiceLineValidation)base.Validation;

	public new CusEntryLine CusEntryLine => (CusEntryLine)base.CusEntryLine;

	public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

	protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.AddInfoJobComInvoiceLine GetNewAddInfo() => new AddInfoJobComInvoiceLine(JI_AddInfoInfo);

	protected override Customs.Business.InvoiceLinePackagePivotCollection GetNewPackagesPivotCore() => new InvoiceLinePackagePivotCollection(this);

	protected override ICusSupplyChainActorReferenceCollection<EU.Business.Declaration.CusSupplyChainActorReference> GetNewCusSupplyChainActorReferenceCollection() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

	public new InvoiceLinePackagePivotCollection PackagesPivot => (InvoiceLinePackagePivotCollection)base.PackagesPivot;

	public new AddInfoJobComInvoiceLineLookups AddInfoLookups => (AddInfoJobComInvoiceLineLookups)base.AddInfoLookups;

	protected override EU.Business.Declaration.AddInfoJobComInvoiceLineLookups GetAddInfoJobComInvoiceLineLookupsCore(EU.Business.Declaration.AddInfoJobComInvoiceLine addInfo) => new AddInfoJobComInvoiceLineLookups(addInfo);

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection()
	{
		var additionalInfos = new AdditionalInfoCollection(this);
		EnableOrDisableAdditionalInfosMaxCountValidation(additionalInfos);
		return additionalInfos;
	}

	protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Italy;
	protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[ITCusSupportingInfoTypeList.Codes.Remarks] = typeof(CusSupportingInfo);
		return result;
	}

	protected override Dictionary<ZString, Type> GetCusCodeDataTypes()
	{
		var result = base.GetCusCodeDataTypes();
		result[EU.Business.CusCodeDataTypeList.Codes.AdditionalProcedureCode] = typeof(AdditionalProcedureCode);
		return result;
	}

	protected new ITCustomsValuationCalculator ValuationCalculator => (ITCustomsValuationCalculator)base.ValuationCalculator;
	protected override ICustomsValuationCalculator GetValuationCalculatorCore() => new ITCustomsValuationCalculator(this);

	#region Implementation of IAdditionalBusinessObjectFetchStrategyProvider

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		yield return new CusCodeDataTypeSupporterFetchStrategy(this);
	}

	#endregion
}
