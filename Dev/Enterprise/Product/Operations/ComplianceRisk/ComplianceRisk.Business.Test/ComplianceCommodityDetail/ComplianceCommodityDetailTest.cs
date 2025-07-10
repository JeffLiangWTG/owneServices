using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceCommodityDetail))]
	public class ComplianceCommodityDetailTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.FillWithValidTestData();

			return commodityDetail;
		}

		public void TestDefaultRiskStatusValue()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodityDetail.CCD_RiskStatus);
		}

		public void TestDefaultAssessmentNotesValue()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			AssertNullOrEmpty(commodityDetail.CCD_AssessmentNotes);
		}

		public override void TestFetchForLoad()
		{
			var commodityDetail = GetNewBusinessObject();
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertNotNull(newFactory.Load<ComplianceCommodityDetail>(commodityDetail.PK));
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			var commodityDetail = GetNewBusinessObject();
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertNotNull(newFactory.Load<ComplianceCommodityDetail>(commodityDetail.PK));
			AssertEquals("Fetch hints should be used", 0, newFactory.ActiveTableFetchHints);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var commodityDetail = GetNewBusinessObject();
			Factory.Save();

			commodityDetail.Delete();
			Factory.Save();

			AssertNull(Factory.Load<ComplianceCommodityDetail>(commodityDetail.PK));
		}

		public void TestCCD_RiskStatusDescription()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;
			commodityDetail.RelatedJobIsAssessmentInitialized = true;
			commodityDetail.CommodityType = CommodityType.RelatedJobLink;
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Clear, commodityDetail.CCD_RiskStatusDescription);

			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.PotentialRisk, commodityDetail.CCD_RiskStatusDescription);
		}

		public void TestImportAlertsForExportJobDescription()
		{
			var job = Factory.NewWithValidTestData<ForwardingShipment>();
			var status = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			status.COR_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			status.COR_ParentID = job.PK;
			status.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			status.InitializeAssessmentWorkflow();

			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;
			commodityDetail.CCD_COR_ComplianceRisk = status.PK;

			commodityDetail.ImportAlertsForExportJobDescription = "HSK";
			AssertEquals("High Risk", commodityDetail.ImportAlertsForExportJobDescription);

			commodityDetail.ImportAlertsForExportJobDescription = "CLR";
			AssertEquals("Clear", commodityDetail.ImportAlertsForExportJobDescription);

			commodityDetail.ImportAlertsForExportJobDescription = "PRS";
			AssertEquals("Possible Risk", commodityDetail.ImportAlertsForExportJobDescription);
		}

		public void TestComplianceCommodityDetailIsReadOnly_WithConditionsInfoIsValidationEnabled()
		{
			var commodityDetail = Factory.NewWithValidTestData<ComplianceCommodityDetail>();
			commodityDetail.ReadOnly = true;

			var conditionsInfo = commodityDetail.ConditionsInfo;
			var validationEnabled = conditionsInfo.BizObj.IsValidationEnabled(conditionsInfo);

			AssertEquals("Validation should be enabled", true, validationEnabled);
		}

		public void TestComplianceCommodityDetail_HarmonizedCodeReadOnly()
		{
			var commodityDetail = Factory.New<ComplianceCommodityDetail>();
			AssertEquals(false, commodityDetail.CCD_HarmonizedCodeInfo.ReadOnly);

			commodityDetail.CommodityType = CommodityType.RelatedJobLink;
			AssertEquals(true, commodityDetail.CCD_HarmonizedCodeInfo.ReadOnly);

			commodityDetail.CommodityType = CommodityType.FetchDataEntry;
			AssertEquals(true, commodityDetail.CCD_HarmonizedCodeInfo.ReadOnly);

			commodityDetail.CommodityType = CommodityType.UserDataEntry;
			AssertEquals(false, commodityDetail.CCD_HarmonizedCodeInfo.ReadOnly);

			commodityDetail.ReadOnly = true;
			AssertEquals(true, commodityDetail.CCD_HarmonizedCodeInfo.ReadOnly);
		}

		public void TestComplianceCommodityDetail_AssessmentPropertiesReadOnly()
		{
			var job = Factory.NewWithValidTestData<ForwardingShipment>();
			var status = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			status.COR_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			status.COR_ParentID = job.PK;
			status.COR_PartyRisk = "CLR";
			status.COR_LocationRisk = "CLR";
			status.COR_OverallRisk = "CLR";
			var commodityDetail = status.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_COR_ComplianceRisk = status.PK;
			Factory.Save();

			AssertEquals(true, commodityDetail.CCD_RiskStatusInfo.ReadOnly);
			AssertEquals(true, commodityDetail.CCD_RiskStatusDescriptionInfo.ReadOnly);
			AssertEquals(true, commodityDetail.CCD_AssessmentNotesInfo.ReadOnly);

			ComplianceRiskStatusWorkflowInitializer.InitializeAssessmentWorkflow(status);
			AssertEquals(false, commodityDetail.CCD_RiskStatusInfo.ReadOnly);
			AssertEquals(false, commodityDetail.CCD_RiskStatusDescriptionInfo.ReadOnly);
			AssertEquals(false, commodityDetail.CCD_AssessmentNotesInfo.ReadOnly);

			commodityDetail.ReadOnly = true;
			AssertEquals(true, commodityDetail.CCD_RiskStatusInfo.ReadOnly);
			AssertEquals(true, commodityDetail.CCD_RiskStatusDescriptionInfo.ReadOnly);
			AssertEquals(true, commodityDetail.CCD_AssessmentNotesInfo.ReadOnly);
		}

		public void TestComplianceCommodityDetail_CanDelete()
		{
			var commodityDetail = Factory.New<ComplianceCommodityDetail>();
			commodityDetail.CommodityType = CommodityType.Unknown;
			AssertEquals(false, commodityDetail.CanDelete);

			commodityDetail.CommodityType = CommodityType.UserDataEntry;
			AssertEquals(true, commodityDetail.CanDelete);

			AssertEquals("Cannot delete the commodity not added by the user.", commodityDetail.ReasonForNotAbleToDelete);
		}

		public void TestComplianceCommodityDetail_ShowLegalBooksLinkForMasterShipmentOnlyAndNotForChildShipments()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = "CLD";

			var masterShipmentComplianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			masterShipmentComplianceRiskStatus.COR_ParentID = masterShipment.PK;
			masterShipmentComplianceRiskStatus.COR_ParentTableCode = masterShipment.TablePrefix;

			var subShipment = masterShipment.CoLoadShipments.AddNew();
			var subShipmentComplianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			subShipmentComplianceRiskStatus.COR_ParentID = subShipment.PK;
			subShipmentComplianceRiskStatus.COR_ParentTableCode = subShipment.TablePrefix;

			var masterCommodityDetail = masterShipmentComplianceRiskStatus.CommodityDetailCollection.AddNew();
			masterCommodityDetail.CCD_HarmonizedCode = "123456";
			masterCommodityDetail.BorderWiseCheckStatus = BorderWiseCheckStatus.Viewable;
			masterCommodityDetail.CCD_SpecificCondition = true;

			var subCommodityDetail = subShipmentComplianceRiskStatus.CommodityDetailCollection.AddNew();
			subCommodityDetail.CCD_HarmonizedCode = "789012";
			subCommodityDetail.BorderWiseCheckStatus = BorderWiseCheckStatus.Viewable;

			Factory.Save();

			var masterShipmentComplianceBizO = new ComplianceRiskBusinessObject(masterShipment);
			var commodityDetailCollection = new ComplianceCommodityDetailCollection(masterShipmentComplianceRiskStatus, masterShipmentComplianceBizO?.ComplianceCommodityRiskStatusProvider);
			commodityDetailCollection.Load();

			CombineAssertions("Commodity Grid", () =>
			{
				AssertEquals("Column Compliance Alerts link should be empty", true, commodityDetailCollection.Cast<ComplianceCommodityDetail>().Any(u => u.CCD_HarmonizedCode == "789012" && u.LegalBookLink.IsEmpty));
				AssertEquals("Column Compliance Alerts link should not be empty", true, commodityDetailCollection.Cast<ComplianceCommodityDetail>().Any(u => u.CCD_HarmonizedCode == "123456" && u.LegalBookLink == "View"));
			});

			CombineAssertions("Compliance Assessment Panel", () =>
			{
				AssertEquals("Hyperlink Compliance Alerts label should be empty", true, commodityDetailCollection.Cast<ComplianceCommodityDetail>().Any(u => u.CCD_HarmonizedCode == "789012" && u.HarmonizedBorderWiseTextual.IsEmpty));
				AssertEquals("Hyperlink Compliance Alerts label should not be empty", true, commodityDetailCollection.Cast<ComplianceCommodityDetail>().Any(u => u.CCD_HarmonizedCode == "123456" && u.HarmonizedBorderWiseTextual == "View Compliance Alerts for 123456"));
			});
		}

		public void TestHarmonizedBorderWiseTextual()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodityDetail1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail1.CCD_HarmonizedCode = "012373";
			commodityDetail1.CCD_SpecificCondition = true;
			commodityDetail1.BorderWiseCheckStatus = BorderWiseCheckStatus.Viewable;

			var commodityDetail2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail2.CCD_HarmonizedCode = string.Empty;
			commodityDetail2.BorderWiseCheckStatus = BorderWiseCheckStatus.Viewable;

			var complianceBizO = new ComplianceRiskBusinessObject(shipment);
			var commodityDetailColection = new ComplianceCommodityDetailCollection(complianceRiskStatus, complianceBizO?.ComplianceCommodityRiskStatusProvider);
			commodityDetailColection.Load();

			CombineAssertions("HarmonizedBorderWiseTextual:", () =>
			{
				AssertEquals("Hyperlink Compliance Alerts label should be empty", true, commodityDetailColection.Cast<ComplianceCommodityDetail>().Any(u => u.CCD_HarmonizedCode.IsEmpty && u.HarmonizedBorderWiseTextual.IsEmpty));
				AssertEquals("Hyperlink Compliance Alerts label should not be empty", true, commodityDetailColection.Cast<ComplianceCommodityDetail>().Any(u => u.CCD_HarmonizedCode == "012373" && u.HarmonizedBorderWiseTextual == "View Compliance Alerts for 012373"));
			});
		}

		public void TestOriginOfGoods()
		{
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.OriginOfGoods = "AU, US, NZ";
			AssertContainsExactElementsInAnyOrder(new[] { "AU", "US", "NZ" }, commodityDetail.Origins);
		}

		public void TestComplianceDecisionChanged()
		{
			var commodityDetail = Factory.New<ComplianceCommodityDetail>();
			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals(false, commodityDetail.IsComplianceDecisionChanged);
				AssertEquals(string.Empty, commodityDetail.CCD_AssessmentNotes);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodityDetail.CCD_RiskStatus);
			});

			commodityDetail.CCD_AssessmentNotes = "123";
			AssertEquals(true, commodityDetail.IsComplianceDecisionChanged);

			commodityDetail.IsComplianceDecisionChanged = false;
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			AssertEquals(true, commodityDetail.IsComplianceDecisionChanged);

			commodityDetail.OnSaving();
			AssertEquals(false, commodityDetail.IsComplianceDecisionChanged);
		}

		public void TestAddComplianceEventLogOnceOnSaving()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG 1";

			shipment.JS_OH_ExportBroker = org1.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

			pluginBizO.RefreshData();

			var complianceCommodityDetail = pluginBizO.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = pluginBizO.ComplianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;
			complianceCommodityDetail.CCD_HarmonizedCode = "123";
			complianceCommodityDetail.Conditions = "CON";
			complianceCommodityDetail.Description = "DES";
			complianceCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk;

			var screeningLog_Org1 = Factory.New<IStmEntityScreeningLog>();
			screeningLog_Org1.PJ_ParentID = org1.PK;
			screeningLog_Org1.PJ_ParentTableCode = "OH";
			screeningLog_Org1.PJ_SourceID = shipment.PK;
			screeningLog_Org1.PJ_SourceTableCode = shipment.TablePrefix;
			screeningLog_Org1.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges;

			var screeningLog_CountryAU = Factory.New<IStmEntityScreeningLog>();
			screeningLog_CountryAU.PJ_ParentID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU").PK;
			screeningLog_CountryAU.PJ_ParentTableCode = RefCountrySchema.Constants.Prefix;
			screeningLog_CountryAU.PJ_SourceID = shipment.PK;
			screeningLog_CountryAU.PJ_SourceTableCode = shipment.TablePrefix;
			screeningLog_CountryAU.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.UserDecisionsRemoveSanctions;

			Factory.Save();

			complianceCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			complianceCommodityDetail.CCD_AssessmentNotes = "Manually Released";
			var complianceEvent = complianceRiskStatus.GetEventLogs().Where(c => c.SCE_EventSubType == ComplianceEventList.Codes.ComplianceDecisionChanged).ToArray();
			AssertEquals(0, complianceEvent.Length);

			Factory.Save();
			complianceEvent = complianceRiskStatus.GetEventLogs().Where(c => c.SCE_EventSubType == ComplianceEventList.Codes.ComplianceDecisionChanged).ToArray();
			AssertEquals(1, complianceEvent.Length);
			var complianceRiskStatusChangeLog = new ComplianceRiskStatusChangeLog(complianceEvent[0]);

			CombineAssertions(() =>
			{
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatusChangeLog.OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Held, complianceRiskStatusChangeLog.OverallRiskDescription);
				AssertEquals(complianceEvent[0].SCE_EventTimeOffset.ToDateTime(), complianceRiskStatusChangeLog.EventDateTime);
				AssertEquals(Environment.Env.CurrentUser.FullName, complianceRiskStatusChangeLog.User);
				AssertEquals(1, complianceRiskStatusChangeLog.CommodityRiskLogCollection.Count);
				AssertEquals(screeningLog_Org1.PK, complianceRiskStatusChangeLog.PartyRiskLogCollection.Cast<CompliancePartyRiskLog>().Single().EntityScreeningLog.PK);
				AssertEquals("Not Screened", complianceRiskStatusChangeLog.PartyRiskLogCollection.Cast<CompliancePartyRiskLog>().Single().StatusDescription);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatusChangeLog.PartyRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatusChangeLog.LocationRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatusChangeLog.CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Descriptions.HighRisk, complianceRiskStatusChangeLog.PartyRiskDescription);
				AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Clear, complianceRiskStatusChangeLog.LocationRiskDescription);
				AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Clear, complianceRiskStatusChangeLog.CommodityRiskDescription);
				AssertEquals(true, complianceRiskStatusChangeLog.SnapshotExists);
				AssertEquals(string.Empty, complianceRiskStatusChangeLog.ClearedReason);
			});
		}

		public void TestGoodsDescription()
		{
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			var descriptionMessage = "Long Description".PadRight(ComplianceCommodityDetailSchema.CCD_Description.MaxLength + 1, 'L');

			commodityDetail.CCD_Description = descriptionMessage;

			AssertEquals(descriptionMessage.Substring(0, ComplianceCommodityDetailSchema.CCD_Description.MaxLength), commodityDetail.CCD_Description);
		}

		public void TestAssessmentInitializedShouldBeFalseWhenRelatedJobIsAssessmentInitializedFalse()
		{
			var commodityDetail = Factory.New<ComplianceCommodityDetail>();
			commodityDetail.CommodityType = CommodityType.RelatedJobLink;
			commodityDetail.RelatedJobIsAssessmentInitialized = true;
			AssertEquals(true, commodityDetail.AssessmentInitialized);

			commodityDetail.CommodityType = CommodityType.RelatedJobLink;
			commodityDetail.RelatedJobIsAssessmentInitialized = false;
			AssertEquals(false, commodityDetail.AssessmentInitialized);
		}

		public void TestAssessmentDeclinedSetRiskStatusToPossibleRisk()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;

			var eventLog = Factory.NewWithValidTestData<StmComplianceEvent>();
			eventLog.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			eventLog.SCE_EventSubType = ComplianceEventList.Codes.AssessmentDeclined;
			eventLog.SCE_ParentID = shipment.PK;

			Factory.Save();

			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			AssertEquals(ComplianceRiskStatusCodeList.Codes.PossibleRisk, commodityDetail.CCD_RiskStatus);
		}

		public void TestTwoUsersSaveDuplicateCommodity_UniqueIndexFailureHandler()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			Factory.Save();

			Db.Connection.ExecuteNonQuery($@"INSERT INTO dbo.ComplianceCommodityDetail(CCD_PK, CCD_COR_ComplianceRisk, CCD_HarmonizedCode, CCD_CountryOrGrouping,CCD_RN_NKOrigin,CCD_Description, CCD_SystemCreateTimeUtc, CCD_SystemCreateUser, CCD_SystemLastEditTimeUtc, CCD_SystemLastEditUser, CCD_RiskStatus, CCD_AssessmentNotes)
VALUES(NEWID(), '{complianceRiskStatus.PK}', '123', 'WCO', 'US', 'TEST', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'HSK', 'TEST')");

			var complianceCommodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;
			complianceCommodityDetail.CCD_HarmonizedCode = "123";
			complianceCommodityDetail.CCD_Description = "TEST";
			complianceCommodityDetail.CCD_RN_NKOrigin = "US";
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk;

			var notifier = new MockNotificationHandler();
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() => Factory.Save(), () => { }, notifier: notifier);

			AssertEquals("While you have been working with this form, another user has made changes.\r\n\r\nThe system will now try to combine your changes with those of the other user.\r\nPlease review the form carefully before clicking the 'Save' button again.\r\n\r\nDuplicate Commodity: Harmonized Code('123')|Description('TEST')|Origin Of Goods('US') must be unique on Compliance Commodity.", notifier.LastMessage);
			AssertEquals("Duplicate Commodity", notifier.LastCaption);
		}

		public void TestInsertDuplicateCommodity_UniqueIndexFailureHandler()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			Factory.Save();
			var complianceCommodityDetail1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail1.CCD_HarmonizedCode = "123456";
			complianceCommodityDetail1.CCD_Description = "52";
			complianceCommodityDetail1.CCD_RN_NKOrigin = "US";
			complianceCommodityDetail1.CCD_CountryOrGrouping = "WCO";

			var complianceCommodityDetail2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail2.CCD_HarmonizedCode = "123456";
			complianceCommodityDetail2.CCD_Description = "5²";
			complianceCommodityDetail2.CCD_RN_NKOrigin = "US";
			complianceCommodityDetail2.CCD_CountryOrGrouping = "WCO";
			AssertNoExceptionThrown(() => Factory.Save());
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

		class MockNotificationHandler : INotificationHandler
		{
			public string LastMessage;
			public string LastCaption;

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				throw new NotSupportedException();
			}

			public void ReportInformation(string message, string caption)
			{
				LastMessage = message;
				LastCaption = caption;
			}
		}
	}
}
