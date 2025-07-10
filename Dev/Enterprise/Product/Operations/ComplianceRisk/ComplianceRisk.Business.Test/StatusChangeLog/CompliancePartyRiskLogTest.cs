using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(CompliancePartyRiskLog))]
	public class CompliancePartyRiskLogTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var entityScreeningLog = Factory.New<IStmEntityScreeningLog>();
			entityScreeningLog.PJ_SystemCreateTimeUtc = new DateTime(2022, 1, 1);
			entityScreeningLog.PJ_HighConfidenceResults = "High";
			entityScreeningLog.PJ_MediumConfidenceResults = "Medium";
			entityScreeningLog.PJ_LowConfidenceResultsCount = 2;
			entityScreeningLog.PJ_IncludedLists = "Included";
			entityScreeningLog.PJ_ExcludedLists = "Excluded";

			var party = new Party
			{
				Code = "ORG A",
				Status = "CLR",
				Description = "S123: Consignee",
				LogPK = Guid.NewGuid(),
				TableCode = "ORG",
				PK = Guid.NewGuid()
			};

			var partyRiskLog = new CompliancePartyRiskLog(party, new ScreeningStatusesList(), entityScreeningLog);

			AssertEquals("ORG A", partyRiskLog.Code);
			AssertEquals("S123: Consignee", partyRiskLog.Description);
			AssertEquals("CLR", partyRiskLog.Status);
			AssertEquals("Clear", partyRiskLog.StatusDescription);
			AssertEquals(entityScreeningLog, partyRiskLog.EntityScreeningLog);
			AssertEquals(entityScreeningLog.PJ_ScreenDate, partyRiskLog.LastScreenDate);
			AssertEquals(entityScreeningLog.PJ_HighConfidenceResults, partyRiskLog.HighConfidence);
			AssertEquals(entityScreeningLog.PJ_MediumConfidenceResults, partyRiskLog.MediumConfidence);
			AssertEquals(entityScreeningLog.PJ_LowConfidenceResultsCount, partyRiskLog.LowConfidence);
			AssertEquals(entityScreeningLog.PJ_IncludedLists, partyRiskLog.IncludedLists);
			AssertEquals(entityScreeningLog.PJ_ExcludedLists, partyRiskLog.ExcludedLists);

			party.Status = "MAT";
			AssertEquals("MAT", partyRiskLog.Status);
			AssertEquals("Matched", partyRiskLog.StatusDescription);

			partyRiskLog = new CompliancePartyRiskLog(party, new ScreeningStatusesList(), null);
			AssertNull(partyRiskLog.EntityScreeningLog);
			AssertEquals(ZDateTime.MinSmallDateTimeValue, partyRiskLog.LastScreenDate);
			AssertEquals(ZString.Empty, partyRiskLog.HighConfidence);
			AssertEquals(ZString.Empty, partyRiskLog.MediumConfidence);
			AssertEquals(0, partyRiskLog.LowConfidence);
			AssertEquals(ZString.Empty, partyRiskLog.IncludedLists);
			AssertEquals(ZString.Empty, partyRiskLog.ExcludedLists);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CompliancePartyRiskLog(new Party(), new ScreeningStatusesList(), null);
		}
	}
}
