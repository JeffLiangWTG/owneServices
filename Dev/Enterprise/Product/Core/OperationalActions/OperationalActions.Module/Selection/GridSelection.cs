using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.Module
{
	internal sealed class GridSelection : TargetRecordSelection
	{
		public GridSelection(ZGrid grid)
		{
			this.grid = grid;
		}

		public GridSelection(ZModuleButtonGrid grid)
			: this(grid.InnerGrid)
		{
		}

		public override ISelectedRecords GetSelectedRecords()
		{
			BusinessObject[] targets = grid.SelectedElements;
			bool noTargetsSelected = targets.Length == 0;

			if (targets.Length == 0)
			{
				IList allValues = grid.ListManager.List;

				targets = new BusinessObject[allValues.Count];
				allValues.CopyTo(targets, 0);
			}

			targets = ExcludeBusinessObject(targets).ToArray();

			return new SelectedRecords()
			{
				AutoSelectedAllKeys = noTargetsSelected,
				PrimaryKeys = targets.Select(bizObj => bizObj.PK).ToArray()
			};
		}

		public override int FilterRowCount => 0;

		readonly ZGrid grid;
	}
}
