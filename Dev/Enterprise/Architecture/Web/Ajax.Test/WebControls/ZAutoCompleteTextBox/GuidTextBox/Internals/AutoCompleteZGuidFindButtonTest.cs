using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.GuidTextBox.Internals
{
	class AutoCompleteZGuidFindButtonForTesting : AutoCompleteZGuidFindButton
	{
		internal void OnPreRenderForTesting(EventArgs e) => OnPreRender(e);
		internal string ButtonClickHandlerForTesting => ButtonClickHandler;
		internal string AdditionalButtonClickHandlerForTesting => AdditionalButtonClickHandler;
		internal new Unit ButtonWidth => base.ButtonWidth;
		internal new HtmlInputButton ButtonControl => base.ButtonControl;
		internal new Unit MinWidth => base.MinWidth;
		internal new Unit ControlHeight => base.ControlHeight;
	}

	sealed class AutoCompleteZGuidFindButtonTest : ZFindBoxTest
	{
		protected override NameValueCollection ExpectedAdditionalParameters
		{
			get
			{
				NameValueCollection result = base.ExpectedAdditionalParameters;
				result.Add(ZGuidFindBox.GuidContainerIDQuery, FindBox.GuidContainerID);
				result.Add(ZFilterPage.ParentPKQuery, ((DependentBizOAutoCompleteHelper)FindBox.Helper).ParentPK.ToString());
				return result;
			}
		}

		#region TestAppearance

		[HttpContextEnabledTest]
		public void TestAppearance()
		{
			ZPage page = new ZPage();

			var findButton = new AutoCompleteZGuidFindButtonForTesting();
			page.Controls.Add(findButton);
			findButton.AutoCompleteTextBox = new ZAutoCompleteTextBox()
			{
				ID = "AutoComplete"
			};
			findButton.OnPreRenderForTesting(EventArgs.Empty);

			AssertEquals("FindButton TextBoxControl should not be visible", "none", findButton.TextBoxControl.Style["display"]);
			AssertEquals("FindButton ButtonControl should not accept focus", "-1", findButton.ButtonControl.Attributes["tabindex"]);

			AssertEquals("MinWidth should not be defined", Unit.Empty, findButton.MinWidth);
			AssertEquals("DefaultWidth should not be defined", Unit.Empty, findButton.DefaultWidth);
			AssertEquals("Width should be equal to Button width", findButton.ButtonWidth, findButton.Width);

			Assert("AutoCompleteTextBox attribute should be assigned in order to position the popup in correct place", !string.IsNullOrEmpty(findButton.TextBoxControl.Attributes["AutoCompleteTextBox"]));
		}

		#endregion

		#region Overrides

		protected override string ExpectedPopupID
		{
			get { return String.Format("{0}{1}", base.ExpectedPopupID, FindBox.ClientID); }
		}

		protected override string ExpectedOKFunctionName
		{
			get { return "ZTextPopup_SetValueAndGuidThenHidePopup"; }
		}

		[HttpContextEnabledTest]
		public override void TestClickHandlerAssignment()
		{
			AssertEquals(string.Format("ZTextPopup_ShowGuidPopup('ctl01_TextBox', 'ctl01_ctl00', '{0}', 'GuidContainerctl01', '{1}');", ExpectedPopupID, ExpectedIFrameSourceString) + FindBox.AdditionalButtonClickHandlerForTesting, FindBox.ButtonClickHandlerForTesting);
		}

		#endregion

		public override void TestAssignSelectedValueToInvalidZType()
		{
			ZDateTime dummyDate = new ZDateTime(2004, 12, 2);
			FindBox.SelectedValue = dummyDate;
			AssertEquals(ZGuid.Empty, FindBox.SelectedValue);
		}

		new AutoCompleteZGuidFindButtonForTesting FindBox
		{
			get
			{
				return Control as AutoCompleteZGuidFindButtonForTesting;
			}
		}

		protected override Control GetNewControl()
		{
			AutoCompleteZGuidFindButtonForTesting findBox = new AutoCompleteZGuidFindButtonForTesting();
			OrgAddressAutoCompleteHelper helper = new OrgAddressAutoCompleteHelper(Factory);
			helper.ParentPK = Factory.New<OrgHeader>().PK;
			findBox.Helper = helper;
			return findBox;
		}
	}
}
