using System.Windows.Forms.VisualStyles;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI;

public sealed class EntryInstructionDetailsBasicUserControlLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ??= CreateEntryInstructionDetailsBasicUserControlLayout();
	PanelLayout layout;

	static PanelLayout CreateEntryInstructionDetailsBasicUserControlLayout()
	{
		var builder = new EU.GUI.EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
		var commonBag = builder.CommonBag;
		var euBag = builder.EUBag;

		builder.AddControlBag(euBag);
		builder.AddColumn();
		builder.Add(commonBag.StyleDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.OtherPartiesSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ToWarehouseLabel, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ToWarehouseAddressControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.FromWarehouseLabel, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.FromWarehouseAddressControl, ControlWidthClass.LongNoCaption);
		builder.Add(euBag.NewOwnerOrganisationControl, ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(commonBag.SubStyleDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.ToWarehouseTypeTextBox, ControlWidthClass.Medium, euBag.ToWarehouseAddressControl);
		builder.Add(euBag.FromWarehouseTypeTextBox, ControlWidthClass.Medium, euBag.FromWarehouseAddressControl);
		builder.Add(euBag.AcceptanceDateEdit, ControlWidthClass.Medium, euBag.NewOwnerOrganisationControl, VerticalAlignment.Bottom);

		builder.AddColumn();
		builder.Add(euBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
		builder.Add(euBag.ToWarehouseCodeTextBox, ControlWidthClass.Medium, euBag.ToWarehouseAddressControl);
		builder.Add(euBag.FromWarehouseCodeTextBox, ControlWidthClass.Medium, euBag.FromWarehouseAddressControl);

		builder.SetVisibility(euBag.LocationOfGoodsUserControl, x => x.IsImport, x => x.JobDeclaration.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.NewOwnerOrganisationControl, x => x.IsImport, x => x.JobDeclaration.JE_MessageTypeInfo);
		builder.SetVisibility(euBag.AcceptanceDateEdit, x => x.IsImport, x => x.JobDeclaration.JE_MessageTypeInfo);

		builder.SetCaption(
			euBag.ToWarehouseLabel,
			instruction =>
				instruction.JobDeclaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport
				? Res.GetData("3474FAC7-4BED-4239-80A3-2E3C91F73A93", "[2/7] To Warehouse")
				: Res.GetData("A64DDCDF-0229-4473-ABBC-051BC33CFB19", "To Warehouse"),
			x => x.JobDeclaration.JE_MessageTypeInfo
		);

		builder.SetCaption(
			euBag.FromWarehouseLabel,
			instruction =>
				instruction.JobDeclaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport
				? Res.GetData("35E57789-8716-4780-A6E1-13FE192F1370", "[2/7] From Warehouse")
				: Res.GetData("898A2DE3-176F-453D-94E7-3A1C6A6750F3", "From Warehouse"),
			x => x.JobDeclaration.JE_MessageTypeInfo
		);

		var layout = builder.Build();
		var columnWidthRuler = layout.CreateRuler(413);
		layout.Include(euBag.RequestedDocumentsGroupBox, columnWidthRuler);

		return layout;
	}
}
