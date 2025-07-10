using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business;

public abstract class CommonQuarantineExDocLineLookups : QuarantineExDocLineLookups
{
	protected CommonQuarantineExDocLineLookups(AutoQuarantineExDocLine parent)
		: base(parent)
	{
	}

	protected sealed override void Setup()
	{
	}

	protected new QuarantineExDocLine Parent => (QuarantineExDocLine)base.Parent;

	protected char CommodityCodeSingleChar
	{
		get
		{
			var commodityCode = Parent.QuarantineExDocHeader?.QH_ProduceType ?? ZString.Empty;
			var commodityCodeSingle = EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(commodityCode);
			return string.IsNullOrEmpty(commodityCodeSingle) ? ' ' : commodityCodeSingle[0];
		}
	}

	public abstract CodeDescriptionPairList FarmType { get; }

	public abstract EXDOCRefCodeCollection ProductTypes { get; }

	public abstract BusinessObjectCollection CategoryCodes { get; }

	public abstract ICollection SupplementaryCodes { get; }

	public abstract CodeDescriptionPairList PackType { get; }

	public abstract CodeDescriptionPairList NatureOfCommodity { get; }

	public abstract CodeDescriptionPairList TreatmentType { get; }

	public abstract CodeDescriptionPairList Preservation { get; }

	public abstract EXDOCRefCodeCollection CutCodes { get; }

	public abstract CodeDescriptionPairList LocationQualifier { get; }

	public abstract CodeDescriptionPairList ProductPart { get; }

	public abstract CodeDescriptionPairList NetQuantityUnits { get; }

	public abstract EXDOCDominantProductCollection DominantProducts { get; }

	public abstract CodeDescriptionPairList NetImperialWeightUnit { get; }

	public abstract CodeDescriptionPairList Weight { get; }

	public abstract CodeDescriptionPairList MetricWeight { get; }

	public abstract CodeDescriptionPairList PackageTypes { get; }

	public abstract CodeDescriptionPairList PackAccuracy { get; }

	public abstract CodeDescriptionPairList AqisCustomsWeightUqList { get; }

	public CodeDescriptionPairList FishWaterIndicatorList => Factory.GetCachedValue<EXDOCFishWaterIndicatorList>();
}
