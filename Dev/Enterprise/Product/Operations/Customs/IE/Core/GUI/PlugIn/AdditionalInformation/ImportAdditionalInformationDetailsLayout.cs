using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class ImportAdditionalInformationDetailsLayout : IPanelLayoutProvider
	{
		public ImportAdditionalInformationDetailsLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateLayout()
		{
			var builder = new AdditionalInformationDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;
			builder.AddColumn();
			builder.Add(commonBag.KindDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.FullTypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);

			builder.SetCaption(commonBag.KindDropEdit, _ => ResourceStringData.Empty, _ => null);
			builder.SetCaption(commonBag.FullTypeCodeFindBox, _ => ResourceStringData.Empty, _ => null);
			builder.SetCaption(commonBag.ReferenceTextBox, _ => ResourceStringData.Empty, _ => null);
			builder.SetCaption(commonBag.DescriptionTextBox, _ => ResourceStringData.Empty, _ => null);

			return builder.Build();
		}
	}
}
