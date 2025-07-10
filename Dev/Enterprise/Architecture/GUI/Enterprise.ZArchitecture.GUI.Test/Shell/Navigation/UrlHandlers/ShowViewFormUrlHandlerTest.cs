namespace Enterprise.ZArchitecture.Modules.Testing
{
	class ShowViewFormUrlHandlerTest : ShowFormUrlHandlerTestCase
	{
		protected override string UrlCommandForTest
		{
			get { return "ShowViewForm"; }
		}

		protected override ShowFormUrlHandler UrlHandlerForTest
		{
			get { return ShowViewFormUrlHandler.Instance; }
		}

		protected override string ExpectedFormCaption
		{
			get { return "View ZDummyForm"; }
		}
	}
}
