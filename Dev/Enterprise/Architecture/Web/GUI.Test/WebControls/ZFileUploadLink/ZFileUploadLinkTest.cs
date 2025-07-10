using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZFileUploadLinkTest : ZTextIFramePopupTest
	{
		public override void TestPopupDimension()
		{
			AssertEquals("Width", GetExpectedWidth(), TestControl.PopupWidthInternal);
			AssertEquals("Height", GetExpectedHeight(), TestControl.PopupHeightInternal);
		}

		protected virtual Unit GetExpectedWidth()
		{
			return Unit.Pixel(400);
		}

		protected virtual Unit GetExpectedHeight()
		{
			return Unit.Pixel(250);
		}

		protected override string ExpectedPathToIFrameSourcePage
		{
			get { return "/Runtime/Enterprise_ZArchitecture_Web_GUI/" + RuntimeVersion + "/ZTextBoxButton/ZTextPopup/ZTextIFramePopup/ZButtonPopup/ZFileUploadLink/"; }
		}

		[HttpContextEnabledTest]
		public override void TestClickHandlerAssignment()
		{
			string expectedClickHandler = string.Format("ZTextPopup_ShowIFramePopup('ctl01_TextBox', 'ctl01_ctl00', '{0}', '{1}', true);", ExpectedPopupID, ExpectedIFrameSourceString);

			AssertEquals(expectedClickHandler, TestControl.ButtonClickHandlerInternal);
		}

		protected override string ExpectedIFrameSourcePage
		{
			get { return ""; }
		}

		public override void TestAssignSelectedValueToInvalidZType()
		{
			Assert(true);
		}

		protected override Control GetNewControl()
		{
			return new ZFileUploadLink();
		}

		protected ZFileUploadLink TestControl
		{
			get
			{
				return Control as ZFileUploadLink;
			}
		}
	}
}
