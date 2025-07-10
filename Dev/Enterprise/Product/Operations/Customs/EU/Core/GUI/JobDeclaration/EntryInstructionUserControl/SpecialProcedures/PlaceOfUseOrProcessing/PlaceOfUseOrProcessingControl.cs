using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class PlaceOfUseOrProcessingControl : ZUserControl, IExtendedControl
	{
		public PlaceOfUseOrProcessingControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		void MoreButton_Click(object sender, EventArgs e)
		{
			ClickOnMoreButtonCore();
		}

		protected PlaceOfUseOrProcessingForm GetPlaceOfUseOrProcessingForm(PlaceOfUseOrProcessing provider) => new PlaceOfUseOrProcessingForm(provider);

		protected virtual void ClickOnMoreButtonCore()
		{
			if (CurrentDataItem is IFirstPlaceOfUseOrProcessingProvider provider)
			{
				if (provider.FirstPlaceOfUseOrProcessing != null)
				{
					using (var form = GetPlaceOfUseOrProcessingForm(provider.FirstPlaceOfUseOrProcessing))
					{
						if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
						{
							provider.FirstPlaceOfUseOrProcessingDescriptionInfo.RefreshBinding();
						}
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("314AFF40-5917-4477-A122-AB94E32EC7E5", "The Place of Use or Processing is currently being edited by another user. Please try later."));
				}
			}
		}

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
