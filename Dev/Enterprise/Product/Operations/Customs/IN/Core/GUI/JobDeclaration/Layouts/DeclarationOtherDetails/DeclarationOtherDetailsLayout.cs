using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class DeclarationOtherDetailsLayout : IPanelLayoutProvider
{
	PanelLayout DeclarationOtherDetails { get; }

	PanelLayout IPanelLayoutProvider.Layout => DeclarationOtherDetails;

	public DeclarationOtherDetailsLayout()
	{
		DeclarationOtherDetails = CreateDeclarationOtherDetailsLayout();
	}

	PanelLayout CreateDeclarationOtherDetailsLayout()
	{
		var builder = new DeclarationOtherDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.IECCodeTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OriginStateDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ExporterClassTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.EPZCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.BranchSerialNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.AuthorizedDealerCodeTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.TypeOfExporterDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.SealByDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.RotationNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.RotationDateDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.StuffingAtDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.SampleAccompaniedDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.GoodsRegistrationSeparatorUserControl, ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.TranshipperGuidFindBox, ControlWidthClass.Auto);

		builder.SetVisibility(commonBag.OriginStateDropEdit, h => h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.ExporterClassTextBox, h => h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.IECCodeTextBox, h => h.IsImport || h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.EPZCodeDropEdit, h => h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.BranchSerialNumberTextBox, h => h.IsImport || h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.AuthorizedDealerCodeTextBox, h => h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.TypeOfExporterDropEdit, h => h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.SealByDropEdit, h => h.IsExport && !h.IsAir, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo);
		builder.SetVisibility(commonBag.RotationNumberTextBox, h => h.IsExport && h.IsSea, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo);
		builder.SetVisibility(commonBag.RotationDateDateEdit, h => h.IsExport && h.IsSea, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo);
		builder.SetVisibility(commonBag.StuffingAtDropEdit, h => h.IsExport && h.IsSea && h.IsContainerised, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo, h => h.JE_ContainerModeInfo);
		builder.SetVisibility(commonBag.SampleAccompaniedDropEdit, h => h.IsExport && h.IsSea && h.IsContainerised && h.IsFactoryStuffed, h => h.JE_MessageTypeInfo, h => h.JE_TransportModeInfo, h => h.JE_ContainerModeInfo, h => h.JE_StuffingAtInfo);
		builder.SetVisibility(commonBag.GoodsRegistrationSeparatorUserControl, h => h.IsExport, h => h.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.TranshipperGuidFindBox, h => h.IsExport, h => h.JE_MessageTypeInfo);

		return builder.Build();
	}
}
