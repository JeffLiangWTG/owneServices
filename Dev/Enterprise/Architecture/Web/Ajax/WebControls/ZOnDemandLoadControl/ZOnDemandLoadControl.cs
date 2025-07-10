using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	[DefaultProperty("CaptionCollapsed"), ToolboxData("<{0}:ZOnDemandLoadControl runat=server></{0}:ZOnDemandLoadControl>")]
	public class ZOnDemandLoadControl : HtmlGenericControl, ISelfBindingWebControl, INamingContainer
	{
		#region Constructors

		public ZOnDemandLoadControl()
			: base(nameof(HtmlTextWriterTag.Div))
		{
		}

		public ZOnDemandLoadControl(string tag)
			: base(nameof(HtmlTextWriterTag.Div))
		{
		}

		#endregion

		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html id suffix")]
		public static class Constants
		{
			public const string OnDemandHeaderID = "Header";
			public const string OnDemandContainerID = "Intrnals";
			public const string HeaderButtonsHolderID = "Buttons";
			public const string ControlButtonID = "ControlButton";
			public const string HiddenStateFieldID = "State";
			public const string UpdatePanelID = "UpdatePanelID";
		}

		#endregion

		#region Properties

		#region Parent Control

		[DefaultValue("")]
		public string ParentControlID { get; set; }

		#endregion

		#region ControlButton

		public string ControlButtonText
		{
			get { return controlButtonText; }
			set { controlButtonText = value; }
		}

		LinkButton ControlButton
		{
			get
			{
				if (controlButton == null)
				{
					controlButton = new LinkButton();
					controlButton.ID = GetControlIDWithPrefix(Constants.ControlButtonID);
					controlButton.PostBackUrl = string.Empty;
				}
				return controlButton;
			}
		}

		#endregion

		#region ShowControl

		public bool ShouldShowControl
		{
			get
			{
				return shouldShowControl;
			}
			set
			{
				shouldShowControl = value;
			}
		}

		bool shouldShowControl = true;

		#endregion

		#region Collapsing

		public bool IsCollapsed
		{
			get
			{
				if (isCollapsed.HasValue)
				{
					return isCollapsed.Value;
				}
				else if (IsCollapsingDisabled)
				{
					return false;
				}

				var state = HttpContext.Current?.Request.Params[StateControlID];

				return !bool.TryParse(state, out var result) || result;
			}
			set
			{
				isCollapsed = value;
			}
		}
		bool? isCollapsed;

		public bool IsCollapsingDisabled
		{
			get { return isCollapsingDisabled; }
			set { isCollapsingDisabled = value; }
		}

		string StateControlID
		{
			get { return GetControlIDWithPrefix(string.Concat(ParentControlID, Constants.HiddenStateFieldID)); }
		}

		#endregion

		#region UpdatePanel

		public bool UseUpdatePanel
		{
			get { return useUpdatePanel; }
			set { useUpdatePanel = value; }
		}
		bool useUpdatePanel = true;

		public UpdatePanel Panel
		{
			get
			{
				if (panel == null)
				{
					panel = new UpdatePanel();
					panel.ID = GetControlIDWithPrefix(Constants.UpdatePanelID);
				}
				return panel;
			}
		}

		public UpdatePanelTriggerCollection Triggers
		{
			get { return Panel.Triggers; }
		}

		public UpdatePanelUpdateMode UpdateMode
		{
			get { return Panel.UpdateMode; }
			set { Panel.UpdateMode = value; }
		}

		#endregion

		#region Containers

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html   tag")]
		public HtmlGenericControl Header
		{
			get
			{
				if (onDemandHeader == null)
				{
					onDemandHeader = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
					onDemandHeader.ID = GetControlIDWithPrefix(Constants.OnDemandHeaderID);
					onDemandHeader.Attributes.Add("Class", "OnDemandHeader");
				}
				return onDemandHeader;
			}
		}

		[DefaultValue(false)]
		public bool AllowCollapseOfAlwaysVisibleHolder { get; set; }

		public HtmlGenericControl AlwaysVisibleHolder
		{
			get
			{
				HtmlGenericControl result = null;
				if (HideButtonsToMenu)
				{
					if (visibleHolder == null)
					{
						visibleHolder = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
					}
					result = visibleHolder;
				}
				return result ?? MenuItems;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html   tag")]
		public HtmlGenericControl MenuItems
		{
			get
			{
				if (buttonsHolder == null)
				{
					buttonsHolder = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
					buttonsHolder.ID = GetControlIDWithPrefix(Constants.HeaderButtonsHolderID);
					buttonsHolder.Visible = false;
					if (HideButtonsToMenu)
					{
						buttonsHolder.Attributes.Add("Class", "OnDemandButtonHolder");
					}
				}
				return buttonsHolder;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html   tag")]
		public HtmlGenericControl Container
		{
			get
			{
				if (onDemandContainer == null)
				{
					onDemandContainer = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
					onDemandContainer.ID = GetControlIDWithPrefix(Constants.OnDemandContainerID);
					onDemandContainer.Attributes.Add("Class", CssConstants.ContentSection);

					if (HideButtonsToMenu)
					{
						onDemandContainer.Controls.Add(MenuControl);
						onDemandContainer.Controls.Add(AlwaysVisibleHolder);
					}

					if (UseUpdatePanel)
					{
						onDemandContainer.Controls.Add(Panel);
					}
				}
				return onDemandContainer;
			}
		}

		#endregion

		#region Menu

		#region HideButtonsToMenu

		public bool HideButtonsToMenu
		{
			get { return hideButtonsToMenu; }
			set { hideButtonsToMenu = value; }
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html   tag")]
		public HtmlGenericControl MenuControl
		{
			get
			{
				if (menuControl == null)
				{
					menuControl = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
					menuControl.ID = GetControlIDWithPrefix("Menu");
					menuControl.Attributes.Add("Class", "OnDemandMenuCaller");
					menuControl.Visible = false;
					menuButton = new Button() { CssClass = "OnDemandMenuCallerButton", ToolTip = "Menu" };
					menuControl.Controls.Add(menuButton);
				}
				return menuControl;
			}
		}
		Button menuButton;
		HtmlGenericControl menuControl;

		#endregion

		#endregion

		#region Implementation

		string controlButtonText = "";
		bool isCollapsingDisabled;
		bool hideButtonsToMenu = true;

		HtmlGenericControl visibleHolder;
		HtmlGenericControl buttonsHolder;
		HtmlGenericControl onDemandHeader;
		HtmlGenericControl onDemandContainer;

		LinkButton controlButton;
		UpdatePanel panel;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "JavaScript code, const more than 10 smbls, javascript - more than 10 smbls")]
		protected virtual void UpdateState()
		{
			if (HideButtonsToMenu && ShouldShowControl)
			{
				MenuControl.Visible = !IsCollapsed;
				Container.Attributes.Add("onmouseover", string.Format("SetCustomizeMenuVisibility(event, '{0}', '{1}');", menuButton.ClientID, MenuItems.ClientID));
																																																																														 //				menuButton.Attributes.Add("onmouseover", string.Format("SetCustomizeMenuVisibility(event, '{0}', '{1}', '{2}', '{3}');", menuButton.ClientID, menuButton.ClientID, MenuItems.ClientID, Container.ClientID));
																																																																														 //				menuButton.Attributes.Add("onmouseout", string.Format("SetCustomizeMenuVisibility(event, '{0}', '{1}', '{2}', '{3}');", menuButton.ClientID, menuButton.ClientID, MenuItems.ClientID, Container.ClientID));
				menuButton.OnClientClick = string.Format("FadeIn('{0}'); return false;", MenuItems.ClientID);
																																																			//Container.Attributes.Add("onmouseover", string.Format("SetCustomizeMenuVisibility(event, '{0}', '{1}', '{2}', '{3}');", Container.ClientID, menuButton.ClientID, MenuItems.ClientID, Container.ClientID));
																																																			//Container.Attributes.Add("onmouseout", string.Format("SetCustomizeMenuVisibility(event, '{0}', '{1}', '{2}', '{3}');", Container.ClientID, menuButton.ClientID, MenuItems.ClientID, Container.ClientID));
																																																			//MenuItems.Attributes.Add("onmouseout", string.Format("SetCustomizeMenuVisibility(event, '{0}', '{1}', '{2}', '{3}');", MenuItems.ClientID, menuButton.ClientID, MenuItems.ClientID, Container.ClientID));
																																																			//MenuItems.Attributes.Add("onmouseover", string.Format("SetCustomizeMenuVisibility(event, '{0}', '{1}', '{2}', '{3}');", MenuItems.ClientID, menuButton.ClientID, MenuItems.ClientID, Container.ClientID));
			}
			MenuItems.Visible = !IsCollapsed;

			if (!string.IsNullOrEmpty(ControlButtonText))
			{
				if (IsCollapsingDisabled)
				{
					Header.Controls.Add(new Label() { Text = ControlButtonText, CssClass = CssConstants.SectionTitle });
				}
				else
				{
					Header.Controls.Add(ControlButton);
					ControlButton.Text = string.Format("{0} {1}", IsCollapsed ? Res.GetString("6AFD5C57-B8F1-4475-A0EA-BF84E4544309", "Show") : Res.GetString("30877434-832F-4DAE-B692-D918626061FC", "Hide"), ControlButtonText);
					ControlButton.Attributes.Add("onClick", string.Format("javascript:$('{0}').value = '{1}'; __doPostBack('{0}','{1}');", StateControlID, !IsCollapsed));
				}
			}
		}

		public ControlCollection ControlsToRender
		{
			get { return UseUpdatePanel ? Panel.ContentTemplateContainer.Controls : Container.Controls; }
		}

		protected string GetControlIDWithPrefix(string controlId)
		{
			return controlId;
		}

		#endregion

#region Overrides

#if DEBUG
		public void OnInitForTesting() => OnInit(EventArgs.Empty);
#endif

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			ControlsToRender.Clear();
			Control[] internalControls = new Control[Controls.Count];
			Controls.CopyTo(internalControls, 0);
			Controls.Clear();

			foreach (Control internalControl in internalControls)
			{
				ControlsToRender.Add(internalControl);
			}
			Controls.Add(Header);
			Controls.Add(MenuItems);
			Controls.Add(Container);
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (UseUpdatePanel && AllowCollapseOfAlwaysVisibleHolder)
			{
				Panel.ContentTemplateContainer.Controls.AddAt(0, AlwaysVisibleHolder);
			}
			UpdateState();
			base.OnPreRender(e);
			((ZPage)Page).ZClientScript.RegisterHiddenField(StateControlID, IsCollapsed.ToString());
		}

#endregion Overrides

#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return dataSource != null && Controls != null;
		}

		public void Bind(object dataSource)
		{
			if (IsBindable(dataSource))
			{
				if (!dataSource.Equals(BusinessEntity))
				{
					if (string.IsNullOrEmpty(BindTo) || BindTo == ".")
					{
						BusinessEntity = dataSource as IBusiness;
					}
					else
					{
						BusinessEntity = ZPropertyAccessor.Get(dataSource, BindTo) as IBusiness;
					}
				}
				if (!IsCollapsed)
				{
					new ZWebControlBinder(BusinessEntity).Bind(Controls);
				}
			}
		}
		protected IBusiness BusinessEntity;

		public void UnBind()
		{
			BusinessEntity = null;
		}

#endregion

#region IBindTo Members

		public string BindTo
		{
			get
			{
				return bindTo;
			}
			set
			{
				bindTo = value;
			}
		}
		string bindTo = ".";

#endregion
	}
}
