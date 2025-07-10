using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IT.H7.Business;

[DependentBusinessObject(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.Bills))]
public class AsycudaBill : EU.H7.Business.AsycudaBill
{
	public AsycudaBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

	public new CusGoodsLocation CusGoodsLocation => (CusGoodsLocation)base.CusGoodsLocation;

	protected override Type GetPackTypeCore() => typeof(AsycudaPack);

	protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

	public new IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> PackedItems => (IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>)base.PackedItems;

	public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;

	protected override IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

	public new IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos => (IAdditionalInfoCollection<AdditionalInfo>)base.AdditionalInfos;

	public new ISupportingDocumentCollection<SupportingDocument> SupportingDocuments => (ISupportingDocumentCollection<SupportingDocument>)base.SupportingDocuments;

	public new IPreviousDocumentCollection<PreviousDocument> PreviousDocuments => (IPreviousDocumentCollection<PreviousDocument>)base.PreviousDocuments;

	protected override IAdditionalInfoCollection<EU.H7.Business.AdditionalInfo> CreateAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);

	protected override ISupportingDocumentCollection<EU.H7.Business.SupportingDocument> CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection<SupportingDocument>(this);

	protected override IPreviousDocumentCollection<EU.H7.Business.PreviousDocument> CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection<PreviousDocument>(this);

	protected override IAsycudaBillPackedItemCollection<ManifestBase.AsycudaPackedItem, ManifestBase.AsycudaBill> CreateNewAsycudaBillPackedItemCollection() => new AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>(this);

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

	public string GetAndSetLRNIfNeeded()
	{
		if (LocalReferenceNumber.IsEmpty)
		{
			var lrnGenerator = new LocalReferenceNumberGenerator(Factory) as ILocalReferenceNumberGenerator;
			var lrn = lrnGenerator.Generate();
			LocalReferenceNumber = lrn;
		}

		return LocalReferenceNumber;
	}
}
