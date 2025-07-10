using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	//#warning move FilterStripBizO into a base class, ManageLayoutPAge can subclass this too
	public partial class SaveLayoutPage : ZIFramePageWithFilterStripBizO
	{
		//#warning move to new base class
		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			PublishLayoutCheckBox.Enabled = SaveLayoutBizO.CanPublishLayouts;
			PublishCompanyLayoutCheckBox.Text = Res.GetString("37e0b915-9b8f-4c90-958b-f03ff2ea9b7d", "Publish across all organizations associated to this company ({0})", Environment.EnvProxy.Instance.CurrentCompany.Code);
			PublishCompanyLayoutCheckBox.Visible = SaveLayoutBizO.CanPublishCompanyLayouts;
			if (PublishCompanyLayoutCheckBox.Visible)
			{
				PublishLayoutCheckBox.Text = Res.GetString("2C96892B-81ED-4BCB-BD85-5BD0829AEED7", "Publish this layout for users from your organization ({0})", this.SiteUser.AffiliationCode);
			}
			NotificationFlags.DisplayErrors = true;
		}

		public override string QueryStringKey
		{
			get { return SaveLayoutPopup.DataSourceIndexerQueryStringKey; }
		}

		#region SaveLayoutBizO

		public SaveLayoutBusinessObject SaveLayoutBizO
		{
			get { return (SaveLayoutBusinessObject)DataSource; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return new SaveLayoutBusinessObject(FilterStripBizO, Page.SiteUser);
		}

		#endregion

		//#warning move to new base class

		#region OK

		protected override void HandleOkButtonClick()
		{
			var layoutBizO = SaveLayoutBizO;
			layoutBizO.RunPreSaveValidation();
			if (!layoutBizO.HasErrors)
			{
				base.HandleOkButtonClick();
				new DataGridLayoutManager().SavePreconfiguredLayout(
				layoutBizO.FilterStripBizO, layoutBizO.LayoutName, layoutBizO.IsPublished, layoutBizO.IsPublishedForCompany,
				layoutBizO.IsSavingColumns ? SaveColumnLayout.Yes : SaveColumnLayout.No);
			}
			else
			{
				NotificationFlags.DisplayErrors = true;
			}
		}

		protected override string[] OKFunctionArguments
		{
			get { return new string[] { string.Format("'{0}'", SaveLayoutBizO.LayoutName), string.Format("'{0}'", SaveLayoutBizO.IsPublished) }; }
		}

		#endregion

		#region Cancel

		protected override string[] CancelFunctionArguments
		{
			get { return null; }
		}

		#endregion

	}
}
