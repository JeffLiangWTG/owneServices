using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class RelatedDocumentsDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout => CreateLayout();

		PanelLayout CreateLayout()
		{
			var builder = new RelatedDocumentsDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;
			builder.AddColumn();
			builder.Add(commonBag.FullTypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);

			builder.SetCaption(commonBag.FullTypeCodeFindBox, _ => ResourceStringData.Empty, _ => null);
			builder.SetCaption(commonBag.ReferenceTextBox, _ => ResourceStringData.Empty, _ => null);
			builder.SetCaption(commonBag.DescriptionTextBox, _ => ResourceStringData.Empty, _ => null);

			return builder.Build();
		}
	}
}
