using System;
using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public abstract class WebServiceResponseConditionTokenTest : WebServiceResponseTokenTest
	{
		#region Test Cases

		public override void TestConstructors()
		{
			AssertResponseToken(GetExpectedCondition(), "ControlID", "TestValue", GetResponseTokenForTesting() as WebServiceResponseConditionToken);
		}

		protected override IEnumerable<Tuple<WebServiceResponseToken, string>> GetExpectedToStringResults()
		{
			var token = (WebServiceResponseConditionToken)GetResponseTokenForTesting();
			token.ConditionValue = "some text";
			yield return new Tuple<WebServiceResponseToken, string>(token, $"'some text'.localeCompare(GetControlValue($('{token.ControlID}')), undefined, {{ sensitivity: \'base\' }})==0");

			token.ConditionValue = 10;
			yield return new Tuple<WebServiceResponseToken, string>(token, $"GetControlNumericValue($('{token.ControlID}'))==10");

			token.ConditionValue = 10.1;
			yield return new Tuple<WebServiceResponseToken, string>(token, $"GetControlNumericValue($('{token.ControlID}'))==10.1");
		}

		public void TestCondition()
		{
			var testResponse = GetResponseTokenForTesting() as WebServiceResponseConditionToken;
			AssertEquals(GetExpectedCondition(), testResponse.Condition);
		}

		#endregion

		#region Implementation

		protected abstract string GetCompareJavaScriptToken();

		protected abstract string GetExpectedCondition();

		protected void AssertResponseToken(string expectedCondition, string expectedControlID, string expectedValue, WebServiceResponseConditionToken token)
		{
			AssertEquals(expectedCondition, token.Condition);
			AssertEquals(expectedControlID, token.ControlID);
			AssertEquals(expectedValue, token.Value);
		}

		#endregion
	}
}
