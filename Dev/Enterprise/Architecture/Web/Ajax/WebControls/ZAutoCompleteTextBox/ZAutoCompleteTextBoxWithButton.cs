using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls.GuidTextBox.Internals;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZAutoCompleteTextBoxWithButton : CompositeControl, ISelfBindingPostbackWebControl, INotificationProvider
	{
		#region Constructors

		public ZAutoCompleteTextBoxWithButton()
			: base()
		{
		}

		#endregion

		#region Controls

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected ZAutoCompleteTextBox TextBox
		{
			get
			{
				if (fTextBox == null)
				{
					fTextBox = new ZAutoCompleteTextBox();
					fTextBox.ID = "tb";
					fTextBox.Helper = Helper;
					fTextBox.BindTo = BindTo;
					fTextBox.BindTextTo = BindTextTo;
					fTextBox.TextChanged += new EventHandler(TextBox_TextChanged);
					fTextBox.AutoPostBack = AutoPostBack;
					fTextBox.AllowEdit = AllowEdit;

					fTextBox.Helper = Helper;
					Controls.Add(TextBox);
				}
				return fTextBox;
			}
			set { fTextBox = value; }
		}
		ZAutoCompleteTextBox fTextBox;

		protected ZFindBox Button
		{
			get
			{
				if (fButton == null)
				{
					if (Helper.UsesGuidKey)
					{
						fButton = new AutoCompleteZGuidFindButton();
					}
					else
					{
						fButton = new AutoCompleteZFindButton();
					}
					((IButtonWithHelper)fButton).AutoCompleteTextBox = this.TextBox;
					fButton.BindTo = BindTo;
					fButton.ModuleID = ModuleID;
					fButton.AutoPostBack = AutoPostBack;
					fButton.Enabled = PopupEnabled;
					fButton.Visible = AllowEdit;

					((IButtonWithHelper)fButton).SetHelper(Helper);

					Controls.Add(Button);
				}
				return fButton;
			}
			set { fButton = value; }
		}
		ZFindBox fButton;

		#endregion

		#region Overriden Methods

		protected override void RenderChildren(HtmlTextWriter writer)
		{
			TextBox.CssClass = CssClass;
			Page.RenderPageControlBeginTag(writer, this);
			base.RenderChildren(writer);
			Page.RenderPageControlEndTag(writer, this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			//TextBox = new ZAutoCompleteTextBox();
			//TextBox.ID = "tb";
			//TextBox.Helper = Helper;
			//TextBox.BindTo = BindTo;
			//TextBox.BindTextTo = BindTextTo;
			//TextBox.TextChanged += new EventHandler(TextBox_TextChanged);
			//TextBox.AutoPostBack = AutoPostBack;

			//if (Helper.UsesGuidKey)
			//{
			//    Button = new AutoCompleteZGuidFindButton();
			//}
			//else
			//{
			//    Button = new AutoCompleteZFindButton();
			//}
			//((IButtonWithHelper)Button).AutoCompleteTextBox = this.TextBox;
			//Button.BindTo = BindTo;
			//Button.ModuleID = ModuleID;
			//Button.AutoPostBack = AutoPostBack;
			//Button.Enabled = PopupEnabled;

			this.Style.Add(HtmlTextWriterStyle.Display, "inline");
			this.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");

			EnsureChildControls();
			SetupPostBackEvents();
		}

		protected void InitControls()
		{
		}

		protected override void CreateChildControls()
		{
		}

		#endregion

		#region Properties

		public override string CssClass
		{
			get
			{
				return cssClass;
			}
			set
			{
				cssClass = value;
			}
		}
		string cssClass = "";

		protected new ZPage Page
		{
			get { return base.Page as ZPage; }
		}

		public string BindTextTo
		{
			get
			{
				return fBindTextTo;
			}
			set
			{
				fBindTextTo = value;
				if (TextBox != null)
				{
					TextBox.BindTextTo = value;
				}
			}
		}
		string fBindTextTo;

		public WebModuleID ModuleID
		{
			get
			{
				return fModuleID;
			}
			set
			{
				fModuleID = value;
				if (Button != null)
				{
					Button.ModuleID = ModuleID;
				}
			}
		}
		WebModuleID fModuleID;

		[Browsable(false)]
		public AutoCompleteHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = fHelper.GetDefault(ModuleID);
				}
				return fHelper;
			}
			set
			{
				fHelper = value;
				if (TextBox != null)
				{
					TextBox.Helper = value;
				}
				if (Button != null)
				{
					((IButtonWithHelper)Button).SetHelper(value);
				}
			}
		}
		AutoCompleteHelper fHelper;

		public bool AutoPostBack
		{
			get
			{
				return fAutoPostBack;
			}
			set
			{
				if (TextBox != null)
				{
					TextBox.AutoPostBack = value;
				}
				if (Button != null)
				{
					Button.AutoPostBack = value;
				}
				fAutoPostBack = value;
			}
		}
		bool fAutoPostBack;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public override Unit Width
		{
			get
			{
				return base.Width;
			}
			set
			{
				if (TextBox != null)
				{
					TextBox.Width = value;
				}
				if (value.Type == UnitType.Pixel)
				{
					Style["display"] = "block";
					base.Width = Unit.Pixel((int)value.Value + 25);
				}
			}
		}

		public bool PopupEnabled
		{
			get
			{
				return fPopupEnabled;
			}
			set
			{
				fPopupEnabled = value;
				if (Button != null)
				{
					Button.Enabled = value;
				}
			}
		}
		bool fPopupEnabled = true;

		public bool AllowEdit
		{
			get { return fAllowEdit; }
			set
			{
				TextBox.AllowEdit =
				Button.Visible =
				PopupEnabled =
				fAllowEdit = value;
			}
		}
		bool fAllowEdit = true;

		#endregion

		#region Events

		public event EventHandler TextChanged;

		#endregion

		#region Event Handlers

		void TextBox_TextChanged(object sender, EventArgs e)
		{
			if (TextChanged != null)
			{
				TextChanged(sender, e);
			}
		}

		#endregion

		#region ISelfBindingPostbackWebControl Members

		public bool HasChanges
		{
			get
			{
				return TextBox.HasChanges;
			}
			set
			{
				TextBox.HasChanges = value;
			}
		}

		#endregion

		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			EnsureChildControls();

			return TextBox.IsBindable(dataSource) && Button.IsBindable(dataSource);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public void Bind(object dataSource)
		{
			BusinessEntity = dataSource;
			EnsureChildControls();

			if (AllowEdit)
			{
				Button.Bind(dataSource);
			}
			TextBox.Bind(dataSource);

			Info = null;

			if (!string.IsNullOrEmpty(BindTo) && ZPropertyAccessor.Get(dataSource, BindTo + "Info") != null)
			{
				Info = ZPropertyAccessor.Get(dataSource, BindTo + "Info") as ZPropertyInfo;
			}
		}

		protected object BusinessEntity
		{
			get { return fBusinessEntity; }
			private set { fBusinessEntity = value; }
		}

		object fBusinessEntity;
		ZPropertyInfo Info;

		public void UnBind()
		{
			EnsureChildControls();

			TextBox.UnBind();
			Button.UnBind();
		}

		#endregion

		#region IBindTo Members

		public string BindTo
		{
			get
			{
				return fBindTo;
			}
			set
			{
				fBindTo = value;
				if (TextBox != null)
				{
					TextBox.BindTo = value;
				}
				if (Button != null)
				{
					Button.BindTo = value;
				}
			}
		}
		string fBindTo;

		#endregion

		#region IPostBackDataHandler Members

		protected void SetupPostBackEvents()
		{
			TextBox.PostDataChanged += new EventHandler(this.OnPostDataChanged);
			Button.PostDataChanged += new EventHandler(this.OnPostDataChanged);
		}

		public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			return TextBox.ProcessPostData(postDataKey, postCollection);
		}

		protected void OnPostDataChanged(object sender, EventArgs e)
		{
			RaisePostDataChangedEvent();
		}

		public void RaisePostDataChangedEvent()
		{
			if (IsBindable(BusinessEntity))
			{
				Bind(BusinessEntity);
			}
			if (PostDataChanged != null)
			{
				PostDataChanged(this, new EventArgs());
			}
		}

		public event EventHandler PostDataChanged;

		#endregion

		#region INotificationProvider

		public IEnumerable<INotification> Notifications
		{
			get { return Info != null ? Info.Notifications : NotificationCollection.Empty; }
		}

		bool INotificationProvider.HasNotifications()
		{
			return Info.HasNotifications();
		}

		bool INotificationProvider.HasNotifications(INotificationType type)
		{
			return Notifications.HasNotifications(type);
		}

		INotificationType INotificationProvider.GetHighestSeverityNotificationType()
		{
			return Info.GetHighestSeverityNotificationType();
		}

		#endregion
	}
}
