using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.IT.H7.Business;

[DependentBusinessObject(typeof(AsycudaBill), nameof(AsycudaBill.PackedItems))]
public class AsycudaPackedItem : EU.H7.Business.AsycudaPackedItem
{
	public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

	public new AsycudaBill Bill => (AsycudaBill)base.Bill;

	public new AsycudaPack Pack => (AsycudaPack)base.Pack;

	public new IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos => (IAdditionalInfoCollection<AdditionalInfo>)base.AdditionalInfos;

	public new ISupportingDocumentCollection<SupportingDocument> SupportingDocuments => (ISupportingDocumentCollection<SupportingDocument>)base.SupportingDocuments;

	public new IPreviousDocumentCollection<PreviousDocument> PreviousDocuments => (IPreviousDocumentCollection<PreviousDocument>)base.PreviousDocuments;

	protected override IAdditionalInfoCollection<EU.H7.Business.AdditionalInfo> CreateAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);

	protected override ISupportingDocumentCollection<EU.H7.Business.SupportingDocument> CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection<SupportingDocument>(this);

	protected override IPreviousDocumentCollection<EU.H7.Business.PreviousDocument> CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection<PreviousDocument>(this);

	#region ICusSupportingInfoTypeSupporter

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
	{
		var result = base.GetCusSupportingInfoTypesCore();
		result[H7CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[H7CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[H7CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	#endregion
}
