using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class GlowOpportunityStageRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<GlowOpportunityStageCollection, GlowOpportunityStageCollection>
	{
		public GlowOpportunityStageRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, GlowOpportunityStageCollection defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public GlowOpportunityStageRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, GlowOpportunityStageCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GlowOpportunityStageRegistryDataType(defaultValue), storage, options, defaultValue))
		{
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.GlowOpportunityStageRegistryItemEditor, Enterprise.Registry.GUI")]
	public class GlowOpportunityStageRegistryDataType : NonPersistentBusinessObjectRegistryDataTypeWithEnabledItem<GlowOpportunityStageCollection>
	{
		public GlowOpportunityStageRegistryDataType(GlowOpportunityStageCollection defaultValue)
			: base(defaultValue)
		{
		}

		protected override bool HasEnabledItem(GlowOpportunityStageCollection proposedValue)
		{
			return proposedValue.Any(x => ((GlowOpportunityStage)x).Bool);
		}
	}
}
