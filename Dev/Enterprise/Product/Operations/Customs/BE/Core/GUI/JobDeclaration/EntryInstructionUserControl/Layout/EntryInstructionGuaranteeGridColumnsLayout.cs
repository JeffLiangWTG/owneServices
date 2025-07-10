using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI
{
	sealed class EntryInstructionGuaranteeGridColumnsLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = EntryInstructionGuaranteeGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.BondTypeDropEditColumn);
			builder.AddColumn(euGridColumnBag.BondNumberMultiControlColumn);
			builder.AddColumn(euGridColumnBag.BondNumber2TextBoxColumn);
			builder.AddColumn(euGridColumnBag.PasswordBoxColumn);
			builder.AddColumn(euGridColumnBag.HolderIdentificationTextBoxColumn);
			builder.AddColumn(euGridColumnBag.NkCurrencyCodeFindBoxColumn);
			builder.AddColumn(euGridColumnBag.BondAmountCalcEditColumn);
			builder.AddColumn(euGridColumnBag.BondFiledPortCodeFindBoxColumn);
			builder.AddColumn(euGridColumnBag.SuretyCodeDropEditColumn);
			return builder.Build();
		}

		#endregion
	}
}
