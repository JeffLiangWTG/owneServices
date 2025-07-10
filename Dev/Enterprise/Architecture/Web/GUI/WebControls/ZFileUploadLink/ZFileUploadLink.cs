using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	[DefaultProperty("Text"), ToolboxData("<{0}:ZFileUploadLink runat=server></{0}:ZFileUploadLink>")]
	public class ZFileUploadLink : ZButtonPopup
	{
		#region Constructors

		public ZFileUploadLink()
			: base()
		{
		}

		#endregion

		#region Properties

		[DefaultValue("400px")]
		public string DialogWidth { get; set; }

		[DefaultValue("125px")]
		public string DialogHeight { get; set; }

		#endregion

		#region New

		public new ZPage Page
		{
			get
			{
				return base.Page;
			}
		}

		#endregion

		#region Scripts

		const string AttachFileScriptKey = "AttachFileScript";

		void RenderAttachFileScript()
		{
			AddAttachFileScript();
		}

		protected virtual void AddAttachFileScript()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), AttachFileScriptKey))
			{
				string script = @"<script type='text/jscript'>        

                                function AttacheNewFileIsComplete()
                                {
                                    ZTextPopup_HidePopup();
                                    __doPostBack('eDocsAddNewLink$TextBox','');
                                }                            

                                 </script>";
				Page.ZClientScript.RegisterClientScriptBlock(GetType(), AttachFileScriptKey, script);
			}
		}

		#endregion

		#region Overrides

		protected override string CallerPK
		{
			get
			{
				return string.IsNullOrEmpty(fCallerPK) ? base.CallerPK : fCallerPK;
			}
		}
		string fCallerPK;

		public void SetDataSource(object dataSource)
		{
			if (dataSource != null && dataSource is IAdditionalSource)
			{
				fCallerPK = ((IAdditionalSource)dataSource).Key.ToString();
				if (Page != null && Page.Session != null)
				{
					Page.Session[CallerPK] = dataSource;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected override string ButtonClickHandler
		{
			get
			{
				EnsureChildControls();

				string formatString = "ZTextPopup_ShowIFramePopup('{0}', '{1}', '{2}', '{3}', true);{4}";

				return string.Format(formatString,
					TextBoxControl.ClientID,			// 0
					ButtonControl.ClientID,				// 1
					PopupID,							// 2
					IFrameSourceString,					// 3
					AdditionalButtonClickHandler);		// 4
			}
		}

		protected override string ButtonToolTip
		{
			get { return ""; }
		}

		protected override string ButtonTextCore
		{
			get { return Res.GetString("3767f395-b7c2-481f-981e-c49e04e579e9", "Add File"); }
		}

		protected override int ButtonControlWidth
		{
			get { return 150; }
		}

		protected override string IFrameSourcePageName
		{
			get { return ""; }
		}

		protected override Unit PopupWidth
		{
			get { return new Unit(400); }
		}

		protected override Unit PopupHeight
		{
			get { return new Unit(250); }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			RenderAttachFileScript();
		}

		#endregion

		#region Internal Properties

		internal Unit PopupWidthInternal => PopupWidth;
		internal Unit PopupHeightInternal => PopupHeight;

		#endregion
	}
}
