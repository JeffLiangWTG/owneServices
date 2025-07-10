using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Base type for all textual boxes.
	/// </summary>
	public abstract class ZTextBoxBase : TextBox, IContainResources, INotificationProvider, ISelfBindingPostbackWebControl, ITextChangedEvent, IPostbackConditionControl, IWebNotificationProvider, ITextTransformer
	{
		public ZTextBoxBase()
		{
			IsReadOnlyAffectingEnabledFlagAndStyles = true;
		}

		#region Properties

		[DefaultValue(TextTransformOptions.None)]
		public TextTransformOptions TextTransform { get; set; }

		protected abstract IZType SelectedValue { get; set; }

		public bool HasChanges
		{
			get { return fHasChanges; }
			set { fHasChanges = value; }
		}
		bool fHasChanges;

		#region ZPage

		protected new ZPage Page
		{
			get { return base.Page as ZPage; }
		}

		#endregion

		#endregion Properties

		#region ISelfBindingPostbackWebControl Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public virtual string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo = "";

		public bool AllowEdit
		{
			get { return fAllowEdit; }
			set { fAllowEdit = value; }
		}
		bool fAllowEdit = true;

		public bool CanBeEnabledByClient { get; set; }

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		public void UnBind()
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

		public override string Text
		{
			get
			{
				var text = base.Text;
				if (MaxLength > 0 && text.Length > MaxLength)
				{
					return text.Substring(0, MaxLength);
				}

				return text;
			}
			set
			{
				base.Text = TransformText(value);
			}
		}

		string TransformText(string text)
		{
			if (TextTransform == TextTransformOptions.UpperCase)
			{
				return text.ToUpper(CultureInfo.CurrentCulture);
			}
			return text;
		}

		protected virtual void BindCore(object dataSource)
		{
			if (!string.IsNullOrWhiteSpace(BindTo))
			{
				fInfo = (ZPropertyInfo)ZPropertyAccessor.Get(dataSource, BindTo + "Info");
				if (fInfo != null)
				{
					int ptyInfoMaxLength = fInfo.MaxLength;

					if (ptyInfoMaxLength > 0)
					{
						MaxLength = ptyInfoMaxLength;
					}
					ReadOnly = fInfo.ReadOnly;
					if (IsReadOnlyAffectingEnabledFlagAndStyles)
					{
						if (fInfo.ReadOnly)
						{
							Enabled = false;
							Style["background-color"] = "transparent";
							Style["border"] = "none";
						}
						else
						{
							Enabled = true;
							Style["background-color"] = "";
							Style["border"] = "";
							Style.Remove("background-color");
							Style.Remove("border");
						}
					}
				}
				if (HasChanges)
				{
					HasChanges = false;
					ZPropertyAccessor.Set(dataSource, BindTo, SelectedValue);
				}
				else
				{
					var valueFromBizO = (IZType)ZPropertyAccessor.Get(dataSource, BindTo);
					if (valueFromBizO != null)
					{
						if (valueFromBizO.IsEmpty)
						{
							SelectedValue = null;
						}
						else
						{
							SelectedValue = valueFromBizO;
						}
					}
					else
					{
						ErrorReporter.ReportOnce("ZWEBTEXTBOXNULLVALUE", String.Format("Value from BizO is returned as NULL in property {0}", BindTo));
					}
				}
			}
		}

		public bool IsReadOnlyAffectingEnabledFlagAndStyles
		{
			get;
			set;
		}

		protected ZPropertyInfo Info
		{
			get { return fInfo; }
		}
		ZPropertyInfo fInfo;

		protected override void RaisePostDataChangedEvent()
		{
			base.RaisePostDataChangedEvent();
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

		public bool ProcessPostData(string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData(postDataKey, postCollection);
		}

		protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			HasChanges = base.LoadPostData(postDataKey, postCollection);
			return fHasChanges;
		}

		internal object BusinessEntity
		{
			get { return fBusinessEntity; }
			set { fBusinessEntity = value; }
		}

		object fBusinessEntity;

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

		#region IPostbackCondition Members

		[DefaultValue("")]
		public string PostbackCondition
		{
			get
			{
				return (fPostbackCondition == null ? "" : fPostbackCondition.Trim());
			}
			set
			{
				fPostbackCondition = value;
			}
		}
		string fPostbackCondition;

		#endregion

		#region Overrides

		protected override void OnPreRender(EventArgs e)
		{
			if (TextTransform == TextTransformOptions.UpperCase)
			{
				CssClass = string.Format("{0} UpperCase", CssClass).Trim();
			}
			base.OnPreRender(e);
			if (!string.IsNullOrEmpty(ValidationPattern))
			{
				if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), ValidationScriptKey))
				{
					Page.ZClientScript.RegisterClientScriptBlock(GetType(), ValidationScriptKey, ValidationScriptBlock);
				}
				this.Attributes.Add("onblur", ValidationFunction);
			}
			this.Attributes.Add("onchange", "ClearValidation('" + NotificationID + "')" + (this.Attributes["onchange"] != null ? ";" + this.Attributes["onchange"] : ""));
		}

		public override void RenderControl(HtmlTextWriter writer)
		{
			if (!CanBeEnabledByClient && ((Info?.ReadOnly ?? false) || !AllowEdit))
			{
				var readOnlyLabel = new Label();
				readOnlyLabel.Text = Text;
				readOnlyLabel.Visible = Visible;
				readOnlyLabel.ID = ClientID;
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

		#endregion

		#region Validation

		protected string ValidationScriptBlock
		{
			get { return string.Format("<SCRIPT src='{0}'></SCRIPT>", ValidationScriptFile.FileName); }
		}

		protected virtual string ValidationScriptKey
		{
			get { return "ZTextBoxBase_ValidationScript"; }
		}

		protected virtual string ValidationPattern
		{
			get { return ""; }
		}

		protected string ValidationFunction
		{
			get { return String.Format("{0}(this, '{1}', '{2}')", ValidationFunctionName, ValidationPattern, ValidationMessage); }
		}

		protected virtual string ValidationFunctionName => "ValidateUserInput";

		protected virtual string ValidationMessage => "Invalid characters have been entered";

		ZWebResource ValidationScriptFile => validationScriptFile ?? (validationScriptFile = GetNewValidationScriptFile());
		ZWebResource validationScriptFile;

		protected virtual ZWebResource GetNewValidationScriptFile() => new ZWebResource(typeof(ZTextBoxBase), "ZTextBoxValidation.js", Page, "Enterprise.ZArchitecture.Web.GUI.WebControls.ZTextBox");

		#endregion

		#region IContainResources Members

		public ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = new ZWebResourceCollection();
				result.Add(ValidationScriptFile);
				return result;
			}
		}

		#endregion

		#region IWebNotificationProvider Members

		public string NotificationID
		{
			get
			{
				return ClientID + "_NotificationID";
			}
		}

		#endregion

		#region Internal Properties

		internal string ValidationPatternInternal => ValidationPattern;
		internal string ValidationScriptKeyInternal => ValidationScriptKey;
		internal void OnPreRenderInternal(EventArgs e) => OnPreRender(e);
		internal void RaisePostDataChangedEventInternal() => RaisePostDataChangedEvent();
		internal IZType SelectedValueInternal
		{
			get { return SelectedValue; }
			set { SelectedValue = value; }
		}

		#endregion
	}

	#endregion
}
