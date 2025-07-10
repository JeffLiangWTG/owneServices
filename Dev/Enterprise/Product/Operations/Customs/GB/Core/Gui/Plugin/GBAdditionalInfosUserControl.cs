using System;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.GUI.Plugin
{
	public partial class GBAdditionalInfosUserControl : AdditionalInfosUserControl
	{
		public GBAdditionalInfosUserControl()
		{
			InitializeComponent();
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			WireHandlersFromBiz((JobDeclaration)JobDeclaration);
		}

		void Dec_OnAppCodeChanged(object sender, EventArgs e)
		{
			AdditionalInfosGroupBox.Text = ((JobDeclaration)sender)?.AdditionalInfoCaption ?? string.Empty;
		}

		void WireHandlersFromBiz(JobDeclaration header)
		{
			if (header != null)
			{
				header.OnApplicationCodeChanged -= Dec_OnAppCodeChanged;
				header.OnApplicationCodeChanged += Dec_OnAppCodeChanged;
				Dec_OnAppCodeChanged(header, null);
			}
		}
	}
}
