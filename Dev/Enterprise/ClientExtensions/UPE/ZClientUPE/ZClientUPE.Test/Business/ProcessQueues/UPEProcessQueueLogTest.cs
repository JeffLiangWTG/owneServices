using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEProcessQueueLog))]
	internal partial class UPEProcessQueueLogTest : BaseStmALogTest
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPEProcessQueueLog>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		[ExpectNoExceptions, TestDate(2005, 1, 1)]
		public void TestThingsDoNotBlowUpWhenProcessQueueDoesNotExist()
		{
			Log.Master = null;
			AssertEquals("", ShipmentStatusData.ShipmentRef);
			AssertEquals(ZDateTime.Empty, ShipmentStatusData.ImportDate);
			AssertEquals("03", ShipmentStatusData.ShipmentStatus);
			AssertEquals("", ShipmentStatusData.HoldReasonCode);
			AssertEquals(false, ShipmentStatusData.InspectIndicator);
			AssertEquals(false, ShipmentStatusData.AddressCorrectionIndicator);
			AssertEquals(ZDateTime.Now, ShipmentStatusData.ImportReleaseDate);
			AssertEquals("", ShipmentStatusData.Remarks);
			AssertEquals("", ShipmentStatusData.ExceptionStatusCode);
			AssertEquals("", ShipmentStatusData.ExceptionResolutionCode);
			AssertEquals(ShipmentStatusType.Special, ShipmentStatusData.ShipmentStatusType);
			AssertEquals(ZDateTime.Now, ShipmentStatusData.StatusChangeDate);
			AssertEquals(false, ShipmentStatusData.HasReasonOrResolutionCode);
		}

		[ExpectNoExceptions, TestDate(2005, 1, 1)]
		public void TestThingsDoNotBlowUpWhenAssociatedCusHAWBDoesNotExist()
		{
			Log.Master = Factory.New<UPECargoReportQueue>();
			AssertEquals("", ShipmentStatusData.ShipmentRef);
			AssertEquals(ZDateTime.Empty, ShipmentStatusData.ImportDate);
			AssertEquals("03", ShipmentStatusData.ShipmentStatus);
			AssertEquals("", ShipmentStatusData.HoldReasonCode);
			AssertEquals(false, ShipmentStatusData.InspectIndicator);
			AssertEquals(false, ShipmentStatusData.AddressCorrectionIndicator);
			AssertEquals(ZDateTime.Now, ShipmentStatusData.ImportReleaseDate);
			AssertEquals("", ShipmentStatusData.Remarks);
			AssertEquals("", ShipmentStatusData.ExceptionStatusCode);
			AssertEquals("", ShipmentStatusData.ExceptionResolutionCode);
			AssertEquals(ShipmentStatusType.Special, ShipmentStatusData.ShipmentStatusType);
			AssertEquals(ZDateTime.Now, ShipmentStatusData.StatusChangeDate);
			AssertEquals(false, ShipmentStatusData.HasReasonOrResolutionCode);
		}

		public void TestLoadingProcessQueueFromParentPK()
		{
			UPECusHAWB.WayBillShort = "YADAYADA";
			Log.SL_Parent = UPECusHAWB.CurrentQueue.PK;
			AssertEquals("YADAYADA", ShipmentStatusData.ShipmentRef);
		}

		public void TestShipmentRefFromCusHAWB()
		{
			UPECusHAWB.WayBillShort = "ASDKLJ";
			Log.Master = UPECusHAWB.CurrentQueue;
			AssertEquals("ASDKLJ", ShipmentStatusData.ShipmentRef);
		}

		public void TestShipmentRefFromJobDeclaration()
		{
			UPEJobDeclaration uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.JE_AgentsReference = "ASDKLJ";
			UPECusHAWB.CS_JE_CustomsFormalEntry = uPEJobDeclaration.PK;
			Log.Master = uPEJobDeclaration.CurrentQueue;
			AssertEquals("ASDKLJ", ShipmentStatusData.ShipmentRef);
		}

		public void TestImportDate()
		{
			ZDateTime expectedDate = new ZDateTime(2005, 6, 1);
			UPECusMAWB.CM_ArrivalDate = expectedDate;
			Log.Master = UPECusHAWB.CurrentQueue;
			AssertEquals(expectedDate, ShipmentStatusData.ImportDate);
		}

		public void TestConsigneePostCode_FromCusHAWB()
		{
			UPECusHAWB.CS_ConsigneePostcode = "ASDKLJ";
			Log.Master = UPECusHAWB.CurrentQueue;
			AssertEquals("ASDKLJ", ShipmentStatusData.ConsigneePostCode);
		}

		public void TestConsigneePostCode_FromDeclaration()
		{
			UPECusHAWB.CS_ConsigneePostcode = "ASDF";
			UPEJobDeclaration uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			UPECusHAWB.CS_JE_CustomsFormalEntry = uPEJobDeclaration.PK;
			Log.Master = uPEJobDeclaration.CurrentQueue;
			AssertEquals("ASDF", ShipmentStatusData.ConsigneePostCode);
		}

		public void TestShipmentStatus()
		{
			AssertEquals("03", ShipmentStatusData.ShipmentStatus);
			Log.Master = UPECusHAWB.CurrentQueue;
			AssertEquals("03", ShipmentStatusData.ShipmentStatus);
			Log.SetQueueDetails("", ResolutionCodeDescriptionPairList.Codes.DA_Released, "", "", "");
			AssertEquals("04", ShipmentStatusData.ShipmentStatus);
		}

		public void TestHoldReasonCode()
		{
			AssertEquals("", ShipmentStatusData.HoldReasonCode);
			Log.Master = UPECusHAWB.CurrentQueue;
			Log.SetQueueDetails("", ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, "", "", "");
			AssertEquals(ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, ShipmentStatusData.HoldReasonCode);
			Log.SetQueueDetails("", ResolutionCodeDescriptionPairList.Codes.DA_Released, "", "", "");
			AssertEquals("", ShipmentStatusData.HoldReasonCode);
			Log.SetQueueDetails(CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.AQUA.Codes.DI_DocInspect, "", "", "");
			AssertEquals("", ShipmentStatusData.HoldReasonCode);
			Log.Master.P4_CustomsStatusInfo.SetValueFromString(ReasonCodeDescriptionPairList.AQUA.Codes.DI_DocInspect);
			Log.SetQueueDetails(CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.AQUA.Codes._05_PkgInspect, "", "", "");
			AssertEquals(ReasonCodeDescriptionPairList.AQUA.Codes._05_PkgInspect, ShipmentStatusData.HoldReasonCode);
			Log.SetQueueDetails(CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.AQUA.Codes.E8_QuarantineHold, "", "", "");
			AssertEquals(ReasonCodeDescriptionPairList.AQUA.Codes.E8_QuarantineHold, ShipmentStatusData.HoldReasonCode);
			Log.Master.P4_CustomsStatusInfo.SetValueFromString(ReasonCodeDescriptionPairList.AQUA.Codes.E8_QuarantineHold);
			Log.SetQueueDetails(CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, ReasonCodeDescriptionPairList.AQUA.Codes._05_PkgInspect, "", "", "");
			AssertEquals("", ShipmentStatusData.HoldReasonCode);
		}

		public void TestInspectIndicator()
		{
			Log.Master = UPECusHAWB.CurrentQueue;
			AssertEquals(false, ShipmentStatusData.InspectIndicator);
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Quarantine;
			UPECusHAWB.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold;
			AssertEquals(true, ShipmentStatusData.InspectIndicator);
			UPECusHAWB.CurrentQueue.P4_CustomsQueue = CargoReportQueueCodeDescriptionPairList.Codes.Pending;
			UPECusHAWB.CS_JE_CustomsFormalEntry = Factory.New(typeof(UPEJobDeclaration)).PK;
			AssertEquals(false, ShipmentStatusData.InspectIndicator);
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.BCA;
			UPECusHAWB.Declaration.CurrentQueue.P4_CustomsStatus = ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold;
			AssertEquals(true, ShipmentStatusData.InspectIndicator);
		}

		public void TestAddressCorrectionIndicator()
		{
			Callout callout = Factory.New<Callout>();
			Log.Master = callout.CurrentQueue;
			callout.CS_CustomsStatus = "";
			callout.IsHoldForCollection = false;
			callout.IsRedirected = false;
			callout.RebillFlag = RebillFlags.Unflagged;
			callout.Payment.PaymentMethod = UPECargoPaymentMethod.None;
			AssertEquals(false, ShipmentStatusData.AddressCorrectionIndicator);
			callout.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement;
			AssertEquals(true, ShipmentStatusData.AddressCorrectionIndicator);
			callout.CS_CustomsStatus = "";
			callout.IsHoldForCollection = true;
			AssertEquals(true, ShipmentStatusData.AddressCorrectionIndicator);
			callout.IsHoldForCollection = false;
			callout.RebillFlag = RebillFlags.IsRTS;
			AssertEquals(true, ShipmentStatusData.AddressCorrectionIndicator);
			callout.RebillFlag = RebillFlags.IsAbandoned;
			AssertEquals(true, ShipmentStatusData.AddressCorrectionIndicator);
			callout.RebillFlag = RebillFlags.Unflagged;
			callout.IsRedirected = true;
			AssertEquals(true, ShipmentStatusData.AddressCorrectionIndicator);
			callout.IsRedirected = false;
			callout.Payment.PaymentMethod = UPECargoPaymentMethod.Cheque;
			AssertEquals(true, ShipmentStatusData.AddressCorrectionIndicator);
			callout.Payment.PaymentMethod = UPECargoPaymentMethod.None;
			AssertEquals(false, ShipmentStatusData.AddressCorrectionIndicator);
			var zone = BrownPostcodeTransportProvider.Zones.AddNew();
			var item = zone.Items.AddNew();
			var postCode1 = Factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "1";
			item.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2";
			item.TQ_ToPostCode = postCode2.RK_CityTownPostCode;
			callout.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			callout.Consignee.MainAddress.OA_PostCode = "2";
			AssertEquals(true, ShipmentStatusData.AddressCorrectionIndicator);
		}

		[TestDate(2004, 1, 2)]
		public void TestImportReleaseDate()
		{
			Log.SL_EventTime = ZDateTime.Now;
			AssertEquals(ZDateTime.Now, ShipmentStatusData.ImportReleaseDate);
			ZDateTime expectedDate = new ZDateTime(2005, 11, 4);
			Log.SL_EventTime = expectedDate;
			AssertEquals(expectedDate, ShipmentStatusData.ImportReleaseDate);
		}

		public void TestRemarks()
		{
			Log.SetQueueDetails("", "", "", "This is my remarks", "");
			AssertEquals("This is my remarks", ShipmentStatusData.Remarks);
		}

		public void TestExceptionStatusCode()
		{
			AssertEquals("", ShipmentStatusData.ExceptionStatusCode);
			Log.Master = UPECusHAWB.CurrentQueue;
			Log.SetQueueDetails("", "", StatusCodeDescriptionPairList.Codes.BQ_NoAnswer, "", "");
			AssertEquals(StatusCodeDescriptionPairList.Codes.BQ_NoAnswer, ShipmentStatusData.ExceptionStatusCode);
			Log.SetQueueDetails("", ResolutionCodeDescriptionPairList.Codes.DA_Released, "", "", "");
			AssertEquals("", ShipmentStatusData.ExceptionStatusCode);
		}

		public void TestExceptionStatusCode_ExcludeEmptyStatuses()
		{
			AssertEquals("", ShipmentStatusData.ExceptionStatusCode);
			Log.Master = UPECusHAWB.CurrentQueue;
			Log.SetQueueDetails("", "", StatusCodeDescriptionPairList.Codes.BQ_NoAnswer, "", "");
			AssertEquals(StatusCodeDescriptionPairList.Codes.BQ_NoAnswer, ShipmentStatusData.ExceptionStatusCode);
			Log.SetQueueDetails("", "", StatusCodeDescriptionPairList.EmptyStatus, "", "");
			AssertEquals("", ShipmentStatusData.ExceptionStatusCode);
		}

		public void TestExceptionResolutionCode()
		{
			AssertEquals("", ShipmentStatusData.ExceptionResolutionCode);
			Log.Master = UPECusHAWB.CurrentQueue;
			Log.SetQueueDetails("", ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, StatusCodeDescriptionPairList.Codes.BQ_NoAnswer, "", "");
			AssertEquals("", ShipmentStatusData.ExceptionResolutionCode);
			Log.SetQueueDetails("", ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms, "", "", "");
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms, ShipmentStatusData.ExceptionResolutionCode);
		}

		public void TestStatusChangeDate()
		{
			ZDateTime expectedDate = new ZDateTime(2002, 2, 2);
			Log.SL_EventTime = expectedDate;
			AssertEquals(expectedDate, ShipmentStatusData.StatusChangeDate);
		}

		public void TestShipmentStatusType()
		{
			Log.SetQueueDetails("", "S1", "S2", "R1", "888");
			AssertEquals(ShipmentStatusType.Special, ShipmentStatusData.ShipmentStatusType);
			Log.SetQueueDetails("Q1", "S1", "S2", "R1", "888");
			Log.SL_Reference = ProcessQueueType.Commercial;
			AssertEquals(ShipmentStatusType.Commercial, ShipmentStatusData.ShipmentStatusType);
			Log.SL_Reference = ProcessQueueType.Customs;
			AssertEquals("No Process Queue", ShipmentStatusType.Unknown, ShipmentStatusData.ShipmentStatusType);
			Log.Master = UPECusHAWB.CurrentQueue;
			AssertEquals(ShipmentStatusType.CargoReport, ShipmentStatusData.ShipmentStatusType);
			fLog = null;
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			Log.SL_Reference = ProcessQueueType.Customs;
			Log.SetQueueDetails("Q1", "S1", "S2", "R1", "888");
			Log.Master = declaration.CurrentQueue;
			AssertEquals(ShipmentStatusType.Declaration, ShipmentStatusData.ShipmentStatusType);
		}

		public void TestHasReasonOrResolutionCode()
		{
			Log.SetQueueDetails("", "", "", "", "");
			AssertEquals(false, ShipmentStatusData.HasReasonOrResolutionCode);
			Log.SetQueueDetails("", "X2", "", "", "");
			AssertEquals(true, ShipmentStatusData.HasReasonOrResolutionCode);
			Log.SetQueueDetails("", "_T2", "", "", "");
			AssertEquals(false, ShipmentStatusData.HasReasonOrResolutionCode);
		}

		#region Implementation
		IShipmentStatusData ShipmentStatusData
		{
			get
			{
				return Log;
			}
		}

		UPEProcessQueueLog Log
		{
			get
			{
				if (fLog == null)
				{
					fLog = Factory.New<UPEProcessQueueLog>();
				}

				return fLog;
			}
		}

		UPECusMAWB UPECusMAWB
		{
			get
			{
				if (fUPECusMAWB == null)
				{
					fUPECusMAWB = Factory.New<UPECusMAWB>();
				}

				return fUPECusMAWB;
			}
		}

		UPECusHAWB UPECusHAWB
		{
			get
			{
				if (fUPECusHAWB == null)
				{
					fUPECusHAWB = (UPECusHAWB)UPECusMAWB.ChildBills.AddNew();
				}

				return fUPECusHAWB;
			}
		}

		UPERateTransportProvider BrownPostcodeTransportProvider
		{
			get
			{
				UPERateTransportProvider result = UPERateTransportProvider.LoadBrownPostcodeTransportProvider(Factory);
				if (result == null)
				{
					result = Factory.New<UPERateTransportProvider>();
					result.TP_OH_RelatedParty = BrownPostcodeTransportOrg.PK;
				}

				return result;
			}
		}

		OrgHeader BrownPostcodeTransportOrg
		{
			get
			{
				OrgHeader result = OrgHeader.LoadFromCode(Factory, UPERateTransportProvider.BrownPostcodeTransportOrgCode);
				if (result == null)
				{
					result = Factory.NewWithValidTestData<OrgHeader>();
					result.OH_Code = UPERateTransportProvider.BrownPostcodeTransportOrgCode;
				}

				return result;
			}
		}

		UPECusMAWB fUPECusMAWB;
		UPECusHAWB fUPECusHAWB;
		UPEProcessQueueLog fLog;
		#endregion
	}
}
