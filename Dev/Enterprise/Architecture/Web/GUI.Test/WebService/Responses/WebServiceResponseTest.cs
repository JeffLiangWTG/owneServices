using System.Web.Script.Serialization;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class WebServiceResponseTest : TestCaseWithFactory
	{
		#region Test Cases

		public void TestAdd()
		{
			var testResponse = new WebServiceResponse();
			testResponse.Add(new SetFocusResponseToken("FocusControlID"));
			testResponse.Add(new ShowErrorResponseToken("Exception Message"));
			testResponse.Add(new UpdateValueResponseToken("UpdateControlID", "UpdateValue"));

			AssertEquals(3, testResponse.Count);
			AssertResponseToken("Focus", "FocusControlID", "", testResponse[0]);
			AssertResponseToken("Error", "", "Exception Message", testResponse[1]);
			AssertResponseToken("Update", "UpdateControlID", "UpdateValue", testResponse[2]);
		}

		public void TestToString()
		{
			var testResponse = new WebServiceResponse();
			testResponse.Add(new SetFocusResponseToken("FocusControlID"));
			testResponse.Add(new ShowErrorResponseToken("Exception Message"));
			testResponse.Add(new UpdateValueResponseToken("UpdateControlID", "UpdateValue"));

			AssertEquals(new JavaScriptSerializer().Serialize(testResponse), testResponse.ToString());
			AssertEquals(new JavaScriptSerializer().Serialize(testResponse), (string)testResponse);
		}

		#endregion

		#region Implementation

		void AssertResponseToken(string expectedAction, string expectedControlID, string expectedValue, WebServiceResponseActionToken token)
		{
			AssertEquals(expectedAction, token.Action);
			AssertEquals(expectedControlID, token.ControlID);
			AssertEquals(expectedValue, token.Value);
		}

		#endregion
	}
}
