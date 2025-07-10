using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Internal
{
	[CodeProperty(Schema.LayoutNameDisplay), DescriptionProperty(Schema.LayoutNameDisplay)]
	public class GridLayoutStorageBizO : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string LayoutNameDisplay = "LayoutNameDisplay";
		}

		public GridLayoutStorageBizO(IGridLayoutStorage gridLayoutStorage, BusinessObjectFactory factory) : base(factory)
		{
			this.gridLayoutStorage = gridLayoutStorage;
		}

		public readonly IGridLayoutStorage gridLayoutStorage;

		public ZString LayoutNameDisplay
		{
			get { return StmModuleFilter.GetDisplayName(gridLayoutStorage.ColumnLayoutDisplayName, isUserDefinedFilter: false); }
		}

		public virtual ZPropertyInfo LayoutNameDisplayInfo
		{
			get { return this.GetZPropertyInfo(Schema.LayoutNameDisplay); }
		}
	}
}
