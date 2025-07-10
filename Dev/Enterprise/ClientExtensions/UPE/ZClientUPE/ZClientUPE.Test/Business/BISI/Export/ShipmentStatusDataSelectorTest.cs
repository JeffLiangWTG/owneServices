using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.MasterData.Business.Tests;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	sealed class ShipmentStatusDataSelectorTest : TestCaseWithFactory
	{
		[TestDate(2006, 1, 1)]
		public void TestForceAllXPLDsToBeUploadedEveryTime()
		{
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = true;
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			UPECusHAWB uPECusHAWB2 = CreateSavedUPECusHAWB();
			uPECusHAWB2.WayBillShort = "1";
			UPECusHAWB uPECusHAWB3 = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB3.WayBillShort = "2";
			UPECusHAWB uPECusHAWB4 = CreateSavedUPECusHAWB();
			uPECusHAWB4.WayBillShort = "3";
			uPECusHAWB4.CS_ConsigneePostcode = "2010";
			UPECusHAWB uPECusHAWB5 = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB5.WayBillShort = "4";
			uPECusHAWB5.CS_ConsigneePostcode = "2010";
			TestDateAttribute.Date = new DateTime(2006, 1, 1);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ResolutionCodeDescriptionPairList.Codes.DA_Released, "", "", "");
			uPECusHAWB2.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, "", "", "");
			uPECusHAWB3.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			uPECusHAWB4.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, "", "", "");
			uPECusHAWB5.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(5, DataSelector.StatusesForExport.Count);
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(5, DataSelector.StatusesForExport.Count);
		}

		#region Everyday Status Upload
		#region Test AddEveryDayStatuses
		[TestDate(2006, 1, 1)]
		public void TestEveryDayXPLD_UploadedWhenLogIsCreated_ResolutionCode()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ResolutionCodeDescriptionPairList.Codes.DA_Released, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.DA_Released, DataSelector.StatusesForExport[0].ExceptionResolutionCode);
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestEveryDayXPLD_UploadedWhenLogIsCreated_MultipleUploadsWhenThereAreMultipleShipments()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, "", "", "");
			Factory.Save();
			uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.WayBillShort = "10001";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(string.Empty, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(2, DataSelector.StatusesForExport.Count);
			var statusesCopy = SortShipmentStatusData(DataSelector.StatusesForExport);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.DA_Released, statusesCopy[0].ExceptionResolutionCode);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, statusesCopy[1].HoldReasonCode);
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(2, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		[ExpectNoExceptions()]
		public void TestDodgeyRegistryItem()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(string.Empty, ResolutionCodeDescriptionPairList.Codes.DA_Released, "", "", "");
			Factory.Save();
			uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.WayBillShort = "10001";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(string.Empty, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			var statusesCopy = SortShipmentStatusData(DataSelector.StatusesForExport);
			AssertEquals(ResolutionCodeDescriptionPairList.Codes.DA_Released, statusesCopy[0].ExceptionResolutionCode);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, statusesCopy[1].HoldReasonCode);
			UPEDataRegistry.Instance.XPLDForWorkingDaysDateOfArrivalPassed = new ReadOnlyCodeDescriptionPairList();
			Assert("Registry empty", UPEDataRegistry.Instance.XPLDForWorkingDaysDateOfArrivalPassed.Count == 0);
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(2, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestEveryDayXPLD_UploadedWhenLogIsCreated_ReasonCode()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, DataSelector.StatusesForExport[0].HoldReasonCode);
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestEveryDayXPLD_NotUploadedWhenLogIsCreated_IfDateOfArrivalIsLater()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(0, DataSelector.StatusesForExport.Count);
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestEveryDayXPLD_UploadedWhenLogIsCreated_ProvidingDateIsGreaterOrEqualToDateOfArrivalOfShipment()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, DataSelector.StatusesForExport[0].HoldReasonCode);
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
		}

		#endregion
		#region Test AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival
		[TestDate(2006, 1, 1)]
		public void TestEveryDayXPLD_ThatHasBeenCreatedBeforeDateOfArrivalOfShipment_IsUploadedOnDateOfArrivalOfTheShipment()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, "", "", "");
			Factory.Save();
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2));
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, DataSelector.StatusesForExport[0].HoldReasonCode);
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestEveryDayDOA_XPLD_ThatHasBeenCreatedOnDateOfArrivalOfShipment_IsNotUploaded()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, "", "", "");
			Factory.Save();
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddDays(1));
			AssertEquals(0, DataSelector.StatusesForExport.Count);
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		#endregion
		#endregion
		#region Working Day Status Upload Both Zones
		#region Test AddWorkingDayStatuses
		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_UploadedWhenLogIsCreated_MultipleUploadsWhenThereAreMultipleShipments()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, "", "", "");
			Factory.Save();
			uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.WayBillShort = "10001";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.FF_RTSAuthorisationRequired, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(2, DataSelector.StatusesForExport.Count);
			var statusesCopy = SortShipmentStatusData(DataSelector.StatusesForExport);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, statusesCopy[0].HoldReasonCode);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.FF_RTSAuthorisationRequired, statusesCopy[1].HoldReasonCode);
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(2, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_UploadedWhenLogIsCreated()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, DataSelector.StatusesForExport[0].HoldReasonCode);
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_NotUploadedWhenLogIsCreated_IfDateOfArrivalIsLater()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_UploadedWhenLogIsCreated_ProvidingDateIsGreaterOrEqualToDateOfArrivalOfShipment()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold, DataSelector.StatusesForExport[0].HoldReasonCode);
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
		}

		#endregion
		#region Test AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival
		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_ThatHasBeenCreatedBeforeDateOfArrivalOfShipment_IsUploadedOnDateOfArrivalOfTheShipment()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, DataSelector.StatusesForExport[0].HoldReasonCode);
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_ThatHasBeenCreatedBeforeDateOfArrivalOfShipment_IsUploadedOnDateOfArrivalOfTheShipment_NonUPEBranches()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			uPECusHAWB.MAWB.CM_GB = branch.PK;
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_FlightNumbersForUploadOnDayOfArrivalPlusOne_StartDateNotEqualsToEndDate_EventDateLessThanArrivalDate_IsUploadedAfterDateOfArrivalOfTheShipment()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1), true);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(2), TestDateAttribute.Date.AddDays(3), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals("Status should be uploaded", 1, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_FlightNumbersForUploadOnDayOfArrivalPlusOne_StartDateNotEqualsToEndDate_EventDateLessThanArrivalDate_IsUploadedAfterDateOfArrivalOfTheShipment_NonUPEBranches()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1), true);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			uPECusHAWB.MAWB.CM_GB = branch.PK;
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(2), TestDateAttribute.Date.AddDays(3), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals("Status should not be uploaded", 0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_FlightNumbersForUploadOnDayOfArrivalPlusOne_StartDateEqualsToEndDate_EventDateLessThanArrivalDate_IsUploadedAfterDateOfArrivalOfTheShipment()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1), true);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(2), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals("Status should be uploaded", 1, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_FlightNumbersForUploadOnDayOfArrivalPlusOne_StartDateNotEqualsToEndDate_EventDateEqualsToArrivalDate_IsUploadedAfterDateOfArrivalOfTheShipment()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date, true);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals("Status should be uploaded", 1, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_FlightNumbersForUploadOnDayOfArrivalPlusOne_StartDateEqualsToEndDate_EventDateEqualsToArrivalDate_IsUploadedAfterDateOfArrivalOfTheShipment()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date, true);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals("Status should be uploaded", 1, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_StartDateNotEqualsToEndDate_EventDateEqualsToArrivalDate_IsNotUploadedAfterDateOfArrivalOfTheShipment()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals("Status should not be uploaded", 0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_StartDateEqualsToEndDate_EventDateEqualsToArrivalDate_IsNotUploadedAfterDateOfArrivalOfTheShipment()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals("Status should not be uploaded", 0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayDOA_XPLD_ThatHasBeenCreatedOnDateOfArrivalOfShipment_IsNotUploaded()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddDays(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			DataSelector.AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_NotUploadTheLatestHoldStatus()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date, true);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			uPECusHAWB.CurrentQueue.CommercialQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(2);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.Completed, "", "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_UploadMultipleLatestHoldStatus()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date, true);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(-1);
			uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1), true);
			uPECusHAWB.WayBillShort = "10001";
			uPECusHAWB.CS_HAWB = "TestHousebill1";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.WayBillShort = "10002";
			uPECusHAWB.CS_HAWB = "TestHousebill2";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(3, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_NotUploadHoldStatusCreatedMoreThanHalfYearAgo()
		{
			DateTime arrivalDate = TestDateAttribute.Date.AddMonths(7);
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(arrivalDate, true);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(arrivalDate.AddDays(1), arrivalDate.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		public void TestGetFlightNumbersSql()
		{
			ShipmentStatusDataSelector selector = new ShipmentStatusDataSelector();
			UPEDataRegistry.Instance.FlightNumbersForUploadOnDayOfArrivalPlusOne = new string[] { "001", "002" };
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			AssertEquals("Flight numbers sql", selector.GetFlightNumbersSql(parameters), "(@FN_0,@FN_1)");
			AssertEquals("Number of parameters should be 2", parameters.Count, 2);
			AssertEquals("First parameter name", parameters[0].ParameterName, "@FN_0");
			AssertEquals("First parameter value", parameters[0].ParameterValueTextSql, "'001'");
			AssertEquals("Second parameter name", parameters[1].ParameterName, "@FN_1");
			AssertEquals("Second parameter value", parameters[1].ParameterValueTextSql, "'002'");
			UPEDataRegistry.Instance.FlightNumbersForUploadOnDayOfArrivalPlusOne = Array.Empty<string>();
			parameters = new ZSqlParameterCollection();
			AssertEquals("No flight numbers sql", selector.GetFlightNumbersSql(parameters), "");
			AssertEquals("Number of parameters should be 0", parameters.Count, 0);
		}

		public void TestGetAndFlightNumbersSql()
		{
			ShipmentStatusDataSelector selector = new ShipmentStatusDataSelector();
			AssertEquals("IN Flight numbers sql", selector.GetAndFlightNumbersSql(true, "JE_VoyageFlightNo", "(@FN_0,@FN_1)"), "AND JE_VoyageFlightNo IN (@FN_0,@FN_1)");
			AssertEquals("NOT IN Flight numbers sql", selector.GetAndFlightNumbersSql(false, "CM_FlightNo", "(@FN_0,@FN_1)"), "AND CM_FlightNo NOT IN (@FN_0,@FN_1)");
			AssertEquals("Empty Flight numbers sql", selector.GetAndFlightNumbersSql(true, "JE_VoyageFlightNo", ""), "");
		}

		#endregion
		#endregion
		#region Metro and Other Zones
		#region Working Day Metro Zone
		#region Working Day
		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_UploadedWhenLogIsCreated_MetroZone()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_UploadedWhenLogIsCreated_MetroZone_NotUploadedWhenPostCodeNotInMetroZone()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CS_ConsigneePostcode = "3010";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_MetroZone_UploadSpecialQueue()
		{
			var originalTestDate = ZDateTime.Now;
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			DeclarationFromAirCargoCreator jobDecCreator = new DeclarationFromAirCargoCreator(uPECusHAWB);
			var declaration = Factory.New<UPEJobDeclaration>();
			jobDecCreator.CreateIgnoreWarnings(declaration);
			declaration.JE_AgentsReference = uPECusHAWB.WayBillShort;
			CodeDescriptionPairList xPLDList = new CodeDescriptionPairList();
			xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection);
			xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration);
			xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments);
			xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment);
			UPEDataRegistry.Instance.XPLDForWorkingDaysDateOfArrivalPassed = xPLDList;
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "Y1", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew("PND", "", "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(60);
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew("LDG", "X2", "", "AUDIT 9 LINES / AUDIT ABN", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew("ADC", "", "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew("SUB", "X2", "__", "AUDIT 9 LINES / AUDIT ABN", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew("CPL", "", "", "AUDIT 9 LINES / AUDIT ABN", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew("CPL", "", "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
			uPECusHAWB.CurrentQueue.CommercialQueueLogs.AddNew("", "OQ", "", "auto upload Local Charges below COD Threshold", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
			uPECusHAWB.CurrentQueue.CommercialQueueLogs.AddNew("CAL", "OQ", "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			DataSelector.AddWorkingDayStatuses(originalTestDate, TestDateAttribute.Date, ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		#endregion
		#region Date Of Arrival
		void FillWithValidDataToCreateDeclaration(UPECusHAWB airCargo)
		{
			TaxOrFeeTestHelper.SetUp();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);
			var orgForMatching = Factory.NewWithValidTestData<OrgHeader>();
			orgForMatching.OH_FullName = "AUSTRALIAN FILM & PIPE MANUFACTURERS";
			orgForMatching.OH_RL_NKClosestPort = "AUSYD";
			orgForMatching.MainAddress.OA_City = "SYDNEY";
			orgForMatching.MainAddress.OA_Phone = "+61297255045";
			orgForMatching.MainAddress.OA_PostCode = "2010";
			orgForMatching.MainAddress.OA_State = "NSW";
			orgForMatching.MainAddress.OA_Address1 = "150 WOODPARK RD";
			orgForMatching.MainAddress.OA_Address2 = "SMITHFIELD";
			orgForMatching.CreatePatternMatchingAddressFromMainAddress(Factory);
			orgForMatching.CreatePatternMatchingName(Factory);
			Factory.Save();
			airCargo.CS_GoodsValue = 500m;
			airCargo.CS_GoodsDescription = "test air cargo goods";
			airCargo.CS_ConsigneeCity = orgForMatching.MainAddress.OA_City;
			airCargo.CS_ConsigneeName = orgForMatching.OH_FullNameTruncated;
			airCargo.CS_ConsigneePhone = orgForMatching.MainAddress.OA_Phone;
			airCargo.CS_ConsigneePostcode = orgForMatching.MainAddress.OA_PostCode;
			airCargo.CS_ConsigneeState = orgForMatching.MainAddress.OA_State;
			airCargo.CS_ConsigneeStreet = orgForMatching.MainAddress.OA_Address1;
			airCargo.CS_ConsigneeStreet2 = orgForMatching.MainAddress.OA_Address2;
			airCargo.CS_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Australia;
			airCargo.CS_ConsignorCity = orgForMatching.MainAddress.OA_City;
			airCargo.CS_ConsignorName = orgForMatching.OH_FullNameTruncated;
			airCargo.CS_ConsignorPhone = orgForMatching.MainAddress.OA_Phone;
			airCargo.CS_ConsignorPostcode = orgForMatching.MainAddress.OA_PostCode;
			airCargo.CS_ConsignorState = orgForMatching.MainAddress.OA_State;
			airCargo.CS_ConsignorStreet = orgForMatching.MainAddress.OA_Address1;
			airCargo.CS_ConsignorStreet2 = orgForMatching.MainAddress.OA_Address2;
		}

		[TestDate(2006, 1, 1)]
		public void TestXPLD_DayOfArrivalCargoReportUploadWhichHasCustomsDeclaration()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			FillWithValidDataToCreateDeclaration(uPECusHAWB);
			Factory.Save();
			uPECusHAWB.CreateFormalDecAndMatch();
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(-2);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddDays(1), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestXPLD_DayOfArrivalCargoReportUploadWhichHasCustomsDeclaration_NonUPEBranches()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			FillWithValidDataToCreateDeclaration(uPECusHAWB);
			Factory.Save();
			uPECusHAWB.CreateFormalDecAndMatch();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "ABC";
			Factory.Save();
			uPECusHAWB.MAWB.CM_GB = branch.PK;
			uPECusHAWB.Declaration.JE_GB = branch.PK;
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(-2);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddDays(1), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestXPLD_DayOfArrivalCargoReportUploadWhichHasCustomsDeclaration_NonUPEBranches_WithFlightNumber()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1), true);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			FillWithValidDataToCreateDeclaration(uPECusHAWB);
			Factory.Save();
			uPECusHAWB.CreateFormalDecAndMatch();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "ABC";
			Factory.Save();
			uPECusHAWB.MAWB.CM_GB = branch.PK;
			uPECusHAWB.Declaration.JE_GB = branch.PK;
			uPECusHAWB.Declaration.JE_DateOfArrival = TestDateAttribute.Date;
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(-2);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddDays(1), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestXPLD_DayOfArrivalCargoReportUploadWhichHasFinance()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration;
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(-2);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(0), TestDateAttribute.Date.AddDays(1), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestXPLD_DayOfArrivalCargoReportUploadWhichHasDeclarationAndFinance()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid, "", "", "");
			FillWithValidDataToCreateDeclaration(uPECusHAWB);
			Factory.Save();
			uPECusHAWB.CreateFormalDecAndMatch();
			uPECusHAWB.Declaration.JE_HouseBill = uPECusHAWB.CS_HAWB;
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			uPECusHAWB.Declaration.JE_DateOfArrival = TestDateAttribute.Date.AddHours(-1).AddDays(1);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.CurrentQueue.P4_Status = ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration;
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(-3);
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date, TestDateAttribute.Date.AddDays(1), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_ThatHasBeenCreatedBeforeDateOfArrivalOfShipment_IsUploadedOnDateOfArrivalOfTheShipment_MetroZone()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_ThatHasBeenCreatedBeforeDateOfArrivalOfShipment_IsUploadedOnDateOfArrivalOfTheShipment_MetroZone_NotUploadedIfOutsideZone()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CS_ConsigneePostcode = "3010";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		#endregion
		#endregion
		#region Working Day Other Zone
		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_UploadedWhenLogIsCreated_OtherZone()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CS_ConsigneePostcode = "3010";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_UploadedWhenLogIsCreated_OtherZone_NotUploadedWhenPostCodeNotInOtherZone()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		#endregion
		#region Date Of Arrival Other Zone
		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_ThatHasBeenCreatedBeforeDateOfArrivalOfShipment_IsUploadedOnDateOfArrivalOfTheShipment_OtherZone()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CS_ConsigneePostcode = "3010";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayXPLD_ThatHasBeenCreatedBeforeDateOfArrivalOfShipment_IsUploadedOnDateOfArrivalOfTheShipment_OtherZone_NotUploadedIfOutsideZone()
		{
			CreateUPSZones();
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(TestDateAttribute.Date.AddDays(1));
			uPECusHAWB.CS_ConsigneePostcode = "2010";
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, "", "", "");
			Factory.Save();
			DataSelector.AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(TestDateAttribute.Date.AddDays(1), TestDateAttribute.Date.AddDays(2), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		#endregion
		[TestDate(2006, 1, 1)]
		public void TestZoneCalculationWorksOnADeclaration_IncludedRange()
		{
			CreateUPSZones();
			UPEDeclarationFromAirCargoCreator uPEDeclarationFromAirCargoCreator = new UPEDeclarationFromAirCargoCreator(CreateSavedUPECusHAWB());
			UPEJobDeclaration declaration = (UPEJobDeclaration)uPEDeclarationFromAirCargoCreator.CreateIgnoreWarnings();
			declaration.FirstCusHAWB.CS_ConsigneePostcode = "2012";
			declaration.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestZoneCalculationWorksOnADeclaration_ExcludedRange()
		{
			CreateUPSZones();
			UPEDeclarationFromAirCargoCreator uPEDeclarationFromAirCargoCreator = new UPEDeclarationFromAirCargoCreator(CreateSavedUPECusHAWB());
			UPEJobDeclaration declaration = (UPEJobDeclaration)uPEDeclarationFromAirCargoCreator.CreateIgnoreWarnings();
			declaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			declaration.Importer.MainAddress.OA_PostCode = "3012";
			declaration.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Metro);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		#endregion
		#region ShipmentStatusDataWithBranches
		[TestDate(2006, 1, 1)]
		public void TestFilteredShipmentStatusDataWithBranches()
		{
			int count1 = 0;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			var branch = company.Branches.AddNew();
			Factory.Save();
			using (branch.SetAsTemporaryContext())
			{
				UPEDataRegistry.Instance.EnableUPECustomisations = true;
				UPECusHAWB uPECusHAWB_BNE = CreateSavedUPECusHAWB();
				var uPEDeclarationFromAirCargoCreator_BNE = new UPEDeclarationFromAirCargoCreator(uPECusHAWB_BNE);
				uPECusHAWB_BNE.CS_JE_CustomsFormalEntry = uPEDeclarationFromAirCargoCreator_BNE.CreateIgnoreWarnings().PK;
				uPECusHAWB_BNE.Declaration.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.AQ_RefusedNoCOD, "", "", "");
				Factory.Save();
				uPECusHAWB_BNE.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "", "", "");
				uPECusHAWB_BNE.CurrentQueue.CommercialQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "", "", "");
				Factory.Save();
				count1 = DataSelector.FilteredShipmentStatusData(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1)).Length;
			}

			int count2 = DataSelector.FilteredShipmentStatusData(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1)).Length;
			UPECusHAWB uPECusHAWB_AMS = CreateSavedUPECusHAWB();
			var uPEDeclarationFromAirCargoCreator_AMS = new UPEDeclarationFromAirCargoCreator(uPECusHAWB_AMS);
			uPECusHAWB_AMS.CS_JE_CustomsFormalEntry = uPEDeclarationFromAirCargoCreator_AMS.CreateIgnoreWarnings().PK;
			uPECusHAWB_AMS.Declaration.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "", "");
			Factory.Save();
			uPECusHAWB_AMS.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "", "", "");
			uPECusHAWB_AMS.CurrentQueue.CommercialQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "", "", "");
			Factory.Save();
			int count3 = DataSelector.FilteredShipmentStatusData(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1)).Length;
			AssertEquals(4, count1);
			AssertEquals(0, count2);
			AssertEquals(4, count3);
		}

		#endregion
		#region TestPriority
		[TestDate(2006, 1, 1)]
		public void TestGetStatusPriority_SpecialTakesPrecedence()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "", "", "");
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "", "");
			UPEDeclarationFromAirCargoCreator uPEDeclarationFromAirCargoCreator = new UPEDeclarationFromAirCargoCreator(uPECusHAWB);
			uPECusHAWB.CS_JE_CustomsFormalEntry = uPEDeclarationFromAirCargoCreator.CreateIgnoreWarnings().PK;
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestGetStatusPriority_DeclarationTakesPrecedence()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			UPEDeclarationFromAirCargoCreator uPEDeclarationFromAirCargoCreator = new UPEDeclarationFromAirCargoCreator(uPECusHAWB);
			uPECusHAWB.CS_JE_CustomsFormalEntry = uPEDeclarationFromAirCargoCreator.CreateIgnoreWarnings().PK;
			uPECusHAWB.Declaration.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "", "");
			Factory.Save();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "", "", "");
			uPECusHAWB.CurrentQueue.CommercialQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestGetStatusPriority_CommercialTakesPrecedence()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CurrentQueue.CommercialQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "", "", "");
			Factory.Save();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestGetStatusPriority_DateIsMoreImportant()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, "", "", "");
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date.AddMinutes(-1), TestDateAttribute.Date.AddHours(1));
			AssertEquals(1, DataSelector.StatusesForExport.Count);
			AssertEquals(ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, DataSelector.StatusesForExport[0].HoldReasonCode);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayUploadShouldNotUploadStatusesIfSuperceeded()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB();
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ResolutionCodeDescriptionPairList.Codes.DA_Released, "", "", "");
			Factory.Save();
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date.AddHours(-1), TestDateAttribute.Date.AddHours(1), ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		[TestDate(2006, 1, 1)]
		public void TestWorkingDayUploadShouldNotUploadStatusesStatusDateIsBeforeDateOfArrival()
		{
			UPECusHAWB uPECusHAWB = CreateSavedUPECusHAWB(new DateTime(2006, 1, 2));
			uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew(DefaultQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, "", "", "");
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(2);
			DataSelector.AddWorkingDayStatuses(TestDateAttribute.Date.AddDays(-2), TestDateAttribute.Date, ShipmentStatusDataSelector.DeliveryArea.Other);
			AssertEquals(0, DataSelector.StatusesForExport.Count);
		}

		#endregion
		#region TestPerformance
		[TestDate(2006, 1, 1)]
		public void TestStatusesNumberOfDBHitsPerformance()
		{
			CreateMasterWithManyHAWBS_AndStatuses();
			Factory.Save();
			DataSelector.AddEveryDayStatuses(TestDateAttribute.Date, TestDateAttribute.Date.AddHours(1));
			int actualDBHits = DataSelector.FactoryForTesting.DatabaseLoadCount;
			bool success = actualDBHits <= 801;
			Assert("There are too many database hits, please ensure that you test the performance", success);
		}

		const int NumberOfHAWBs = 200;
		UPECusMAWB CreateMasterWithManyHAWBS_AndStatuses()
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_MAWB = "08166666666";
			uPECusMAWB.CM_RL_NKLoadPort = "SGSIN";
			uPECusMAWB.CM_RL_NKDischargePort = "AUSYD";
			uPECusMAWB.CM_ArrivalDate = ZDateTime.Now;
			for (int i = 0; i < NumberOfHAWBs; i++)
			{
				UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
				uPECusHAWB.CS_CM = uPECusMAWB.PK;
				uPECusHAWB.CS_ConsigneeName = "TEST";
				uPECusHAWB.CS_ConsignorName = "TEST";
				uPECusHAWB.CS_Weight = 10m;
				uPECusHAWB.CS_WeightUQ = Core.Constants.Weight.Kilograms;
				uPECusHAWB.CS_GoodsDescription = "TEST";
				uPECusHAWB.CS_GoodsValue = 1001m;
				uPECusHAWB.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Australia;
				uPECusHAWB.CS_HAWB = "TESTHAWB" + i.ToString();
				uPECusHAWB.CS_PiecesManifested = (short)i;
				uPECusHAWB.CS_RS_NK_ServiceLevel = "1";
				uPECusHAWB.CS_RL_NKOrigin = "SGSIN";
				uPECusHAWB.CS_RL_NKDestination = "AUSYD";
				uPECusHAWB.WayBillShort = "SHORT" + i.ToString();
				uPECusHAWB.CurrentQueue.CustomsQueueLogs.AddNew("", ResolutionCodeDescriptionPairList.Codes.DA_Released, "", "", "");
			}

			return uPECusMAWB;
		}

		#endregion
		#region Setup
		UPECusHAWB CreateSavedUPECusHAWB()
		{
			return CreateSavedUPECusHAWB(TestDateAttribute.Date);
		}

		UPECusHAWB CreateSavedUPECusHAWB(DateTime dateOfArrival, bool flightNumbersForUploadOnDayOfArrivalPlusOne = false)
		{
			DateTime testDate = TestDateAttribute.Date;
			TestDateAttribute.Date = testDate.AddHours(-1); // ensure that any logs created by factory.save are before the testdate
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_ArrivalDate = dateOfArrival;
			if (flightNumbersForUploadOnDayOfArrivalPlusOne)
			{
				uPECusMAWB.CM_FlightNo = "QA100";
				UPEDataRegistry.Instance.FlightNumbersForUploadOnDayOfArrivalPlusOne = new string[] { uPECusMAWB.CM_FlightNo };
			}

			UPECusHAWB uPECusHAWB = (UPECusHAWB)uPECusMAWB.ChildBills.AddNew();
			uPECusHAWB.CS_HAWB = "TestHousebill";
			uPECusHAWB.WayBillShort = "10000";
			Factory.Save();
			TestDateAttribute.Date = testDate;
			return uPECusHAWB;
		}

		void CreateUPSZones()
		{
			ZonesTestHelper.CreateUPSZones(Factory);
		}

		ZonesTestHelper ZonesTestHelper
		{
			get
			{
				if (fZonesTestHelper == null)
				{
					fZonesTestHelper = new ZonesTestHelper();
				}

				return fZonesTestHelper;
			}
		}

		ZonesTestHelper fZonesTestHelper;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime = false;
			DataSelector = new ShipmentStatusDataSelector();
		}

		ShipmentStatusDataSelector DataSelector;
		#endregion

		IReadOnlyList<IShipmentStatusData> SortShipmentStatusData(IReadOnlyList<IShipmentStatusData> statuses)
		{
			var sortedList = statuses.ToList();
			sortedList.Sort(new ShipmentStatusDataComparer());
			return sortedList.AsReadOnly();
		}

		sealed class ShipmentStatusDataComparer : IComparer<IShipmentStatusData>
		{
			public int Compare(IShipmentStatusData x, IShipmentStatusData y)
			{
				return x.ShipmentRef.CompareTo(y.ShipmentRef);
			}
		}
	}
}
