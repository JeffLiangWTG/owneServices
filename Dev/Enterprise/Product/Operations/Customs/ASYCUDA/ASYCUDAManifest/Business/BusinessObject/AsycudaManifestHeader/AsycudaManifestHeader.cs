using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDAManifest.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool AMA_MessageStatus_ReadOnly => false;
		public bool ShowExportGeneralManifest => AMA_RN_NKCountry == Core.Constants.CountryCodes.Bangladesh && AMA_Nature == Universal.Helper.ShipmentTypeList.Codes.Export22;
		public new ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;
		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);
		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		public new ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader> Persons => (ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader>)base.Persons;
		protected override ASYCUDA.Business.CusPersonCollection CreateNewCusPersonCollection() => new ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader>(this);
		protected override ASYCUDA.Business.IAsycudaArrivalHeaderCollection<ASYCUDA.Business.AsycudaArrivalHeader> CreateNewAsycudaArrivalHeaderCollection() => new ASYCUDA.Business.AsycudaArrivalHeaderCollection<AsycudaArrivalHeader>(this);
		protected override Type GetArrivalHeaderTypeCore() => typeof(AsycudaArrivalHeader);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		protected override Type GetPersonTypeCore() => typeof(CusPerson);
		protected override bool RegistrationDetails_ReadOnly => false;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = ASYCUDAManifestTypes.Codes.ASY;
		}
#endif
	}
}
