using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.Integration;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceLocationRiskLog))]
	public class ComplianceLocationRiskLogTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var entityScreeningLog = Factory.New<IStmEntityScreeningLog>();
			var country = new Country
			{
				Code = "AU",
				Name = "Australia",
				Description = "S123: Routing",
				IsSanctioned = true,
				LogPK = Guid.NewGuid()
			};
			var locationRiskLog = new ComplianceLocationRiskLog(country, null);

			AssertEquals("AU", locationRiskLog.Location);
			AssertEquals("Australia", locationRiskLog.LocationDescription);
			AssertEquals("S123: Routing", locationRiskLog.Description);
			AssertEquals("Potential Risk", locationRiskLog.RiskStatus);

			locationRiskLog = new ComplianceLocationRiskLog(country, ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertEquals("Blocked", locationRiskLog.RiskStatus);

			country.IsSanctioned = false;
			AssertEquals("Clear", locationRiskLog.RiskStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComplianceLocationRiskLog(new Country(), null);
		}
	}
}
