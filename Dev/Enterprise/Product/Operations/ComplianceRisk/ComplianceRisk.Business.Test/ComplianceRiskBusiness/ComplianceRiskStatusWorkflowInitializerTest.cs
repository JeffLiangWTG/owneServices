using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceRiskStatusWorkflowInitializerTest : TestCaseWithFactory
	{
		public void TestInitializeAssessmentWorkflowWithNoCommodities()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			AssertEquals(true, complianceRiskStatus.InitializeAssessmentWorkflow());
			AssertEquals(true, complianceRiskStatus.IsInDatabase);

			var initialisedEventLogsCount = complianceRiskStatus.GetEventLogs().Count();

			CombineAssertions("Should create StmALog log: Compliance Assessment Initialized.", () =>
			{
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);

				var log = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ComplianceRiskInteraction.Code)).Single();
				AssertEquals("|MST=Compliance Assessment|NEW=CAI|TYP=USR", log.SL_Reference);
				AssertEquals("Compliance Assessment Initialized by User", log.DisplayEventReference);
			});

			CombineAssertions("Should create StmComplianceEvent log: Compliance Assessment Initialized.", () =>
			{
				var eventLog = complianceRiskStatus.GetEventLogs().Single(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
				AssertNotNull(eventLog);
				AssertEquals("CRI", eventLog.SCE_EventType);
				AssertEquals("CAI", eventLog.SCE_EventSubType);
				AssertEquals("|MST=Compliance Assessment|NEW=CAI|TYP=USR", eventLog.SCE_EventReference);
			});

			CombineAssertions("Should not create log: Compliance Assessment Initialized already exists.", () =>
			{
				AssertEquals("When Status Is Not UNI or CAD, not Initialized again", false, complianceRiskStatus.InitializeAssessmentWorkflow());

				var logs = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ComplianceRiskInteraction.Code));
				var eventLogs = complianceRiskStatus.GetEventLogs();
				AssertEquals(1, logs.Length);
				AssertEquals(initialisedEventLogsCount, eventLogs.Count());
				AssertEquals("Assessment Initialized", true, eventLogs.Any(e => e.SCE_EventType == "CRI" && e.SCE_EventSubType == "CAI"));
			});
		}

		public void TestInitializeAssessmentWorkflowWarnings()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.BorderWiseCheckStatus = BorderWiseCheckStatus.NotViewable;
			commodity.CCD_NomenclatureCondition = true;
			commodity.CCD_SpecificCondition = true;
			commodity.BlockedByComplianceRule = true;
			commodity.Validation.CheckCommodityStatus();

			CombineAssertions("Should not show warning message before Assessment Initialized.", () =>
			{
				AssertEquals(false, complianceRiskStatus.IsAssessmentInitialized);
				AssertNoRowWarningContaining(commodity, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);
				AssertNoRowWarningContaining(commodity, ComplianceCommodityDetailValidationReal.GetMessages.ReviewComplianceAlertMessage);
				AssertNoRowWarningContaining(commodity, ComplianceCommodityDetailValidationReal.GetMessages.ComplianceRuleAdministratorMessage);
			});

			AssertEquals(true, complianceRiskStatus.InitializeAssessmentWorkflow());
			AssertEquals(true, complianceRiskStatus.IsInDatabase);

			CombineAssertions("Should show warning message after Assessment Initialized.", () =>
			{
				AssertEquals(true, complianceRiskStatus.IsAssessmentInitialized);
				AssertHasRowWarningContaining(commodity, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);
				AssertHasRowWarningContaining(commodity, ComplianceCommodityDetailValidationReal.GetMessages.ReviewComplianceAlertMessage);
				AssertHasRowWarningContaining(commodity, ComplianceCommodityDetailValidationReal.GetMessages.ComplianceRuleAdministratorMessage);
			});
		}

		public void TestDeclinedAssessmentWorkflowWithNoCommodities()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			CombineAssertions("Should create StmALog log: Compliance Assessment Declined.", () =>
			{
				AssertEquals(true, complianceRiskStatus.DeclinedAssessmentWorkflow());
				AssertEquals(true, complianceRiskStatus.IsInDatabase);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.PossibleRisk, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

				var log = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ComplianceRiskInteraction.Code)).Single();
				AssertEquals("|MST=Compliance Assessment|NEW=CAD", log.SL_Reference);
				AssertEquals("Compliance Assessment Declined", log.DisplayEventReference);
			});

			CombineAssertions("Should create StmComplianceEvent log: Compliance Assessment Declined.", () =>
			{
				var eventLog = complianceRiskStatus.GetEventLogs().Single(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
				AssertNotNull(eventLog);
				AssertEquals("CRI", eventLog.SCE_EventType);
				AssertEquals("CAD", eventLog.SCE_EventSubType);
				AssertEquals("|MST=Compliance Assessment|NEW=CAD", eventLog.SCE_EventReference);
			});

			CombineAssertions("Should not create log: Compliance Assessment Declined already exists.", () =>
			{
				AssertEquals("When Status Is Not UNI, not Declined again", false, complianceRiskStatus.DeclinedAssessmentWorkflow());
				AssertEquals(true, complianceRiskStatus.IsAssessmentDeclined);
			});

			CombineAssertions("Execute Compliance Assessment Initialized", () =>
			{
				AssertEquals(true, complianceRiskStatus.InitializeAssessmentWorkflow());
				AssertEquals(true, complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Incomplete, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
			});

			CombineAssertions("Should create StmALog log: Compliance Assessment Initialized.", () =>
			{
				var log = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ComplianceRiskInteraction.Code));
				AssertEquals(2, log.Length);
				AssertEquals(true, log.Any(u => u.SL_Reference == "|MST=Compliance Assessment|NEW=CAI|TYP=USR"));
				AssertEquals(true, log.Any(u => u.SL_Reference == "|MST=Compliance Assessment|NEW=CAD"));
			});

			CombineAssertions("Should create StmComplianceEvent log: Compliance Assessment Initialized.", () =>
			{
				var eventLog = complianceRiskStatus.GetEventLogs().Where(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
				AssertEquals(2, eventLog.Count());
				AssertEquals(true, eventLog.Any(u => u.SCE_EventSubType == "CAI" && u.SCE_EventReference == "|MST=Compliance Assessment|NEW=CAI|TYP=USR"));
				AssertEquals(true, eventLog.Any(u => u.SCE_EventSubType == "CAD" && u.SCE_EventReference == "|MST=Compliance Assessment|NEW=CAD"));
			});
		}

		public void TestComplianceJobDirection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var complianceJobDirection = (shipment as IComplianceJobDirectionProvider);

			shipment.JS_RL_NKOrigin = "";
			shipment.JS_RL_NKDestination = "";
			AssertEquals(true, shipment.IsUnknown());
			AssertEquals(false, complianceJobDirection.IsInternational);
			AssertEquals("Unknown", complianceJobDirection.JobDirection.ToString());

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals(true, shipment.IsExport());
			AssertEquals(true, complianceJobDirection.IsInternational);
			AssertEquals("Export", complianceJobDirection.JobDirection.ToString());

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals(true, shipment.IsImport());
			AssertEquals(true, complianceJobDirection.IsInternational);
			AssertEquals("Import", complianceJobDirection.JobDirection.ToString());

			shipment.JS_RL_NKOrigin = "INBOM";
			shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals(true, shipment.IsCrossTrade());
			AssertEquals(true, complianceJobDirection.IsInternational);
			AssertEquals("CrossTrade", complianceJobDirection.JobDirection.ToString());

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals(true, shipment.IsDomestic());
			AssertEquals(false, complianceJobDirection.IsInternational);
			AssertEquals("Domestic", complianceJobDirection.JobDirection.ToString());
		}

		public void TestInitializeAssessmentWorkflowAndApplyComplianceWithoutSave()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Unknown;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "123";

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "456";

			complianceRiskStatus.InitializeAssessmentWorkflowAndApplyComplianceWithoutSave();

			AssertEquals(false, complianceRiskStatus.IsInDatabase);

			CombineAssertions("Should create StmALog log: Compliance Assessment Initialized.", () =>
			{
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);

				var log = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ComplianceRiskInteraction.Code)).Single();
				AssertEquals("|MST=Compliance Assessment|NEW=CAI|TYP=RUL", log.SL_Reference);
				AssertEquals("Compliance Assessment Initialized by Compliance Rule", log.DisplayEventReference);
			});

			CombineAssertions("Should create StmComplianceEvent log: Compliance Assessment Initialized.", () =>
			{
				var eventLog = complianceRiskStatus.GetEventLogs().Single(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
				AssertNotNull(eventLog);
				AssertEquals("CRI", eventLog.SCE_EventType);
				AssertEquals("CAI", eventLog.SCE_EventSubType);
				AssertEquals("|MST=Compliance Assessment|NEW=CAI|TYP=RUL", eventLog.SCE_EventReference);
			});

			CombineAssertions("Should create StmComplianceEvent log: Compliance Value Obtained.", () =>
			{
				var eventLog = complianceRiskStatus.GetEventLogs().Single(u => u.SCE_EventType == "CVO");
				AssertEquals("CPC", eventLog.SCE_EventSubType);
				AssertEquals(2, eventLog.SCE_ItemsCount);
				AssertEquals(ZArchitecture.Environment.User.ServiceUserCode, eventLog.SCE_SystemCreateUser);
				AssertEquals(ZArchitecture.Environment.User.ServiceUserCode, eventLog.SCE_SystemLastEditUser);
			});
		}

		public void TestInitializeAssessmentWithNoCommodities_CommodityAggregateRiskStatusIncomplete()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			AssertEquals("Precondition: Commodity Aggregate risk status UNK - Unknown", ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);

			complianceRiskStatus.InitializeAssessmentWorkflow();

			var eventLog = complianceRiskStatus.GetEventLogs().Where(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
			CombineAssertions("Should change Commodity Aggregate risk status INC - Incomplete", () =>
			{
				AssertEquals(1, eventLog.Count());
				AssertEquals(true, eventLog.Any(u => u.SCE_EventSubType == "CAI" && u.SCE_EventReference == "|MST=Compliance Assessment|NEW=CAI|TYP=USR"));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Incomplete, complianceRiskStatus.COR_CommodityRisk);
			});
		}

		public void TestResetCommoditiesAggregateAndLineRiskStatus_WhenInitializeAssessment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "123";

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "456";

			CombineAssertions("Precondition:", () =>
			{
				AssertEquals("Commodity Aggregate risk status UNK - Unknown", ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals("Commodity (1) risk status NCH - Not Checked", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals("Commodity (2) risk status NCH - Not Checked", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity2.CCD_RiskStatus);
			});

			complianceRiskStatus.InitializeAssessmentWorkflow();

			var eventLog = complianceRiskStatus.GetEventLogs().Where(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
			CombineAssertions("Should change Commodity Aggregate & Line risk status (Unknown & Not Checked)", () =>
			{
				AssertEquals(1, eventLog.Count());
				AssertEquals(true, eventLog.Any(u => u.SCE_EventSubType == "CAI" && u.SCE_EventReference == "|MST=Compliance Assessment|NEW=CAI|TYP=USR"));
				AssertEquals("Commodity Aggregate risk status UNK - Unknown", ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals("Commodity (1) risk status NCH - Not Checked", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals("Commodity (2) risk status NCH - Not Checked", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity2.CCD_RiskStatus);
			});
		}

		public void TestResetCommoditiesAggregateAndLineRiskStatus_WhenDeclinedThenInitializeAssessment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "123";

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "456";

			CombineAssertions("Precondition:", () =>
			{
				AssertEquals("Commodity Aggregate risk status UNK - Unknown", ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals("Commodity (1) risk status NCH - Not Checked", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals("Commodity (2) risk status NCH - Not Checked", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity2.CCD_RiskStatus);
			});

			complianceRiskStatus.DeclinedAssessmentWorkflow();

			var eventLog = complianceRiskStatus.GetEventLogs().Where(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
			CombineAssertions("Should change Commodity Aggregate & Line risk status PRS - Possible Risk", () =>
			{
				AssertEquals(1, eventLog.Count());
				AssertEquals(true, eventLog.Any(u => u.SCE_EventSubType == "CAD" && u.SCE_EventReference == "|MST=Compliance Assessment|NEW=CAD"));
				AssertEquals("Commodity Aggregate risk status PRS - Possible Risk", ComplianceRiskStatusCodeList.Codes.PossibleRisk, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals("Commodity (1) risk status PRS - Possible Risk", ComplianceRiskStatusCodeList.Codes.PossibleRisk, commodity1.CCD_RiskStatus);
				AssertEquals("Commodity (2) risk status PRS - Possible Risk", ComplianceRiskStatusCodeList.Codes.PossibleRisk, commodity2.CCD_RiskStatus);
			});

			complianceRiskStatus.InitializeAssessmentWorkflow();

			eventLog = complianceRiskStatus.GetEventLogs().Where(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
			CombineAssertions("Should change Commodity Aggregate & Line risk status (Unknown & Not Checked)", () =>
			{
				AssertEquals(2, eventLog.Count());
				AssertEquals(true, eventLog.Any(u => u.SCE_EventSubType == "CAI" && u.SCE_EventReference == "|MST=Compliance Assessment|NEW=CAI|TYP=USR"));
				AssertEquals("Commodity Aggregate risk status UNK - Unknown", ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals("Commodity (1) risk status NCH - Not Checked", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals("Commodity (2) risk status NCH - Not Checked", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity2.CCD_RiskStatus);
			});
		}

		public void TestResetCommoditiesAggregateAndLineRiskStatus_WhenDeclinedAssessmentWithCommodities()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123";

			AssertEquals("Precondition: Commodity Aggregate risk status UNK - Unknown", ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
			AssertEquals("Commodity Line risk status NOT - Not Checked", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity.CCD_RiskStatus);

			complianceRiskStatus.DeclinedAssessmentWorkflow();

			var eventLog = complianceRiskStatus.GetEventLogs().Where(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
			CombineAssertions("Should create StmALog log: Compliance Assessment Declined - Commodity Aggregate risk status PRS - Possible Risk", () =>
			{
				AssertEquals(1, eventLog.Count());
				AssertEquals(true, eventLog.Any(u => u.SCE_EventSubType == "CAD" && u.SCE_EventReference == "|MST=Compliance Assessment|NEW=CAD"));
				AssertEquals("Commodity Aggregate risk status PRS - Possible Risk", ComplianceRiskStatusCodeList.Codes.PossibleRisk, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals("Commodity Line risk status PRS - Possible Risk", ComplianceRiskStatusCodeList.Codes.PossibleRisk, commodity.CCD_RiskStatus);
			});
		}

		public void TestAssessmentDecisionRequiredWorkflow()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			complianceRiskStatus.AssessmentDecisionRequiredWorkflow();

			CombineAssertions("Should create StmComplianceEvent log: Compliance Assessment Decision Required", () =>
			{
				var eventLog = complianceRiskStatus.GetEventLogs().Single(u => u.SCE_EventType == AutoEvents.ComplianceRiskInteraction.Code);
				AssertNotNull(eventLog);
				AssertEquals("CRI", eventLog.SCE_EventType);
				AssertEquals("REQ", eventLog.SCE_EventSubType);
				AssertEquals("|MST=Compliance Assessment|NEW=REQ", eventLog.SCE_EventReference);
			});

			CombineAssertions("Should create StmALog log: Compliance Assessment Decision Required", () =>
			{
				var log = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ComplianceRiskInteraction.Code)).Single();
				AssertEquals("|MST=Compliance Assessment|NEW=REQ", log.SL_Reference);
			});
		}

		public void TestAssessmentStatusChangedNoticeLinkedModuleCommodity()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = "JE";
			complianceRiskStatus.COR_ParentID = declaration.PK;

			var helper = new DummyInteractionWithComplianceWiseCommoditiesHelper();
			((ISupportInteractionWithComplianceWiseCommodities)declaration).Helper = helper;

			AssertEquals(0, helper.AssessmentStatusChangedCount);
			complianceRiskStatus.InitializeAssessmentWorkflow();
			AssertEquals(1, helper.AssessmentStatusChangedCount);
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
