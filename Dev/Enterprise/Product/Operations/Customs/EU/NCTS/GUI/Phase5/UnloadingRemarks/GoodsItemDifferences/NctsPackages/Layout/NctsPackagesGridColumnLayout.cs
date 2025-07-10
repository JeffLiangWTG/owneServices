using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class NctsPackagesGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ??= CreateLayout();
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = NctsPackagesGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.SequenceNumberCalcEditColumn);
			builder.AddColumn(euGridColumnBag.TypeOfDifferenceDropEditColumn);
			builder.AddColumn(euGridColumnBag.UnitCountCalcEditColumn);
			builder.AddColumn(euGridColumnBag.UnitTypeDropEditColumn);
			builder.AddColumn(euGridColumnBag.MarksAndNumbersTextBoxColumn);

			return builder.Build();
		}

		#endregion
	}
}
