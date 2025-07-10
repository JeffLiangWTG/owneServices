using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class SupportingDocumentsGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ??= CreateLayout();
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = SupportingDocumentsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			GetColumns(euGridColumnBag).ForEach(column => builder.AddColumn(column));

			return builder.Build();
		}

		protected virtual IEnumerable<IGridColumnReference> GetColumns(SupportingDocumentsGridColumnsBag euGridColumnBag)
		{
			yield return euGridColumnBag.CodeCodeFindBoxColumn;
			yield return euGridColumnBag.ReferenceNumberTextBoxColumn;
			yield return euGridColumnBag.ItemNumberCalcEditColumn;
			yield return euGridColumnBag.ReferenceNumber2TextBoxColumn;
		}

		#endregion
	}
}
