using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public class ManageLayoutsPopup : ZButtonPopup
	{
		protected override void RenderPopupContainer(HtmlTextWriter writer)
		{
			Page.SaveDataSourceToSession();
			base.RenderPopupContainer(writer);
		}

		public const string DataSourceIndexerQueryStringKey = "dataSourceIndexer";

		protected override NameValueCollection AdditionalParameters
		{
			get
			{
				NameValueCollection result = base.AdditionalParameters;
				result.Add(DataSourceIndexerQueryStringKey, Page.DataSourceIndexer.ToString());
				return result;
			}
		}

		#region Setup

		public override bool CanAutosizeSelf
		{
			get
			{
				return true;
			}
		}

		protected override string IFrameSourcePageName
		{
			get { return "ManageLayoutsPage.aspx"; }
		}

		#endregion

		#region Dimensions

		protected override Unit PopupHeight
		{
			get { return 325; }
		}

		protected override Unit PopupWidth
		{
			get { return 520; }
		}

		protected override int ButtonControlWidth
		{
			get { return 170; }
		}

		#endregion

		#region internal wrapper properties

		protected internal Unit InternalPopupHeight() => PopupHeight;

		protected internal Unit InternalPopupWidth() => PopupWidth;

		protected internal ZGuid InternalDataSourceIndexer() => Page.DataSourceIndexer;

		#endregion

		#region Button Text / ToolTip

		protected override string ButtonTextCore
		{
			get { return Res.GetString("4f0308d9-d540-4b4d-a7a0-28118871ec58", "Manage Layouts"); }
		}

		protected override string ButtonToolTip
		{
			get { return Res.GetString("5b7bfa8e-f87c-4a7a-87dc-65a61ded4902", "Manage my filter layouts."); }
		}

		protected override Unit ControlHeight
		{
			get { return ZFilterStripConstants.Controls.ButtonHeight; }
		}

		#endregion
	}
}
