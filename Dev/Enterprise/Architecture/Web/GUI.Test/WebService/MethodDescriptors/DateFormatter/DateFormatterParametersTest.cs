using System;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class DateFormatterParametersTest : WebServiceParametersTest<DateFormatterParameters>
	{
		#region Implementation

		protected override void AssertEquals(DateFormatterParameters expected, DateFormatterParameters actual)
		{
			AssertEquals(expected.DateControlID, actual.DateControlID);
			AssertEquals(expected.DateFormatType, actual.DateFormatType);
			AssertEquals(expected.DateValue, actual.DateValue);
			AssertEquals(expected.TimeControlID, actual.TimeControlID);
			AssertEquals(expected.TimeValue, actual.TimeValue);
		}

		protected override void AssertParsedParameters(DateFormatterParameters parameters)
		{
			AssertEquals("Test", parameters.DateControlID);
		}

		protected override string GetExpectedExceptionMessageForStringWithValidationErrors()
		{
			return new ArgumentNullException("DateControlID").Message;
		}

		protected override DateFormatterParameters GetNewParameters()
		{
			DateFormatterParameters result = new DateFormatterParameters();
			result.DateControlID = "TestID";
			result.DateFormatType = "Short";
			result.DateValue = "12-02-10";
			result.TimeControlID = "ID";
			result.TimeValue = "00:11";
			return result;
		}

		protected override DateFormatterParameters GetNewParametersWithValidationErrors()
		{
			return new DateFormatterParameters();
		}

		protected override string GetParametersInvalidString()
		{
			return "BLAH : BLAH;";
		}

		protected override string GetParametersString()
		{
			return "{DateControlID: 'Test'}";
		}

		protected override string GetParametersStringWithValidationErrors()
		{
			return "{DateControlID: ''}";
		}

		#endregion
	}
}
