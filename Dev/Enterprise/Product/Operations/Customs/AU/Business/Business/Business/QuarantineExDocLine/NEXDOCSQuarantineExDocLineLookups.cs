using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCSQuarantineExDocLineLookups : CommonQuarantineExDocLineLookups
	{
		public NEXDOCSQuarantineExDocLineLookups(QuarantineExDocLine parent)
			: base(parent)
		{
		}

		public override ICollection SupplementaryCodes => RefCusAUNexdocECMCodeHelper.GetSupplementaryCachedList(Factory, CommodityCodeSingleChar, Parent.QL_ProductType, ZDate.Today);

		public override CodeDescriptionPairList FarmType => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSFarmType, ZDate.Today);

		public override CodeDescriptionPairList PackType => RefCusAUNexdocECMCodeHelper.GetPackTypeCachedList(Factory, CommodityCodeSingleChar, Parent.QL_ProductType, ZDate.Today);

		public override CodeDescriptionPairList NatureOfCommodity => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSNatureOfCommodity, ZDate.Today);

		public override CodeDescriptionPairList Preservation => RefCusAUNexdocECMCodeHelper.GetPreservationCachedList(Factory, CommodityCodeSingleChar, Parent.QL_ProductType, ZDate.Today);

		public override CodeDescriptionPairList NetQuantityUnits => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSUnitOfMeasurement, ZDate.Today);

		public override CodeDescriptionPairList PackageTypes => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSPackageType, ZDate.Today);

		public override CodeDescriptionPairList Weight => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSUnitOfMeasurement, ZDate.Today);

		public override CodeDescriptionPairList MetricWeight => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSUnitOfMeasurement, ZDate.Today);

		public override EXDOCRefCodeCollection ProductTypes => new NEXDOCProductTypeCollection(Parent.QuarantineExDocHeader);

		public override BusinessObjectCollection CategoryCodes => new NEXDOCCategoryCodesCollection(Parent.QuarantineExDocHeader, Parent.QL_ProductType);

		public override CodeDescriptionPairList TreatmentType => Factory.GetCachedValue<EXDOCTreatmentTypeCode>();

		public override EXDOCRefCodeCollection CutCodes => new NEXDOCCutCodesCollection(Parent.QuarantineExDocHeader);

		public override CodeDescriptionPairList LocationQualifier => Factory.GetCachedValue<EXDOCLocationQualifier>();

		public override CodeDescriptionPairList ProductPart => null;  // EXDOC list only currently.

		public override EXDOCDominantProductCollection DominantProducts => new EXDOCDominantProductCollection(Parent.QuarantineExDocHeader);

		public override CodeDescriptionPairList NetImperialWeightUnit => Factory.GetCachedValue<EXDOCImperialWeightUnitCodes>();

		public override CodeDescriptionPairList AqisCustomsWeightUqList => Factory.GetCachedValue<EXDOCErrata32List36CustomsWeightUnits>();

		public override CodeDescriptionPairList PackAccuracy => Factory.GetCachedValue<EXDOCPackAccuracyCodes>();
	}
}
