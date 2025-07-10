using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CusGoodsLocationForm : ZChildForm, IFindBoxPopup
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		CusGoodsLocationForm()
		{
		}

		public CusGoodsLocationForm(ICusGoodsLocationProvider provider) : base(Argument.NotNull(provider, nameof(provider)).GoodsLocation)
		{
			Provider = provider;
			UpdateDynamicGoodsLocationPanelLayout();
			UpdateAuthorizationCodeFindBoxCharacterCasing();
		}

		void UpdateAuthorizationCodeFindBoxCharacterCasing()
		{
			var authorizationCodeFindBox = (ZCodeFindBox)this.Controls.Find("AuthorizationCodeFindBox", true).FirstOrDefault();
			if (authorizationCodeFindBox != null)
			{
				var mixedCaseProvider = Provider as ICusGoodsLocationProviderWhichAllowsMixedCase;
				if (mixedCaseProvider != null)
				{
					authorizationCodeFindBox.CodeBox.CharacterCasing = mixedCaseProvider.AllowMixedCaseAuthorisationNumbers ? System.Windows.Forms.CharacterCasing.Normal : authorizationCodeFindBox.CodeBox.CharacterCasing;
				}
			}
		}

		public ICusGoodsLocationProvider Provider { get; }

		void UpdateDynamicGoodsLocationPanelLayout()
		{
			var layout = GetGoodsLocationLayout();
			if (layout != null)
			{
				DynamicGoodsLocationPanel.UpdateLayout(layout);
			}
		}

		IPanelLayoutProvider GetGoodsLocationLayout() => GoodsLocationFormLayoutProvider.GetGoodsLocationLayout();

		void OKButton_Click(object sender, EventArgs e)
		{
			Provider.ValidateGoodsLocationDescription();
			Provider.GoodsLocationDescriptionInfo?.RefreshBinding();
			if (Provider.GoodsLocation.HasErrors)
			{
				Globals.Message.ShowInformation(Res.GetString("7AFC30D5-46C5-4FC2-86D7-87D9A034922C", "Please resolve all errors before saving."));
			}
			else
			{
				Close();
			}
		}

		public void ShowModal(IFindBox findBox, Form parentForm) => ZFormModaliser.Show(this, parentForm);

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup) => SilentSelectResult.None;

		public void SelectRowByPK(ZGuid pK)
		{
		}

		protected IGoodsLocationFormLayoutProvider GoodsLocationFormLayoutProvider
		{
			get
			{
				if (goodsLocationFormLayoutProvider is null)
				{
					goodsLocationFormLayoutProvider = GUI.GoodsLocationFormLayoutProvider.GetLayoutProvider(Provider);
				}
				return goodsLocationFormLayoutProvider;
			}
		}

		IGoodsLocationFormLayoutProvider goodsLocationFormLayoutProvider;
	}
}
