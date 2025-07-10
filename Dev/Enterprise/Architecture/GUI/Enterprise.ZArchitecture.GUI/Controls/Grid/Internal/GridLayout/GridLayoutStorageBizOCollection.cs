using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Business.Internal
{
	public class GridLayoutStorageBizOCollection
		: NonPersistentBusinessObjectCollection<GridLayoutStorageBizO>
	{
		public GridLayoutStorageBizOCollection(string[] gridIDsForStmModuleFilter, string[] gridIDsForStmData, ZGuid layoutContextPK, BusinessObjectFactory factory)
			: base(factory)
		{
			this.gridIDsForStmModuleFilter = gridIDsForStmModuleFilter;
			this.gridIDsForStmData = gridIDsForStmData;
			this.layoutContextPK = layoutContextPK;
			BuildCollection();
		}

		readonly string[] gridIDsForStmModuleFilter;
		readonly string[] gridIDsForStmData;
		readonly ZGuid layoutContextPK;

		void BuildCollection()
		{
			foreach (var layout in new DataGridLayoutDataAccessor().GetSortedAllLayoutsForFormGrid(Factory, gridIDsForStmModuleFilter, gridIDsForStmData, layoutContextPK))
			{
				Add(new GridLayoutStorageBizO(layout, Factory));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
