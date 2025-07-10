using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusClassPartPivotAddInfoLookups : AUAddInfoLookups
	{
		public CusClassPartPivotAddInfoLookups(CusClassPartPivotAddInfo parent)
			: base(parent)
		{
		}

		protected new CusClassPartPivotAddInfo AddInfo
		{
			get { return (CusClassPartPivotAddInfo)Parent; }
		}

		#region AQIS Lookups and Lists
		public CodeDescriptionPairList ProduceTypeList
		{
			get
			{
				var isOtherActive = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today);
				return Factory.GetCachedValue("QuarantineExDocHeader_ProduceType" + isOtherActive, () =>
				{
					var result = new EXDOCCommodityCodes();
					if (!isOtherActive)
					{
						result.RemoveCode(EXDOCCommodityCodes.Codes.OtherGoods);
					}
					return result;
				});
			}
		}

		public EXDOCProductTypeCollection ProductList
		{
			get { return new EXDOCProductTypeCollection(AddInfo); }
		}

		public EXDOCSupplementaryCodeCollection SupplementaryCodesList
		{
			get { return new EXDOCSupplementaryCodeCollection(AddInfo); }
		}

		public CodeDescriptionPairList PackTypeList
		{
			get { return Factory.GetCachedValue<EXDOCPackTypeCodes>(); }
		}

		public CodeDescriptionPairList PreservationList
		{
			get { return Factory.GetCachedValue<EXDOCPreservationTypeCodes>(); }
		}

		public EXDOCCutCodeCollection CutCodesList
		{
			get { return new EXDOCCutCodeCollection(AddInfo); }
		}

		public NEXDOCCategoryCodesCollection CategoryCodes => new NEXDOCCategoryCodesCollection(AddInfo, AddInfo.ZA_AQISProduct_Hidden);
		#endregion
	}
}
