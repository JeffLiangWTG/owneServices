using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class GlowOpportunityStatusRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<GlowOpportunityStatusCollection, GlowOpportunityStatusCollection>
	{
		public GlowOpportunityStatusRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, GlowOpportunityStatusCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GlowOpportunityStatusRegistryDataType(defaultValue), storage, options, defaultValue))
		{
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.GlowOpportunityStatusRegistryItemEditor, Enterprise.Registry.GUI")]
	public class GlowOpportunityStatusRegistryDataType : NonPersistentBusinessObjectRegistryDataTypeWithEnabledItem<GlowOpportunityStatusCollection>
	{
		public GlowOpportunityStatusRegistryDataType(GlowOpportunityStatusCollection defaultValue)
			: base(defaultValue)
		{
		}

		protected override string ValidationMessage => ResString.GetMultilingualString("04a0c73a-d8d5-e4b0-4854-c577239c2d51", "At least one each of ACT, UNS and SUC must be available and enabled.");

		protected override bool HasEnabledItem(GlowOpportunityStatusCollection proposedValue)
		{
			var enabledItems = new HashSet<ZString>(proposedValue.Where(x => ((GlowOpportunityStatus)x).Bool).Select(x => ((GlowOpportunityStatus)x).TradeStatus));

			return enabledItems.Contains(OpportunityTradeStatus.Codes.Active) && enabledItems.Contains(OpportunityTradeStatus.Codes.Successful) && enabledItems.Contains(OpportunityTradeStatus.Codes.Unsuccessful);
		}
	}
}
