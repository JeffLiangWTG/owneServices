using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class EntryInstructionDetailsBasicUserControlLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ??= CreateEntryInstructionDetailsBasicUserControlLayout();
	PanelLayout layout;

	static PanelLayout CreateEntryInstructionDetailsBasicUserControlLayout()
	{
		var builder = new EU.GUI.EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
		var commonBag = builder.CommonBag;

		var esBag = EntryInstructionDetailsBasicUserControlBag.Instance;
		var euBag = EU.GUI.EntryInstructionBasicDetailsControlBag.Instance;

		builder.AddControlBag(esBag);
		builder.AddControlBag(euBag);

		builder.AddColumn();

		builder.Add(esBag.ActivateByOperatorCheckBox, widthClass: ControlWidthClass.Long);
		builder.Add(esBag.IncludeRoutingSecurityDataCheckBox, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.OtherPartiesSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ToWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ToWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.FromWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.FromWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(esBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(esBag.NewOwnerOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(esBag.RequestLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(esBag.NumberOfDaysCalcEdit, widthClass: ControlWidthClass.Medium);
		builder.Add(esBag.JustificationTextBox, widthClass: ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(esBag.LocationOfGoodsUserControl, widthClass: ControlWidthClass.Long);
		builder.Add(esBag.RemoverOrganisationControl, alignToControl: esBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.Long);
		builder.Add(esBag.RequestTypeDropEdit, alignToControl: esBag.NumberOfDaysCalcEdit, widthClass: ControlWidthClass.Long);
		builder.Add(esBag.NationalCheckBox, alignToControl: esBag.JustificationTextBox, widthClass: ControlWidthClass.Long);
		builder.Add(esBag.IndirectTypeDropEdit, alignToControl: esBag.JustificationTextBox, widthClass: ControlWidthClass.Long);

		builder.SetVisibility(esBag.ActivateByOperatorCheckBox, i => !i.IsH2 && !i.IsT2C && !i.IsT2L && i.JobDeclaration.IsUCC6AndIsImport, i => i.CEI_StyleInfo, i => i.CEI_SubStyleInfo, i => i.JobDeclaration.JE_MessageTypeInfo);
		builder.SetVisibility(esBag.IncludeRoutingSecurityDataCheckBox, i => i.JobDeclaration.IsExport || (i.JobDeclaration.IsImport && !i.JobDeclaration.IsUCC6), i => i.JobDeclaration.JE_MessageTypeInfo);
		builder.SetVisibility(esBag.RequestLabel, i => i.IsT2L, i => i.CEI_SubStyleInfo);
		builder.SetVisibility(esBag.NumberOfDaysCalcEdit, i => i.IsT2L, i => i.CEI_SubStyleInfo);
		builder.SetVisibility(esBag.JustificationTextBox, i => i.IsT2L, i => i.CEI_SubStyleInfo);
		builder.SetVisibility(esBag.RequestTypeDropEdit, i => i.IsT2L, i => i.CEI_SubStyleInfo);
		builder.SetVisibility(esBag.NationalCheckBox, i => i.IsT2L && i.JobDeclaration.IsExport, i => i.CEI_SubStyleInfo, i => i.JobDeclaration.JE_MessageTypeInfo);
		builder.SetVisibility(esBag.IndirectTypeDropEdit, i => i.IsT2L && i.JobDeclaration.IsImport, i => i.CEI_SubStyleInfo, i => i.JobDeclaration.JE_MessageTypeInfo);

		return builder.Build();
	}
}
