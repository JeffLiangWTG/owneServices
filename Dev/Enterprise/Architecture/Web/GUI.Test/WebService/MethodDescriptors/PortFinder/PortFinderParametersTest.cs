using System;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class PortFinderParametersTest : WebServiceParametersTest<PortFinderParameters>
	{
		#region Implementation

		protected override void AssertEquals(PortFinderParameters expected, PortFinderParameters actual)
		{
			AssertEquals(expected.City, actual.City);
			AssertEquals(expected.Country, actual.Country);
			AssertEquals(expected.PortControlID, actual.PortControlID);
			AssertEquals(expected.PostalCode, actual.PostalCode);
			AssertEquals(expected.State, actual.State);
		}

		protected override void AssertParsedParameters(PortFinderParameters parameters)
		{
			AssertEquals("Test", parameters.PortControlID);
		}

		protected override string GetExpectedExceptionMessageForStringWithValidationErrors()
		{
			return new ArgumentNullException("PortControlID").Message;
		}

		protected override PortFinderParameters GetNewParameters()
		{
			PortFinderParameters result = new PortFinderParameters();
			result.PortControlID = "TestID";
			result.PostalCode = "60084";
			result.State = "Illinois";
			result.City = "Chicago";
			result.Country = "United States";
			return result;
		}

		protected override PortFinderParameters GetNewParametersWithValidationErrors()
		{
			return new PortFinderParameters();
		}

		protected override string GetParametersInvalidString()
		{
			return "BLAH : BLAH;";
		}

		protected override string GetParametersString()
		{
			return "{PortControlID: 'Test'}";
		}

		protected override string GetParametersStringWithValidationErrors()
		{
			return "{PortControlID: ''}";
		}

		#endregion
	}
}
