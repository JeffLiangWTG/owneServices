using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyWithRelatedFilters : DummyBusinessObject, IRelatedModuleFilterSupportable
	{
		public DummyWithRelatedFilters(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public FilterRuleProvider FilterProvider => filterProvider ?? (filterProvider = new FilterRuleProvider(ModuleIdentifier, this));

		protected virtual ModuleIdentifier ModuleIdentifier => DummyModuleIDs.Dummy;

		FilterRuleProvider filterProvider;

		public StmModuleFilter Filter => FilterProvider.GetOrCreateAndCacheFilter();

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (DummyWithRelatedFilters)base.CloneInternal(args);

			FilterProvider.CopyFilterStrips(() => clone.Filter);

			return clone;
		}

		public override void Delete()
		{
			FilterProvider.DeleteFilter();
			base.Delete();
		}

		#region Overrides of DummyBaseBusinessObject

		protected override ZString HumanReadableNameCore => "Dummy Business Object: " + Z0_Description;

		#endregion
	}
}
