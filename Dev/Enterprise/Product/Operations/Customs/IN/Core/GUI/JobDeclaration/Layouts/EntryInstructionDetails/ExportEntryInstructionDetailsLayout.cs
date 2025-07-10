using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class ExportEntryInstructionDetailsLayout : IPanelLayoutProvider
{
	#region IPanelLayoutProvider

	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	PanelLayout layout;

	#endregion

	PanelLayout CreateLayout()
	{
		var builder = new EntryInstructionsDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var inBag = builder.INBag;
		builder.AddControlBag(inBag);

		builder.AddColumn();
		builder.Add(commonBag.StyleDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.SubStyleDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(inBag.PackagesQtyCalcDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(inBag.LoosePackagesCalcDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(inBag.TotalContainerZIntEdit, widthClass: ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(inBag.TotalGrossWeightAndNetWeightGroupBox, widthClass: ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(inBag.ShippingBillOverrideUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(inBag.RBIWaiverNumberTextBox, widthClass: ControlWidthClass.Long);
		builder.Add(inBag.RBIWaiverDateEdit, widthClass: ControlWidthClass.Auto);

		builder.SetVisibility(inBag.LoosePackagesCalcDropEdit, h => h.JobDeclaration?.IsSeaAndContainerised ?? false, h => h.JobDeclaration?.JE_TransportModeInfo, h => h.JobDeclaration?.JE_ContainerModeInfo);
		builder.SetVisibility(inBag.TotalContainerZIntEdit, h => h.JobDeclaration?.IsSeaAndContainerised ?? false, h => h.JobDeclaration?.JE_TransportModeInfo, h => h.JobDeclaration?.JE_ContainerModeInfo);

		return builder.Build();
	}
}
