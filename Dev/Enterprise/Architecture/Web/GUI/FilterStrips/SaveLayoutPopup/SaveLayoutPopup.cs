using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public class SaveLayoutPopup : ZButtonPopup
	{
		//#warning move to base class and MAnage layouts reuses this too

		#region Constants

		public const string DataSourceIndexerQueryStringKey = "dataSourceIndexer";
		public static string DefaultButtonToolTip
		{
			get { return Res.GetString("cc2893ad-7f14-4e31-b56d-9d792ffd3b80", "Save my filter layout."); }
		}
		public static string SavingDisabledButtonToolTip
		{
			get { return Res.GetString("896a878c-dfb5-4e25-a001-b6679c642400", "You can't save layout, which doesn't contain at least one filter."); }
		}

		#endregion

		#region Properties

		protected override HtmlInputButton GetPopupButton()
		{
			var submitButton = new HtmlInputSubmit();
			submitButton.ServerClick += PopupButton_ServerClick;

			return submitButton;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "js string")]
		protected override string ButtonClickHandler => string.Format(CultureInfo.InvariantCulture, "javascript:__doPostBack('{0}', ''); return true;", ClientID);

		void PopupButton_ServerClick(object sender, EventArgs e)
		{
			ScriptManager.RegisterStartupScript(this, typeof(SaveLayoutPopup), ClientID, base.ButtonClickHandler, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "css style value should not be translated")]
		public WebControls.ZTextBox IsPublishedTextBoxControl
		{
			get
			{
				if (fIsPublishedTextBoxControl == null)
				{
					fIsPublishedTextBoxControl = new WebControls.ZTextBox();
					fIsPublishedTextBoxControl.ID = "IsPublishedTextBox";
					fIsPublishedTextBoxControl.Style[HtmlTextWriterStyle.Width] = TextBoxWidth;
					fIsPublishedTextBoxControl.ToolTip = this.ToolTip;
					fIsPublishedTextBoxControl.MaxLength = MaxLength;
					fIsPublishedTextBoxControl.Enabled = this.Enabled;
					fIsPublishedTextBoxControl.Style[HtmlTextWriterStyle.Display] = "none";
				}
				return fIsPublishedTextBoxControl;
			}
		}
		WebControls.ZTextBox fIsPublishedTextBoxControl;

		protected override string AdditionalButtonClickHandler
		{
			get
			{
				string formatString = "ZTextPopup_SetIsPublishedTextBoxID('{0}');"; // javascript code should not be translated
				return string.Format(formatString, IsPublishedTextBoxControl.ClientID);
			}
		}

		#endregion

		#region Overrides

		protected override void RenderPopupContainer(HtmlTextWriter writer)
		{
			Page.SaveDataSourceToSession();
			base.RenderPopupContainer(writer);
		}

		protected override NameValueCollection AdditionalParameters
		{
			get
			{
				NameValueCollection result = base.AdditionalParameters;
				result.Add(DataSourceIndexerQueryStringKey, Page.DataSourceIndexer.ToString());
				return result;
			}
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			Controls.Add(IsPublishedTextBoxControl);
		}

		#endregion

		#region Setup

		public override bool CanAutosizeSelf
		{
			get
			{
				return true;
			}
		}

		protected override string IFrameSourcePageName
		{
			get { return "SaveLayoutPage.aspx"; }
		}

		#endregion

		#region Dimensions

		protected override Unit PopupHeight
		{
			get { return 130; }
		}

		protected override Unit PopupWidth
		{
			get { return 494; }
		}

		protected override int ButtonControlWidth
		{
			get { return 170; }
		}

		protected override Unit ControlHeight
		{
			get { return ZFilterStripConstants.Controls.ButtonHeight; }
		}

		#endregion

		#region internal wrapper properties

		protected internal Unit InternalPopupHeight() => PopupHeight;

		protected internal Unit InternalPopupWidth() => PopupWidth;

		protected internal ZGuid InternalDataSourceIndexer() => Page.DataSourceIndexer;

		protected internal string InternalAdditionalButtonClickHandler() => AdditionalButtonClickHandler;

		protected internal void InternalEnsureChildControls() => EnsureChildControls();

		protected internal ZClientScriptManager InternalZClientScript() => Page.ZClientScript;

		protected internal string InternalButtonClickHandler() => ButtonClickHandler;

		#endregion

		#region Button Text / ToolTip

		protected override string ButtonTextCore
		{
			get { return Res.GetString("05c2dd7d-c694-4d52-b0a1-f6038e494965", "Save Layout"); }
		}

		protected override string ButtonToolTip
		{
			get { return DefaultButtonToolTip; }
		}

		#endregion

		#region Disabling Saving

#if DEBUG
		protected
#endif
 void DisableSaving()
		{
			ButtonControl.Disabled = !Enabled;
			ButtonControl.Attributes[nameof(HtmlTextWriterAttribute.Title)] = Enabled ? DefaultButtonToolTip : SavingDisabledButtonToolTip;
		}

		#endregion

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisableSaving();
		}
	}
}
