using System;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public abstract class ZIFramePage : ZPage
	{
		public const string OKFunctionQuery = "OKFunction";
		public const string CancelFunctionQuery = "CancelFunction";
		public const string ControlIDQuery = "ControlID";
		public const string CallerPKQuery = "CallerPK";

		#region Properties

		#region Controls 

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected internal Button OKButton
		{
			get
			{
				if (fOKButton == null)
				{
					fOKButton = new Button();
					fOKButton.ID = "OK";
					fOKButton.Text = GetOKButtonCaption();
					fOKButton.Style[HtmlTextWriterStyle.Width] = GetOKButtonWidth();
					fOKButton.Style[HtmlTextWriterStyle.MarginRight] = "8px";
					fOKButton.Style[HtmlTextWriterStyle.MarginBottom] = "6px";
					fOKButton.Click += new EventHandler(OKButton_Click);
					fOKButton.Enabled = !DisableOKButton;
				}
				return fOKButton;
			}
		}
		Button fOKButton;

		protected virtual string GetOKButtonWidth()
		{
			return "100px";
		}

		protected virtual string GetOKButtonCaption()
		{
			return "OK";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected internal Button CancelButton
		{
			get
			{
				if (fCancelButton == null)
				{
					fCancelButton = new Button();
					fCancelButton.ID = "Cancel";
					fCancelButton.Text = "Cancel";
					fCancelButton.Style[HtmlTextWriterStyle.Width] = "100px";
					fCancelButton.Style[HtmlTextWriterStyle.MarginRight] = "8px";
					fCancelButton.Style[HtmlTextWriterStyle.MarginBottom] = "6px";
					fCancelButton.Click += new EventHandler(CancelButton_Click);
				}
				return fCancelButton;
			}
		}
		Button fCancelButton;

		protected Panel ButtonsContainer
		{
			get { return fButtonsContainer ?? (fButtonsContainer = GetButtonsContainer()); }
		}
		Panel fButtonsContainer;

		protected virtual Panel GetButtonsContainer()
		{
			Panel result = new Panel();
			result.HorizontalAlign = HorizontalAlign.Right;
			result.Style[HtmlTextWriterStyle.VerticalAlign] = nameof(VerticalAlign.Bottom);

			return result;
		}

		#endregion Controls 

		public IModuleFilterProvider ModuleFilterProvider
		{
			get
			{
				if (CallerPK.IsValid && !CallerPK.IsEmpty)
				{
					IModuleFilterProvider result = LoadDataSource(CallerPK) as IModuleFilterProvider;
					return result;
				}
				return null;
			}
		}

		protected ZGuid CallerPK => fCallerPK;
		ZGuid fCallerPK = ZGuid.Empty;

		protected abstract string[] OKFunctionArguments { get; }

		protected abstract string[] CancelFunctionArguments { get; }

		protected internal string OKFunctionName => fOKFunctionName;
		string fOKFunctionName;

		protected internal string CancelFunctionName => fCancelFunctionName;
		string fCancelFunctionName;

		protected internal string ParentControlID
		{
			get { return fParentControlID; }
		}
		string fParentControlID;

		protected override bool ShowFooter
		{
			get { return false; }
		}

		#endregion

		#region Control overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			CheckForRequiredQueries();
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			if (FormControl != null)
			{
				if (!DisableOKCancelButtons)
				{
					ButtonsContainer.Controls.Add(OKButton);
					ButtonsContainer.Controls.Add(CancelButton);
				}
				FormControl.Controls.Add(ButtonsContainer);
			}
		}

		protected virtual bool DisableOKCancelButtons
		{
			get { return false; }
		}

		protected virtual bool DisableOKButton => false;

		/// <summary>
		/// No login information should be displayed on pages in Pop ups
		/// </summary>
		protected override bool ShowLoginStatus
		{
			get { return false; }
		}

		#endregion

		#region Implementation

		protected override bool PageRequiresLogin(Uri url)
		{
			return !Globals.IsTest && (Page.Session == null || Page.Session.IsNewSession);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html code should not be localized")]
		protected override void ResolveUnAuthenticatedUser()
		{
			if (!Globals.IsTest)
			{
				Response.Write("<script language=\"javascript\">\nalert(\"");
				Response.Write(Res.GetString("18818291-c2e6-4222-afee-d1781e8320ed", "Your session has expired due to inactivity."));
				Response.Write("\");\nwindow.parent.location.replace(window.parent.location.href);\n</script>\n");
			}
		}

		protected void CheckForRequiredQueries()
		{
			CheckForRequiredQueriesInternal(RequestQueryString);
		}

		protected internal void CheckForRequiredQueriesInternal(NameValueCollection queryString)
		{
			if (queryString[OKFunctionQuery] == null || queryString[CancelFunctionQuery] == null || queryString[ControlIDQuery] == null || !ZGuid.TryParse(queryString[CallerPKQuery], out fCallerPK))
			{
				HttpContext.Current.Response.Redirect($"{AppInstance.ErrorPage}?invalidQuery=true"); // redirect path
			}

			fOKFunctionName = WebUtility.HtmlEncode(queryString[OKFunctionQuery]);
			fCancelFunctionName = WebUtility.HtmlEncode(queryString[CancelFunctionQuery]);
			fParentControlID = queryString[ControlIDQuery];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected internal void RegisterClientScriptCaller(string functionName, params string[] arguments)
		{
			RegisterClientScriptCaller("onload", functionName, arguments);
		}

		protected void RegisterClientScriptCaller(string hookupEvent, string functionName, params string[] arguments)
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), "ZIFramePage_ClientScriptCaller"))
			{
				string strArguments = "";
				if (arguments != null && arguments.Length > 0)
				{
					strArguments = arguments[0];
					for (int i = 1; i < arguments.Length; i++)
					{
						strArguments += ',' + arguments[i];
					}
				}

				string scriptString = string.Format("<SCRIPT TYPE=\"text/javascript\">if (typeof parent[\"{0}\"] === \"function\") parent[\"{0}\"]({1}) </SCRIPT>", functionName, strArguments); // May be an identifier or GUID.
				Page.ZClientScript.RegisterClientScriptBlock(GetType(), "ZIFramePage_ClientScriptCaller", scriptString);
			}
		}

		#endregion

		#region Event Handlers

		internal void OKButton_Click(object sender, EventArgs e)
		{
			HandleOkButtonClick();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected virtual void HandleOkButtonClick()
		{
			RegisterClientScriptCaller("onload", OKFunctionName, OKFunctionArguments);
		}

		protected virtual void CancelButton_Click(object sender, EventArgs e)
		{
			HandleCancelButtonClick();
		}

		protected virtual void HandleCancelButtonClick()
		{
			RegisterClientScriptCaller(CancelFunctionName, CancelFunctionArguments);
		}

		#endregion

		#region Internal Properties

		internal void EnsureChildControlsInternal() => EnsureChildControls();
		internal Panel ButtonsContainerInternal => ButtonsContainer;
		internal string[] OKFunctionArgumentsInternal => OKFunctionArguments;
		internal string[] CancelFunctionArgumentsInternal => CancelFunctionArguments;
		internal void CancelButton_Click_Internal(object sender, EventArgs e) => CancelButton_Click(sender, e);

		#endregion
	}
}
