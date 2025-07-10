using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCSQuarantineExDocLineLookups : CommonQuarantineExDocLineLookups
	{
		public EXDOCSQuarantineExDocLineLookups(AutoQuarantineExDocLine parent)
			: base(parent)
		{
		}

		public override ICollection SupplementaryCodes => new EXDOCSupplementaryCodeCollection(Parent.QuarantineExDocHeader);

		public override CodeDescriptionPairList FarmType => null;

		public override CodeDescriptionPairList PackType => Factory.GetCachedValue<EXDOCPackTypeCodes>();

		public override CodeDescriptionPairList NatureOfCommodity => Factory.GetCachedValue<EXDOCNatureOfCommodityCodes>();

		public override CodeDescriptionPairList Preservation => Factory.GetCachedValue<EXDOCPreservationTypeCodes>();

		public override CodeDescriptionPairList NetQuantityUnits => QuantityUnits;

		public override CodeDescriptionPairList PackageTypes
		{
			get
			{
				var destinationCountryCode = Parent.QuarantineExDocHeader?.Declaration?.FinalDestination?.Country.Code ?? ZString.Empty;
				if (destinationCountryCode.IsEmpty)
				{
					return Factory.GetCachedValue<EXDOCPacakgeTypeCodes>();
				}
				else
				{
					if (Factory.IsMemberOfEU(destinationCountryCode) || destinationCountryCode == Core.Constants.CountryCodes.Turkey)
					{
						return Factory.GetCachedValue("RFPPackageTypesForEUDestination", () =>
						{
							var packageTypes = new EXDOCPacakgeTypeCodes();
							packageTypes.RemoveCode("PB");
							packageTypes.RemoveCode("PC");
							return packageTypes;
						});
					}
					else
					{
						return Factory.GetCachedValue("RFPPackageTypesForNonEUDestination", () =>
						{
							var packageTypes = new EXDOCPacakgeTypeCodes();
							packageTypes.RemoveCode("QR");
							packageTypes.RemoveCode("PP");
							return packageTypes;
						});
					}
				}
			}
		}

		public override CodeDescriptionPairList Weight => (Parent.QuarantineExDocHeader?.QH_ProduceType ?? ZString.Empty) == EXDOCCommodityCodes.Codes.Meat ? Factory.GetCachedValue<EXDOCMetricWeightUnitCodes>() : QuantityUnits;

		public override CodeDescriptionPairList MetricWeight => Factory.GetCachedValue<EXDOCMetricWeightUnitCodes>();

		public override EXDOCRefCodeCollection ProductTypes => new EXDOCProductTypeCollection(Parent.QuarantineExDocHeader);

		public override BusinessObjectCollection CategoryCodes => null;

		public override CodeDescriptionPairList TreatmentType => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSTreatmentType, ZDate.Today);

		public override EXDOCRefCodeCollection CutCodes => new EXDOCCutCodeCollection(Parent.QuarantineExDocHeader);

		public override CodeDescriptionPairList LocationQualifier => Factory.GetCachedValue<EXDOCLocationQualifier>();

		public override CodeDescriptionPairList ProductPart => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSProductPart, ZDate.Today);

		public override EXDOCDominantProductCollection DominantProducts => new EXDOCDominantProductCollection(Parent.QuarantineExDocHeader);

		public override CodeDescriptionPairList NetImperialWeightUnit => Factory.GetCachedValue<EXDOCImperialWeightUnitCodes>();

		public override CodeDescriptionPairList AqisCustomsWeightUqList => Factory.GetCachedValue<EXDOCErrata32List36CustomsWeightUnits>();

		public override CodeDescriptionPairList PackAccuracy => Factory.GetCachedValue<EXDOCPackAccuracyCodes>();

		CodeDescriptionPairList QuantityUnits => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSUnitOfMeasurement, ZDate.Today);
	}
}
