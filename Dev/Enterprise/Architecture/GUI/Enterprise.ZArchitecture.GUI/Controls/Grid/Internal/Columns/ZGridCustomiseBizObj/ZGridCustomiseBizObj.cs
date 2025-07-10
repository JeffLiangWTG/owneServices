using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Internal
{
	public class ZGridCustomiseBizObj : AutoZGridCustomiseBizObj
	{
		public ZGridCustomiseBizObj(string gridID, IGridLayoutStorage currentLayout)
			: this(new[] { gridID }, new[] { gridID }, currentLayout, ZGuid.Empty)
		{
		}

		public ZGridCustomiseBizObj(string[] gridIDsForStmModuleFilter, string[] gridIDsForStmData, IGridLayoutStorage currentLayout, ZGuid layoutContextPK)
			: base(new BusinessObjectFactory())
		{
			GridIDsForStmModuleFilter = gridIDsForStmModuleFilter;
			GridIDsForStmData = gridIDsForStmData;
			LayoutContextPK = layoutContextPK;

			using (GetValidationSuspender())
			{
				CurrentLayout = currentLayout;
			}
		}

		internal readonly string[] GridIDsForStmModuleFilter;
		internal readonly string[] GridIDsForStmData;
		internal readonly ZGuid LayoutContextPK;

		[List("Lookups.Layouts")]
		public override ZString CurrentLayoutNameDisplay
		{
			get { return base.CurrentLayoutNameDisplay; }
			set { base.CurrentLayoutNameDisplay = value; }
		}

		public IGridLayoutStorage CurrentLayout
		{
			get
			{
				if (CurrentLayoutCached == null || CurrentLayoutCached.IsDeleted || StmModuleFilter.GetDisplayName(CurrentLayoutCached.ColumnLayoutName, false) != CurrentLayoutNameDisplay)
				{
					var current = (GridLayoutStorageBizO)Lookups.Layouts.FirstOrDefault(layout => ((GridLayoutStorageBizO)layout).LayoutNameDisplay == CurrentLayoutNameDisplay);
					return current?.gridLayoutStorage;
				}
				else
				{
					return CurrentLayoutCached;
				}
			}
			set
			{
				CurrentLayoutNameDisplay = value == null || value.IsDeleted ? ZString.Empty : StmModuleFilter.GetDisplayName(value.ColumnLayoutDisplayName, isUserDefinedFilter: false);
				CurrentLayoutCached = value;
			}
		}

		public IGridLayoutStorage CurrentLayoutCached
		{
			get;
			private set;
		}

		public ZGridCustomiseBizObjLookups Lookups
		{
			get { return lookups ?? (lookups = new ZGridCustomiseBizObjLookups(this)); }
		}
		ZGridCustomiseBizObjLookups lookups;
	}
}
