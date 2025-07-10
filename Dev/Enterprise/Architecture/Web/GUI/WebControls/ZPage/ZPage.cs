using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Design;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.ServerServices;
using Enterprise.ZArchitecture.Web.Utilities;

#if DEBUG
using Enterprise.ZArchitecture.Web.GUI.Testing;
#endif

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region BrowserType Enum

	public enum BrowserType
	{
		Unknown,
		IE,
		Mozilla
	}

	#endregion BrowserType Enum

	public class ZPage : Page, IDesignTimeDataSourceType, IContainResources
	{
		public ZPage()
		{
			topLevelDataSourceTypeHelper = new TopLevelDataSourceTypeHelper(this);
			translationFeedbackManager = new TranslationFeedbackManager();
			UserActivityLogger.AddNewLog();
		}

		protected override void InitializeCulture()
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (!IsErrorPage() && !LanguageSet)
				{
					var language = Session["Language"] as string;
					var languageCookie = Request.Cookies["Language"];
					if (languageCookie != null && !string.IsNullOrEmpty(languageCookie.Value) && languageCookie.Value != language)
					{
						if (Env.CurrentUser == null)
						{
							ReportSessionLostError();
							return;
						}
						WebAppEnvironment.SetupLanguage();
						language = Session["Language"] as string;
					}
					if (!string.IsNullOrEmpty(language))
					{
						ObjectFactory.Get<IResourceStrings>().CurrentLanguage = language;
					}
					translationFeedbackManager.Initialize(this);
				}

				base.InitializeCulture();
			}
		}

		protected bool LanguageSet;

		protected bool IsErrorPage()
		{
			return RequestUrl.LocalPath.EndsWith("Error.aspx");
		}

		bool IsLoginPage()
		{
			return RequestUrl.LocalPath.EndsWith(AppInstance.LoginPage);
		}

		public const string DataSourceInSessionParameterName = "DIS";
		public const string DataSourceInSessionParameterValue = "Y";

		#region Headers

		protected virtual LoginStatus GetLoginStatus()
		{
			if (FormControl != null)
			{
				foreach (Control childControl in FormControl.Controls)
				{
					LoginStatus loginStatus = childControl as LoginStatus;
					if (loginStatus != null)
					{
						return loginStatus;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Adds common functionality to all pages in all projects:
		/// - Header control
		/// - Validation summary label
		/// - Validation DIV, where all error providers are being added to
		/// </summary>
		protected internal void SetHeadersAndStyle()
		{
			if (FormControl != null)
			{
				if (ShowLoginStatus && SiteUser != null && SiteUser.IsLoggedIn)
				{
					LoginStatus loginStatus = (LoginStatus)Page.LoadControl(LoginStatusControl.FileName);
					loginStatus.ShowLogOffLinkButton = ShowLogOffLinkButton;
					loginStatus.ShowChangePasswordLinkButton = ShowChangePasswordLinkButton;
					FormControl.Controls.AddAt(0, loginStatus);
				}

				LoadPageHeader();

				AddNotificationsArea();
			}
		}

		protected void SetFooter()
		{
			if (FormControl != null && ShowFooter)
			{
				FormControl.Controls.Add(Footer);
			}
		}

		protected virtual void AddNotificationsArea()
		{
			FormControl.Controls.AddAt(0, NotificationsArea);
		}

		protected virtual void LoadPageHeader()
		{
			if (!string.IsNullOrEmpty(PageHeaderControlPath))
			{
				FormControl.Controls.AddAt(0, Page.LoadControl(AppInstance.ApplicationRoot + PageHeaderControlPath));
			}
		}

		protected virtual bool ShowLoginStatus
		{
			get { return true; }
		}

		protected virtual bool ShowLogOffLinkButton
		{
			get { return true; }
		}

		protected virtual bool ShowChangePasswordLinkButton
		{
			get { return true; }
		}

		protected virtual string PageHeaderControlPath
		{
			get { return ""; }
		}

		protected internal HtmlGenericControl NotificationsArea
		{
			get
			{
				if (notificationsArea == null)
				{
					notificationsArea = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
					notificationsArea.ID = "ValidationControls";
				}
				return notificationsArea;
			}
		}
		HtmlGenericControl notificationsArea;

		#endregion Headers

		#region IDesignTimeDataSourceType

		internal TopLevelDataSourceTypeHelper topLevelDataSourceTypeHelper;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string DataSourceAssemblyName
		{
			get { return topLevelDataSourceTypeHelper.DataSourceAssemblyName; }
			set { topLevelDataSourceTypeHelper.DataSourceAssemblyName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string DataSourceTypeName
		{
			get { return topLevelDataSourceTypeHelper.DataSourceTypeName; }
			set { topLevelDataSourceTypeHelper.DataSourceTypeName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Type DataSourceType
		{
			get { return topLevelDataSourceTypeHelper.DataSourceType; }
		}

		#endregion

		#region Properties

		#region ExternalServicesSupported

		public bool ExternalServicesSupported
		{
			get { return GetExternalServicesSupported(); }
		}

		protected virtual bool GetExternalServicesSupported()
		{
			return false;
		}

		#endregion

		public int NextUniqueID
		{
			get
			{
				fNextUniqueID++;
				return fNextUniqueID;
			}
		}
		int fNextUniqueID;

		#region DataSource

		/// <summary>
		/// Needs to be renamed into DataSource
		/// Used as a business object to validate and bind to
		/// </summary>
		public BusinessObject DataSource
		{
			get { return fDataSource; }
		}
		internal BusinessObject fDataSource;

		/// <summary>
		/// ID of a DataSource business object saved on the session 
		/// between postbacks
		/// Managed by the view state (See LoadViewState and SaveViewState)
		/// </summary>
		public ZGuid DataSourceIndexer
		{
			get
			{
				return GetDataSourceIndexer();
			}
			protected set { fDataSourceIndexer = value; }
		}

		ZGuid fDataSourceIndexer;

		protected virtual ZGuid GetDataSourceIndexer()
		{
			if (fDataSourceIndexer.IsEmpty)
			{
				if (IsDataSourceInSession)
				{
					fDataSourceIndexer = GetGuidFromParameter(RefParameterName);
				}
				if (fDataSourceIndexer.IsEmpty || !fDataSourceIndexer.IsValid)
				{
					fDataSourceIndexer = ZGuid.NewZGuid();
				}
			}
			return fDataSourceIndexer;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Request parameter name")]
		public const string RefParameterName = "Ref";

		public string SaveButtonClientID
		{
			get
			{
				return GetSaveButtonClientID();
			}
		}

		protected virtual string GetSaveButtonClientID()
		{
			return "";
		}

		/// <summary>
		/// A method that provides a new DataSource for the page
		/// Load your business object or create a new one 
		/// </summary>
		/// <returns></returns>
		protected virtual BusinessObject GetNewDataSource()
		{
			return null;
		}

		public bool SaveDataSourceFactory()
		{
			return SaveDataSourceFactory(true);
		}

		protected internal bool SaveDataSourceFactory(bool bubbleException)
		{
			bool result = false;
			SaveRequest = true;
			HandleSaveConfirmations();
			if (BusinessObjectToValidate == null)
			{
				ReportNullDataSourceError();
				return false;
			}
			BusinessObjectToValidate.ClearRowNotifications();
			BusinessObjectToValidate.RunPreSaveValidation();
			NotificationFlags.DisplayErrors = true;

			ClearAllNotificationsForNewEmptyGridRows();
			if (!BusinessObjectHasNotifications)
			{
				if (!HasOutstandingRequiredConfirmations())
				{
					try
					{
						result = SaveDataSourceFactoryNoValidation(bubbleException, true);
					}
					catch (ZSaveException)
					{
						return false;
					}
				}
			}
			return result;
		}

		void ReportNullDataSourceError()
		{
			if (AppInstance != null)
			{
				var message = Res.GetString("67846f80-c8aa-4bc9-8df2-eba6c388350f", "The data source of current page has been lost, please try again.");
				var title = Res.GetString("344e58e4-9d2a-413e-991b-40643c26e2d2", "Missing data source");
				AppInstance.ReportError(title, message);
			}
		}

		void ReportSessionLostError()
		{
			if (AppInstance != null)
			{
				var message = Res.GetString("d3f0babd-8152-4d7d-8f5f-9be820ca035b", "The session of current page has been lost, please try again.");
				var title = Res.GetString("7856e09a-d6cc-4dcc-b3d9-48840dc6bb12", "Missing session");
				AppInstance.ReportError(title, message);
			}
		}

		protected bool BusinessObjectHasNotifications
		{
			get
			{
				return BusinessObjectToValidate.HasErrors || (NotificationFlags.DisplayMessageErrors && BusinessObjectToValidate.HasMessageErrors);
			}
		}

		protected bool SaveRequest;

		protected void HandleSaveConfirmations()
		{
			var confirmations = PageConfirmations.Where(c => c.IsSaveConfirmation && c.IsRequired && c.HasResponse);
			foreach (var confirmation in confirmations)
			{
				confirmation.HandleResponse();
			}
		}

		protected void HandlePostbackConfirmations()
		{
			var confirmations = PageConfirmations.Where(c => !c.IsSaveConfirmation && c.IsRequired && c.HasResponse);
			foreach (var confirmation in confirmations)
			{
				confirmation.HandleResponse();
			}
		}

		protected bool HasOutstandingRequiredConfirmations()
		{
			foreach (ZPageConfirmation confirmation in PageConfirmations)
			{
				if (confirmation.IsRequired && !confirmation.HasResponse)
				{
					return true;
				}
			}
			return false;
		}

		protected ZPageConfirmation GetConfirmationByType(Type confirmationType)
		{
			foreach (ZPageConfirmation confirmation in PageConfirmations)
			{
				if (confirmation.GetType().Equals(confirmationType))
				{
					return confirmation;
				}
			}
			return null;
		}

		protected bool SaveDataSourceFactoryNoValidation()
		{
			return SaveDataSourceFactoryNoValidation(true, false);
		}

		protected bool SaveDataSourceFactoryNoValidation(bool bubbleException, bool returnZSaveExceptionIfHasErrors)
		{
			bool result = true;
			RemoveNewEmptyGridRows();
			try
			{
				OnBeforeDataSourceFactorySaved();
				DataSource.Factory.Save();
				OnDataSourceFactorySaved();
			}
			catch (ZSaveException ex)
			{
				string saveErrorMessage = Res.GetString("fb6464ed-a684-49a0-a3dc-e10da75f1f2b", "There was a problem while saving your changes.  Please try saving again.\r\nError details:{0}", ex.FriendlyMessage);
				if (BusinessObjectHasNotifications && returnZSaveExceptionIfHasErrors)
				{
					throw;
				}
				try
				{
					var notifier = new ZWebNotificationHandler(this);
					ZExceptionReporting.HandleSaveException(ex, notifier);
					if (ex is ZSaveConcurrencyException)
					{
						saveErrorMessage = notifier.Message;
					}
				}
				catch (ZSaveConcurrencyException)
				{ }

				Page.Bind();
				BusinessObjectToValidate.AddRowError(saveErrorMessage);
				result = false;
			}
			catch (ZCannotSaveException ex)
			{
				var saveErrorMessage = Res.GetString("fb6464ed-a684-49a0-a3dc-e10da75f1f2b", "There was a problem while saving your changes.  Please try saving again.\r\nError details:{0}", ex.Message);
				if (BusinessObjectHasNotifications && returnZSaveExceptionIfHasErrors)
				{
					throw;
				}
				var notifier = new ZWebNotificationHandler(this);
				ZExceptionReporting.HandleSaveException(ex, notifier);
				saveErrorMessage = notifier.Message;

				Page.Bind();
				BusinessObjectToValidate.AddRowError(saveErrorMessage);
				result = false;
			}
			return result;
		}

		protected virtual void OnBeforeDataSourceFactorySaved()
		{
		}

		protected virtual void OnDataSourceFactorySaved()
		{
		}

		/// <summary>
		/// Loads data source of the page.
		/// Needs to be called only after the ViewState is loaded
		/// because DataSourceIndexer is stored on the ViewState
		/// NOT INTENDED TO BE CALLED ANYWHERE ELSE BUT THIS FILE IN LoadViewState METHOD!!!
		/// </summary>
		protected internal void LoadOrCreateDataSource()
		{
			if (fDataSource == null)
			{
				if ((IsPersistDataSourceBetweenPostbacks && IsPostBack) || IsDataSourceInSession)
				{
					fDataSource = LoadDataSource(DataSourceIndexer) as BusinessObject;
				}
				if (fDataSource == null)
				{
					fDataSource = GetNewDataSource();
					AddBusinessObjectChangesEmailNotifier(fDataSource);
				}
			}
		}

		#region Test
#if DEBUG
		public void ResetDataSource()
		{
			fDataSource = null;
		}
#endif
		#endregion

		protected virtual void AddBusinessObjectChangesEmailNotifier(BusinessObject bizO) { }

		#region IsSaveDataSource

		/// <summary>
		/// Save Datasource between postbacks on the Session
		/// Should be TRUE by default (can be set to FALSE on READ-ONLY pages). 
		/// 
		/// However, in order to be consistent with old functionality it's FALSE by default. 
		/// Should be changed to TRUE
		/// </summary>
		protected virtual bool IsPersistDataSourceBetweenPostbacks
		{
			get { return false; }
		}

		#endregion

		#endregion

		#region SiteUser

		public WebUser SiteUser
		{
			get { return (AppInstance == null) ? null : AppInstance.SiteUser; }
		}

		#endregion SiteUser 

		#region IsDesignMode

		bool IsDesignMode
		{
			get { return this.Site != null && this.Site.DesignMode; }
		}
		#endregion

		#region AppInstance

		public virtual ZGlobal AppInstance
		{
			get
			{
				if (fAppInstance == null)
				{
#if DEBUG
					if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
					{
						if (IsCreateNewAppInstanceIfNullForTest)
						{
							fAppInstance = GetNewTestGlobal();
						}
					}
					else
#endif
					{
						fAppInstance = (Context != null) ? (ZGlobal)Context.ApplicationInstance : null;
					}
				}
				return fAppInstance;
			}
		}
		ZGlobal fAppInstance;

#if DEBUG

		protected virtual ZGlobal GetNewTestGlobal()
		{
			return new ZTestGlobal();
		}

		bool fIsCreateNewAppInstanceIfNullForTest = true;
		public bool IsCreateNewAppInstanceIfNullForTest
		{
			get { return fIsCreateNewAppInstanceIfNullForTest; }
			set
			{
				fIsCreateNewAppInstanceIfNullForTest = value;
				//resetting AppInstance for testing purposes
				if (!value)
				{
					fAppInstance = null;
				}
			}
		}
#endif
		#endregion

		#region BrowserType

		public BrowserType BrowserType
		{
			get { return fBrowserType; }
		}
		internal BrowserType fBrowserType;
		#endregion

		#region FormControl
		protected internal HtmlForm FormControl
		{
			get
			{
				if (fFormControl == null)
				{
					fFormControl = GetForm(this);
				}

				return fFormControl;
			}
		}
		HtmlForm fFormControl;

		#endregion

		#region GetForm

#if DEBUG
		protected virtual
#endif
			HtmlForm GetForm(Control parent)
		{
			if (parent is HtmlForm)
			{
				return (HtmlForm)parent;
			}

			foreach (Control child in parent.Controls)
			{
				HtmlForm result = GetForm(child);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}
		#endregion

		#endregion

		#region Factory

		internal BusinessObjectFactory fFactory;
		public BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = (fDataSource == null || fDataSource.Factory == null) ? GetNewFactory() : fDataSource.Factory;
					if (!IsPersistDataSourceBetweenPostbacks && !Globals.IsTest)
					{
						fFactory.SuspendValidation();
					}
				}
				return fFactory;
			}
		}

		protected virtual BusinessObjectFactory GetNewFactory()
		{
			return new BusinessObjectFactory();
		}

		#endregion

		#region Notifications

		protected ZControlNotifications ControlNotifications
		{
			get
			{
				return new ZControlNotifications(this);
			}
		}

		public void RenderPageControlBeginTag(HtmlTextWriter writer, INotificationProvider control)
		{
			ControlNotifications.RenderBeginTag(writer, control);
		}

		public void RenderPageControlEndTag(HtmlTextWriter writer, INotificationProvider control)
		{
			ControlNotifications.RenderEndTag(writer, control);
		}

		public ZNotificationFlags NotificationFlags
		{
			get
			{
				if (fNotificationFlags == null)
				{
					ZNotificationFlags flags = ViewState["ZPage_ZNotificationFlags"] as ZNotificationFlags;
					if (flags == null)
					{
						flags = new ZNotificationFlags();
						flags.DisplayAll = true;
						ViewState["ZPage_ZNotificationFlags"] = flags;
					}
					fNotificationFlags = flags;
				}
				return fNotificationFlags;
			}
		}
		ZNotificationFlags fNotificationFlags;

#if DEBUG
		public new virtual bool IsPostBack
		{
			get { return Globals.IsTest ? fIsPostBackForTesting : base.IsPostBack; }
			set { fIsPostBackForTesting = value; }
		}
		bool fIsPostBackForTesting;
#endif

		protected internal HtmlGenericControl Footer
		{
			get
			{
				if (fFooter == null)
				{
					fFooter = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
					fFooter.ID = "PageFooter";
					var label = new ZTextLabel();
					label.EnableHtmlEncoding = false;
					label.Text = CopyrightText;
					fFooter.Controls.Add(label);
					if (!IsErrorPage())
					{
						fFooter.Controls.Add(new LanguageSelectionControl());
					}
				}
				return fFooter;
			}
		}
		HtmlGenericControl fFooter;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "We aren`t usisn ZDateTime.Now because this will cause database hit which isn't necessary because all we're after is the year. According to WI00006679.")]
		protected virtual string CopyrightText => string.Format(CultureInfo.InvariantCulture,
			"&copy; <a href=\"http://www.wisetechglobal.com\">WiseTech Global</a> {0}. All rights reserved. ",
			DateTime.Now.Year); // We aren`t usisn ZDateTime.Now because this will cause database hit which isn't necessary because all we're after is the year. According to WI00006679.

		protected virtual bool ShowFooter
		{
			get { return true; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		protected internal HtmlGenericControl NoScriptBlock
		{
			get
			{
				if (fNoScriptBlock == null)
				{
					fNoScriptBlock = new HtmlGenericControl(nameof(HtmlTextWriterTag.Noscript));
					Panel noScriptPanel = new Panel();
					noScriptPanel.Style.Add("z-index", "999");
					noScriptPanel.Style.Add("background", nameof(System.Drawing.KnownColor.LightYellow));
					noScriptPanel.Style.Add("border", "solid");
					noScriptPanel.Style.Add("border-width", "1px");
					noScriptPanel.Style.Add("border-color", "black");
					noScriptPanel.Style.Add("position", "absolute");
					noScriptPanel.Style.Add("top", "0px");
					noScriptPanel.Style.Add("left", "0px");
					var label = new ZTextLabel();
					label.EnableHtmlEncoding = false;
					label.Text = @"You do not have javascript enabled. This site requires javascript in order to operate correctly.<br>
Please enable javascript or add this site to the list of trusted sites.<p>
To enable javascript, do the following:<br>
<ol>
<li>Select the Tools Menu in Internet Explorer</li>
<li>Select <b>Internet Options</b></li>
<li>Select the <b>Security</b> tab</li>
<li>Click the <b>Custom Level</b> button</li>
<li>Scroll down to the <b>Scripting</b> heading</li>
<li>Select the <b>enable</b> option of <b>Active Scripting</b></li>
<li>Click <b>OK</b> in all open dialogs</li>
</ol>
For more detailed instructions regarding configuring trusted sites please <a href=http://www.microsoft.com/windows/ie/using/howto/security/settings.mspx>click here.</a>";
					noScriptPanel.Controls.Add(label);
					fNoScriptBlock.Controls.Add(noScriptPanel);
				}
				return fNoScriptBlock;
			}
		}
		HtmlGenericControl fNoScriptBlock;

		#endregion

		#region Page Overrides

		#region ViewState

		protected override void LoadViewState(object savedState)
		{
			object[] viewState = (object[])savedState;
			base.LoadViewState(viewState[0]);
			if (IsPersistDataSourceBetweenPostbacks)
			{
				fDataSourceIndexer = (ZGuid)viewState[1];
			}
		}

		protected override object SaveViewState()
		{
			object[] viewState = new object[2];
			viewState[0] = base.SaveViewState();
			if (IsPersistDataSourceBetweenPostbacks)
			{
				viewState[1] = fDataSourceIndexer = SaveDataSource(DataSourceIndexer, DataSource);
			}
			return (viewState);
		}

		#endregion

		protected new ZPage Page
		{
			get { return base.Page as ZPage; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		protected override void OnInit(EventArgs e)
		{
			SuspendValidationMessages = false;
			SaveRequest = false;
			LanguageSet = false;
			UserActivityLogger.SetLogActionTime(UserActivityLog.PageInitCall);
			base.OnInit(e);

			Page.MaintainScrollPositionOnPostBack = true;
			Page.Controls.Add(PageSmokeScreen);
			if ((this is IModulePage) || (this is ZIFramePage))
			{
				ZDateEdit scriptDateBox = new ZDateEdit();
				scriptDateBox.DateTimeFormat = ZDateTimePickerFormat.Long;
				scriptDateBox.Visible = false;
				this.Controls.Add(scriptDateBox);
			}
			if (FormControl != null)
			{
				AddLightBox(FormControl);
				HiddenField updateLastFocus = new HiddenField();
				updateLastFocus.ID = "__UPDATELASTFOCUSID";
				updateLastFocus.Value = "";
				FormControl.Controls.AddAt(0, updateLastFocus);

				//TextBox LastFocus = new TextBox();
				//LastFocus.Width = 300;
				//LastFocus.ID = "__LASTFOCUSID";
				//LastFocus.Text = "";
				//FormControl.Controls.AddAt(0, LastFocus);

				//TextBox BeforeLastFocus = new TextBox();
				//BeforeLastFocus.Width = 300;
				//BeforeLastFocus.ID = "__BEFORELASTFOCUSID";
				//BeforeLastFocus.Text = "";
				//FormControl.Controls.AddAt(0, BeforeLastFocus);

				HiddenField lastFocus = new HiddenField();
				lastFocus.ID = "__LASTFOCUSID";
				lastFocus.Value = "";
				FormControl.Controls.AddAt(0, lastFocus);

				HiddenField beforeLastFocus = new HiddenField();
				beforeLastFocus.ID = "__BEFORELASTFOCUSID";
				beforeLastFocus.Value = "";
				FormControl.Controls.AddAt(0, beforeLastFocus);

				HiddenField testing = new HiddenField();
				testing.ID = "Testing";
				testing.Value = "";
				FormControl.Controls.AddAt(0, testing);

				foreach (ZPageConfirmation confirmation in PageConfirmations)
				{
					FormControl.Controls.Add(confirmation.ResponseHolder);
				}
			}

			if (!IsDesignMode)
			{
				// The following code checks if the session has disappeared while the user attempts to access protected content
				if (PageRequiresLogin(RequestUrl) && (SiteUser == null || !SiteUser.IsLoggedIn))
				{
					ResolveUnAuthenticatedUser();
				}

				fBrowserType = GetBrowserType();
				if (ShouldTraverseControlTreeAndExtractOnInit)
				{
					TraverseControlTreeAndExtractResources(this);
				}
				SetHeadersAndStyle();
				SetupGrids();
				SetFooter();
			}
			SetupPostBackConditionControls();

			if (Session != null && !IsLoginPage() && SiteUser?.IsLoggedIn == true)
			{
				ViewStateUserKey = Session.SessionID;
			}
		}

		protected virtual void AddLightBox(HtmlForm form)
		{
			form.Controls.Add(LightBox);
		}

		protected virtual void RenderLightBoxScript()
		{
			LightBox.RenderScript();
		}

		public bool RqdAth
		{
			get { return PageRequiresLogin(RequestUrl); }
		}

		protected virtual void ResolveUnAuthenticatedUser()
		{
			System.Web.Security.FormsAuthentication.SignOut();
			Session.Abandon();
			HttpContext.Current.Response.Redirect(AppInstance.LoginPage, true);
		}

		protected virtual bool ShouldTraverseControlTreeAndExtractOnInit
		{
			get { return true; }
		}

		protected
#if DEBUG
		virtual
#endif
		Uri RequestUrl
		{
			get { return Request.Url; }
		}

		protected
#if DEBUG
		virtual
#endif
		NameValueCollection RequestQueryString
		{
			get { return Request.QueryString; }
		}

		protected virtual bool PageRequiresLogin(Uri url)
		{
			return false;
		}

		protected override void OnLoad(EventArgs e)
		{
			UserActivityLogger.SetLogActionTime(UserActivityLog.PageLoadCall);
			//datasource should be loaded after ViewState is loaded so we can take the 
			//DataSourceIndexer from the ViewState and load the datasource from the Session
			LoadOrCreateDataSource();
			if (!IsDesignMode && !ShouldTraverseControlTreeAndExtractOnInit)
			{
				TraverseControlTreeAndExtractResources(this);
			}
			base.OnLoad(e);
			Bind();
			HandlePostbackConfirmations();
		}

		public override void ProcessRequest(HttpContext context)
		{
			using (Db.DisposableActionForDbConnection())
			{
				base.ProcessRequest(context);
			}
		}

		protected override void OnUnload(EventArgs e)
		{
			base.OnUnload(e);

			this.RemoveExpiredSessionData();
			translationFeedbackManager.OnPageUnload(this);
			Db.DisposeThreadConnection();
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			AddFindButtonToDisabledControls();
			Controls.Add(NoScriptBlock);
			EnsureChildControls();
			UpdateErrorsDisplay();
			RenderScripts();
			SetupControlsWithPostbackCondition(this.Controls);
			TraverseControlTreeAndExtractResources(this);
			ClearAllNotificationsForNewEmptyGridRows();
			Localize();
		}

		protected void AddFindButtonToDisabledControls()
		{
			var modulePage = this as IModulePage;
			if (modulePage != null && modulePage.SearchControl != null && modulePage.SearchControl.FilterStripControl != null && modulePage.SearchControl.FilterStripControl.FindButton != null)
			{
				DisabledSubmitButtons.Add(modulePage.SearchControl.FilterStripControl.FindButton.ClientID);
			}
		}

		void SetupControlsWithPostbackCondition(ControlCollection pageControls)
		{
			foreach (Control pageControl in pageControls)
			{
				if (pageControl is IPostbackConditionControl && pageControl is WebControl)
				{
					IPostbackConditionControl postbackControl = (IPostbackConditionControl)pageControl;
					WebControl pageWebControl = (WebControl)pageControl;
					if (pageWebControl.Attributes["beforePostback"] != null)
					{
						pageWebControl.Attributes.Remove("beforePostback");
					}
					if (pageWebControl.Attributes["postbackCondition"] != null)
					{
						pageWebControl.Attributes.Remove("postbackCondition");
					}
					if (postbackControl.AutoPostBack)
					{
						if (pageWebControl.Attributes["onChange"] != null)
						{
							pageWebControl.Attributes.Add("beforePostback", pageWebControl.Attributes["onChange"]);
						}
						if (!string.IsNullOrEmpty(postbackControl.PostbackCondition))
						{
							pageWebControl.Attributes.Add("postbackCondition", postbackControl.PostbackCondition);
						}
					}
				}
				SetupControlsWithPostbackCondition(pageControl.Controls);
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			if (fFactory != null)
			{
				fFactory = null;
			}
		}

		#endregion

		#region JavaScript rendering

		protected virtual string StyleSheetFileName
		{
			get { return AppInstance.BaseStyleSheet; }
		}

		public StringCollection AdditionalStyleSheets
		{
			get
			{
				if (fAdditionalStyleSheets == null)
				{
					fAdditionalStyleSheets = new StringCollection();
				}
				return fAdditionalStyleSheets;
			}
		}
		StringCollection fAdditionalStyleSheets;

		Control GetControl(ControlCollection controls, string id)
		{
			Control result = null;
			foreach (Control childControl in controls)
			{
				if (childControl.ID == id)
				{
					return childControl;
				}
				result = GetControl(childControl.Controls, id);
				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		int CountControls(ControlCollection controls, string iD)
		{
			int result = 0;
			foreach (Control childControl in controls)
			{
				if (childControl.ID == iD)
				{
					result++;
				}
				result += CountControls(childControl.Controls, iD);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		protected override void Render(HtmlTextWriter writer)
		{
			RenderButtonsDisablingScript();
			UserActivityLogger.SetLogActionTime(UserActivityLog.PageRenderCall);

			var builder = new StringBuilder();
			using (var builderWriter = new StringWriter(builder))
			{
				base.Render(new HtmlTextWriter(builderWriter));
			}
			var output = builder.ToString();
			builder.Clear();

			var headClosingTag = "</head>";
			var positionOfHeadEndinfTag = output.IndexOf(headClosingTag, StringComparison.OrdinalIgnoreCase);
			if (positionOfHeadEndinfTag > 0)
			{
				builder.Append($"<link type=\"text/css\" rel=\"stylesheet\" href=\"{StyleSheetFileName}\" />");
				foreach (var additionalStyleSheet in AdditionalStyleSheets)
				{
					builder.Append($"{System.Environment.NewLine}<link type=\"text/css\" rel=\"stylesheet\" href=\"{additionalStyleSheet}\" />");
				}

				output = output.Insert(positionOfHeadEndinfTag, $"{builder}{System.Environment.NewLine}");
				builder.Clear();
			}

			writer.Write(output);
		}

		internal void RenderScripts()
		{
			RenderPageBaseScript();

			RenderDebugConsoleScript();

			RenderLightBoxScript();

			if (NotificationFlags.DisplayAny && DataSource != null && !DataSource.IsDeleted && !SuspendValidationMessages)
			{
				RenderValidationErrorMessageScript();
			}

			RenderJavascriptFrameworkScript();

			if (!ZClientScript.IsClientScriptIncludeRegistered("ZScreen_Scripts"))
			{
				ZClientScript.RegisterClientScriptInclude("ZScreen_Scripts", ScreenScriptFile.FileName);
			}

			if (!ZClientScript.IsClientScriptIncludeRegistered("ZPage_Scripts"))
			{
				ZClientScript.RegisterClientScriptInclude("ZPage_Scripts", CommonPageScriptFile.FileName);
			}

			RenderPostbackOverrideScript();

			RenderPageSpecificScripts();

			RenderPageAnchorScript();

			RenderWebServerServicesScripts();

			RenderWebValidationScript();

			if (!SuspendValidationMessages)
			{
				RenderCustomAlertMessageFromSession();

				RenderPageConfirmationScript();
			}

			RenderAutoIFrameScript();

			translationFeedbackManager.RenderTranslationFeedbackScript(this);
		}

		protected void RenderWebServerServicesScripts()
		{
			RenderWebServerServicesScriptsCore(Controls);
		}

		protected virtual void RenderWebServerServicesScriptsCore(ControlCollection controls)
		{
			foreach (Control childControl in controls)
			{
				if (childControl is IWebServiceMethodsCaller)
				{
					RegisterWebServerServiceCallerScripts((IWebServiceMethodsCaller)childControl);
				}
				RenderWebServerServicesScriptsCore(childControl.Controls);
			}
		}

		protected virtual void RegisterWebServerServiceCallerScripts(IWebServiceMethodsCaller methodsCaller)
		{
			foreach (IWebServiceMethod webServiceMethod in methodsCaller.WebServiceMethods)
			{
				RegisterWebServerServiceMethodScripts(webServiceMethod);
			}
		}

		protected virtual void RegisterWebServerServiceMethodScripts(IWebServiceMethod webServiceMethod)
		{
			Dictionary<string, ZWebResource> serviceScripts = webServiceMethod.ServiceScripts(this);
			foreach (string scriptKey in serviceScripts.Keys)
			{
				if (!Page.ZClientScript.IsClientScriptIncludeRegistered(scriptKey))
				{
					Page.ZClientScript.RegisterClientScriptInclude(scriptKey, serviceScripts[scriptKey].FileName);
				}
			}
		}

		void RenderAutoIFrameScript()
		{
			string fZIFramePage_SizeScript = "ZIFramePage_SizeScript";
			if (!ZClientScript.IsClientScriptBlockRegistered(GetType(), fZIFramePage_SizeScript))
			{
				string script =
					@"
<script type='text/javascript'>        
function autoSizeIframe(frameId){
	try{
		var iframe = parent.document.getElementById(frameId);
		var innerDoc = (iframe.contentDocument) ? iframe.contentDocument : iframe.contentWindow.document;
        var objToResize = (iframe.style) ? iframe.style : iframe;

		objToResize.height = innerDoc.body.offsetHeight + GetMarginsSize(innerDoc.body);
	}
	catch(err){
		window.status = err.message;
	}
}

function GetMarginsSize(control){
	var margins = 0;
	var marginTop;
	var marginBottom;
	if (control.currentStyle){
		marginTop = control.currentStyle.marginTop;
		marginBottom = control.currentStyle.marginBottom;
	}
	else {
		marginTop = control.getStyle('margin-Top');
		marginBottom = control.getStyle('margin-Bottom');
	}

	if (marginTop){
		margins = parseInt(marginTop);
	}
	if (marginBottom){
		margins = margins + parseInt(marginBottom);
	}
	return margins;
}
</script>";
				ZClientScript.RegisterClientScriptBlock(GetType(), fZIFramePage_SizeScript, script);
			}
		}

		void RenderCustomAlertMessageFromSession()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (!CanUseSessionForTesting)
				{
					return;
				}
			}
#endif
			object alert = null;
			if (Session != null)
			{
				alert = Session[zPageCustomAlertMessageIndexer];
				Session.Remove(zPageCustomAlertMessageIndexer);
			}
			if (alert != null && !string.IsNullOrEmpty(alert.ToString()))
			{
				ShowClientSideAlert(alert.ToString());
			}
		}
		public const string zPageCustomAlertMessageIndexer = "ZPageCustomAlertMessage";

#if DEBUG
		protected bool CanUseSessionForTesting
		{
			get
			{
				if (!Globals.IsTest)
				{
					return true;
				}
				try
				{
					if (Session == null)
					{
						return false;
					}
				}
				catch (Exception) // CriticalExceptionIsHandled Reason = Test only method.
				{
					return false;
				}
				return true;
			}
		}
#endif

		protected virtual void RenderPageAnchorScript()
		{
		}

		protected virtual void RenderPageSpecificScripts()
		{
		}

		public ZClientScriptManager ZClientScript
		{
			get
			{
				if (fZClientScript == null)
				{
					fZClientScript = GetZClientScript();
				}
				return fZClientScript;
			}
		}
		ZClientScriptManager fZClientScript;

		protected virtual ZClientScriptManager GetZClientScript()
		{
			return new ZClientScriptManager(this);
		}

		[Obsolete("Use ZClientScript instead", true)]
		public new ClientScriptManager ClientScript
		{
			get { throw new InvalidOperationException("A control attempted to registere a client script directly with the ScriptManager. ZPage.ZClientScript should be used as ClientScripts are bodgy."); }
		}

		internal ClientScriptManager ClientScriptInternal
		{
			get { return base.ClientScript; }
		}

		#region RenderJavascriptFrameworkScript

		const string JavascriptFrameworkScriptKey = "ZPage_JavascriptFrameworkScript";

		void RenderJavascriptFrameworkScript()
		{
			if (!ZClientScript.IsClientScriptBlockRegistered(JavascriptFrameworkScriptKey))
			{
				string javascriptFrameworkScript = string.Format("<SCRIPT src='{0}'></SCRIPT>", JavascriptFramework.FileName);
				ZClientScript.RegisterClientScriptBlock(GetType(), JavascriptFrameworkScriptKey, javascriptFrameworkScript);
			}
		}

		#endregion

		#region RenderButtonsDisablingScript

		const string ButtonsDisablingScriptKey = "ZPage_ButtonsDisablingScriptScript";

		void RenderButtonsDisablingScript()
		{
			if (!ZClientScript.IsClientScriptBlockRegistered(ButtonsDisablingScriptKey))
			{
				if (DisabledSubmitButtons.Count > 0)
				{
					var disabledButtonsScript = "var DisabledButtons = new Array();";
					if (DisabledSubmitButtons != null)
					{
						var i = 0;
						foreach (var buttonID in DisabledSubmitButtons)
						{
							disabledButtonsScript += string.Format("{0}DisabledButtons[{1}]='{2}';", System.Environment.NewLine, i, buttonID); // javascript code should not be translated
							i++;
						}
					}

					var buttonsDisablingScript =
						@"<SCRIPT>
					 " + disabledButtonsScript + @"
					  </SCRIPT>";
					ZClientScript.RegisterClientScriptBlock(GetType(), ButtonsDisablingScriptKey, buttonsDisablingScript);
				}
			}
		}

		#endregion

		#region RenderPageBaseScript

		const string PageBaseScriptKey = "PageBaseScript";

		void RenderPageBaseScript()
		{
			if (!ZClientScript.IsClientScriptBlockRegistered(GetType(), PageBaseScriptKey))
			{
				string script =
						@"<SCRIPT>

                    function AttachToScrollEvent(EventHandlerFunction)
                    {
                        if(window.addEventListener) // Firefox 1+, Opera 9, Safari 3+, etc.
                        {
                            window.addEventListener(""scroll"", EventHandlerFunction, false);
                        }
                        else if(document.addEventListener) // Opera 7, Opera 8
                        {
                            document.addEventListener(""scroll"", EventHandlerFunction, false);
                        }
                        else if(""onscroll"" in self) // MSIE 6, 7 and MSIE 8
                        {
                            var oldonscroll = self.onscroll; 
                            if (typeof self.onscroll != 'function') 
                            { 
                                self.onscroll = EventHandlerFunction; 
                            } else 

                            { 
                                self.onscroll = function() 
                                    { 
                                        if (oldonscroll) 
                                        { 
                                            oldonscroll(); 
                                        } 
                                        EventHandlerFunction(); 
                                    } 
                            } 
                        };
                    }

                    function AttachToResizeEvent(EventHandlerFunction)
                    {
                        if(window.addEventListener) // Firefox 1+, Opera 9, Safari 3+, etc.
                        {
                            window.addEventListener(""resize"", EventHandlerFunction, false);
                        }
                        else if(document.addEventListener) // Opera 7, Opera 8
                        {
                            document.addEventListener(""resize"", EventHandlerFunction, false);
                        }
                        else if(""onresize"" in self) // MSIE 6, 7 and MSIE 8
                        {
                            var oldonresize = self.onscroll; 
                            if (typeof self.onresize != 'function') 
                            { 
                                self.onresize = EventHandlerFunction; 
                            } else 

                            { 
                                self.onresize = function() 
                                    { 
                                        if (oldonresize) 
                                        { 
                                            oldonresize(); 
                                        } 
                                        EventHandlerFunction(); 
                                    } 
                            } 
                        };
                    }

                    function DisablePage()
                    {
                        var PageSmokeScreen = $('" + PageSmokeScreenID + @"');
                        if (PageSmokeScreen)
                        {
                            var DocumentSize = documentSize();
                            $(PageSmokeScreen).setStyle('left', '0px');
                            $(PageSmokeScreen).setStyle('top', '0px');
                            $(PageSmokeScreen).setStyle('width', DocumentSize.width + 'px');
                            $(PageSmokeScreen).setStyle('height', DocumentSize.height + 'px');
                            $(PageSmokeScreen).setStyle('z-index', '1500');
                            $(PageSmokeScreen).setStyle('display', 'block');
                        }
                    }

                    function EnablePage()
                    {
                        var PageSmokeScreen = $('" + PageSmokeScreenID + @"');
                        if (PageSmokeScreen)
                        {
                            $(PageSmokeScreen).setStyle('z-index', 0);
                            $(PageSmokeScreen).setStyle('display', 'none');
                        }
                    }

                    

					</SCRIPT>";

				ZClientScript.RegisterClientScriptBlock(GetType(), PageBaseScriptKey, script);
			}
		}

		public List<string> DisabledSubmitButtons
		{
			get { return disabledSubmitButtons ?? (disabledSubmitButtons = new List<string>()); }
		}
		List<string> disabledSubmitButtons;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected Panel PageSmokeScreen
		{
			get
			{
				if (pageSmokeScreen == null)
				{
					pageSmokeScreen = new Panel();
					pageSmokeScreen.ID = PageSmokeScreenID;
					pageSmokeScreen.CssClass = "PageSmokeScreen";
					pageSmokeScreen.Style["display"] = "none";
					pageSmokeScreen.Style["position"] = "absolute";
				}
				return pageSmokeScreen;
			}
		}

		Panel pageSmokeScreen;

		protected virtual string PageSmokeScreenID
		{
			get
			{
				return "_PageSmokeScreen";
			}
		}

		protected ZLightBox LightBox
		{
			get
			{
				if (lightBox == null)
				{
					lightBox = new ZLightBox();
					lightBox.ID = LightBoxID;
				}
				return lightBox;
			}
		}

		ZLightBox lightBox;

		protected virtual string LightBoxID
		{
			get
			{
				return "_LightBox";
			}
		}

		#endregion

		#region RenderPageConfirmationScript

		const string PageConfirmationScriptKey = "PageConfirmationScript";

		void RenderPageConfirmationScript()
		{
			foreach (ZPageConfirmation confirmation in PageConfirmations)
			{
				if (confirmation.IsRequired && !confirmation.HasResponse)
				{
					AddClientSidePageConfirmationScript(PageConfirmationScriptKey, confirmation);
					break;
				}
			}
		}

		protected virtual void AddClientSidePageConfirmationScript(string key, ZPageConfirmation pageConfirmation)
		{
		}

		#endregion

		#region RenderDebugConsoleScript

		const string DebugConsoleScriptKey = "DebugConsoleScript";

		void RenderDebugConsoleScript()
		{
			if (!(this is ZIFramePage))
			{
				if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), DebugConsoleScriptKey) && FormControl != null)
				{
					string script =
							@"<SCRIPT>
					function DebugConsole_NewMessage(Message)
					{
					}

					function DebugConsole_Clear()
					{
					}
					</SCRIPT>";

#if DEBUG
					script =
							@"<SCRIPT>
                    function ResizeDetected(evt) 
                    {
                        PositionDebugConsole();
                    }

                    function ScrollingDetected(evt) 
                    {
                        PositionDebugConsole();
                    }

                    function PositionDebugConsole()
                    {
                        var DebugConsole = $('DebugConsole');
                        if (DebugConsole)
                        {
                            DebugConsole.style.left=posRight()-parseInt(DebugConsole.style.width)-15+'px';
                            DebugConsole.style.top=posTop()+'px';
                        }
                    }

                    function OnDebugLoad()
                    {
                        PositionDebugConsole();
                    }

                    function addDebugLoadEvent(func) 
                    { 
                        var oldonload = window.onload; 
                        if (typeof window.onload != 'function') 
                        { 
                            window.onload = func; 
                        } 
                        else 
                        { 
                            window.onload = function() 
                                { 
                                    if (oldonload) 
                                    { 
                                        oldonload(); 
                                    } 
                                    func(); 
                                } 
                        } 
                    }

                    addDebugLoadEvent(PositionDebugConsole);

                    AttachToScrollEvent(ScrollingDetected);

                    AttachToResizeEvent(ResizeDetected);

					function DebugConsole_NewMessage(Message)
					{
                        try{
                            if (Message)
                            {
                                var currentTime = new Date();
                                var hours = currentTime.getHours();
                                var minutes = currentTime.getMinutes();
                                var seconds = currentTime.getSeconds();
                                Message='<b>' + hours.toPrecision(2) + ':' + minutes.toPrecision(2) + ':' + parseInt(seconds).toPrecision(2) + ' ' + (hours > 11 ? 'PM' : 'AM') + '</b> - ' + Message;

                                var Str = $('DebugConsoleOutput').innerHTML;
                                Str += Message.toString() + '<br />';
                                $('DebugConsoleOutput').innerHTML = Str;
                            }
                        } catch(err) { }
					}

					function DebugConsole_Clear()
					{
                        $('DebugConsoleOutput').innerHTML = '';
					}
                    </SCRIPT>";

					Panel debugConsole = new Panel();
					debugConsole.ID = "DebugConsole";
					debugConsole.Style.Add("position", "absolute");
					debugConsole.Style.Add("border", "1px solid #000000");
					debugConsole.Style.Add("width", "250px");
					debugConsole.Style.Add("height", "250px");
					debugConsole.Style.Add("background-color", "#ffffff");
					debugConsole.Style.Add("display", "inline");
					debugConsole.Style.Add("padding", "5px");
					Label debugConsoleHeader = new Label();
					debugConsoleHeader.Style.Add("height", "20px");
					debugConsoleHeader.Text = "JavaScript Console Started";
					debugConsole.Controls.Add(debugConsoleHeader);

					Panel debugConsoleOutput = new Panel();
					debugConsoleOutput.ID = "DebugConsoleOutput";
					debugConsoleOutput.Style.Add("width", "250px");
					debugConsoleOutput.Style.Add("height", "230px");
					debugConsoleOutput.Style.Add("overflow", "auto");
					debugConsole.Style.Add("display", "inline");

					debugConsole.Controls.Add(debugConsoleOutput);
					FormControl.Controls.Add(debugConsole);

#endif
					ZClientScript.RegisterClientScriptBlock(GetType(), DebugConsoleScriptKey, script);
				}
			}
		}

		#endregion

		#region PostbackOverrideScript

		const string PostbackOverrideScriptKey = "ZPage_PostbackOverrideScript";

		void RenderPostbackOverrideScript()
		{
			AddPostbackOverrideScript();
		}

		protected virtual void AddPostbackOverrideScript()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), PostbackOverrideScriptKey))
			{
				string script = @"<SCRIPT>
                        var originalDoPostBack;

                        function customDoPostBack(eventTarget, eventArgument)
                        {{
                            var performPostBack = true;
                            if (getElement(eventTarget))
                            {{
                                try
                                {{
                                    var postbackCondition = getElement(eventTarget).getProperty('postbackCondition'); 
                                    performPostBack = evaluateTrueFalseFunction(postbackCondition)
                                }}
                                catch(e){{}}
                            }}
                            if (performPostBack)
                            {{
								{0}
                                originalDoPostBack(eventTarget, eventArgument);
                            }}
                        }}

                        function evaluateTrueFalseFunction(functionCode)
                        {{
                            if (functionCode && functionCode!='' && functionCode!='undefined')
                            {{
                                var newFunction = new Function(functionCode);
                                try
                                {{
                                    return newFunction();
                                }}
                                catch(e){{}}
                            }}
                            return true;
                        }}

                        function setupPostBackHandlers()
                        {{
                            originalDoPostBack = __doPostBack;
                            __doPostBack = customDoPostBack;
                        }}

                        addLoadEvent(setupPostBackHandlers);
						
						{1}

                        </SCRIPT>";

				ZClientScript.RegisterClientScriptBlock(GetType(), PostbackOverrideScriptKey, string.Format(script, GetPostbackPerformAdditionalFunctionCalls(), GetPostbackAdditionalJavaScript()));
			}
		}

		protected virtual string GetPostbackAdditionalJavaScript()
		{
			return string.Empty;
		}

		protected virtual string GetPostbackPerformAdditionalFunctionCalls()
		{
			return string.Empty;
		}

		#endregion

		#region RenderWebValidationScript

		const string WebValidationScriptKey = "WebValidationScript";

		/// <summary>
		/// Renders script to display an error message on page load when business object has errors
		/// </summary>
		protected virtual void RenderWebValidationScript()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), WebValidationScriptKey))
			{
				string javaScript = @"<SCRIPT>
                                        function ClearValidation(NotificationID)
                                        { 
                                            var Notification=getElement(NotificationID);
                                            if (Notification!=null)
                                            {
                                                Notification.setStyle('display', 'none');
                                            }
                                            return true;
                                        }
                                        </SCRIPT>";
				ZClientScript.RegisterClientScriptBlock(GetType(), WebValidationScriptKey, javaScript);
			}
		}

		#endregion

		#region RenderValidationErrorMessageScript

		public bool DisablePageBusinessObjectValidation
		{
			get { return disablePageBusinessObjectValidation; }
			set { disablePageBusinessObjectValidation = value; }
		}

		bool disablePageBusinessObjectValidation;

		public void SuspendValidationErrorMessageForCurrentLoad()
		{
			SuspendValidationMessages = true;
		}

		protected bool SuspendValidationMessages
		{
			get { return suspendValidationMessages; }
			set { suspendValidationMessages = value; }
		}
		bool suspendValidationMessages;

		const string ValidationScriptKey = "ValidationScript";

		/// <summary>
		/// Renders script to display an error message on page load when business object has errors
		/// </summary>
		void RenderValidationErrorMessageScript()
		{
			if (!DisablePageBusinessObjectValidation && BusinessObjectToValidate != null &&
			((BusinessObjectToValidate.HasErrors && NotificationFlags.DisplayErrors)
			|| (BusinessObjectToValidate.HasMessageErrors && NotificationFlags.DisplayMessageErrors)
			|| (BusinessObjectToValidate.HasWarnings && NotificationFlags.DisplayWarnings)))
			{
				AddClientSideScript(GetBusinessObjectErrors(), ValidationScriptKey);
			}
		}

		public void ScriptNotificationMessage(string errorMessage)
		{
			AddClientSideScript(errorMessage, ValidationScriptKey);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		protected virtual void AddClientSideScript(string errorMessage, string key)
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), key))
			{
				string script = (this.BrowserType == BrowserType.IE)
					? // ie script
										@"<SCRIPT>
					window.attachEvent('onload', _Base_Page_Load);
					
					function _Base_Page_Load()
					{{
						{2}
						//ZErrorProvider_PositionControls('{0}');
					}}

					function _Base_DisplayValidationMessage()
					{{
						alert('{1}');
					}}
					</SCRIPT>"

										: // mozilla script
					@"<SCRIPT event='onload'>				
					
					{2}
					//ZErrorProvider_PositionControls('{0}');

					function _Base_DisplayValidationMessage()
					{{
						alert('{1}');
					}}
					</SCRIPT>";

				script += String.Format(@"<SCRIPT src='{0}'></SCRIPT>", ScriptFile.FileName);
				script = String.Format(script, NotificationsArea.ClientID, errorMessage, DialogFunction);
				ZClientScript.RegisterClientScriptBlock(GetType(), key, script);
			}
		}

		#endregion

		#region ShowClientSideAlert

		public void ShowClientSideAlert(ZString message)
		{
			if (message == ValidationScriptKey)
			{
				throw new ArgumentException("The message/key '" + ValidationScriptKey + "' is reserved for the web architecture.");
			}
			else if (!message.IsEmpty)
			{
				ZString key = message;

				if (!Page.ZClientScript.IsStartupScriptRegistered(GetType(), key))
				{
					var script = string.Format(@"<SCRIPT>
						var myCookie = Cookie.read('{0}');
						if (!myCookie)
						{{
							Cookie.write('{0}', '{0}');
							alert('{1}');
						}}
						</SCRIPT>", Guid.NewGuid().ToString(), message);

					ZClientScript.RegisterStartupScript(GetType(), key, script);
				}
			}
		}

		#endregion

		protected string DialogFunction
		{
			get
			{
				return !SuppressErrorDialog ? "_Base_DisplayValidationMessage();" : "";
			}
		}

		protected bool SuppressErrorDialog
		{
			get
			{
				object res = ViewState["ZPage_SuppressErrorDialog"];
				return (res != null) && (bool)res;
			}
			set { ViewState["ZPage_SuppressErrorDialog"] = value; }
		}

		#endregion

		#region Page Confirmations

		protected List<ZPageConfirmation> PageConfirmations
		{
			get
			{
				if (pageConfirmations == null)
				{
					pageConfirmations = new List<ZPageConfirmation>();
					AddPageConfirmations(pageConfirmations);
				}
				return pageConfirmations;
			}
		}

		List<ZPageConfirmation> pageConfirmations;

		protected virtual void AddPageConfirmations(List<ZPageConfirmation> confirmations)
		{
		}

		#endregion

		#region Binding

		/// <summary>
		/// Binding DataSource to controls
		/// </summary>
		protected internal void Bind()
		{
			UserActivityLogger.SetLogActionTime(UserActivityLog.PageBindCall);
			if (DataSource != null && ControlsToBind != null)
			{
				OnPreBind();
				new ZWebControlBinder(DataSource).Bind(ControlsToBind);
				OnAfterBind();
			}
		}

		protected virtual void OnAfterBind()
		{
		}

		protected override void RaisePostBackEvent(IPostBackEventHandler sourceControl, string eventArgument)
		{
			ProcessExternallyFiredPostBackEvents((Control)sourceControl, eventArgument);
			base.RaisePostBackEvent(sourceControl, eventArgument);
		}

		/// <summary>
		/// Method to Setup grids. Call during PageInit
		/// You should NOT have references to DataSource in this method, because when it's called
		/// the DataSource is ALWAYS NULL
		/// Every grid that needs to be edited on this page needs to be set up here
		/// </summary>
		protected virtual void SetupGrids()
		{
		}

		/// <summary>
		/// Anything that requires DataSource before binding should be set in this method
		/// Any grid that is set up in thios method will NOT BE able to remember values 
		/// between postbacks when it's being edited
		/// </summary>
		protected virtual void OnPreBind()
		{
		}

		protected virtual ControlCollection ControlsToBind
		{
			get { return Controls; }
		}

		protected void ProcessExternallyFiredPostBackEvents(Control source, string eventArgument)
		{
			if (source != null)
			{
				foreach (IExternallyFiredPostBackHandler handler in GetExternallyFiredPostBackEventHandlers(Controls))
				{
					if ((handler != source) && !IsChildControl(handler.Controls, source))
					{
						handler.HandleExternallyFiredPostBack(source, EventArgs.Empty);
					}
				}
			}
		}

		IExternallyFiredPostBackHandler[] GetExternallyFiredPostBackEventHandlers(ControlCollection controls)
		{
			ArrayList handlers = new ArrayList();
			foreach (Control control in controls)
			{
				if (control is IExternallyFiredPostBackHandler)
				{
					handlers.Add(control);
				}
				handlers.AddRange(GetExternallyFiredPostBackEventHandlers(control.Controls));
			}
			return (IExternallyFiredPostBackHandler[])handlers.ToArray(typeof(IExternallyFiredPostBackHandler));
		}

		#endregion

		#region Validation

		public override void Validate(string validationGroup)
		{
			// base.Validate(validationGroup) relies on _validators to be not null for the logic to fire. Possible bug in ASP.NET
			if (Validators.Count < -1) { }
			base.Validate(validationGroup);
		}

		public override void Validate()
		{
			Validate(BusinessObjectToValidate);
		}

		public void Validate(BusinessObject bizOToValidate)
		{
			base.Validate();
			ValidateBusinessObject(bizOToValidate);
		}

		protected void ClearAllNotificationsForNewEmptyGridRows()
		{
			ClearAllNotificationsForNewEmptyGridRows(this.Controls);
		}

		protected void ClearAllNotificationsForNewEmptyGridRows(ControlCollection webControls)
		{
			foreach (Control webControl in webControls)
			{
				if (webControl is ZDataGrid)
				{
					ClearAllNotificationsForNewEmptyGridRows((ZDataGrid)webControl);
				}
				else
				{
					if (webControl.Controls.Count > 0)
					{
						ClearAllNotificationsForNewEmptyGridRows(webControl.Controls);
					}
				}
			}
		}

		protected internal void ClearAllNotificationsForNewEmptyGridRows(ZDataGrid gridControl)
		{
			if (!gridControl.ReadOnly && gridControl.Collection != null && (gridControl.AllowAdd || gridControl.AllowEdit))
			{
				for (int i = gridControl.Collection.Count - 1; i >= 0; i--)
				{
					BusinessObject bO = (BusinessObject)gridControl.Collection[i];
					if (!(bO.HasChanges || bO.IsInDatabase || bO.IsDeleted))
					{
						bO.ClearAllNotifications();
					}
				}
			}
		}

		protected void RemoveNewEmptyGridRows()
		{
			RemoveNewEmptyGridRows(this.Controls);
		}

		protected void RemoveNewEmptyGridRows(ControlCollection webControls)
		{
			foreach (Control webControl in webControls)
			{
				if (webControl is ZDataGrid)
				{
					RemoveNewEmptyGridRows((ZDataGrid)webControl);
				}
				else
				{
					if (webControl.Controls.Count > 0)
					{
						RemoveNewEmptyGridRows(webControl.Controls);
					}
				}
			}
		}

		protected internal void RemoveNewEmptyGridRows(ZDataGrid gridControl)
		{
			if (!gridControl.ReadOnly && gridControl.Collection != null && (gridControl.AllowAdd || gridControl.AllowEdit))
			{
				for (int i = gridControl.Collection.Count - 1; i >= 0; i--)
				{
					BusinessObject bO = (BusinessObject)gridControl.Collection[i];
					if (!(bO.HasChanges || bO.IsInDatabase || bO.IsDeleted))
					{
						gridControl.Collection.RemoveAt(i);
						bO.Delete();
					}
				}
			}
			RemoveNewEmptyGridRows(gridControl.Controls);
		}

		/// <summary>
		/// Validates business object behind the page
		/// </summary>
		void ValidateBusinessObject(BusinessObject bizOToValidate)
		{
			if (bizOToValidate != null && !Factory.IsValidationSuspended)
			{
				bizOToValidate.RunPreSaveValidation();
			}
		}

		/// <summary>
		/// Displays validation errors as a summary as well as error providers 
		/// next to each control
		/// </summary>
		void UpdateErrorsDisplay()
		{
			//if (DataSource != null && !DataSource.IsDeleted)
			//{
			//    Control[] Children = GetControlsRecursively(typeof(INotificationProvider));
			//    foreach (Control Ctrl in Children)
			//    {
			//        INotificationProvider NotificationControl = (INotificationProvider)Ctrl;
			//        IEnumerable<INotification> Notifications = NotificationControl.Notifications;

			//        if (NotificationsArea != null)
			//        {
			//            if (Notifications.GetErrors().Count() > 0 && NotificationFlags.DisplayErrors)
			//            {
			//                NotificationsArea.Controls.Add(new ZErrorProvider(Ctrl, Notifications));
			//            }
			//            else if (Notifications.GetMessageErrors().Count() > 0 && NotificationFlags.DisplayMessageErrors)
			//            {
			//                NotificationsArea.Controls.Add(new ZMessageErrorProvider(Ctrl, Notifications));
			//            }
			//            else if (Notifications.GetWarnings().Count() > 0 && NotificationFlags.DisplayWarnings)
			//            {
			//                NotificationsArea.Controls.Add(new ZWarningProvider(Ctrl, Notifications));
			//            }
			//        }
			//    }
			//}
		}

		protected string GetBusinessObjectErrors()
		{
			string result = "";
			if (BusinessObjectToValidate != null && !BusinessObjectToValidate.IsDeleted)
			{
				result = GetBusinessObjectErrorsForBizO(BusinessObjectToValidate);
			}
			return result;
		}

		protected virtual BusinessObject BusinessObjectToValidate
		{
			get { return DataSource; }
		}

		/// <summary>
		/// returns all the errors for a given business object
		/// </summary>
		protected string GetBusinessObjectErrorsForBizO(BusinessObject bizO)
		{
			ZStringBuilder message = new ZStringBuilder();
			message.Append(Res.GetString("f0456e9f-9222-41df-9615-28def45e09d6", "Please fix the following errors before proceeding:")).Append("\\n\\n");

			ZNotificationCollector bONotificationsIncludingChildren = new ZNotificationCollector(bizO, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

			if (bizO.HasErrors && NotificationFlags.DisplayErrors)
			{
				message.Append(bONotificationsIncludingChildren.GetErrors().ToUniqueMessageListString().Replace("'", "\\'").Replace("\r", "").Replace("\n", "\\n"));
				message.Append(String.Format("\\n\\n"));
			}
			if (bizO.HasMessageErrors && NotificationFlags.DisplayMessageErrors)
			{
				message.Append(bONotificationsIncludingChildren.GetMessageErrors().ToUniqueMessageListString().Replace("'", "\\'").Replace("\r", "").Replace("\n", "\\n"));
				message.Append(String.Format("\\n\\n"));
			}
			if (bizO.HasWarnings && NotificationFlags.DisplayWarnings)
			{
				message.Append(bONotificationsIncludingChildren.GetWarnings().ToUniqueMessageListString().Replace("'", "\\'").Replace("\r", "").Replace("\n", "\\n"));
				message.Append(String.Format("\\n\\n"));
			}
			return message.ToString();
		}

		#endregion

		#region Getting controls

		public Control[] FindControls(string bindTo)
		{
			return FindControlsHelper(bindTo, Controls).ToArray();
		}

		List<Control> FindControlsHelper(string bindTo, ControlCollection controls)
		{
			List<Control> result = new List<Control>();
			foreach (Control childControl in controls)
			{
				if (childControl is ISelfBindingWebControl)
				{
					if (((ISelfBindingWebControl)childControl).BindTo == bindTo)
					{
						result.Add(childControl);
					}
				}
				result.AddRange(FindControlsHelper(bindTo, childControl.Controls));
			}
			return result;
		}

		public Control[] GetControlsRecursively(Type controlType)
		{
			ArrayList types = new ArrayList();
			types.Add(controlType);
			return GetControlsRecursively(types);
		}

		public Control[] GetControlsRecursively(ArrayList controlTypes)
		{
			return (Control[])GetControlsRecursivelyFromControl(this, controlTypes).ToArray(typeof(Control));
		}

		protected bool IsControlOfaType(ArrayList controlTypes, Control ctrl)
		{
			foreach (Type t in controlTypes)
			{
				if (t.IsInstanceOfType(ctrl))
				{
					return true;
				}
			}

			return false;
		}

		protected ArrayList GetControlsRecursivelyFromControl(Control startingPoint, ArrayList controlTypes)
		{
			ArrayList result = new ArrayList();

			foreach (Control ctrl in startingPoint.Controls)
			{
				if (IsControlOfaType(controlTypes, ctrl))
				{
					result.Add(ctrl);
				}

				ArrayList children = GetControlsRecursivelyFromControl(ctrl, controlTypes);

				foreach (Control childCtrl in children)
				{
					if (IsControlOfaType(controlTypes, childCtrl))
					{
						result.Add(childCtrl);
					}
				}
			}

			return result;
		}

		#endregion

		#region Controls With Postback Condition Setup

		protected virtual void SetupPostBackConditionControls()
		{
		}

		#endregion

		#region Implementation

		protected virtual bool IsDataSourceInSession
		{
			get
			{
				if (!Globals.IsTest)
				{
					if (GetStringFromParameter(DataSourceInSessionParameterName) == DataSourceInSessionParameterValue)
					{
						return true;
					}
				}
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		protected virtual BrowserType GetBrowserType()
		{
			BrowserType result = BrowserType.Unknown;
			if (Request.Browser.Type.StartsWith("IE"))
			{
				result = BrowserType.IE;
			}
			else if (Request.Browser.Type.StartsWith("Netscape"))
			{
				result = BrowserType.Mozilla;
			}
			else if (Request.Browser.Type.StartsWith("Firefox"))
			{
				result = BrowserType.Mozilla;
			}

			return result;
		}

		protected void TraverseControlTreeAndExtractResources(Control parentControl)
		{
			IContainResources resourceControl = parentControl as IContainResources;
			if (resourceControl != null)
			{
				foreach (ZWebResource resource in resourceControl.Resources)
				{
#if DEBUG
					if (Globals.IsTest && !ServerMappedPathForTest.IsEmpty)
					{
						resource.SetServerMappedPathForTest(ServerMappedPathForTest);
					}
#endif
					resource.Extract();
				}
			}
			foreach (Control childControl in parentControl.Controls)
			{
				TraverseControlTreeAndExtractResources(childControl);
			}
		}

#if DEBUG
		public void SetServerMappedPathForTest(string path)
		{
			if (!Globals.IsTest)
			{
				throw new InvalidOperationException("Should only be used for testing");
			}
			ServerMappedPathForTest = path;
		}
		internal ZString ServerMappedPathForTest;
#endif
		#endregion

		#region IContainResources Members

		#region ScriptFile

		protected internal ZWebResource ScriptFile
		{
			get
			{
				if (fScriptFile == null)
				{
					fScriptFile = new ZWebResource(typeof(ZPage), "ErrorProviderPlacement.js", this);
				}

				return fScriptFile;
			}
		}
		ZWebResource fScriptFile;

		#endregion

		#region LoginStatusControl

		protected internal ZWebResource LoginStatusControl
		{
			get
			{
				if (fLoginStatusControl == null)
				{
					fLoginStatusControl = new ZWebResource(typeof(ZPage), "LoginStatus.ascx", this);
				}

				return fLoginStatusControl;
			}
		}
		ZWebResource fLoginStatusControl;

		#endregion

		#region SearchControlResource

		protected internal ZWebResource SearchControlResource
		{
			get
			{
				if (fSearchControlResource == null)
				{
					fSearchControlResource = new ZWebResource(typeof(ZPage), "SearchControl.ascx", this);
				}
				return fSearchControlResource;
			}
		}
		ZWebResource fSearchControlResource;

		#endregion

		#region Navigation bar resource

		public ZWebResource NavigationBarResource
		{
			get
			{
				if (fNavigationBarResource == null)
				{
					fNavigationBarResource = new ZWebResource(typeof(ZPage), "ZNavigationBar.ascx", this, "Enterprise.ZArchitecture.Web.GUI.Navigation");
				}
				return fNavigationBarResource;
			}
		}
		ZWebResource fNavigationBarResource;

		#endregion

		#region DocAddressControlResource

		protected ZWebResource DocAddressControlResource
		{
			get
			{
				if (fDocAddressControlResource == null)
				{
					fDocAddressControlResource = new ZWebResource(typeof(ZPage), "ZDocAddressControl.ascx", this, "Enterprise.ZArchitecture.Web.GUI.WebControls.ZDocAddressControl");
				}
				return fDocAddressControlResource;
			}
		}
		ZWebResource fDocAddressControlResource;

		#endregion

		#region CommonPageScriptFile

		protected internal ZWebResource CommonPageScriptFile
		{
			get
			{
				return commonPageScriptFile ?? (commonPageScriptFile = new ZWebResource(typeof(ZPage), "ZPage.js", this));
			}
		}
		ZWebResource commonPageScriptFile;

		#endregion

		#region ScreenScriptFile

		protected internal ZWebResource ScreenScriptFile
		{
			get
			{
				return screenScriptFile ?? (screenScriptFile = new ZWebResource(typeof(ZPage), "ZScreen.js", this));
			}
		}
		ZWebResource screenScriptFile;

		#endregion

		#region Javascript Framwork Resource

		protected internal ZWebResource JavascriptFramework
		{
			get
			{
				if (fJavascriptFramework == null)
				{
					fJavascriptFramework = new ZWebResource(typeof(ZPage), "mootools.js", this);
				}
				return fJavascriptFramework;
			}
		}
		ZWebResource fJavascriptFramework;

		#endregion

		public virtual ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = new ZWebResourceCollection();
				result.Add(ScriptFile);
				result.Add(JavascriptFramework);
				result.Add(ScreenScriptFile);
				result.Add(CommonPageScriptFile);
				result.Add(LoginStatusControl);
				result.Add(SearchControlResource);
				result.Add(NavigationBarResource);
				result.Add(DocAddressControlResource);
				AddWebServiceResources(result);
				return result;
			}
		}

		protected void AddWebServiceResources(ZWebResourceCollection resourceCollection)
		{
			Dictionary<string, ZWebResource> resources = WebServiceResources;
			foreach (string resourceFileName in resources.Keys)
			{
				resourceCollection.Add(resources[resourceFileName]);
			}
		}

		protected Dictionary<string, ZWebResource> WebServiceResources
		{
			get
			{
				return GetWebServiceResources(Controls, new Dictionary<string, ZWebResource>());
			}
		}

		protected Dictionary<string, ZWebResource> GetWebServiceResources(ControlCollection controls, Dictionary<string, ZWebResource> resources)
		{
			foreach (Control childContol in controls)
			{
				if (childContol is IWebServiceMethodsCaller)
				{
					foreach (IWebServiceMethod webServiceMethod in ((IWebServiceMethodsCaller)childContol).WebServiceMethods)
					{
						Dictionary<string, ZWebResource> serviceScripts = webServiceMethod.ServiceScripts(this);
						foreach (string scriptKey in serviceScripts.Keys)
						{
							if (!resources.ContainsKey(serviceScripts[scriptKey].FileName))
							{
								resources.Add(serviceScripts[scriptKey].FileName, serviceScripts[scriptKey]);
							}
						}
					}
				}
				resources = GetWebServiceResources(childContol.Controls, resources);
			}
			return resources;
		}

		#endregion

		#region Utility Methods

		internal void SaveDataSourceToSession()
		{
			fDataSourceIndexer = SaveDataSource(DataSourceIndexer, DataSource);
		}

		protected ZGuid SaveDataSource(ZGuid indexer, object dataSource)
		{
			if (indexer == ZGuid.Empty)
			{
				indexer = ZGuid.NewZGuid();
			}
			Session[indexer.ToString()] = dataSource;

			return indexer;
		}

		protected object LoadDataSource(ZGuid indexer)
		{
			return (indexer != ZGuid.Empty) ? Session[indexer.ToString()] : null;
		}

		protected void ClearCurrentDataSource()
		{
			ClearDataSource(DataSourceIndexer);
		}

		protected void ClearDataSource(ZGuid indexer)
		{
			Session.Remove(indexer.ToString());
		}

		public static bool IsChildControl(ControlCollection controls, Control target)
		{
			bool result = false;
			foreach (Control ctrl in controls)
			{
				if (ctrl == target)
				{
					result = true;
				}
				else
				{
					result = IsChildControl(ctrl.Controls, target);
				}
				if (result)
				{
					break;
				}
			}
			return result;
		}

		#endregion Utility Methods

		#region Url parsing

		protected ZGuid GetGuidFromParameter(string paramName)
		{
			return GetGuidFromParameter(paramName, HttpContext.Current.Request.Params);
		}

		protected internal ZGuid GetGuidFromParameter(string paramName, NameValueCollection @params)
		{
			ZGuid result = ZGuid.Empty;
			try
			{
				result = new ZGuid(@params[paramName]);
			}
			catch (ZTypeValueException) { }
			catch (NotSupportedException) { }
			catch (FormatException) { }
			return result;
		}

		protected ZString GetStringFromParameter(string paramName)
		{
			return GetStringFromParameter(paramName, HttpContext.Current.Request.Params);
		}

		protected internal ZString GetStringFromParameter(string paramName, NameValueCollection parameters)
		{
			ZString result = "";
			try
			{
				result = parameters[paramName];
			}
			catch (NotSupportedException) { }
			catch (FormatException) { }
			return result;
		}

		#endregion

		#region ClientFunctions

		public static class ClientFunctions
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
			static void RegisterReturnKeyCapture(Control sourceControl, string buttonClientID)
			{
				System.Web.UI.AttributeCollection attributes = null;
				if (sourceControl is WebControl)
				{
					attributes = ((WebControl)sourceControl).Attributes;
				}
				else if (sourceControl is HtmlControl)
				{
					attributes = ((HtmlControl)sourceControl).Attributes;
				}

				if (attributes != null)
				{
					attributes["onkeydown"] = string.Format("return ZPage_ProcessKeyDown(13, '{0}', event)", buttonClientID);
				}
			}

			public static void RegisterReturnKeyCapture(Control sourceControl, Button button)
			{
				RegisterReturnKeyCapture(sourceControl, button.ClientID);
			}

			public static void RegisterReturnKeyCapture(Control sourceControl, HtmlInputButton button)
			{
				RegisterReturnKeyCapture(sourceControl, button.ClientID);
			}
		}

		#endregion

		#region Relative Path

		public string PageRelativePath
		{
			get
			{
				return GetPageRelativePath();
			}
		}

		protected virtual string GetPageRelativePath()
		{
			return string.Empty;
		}

		#endregion

		#region Session Time Sensitive Data

		const string timeSensitiveDataKeysIndexer = "TimeSensitiveDataKeys";

		List<string> GetTimeSensitiveDataKeys(bool create = true)
		{
			var session = Session;
			if (session != null)
			{
				var keys = (List<string>)session[timeSensitiveDataKeysIndexer];
				if (keys == null && create)
				{
					keys = new List<string>();
					session[timeSensitiveDataKeysIndexer] = keys;
				}

				return keys;
			}

			return null;
		}

		public void SetSessionTimeSensitiveData(string key, object data)
		{
			var keys = GetTimeSensitiveDataKeys();

			if (keys != null)
			{
				var session = Session;
				if (session != null)
				{
					session[key] = new TimeSensitiveData() { Data = data };
					keys.Add(key);
				}
			}
		}

		public object GetSessionTimeSensitiveData(string key)
		{
			var session = Session;
			if (session != null)
			{
				var timeSensitiveData = (TimeSensitiveData)session[key];

				return timeSensitiveData?.Data;
			}

			return null;
		}

		public void RemoveExpiredSessionData()
		{
			var keys = GetTimeSensitiveDataKeys(false);

			if (keys != null)
			{
				var session = Session;
				if (session != null)
				{
					var expiredKeys = keys.OfType<string>().Where(key => session[key] is TimeSensitiveData timeSensitiveData && timeSensitiveData.HasExpired).ToArray();
					foreach (var key in expiredKeys)
					{
						session.Remove(key);
						keys.Remove(key);
					}
				}
			}
		}

		#endregion

		public string ControlKeyIdentifier(Control control)
		{
			return GetControlKeyIdentifier(control);
		}

		protected virtual string GetControlKeyIdentifier(Control control)
		{
			if (this is IModulePage)
			{
				IModulePage modulePage = (IModulePage)this;
				if (modulePage.SearchControl != null && modulePage.SearchControl.Module != null && modulePage.SearchControl.ModuleID != null && modulePage.SearchControl.ModuleID != WebModuleIDs.NotAssigned)
				{
					return modulePage.SearchControl.Module.ID.ToString();
				}
			}
			if (!string.IsNullOrEmpty(PageName))
			{
				return string.Format("{0}.{1}", PageName, control.ID);
			}
			return string.Empty;
		}

		public string PageName
		{
			get { return GetPageName(); }
		}

		protected virtual string GetPageName()
		{
			return string.Empty;
		}

		void Localize()
		{
			if (Res.CurrentLanguage != Res.DefaultLanguage)
			{
				Localize(this);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "property name for reflection")]
		protected virtual void Localize(Control control)
		{
			LocalizeProperty(control, "Text");
			LocalizeProperty(control, "Caption");
			LocalizeProperty(control, "Label");
			foreach (Control child in control.Controls)
			{
				Localize(child);
			}
		}

		void LocalizeProperty(Control control, string propertyName)
		{
			var property = control.GetType().GetProperty(propertyName);
			if (property != null && property.CanRead && property.CanWrite)
			{
				string value = property.GetValue(control, Array.Empty<object>()) as string;
				if (!string.IsNullOrEmpty(value))
				{
					property.SetValue(control, Translate(value), Array.Empty<object>());
				}
			}
		}

		public string Translate(string pageFragment)
		{
			return SimpleMarkupTranslator.Instance.Translate(LocalizationAsmid, LocalizationKeyPrefix, pageFragment);
		}

		string LocalizationKeyPrefix
		{
			get
			{
				if (localizationKeyPrefix == null)
				{
					localizationKeyPrefix = "wc|" + GetType().BaseType.Assembly.GetName().Name + "|";
				}
				return localizationKeyPrefix;
			}
		}

		string localizationKeyPrefix;

		internal UInt16 LocalizationAsmid
		{
			get
			{
				if (!localizationAsmid.HasValue)
				{
					unchecked
					{
						localizationAsmid = (UInt16)ZrsFile.CalculateAsmid("wc." + GetType().BaseType.Assembly.GetName().Name);
					}
				}
				return localizationAsmid.Value;
			}
		}
		UInt16? localizationAsmid;

		readonly TranslationFeedbackManager translationFeedbackManager;

#if DEBUG
		public TranslationFeedbackManager GetTranslationFeedbackManagerForTest() => translationFeedbackManager;
#endif

		#region Internal Properties

		internal void InitializeCultureInternal() => InitializeCulture();
		internal BusinessObject GetNewDataSourceInternal() => GetNewDataSource();
		internal void OnUnloadInternal(EventArgs e) => OnUnload(e);
		internal void OnPreLoadInternal(EventArgs e) => OnPreLoad(e);
		internal object SaveViewStateInternal() => SaveViewState();
		internal void OnPreRenderInternal(EventArgs e) => OnPreRender(e);
		internal string PageHeaderControlPathInternal => PageHeaderControlPath;
		internal bool ShowLogOffLinkButtonInternal => ShowLogOffLinkButton;
		internal bool ShowChangePasswordLinkButtonInternal => ShowChangePasswordLinkButton;
		internal void RaisePostBackEventInternal(IPostBackEventHandler sourceControl, string eventArgument) => RaisePostBackEvent(sourceControl, eventArgument);
		internal LoginStatus GetLoginStatusInternal() => GetLoginStatus();
		internal void OnInitInternal(EventArgs e) => OnInit(e);

		#endregion
	}
}
