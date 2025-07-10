using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentCollection(this);
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}

				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;

		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Spain;

		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}
	}
}
