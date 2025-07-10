using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZFileUploadDialogTest : ZIFramePageTest
	{
		public override void TestSetHeadersAndStyle()
		{
			Assert(true);
		}

		public override void TestSaveDataSourceFactoryNoBubble()
		{
			Assert(true);
		}

		public override void TestGetNewDataSource()
		{
			Assert(true);
		}

		protected override Control GetNewControl()
		{
			ZFileUploadDialog result = new ZFileUploadDialog();
			result.CreateChildControlsInternal();
			result.EnableViewState = true;
			return result;
		}

		public override void TestOKFunctionArguments()
		{
			AssertNotNull(FileUploadDialogTest.OKFunctionArgumentsInternal);
		}

		public override void TestCancelFunctionArguments()
		{
			AssertNull(FileUploadDialogTest.CancelFunctionArgumentsInternal);
		}

		protected override string GetExpOKButtonWidth() => "120px";

		protected override bool OKButtonIsVisible => true;

		protected ZFileUploadDialog FileUploadDialogTest => (ZFileUploadDialog)TestIFramePage;
	}
}
