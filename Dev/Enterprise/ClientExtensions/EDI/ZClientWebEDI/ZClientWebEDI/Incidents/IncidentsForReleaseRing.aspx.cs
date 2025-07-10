using System;
using System.Web.UI;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class IncidentsForReleaseRing : BasePage
	{
		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			WebFilterBusinessObjectFactory filterFactory = new WebFilterBusinessObjectFactory(Factory);
			WebSupportIncidentFilterBusinessObject result = filterFactory.New<WebSupportIncidentFilterBusinessObject>();
			result.IM_OH_Client = SiteUser.LoggedInOrganisation.PK;
			result.Status = SupportIncidentLookups.Status.Closed;
			result.CurrentReleaseBuildPK = GetReleaseBuildPK("CurrentReleaseBuildPK");
			result.RequestedReleaseBuildPK = GetReleaseBuildPK("RequestedReleaseBuildPK");

			return result;
		}

		protected virtual string GetReleaseBuildPK(string key)
		{
			return Request[key];
		}

		protected WebSupportIncidentFilterBusinessObject FilterBusinessObject
		{
			get { return DataSource as WebSupportIncidentFilterBusinessObject; }
		}

		#endregion

		#region Search Control Setup

		void SetupSearchControl()
		{
			SearchControl = GetNewSearchControl();
			SearchControl.ModuleID = WebModuleIDs.CargoWiseEDIIncidents;
			SearchControl.NewButtonVisible = false;
			SearchControl.PageSize = 30;
			SearchControl.MaxRows = 150;
			SearchControl.CustomiseColumnsButtonVisible = false;
			SearchControlHolder.Controls.AddAt(0, SearchControl);
		}

		protected virtual SearchControl GetNewSearchControl()
		{
			return Page.LoadControl(SearchControlResource.FileName) as SearchControl;
		}

		protected override ControlCollection ControlsToBind
		{
			get { return SearchControl.Controls; }
		}

		SearchControl SearchControl;

		#endregion

		#region Web Form Designer generated code

		protected System.Web.UI.HtmlControls.HtmlGenericControl search;
		protected IncidentSearchUserControl IncidentSearch;
		protected System.Web.UI.HtmlControls.HtmlGenericControl Contents;

		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
			SetupSearchControl();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SearchControl.FindButton_Click(this, e);
			SearchControl.HideFilterHeader();

			SetupHeader();
			AdjustControlsForLiteViewMode();
		}

		void InitializeComponent()
		{
			this.DataSourceAssemblyName = "ZClientEDI";
		}

		#endregion

		void SetupHeader()
		{
			var releaseName = Request["RequestedReleaseBuildName"];
			if (!string.IsNullOrEmpty(releaseName))
			{
				LabelHeader.Text = " Fixed in " + releaseName;
			}
		}

		void AdjustControlsForLiteViewMode()
		{
			if (IsInLiteViewMode)
			{
				Breadcrumb.Visible = false;
			}
		}
	}
}
