using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

[assembly: WebResource("ZTextBox/ZTextBoxButton.gif", "image/gif")]

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Abstract class for controls consisting of TextBox and Button
	/// </summary>	
	public abstract class ZTextBoxButton : WebControl, INamingContainer, INotificationProvider, IFocusCompositeControl, ISelfBindingPostbackWebControl, IContainResources, ITextChangedEvent, IUINotiicationsProvider, IWebNotificationProvider
	{
		public ZTextBoxButton()
			: base(HtmlTextWriterTag.Span)
		{
			Width = MinWidth;
			Height = ButtonWidth;
			this.displayNotifications = true;
		}

		#region Properties

		protected new ZPage Page
		{
			get { return base.Page as ZPage; }
		}

		/// <summary>
		/// The minimum width for the entire control
		/// </summary>
		protected virtual Unit MinWidth
		{
			get { return Unit.Pixel(120); }
		}

		protected virtual Unit ButtonWidth
		{
			get { return Unit.Pixel(25); }
		}

		protected virtual Unit ControlHeight
		{
			get { return Unit.Pixel(20); }
		}

		string fToolTip;
		public override string ToolTip
		{
			get { return fToolTip; }
			set
			{
				fToolTip = value;
				TextBoxControl.ToolTip = fToolTip;
			}
		}

		public override Unit Width
		{
			get
			{
				return base.Width;
			}
			set
			{
				if (value.Value == 0)
				{
					value = DefaultWidth;
				}

				if (value.Value < MinWidth.Value)
				{
					base.Width = MinWidth;
				}
				else
				{
					base.Width = value;
				}

				TextBoxControl.Style[HtmlTextWriterStyle.Width] = TextBoxWidth;
			}
		}

		[Browsable(false), Description("Height is currently set fixed at 20 pixel.")]
		public override Unit Height
		{
			get { return ControlHeight; }
			set
			{
				if (value.Value != ControlHeight.Value)
				{
					base.Height = ControlHeight;
				}
			}
		}

		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				base.Enabled = value;
				TextBoxControl.Enabled = value;
				ButtonControl.Disabled = !value;
			}
		}

		public bool ReadOnly
		{
			get { return TextBoxControl.ReadOnly; }
			set
			{
				TextBoxControl.ReadOnly = value;
				ButtonControl.Disabled = value;
			}
		}

		public bool DisplayNotifications
		{
			get
			{
				return displayNotifications;
			}
			set
			{
				displayNotifications = value;
			}
		}

		bool displayNotifications;

		public bool HasChanges
		{
			get { return TextBoxControl.HasChanges; }
			set { TextBoxControl.HasChanges = value; }
		}

		public string ButtonText
		{
			get { return ButtonControl.Value; }
			set { ButtonControl.Value = value; }
		}

		public string Text
		{
			get { return TextBoxControl.Text; }
			set { TextBoxControl.Text = value; }
		}

		public override FontInfo Font
		{
			get { return TextBoxControl.Font; }
		}

		protected virtual int MaxLength
		{
			get { return 0; }
		}

		public bool HideTextBox
		{
			get { return TextBoxControl.Style[HtmlTextWriterStyle.Display] == "none"; }
			set
			{
				if (value)
				{
					TextBoxControl.Style[HtmlTextWriterStyle.Display] = "none";
					TextBoxControl.Style[HtmlTextWriterStyle.Width] = Unit.Pixel(0).ToString();
					ButtonControl.Style[HtmlTextWriterStyle.Width] = Width.ToString();
				}
				else
				{
					TextBoxControl.Style[HtmlTextWriterStyle.Display] = null;
					TextBoxControl.Style[HtmlTextWriterStyle.Width] = TextBoxWidth;
					ButtonControl.Style[HtmlTextWriterStyle.Width] = ButtonWidth.ToString();
				}
			}
		}

		public ZTextBox TextBoxControl
		{
			get
			{
				if (fTextBoxControl == null)
				{
					fTextBoxControl = new ZTextBox();
					fTextBoxControl.ID = "TextBox";
					fTextBoxControl.Style[HtmlTextWriterStyle.Width] = TextBoxWidth;
					fTextBoxControl.ToolTip = this.ToolTip;
					fTextBoxControl.MaxLength = MaxLength;
					fTextBoxControl.Enabled = this.Enabled;
					fTextBoxControl.TextChanged += new EventHandler(this.TextBox_TextChanged);
					fTextBoxControl.PostDataChanged += new EventHandler(this.OnPostDataChanged);
				}
				return fTextBoxControl;
			}
		}
		ZTextBox fTextBoxControl;

		protected virtual HtmlInputButton ButtonControl
		{
			get
			{
				if (fButtonControl == null)
				{
					fButtonControl = new HtmlInputButton();
					fButtonControl.Style[HtmlTextWriterStyle.Width] = ButtonWidth.ToString();
					fButtonControl.Style[HtmlTextWriterStyle.Height] = ControlHeight.ToString();
					fButtonControl.Attributes.Add(nameof(HtmlTextWriterAttribute.Class), CssConstants.PopupButton);
					fButtonControl.Attributes.Add(nameof(HtmlTextWriterAttribute.Tabindex), "-1");
					fButtonControl.Disabled = !Enabled;
				}
				return fButtonControl;
			}
		}
		HtmlInputButton fButtonControl;

		protected virtual string ButtonBackgroundStyle
		{
			get
			{
				return String.Format("url({0}) {1} {2} {2}",
				  ZTextBoxButtonImage.FileName,
				  "no-repeat", "center");
			}
		}
		public virtual string TextBoxWidth
		{
			get { return Unit.Pixel(Convert.ToInt32(Width.Value - ButtonWidth.Value - 4)).ToString(); }
		}

		public virtual string PopupButtonWidth
		{
			get { return ButtonWidth.ToString(); }
		}

		[Browsable(true)]
		[Category("Behavior")]
		[DefaultValue(false)]
		public bool AutoPostBack
		{
			get { return TextBoxControl.AutoPostBack; }
			set { TextBoxControl.AutoPostBack = value; }
		}

		[DefaultValue("")]
		public string PostbackCondition
		{
			get { return TextBoxControl.PostbackCondition; }
			set { TextBoxControl.PostbackCondition = value; }
		}

		public virtual Unit DefaultWidth { get { return Unit.Pixel(120); } }

		[Browsable(false)]
		public IZType SelectedValue
		{
			get
			{
				EnsureChildControls();
				return GetSelectedValue();
			}
			set
			{
				UpdateControlWithNewSelectedValue(value);
			}
		}

		protected virtual void UpdateControlWithNewSelectedValue(IZType newValue)
		{
			string validatedValue = GetTextFromValue(newValue);
			if (validatedValue != null)
			{
				TextBoxControl.Text = validatedValue;
			}
			else
			{
				TextBoxControl.Text = (newValue != null) ? newValue.ToString() : "";
			}
		}

		protected char[] Whitespace = { '\r', '\n', ' ', '	' };

		protected abstract string GetTextFromValue(IZType newValue);
		protected abstract IZType GetSelectedValue();

		protected abstract string ButtonClickHandler { get; }

		#endregion

		#region Control overrides

		protected override void CreateChildControls()
		{
			Controls.Add(TextBoxControl);
			Controls.Add(ButtonControl);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			Style[HtmlTextWriterStyle.Display] = "inline";

			ButtonControl.Attributes["onclick"] = ButtonClickHandler;
			// This needs the page so must be done OnPreRender
			if (!string.IsNullOrEmpty(ButtonBackgroundStyle))
			{
				ButtonControl.Style.Add("background", ButtonBackgroundStyle);
			}
			else if (string.IsNullOrEmpty(ButtonText))
			{
				ButtonText = "...";
			}
		}

		public bool CanBeEnabledByClient { get; set; }

		public bool DataReadOnly => Info?.ReadOnly ?? false;

		public override void RenderControl(HtmlTextWriter writer)
		{
			if (!CanBeEnabledByClient && DataReadOnly)
			{
				var readOnlyLabel = new Label();
				readOnlyLabel.Text = Text;
				readOnlyLabel.Visible = Visible;
				readOnlyLabel.RenderControl(writer);
			}
			else
			{
				base.RenderControl(writer);
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			if (Page != null)
			{
				Page.RenderPageControlBeginTag(writer, this);
			}
			base.RenderBeginTag(writer);
		}

		public override void RenderEndTag(HtmlTextWriter writer)
		{
			base.RenderEndTag(writer);
			if (Page != null)
			{
				Page.RenderPageControlEndTag(writer, this);
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			EnsureChildControls();

			if (!RenderContentsOnly)
			{
				base.Render(writer);
			}
			else
			{
				RenderContents(writer);
			}
		}

		public bool RenderContentsOnly
		{
			get { return renderContentsOnly; }
			set { renderContentsOnly = value; }
		}
		bool renderContentsOnly;

		#endregion

		#region IBindTo Members

		string fBindTo = "";

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public virtual string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}

		#endregion

		#region ISelfBindingPostbackWebControl Members

		public virtual bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		object BusinessEntity
		{
			get { return fBusinessEntity; }
			set { fBusinessEntity = value; }
		}

		object fBusinessEntity;

		public virtual void UnBind()
		{
			BusinessEntity = null;
			BindTo = null;
			SelectedValue = null;
		}

		public void Bind(object dataSource)
		{
			EnsureChildControls();
			BusinessEntity = dataSource;
			BindCore(dataSource);
		}

		protected virtual void BindCore(object dataSource)
		{
			Info = (ZPropertyInfo)ZPropertyAccessor.Get(dataSource, BindTo + "Info");
			if (HasChanges)
			{
				ZPropertyAccessor.Set(dataSource, BindTo, SelectedValue);
				HasChanges = false;
			}
			else
			{
				object valueFromBizO = ZPropertyAccessor.Get(dataSource, BindTo);
				if (valueFromBizO != null)
				{
					if (((IZType)valueFromBizO).IsEmpty)
					{
						SelectedValue = null;
					}
					else
					{
						SelectedValue = (IZType)valueFromBizO;
					}
				}
				else
				{
					ErrorReporter.ReportOnce("ZWEBTEXTBOXBUTTONNULLVALUE", String.Format("Value from BizO is returned as NULL in property {0}", BindTo));
				}
			}

			int maximumInputLength = (int)ZPropertyAccessor.Get(dataSource, BindTo + "Info" + ".MaxLength");
			if (maximumInputLength > 0)
			{
				this.TextBoxControl.MaxLength = maximumInputLength;
			}
		}

		protected ZPropertyInfo Info;

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

		public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			return TextBoxControl.ProcessPostData(postDataKey, postCollection);
		}

		#endregion

		#region Child Event Handler

		public event EventHandler TextChanged;

		protected void OnTextChanged(EventArgs e)
		{
			if (TextChanged != null)
			{
				TextChanged(this, e);
			}
		}

		protected void OnPostDataChanged(object sender, EventArgs e)
		{
			RaisePostDataChangedEvent();
		}

		void TextBox_TextChanged(object sender, EventArgs e)
		{
			OnTextChanged(EventArgs.Empty);
		}

		public event EventHandler PostDataChanged;

		#endregion Child Event Handler

		#region IContainResources Members

		protected ZWebResource ZTextBoxButtonImage
		{
			get
			{
				return fZTextBoxButtonImage ?? (fZTextBoxButtonImage = new ZWebResource(typeof(ZTextBoxButton), "ZTextBoxButton.gif", Page, "Enterprise.ZArchitecture.Web.GUI.WebControls.ZTextBox"));
			}
		}
		ZWebResource fZTextBoxButtonImage;

		public virtual ZWebResourceCollection Resources
		{
			get { return new ZWebResourceCollection { ZTextBoxButtonImage }; }
		}

		#endregion

		#region IFocusCompositeControl Members

		public string ChildControlIDForFocus
		{
			get { return TextBoxControl.ClientID; }
		}

		#endregion

		#region IWebNotificationProvider Members

		public string NotificationID
		{
			get
			{
				return TextBoxControl.NotificationID;
			}
		}

		#endregion

		#region Internal Properties

		internal void EnsureChildControlsInternal() => EnsureChildControls();
		internal HtmlInputButton ButtonControlInternal => ButtonControl;
		internal Unit ButtonWidthInternal => ButtonWidth;
		internal Unit MinWidthInternal => MinWidth;
		internal Unit ControlHeightInternal => ControlHeight;
		internal void RenderInternal(HtmlTextWriter writer) => Render(writer);
		internal void OnPreRenderInternal(EventArgs e) => OnPreRender(e);

		#endregion
	}

	#endregion
}

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Design
{
	using System.IO;
	using System.Web.UI.Design;

	public class ZTextBoxButtonDesigner : ControlDesigner
	{
		public override string GetDesignTimeHtml()
		{
			using (StringWriter stringWriter = new StringWriter())
			{
				((ZTextBoxButton)Component).RenderControl(new HtmlTextWriter(stringWriter));
				return stringWriter.ToString();
			}
		}
	}
}
