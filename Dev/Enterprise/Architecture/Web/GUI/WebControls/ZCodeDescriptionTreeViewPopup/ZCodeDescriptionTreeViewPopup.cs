using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.Internal;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Web Control that displays a code and description and presents a TreeView popup for selection.
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZCodeDescriptionTreeViewPopup runat=server />"),
	Designer(typeof(Design.ZCodeDescriptionTreeViewPopupDesigner))]
	public class ZCodeDescriptionTreeViewPopup : ZTextIFramePopup
	{
		readonly int FixedHeight = 25;

		public ZCodeDescriptionTreeViewPopup()
			: base()
		{
			Width = MinWidth;
			Height = Unit.Pixel(FixedHeight);
		}

		#region ModuleID

		public const string ModuleIDQuery = "ZCodeDescriptionTreeViewPopup_ModuleID";
		public const string SelectedValueQuery = "ZCodeDescriptionTreeViewPopup_SelectedValue";

		protected WebModuleID fModuleID = WebModuleIDs.NotAssigned;
		[Category(ZGUIConstants.DesignerCategory)]
		[Editor(typeof(WebModuleIDEditor), typeof(UITypeEditor))]
		public virtual WebModuleID ModuleID
		{
			get { return fModuleID; }
			set { fModuleID = value; }
		}

		// Do *not* remove this method!
		// .NET will use this method to determine whether or not to serialize the ModuleID property value.
		bool ShouldSerializeModuleID()
		{
			return (ModuleID != WebModuleIDs.NotAssigned);
		}

		#endregion ModuleID	

		#region Module Helpers

		ZTreeViewModule fModule;
		protected ZTreeViewModule Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = ZWebModuleFactory.Create(ModuleID, Factory) as ZTreeViewModule;
					if (fModule == null)
					{
						throw new ZException("Module ID : " + ModuleID.ToString() + " resolves to null. Web CodeDescriptionTreeView Modules must be of type ZTreeViewModule.");
					}
				}
				return fModule;
			}
		}

		BusinessObjectFactory fFactory;
		public BusinessObjectFactory Factory
		{
			get
			{
				if (Page is ZPage)
				{
					return Page.Factory;
				}
				else
				{
					if (fFactory == null)
					{
						fFactory = new BusinessObjectFactory();
					}
					return fFactory;
				}
			}
		}

		#endregion Module Helpers

		#region Properties 

		bool fIsValidCode;
		protected bool IsValidCode
		{
			get { return fIsValidCode; }
			set { fIsValidCode = value; }
		}

		protected override IZType GetSelectedValue()
		{
			ZString unformattedText = TextBoxControl.Text.Trim(Whitespace);
			ZString formattedCode = Module.FormatCode(unformattedText);
			TextBoxControl.Text = formattedCode;
			DescriptionBoxControl.Text = Module.GetDescriptionForCode(TextBoxControl.Text, true);
			return formattedCode;
		}

		protected override string GetTextFromValue(IZType newValue)
		{
			if (newValue is ZString)
			{
				ZString valueString = (ZString)newValue;
				ZString unformattedText = valueString.Trim(Whitespace);
				return Module.FormatCode(unformattedText);
			}
			return null;
		}

		protected override void UpdateControlWithNewSelectedValue(IZType newValue)
		{
			base.UpdateControlWithNewSelectedValue(newValue);
			DescriptionBoxControl.Text = Module.GetDescriptionForCode(TextBoxControl.Text, true);
		}

		public string Description
		{
			get { return DescriptionBoxControl.Text; }
		}

		protected override string IFrameSourcePageName
		{
			get { return "ZCodeDescriptionTreeViewPage.aspx"; }
		}

		protected override Unit MinWidth
		{
			get { return Unit.Pixel(200); }
		}

		protected override Unit PopupHeight
		{
			get { return Unit.Pixel(500); }
		}

		protected override Unit PopupWidth
		{
			get { return Unit.Pixel(500); }
		}

		public override string PopupButtonWidth
		{
			get { return ButtonWidth.Value.ToString(); }
		}

		public override string TextBoxWidth
		{
			get { return TextFieldWidth.Value.ToString(); }
		}

		protected override Unit ButtonWidth
		{
			get { return Unit.Pixel(25); }
		}

		protected Unit TextFieldWidth
		{
			get
			{
				return Convert.ToInt32((this.Width.Value - ButtonWidth.Value) * 0.3);
			}
		}

		protected string DescriptionBoxWidth
		{
			get { return DescriptionFieldWidth.Value.ToString(); }
		}

		protected Unit DescriptionFieldWidth
		{
			get
			{
				return new Unit(this.Width.Value - ButtonWidth.Value - TextFieldWidth.Value);
			}
		}

		ZPage BasePage
		{
			get { return Page; }
		}

		string fDescriptionToolTip;
		public string DescriptionToolTip
		{
			get { return fDescriptionToolTip; }
			set { fDescriptionToolTip = value; }
		}

		public override Unit Width
		{
			get
			{
				return base.Width;
			}
			set
			{
				if (value.Value < MinWidth.Value)
				{
					base.Width = MinWidth;
				}
				else
				{
					base.Width = value;
				}

				EnsureChildControls();
				TextBoxControl.Style["WIDTH"] = TextBoxWidth;
				DescriptionBoxControl.Style["WIDTH"] = DescriptionBoxWidth;
			}
		}

		protected string CodeDescriptionOKFunction
		{
			get
			{
				EnsureChildControls();
				return string.Format("ZCodeDescriptionTreeViewPopup_ProcessSelection");
			}
		}

		protected override string AdditionalButtonClickHandler
		{
			get
			{
				EnsureChildControls();
				return string.Format("ZCodeDescriptionTreeViewPopup_ShowPopup('{0}')", DescriptionBoxControl.ClientID);
			}
		}

		#endregion Properties

		#region Control Overrides

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			ZTextBox descriptionBox = new ZTextBox();
			Controls.Add(descriptionBox);
			descriptionBox.ID = "DescriptionBox";
			descriptionBox.Style["WIDTH"] = TextBoxWidth;
			descriptionBox.ToolTip = this.DescriptionToolTip;
			descriptionBox.ReadOnly = true;
			descriptionBox.TabIndex = -1;
			descriptionBox.Enabled = this.Enabled;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			EnsureChildControls();
		}

		protected ZTextBox DescriptionBoxControl
		{
			get
			{
				EnsureChildControls();
				return (ZTextBox)Controls[2];
			}
		}

		protected override void RenderScriptBlocks()
		{
			base.RenderScriptBlocks();
			if (BasePage != null)
			{
				if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), "ZCodeDescriptionTreeViewPopup_ScriptBlock"))
				{
					string scriptString = string.Format("<SCRIPT src='{0}'></SCRIPT>", ScriptBlock.FileName);
					Page.ZClientScript.RegisterClientScriptBlock(GetType(), "ZCodeDescriptionTreeViewPopup_ScriptBlock", scriptString);
				}
			}
		}

		NameValueCollection fAdditionalParameters;
		protected override NameValueCollection AdditionalParameters
		{
			get
			{
				if (fAdditionalParameters == null)
				{
					fAdditionalParameters = base.AdditionalParameters;
					fAdditionalParameters.Add(ModuleIDQuery, ModuleID.ToString());
					fAdditionalParameters.Add(SelectedValueQuery, SelectedValue.ToString());
				}
				return fAdditionalParameters;
			}
		}

		protected override string OKFunctionName
		{
			get
			{
				return CodeDescriptionOKFunction;
			}
		}

		#endregion Control Overrides

		#region Resources

		protected ZWebResource ScriptBlock
		{
			get
			{
				if (fScriptBlock == null)
				{
					fScriptBlock = new ZWebResource(typeof(ZCodeDescriptionTreeViewPopup), "ZCodeDescriptionScriptBlock.js", BasePage);
				}

				return fScriptBlock;
			}
		}
		ZWebResource fScriptBlock;

		protected ZWebResource ASPXPage
		{
			get
			{
				if (fASPXPage == null)
				{
					fASPXPage = new ZWebResource(typeof(ZCodeDescriptionTreeViewPopup), IFrameSourcePageName, BasePage);
				}

				return fASPXPage;
			}
		}
		ZWebResource fASPXPage;

		protected ZWebResource StyleSheet
		{
			get
			{
				if (fStyleSheet == null)
				{
					fStyleSheet = new ZWebResource(typeof(ZCodeDescriptionTreeViewPopup), "ZCodeDescription.css", BasePage);
				}

				return fStyleSheet;
			}
		}
		protected ZWebResource fStyleSheet;

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(ScriptBlock);
				result.Add(StyleSheet);
				result.Add(ASPXPage);
				return result;
			}
		}

		#endregion
	}

	#endregion
}

#region ZCodeDescriptionTreeViewPopupDesigner

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Design
{
	using System.IO;
	using System.Web.UI.Design;

	public class ZCodeDescriptionTreeViewPopupDesigner : ControlDesigner
	{
		public override string GetDesignTimeHtml()
		{
			using (StringWriter stringWriter = new StringWriter())
			{
				HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
				ZCodeDescriptionTreeViewPopup control = (ZCodeDescriptionTreeViewPopup)this.Component;
				control.RenderControl(htmlWriter);
				return stringWriter.ToString();
			}
		}
	}
}

#endregion ZCodeDescriptionTreeViewPopupDesigner
