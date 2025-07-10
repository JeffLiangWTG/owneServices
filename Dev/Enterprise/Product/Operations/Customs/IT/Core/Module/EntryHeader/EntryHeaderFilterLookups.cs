using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Module;

public class EntryHeaderFilterLookups : EU.Module.EntryHeaderFilterLookups
{
	public EntryHeaderFilterLookups(EntryHeaderFilterBusinessObject filterBizObj) : base(filterBizObj)
	{
	}

	public CustomsChannelCodeList ControlChannelList => Factory.GetCachedValue<CustomsChannelCodeList>();

	public override CodeDescriptionPairList EntryInstructionStyleList
	{
		get
		{
			return Factory.GetCachedValue("IT.EntryHeaderFilterLookups.EntryInstructionStyleList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(new SADDeclarationTypeList());
				result.AddRange(new ImportUCC6DeclarationTypeList());
				result.AddRange(new ExportUCC6DeclarationTypeList());
				return result;
			});
		}
	}

	public RefCurrencyCollection CurrencyList
	{
		get
		{
			var currencyCollection = new RefCurrencyCollection(Factory);
			currencyCollection.ApplySort(RefCurrency.Schema.RX_Code, System.ComponentModel.ListSortDirection.Ascending);
			return currencyCollection;
		}
	}

	public ExitStatusList ExitStatusList => Factory.GetCachedValue<ExitStatusList>();

	public ITGuaranteeReleaseResultList ArrivalStatusList => Factory.GetCachedValue<ITGuaranteeReleaseResultList>();
}
