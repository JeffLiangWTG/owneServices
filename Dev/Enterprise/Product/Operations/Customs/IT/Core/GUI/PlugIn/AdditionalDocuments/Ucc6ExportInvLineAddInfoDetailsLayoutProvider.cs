using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class Ucc6ExportInvLineAddInfoDetailsLayoutProvider : IPanelLayoutProvider
{
	#region IPanelLayoutProvider

	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	PanelLayout layout;

	#endregion

	PanelLayout CreateLayout()
	{
		var builder = new AdditionalInformationDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.KindDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.FullTypeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
