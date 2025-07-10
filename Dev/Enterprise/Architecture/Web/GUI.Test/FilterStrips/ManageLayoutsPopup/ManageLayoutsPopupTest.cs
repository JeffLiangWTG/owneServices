using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	sealed class ManageLayoutsPopupTest : ZTextIFramePopupTest
	{
		public override void TestPopupDimension()
		{
			AssertEquals(Unit.Pixel(520), Popup.InternalPopupWidth());
			AssertEquals(Unit.Pixel(325), Popup.InternalPopupHeight());
		}

		public override void TestAssignSelectedValueToInvalidZType()
		{
			Assert("N/A", true);
		}

		#region Implementation

		protected override string ExpectedIFrameSourcePage
		{
			get { return "ManageLayoutsPage.aspx"; }
		}

		protected override string ExpectedPathToIFrameSourcePage
		{
			get { return "/Runtime/Enterprise_ZArchitecture_Web_GUI/" + RuntimeVersion + "/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZButtonPopup/ManageLayoutsPopup/"; }
		}

		protected override NameValueCollection ExpectedAdditionalParameters
		{
			get
			{
				NameValueCollection result = base.ExpectedAdditionalParameters;
				result.Add(ManageLayoutsPopup.DataSourceIndexerQueryStringKey, Popup.InternalDataSourceIndexer().ToString());
				return result;
			}
		}

		protected override Control GetNewControl()
		{
			return new ManageLayoutsPopup();
		}

		new ManageLayoutsPopup Popup
		{
			get { return (ManageLayoutsPopup)Control; }
		}

		#endregion
	}
}
