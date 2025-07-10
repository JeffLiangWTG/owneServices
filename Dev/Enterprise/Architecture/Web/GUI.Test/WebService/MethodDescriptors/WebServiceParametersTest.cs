using System;
using System.Web.Script.Serialization;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public abstract class WebServiceParametersTest<T> : TestCaseWithFactory
			where T : WebServiceParameters
	{
		#region Test Cases

		public void TestToString()
		{
			T expected = GetNewParameters();
			T actual = new JavaScriptSerializer().Deserialize<T>(expected.ToString());
			AssertEquals(expected, actual);
		}

		public void TestParse()
		{
			try
			{
				AssertParsedParameters(WebServiceParameters.Parse<T>(GetParametersString()));
			}
			catch (Exception)
			{
				Assert("No exception was expected here", false);
			}

			try
			{
				WebServiceParameters.Parse<T>(GetParametersStringWithValidationErrors());
				Assert("Expected exception: " + GetExpectedExceptionMessageForStringWithValidationErrors(), false);
			}
			catch (Exception ex)
			{
				AssertEquals("Expected exception: " + GetExpectedExceptionMessageForStringWithValidationErrors(), GetExpectedExceptionMessageForStringWithValidationErrors(), ex.Message);
			}

			try
			{
				WebServiceParameters.Parse<T>(GetParametersInvalidString());
				Assert("Expected exception: Please provide valid parameters for this method", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Expected exception: Please provide valid parameters for this method", "Please provide valid parameters for this method", ex.Message);
			}
		}

		public void TestValidate()
		{
			T testParameters = GetNewParameters();
			try
			{
				testParameters.Validate();
			}
			catch (Exception)
			{
				Assert("No exception was expected here", false);
			}

			testParameters = GetNewParametersWithValidationErrors();
			try
			{
				testParameters.Validate();
				Assert("Expected exception: " + GetExpectedExceptionMessageForStringWithValidationErrors(), false);
			}
			catch (Exception ex)
			{
				AssertEquals("Expected exception: " + GetExpectedExceptionMessageForStringWithValidationErrors(), GetExpectedExceptionMessageForStringWithValidationErrors(), ex.Message);
			}
		}

		#endregion

		#region Implementation

		protected abstract void AssertEquals(T expected, T actual);
		protected abstract void AssertParsedParameters(T parameters);
		protected abstract string GetExpectedExceptionMessageForStringWithValidationErrors();
		protected abstract string GetParametersString();
		protected abstract string GetParametersStringWithValidationErrors();
		protected abstract string GetParametersInvalidString();
		protected abstract T GetNewParameters();
		protected abstract T GetNewParametersWithValidationErrors();

		#endregion
	}
}
