using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccUsageCollectorProvider))]
	public class AccUsageCollectorProviderTest : TestCaseWithFactory
	{
		public void TestReportNonCurrrentCompany()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var nonCurrentCompany = testObjectCreator.NonCurrentCompany;
			new AccUsageCollectorProvider().Report(UsageFeatures.Codes.AccGeneralLedgerData, nonCurrentCompany.PK.ToGuid());
			var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("Wrong number of rows in EDIMessage table.", 1, ediMessages.Length);
			var jObject = ediMessages.Select(msg => JObject.Parse(msg.EM_MessageTextDetail)).First();
			AssertEquals(UsageFeatures.Codes.AccGeneralLedgerData, jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.FeatureCode, StringComparison.InvariantCulture))?.Value.ToString());
			AssertEquals(nonCurrentCompany.OrgProxy.OH_FullName, jObject.Properties().FirstOrDefault(kp => kp.Name.Equals(UsageProperties.OrganisationName, StringComparison.InvariantCulture))?.Value.ToString());
		}
	}
}
