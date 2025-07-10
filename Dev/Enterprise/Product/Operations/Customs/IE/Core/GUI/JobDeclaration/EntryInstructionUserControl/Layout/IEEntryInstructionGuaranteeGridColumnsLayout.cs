using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.IE.GUI
{
	public class IEEntryInstructionGuaranteeGridColumnsLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euColumnsBag = EntryInstructionGuaranteeGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			var guaranteeMultiControlColumn = new GridColumnReference<ZGuidFindBoxColumnStyleInfo>(nameof(GuaranteeForEntryInstruction.PW_CPH_Guarantee), 120, (ZGuidFindBoxColumnStyleInfo c) =>
			{
				c.ModuleID = ModuleIDs.Customs.Guarantees;
			});
			builder.AddColumn(guaranteeMultiControlColumn);
			builder.AddColumn(euColumnsBag.BondTypeDropEditColumn);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoCusBondDetail.Schema.PW_BondNumber, 120);
			builder.AddColumn<ZTextBoxColumnStyleInfo>(AutoCusBondDetail.Schema.PW_GuaranteeDescription, 120);
			builder.AddColumn(euColumnsBag.PasswordBoxColumn);
			var bondAmountCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(nameof(GuaranteeForEntryInstruction.PW_BondAmount), 80, (ZCalcEditColumnStyleInfo c) =>
			{
				c.BindToDecimalPlaces = nameof(GuaranteeForEntryInstruction.GuaranteeBondAmountDecimalPlaces);
				c.Decimals = 2;
				c.MaxValue = 999999999;
			});

			builder.AddColumn(bondAmountCalcEditColumn);
			builder.AddColumn<ZDropEditColumnStyleInfo>(nameof(GuaranteeForEntryInstruction.PW_RX_NKCurrency), 80);
			builder.AddColumn(euColumnsBag.BondFiledPortCodeFindBoxColumn);
			if (EUCustomsDataRegistry.Instance.EnableCentralizedClearanceForImport.Value)
			{
				builder.AddColumn<ZDropEditColumnStyleInfo>(AutoCusBondDetail.Schema.PW_RN_NKCountryOfIssue, 120);
			}

			return builder.Build();
		}

		#endregion
	}
}

