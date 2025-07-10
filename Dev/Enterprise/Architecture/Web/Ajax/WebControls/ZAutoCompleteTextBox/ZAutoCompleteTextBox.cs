using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls.GuidTextBox.Internals;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	[DefaultProperty("Text"), ToolboxData("<{0}:ZAutoCompleteTextBox runat=server></{0}:ZAutoCompleteTextBox>")]
	public class ZAutoCompleteTextBox : TextBox, IContainResources, ISelfBindingPostbackWebControl
	{
		#region Properties

		[Browsable(false)]
		public AutoCompleteHelper Helper
		{
			get
			{
				return helper;
			}
			set
			{
				helper = value;
			}
		}
		AutoCompleteHelper helper;

		public string BindTextTo
		{
			get
			{
				return bindTextTo;
			}
			set
			{
				bindTextTo = value;
			}
		}
		string bindTextTo;

		public string ReadOnlyCssClass
		{ get; set; }

		#endregion

		#region GuidContainer

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			writer.AddAttribute("RelatedKeyContainer", KeyContainerID);
			base.AddAttributesToRender(writer);
		}

		public string KeyContainerID
		{
			get { return "AcmTBKeyContainer_" + ClientID; }
		}

		protected virtual string KeyContainerValue
		{
			get { return Page == null ? "" : Page.Request.Params[KeyContainerID]; }
		}

		#endregion

		#region Overriden Methods

		public override void RenderControl(HtmlTextWriter writer)
		{
			if (AllowEdit)
			{
				base.RenderControl(writer);
			}
			else
			{
				Label readOnlyLabel = new Label();
				readOnlyLabel.Text = Text;
				readOnlyLabel.Visible = Visible;
				readOnlyLabel.RenderControl(writer);
				readOnlyLabel.CssClass = ReadOnlyCssClass;
			}
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			Page.ZClientScript.RegisterHiddenField(KeyContainerID, ZString.Empty);//GetHiddenText());
		}

		protected new ZPage Page
		{
			get
			{
				return (ZPage)base.Page;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if (!ReadOnly && AllowEdit)
			{
				RegisterStyleSheet();
				RegisterIncludedScripts();
				RegisterStartupScripts();

				Style["background-image"] = string.Format("url({0})", AutoCompleterImage.FileName);
				Style["background-position"] = "right center";
				Style["background-repeat"] = "no-repeat";
			}
		}
		#endregion
		#region Register Scripts

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void RegisterStyleSheet()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(typeof(ZAutoCompleteTextBox), "AutoCompleterCSS"))
			{
				string link = string.Format("<link rel=\"stylesheet\" href=\"{0}\" type=\"text/css\">", AutoCompleterCSS.FileName);
				Page.ZClientScript.RegisterClientScriptBlock(typeof(ZAutoCompleteTextBox), "AutoCompleterCSS", link);
			}
		}

		void RegisterIncludedScripts()
		{
			if (!Page.ZClientScript.IsClientScriptIncludeRegistered("ObserverScript"))
			{
				Page.ZClientScript.RegisterClientScriptInclude("ObserverScript", ObserverScript.FileName);
			}

			if (!Page.ZClientScript.IsClientScriptIncludeRegistered("AutoCompleterScript"))
			{
				Page.ZClientScript.RegisterClientScriptInclude("AutoCompleterScript", AutoCompleterScript.FileName);
			}

			if (!Page.ZClientScript.IsClientScriptIncludeRegistered("AutoCompleterRequestScript"))
			{
				Page.ZClientScript.RegisterClientScriptInclude("AutoCompleterRequestScript", AutoCompleterRequestScript.FileName);
			}
		}

		void RegisterStartupScripts()
		{
			string script = GetStartupScript();

			ScriptManager sm = ScriptManager.GetCurrent(Page);
			if (sm != null && sm.IsInAsyncPostBack)
			{
				ScriptManager.RegisterStartupScript(this, typeof(ZAutoCompleteTextBox), this.ClientID, script, true);
			}
			else
			{
				script = "document.addEvent('domready', function() {" + script + "});";
				Page.ZClientScript.RegisterStartupScript(typeof(ZAutoCompleteTextBox), this.ClientID, script, true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected string GetStartupScript()
		{
			QueryParamsEncoder encoder = new QueryParamsEncoder();

			List<string> postDataArgs = new List<string>();
			postDataArgs.Add(string.Format("{0}:'{1}'", AutoCompleteTextBoxRequestHandler.Helper, encoder.Encrypt(Helper.GetType().AssemblyQualifiedName)));
			postDataArgs.Add(string.Format("{0}:'{1}'", AutoCompleteTextBoxRequestHandler.Params, encoder.Encrypt(Helper.SerializeAdditionalParamsToString())));
			postDataArgs.Add(string.Format("{0}:'{1}'", AutoCompleteTextBoxRequestHandler.MaxItemsCount, encoder.Encrypt(Helper.MaxOptionsCount.ToString())));

			List<string> options = new List<string>();
			options.Add("'postData': {" + string.Join(",", postDataArgs.ToArray()) + "}");

			if (AutoPostBack)
			{
				options.Add("'autoSubmit': true");
			}

			options.Add("'maxChoices': " + Helper.MaxOptionsCount.ToString());

			if (Helper.UsesMultiRowOptions)
			{
				string injectChoiceFunction =
@"function(choice) {
    var text = choice.getFirst();
    var value = text.innerHTML.replace(""&amp;"", ""&"");
    choice.inputValue = value;
    text.set('html', this.markQueryValue(value));
    this.addChoiceEvents(choice);
}";
				options.Add("'injectChoice': " + injectChoiceFunction);
			}

			if (this.Attributes["onChange"] != null)
			{
				options.Add("'complete': " + @"function() { " + this.Attributes["onChange"] + " }");
			}

			return string.Format("new Autocompleter.Request.HTML($('{0}'), '{1}', {{{2}}});",
					this.ClientID,
					ResolveClientUrl("~/" + AutoCompleteTextBoxRequestHandler.BaseUrl),
					string.Join(",", options.ToArray()));
		}

		#endregion

		#region IContainResources Members

		ZWebResourceCollection IContainResources.Resources
		{
			get
			{
				ZWebResourceCollection result = new ZWebResourceCollection();
				result.Add(ObserverScript);
				result.Add(AutoCompleterScript);
				result.Add(AutoCompleterRequestScript);
				result.Add(AutoCompleterCSS);
				result.Add(AutoCompleterImage);
				return result;
			}
		}

		protected ZWebResource AutoCompleterScript
		{
			get
			{
				return autoCompleterScript ?? (autoCompleterScript = new ZWebResource(typeof(ZAutoCompleteTextBox), "autocompleter.js", Page, "Enterprise.ZArchitecture.Web.GUI.Ajax.WebControls.ZAutoCompleteTextBox"));
			}
		}
		ZWebResource autoCompleterScript;

		protected ZWebResource AutoCompleterRequestScript
		{
			get
			{
				return autoCompleterRequestScript ?? (autoCompleterRequestScript = new ZWebResource(typeof(ZAutoCompleteTextBox), "autocompleter.request.js", Page, "Enterprise.ZArchitecture.Web.GUI.Ajax.WebControls.ZAutoCompleteTextBox"));
			}
		}
		ZWebResource autoCompleterRequestScript;

		protected ZWebResource ObserverScript
		{
			get
			{
				return observerScript ?? (observerScript = new ZWebResource(typeof(ZAutoCompleteTextBox), "observer.js", Page, "Enterprise.ZArchitecture.Web.GUI.Ajax.WebControls.ZAutoCompleteTextBox"));
			}
		}
		ZWebResource observerScript;

		protected ZWebResource AutoCompleterCSS
		{
			get
			{
				return autoCompleterCSS ?? (autoCompleterCSS = new ZWebResource(typeof(ZAutoCompleteTextBox), "autocompleter.css", Page, "Enterprise.ZArchitecture.Web.GUI.Ajax.WebControls.ZAutoCompleteTextBox"));
			}
		}
		ZWebResource autoCompleterCSS;

		protected ZWebResource AutoCompleterImage
		{
			get
			{
				return autoCompleterImage ?? (autoCompleterImage = new ZWebResource(typeof(ZAutoCompleteTextBox), "autocomplete.gif", Page, "Enterprise.ZArchitecture.Web.GUI.Ajax.WebControls.ZAutoCompleteTextBox"));
			}
		}
		ZWebResource autoCompleterImage;

		#endregion

		#region ISelfBindingPostbackWebControl Members

		public bool HasChanges { get; set; }

#endregion

		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public void Bind(object dataSource)
		{
			BusinessEntity = dataSource;

			if (HasChanges)
			{
				HasChanges = false;
				ZPropertyAccessor.Set(dataSource, BindTo, Key);
				if (!string.IsNullOrEmpty(BindTextTo))
				{
					ZPropertyAccessor.Set(dataSource, BindTextTo, Name);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(BindTo))
				{
					Key = (IZType)ZPropertyAccessor.Get(dataSource, BindTo);

					ZPropertyInfo info = (ZPropertyInfo)ZPropertyAccessor.Get(dataSource, BindTo + "Info");
					if (info != null)
					{
						ReadOnly = info.ReadOnly;
						Enabled = !info.ReadOnly;

						if (info.ReadOnly)
						{
							Style["background-color"] = "transparent";
							Style["border"] = "none";
						}
						else
						{
							Style.Remove("background-color");
							Style.Remove("border");
						}
					}
				}
				if (!string.IsNullOrEmpty(BindTextTo))
				{
					Name = (ZString)ZPropertyAccessor.Get(dataSource, BindTextTo);

					ZPropertyInfo textPropertyInfo = (ZPropertyInfo)ZPropertyAccessor.Get(dataSource, BindTextTo + "Info");
					if (textPropertyInfo != null && textPropertyInfo.MaxLength > 0)
					{
						MaxLength = textPropertyInfo.MaxLength;
					}
				}
			}
		}

		public void UnBind()
		{
			BindTo = "";
			BindTextTo = "";
			Key = null;
			Name = "";
		}

		#endregion

		#region IBindTo Members

		public virtual string BindTo
		{
			get { return bindTo; }
			set { bindTo = value; }
		}
		string bindTo = "";

		#endregion

		#region IPostBackDataHandler Members

		protected object BusinessEntity
		{
			get { return fBusinessEntity; }
			set { fBusinessEntity = value; }
		}

		object fBusinessEntity;

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

		public bool ProcessPostData(string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData(postDataKey, postCollection);
		}

		public event EventHandler PostDataChanged;

		protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			HasChanges = base.LoadPostData(postDataKey, postCollection);
			try
			{
				if (!string.IsNullOrEmpty(KeyContainerValue))
				{
					Regex r = new Regex(ZAutoCompleteTextWithGuidValueHelper.PKWithDivPattern, RegexOptions.IgnoreCase);
					Match m = r.Match(KeyContainerValue);
					if (m.Success)
					{
						Text = KeyContainerValue;
						HasChanges = true;
					}
				}
			}
			catch (RegexMatchTimeoutException) { }
			catch (ArgumentException) { }
			catch (HttpException) { }

			return HasChanges;
		}

		#endregion

		#region Implementation

		public bool AllowEdit
		{
			get { return fAllowEdit; }
			set { fAllowEdit = value; }
		}
		bool fAllowEdit = true;

		void UpdateTextValue()
		{
			if (Key == null || Key.IsEmpty)
			{
				if (string.IsNullOrEmpty(BindTextTo))
				{
					Name = "";
				}
			}
			else
			{
				Name = Helper.GetText(Key);

				if (Name == "")
				{
					Key = null;
				}
			}
		}

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value.GetValue();
				IZType keyFromValue = value.TryGetKey();
				if (keyFromValue != null && !keyFromValue.IsEmpty)
				{
					Key = keyFromValue;
				}
				if (Key.IsEmpty)
				{
					Key = Helper.GetKey(Text);
				}
			}
		}

		public ZString Code
		{
			get;
			set;
		}

		public ZGuid PK
		{
			get;
			set;
		}

		IZType Key
		{
			get
			{
				return Helper.UsesGuidKey ? PK : Code;
			}
			set
			{
				if (Helper.UsesGuidKey)
				{
					PK = value == null ? ZGuid.Empty : (ZGuid)value;
				}
				else
				{
					if (value is ZString)
					{
						Code = (ZString)value;
					}
					else
					{
						Code = ZString.Empty;
					}
				}

				UpdateTextValue();
			}
		}

		ZString Name
		{
			get
			{
				return Text;
			}
			set
			{
				base.Text = value;
			}
		}

		#endregion
	}
}
