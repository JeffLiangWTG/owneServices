using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class EntryInstructionGuaranteeGridColumnsLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
	IGridColumnLayout layout;

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var euColumnsBag = EntryInstructionGuaranteeGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euColumnsBag.BondTypeDropEditColumn);
		builder.AddColumn(euColumnsBag.BondNumberMultiControlColumn);

		builder.AddColumn<ZMultiControlColumnStyleInfo>(CusBondDetail.Schema.PW_BondNumber2, 120, c =>
		{
			c.BindToDecimalPlaces = null;
			c.FieldTypeColumnName = nameof(CommonGuarantee.ReferenceNumberFieldType);
			c.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Guarantees;
		});

		builder.AddColumn(euColumnsBag.PasswordBoxColumn);
		builder.AddColumn(euColumnsBag.HolderIdentificationTextBoxColumn);
		builder.AddColumn(euColumnsBag.NkCurrencyCodeFindBoxColumn);
		builder.AddColumn(euColumnsBag.BondAmountCalcEditColumn);
		builder.AddColumn(euColumnsBag.BondFiledPortCodeFindBoxColumn);
		builder.AddColumn(euColumnsBag.SuretyCodeDropEditColumn);

		return builder.Build();
	}

	#endregion
}
