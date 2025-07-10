using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.GUI
{
	public partial class PlaceOfUseOrProcessingForm : ZChildForm, IFindBoxPopup
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		PlaceOfUseOrProcessingForm()
		{
		}

		public PlaceOfUseOrProcessingForm(PlaceOfUseOrProcessing provider) : base(Argument.NotNull(provider, nameof(provider)))
		{
			Provider = provider;
			UpdateDynamicGoodsLocationPanelLayout();
		}

		public PlaceOfUseOrProcessing Provider { get; }

		void UpdateDynamicGoodsLocationPanelLayout()
		{
			var layout = GetPlaceOfUseOrProcessingLayout();
			if (layout != null)
			{
				DynamicPlaceOfUseOrProcessingPanel.UpdateLayout(layout);
			}
		}

		IPanelLayoutProvider GetPlaceOfUseOrProcessingLayout() => new PlaceOfUseOrProcessingLayout();

		void OKButton_Click(object sender, EventArgs e)
		{
			Provider.Validation.ValidateAll();
			if (Provider.HasErrors)
			{
				Globals.Message.ShowInformation(Res.GetString("6DDD9C13-6203-47A4-8118-7D3CB61753E2", "Please resolve all errors before saving."));
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		public void ShowModal(IFindBox findBox, Form parentForm) => ZFormModaliser.Show(this, parentForm);

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup) => SilentSelectResult.None;

		public void SelectRowByPK(ZGuid pK)
		{
		}
	}
}
