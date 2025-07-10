using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	sealed class SaveLayoutPopup_SaveLayoutPopupTest : ZTextIFramePopupTest
	{
		protected override NameValueCollection ExpectedAdditionalParameters
		{
			get
			{
				NameValueCollection result = base.ExpectedAdditionalParameters;
				result.Add(SaveLayoutPopup.DataSourceIndexerQueryStringKey, Popup.InternalDataSourceIndexer().ToString());
				return result;
			}
		}

		protected override string ExpectedIFrameSourcePage
		{
			get { return "SaveLayoutPage.aspx"; }
		}

		protected override string ExpectedPathToIFrameSourcePage
		{
			get { return "/Runtime/Enterprise_ZArchitecture_Web_GUI/" + RuntimeVersion + "/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZButtonPopup/SaveLayoutPopup/"; }
		}

		public override void TestPopupDimension()
		{
			AssertEquals(Unit.Pixel(494), Popup.InternalPopupWidth());
			AssertEquals(Unit.Pixel(130), Popup.InternalPopupHeight());
		}

		public override void TestAssignSelectedValueToInvalidZType()
		{
			Assert("N/A", true);
		}

		public override void TestAdditionalButtonClickHandler()
		{
			AssertEquals(string.Format("ZTextPopup_SetIsPublishedTextBoxID('{0}');", Popup.IsPublishedTextBoxControl.ClientID), Popup.InternalAdditionalButtonClickHandler());
		}

		protected override void AssertChildControlsNameAndType()
		{
			CombineAssertions(() =>
			{
				AssertEquals(3, TextBoxButton.Controls.Count);
				AssertEquals(typeof(WebControls.ZTextBox), TextBoxButton.Controls[0].GetType());
				AssertEquals(typeof(HtmlInputSubmit), TextBoxButton.Controls[1].GetType());
				AssertEquals(typeof(WebControls.ZTextBox), TextBoxButton.Controls[2].GetType());
				AssertEquals("TextBox", TextBoxButton.Controls[0].ID);
				AssertEquals("IsPublishedTextBox", TextBoxButton.Controls[2].ID);
			});
		}

		[HttpContextEnabledTest]
		public override void TestClickHandlerAssignment()
		{
			Popup.InternalEnsureChildControls();
			var expectedClickHandler = string.Format("javascript:__doPostBack('{0}', ''); return true;", Popup.ClientID);
			AssertEquals(expectedClickHandler, Popup.InternalButtonClickHandler());

			var button = (HtmlInputSubmit)TextBoxButton.Controls[1];
			PerformServerClick(button);

			AssertEquals(true, Popup.InternalZClientScript().IsStartupScriptRegistered(typeof(SaveLayoutPopup), Popup.ClientID));
		}

		void PerformServerClick(HtmlInputSubmit button)
		{
			typeof(HtmlInputSubmit).InvokeMember("OnServerClick", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, button, new object[] { EventArgs.Empty });
		}

		#region Implementation

		protected override Control GetNewControl()
		{
			return new SaveLayoutPopup();
		}

		new SaveLayoutPopup Popup
		{
			get { return (SaveLayoutPopup)Control; }
		}

		#endregion
	}
}
