using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public abstract class WebServiceResponseTokenTest : TestCaseWithFactory
	{
		#region Test Cases

		public virtual void TestConstructors()
		{
			Assert(false);
		}

		public void TestToString()
		{
			AssertNotEquals(string.Empty, GetResponseTokenForTesting().ToString());
			AssertNotEquals(string.Empty, (string)GetResponseTokenForTesting());

			var expectedToStringResults = GetExpectedToStringResults();
			AssertNotNull(expectedToStringResults);
			AssertNotEquals(0, expectedToStringResults.Count());

			foreach (var expectedResult in expectedToStringResults)
			{
				AssertEquals(expectedResult.Item2, expectedResult.Item1.ToString());
				AssertEquals(expectedResult.Item2, (string)expectedResult.Item1);
			}
		}

		protected virtual IEnumerable<Tuple<WebServiceResponseToken, string>> GetExpectedToStringResults()
		{
			var token = GetResponseTokenForTesting();
			yield return new Tuple<WebServiceResponseToken, string>(token, new JavaScriptSerializer().Serialize(token));
		}

		public void TestControlID()
		{
			var testResponse = GetResponseTokenForTesting();

			testResponse.ControlID = "Test1";
			AssertEquals("Test1", testResponse.ControlID);

			testResponse.ControlID = "Test2";
			AssertEquals("Test2", testResponse.ControlID);
		}

		public void TestValue()
		{
			var testResponse = GetResponseTokenForTesting();

			testResponse.Value = "Test1";
			AssertEquals("Test1", testResponse.Value);

			testResponse.Value = "Test2";
			AssertEquals("Test2", testResponse.Value);
		}

		#endregion

		#region Implementation

		protected abstract WebServiceResponseToken GetResponseTokenForTesting();

		#endregion
	}
}
