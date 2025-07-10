using System;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Web;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class Reports : BasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			SetupFilterControl();
			AdjustControlsForLiteViewMode();
		}

		void AdjustControlsForLiteViewMode()
		{
			if (IsInLiteViewMode)
			{
				Breadcrumb.Visible = false;
			}
		}

		#region DataSource

		protected new WebReportHolder DataSource
		{
			get { return base.DataSource as WebReportHolder; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return new WebReportHolder(Factory, SiteUser);
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		#endregion
		#region Report Filter Control setup

		protected ReportFilterControl FilterControl;

		void SetupFilterControl()
		{
			FilterControl = GetNewFilterControl();
			FilterControlHolder.Controls.Clear();
			FilterControlHolder.Controls.AddAt(0, FilterControl);
			FilterControl.SetupFilterControl(DataSource.SelectedReport);
		}

		protected virtual ReportFilterControl GetNewFilterControl()
		{
			return Page.LoadControl(FilterControlResource.FileName) as ReportFilterControl;
		}

		#endregion

		#region FilterControlResource

		protected ZWebResource FilterControlResource
		{
			get
			{
				if (fFilterControlResource == null)
				{
					fFilterControlResource = new ZWebResource(typeof(Reports), "ReportFilterControl.ascx", this, "Enterprise.Tracking.Web", typeof(ReportFilterControl).Assembly);
				}
				return fFilterControlResource;
			}
		}
		ZWebResource fFilterControlResource;

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(FilterControlResource);
				return result;
			}
		}

		#endregion

		protected void ReportsDropDownList_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetupFilterControl();
			using (DataSource.SelectedReport?.GetValidationSuspender())
			{
				FilterControl.Bind(DataSource.SelectedReport);
			}
		}

		public override void Validate()
		{
			base.Validate();
			DataSource.SelectedReport?.RunPreSaveValidation();
		}
	}
}
