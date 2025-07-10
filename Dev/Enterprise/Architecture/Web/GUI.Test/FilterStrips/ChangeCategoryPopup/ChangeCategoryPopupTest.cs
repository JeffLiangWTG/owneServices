using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	sealed class ChangeCategoryPopupTest : ZTextIFramePopupTest
	{
		protected override string ExpectedIFrameSourcePage
		{
			get { return "ChangeCategoryPage.aspx"; }
		}

		protected override string ExpectedPathToIFrameSourcePage
		{
			get { return "/Runtime/Enterprise_ZArchitecture_Web_GUI/" + RuntimeVersion + "/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZButtonPopup/ChangeCategoryPopup/"; }
		}

		public override void TestPopupDimension()
		{
			AssertEquals(Unit.Pixel(180), Popup.InternalPopupWidth());
			AssertEquals(Unit.Pixel(183), Popup.InternalPopupHeight());
		}

		public override void TestAssignSelectedValueToInvalidZType()
		{
			Assert("N/A", true);
		}

		#region Implementation

		protected override NameValueCollection ExpectedAdditionalParameters
		{
			get
			{
				NameValueCollection result = base.ExpectedAdditionalParameters;
				result.Add(ChangeCategoryPopup.DataSourceIndexerQueryStringKey, Popup.InternalDataSourceIndexer().ToString());
				result.Add(ChangeCategoryPopup.FilterStripPKQueryStringKey, Popup.filterStripPK.ToString());
				if (result[ZIFramePage.CallerPKQuery] != null)
				{
					result[ZIFramePage.CallerPKQuery] = Popup.InternalDataSourceIndexer().ToString();
				}
				else
				{
					result.Add(ZIFramePage.CallerPKQuery, Popup.InternalDataSourceIndexer().ToString());
				}
				return result;
			}
		}

		protected override Control GetNewControl()
		{
			return new ChangeCategoryPopup();
		}

		new ChangeCategoryPopup Popup
		{
			get { return (ChangeCategoryPopup)Control; }
		}

		#endregion
	}
}
