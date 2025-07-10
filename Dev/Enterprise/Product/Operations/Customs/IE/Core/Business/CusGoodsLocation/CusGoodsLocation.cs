using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business
{
	public class CusGoodsLocation : EU.Business.CusGoodsLocation, Integration.Customs.IE.ICusGoodsLocation
	{
		public const int CGL_AdditionalIdentifierMaxLength = 3;

		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void BeginEditCore()
		{
			SetDefaultQualifier();
		}

		protected override void SetDefaultsForNew()
		{
			SetDefaultQualifier();
		}

		public ICusGoodsLocationProviderWithUCCVersion UCCVersionProvider => CachedValueHelper.GetValue(ref uccVersionProviderCache, () => Parent as ICusGoodsLocationProviderWithUCCVersion);
		CachedValue<ICusGoodsLocationProviderWithUCCVersion> uccVersionProviderCache;

		protected override bool CGL_QualifierReadonlyCore => UCCVersionProvider is ICusGoodsLocationProviderWithUCCVersion provider && provider.IsUCC5;

		public void SetDefaultQualifier()
		{
			if (GetDefaultQualifier() is string defaultQualifier && defaultQualifier != CGL_Qualifier)
			{
				CGL_Qualifier = defaultQualifier;
			}
		}

		string GetDefaultQualifier() => UCCVersionProvider is ICusGoodsLocationProviderWithUCCVersion provider && provider.IsUCC5
			? Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode
			: null;

		public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

		public override ZString Unlocode
		{
			get
			{
				return CGL_CustomsOffice;
			}
			set
			{
				CGL_CustomsOffice = value;
			}
		}
		public new ZPropertyInfo UnlocodeInfo => GetWrappedZPropertyInfo(nameof(Unlocode), x => CGL_CustomsOfficeInfo);
		protected override int AdditionalIdentifierMaxLength => CGL_AdditionalIdentifierMaxLength;
	}
}
