using System;
using System.Net;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class AIRSWebPageNaviagtorForm : ZChildForm
	{
		public AIRSWebPageNaviagtorForm(AIRSWebpageNavigator navigator)
			: base(navigator)
		{
		}

		AIRSWebpageNavigator AIRSNavigator
		{
			get { return (AIRSWebpageNavigator)base.DataSource; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			this.Text = Res.GetString("CF872057-6AA7-470E-9734-03F8B7E98E35", "Automated Import Reference System (AIRS) Search : ") + AIRSNavigator.TariffCodePassedIn;

			var configuration = AIRSNavigator.WebPageConfiguration;
			if (!configuration.IsValid)
			{
				WebBrowser.DocumentText = AIRSHtmlHelper.WriteMessageErrorHtml(AIRSHtmlHelper.Constant.FailedLoadConfigurationErrorMessage);
			}
			else
			{
				try
				{
					WebBrowser.Navigate(configuration.Url, ZString.Empty, AIRSHtmlHelper.GetPostData(configuration, AIRSNavigator.TariffCodePassedIn, AIRSHtmlHelper.GetInitialRequest), AIRSHtmlHelper.Constant.PostContentType);

					WebBrowser.Navigated += delegate
					{
						WebBrowser.Visible = true;
						LoadingLabel.Visible = false;
						WebBrowser.DocumentCompleted -= WebBrowser_DocumentCompleted;
						WebBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
						WebBrowser_DocumentCompleted(null, null);
					};
				}
				catch (WebException ex)
				{
					WebBrowser.DocumentText = AIRSHtmlHelper.WriteMessageErrorHtml(ex.Message);
				}
			}
		}

		void WebBrowser_DocumentCompleted(object sender, EventArgs e)
		{
			var document = WebBrowser.Document;
			ZString title = document?.Title ?? ZString.Empty;
			if (title.EqualsIgnoringCase(AIRSNavigator.WebPageConfiguration.IIDWebPageTitle))
			{
				CopyValueButton.Visible = true;
				AIRSSelectionLoader.InitializeAIRSNavigator(document, AIRSNavigator);
			}
			else
			{
				CopyValueButton.Visible = false;
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}

		void CopyValueButton_Click(object sender, EventArgs e)
		{
			using (var selectionForm = new AIRSSelectionForm(AIRSNavigator))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(selectionForm) == DialogResult.OK)
				{
					DialogResult = System.Windows.Forms.DialogResult.OK;
					Close();
				}
			}
		}
	}
}
