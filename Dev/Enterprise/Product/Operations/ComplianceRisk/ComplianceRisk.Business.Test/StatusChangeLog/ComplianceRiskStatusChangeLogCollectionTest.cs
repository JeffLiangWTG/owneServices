using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskStatusChangeLogCollection))]
	public class ComplianceRiskStatusChangeLogCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComplianceRiskStatusChangeLogCollection>
	{
		public void TestLoadCollection()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			Factory.Save();

			AssertEquals("Pre-Condition", "BLK", complianceRiskStatus.COR_OverallRisk);
			AssertNotEquals("Pre-Condition", "CLR", complianceRiskStatus.COR_CommodityRisk);

			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var basicLogsCount = 1;
			var collection = new ComplianceRiskStatusChangeLogCollection((IComplianceItemRiskStatusProvider)shipment);
			collection.LoadCollection();
			AssertEquals("StmComplianceEvent related to Overall risk status change", basicLogsCount, collection.Count);

			complianceRiskStatus.COR_CommodityRisk = "CLR";
			Factory.Save();
			collection.LoadCollection();
			AssertEquals("Only care about Overall risk status change log", basicLogsCount, collection.Count);

			complianceRiskStatus.COR_OverallRisk = "CLR";
			Factory.Save();
			collection.LoadCollection();
			AssertEquals("StmComplianceEvent related to Overall risk status change", basicLogsCount + 1, collection.Count);

			complianceRiskStatus.COR_OverallRisk = "BLK";
			Factory.Save();
			collection.LoadCollection();
			AssertEquals("StmComplianceEvent related to Overall risk status change", basicLogsCount + 2, collection.Count);

			UpdateLogsTime();

			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			complianceRiskStatus.COR_OverallRisk = "CLR";
			Factory.Save();
			collection.LoadCollection();
			AssertEquals("1 StmComplianceEvent related to Compliance Decision Change.", basicLogsCount + 3, collection.Count);

			UpdateLogsTime();

			commodityDetail.CCD_AssessmentNotes = "some notes";
			complianceRiskStatus.COR_OverallRisk = "BLK";
			Factory.Save();
			collection.LoadCollection();
			AssertEquals("2 StmComplianceEvents related to Compliance Decision Change.", basicLogsCount + 4, collection.Count);

			UpdateLogsTime();

			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			Factory.Save();
			collection.LoadCollection();
			AssertEquals("Still 2 StmComplianceEvents related to Compliance Decision Change.", basicLogsCount + 4, collection.Count);

			UpdateLogsTime();

			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			complianceRiskStatus.COR_OverallRisk = "CLR";
			Factory.Save();
			collection.LoadCollection();
			AssertEquals("3 StmComplianceEvents related to Compliance Decision Change.", basicLogsCount + 5, collection.Count);

			void UpdateLogsTime()
			{
				// In case the logs time are the same
				complianceRiskStatus.GetEventLogs().ForEach(log =>
				{
					log.SCE_SystemCreateTimeUtc = log.SCE_SystemCreateTimeUtc.AddSeconds(-10);
				});
			}
		}

		public void TestLoadCollectionShouldTakeCCDLogIfOVLAndCCDInSameSecond()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			Factory.Save();

			AssertEquals("Pre-Condition", "CLR", complianceRiskStatus.COR_OverallRisk);

			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var collection = new ComplianceRiskStatusChangeLogCollection((IComplianceItemRiskStatusProvider)shipment);
			collection.LoadCollection();
			AssertEquals("Pre-Condition:initialised StmComplianceEvent log", 1, collection.Count);

			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			Factory.Save();

			var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, shipment.PK);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.StatusUpdatedCode);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, new[] { ComplianceEventList.Codes.OverallRisk, ComplianceEventList.Codes.ComplianceDecisionChanged });
			var complianceEvent = Factory.Load<StmComplianceEvent>(query);
			AssertEquals("3 StmComplianceEvent related to CDC and OVL changes", 3, complianceEvent.Length);
			var eventCDCLog = complianceEvent.First(c => c.SCE_EventSubType == ComplianceEventList.Codes.ComplianceDecisionChanged && c.SCE_NewValue == ComplianceRiskStatusCodeList.Codes.Blocked);
			var eventOVLLog = complianceEvent.First(c => c.SCE_EventSubType == ComplianceEventList.Codes.OverallRisk && c.SCE_NewValue == ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertNotNull(eventCDCLog);
			AssertNotNull(eventOVLLog);

			eventCDCLog.SCE_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMilliseconds(-450);
			eventCDCLog.SCE_NewValue = ComplianceRiskStatusCodeList.Codes.Blocked;
			eventOVLLog.SCE_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMilliseconds(450);
			eventOVLLog.SCE_NewValue = ComplianceRiskStatusCodeList.Codes.Blocked;
			Factory.Save();
			collection.LoadCollection();
			AssertEquals("If the two event with same SCE_NewValue logs are OVL and CDC and are within a one-second interval,only 1 StmComplianceEvent should be loaded", 2, collection.Count);
			Assert("The loaded log should be the CDC change log with snapshot", collection[0].SnapshotExists);

			eventCDCLog.SCE_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMilliseconds(-450);
			eventCDCLog.SCE_NewValue = ComplianceRiskStatusCodeList.Codes.Clear;
			eventOVLLog.SCE_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMilliseconds(450);
			eventOVLLog.SCE_NewValue = ComplianceRiskStatusCodeList.Codes.Blocked;
			Factory.Save();
			collection.LoadCollection();
			AssertEquals("If the two event logs with different SCE_NewValue are OVL and CDC and are within a one-second interval, 2 StmComplianceEvent should be loaded", 3, collection.Count);
			Assert("The loaded log should be the CDC change log with snapshot", collection[0].SnapshotExists);

			eventCDCLog.SCE_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMilliseconds(-550);
			eventCDCLog.SCE_NewValue = ComplianceRiskStatusCodeList.Codes.Blocked;
			eventOVLLog.SCE_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMilliseconds(550);
			eventOVLLog.SCE_NewValue = ComplianceRiskStatusCodeList.Codes.Blocked;
			Factory.Save();

			collection.LoadCollection();
			AssertEquals("If the two event logs with same SCE_NewValue are OVL and CDC and are not within a one-second interval,2 StmComplianceEvent should be loaded", 3, collection.Count);

			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Held;
			Factory.Save();
			collection.LoadCollection();
			AssertEquals("Event logs that are not within the same second should be displayed normally.", 4, collection.Count);
		}

		public void TestAllowNewCore()
		{
			AssertEquals("Suppress user interface to create log", false, GetCollectionToTest().AllowNew);
		}

		public void TestAllowRemoveCore()
		{
			AssertEquals("Suppress user interface to delete log", false, GetCollectionToTest().AllowRemove);
		}

		protected override ComplianceRiskStatusChangeLogCollection GetCollectionToTest()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			return new ComplianceRiskStatusChangeLogCollection((IComplianceItemRiskStatusProvider)shipment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceRiskStatusChangeLog(Factory.New<StmComplianceEvent>());
		}

		IDisposable setAllowComplianceCommodityRiskAssessmentToTrue;
		protected override void SetUp()
		{
			base.SetUp();
			setAllowComplianceCommodityRiskAssessmentToTrue = OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			setAllowComplianceCommodityRiskAssessmentToTrue.Dispose();
		}
	}
}
