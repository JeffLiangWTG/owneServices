using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business
{
	public class CusGoodsLocation : EU.Business.CusGoodsLocation
	{
		public new class Schema : AutoCusGoodsLocation.Schema
		{
			public const int UnlocodeMaxLength = 17;
		}

		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly EU.Business.CusGoodsLocationTypeDecider TypeDecider = new CusGoodsLocationTypeDecider();

		public new AsycudaBill Parent => base.Parent as AsycudaBill;

		public AsycudaManifestHeader ManifestHeaderParent => base.Parent as AsycudaManifestHeader;

		public new CusGoodsLocationAddress Address => base.Address as CusGoodsLocationAddress;

		protected override Type AddressType => typeof(CusGoodsLocationAddress);

		public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

		protected override TypeLoaderCollection GetParentLoaders()
		{
			var typeLoaderCollection = base.GetParentLoaders();
			typeLoaderCollection.Add(ParentType);

			return typeLoaderCollection;
		}

		protected virtual Type ParentType => typeof(AsycudaBill);

		[MaxLength(Schema.UnlocodeMaxLength)]
		public override ZString Unlocode { get => base.Unlocode; set => base.Unlocode = value; }

		public override ZPropertyInfo CGL_TypeInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.CGL_TypeInfo); }
		}

		public override ZPropertyInfo CGL_QualifierInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.CGL_QualifierInfo); }
		}

		public new ZPropertyInfo CGL_AdditionalIdentifierInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.CGL_AdditionalIdentifierInfo); }
		}

		ZPropertyInfo GetInfoWithUpdatedHumanReadableName(ZPropertyInfo info)
		{
			return new CusGoodsLocationPropertyInfoStringWithHumanReadableNameHook(this, info);
		}

		#region Clone

		protected override bool SupportsCloneCore() => true;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var templateCopy = (CusGoodsLocation)base.CloneInternal(args);

			var clonedAddress = (EU.Business.CusGoodsLocationAddress)Address.Clone();
			clonedAddress.E2_ParentID = templateCopy.PK;
			clonedAddress.E2_ParentTableCode = CusGoodsLocationSchema.Constants.Prefix;
			clonedAddress.E2_AddressType = MasterFiles.Integration.AutoDocAddressTypes.Codes.Location;
			return templateCopy;
		}

		#endregion
	}
}
