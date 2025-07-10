using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;

		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;

		public override AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;

		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);

		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Spain;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			AMA_ManifestType = ESManifestTypes.Codes.ICS;
		}
#endif
	}
}
