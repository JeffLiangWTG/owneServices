using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class WebServiceResponseActionsTest : TestCaseWithFactory
	{
		#region Test Cases

		public void TestConstants()
		{
			AssertEquals("None", WebServiceResponseActions.None);
			AssertEquals("Error", WebServiceResponseActions.ShowError);
			AssertEquals("Update", WebServiceResponseActions.UpdateValue);
			AssertEquals("Focus", WebServiceResponseActions.SetFocus);
			AssertEquals("PostBack", WebServiceResponseActions.PostBack);
			AssertEquals("ReadOnly", WebServiceResponseActions.SetReadOnly);
		}

		#endregion
	}
}
