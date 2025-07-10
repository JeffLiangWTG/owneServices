using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// A collapsable Panel to optimise screen space
	/// </summary>
	[DefaultProperty("Show"), ToolboxData("<{0}:ZCollapsablePanel runat=server></{0}:ZCollapsablePanel>")]
	[Designer(typeof(PanelContainerDesigner))]
	public class ZCollapsablePanel : Panel, IContainResources, INamingContainer
	{
		public ZCollapsablePanel()
		{
			this.EnableViewState = true;
		}

		#region Control Overrides

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			BaseControls.Add(DivAroundControl);
			DivAroundControl.Controls.Add(ContentsTable);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), "ZCollapsablePanel_ScriptKey"))
			{
				Page.ZClientScript.RegisterClientScriptBlock(GetType(), "ZCollapsablePanel_ScriptKey", ShowHideScriptBlock);
			}
			Page.ZClientScript.RegisterHiddenField(ShowHideID, ShowHide);
			OnPreRenderInternal();
		}

		string ShowHideID
		{
			get { return "ShowHideValue" + this.ClientID; }
		}

		string ShowHide
		{
			get
			{
#if DEBUG
				if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
				{
					return false.ToString();
				}
				else
#endif
				{
					return Page.Request.Params[ShowHideID];
				}
			}
		}

		protected internal void OnPreRenderInternal()
		{
			LabelControl.Text = Label;
			LabelControl.CssClass = CssClass;
			LabelControl.Attributes.Add("unselectable", "on");
			if (!DisableCollapsing)
			{
				LabelControl.Attributes.Add("ondblclick", ShowHideClickHandler);

				ExpandCollapseIcon.Style.Add("cursor", "hand");
				ExpandCollapseIcon.Attributes.Add("onclick", ShowHideClickHandler);
			}

			bool isShown;
			if (string.IsNullOrEmpty(ShowHide))
			{
				isShown = Show;
			}
			else
			{
				isShown = ShowHide == ZBool.True.ToString();
			}

			CollapsablePanel.Style["display"] = isShown ? "inline" : "none";
			if (!DisableCollapsing)
			{
				ExpandCollapseIcon.ImageUrl = isShown ? CollapseIcon.FileName : ExpandIcon.FileName;
			}
		}

		public override ControlCollection Controls
		{
			get
			{
				EnsureChildControls();
				return CollapsablePanel.Controls;
			}
		}

		protected internal ControlCollection BaseControls
		{
			get { return base.Controls; }
		}

		#endregion Control Overrides

		#region Controls

		#region ContentsTable

		protected internal HtmlGenericControl DivAroundControl
		{
			get
			{
				if (fDivAroundControl == null)
				{
					fDivAroundControl = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
					fDivAroundControl.Attributes.Add("Class", CssConstants.ContentSection);
				}
				return fDivAroundControl;
			}
		}
		protected HtmlGenericControl fDivAroundControl;

		protected internal Table ContentsTable
		{
			get
			{
				if (fContentsTable == null)
				{
					fContentsTable = new Table();
					fContentsTable.CssClass = CssConstants.ResultsTable;
					//					fContentsTable.BorderStyle = BorderStyle.None;
					//					fContentsTable.CellPadding = 0;

					TableRow firstRow = new TableRow();
					TableRow secondRow = new TableRow();

					if (!DisableCollapsing)
					{
						TableCell iconCell = new TableCell();
						iconCell.Controls.Add(ExpandCollapseIcon);
						firstRow.Cells.Add(iconCell);
						secondRow.Cells.Add(new TableCell());
					}
					TableCell labelCell = new TableCell();
					labelCell.Controls.Add(LabelControl);
					firstRow.Cells.Add(labelCell);
					fContentsTable.Rows.Add(firstRow);

					TableCell panelCell = new TableCell();
					panelCell.Controls.Add(CollapsablePanel);
					//PanelCell.Controls.Add(HiddenTextBox);

					secondRow.Cells.Add(panelCell);
					fContentsTable.Rows.Add(secondRow);
				}
				return fContentsTable;
			}
		}
		Table fContentsTable;

		#endregion ContentsTable

		#region LabelControl

		protected internal ZTextLabel LabelControl
		{
			get
			{
				if (fLabelControl == null)
				{
					fLabelControl = new ZTextLabel();
					fLabelControl.ID = "Label";
				}
				return fLabelControl;
			}
		}
		ZTextLabel fLabelControl;

		#endregion fLabelControl

		#region ExpandCollapseIcon

		protected internal Image ExpandCollapseIcon
		{
			get
			{
				if (fExpandCollapseIcon == null)
				{
					fExpandCollapseIcon = new Image();
					fExpandCollapseIcon.ID = "ExpandCollapseIcon";
				}
				return fExpandCollapseIcon;
			}
		}
		Image fExpandCollapseIcon;

		#endregion ShowHideIcon;

		#region CollapsablePanel

		protected internal Panel CollapsablePanel
		{
			get
			{
				if (fCollapsablePanel == null)
				{
					fCollapsablePanel = new Panel();
					fCollapsablePanel.ID = "Panel";
					fCollapsablePanel.CssClass = CssConstants.ZCollapsablePanel;
				}
				return fCollapsablePanel;
			}
		}
		Panel fCollapsablePanel;

		#endregion CollapsablePanel

		#endregion Controls

		#region Properties

		#region ZPage

		protected new ZPage Page
		{
			get { return base.Page as ZPage; }
		}
		#endregion

		#region CssStyle

		public override string CssClass
		{
			get
			{
				return ZCssHelper.Join(CssConstants.ZCollapsablePanelLabel, LabelControl.CssClass);
			}
			set
			{
				LabelControl.CssClass = value;
			}
		}

		#endregion CssStyle

		#region Label

		[Bindable(true),
		Category("Appearance")]
		public string Label
		{
			get
			{
				object obj = this.ViewState["ZCollapsablePanel_Label"];
				return (obj != null) ? (string)obj : "";
			}
			set { this.ViewState["ZCollapsablePanel_Label"] = value; }
		}

		#endregion Label

		#region DisableCollapsing

		[Bindable(true),
		Category("Appearance")]
		public bool DisableCollapsing
		{
			get
			{
				return fDisableCollapsing;
			}
			set
			{
				fDisableCollapsing = value;
				if (value)
				{
					this.ViewState["ZCollapsablePanel_Show"] = value;
				}
			}
		}
		bool fDisableCollapsing;

		#endregion

		#region Show

		[Bindable(true),
		Category("Appearance")]
		public bool Show
		{
			get
			{
				bool result = DisableCollapsing;
				if (!result)
				{
					object obj = this.ViewState["ZCollapsablePanel_Show"];
					result = !(obj != null) || (bool)obj;
				}
				return result;
			}
			set
			{
				this.ViewState["ZCollapsablePanel_Show"] = value || DisableCollapsing;
			}
		}

		#endregion Show

		#region ShowHideClickHandler

		protected internal ZString ShowHideClickHandler
		{
			get
			{
				return ZString.Format("ZCollapsablePanel_ExpandCollapse('{0}', '{1}', '{2}', '{3}', '{4}')", CollapsablePanel.ClientID, ExpandCollapseIcon.ClientID, ExpandIcon.FileName, CollapseIcon.FileName, ShowHideID);
			}
		}

		#endregion ShowHideClickHandler

		#region ShowHideScriptBlock

		protected internal string ShowHideScriptBlock
		{
			get { return String.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", ShowHideScript.FileName); }
		}

		#endregion ShowHideScriptBlock

		#endregion Properties

		#region IContainResources Members

		protected internal ZWebResource ExpandIcon
		{
			get
			{
				if (fExpandIcon == null)
				{
					fExpandIcon = new ZWebResource(typeof(ZCollapsablePanel), "plus.bmp", BasePage);
				}

				return fExpandIcon;
			}
		}
		ZWebResource fExpandIcon;

		protected internal ZWebResource CollapseIcon
		{
			get
			{
				if (fCollapseIcon == null)
				{
					fCollapseIcon = new ZWebResource(typeof(ZCollapsablePanel), "minus.bmp", BasePage);
				}

				return fCollapseIcon;
			}
		}
		ZWebResource fCollapseIcon;

		protected internal ZWebResource ShowHideScript
		{
			get
			{
				if (fShowHideScript == null)
				{
					fShowHideScript = new ZWebResource(typeof(ZCollapsablePanel), "ZCollapsablePanelScript.js", BasePage);
				}

				return fShowHideScript;
			}
		}
		ZWebResource fShowHideScript;

		public ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = new ZWebResourceCollection();
				result.Add(ShowHideScript);
				result.Add(ExpandIcon);
				result.Add(CollapseIcon);
				return result;
			}
		}

		ZPage BasePage
		{
			get { return Page; }
		}

		#endregion IContainResources members
	}

	#endregion
}
