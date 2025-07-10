namespace Enterprise.ZArchitecture.Modules.Testing
{
	class ShowDeleteFormUrlHandlerTest : ShowFormUrlHandlerTestCase
	{
		protected override string UrlCommandForTest
		{
			get { return "ShowDeleteForm"; }
		}

		protected override ShowFormUrlHandler UrlHandlerForTest
		{
			get { return ShowDeleteFormUrlHandler.Instance; }
		}

		protected override string ExpectedFormCaption
		{
			get { return "Delete ZDummyForm"; }
		}
	}
}
