using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	public class ZFilterPage : ZIFramePage
	{
		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			return SearchControl?.Module.CreateNewFilterBusinessObject();
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected FilterBusinessObject FilterBusinessObject
		{
			get { return DataSource as FilterBusinessObject; }
		}

		#endregion DataSource

		#region Page Overrides

		public const string ParentPKQuery = "ParentBizO";

		protected override ControlCollection ControlsToBind
		{
			get { return SearchControl?.Controls; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			OKButton.Visible = false;
			CancelButton.Attributes[nameof(HtmlTextWriterAttribute.Class)] = "Button";
			CancelButton.Style.Add("height", ZFilterStripConstants.Controls.ButtonHeight.ToString());
			FindAutomaticallyIfRequired();
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			RenderScripts();
		}

		void FindAutomaticallyIfRequired()
		{
			if (!IsPostBack && HttpContext.Current.Request[ParentPKQuery] != null)
			{
				SearchControl?.FindButton_Click(this, EventArgs.Empty);
			}
		}

		#endregion

		#region FilterPage override 

		protected override Panel GetButtonsContainer()
		{
			Panel result = base.GetButtonsContainer();
			result.Style[HtmlTextWriterStyle.Position] = "absolute";
			result.Style["right"] = "0px";
			result.Style["bottom"] = "0px";

			return result;
		}

		protected override string[] OKFunctionArguments
		{
			get
			{
				if (SearchControl != null)
				{
					var selected = ((LinkButton)SearchControl.SearchResultsDataGrid.SelectedItem.Cells[SearchControl.SelectionColumnIndex].Controls[0]);

					return new[] { '\'' + selected.Text + '\'', '\'' + SearchControl.GetSelectionPK().ToString() + '\'' };
				}

				return new[] { string.Empty };
			}
		}

		protected override string[] CancelFunctionArguments
		{
			get { return null; }
		}

		#endregion

		#region Module

		protected internal WebModuleID ModuleID
		{
			get
			{
				if (fModuleID == null)
				{
					fModuleID = ZWebModuleFactory.GetWebModuleIDByName(GetModuleID());
				}

				return fModuleID;
			}
		}
		internal WebModuleID fModuleID;

		protected virtual ZString GetModuleID()
		{
			return RequestQueryString[ZFindBox.ModuleIDQuery];
		}
		#endregion

		#region Resources

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(GridResizeScript);
				return result;
			}
		}

		protected internal ZWebResource GridResizeScript
		{
			get { return new ZWebResource(typeof(ZFilterPage), "ZFilterPage_GridResizeScript.js", this, "Enterprise.ZArchitecture.Web.GUI.WebControls.ZFindBox"); }
		}

		#endregion Resources

		#region Script

		protected internal string GridResizeScriptBlock
		{
			get { return String.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", GridResizeScript.FileName); }
		}

		protected internal string GridResizeScriptHandler
		{
			get
			{
				if (SearchControl != null)
				{
					var script = "<SCRIPT type=\"text/javascript\">window.onload = function() { ";
					script += string.Format("ZFilterPage_GridResize('{0}','{1}')", SearchControl.ResultsGridDivClientID, CancelButton.ClientID);
					script += "; };</SCRIPT>";

					return script;
				}

				return string.Empty;
			}
		}

		internal new void RenderScripts()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), "ZFilterPage_GridResize_Script"))
			{
				Page.ZClientScript.RegisterClientScriptBlock(GetType(), "ZFilterPage_GridResize_Script", GridResizeScriptBlock);
			}
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), "ZFilterPage_GridResize_ScriptHandler"))
			{
				Page.ZClientScript.RegisterClientScriptBlock(GetType(), "ZFilterPage_GridResize_ScriptHandler", GridResizeScriptHandler);
			}
		}

		#endregion Script

		public override void Dispose()
		{
			SearchControl?.Dispose();
			base.Dispose();
		}

		#region Automatically generated

		protected ZRadioButton StartsWithRadioButton;
		protected ZRadioButton ContainsRadioButton;
		protected ZDropDownList DetailsList;
		protected internal ZTextBox Details;
		protected System.Web.UI.HtmlControls.HtmlGenericControl FilterControl;

		protected SearchControl SearchControl;

		#region Web Form Designer generated code

		protected override void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			base.OnInit(e);
			InitializeComponent();
			if (ModuleID != null && ModuleID != WebModuleIDs.NotAssigned)
			{
				SetupSearchControl();
			}
			else
			{
				HttpContext.Current.Response.Redirect($"{AppInstance.ErrorPage}?invalidQuery=true");
			}
		}

		void SearchResultsDataGrid_SelectedIndexChanged(object sender, EventArgs e)
		{
			HandleOkButtonClick();
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
		}

		void SetupSearchControl()
		{
			SearchControl = GetNewSearchControl();
			SearchControl.ModuleID = this.ModuleID;
			SearchControl.CssClassPrefix = CssConstants.ZFilterPage;
			SearchControl.ScrollingStyle = ScrollingStyle.Full;
			SearchControl.ExportToExcelButtonVisible = false;

			FormControl.Controls.AddAt(0, SearchControl);
			SearchControl.SearchResultsDataGrid.SelectedIndexChanged += new EventHandler(SearchResultsDataGrid_SelectedIndexChanged);
		}

		protected virtual SearchControl GetNewSearchControl()
		{
			return Page.LoadControl(SearchControlResource.FileName) as SearchControl;
		}
		#endregion

		#endregion

		#region Internal Properties

		internal void HandleOkButtonClickInternal() => HandleOkButtonClick();

		#endregion
	}

	#endregion
}
