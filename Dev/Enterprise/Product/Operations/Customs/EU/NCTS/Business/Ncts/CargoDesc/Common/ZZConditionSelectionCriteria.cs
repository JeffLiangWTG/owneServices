using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business;

public class ZZConditionSelectionCriteria<T> : IZZConditionSelectionCriteria where T : NctsCommonCargoDesc
{
	public ZZConditionSelectionCriteria(T cargoDesc) => this.cargoDesc = cargoDesc;
	public ZZConditionSelectionCriteria(T cargoDesc, ZString conditionClass, ZString conditionType)
		: this(cargoDesc)
	{
		ConditionClass = conditionClass;
		ConditionType = conditionType;
	}
	protected T cargoDesc;

	public ZDateTime EffectiveDate => cargoDesc.ValuationDate;
	public virtual ZString TradeGroupCountry => cargoDesc.EffectiveCountryOfOrigin;
	public ZString PrimaryPreference => ZString.Empty;
	public virtual ISet<ZString> AdditionalCodes => new HashSet<ZString> { ZString.Empty };
	public ZString ConcessionOrder => ZString.Empty;
	public ZString DataGrouping => cargoDesc.DataGroupingCode;
	public virtual ConditionChecker.ConditionDirection Direction => ConditionChecker.ConditionDirection.Import;
	public ZString ConditionClass { get; }
	public ZString ConditionType { get; }
	public ISet<ZString> SecondTradeGroups => new HashSet<ZString>() { ZString.Empty };
}
