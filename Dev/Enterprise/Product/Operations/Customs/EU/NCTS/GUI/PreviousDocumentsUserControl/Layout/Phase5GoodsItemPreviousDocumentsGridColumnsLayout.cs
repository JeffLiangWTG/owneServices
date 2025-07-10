using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class Phase5GoodsItemPreviousDocumentsGridColumnsLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = Phase5GoodsItemPreviousDocumentsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			GetColumns(euGridColumnBag).ForEach(column => builder.AddColumn(column));

			return builder.Build();
		}

		protected virtual IEnumerable<IGridColumnReference> GetColumns(Phase5GoodsItemPreviousDocumentsGridColumnsBag euGridColumnBag)
		{
			yield return euGridColumnBag.TypeCodeFindBoxColumn;
			yield return euGridColumnBag.ReferenceNumberTextBoxColumn;
			yield return euGridColumnBag.ItemNumberCalcEditColumn;
			yield return euGridColumnBag.NumOfPackagesCalcEditColumn;
			yield return euGridColumnBag.PackageTypeDropEditColumn;
			yield return euGridColumnBag.QuantityCalcEditColumn;
			yield return euGridColumnBag.UnitOfQuantityDropEditColumn;
			yield return euGridColumnBag.ComplementTextBoxColumn;
		}

		IGridColumnLayout layout;

		#endregion
	}
}
