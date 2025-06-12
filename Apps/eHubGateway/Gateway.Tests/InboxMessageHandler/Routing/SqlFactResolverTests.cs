using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CargoWise.eHub.Gateway.Routing;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler.Routing
{
	[TestClass]
	public class SqlFactResolverTests
	{
		[TestMethod]
		public void TestResolve_HasValidParams_ValueOfFactAssigned()
		{
			var stubContext = MockRepository.GenerateStub<eHubTransactionsContext>();
			var mockResult = MockRepository.GenerateMock<IEnumerable<string>>();
			var result = new List<string> { "email@carrier.com" }.AsEnumerable().GetEnumerator();
			mockResult.Stub(x => x.GetEnumerator()).Return(result);
			var sqlCommand = "EXEC GetRecipientCode 'CARRIER_EMAIL_VM', 'CARRIER_EMAIL_VM', 'Verified Gross Container Weight VERMAS to Carrier', 'Email Lookup', 'Email', @PortCode, @CountryCode, @SCAC";
			var facts = new Fact[]
			{
				new Fact { Type = "XPATH", Name = "PortCode", Value = "AUSYD" },
				new Fact { Type = "XPATH", Name = "CountryCode", Value = "AU" },
				new Fact { Type = "XPATH", Name = "SCAC", Value = "COSU" },
				new Fact { Type = "SQL", Name = "Email", Query = sqlCommand }
			};
			stubContext.Stub(x => x.SqlQuery<string>(Arg<string>.Is.Equal(sqlCommand), Arg<object[]>.Is.Anything)).Return(mockResult);

			var sqlFactResolver = new SqlFactResolver(stubContext);
			sqlFactResolver.Resolve(facts);

			stubContext.AssertWasCalled(x => x.SqlQuery<string>(Arg<string>.Is.Equal(sqlCommand), Arg<object[]>.Matches(
				a =>
					(a[0] as SqlParameter).ParameterName == "@PortCode" &&
					(a[0] as SqlParameter).Value.ToString() == "AUSYD" &&
					(a[1] as SqlParameter).ParameterName == "@CountryCode" &&
					(a[1] as SqlParameter).Value.ToString() == "AU" &&
					(a[2] as SqlParameter).ParameterName == "@SCAC" && (a[2] as SqlParameter).Value.ToString() == "COSU"
			)));
			Assert.IsTrue(facts.First(f => f.Name == "Email").Value.Contains("email@carrier.com"));
		}
	}
}
