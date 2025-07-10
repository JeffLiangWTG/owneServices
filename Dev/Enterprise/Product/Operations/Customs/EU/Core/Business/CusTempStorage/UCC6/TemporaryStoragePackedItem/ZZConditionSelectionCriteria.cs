using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.CusTempStorage;

public class ZZConditionSelectionCriteria<T> : IZZConditionSelectionCriteria where T : TemporaryStoragePackedItem
{
	public ZZConditionSelectionCriteria(T tempStoragePackedItem) => this.tempStoragePackedItem = tempStoragePackedItem;
	public ZZConditionSelectionCriteria(T tempStoragePackedItem, ZString conditionClass, ZString conditionType)
		: this(tempStoragePackedItem)
	{
		ConditionClass = conditionClass;
		ConditionType = conditionType;
	}
	protected T tempStoragePackedItem;

	public ZDateTime EffectiveDate => tempStoragePackedItem.ValuationDate;
	public virtual ZString TradeGroupCountry => tempStoragePackedItem.EffectiveCountryOfOrigin;
	public ZString PrimaryPreference => ZString.Empty;
	public virtual ISet<ZString> AdditionalCodes => new HashSet<ZString> { ZString.Empty };
	public ZString ConcessionOrder => ZString.Empty;
	public ZString DataGrouping => tempStoragePackedItem.GetDataGroupingForUniversalTariff;
	public virtual ConditionChecker.ConditionDirection Direction => ConditionChecker.ConditionDirection.Import;
	public ZString ConditionClass { get; }
	public ZString ConditionType { get; }
	public ISet<ZString> SecondTradeGroups => new HashSet<ZString>() { ZString.Empty };
}
