using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class RegistrationNumberCollectionExtensionsTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetValue()
		{
			List<RegistrationNumber> regisrations = null;
			var value = regisrations.GetValue("CCC", "JP");
			AssertEquals(false, value.HasValue);
			regisrations = new List<RegistrationNumber>();
			value = regisrations.GetValue("CCC", "JP");
			AssertEquals(false, value.HasValue);
			var registration = new RegistrationNumber() { Type = new RegistrationNumberType(), CountryOfIssue = new Country(), Value = "RG2342" };
			regisrations.Add(registration);
			value = regisrations.GetValue("CCC", "JP");
			AssertEquals(false, value.HasValue);
			registration.Type.Code = "CCC";
			value = regisrations.GetValue("CCC", "JP");
			AssertEquals(false, value.HasValue);
			registration.CountryOfIssue.Code = "JP";
			value = regisrations.GetValue("CCC", "JP");
			AssertEquals(true, value.HasValue);
			AssertEquals("RG2342", value.Value);
			registration.Value = null;
			value = regisrations.GetValue("CCC", "JP");
			AssertEquals(false, value.HasValue);
			registration.Value = "RE23423";
			value = regisrations.GetValue("CCC", "JP");
			AssertEquals(true, value.HasValue);
			AssertEquals("RE23423", value.Value);
			regisrations.Add(new RegistrationNumber() { Type = new RegistrationNumberType() { Code = "CCC" }, CountryOfIssue = new Country() { Code = "JP" }, Value = "GD895" });
			value = regisrations.GetValue("CCC", "JP");
			AssertEquals(true, value.HasValue);
			AssertEquals("RE23423", value.Value);
			regisrations.Remove(registration);
			value = regisrations.GetValue("CCC", "JP");
			AssertEquals(true, value.HasValue);
			AssertEquals("GD895", value.Value);
		}
	}
}
