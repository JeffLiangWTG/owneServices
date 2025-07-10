using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class OpportunityClosedReasonsRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<OpportunityClosedReasonsCollection, OpportunityClosedReasonsCollection>
	{
		public OpportunityClosedReasonsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, OpportunityClosedReasonsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new OpportunityClosedReasonsRegistryDataType(defaultValue), storage, options, defaultValue))
		{
		}

		public override int MaxLength => 256;

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var collection = (OpportunityClosedReasonsCollection)base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK);
			collection.CurrentFallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK);
			return collection;
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.OpportunityClosedReasonsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class OpportunityClosedReasonsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OpportunityClosedReasonsCollection>
	{
		public OpportunityClosedReasonsRegistryDataType(OpportunityClosedReasonsCollection defaultValue)
			: base(defaultValue)
		{
		}
	}
}
