using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public sealed class CusTempStorageRegLinesSelectionHeader : NonPersistentBusinessObject
	{
		public CusTempStorageRegLinesSelectionHeader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusTempStorageSelectableRegLineCollection SelectableLines => selectableLines ??= [];
		CusTempStorageSelectableRegLineCollection selectableLines;

		public CusTempStorageSelectableRegLineCollection SelectedLines => selectedLines ??= [];
		CusTempStorageSelectableRegLineCollection selectedLines;

		public void ClearDrawQuantities()
		{
			foreach (CusTempStorageSelectableRegLine selectableLine in selectableLines)
			{
				selectableLine.GrossWeightToDraw = ZDecimal.Zero;
				selectableLine.PackagesToDraw = ZInt.Zero;
			}
		}

		public void FillOutDrawQuantities()
		{
			foreach (CusTempStorageSelectableRegLine selectableLine in selectableLines)
			{
				selectableLine.GrossWeightToDraw = selectableLine.GrossWeightOnHand;
				selectableLine.PackagesToDraw = selectableLine.PackagesQtyOnHand;
			}
		}

		public void Load(IEnumerable<ICusTempStorageRegLine> selectableLines)
		{
			SelectableLines.RemoveAll();
			SelectedLines.RemoveAll();

			foreach (var item in selectableLines)
			{
				var tempStorageRegLine = new CusTempStorageSelectableRegLine(item);
				tempStorageRegLine.PackagesToDrawInfo.ValueChanged += UpdateSelectableItemCollection;
				tempStorageRegLine.GrossWeightToDrawInfo.ValueChanged += UpdateSelectableItemCollection;
				SelectableLines.Add(tempStorageRegLine);
			}
		}

		void UpdateSelectableItemCollection(object sender, EventArgs e)
		{
			SelectedLines.RemoveAll();
			SelectedLines.AddRange(SelectableLines.Where(x => x.PackagesToDraw > ZInt.Zero && !x.PackagesToDrawInfo.HasNotifications() || x.GrossWeightToDraw > ZDecimal.Zero && !x.GrossWeightToDrawInfo.HasNotifications()));
		}
	}
}
