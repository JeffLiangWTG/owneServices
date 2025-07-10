using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public sealed class AdditionalInfoDetailsLayout : IPanelLayoutProvider
	{
		static PanelLayout CreateLayout()
		{
			var builder = new AdditionalInfoDetailsLayoutBuilder<AdditionalInfo>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.AddInfoTypeCodeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.AddInfoDescriptionTextBox, ControlWidthClass.Long);

			return builder.Build();
		}

		public PanelLayout Layout => CreateLayout();
	}
}
