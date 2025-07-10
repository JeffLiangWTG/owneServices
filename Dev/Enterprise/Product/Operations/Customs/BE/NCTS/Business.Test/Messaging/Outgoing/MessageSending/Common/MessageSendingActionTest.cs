using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(MessageSendingAction))]
	sealed class MessageSendingActionTest : EU.NCTS.Business.Testing.NctsHeaderMessageSendingObjectTest
	{
		public new void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new MessageSendingAction(null));
		}

		public void TestProperties()
		{
			EU.NCTS.Business.Testing.NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", commonMovement.Representative, "1");

			commonMovement.BM_AdditionalDeclarationType = "X";
			commonMovement.BM_PaperlessInbondNum = "Y";
			commonMovement.BM_CustomsStatus = "Z";
			commonMovement.BM_ArrivalDate = ZDateTime.BrettsBirthday;

			commonMovement.Representative.Organisation.SetCustomsCode(OrgCusCode.CodeTypes.BrokerageRegistration, RefCountry.LoadFromCountryCode(Factory, "BE"), "CBR123");

			var mrn = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN123";
			messageSendingAction.MessageType = "MTP";
			messageSendingAction.IsTestDeclaration = true;
			messageSendingAction.AgreeWithMinorDiscrepancies = true;

			CombineAssertions(() =>
			{
				AssertEquals("SubApplicationCode value", "D", messageSendingAction.SubApplicationCode);
				AssertEquals("AdditionalDeclarationType value", "X", messageSendingAction.AdditionalDeclarationType);
				AssertEquals("AdditionalDeclarationType ReadOnly", true, messageSendingAction.AdditionalDeclarationTypeInfo.ReadOnly);
				AssertEquals("LRN value", "Y", messageSendingAction.LRN);
				AssertEquals("LRN ReadOnly", true, messageSendingAction.LRNInfo.ReadOnly);
				AssertEquals("MRN value", "MRN123", messageSendingAction.MRN);
				AssertEquals("MRN ReadOnly", true, messageSendingAction.MRNInfo.ReadOnly);
				AssertEquals("MessageType value", "MTP", messageSendingAction.MessageType);
				AssertEquals("EntryStatus value", "Z", messageSendingAction.EntryStatus);
				AssertEquals("EntryStatus ReadOnly", true, messageSendingAction.EntryStatusInfo.ReadOnly);
				AssertEquals("PresentationDate value", ZDateTime.BrettsBirthday, messageSendingAction.PresentationDateTime);
				AssertEquals("IsTestDeclaration value", true, messageSendingAction.IsTestDeclaration);
				AssertEquals("AgreeWithMinorDiscrepancies value", true, messageSendingAction.AgreeWithMinorDiscrepancies);
				AssertEquals("HasRepresentative value", true, messageSendingAction.HasRepresentative);
				AssertEquals("RepresentativeCBRNumber value", "CBR123", messageSendingAction.RepresentativeCBRNumber);
				AssertEquals("RepresentativeCBRNumber ReadOnly", true, messageSendingAction.RepresentativeCBRNumberInfo.ReadOnly);
			});
		}

		public new void TestShouldSend()
		{
			commonMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			var messageSendingAction1 = new MessageSendingAction(commonMovement);
			var orgEntryType = messageSendingAction1.EntryType;
			CombineAssertions(() =>
			{
				messageSendingAction1.ShouldSend = true;
				messageSendingAction1.EntryType = "";
				AssertEquals("ShouldSend is not ticked when no entry type", false, messageSendingAction1.ShouldSend);
				messageSendingAction1.ShouldSend = true;
				messageSendingAction1.EntryType = orgEntryType;
				AssertEquals($"ShouldSend is ticked with entry type {messageSendingAction1.EntryType}", true, messageSendingAction1.ShouldSend);
				messageSendingAction1.ShouldSend = true;
				messageSendingAction1.EntryType = "XX";
				AssertEquals($"ShouldSend is not ticked with invalid entry type {messageSendingAction1.EntryType}", false, messageSendingAction1.ShouldSend);
			});
		}

		public void TestShouldSend_ReadOnly()
		{
			commonMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			var messageSendingAction1 = new MessageSendingAction(commonMovement);
			var shouldSendInfo = messageSendingAction1.ShouldSendInfo;
			CombineAssertions(() =>
			{
				AssertEquals("ShouldSend is not ReadOnly", false, shouldSendInfo.ReadOnly);
				messageSendingAction1.EntryType = "";
				AssertEquals("ShouldSend is ReadOnly", true, shouldSendInfo.ReadOnly);
				messageSendingAction1.EntryType = "XX";
				AssertEquals("ShouldSend is ReadOnly", true, shouldSendInfo.ReadOnly);
			});
		}

		public void TestEntryType_MaxLength()
		{
			AssertEquals(3, messageSendingAction.EntryTypeInfo.MaxLength);
		}

		public void TestEntryType_Lookup()
		{
			AssertEquals("Lookups.EntryTypeList", messageSendingAction.EntryTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestGetDefaultEntryType()
		{
			commonMovement.BM_SubApplicationCode = "D";
			commonMovement.BM_AdditionalDeclarationType = "A";
			commonMovement.BM_CustomsStatus = string.Empty;
			commonMovement.BM_Phase = string.Empty;
			var goodsLocation = ((NctsDepartureMovementHeader)commonMovement).GoodsLocation;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			goodsLocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;

			var messageSendingAction1 = new MessageSendingAction(commonMovement);

			CombineAssertions(() =>
			{
				AssertEquals("EntryTypeList has only one code", 1, messageSendingAction1.Lookups.EntryTypeList.Count);
				AssertEquals("EntryType uses the only code", NctsMessageTypeList.Codes.Declaration, messageSendingAction1.EntryType);
				AssertEquals("ShouldSend is thicked", true, messageSendingAction1.ShouldSend);
				commonMovement.BM_CustomsStatus = "MRN";
				commonMovement.BM_Phase = "015";
				var messageSendingAction2 = new MessageSendingAction(commonMovement);
				AssertEquals("EntryTypeList has multiple codes", 2, messageSendingAction2.Lookups.EntryTypeList.Count);
				AssertNullOrEmpty("EntryType is empty", messageSendingAction2.EntryType);
				AssertEquals("ShouldSend not thicked", false, messageSendingAction2.ShouldSend);
			});
		}

		public void TestPresentationDateTime_Arrival_ReadOnly()
		{
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovement = arrivalHeader.ArrivalMovementHeader;
			var arrivalMessageSendingAction = new MessageSendingAction(arrivalMovement);

			var presentationDateTimeInfo = arrivalMessageSendingAction.PresentationDateTimeInfo;
			CombineAssertions(() =>
			{
				arrivalMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				AssertEquals("PresentationDateTime should be ReadOnly", true, presentationDateTimeInfo.ReadOnly);
				arrivalMovement.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
				AssertEquals("PresentationDateTime should not be ReadOnly", false, presentationDateTimeInfo.ReadOnly);
			});
		}

		public void TestPresentationDateTime_Departure_ReadOnly()
		{
			var presentationDateTimeInfo = messageSendingAction.PresentationDateTimeInfo;
			CombineAssertions(() =>
			{
				commonMovement.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationRejected;
				commonMovement.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.MrnAllocated;
				AssertEquals("PresentationDateTime should not be ReadOnly for DRJ, DMA", false, presentationDateTimeInfo.ReadOnly);

				commonMovement.BM_CustomsStatus = NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent;
				AssertEquals("PresentationDateTime should be ReadOnly for MAM", true, presentationDateTimeInfo.ReadOnly);

				commonMovement.BM_CustomsStatus = NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent;
				messageSendingAction.EntryType = NctsMessageTypeList.Codes.Amendment;
				AssertEquals("PresentationDateTime should not be ReadOnly for AMD, unless log with GIV reference exists", false, presentationDateTimeInfo.ReadOnly);

				nctsHeader.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: ZDateTimeOffset.Now, reference: "GIV"));
				messageSendingAction = new MessageSendingAction(commonMovement);
				messageSendingAction.EntryType = NctsMessageTypeList.Codes.Amendment;
				AssertEquals("PresentationDateTime should be ReadOnly for AMD and log with GIV reference exists", true, messageSendingAction.PresentationDateTimeInfo.ReadOnly);
			});
		}

		public void TestArrivalDate()
		{
			commonMovement.BM_ArrivalDate = ZDateTime.Empty;
			messageSendingAction.PresentationDateTime = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, commonMovement.BM_ArrivalDate);
		}

		public void TestMessageSendingActionLookups()
		{
			AssertType<MessageSendingActionLookups>(messageSendingAction.Lookups);
		}

		public void TestAdditionalDeclarationType_Caption()
		{
			AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.AdditionalDeclarationTypeInfo).Caption);
		}

		public void TestLRN_Caption()
		{
			AssertEquals("Reference Number", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.LRNInfo).Caption);
		}

		public void TestPresentationDateTime_Caption()
		{
			AssertEquals("Presentation Date And Time", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.PresentationDateTimeInfo).Caption);
		}

		public void TestTCI11_Caption()
		{
			AssertEquals("TCI11 Date", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.TCI11Info).Caption);
		}

		public void QueryInformation_Caption()
		{
			AssertEquals("Query Information", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.QueryInformationInfo).Caption);
		}

		public void QueryInformation_MaxLength()
		{
			AssertEquals(200, messageSendingAction.QueryInformationInfo.MaxLength);
		}

		public void EntryType_Caption()
		{
			AssertEquals("Entry Type", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.EntryTypeInfo).Caption);
		}

		public void EntryStatus_Caption()
		{
			AssertEquals("Entry Status", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.EntryStatusInfo).Caption);
		}

		public void Justification_Caption()
		{
			AssertEquals("Justification", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.JustificationInfo).Caption);
		}

		public void TestIsTestDeclaration_Caption()
		{
			AssertEquals("Test?", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.IsTestDeclarationInfo).Caption);
		}

		public void TestAgreeWithMinorDiscrepancies_Caption()
		{
			AssertEquals("Agree with minor discrepancies?", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.AgreeWithMinorDiscrepanciesInfo).Caption);
		}

		public void TestIsPresentationDateTimeEnabledForCustomsStatusAndPhase()
		{
			var variations = GetPresentationDateTimeEnabledVariations();

			CombineAssertions(() =>
			{
				foreach (var (customsStatus, phase, enabled) in variations)
				{
					commonMovement.BM_CustomsStatus = customsStatus;
					commonMovement.BM_Phase = phase;
					AssertEquals($"CustomsStatus: {customsStatus}, Phase: {phase}, Expected Enabled: {enabled}", enabled, !messageSendingAction.PresentationDateTimeInfo.ReadOnly);
				}
			});
		}

		public void TestShowValidationErrors()
		{
			var action = new MessageSendingAction(commonMovement);

			CombineAssertions(() =>
			{
				AssertEquals("default is true", true, action.ShowValidationErrors);

				action.EntryType = NctsMessageTypeList.Codes.InvalidationCancellation;
				AssertEquals("EntryType is INV, ShowValidationErrors is false", false, action.ShowValidationErrors);

				action.EntryType = NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;
				AssertEquals("EntryType is RNM, ShowValidationErrors is false", false, action.ShowValidationErrors);

				action.EntryType = NctsMessageTypeList.Codes.RequestARelease;
				AssertEquals("EntryType is RRL, ShowValidationErrors is false", false, action.ShowValidationErrors);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => messageSendingAction;

		List<(string customsStatus, string phase, bool enabled)> GetPresentationDateTimeEnabledVariations()
		{
			var variations = new List<(string customsStatus, string phase, bool enabled)>();
			var customsStatuses = new NctsTransitStatusList().GetAllCodes().ToList<string>();
			customsStatuses.Add(NctsMessageTypeList.Codes.Amendment);
			foreach (var customsStatus in customsStatuses)
			{
				foreach (var phase in new NctsMovementHeaderTransactionStatusList().GetAllCodes())
				{
					bool enabled = ExpectPresentationDateTimeEnabled(customsStatus, phase);
					variations.Add((customsStatus, phase, enabled));
				}
			}
			return variations;
		}

		bool ExpectPresentationDateTimeEnabled(string customsStatus, string phase)
		{
			var enabledForCustomsStatus = false;
			var enabledForPhase = true;

			switch (customsStatus)
			{
				case NctsTransitStatusList.Codes.DeclarationAccepted:
				case NctsTransitStatusList.Codes.DeclarationRejected:
				case NctsTransitStatusList.Codes.RequestForAmendment:
				case NctsMessageTypeList.Codes.Amendment:
				case NctsTransitStatusList.Codes.Unknown:
					enabledForCustomsStatus = true;
					break;
			}

			if (enabledForCustomsStatus)
			{
				switch (phase)
				{
					case NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent:
					case NctsMovementHeaderTransactionStatusList.Codes.InvalidationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.DeclarationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.RequestForReleaseSent:
					case NctsMovementHeaderTransactionStatusList.Codes.InformationNonArrivedMovementSent:
					case NctsMovementHeaderTransactionStatusList.Codes.PresentationNotificationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarksSent:
						enabledForPhase = false;
						break;
				}
			}

			return enabledForCustomsStatus && enabledForPhase;
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			commonMovement = nctsHeader.MovementHeader;
			messageSendingAction = new MessageSendingAction(commonMovement);
		}
		NctsHeader nctsHeader;
		NctsCommonMovementHeader commonMovement;
		MessageSendingAction messageSendingAction;
	}
}
