using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.GUI;
using Enterprise.Customs.ES.GUI.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.ES.NCTS.Business.Testing;
using Enterprise.Customs.ES.TemporaryStorage.GUI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.ESConstants;
using BuilderHelperTest = Enterprise.Customs.ES.Business.Testing.BuilderHelperTest;
using CusGuaranteeHeader = Enterprise.Customs.ES.Business.CusGuaranteeHeader;
using CusTempStorageRegLineTransactionInternalReferenceTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionInternalReferenceTypeList;
using CusTempStorageRegLineTransactionStatusList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionStatusList;
using CusTempStorageRegLineTransactionTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;
using CusTempStorageRegPremisesTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList;
using NctsDepartureCargoDesc = Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc;
using NctsDepartureHeaderContainer = Enterprise.Customs.ES.NCTS.Business.NctsDepartureHeaderContainer;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;
using NctsHeaderMessageSendingObject = Enterprise.Customs.ES.NCTS.Business.NctsHeaderMessageSendingObject;
using NctsHeaderMessageSendingObjectParent = Enterprise.Customs.ES.NCTS.Business.NctsHeaderMessageSendingObjectParent;
using UniversalReferenceTestDataHelper = Enterprise.Customs.Universal.Testing.UniversalReferenceTestDataHelper;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing;

sealed class Phase5MessagingMenuProviderTest : TestCaseWithFactory
{
	public void TestCreateMenuItems()
	{
		AssertContainsExactElementsInExactOrder(
			[
				"Send to Customs",
				"Synchronize with Customs",
				"Download TAD (Transit Accompanying Document)",
				"Make Arrival Notification for this Departure",
				"Make TNN for this Arrival",
				"Load Data for Unloading",
				"-",
				"Check for Inbox Notifications",
				"Capture from Customs",
				"View on Customs Website",
				"Create EXS declaration",
				"Set Entry as Failed From Transmission",
				"-",
				"Into Temporary Storage",
				"View TS Register",
				"-",
				"Inventory Management",
				"TS Register Management",
				"-",
				"Import Entry Lines",
				"Import Invoice Lines",
				"&Copy Previous Goods Item",
				"Lock Customs Declaration",
				"Unlock Customs Declaration",
			], menuItems.Select(x => x.Text));
	}

	#region Refresh Menu

	public void TestRefreshMenu_DepartureWithMRNDepartureStatusEmpty()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Download TAD (Transit Accompanying Document)",
			"-",
			"Check for Inbox Notifications",
			"Capture from Customs",
			"View on Customs Website",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNAndDepartureStatusEmpty()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		AssertVisibleMenuItems(
			"Send to Customs",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusSNTAndDepartureStatusEmpty()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertVisibleMenuItems(
			"Set Entry as Failed From Transmission",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusREJAndDepartureStatusEmpty()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		AssertVisibleMenuItems(
			"Send to Customs",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusSNTAndDepartureStatusINV()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Invalidated;
		AssertVisibleMenuItems(
			"Set Entry as Failed From Transmission",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusSNTAndDepartureStatusPREReleaseStatusEmpty()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		nctsHeader.BH_ReleaseStatus = ZString.Empty;
		AssertVisibleMenuItems(
			"Synchronize with Customs",
			"-",
			"Set Entry as Failed From Transmission",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusSNTAndDepartureStatusPREReleaseStatus1()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		nctsHeader.BH_ReleaseStatus = "1";
		AssertVisibleMenuItems(
			"Synchronize with Customs",
			"-",
			"Set Entry as Failed From Transmission",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusREJAndDepartureStatusPREReleaseStatusEmpty()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		nctsHeader.BH_ReleaseStatus = ZString.Empty;
		AssertVisibleMenuItems(
			"Send to Customs",
			"Synchronize with Customs",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusREJAndDepartureStatusPREReleaseStatus1()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		nctsHeader.BH_ReleaseStatus = "1";
		AssertVisibleMenuItems(
			"Send to Customs",
			"Synchronize with Customs",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusREJAndDepartureStatusPREReleaseStatusNot1()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		nctsHeader.BH_ReleaseStatus = "A";
		AssertVisibleMenuItems(
			"Send to Customs",
			"Synchronize with Customs",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithMRNWithMessageStatusSNTAndDepartureStatusPREReleaseStatusEmpty()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		nctsHeader.BH_ReleaseStatus = ZString.Empty;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Synchronize with Customs",
			"-",
			"Check for Inbox Notifications",
			"View on Customs Website",
			"Set Entry as Failed From Transmission",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithMRNWithMessageStatusSNTAndDepartureStatusPREReleaseStatus1()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		nctsHeader.BH_ReleaseStatus = "1";
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Synchronize with Customs",
			"-",
			"Check for Inbox Notifications",
			"View on Customs Website",
			"Set Entry as Failed From Transmission",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithMRNWithMessageStatusREJAndDepartureStatusPREReleaseStatusEmpty()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		nctsHeader.BH_ReleaseStatus = ZString.Empty;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Synchronize with Customs",
			"-",
			"Check for Inbox Notifications",
			"View on Customs Website",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithMRNWithMessageStatusREJAndDepartureStatusPREReleaseStatus1()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		nctsHeader.BH_ReleaseStatus = "1";
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Synchronize with Customs",
			"-",
			"Check for Inbox Notifications",
			"View on Customs Website",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithMRNWithMessageStatusREJAndDepartureStatusPREReleaseStatusNot1()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		nctsHeader.BH_ReleaseStatus = "A";
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Synchronize with Customs",
			"-",
			"Check for Inbox Notifications",
			"View on Customs Website",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusSNTAndDepartureStatusDGP()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance;
		AssertVisibleMenuItems(
			"Make Arrival Notification for this Departure",
			"-",
			"Set Entry as Failed From Transmission",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNAndMessageStatusREJAndDepartureStatusDGP()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance;
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make Arrival Notification for this Departure",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusSNTAndDepartureStatusC01()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
		AssertVisibleMenuItems(
			"Make Arrival Notification for this Departure",
			"-",
			"Set Entry as Failed From Transmission",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusREJAndDepartureStatusC01()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make Arrival Notification for this Departure",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusSNTAndCustomsStatusREL()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
		AssertVisibleMenuItems(
			"Make Arrival Notification for this Departure",
			"-",
			"Set Entry as Failed From Transmission",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusREJAndCustomsStatusREL()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make Arrival Notification for this Departure",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusREJAndDepartureStatusEmptyAndPhaseStatusTNN()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
		AssertVisibleMenuItems(
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithoutMRNWithMessageStatusREJDepartureStatusEmpty()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
		AssertVisibleMenuItems(
			"Send to Customs",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithMRNDepartureStatusCAN()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Cancelled;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithMRNDepartureStatusINV()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Invalidated;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithMRNDepartureStatusMRN()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithMRNDepartureStatusPRE()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Synchronize with Customs",
			"-",
			"Check for Inbox Notifications",
			"View on Customs Website",
			"-",
			"Import Entry Lines",
			"Import Invoice Lines",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_DepartureWithMRNDepartureStatusCO1()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Download TAD (Transit Accompanying Document)",
			"Make Arrival Notification for this Departure",
			"-",
			"Check for Inbox Notifications",
			"Capture from Customs",
			"View on Customs Website",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithoutCustomsStatusWithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertVisibleMenuItems(
			"Make TNN for this Arrival",
			"-",
			"Set Entry as Failed From Transmission",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithCustomsStatusCL1()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Load Data for Unloading",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"Create EXS declaration",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithCustomsStatusCL1WithMessageStatusACK()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Acknowledged;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Load Data for Unloading",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithCustomsStatusCL1()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		AssertVisibleMenuItems(
			"Create EXS declaration",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithCustomsStatusCL1WithMessageStatusACK()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Acknowledged;
		AssertVisibleMenuItems("-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithoutCustomsStatus()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
			CreateEntryNumber(nctsHeader);
			AssertVisibleMenuItems(
				"Send to Customs",
				"Make TNN for this Arrival",
				"-",
				"Capture from Customs",
				"View on Customs Website",
				"-");

			nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
			AssertVisibleMenuItems(
				"Send to Customs",
				"-",
				"Capture from Customs",
				"View on Customs Website",
				"-");
		});
	}

	public void TestRefreshMenu_ArrivalWithMRNWithAndWithoutHeaderTNN()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			CreateEntryNumber(nctsHeader);
			nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
			var nctsHeaderTNN = Factory.New<NctsHeader>();
			nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
			CreateEntryNumber(nctsHeaderTNN);
			var tnnMovement = nctsHeaderTNN.MovementHeader;
			tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

			AssertVisibleMenuItems(
				"Send to Customs",
				"-",
				"Capture from Customs",
				"View on Customs Website",
				"-");

			tnnMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			AssertVisibleMenuItems(
				"Send to Customs",
				"-",
				"Capture from Customs",
				"View on Customs Website",
				"-");

			nctsHeader.ESNctsHeader.CEN_TNNArrival = false;
			AssertVisibleMenuItems(
				"Send to Customs",
				"Make TNN for this Arrival",
				"-",
				"Capture from Customs",
				"View on Customs Website",
				"-");
		});
	}

	public void TestRefreshMenu_ArrivalWithMRN_WithHeaderTNN()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		CreateEntryNumber(nctsHeader);
		nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
		var nctsHeaderTNN = Factory.New<NctsHeader>();
		nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
		CreateEntryNumber(nctsHeaderTNN);
		var tnnMovement = nctsHeaderTNN.MovementHeader;
		tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

		nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;
		AssertVisibleMenuItems(
			"Send to Customs",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-");

		tnnMovement.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
		AssertVisibleMenuItems(
			"Send to Customs",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-");

		nctsHeader.ESNctsHeader.CEN_TNNArrival = false;
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make TNN for this Arrival",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRN_WithoutCustomsStatus()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ZString.Empty;
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make TNN for this Arrival",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithCustomsStatusUAPWithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"View on Customs Website",
			"Set Entry as Failed From Transmission",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithCustomsStatusUAPWithoutMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Load Data for Unloading",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithCustomsStatusUAPWithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertVisibleMenuItems(
			"Set Entry as Failed From Transmission",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithCustomsStatusUAPWithoutMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = ZString.Empty;
		AssertVisibleMenuItems(
			"Send to Customs",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithPhase007WithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Make TNN for this Arrival",
			"-",
			"View on Customs Website",
			"Set Entry as Failed From Transmission",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithBM_CustomsStatusWithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"View on Customs Website",
			"Set Entry as Failed From Transmission",
			"-");

		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		AssertVisibleMenuItems(
			"View on Customs Website",
			"Set Entry as Failed From Transmission",
			"-");

		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
		AssertVisibleMenuItems(
			"View on Customs Website",
			"Set Entry as Failed From Transmission",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithPhase007WithoutMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make TNN for this Arrival",
			"Load Data for Unloading",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithBM_CustomsStatusWithoutMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Load Data for Unloading",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-");

		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		AssertVisibleMenuItems(
			"Load Data for Unloading",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"Create EXS declaration",
			"-");

		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
		AssertVisibleMenuItems(
			"Load Data for Unloading",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithPhase007WithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertVisibleMenuItems(
			"Make TNN for this Arrival",
			"-",
			"Set Entry as Failed From Transmission",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithBM_CustomsStatusWithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertVisibleMenuItems(
			"Set Entry as Failed From Transmission",
			"-");

		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		AssertVisibleMenuItems(
			"Set Entry as Failed From Transmission",
			"-");

		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
		AssertVisibleMenuItems(
			"Set Entry as Failed From Transmission",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithPhase007WithoutMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make TNN for this Arrival",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithBM_CustomsStatusWithoutMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
		AssertVisibleMenuItems(
			"Send to Customs",
			"-");

		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		AssertVisibleMenuItems(
			"Create EXS declaration",
			"-");

		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
		provider.RefreshMenu();
		AssertVisibleMenuItems("-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithCustomsStatusC01()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl;
		AssertVisibleMenuItems("-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithCustomsStatusCD4()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
		AssertVisibleMenuItems("-");
	}

	public void TestRefreshMenu_ArrivalAndDepartureWithMRN()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"-",
			"Check for Inbox Notifications",
			"Capture from Customs",
			"View on Customs Website",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_ArrivalAndDepartureWithoutMRN()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
		AssertVisibleMenuItems(
			"Send to Customs",
			"-",
			"&Copy Previous Goods Item");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithPhase044WithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Make TNN for this Arrival",
			"-",
			"View on Customs Website",
			"Set Entry as Failed From Transmission",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithPhaseTSAWithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNcts5ArrivalPhaseList.Codes.TemporaryStorageActivated;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Make TNN for this Arrival",
			"-",
			"View on Customs Website",
			"Set Entry as Failed From Transmission",
			"View TS Register",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithPhase044WithoutMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make TNN for this Arrival",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-",
			"Into Temporary Storage",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithMRNWithPhaseTSAWithoutMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNcts5ArrivalPhaseList.Codes.TemporaryStorageActivated;
		CreateEntryNumber(nctsHeader);
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make TNN for this Arrival",
			"-",
			"Capture from Customs",
			"View on Customs Website",
			"-",
			"Into Temporary Storage",
			"View TS Register",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithPhase044WithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertVisibleMenuItems(
			"Make TNN for this Arrival",
			"-",
			"Set Entry as Failed From Transmission",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithPhaseTSAWithMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNcts5ArrivalPhaseList.Codes.TemporaryStorageActivated;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertVisibleMenuItems(
			"Make TNN for this Arrival",
			"-",
			"Set Entry as Failed From Transmission",
			"View TS Register",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithPhase044WithoutMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make TNN for this Arrival",
			"-",
			"Into Temporary Storage",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithoutMRNWithPhaseTSAWithoutMessageStatusSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNcts5ArrivalPhaseList.Codes.TemporaryStorageActivated;
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make TNN for this Arrival",
			"-",
			"Into Temporary Storage",
			"View TS Register",
			"-");
	}

	public void TestRefreshMenu_ArrivalWithPhaseTSAViewTSRegisterEnabled()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNcts5ArrivalPhaseList.Codes.TemporaryStorageActivated;
		AssertVisibleMenuItems(
			"Send to Customs",
			"Make TNN for this Arrival",
			"-",
			"Into Temporary Storage",
			"View TS Register",
			"-");
	}

	public void TestRefreshMenu_SendToCustomsIsNotAvailable_WhenStatusIsSNT()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
		AssertVisibleMenuItems(
			"Set Entry as Failed From Transmission",
			"-",
			"&Copy Previous Goods Item");
	}

	void CreateEntryNumber(NctsHeader header)
	{
		var newEntryNumber = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = "23ES239928883";
	}

	#endregion

	#region Launch Customs Website

	public void TestLaunchCustomsWebsiteArrival()
	{
		AssertLaunchCustomsWebsite(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
	}

	public void TestLaunchCustomsWebsite_NctsDepartureAndArrival()
	{
		AssertLaunchCustomsWebsite(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
	}

	public void TestLaunchCustomsWebsite_NctsDeparture()
	{
		AssertLaunchCustomsWebsite(EU.NCTS.Business.NctsMovementType.Codes.Departure);
	}

	void AssertLaunchCustomsWebsite(ZString nctsMovementType)
	{
		var expectedMRN = "AACCRRRRRRNNNNNNNN";
		var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADTR-JDIT/Ncts5Detalle?CLAVE=" + expectedMRN;

		nctsHeader.SetMovementType(nctsMovementType);
		if (nctsHeader.IsArrivalMovement)
		{
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl;
		}

		var commonMenuProvider = new ESNctsCommonMessagingMenuProvider(nctsHeader);

		WebUrlLauncher.ClearLastUrlLaunched();

		CombineAssertions(() =>
		{
			commonMenuProvider.LaunchCustomsWebsite();
			AssertNullOrEmpty("No url was launched when Header has no MRN", WebUrlLauncher.LastUrlLaunched);

			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = expectedMRN;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			commonMenuProvider.LaunchCustomsWebsite();
			AssertEquals("The correct url has been launched", expectedUrl, WebUrlLauncher.LastUrlLaunched);
			WebUrlLauncher.ClearLastUrlLaunched();
		});
	}

	#endregion

	#region Create EXS Declaration

	public void TestCreateEXSDeclaration_PreSave()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var menuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Create EXS declaration");

			CombineAssertions(() =>
			{
				menuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before Send Pre-Declaration", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	public void TestCreateEXSDeclaration()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
		nctsHeader.EffectiveMessageStatus = ZString.Empty;
		nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		Factory.Save();

		ZFormModaliser.ShowDialogsInTest = false;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var menuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Create EXS declaration");
			menuItem.PerformClick();
			AssertEquals("Message when create EXS declaration with arrival", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("EXS custom declaration with number"));
		}
	}

	public void TestCreateEXSDeclaration_Error()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = Business.NctsTransitStatusList.Codes.DeclarationInitial;
		nctsHeader.EffectiveMessageStatus = ZString.Empty;
		Factory.Save();

		ZFormModaliser.ShowDialogsInTest = false;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var menuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Create EXS declaration");
			menuItem.PerformClick();
			AssertEquals("Error message when create EXS declaration", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("EXS custom declaration could not be created"));
		}
	}

	#endregion

	#region Sending Common Departure validations

	public void TestSendToCustomsCore_Departure_Validations()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.GoodsItems.AddNew();
			goodsItem.BY_Type = "A";
			goodsItem.BY_Description = GoodsDescription;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.Guarantees.AddNew();

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after broker not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.BH_CustomsProfile = ZString.Empty;
					nctsHeader.Factory.Save();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared invalid", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared not authorized", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	public void TestSendToCustomsCore_Departure_PreSave()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
		nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
		nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
		nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

			CombineAssertions(() =>
			{
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before Send to Customs", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	public void TestSendToCustomsClick_Departure_NotSendableCustomsStatus()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.MovementHeader.BM_CustomsStatus = "ACS";

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, string.Empty))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();

					sendToCustomsMenuItem.PerformClick();
					AssertEquals("No sending options available", NoSendingOptionsAvailablePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	#endregion

	#region Sending Departure

	[RequiresSTA]
	public void TestSendToCustomsCore_Departure()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.GoodsItems.AddNew();
			goodsItem.BY_Type = "A";
			goodsItem.BY_Description = GoodsDescription;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);
				var declarationTypeDropEdit = FindDeclarationTypeDropEdit(nctsMovementForm);

				CombineAssertions(() =>
				{
					AssertEquals("When the declaration has not been sent, MainTabPage should not be locked, DeclarationTypeDropEdit.ReadOnly", false, declarationTypeDropEdit.ReadOnly);

					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("When the declaration has been sent, MainTabPage should be locked, DeclarationTypeDropEdit.ReadOnly", true, declarationTypeDropEdit.ReadOnly);
				});
			}
		}
	}

	public void TestSendToCustomsCore_Departure_EditMessageText()
	{
		using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.GoodsItems.AddNew();
			goodsItem.BY_Type = "A";
			goodsItem.BY_Description = GoodsDescription;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.Guarantees.AddNew();

			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var nctsMessagingMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Spain, new TestObjectHandle(new Phase5MessagingMenuProviderForTest(nctsHeader, false, true)) }
			};

			using (ObjectFactory.Substitute("NCTSMessagingMenuProviders", nctsMessagingMenuProviders))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);
				var declarationTypeDropEdit = FindDeclarationTypeDropEdit(nctsMovementForm);

				CombineAssertions(() =>
				{
					AssertEquals("When the declaration has not been sent, MainTabPage should not be locked, DeclarationTypeDropEdit.ReadOnly", false, declarationTypeDropEdit.ReadOnly);

					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertContains("New message's text has been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("When the declaration has been sent, MainTabPage should be locked, DeclarationTypeDropEdit.ReadOnly", true, declarationTypeDropEdit.ReadOnly);
				});
			}
		}
	}

	#endregion

	#region Sending Departure Pre-Declaration

	public void TestSendToCustomsCore_DeparturePreDeclaration()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.GoodsItems.AddNew();
			goodsItem.BY_Type = "A";
			goodsItem.BY_Description = GoodsDescription;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Resend Message Popup

	public void TestResendMessagePopup_Departure()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var consignment = nctsHeader.Bills.AddNew();
			var goodsItem = consignment.GoodsItems.AddNew();
			goodsItem.BY_Type = "A";
			goodsItem.BY_Description = GoodsDescription;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.Guarantees.AddNew();

			nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			nctsHeader.MovementHeader.BM_CustomsStatus = "";

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure, isResending: true))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Resend pop up is shown when departure is readonly", "Are you sure you want to resend this message?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}

	[RequiresSTA]
	public void TestResendMessagePopup_Arrival()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, isResending: true))
			using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Resend pop up is shown when arrival is readonly", "Are you sure you want to resend this message?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}

	#endregion

	#region Sending Arrival

	public void TestSendToCustomsCore_Arrival_Validations()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.GetEffectiveGuarantees().AddNew();
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after broker not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.BH_CustomsProfile = ZString.Empty;
					nctsHeader.Factory.Save();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared invalid", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared not authorized", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	public void TestSendToCustomsCore_Arrival()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMrnFromUser = GoodsDescription;
			nctsHeader.DestinationCustomsOfficeCodeForArrival = "ES009999";
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;
			nctsHeader.GetEffectiveGuarantees().AddNew();

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification))
			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsArrivalMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.ArrivalMovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 007", ESNctsMovementHeaderTransactionStatusList.Codes.Arrival, nctsHeader.ArrivalMovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	public void TestSendToCustomsCore_Arrival_EditMessageText()
	{
		using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMrnFromUser = GoodsDescription;
			nctsHeader.DestinationCustomsOfficeCodeForArrival = "ES009999";
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;
			nctsHeader.GetEffectiveGuarantees().AddNew();
			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var nctsMessagingMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Spain, new TestObjectHandle(new Phase5MessagingMenuProviderForTest(nctsHeader, false, true)) }
			};

			using (ObjectFactory.Substitute("NCTSMessagingMenuProviders", nctsMessagingMenuProviders))
			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsArrivalMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.ArrivalMovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 007", ESNctsMovementHeaderTransactionStatusList.Codes.Arrival, nctsHeader.ArrivalMovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, msg.EM_MessageType);
					AssertContains("New message's text has been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	public void TestSendToCustomsCore_Arrival_IncidentsTabPageReadOnly()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
			nctsHeader.EffectiveMessageStatus = ZString.Empty;
			nctsHeader.EnRouteIncidents.AddNew();
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification))
			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsArrivalMovementForm);
				var mainTabControl = nctsArrivalMovementForm.FindSingle<ZTemplateTabControl>("MainTabControl");
				var incidentsTabPage = mainTabControl.TabPages.Cast<ZTabPage>().Single(x => x.Name == "IncidentsTabPage");
				incidentsTabPage.Show();
				var incidentsGrid = incidentsTabPage.FindSingle<ZGrid>("IncidentsGrid");
				var incidentCodeDropEdit = incidentsTabPage.FindSingle<ZDropEdit>("IncidentCodeDropEdit");
				var informationTextBox = incidentsTabPage.FindSingle<ZTextBox>("InformationTextBox");

				CombineAssertions(() =>
				{
					AssertEquals("IncidentsTabPage's IncidentsGrid is not readonly when EffectiveMessageStatus is empty", false, incidentsGrid.ReadOnly);
					AssertEquals("IncidentsTabPage's IncidentCodeDropEdit is not readonly when EffectiveMessageStatus is empty", false, incidentCodeDropEdit.ReadOnly);
					AssertEquals("IncidentsTabPage's InformationTextBox is not readonly when EffectiveMessageStatus is empty", false, informationTextBox.ReadOnly);

					sendToCustomsMenuItem.PerformClick();

					AssertEquals("EffectiveMessageStatus is SNT", NctsMessageStatusList.Codes.SentToCustoms, nctsHeader.EffectiveMessageStatus);
					AssertEquals("IncidentsTabPage's IncidentsGrid is readonly when EffectiveMessageStatus is SNT", true, incidentsGrid.ReadOnly);
					AssertEquals("IncidentsTabPage's IncidentCodeDropEdit is readonly when EffectiveMessageStatus is SNT", true, incidentCodeDropEdit.ReadOnly);
					AssertEquals("IncidentsTabPage's InformationTextBox is readonly when EffectiveMessageStatus is SNT", true, informationTextBox.ReadOnly);
				});
			}
		}
	}

	#endregion

	#region Sending Departure Notification Goods

	public void TestSendToCustomsCore_NotificationGoods_Validations()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_CustomsStatus = "";
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();

					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Cannot send Notification of Goods message", CannotSendNotificationOfGoodsPopupText, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	const string ComparatorTestFilesPath = "Enterprise.Customs.ES.NCTS.Business.Testing.NCTS.NctsDeclarationComparator.TestFiles";

	public void TestSendToCustomsCore_NotificationGoods_CheckPreDeclarationChanges()
	{
		var expectedError = @"There are changes in the Pre-declaration which have not been submitted to Customs. Please, send an Amendment if the changes are correct or use the menu option ‘Synchronize with Customs‘ to get declared data from Customs.
There are changes in:
Consignment/Destination Country-Region
Consignment/Gross Weight
Consignment/Unique Consignment Reference";
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movement = nctsHeader.MovementHeader;
			movement.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			movement.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
			nctsHeader.BH_ReleaseStatus = "1";
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";

			var glbBranch = Factory.New<GlbBranch>();
			glbBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			glbBranch.GB_Code = "XAX";

			var sentGuid = new ZGuid("A7436A81-BEBB-4A8F-81AD-80E4212F7C37");

			var messageText = ESNctsTestFileReader.GetEmbeddedFileText(ComparatorTestFilesPath, "TestDPM_Simplified.txt");

			var message = Factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment;
			message.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse;
			message.EM_SystemCreateTimeUtc = DateTime.Now;
			message.EM_GB = glbBranch.PK;

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = new ZGuid("464EF95C-010C-42B2-971A-8AC2DEFD4822");
			responseInterchange.EI_From = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;
			responseInterchange.EI_To = "CW1";
			responseInterchange.EI_HeaderText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AH</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>N</TestMessage>
  <Service>Service</Service>
  <Operation>Operation</Operation>
  <SentEDIMessageNumber>10</SentEDIMessageNumber>
</Headers>");
			responseInterchange.ContainedMessages.Add(message);

			nctsHeader.Messages.Add(message);

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			message.EM_EI = interchange.PK;
			interchange.EI_SessionGUID = sentGuid;

			var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			sentInterchange.EI_SessionGUID = sentGuid;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			var sentMessage = Factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
			sentMessage.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment;
			sentMessage.EM_EI = sentInterchange.PK;
			sentMessage.EM_GB = glbBranch.PK;
			sentMessage.EM_MessageText = messageText;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Message informing declaration has changes not declared", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared not authorized", UnitTestUserNotification.Instance.LastMessage.Text);

					movement.BM_TypeOfSecurity = "EXI";
					movement.BM_RL_NKDestinationPort = "ES";
					movement.BM_GrossWeight = 2;
					movement.BM_GrossWeightUQ = "KG";
					movement.BM_UniqueConsignmentReference = "REF";

					nctsHeader.Factory.Save();
					sendToCustomsMenuItem.PerformClick();
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);

					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	public void TestSendToCustomsCore_NotificationGoods()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					var mainTabPage = FindMainTabPage(nctsMovementForm);
					var declarationTypeDropEdit = FindDeclarationTypeDropEdit(mainTabPage);
					AssertEquals("Expected not ReadOnly declarationTypeDropEdit", false, declarationTypeDropEdit.ReadOnly);

					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("Expected ReadOnly declarationTypeDropEdit", true, declarationTypeDropEdit.ReadOnly);

					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Sending Departure Cancel

	public void TestSendToCustomsCore_Cancellation_Validations()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_CustomsStatus = "";
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();

					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Cannot send Cancellation message", CannotSendCancellationPopupText, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustomsCore_Cancellation()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					var mainTabPage = FindMainTabPage(nctsMovementForm);
					var declarationTypeDropEdit = FindDeclarationTypeDropEdit(mainTabPage);
					AssertEquals("Expected not ReadOnly declarationTypeDropEdit", false, declarationTypeDropEdit.ReadOnly);

					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 014", ESNctsMovementHeaderTransactionStatusList.Codes.Cancellation, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("Expected ReadOnly declarationTypeDropEdit", true, declarationTypeDropEdit.ReadOnly);

					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Sending Departure Amendment

	public void TestSendToCustomsCore_Amendment_Validations()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_CustomsStatus = "";
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();

					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Cannot send Amendment message", CannotSendAmendmentPopupText, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	public void TestSendToCustomsCore_Amendment()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					var mainTabPage = FindMainTabPage(nctsMovementForm);
					var declarationTypeDropEdit = FindDeclarationTypeDropEdit(mainTabPage);
					AssertEquals("Expected not ReadOnly declarationTypeDropEdit", false, declarationTypeDropEdit.ReadOnly);

					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 013", ESNctsMovementHeaderTransactionStatusList.Codes.Amendment, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("Expected ReadOnly declarationTypeDropEdit", true, declarationTypeDropEdit.ReadOnly);

					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Synchronize with Customs

	[RequiresSTA]
	public void TestSynchronizeWithCustoms_Validations()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = "ZZ";
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var synchronizeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Synchronize with Customs");

				CombineAssertions(() =>
				{
					synchronizeMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after broker not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.BH_CustomsProfile = ZString.Empty;
					nctsHeader.Factory.Save();
					synchronizeMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					synchronizeMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					var expectedChangesError = "There are no changes in data already sent in Pre-declaration so this action will not be triggered";

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared invalid", UnitTestUserNotification.Instance.LastMessage.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					synchronizeMenuItem.PerformClick();
					AssertEquals("Message informing there are no changes in declaration", expectedChangesError, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared not authorized", UnitTestUserNotification.Instance.LastMessage.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					nctsHeader.Reload();
					synchronizeMenuItem.PerformClick();
					AssertEquals("Message informing there are no changes in declaration, even if we haven't done a new save& validation", expectedChangesError, UnitTestUserNotification.Instance.LastMessage.Text);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
					synchronizeMenuItem.PerformClick();
					AssertEquals("Should have message asking if the user wants to send a TQU, should be last message when answer is No", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSendTQUMessageToSynchronizePopUpText));

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after broker not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					synchronizeMenuItem.PerformClick();
					AssertEquals("Should have message asking if the user wants to send a TQU, should not be last message when answer is Yes", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSendTQUMessageToSynchronizePopUpText));
				});
			}
		}
	}

	public void TestSynchronizeWithCustoms_PreSave()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
		nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
		nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var synchronizeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Synchronize with Customs");

			CombineAssertions(() =>
			{
				synchronizeMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before Synchronize with Customs", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	[RequiresSTA]
	public void TestSynchronizeWithCustoms()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
			nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;

			var sentGuid = new ZGuid("A7436A81-BEBB-4A8F-81AD-80E4212F7C37");

			var messageText = ESNctsTestFileReader.GetEmbeddedFileText(ComparatorTestFilesPath, "TestDPM_Simplified.txt");

			var message = Factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment;
			message.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse;
			message.EM_SystemCreateTimeUtc = DateTime.Now;
			message.EM_GB = Env.CurrentBranchPK;

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = new ZGuid("464EF95C-010C-42B2-971A-8AC2DEFD4822");
			responseInterchange.EI_From = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;
			responseInterchange.EI_To = "CW1";
			responseInterchange.EI_HeaderText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AH</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>N</TestMessage>
  <Service>Service</Service>
  <Operation>Operation</Operation>
  <SentEDIMessageNumber>10</SentEDIMessageNumber>
</Headers>");
			responseInterchange.ContainedMessages.Add(message);

			nctsHeader.Messages.Add(message);

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			message.EM_EI = interchange.PK;
			interchange.EI_SessionGUID = sentGuid;

			var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			sentInterchange.EI_SessionGUID = sentGuid;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			var sentMessage = Factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
			sentMessage.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment;
			sentMessage.EM_EI = sentInterchange.PK;
			sentMessage.EM_GB = Env.CurrentBranchPK;
			sentMessage.EM_MessageText = messageText;

			var sendingObject = new NctsMessageSendingObject(nctsHeader, staff);
			Factory.Save();

			_ = nctsHeader.CheckPreDeclarationChanges(sendingObject);

			ZFormModaliser.ShowDialogsInTest = false;
			UnitTestUserNotification.Instance.AddYesAnswer();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var synchronizeMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Synchronize with Customs");

				CombineAssertions(() =>
				{
					var mainTabPage = FindMainTabPage(nctsMovementForm);
					var declarationTypeDropEdit = FindDeclarationTypeDropEdit(mainTabPage);
					AssertEquals("Expected not ReadOnly declarationTypeDropEdit", false, declarationTypeDropEdit.ReadOnly);

					nctsHeader.Factory.Save();
					AssertEquals("Prereq: UpdatePreDeclaration", false, nctsHeader.UpdatePreDeclaration);
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					synchronizeMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("UpdatePreDeclaration is set to true", true, nctsHeader.UpdatePreDeclaration);
					AssertEquals("Expected ReadOnly declarationTypeDropEdit", true, declarationTypeDropEdit.ReadOnly);

					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.TransitNcts5Query, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Sending Departure Annexes

	[RequiresSTA]
	public void TestSendToCustomsCore_Annexes_Validations()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_CustomsStatus = "";
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();

					sendToCustomsMenuItem.PerformClick();
					AssertEquals("Cannot send Annexes message", CannotSendAnnexesPopupText, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustomsCore_Annexes()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_CustomsStatus = "CO1";
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			var eDoc1 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot1 = nctsHeader.EDocPivotCollection.AddNew();
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;

			nctsHeader.Factory.Save();
			nctsHeader.DocManagerInfo.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, isResending: false))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					var mainTabPage = FindMainTabPage(nctsMovementForm);
					var declarationTypeControl = FindDeclarationTypeDropEdit(mainTabPage);
					AssertEquals("Expected ReadOnly DeclarationType Control", true, declarationTypeControl.ReadOnly);

					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is DOT", ESNctsMovementHeaderTransactionStatusList.Codes.Annexes, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("Expected ReadOnly DeclarationType Control", true, declarationTypeControl.ReadOnly);

					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Request Inbox Notifications

	public void TestRequestInboxNotifications_CertificateValidation()
	{
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.BM_GS_NKCusAgent = ZString.Empty;
		nctsHeader.BH_CustomsProfile = ZString.Empty;

		nctsHeader.Factory.Save();

		using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
		{
			var requestInboxNotifEntryMenuItem = nctsMovementForm.FindMenuItem_ForTest("Check for Inbox Notifications");

			CombineAssertions(() =>
			{
				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Message informing declaration need broker and certificate declared", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
				nctsHeader.BH_CustomsProfile = ZString.Empty;
				nctsHeader.Factory.Save();
				provider.RefreshMenu();
				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Message informing declaration need broker and certificate declared when broker is declared but certificate is empty", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.BH_CustomsProfile = "INVALID";
				nctsHeader.Factory.Save();
				provider.RefreshMenu();
				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

				nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
				nctsHeader.Factory.Save();
				provider.RefreshMenu();
				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				nctsHeader.Reload();
				provider.RefreshMenu();
				requestInboxNotifEntryMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestRequestInboxNotifications_DepartureStatusValidation()
	{
		var nctsHeader = GetNctsDeparture();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				var requestInboxNotifEntryMenuItem = nctsMovementForm.FindMenuItem_ForTest("Check for Inbox Notifications");

				CombineAssertions(() =>
				{
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertEquals("Message informing declarations need to be in C01, PRE, DGP or REL status", "Current NCTS status does not expect any Inbox Notification message.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
					provider.RefreshMenu();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertNotEquals("Message informing declarations need to be in C01, PRE, DGP or REL status not display cause the status is C01", "Current NCTS status does not expect any Inbox Notification message.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Invalidated;
					provider.RefreshMenu();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertEquals("Message informing declarations need to be in C01, PRE, DGP or REL status", "Current NCTS status does not expect any Inbox Notification message.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.PreLodged;
					provider.RefreshMenu();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertNotEquals("Message informing declarations need to be in C01, PRE, DGP or REL status not display cause the status is PRE", "Current NCTS status does not expect any Inbox Notification message.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Acknowledged;
					provider.RefreshMenu();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertEquals("Message informing declarations need to be in C01, PRE, DGP or REL status", "Current NCTS status does not expect any Inbox Notification message.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance;
					provider.RefreshMenu();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertNotEquals("Message informing declarations need to be in C01, PRE, DGP or REL status not display cause the status is DGP", "Current NCTS status does not expect any Inbox Notification message.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Cancelled;
					provider.RefreshMenu();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertEquals("Message informing declarations need to be in C01, PRE, DGP or REL status", "Current NCTS status does not expect any Inbox Notification message.", UnitTestUserNotification.Instance.LastMessage.Text);

					nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
					provider.RefreshMenu();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertNotEquals("Message informing declarations need to be in C01, PRE, DGP or REL status not display cause the status is REL", "Current NCTS status does not expect any Inbox Notification message.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	public void TestRequestInboxNotifications_C01_EHub()
	{
		var nctsHeader = GetNctsDeparture(ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl);
		Factory.Save();

		AssertRequestInboxNotifications_EHub(nctsHeader, "2 In-box Notification requests created",
												new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance,
																DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture });
	}

	public void TestRequestInboxNotifications_C01_xT()
	{
		var nctsHeader = GetNctsDeparture(ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl);
		Factory.Save();

		AssertRequestInboxNotifications_xT(nctsHeader, "2 In-box Notification requests created",
													new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance,
																	DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture },
													new ZString[] { InboxNotificationResponseTypes.NCTSClearance,
																	InboxNotificationResponseTypes.NCTSNonConformity });
	}

	public void TestRequestInboxNotifications_PRE_EHub()
	{
		var nctsHeader = GetNctsDeparture(ESNCTS5DepartureCustomsStatusList.Codes.PreLodged);
		Factory.Save();

		AssertRequestInboxNotifications_EHub(nctsHeader, "1 In-box Notification request created",
												new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation });
	}

	public void TestRequestInboxNotifications_PRE_xT()
	{
		var nctsHeader = GetNctsDeparture(ESNCTS5DepartureCustomsStatusList.Codes.PreLodged);
		Factory.Save();

		AssertRequestInboxNotifications_xT(nctsHeader, "1 In-box Notification request created",
													new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation },
													new ZString[] { InboxNotificationResponseTypes.NCTSInvalidation });
	}

	public void TestRequestInboxNotifications_DGP_EHub()
	{
		var nctsHeader = GetNctsDeparture(ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance);
		Factory.Save();

		AssertRequestInboxNotifications_EHub(nctsHeader, "3 In-box Notification requests created",
												new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance,
																DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation,
																DeclarationMessageTypeList.Codes.InboxNotificationNctsControls });
	}

	public void TestRequestInboxNotifications_DGP_xT()
	{
		var nctsHeader = GetNctsDeparture(ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance);
		Factory.Save();

		AssertRequestInboxNotifications_xT(nctsHeader, "3 In-box Notification requests created",
													new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance,
																	DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation,
																	DeclarationMessageTypeList.Codes.InboxNotificationNctsControls },
													new ZString[] { InboxNotificationResponseTypes.NCTSClearance,
																	InboxNotificationResponseTypes.NCTSInvalidation,
																	InboxNotificationResponseTypes.NCTSCceControl });
	}

	public void TestRequestInboxNotifications_REL_EHub()
	{
		var nctsHeader = GetNctsDeparture(ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);
		Factory.Save();

		AssertRequestInboxNotifications_EHub(nctsHeader, "1 In-box Notification request created",
												new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation });
	}

	public void TestRequestInboxNotifications_REL_xT()
	{
		var nctsHeader = GetNctsDeparture(ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);
		Factory.Save();

		AssertRequestInboxNotifications_xT(nctsHeader, "1 In-box Notification request created",
													new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation },
													new ZString[] { InboxNotificationResponseTypes.NCTSInvalidation });
	}

	void AssertRequestInboxNotifications_EHub(NctsHeader nctsHeader, ZString notificationText, ZString[] types)
	{
		using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var requestInboxNotifEntryMenuItem = nctsMovementForm.FindMenuItem_ForTest("Check for Inbox Notifications");

			nctsHeader.HasChanges = true;
			provider.RefreshMenu();

			CombineAssertions(() =>
			{
				TestHelper.CheckFactoryHasNoPendingChanges("Before sending inbox notification request", nctsHeader.Factory);
				requestInboxNotifEntryMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After sending inbox notification request", nctsHeader.Factory);

				AssertEquals("Should have message asking to save the declaration before resetting", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));

				AssertEquals("Inbox Notification Request was sent correctly", notificationText, UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNewRequestEDIMessagesCreated(nctsHeader, types);
			});
		}
	}

	void AssertNewRequestEDIMessagesCreated(NctsHeader nctsHeader, ZString[] messageTypes, string messageSubType = "")
	{
		var newRequestMessages = nctsHeader.Messages;

		AssertEquals("messages count is correct", messageTypes.Length, newRequestMessages.Count);

		foreach (EDIMessage message in newRequestMessages)
		{
			AssertEquals("message.EM_MessageType", true, messageTypes.Contains(message.EM_MessageType));
			AssertRequestEDIMessageCreated(message, messageSubType);
		}
	}

	void AssertRequestInboxNotifications_xT(NctsHeader nctsHeader, ZString notificationText, ZString[] types, ZString[] urls)
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_FullName = DeclarantName;
		declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);

		nctsHeader.MovementHeader.Representative.OrganisationPK = declarant.PK;

		var mrnCode = "20ES00999830001277";
		CreateCusEntryNumber(nctsHeader, mrnCode, ZDateTime.Today);

		nctsHeader.Factory.Save();

		using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var requestInboxNotifEntryMenuItem = nctsMovementForm.FindMenuItem_ForTest("Check for Inbox Notifications");

			nctsHeader.HasChanges = true;
			provider.RefreshMenu();

			CombineAssertions(() =>
			{
				TestHelper.CheckFactoryHasNoPendingChanges("Before sending inbox notification request", nctsHeader.Factory);
				requestInboxNotifEntryMenuItem.PerformClick();
				TestHelper.CheckFactoryHasNoPendingChanges("After sending inbox notification request", nctsHeader.Factory);

				AssertEquals("Should have message asking to save the declaration before resetting", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));

				AssertEquals("Inbox Notification Request was sent correctly", notificationText, UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNewCusPollingTransaction(nctsHeader.PK, nctsHeader.TablePrefix, types, mrnCode);

				AssertNewInboxListMessages(urls);
			});
		}
	}

	void AssertNewCusPollingTransaction(ZGuid parentID, ZString parentTablePrefix, ZString[] messageTypes, ZString mrn)
	{
		var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
		query.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, mrn);
		var newTransactions = Factory.Load<CusPollingTransaction>(query);

		AssertEquals("There should only be 1 ESC transaction for each messageType", messageTypes.Length, newTransactions.Length);

		foreach (var transaction in newTransactions)
		{
			AssertEquals(transaction.CPT_Type + " transaction.CPT_ApplicationCode", Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage, transaction.CPT_ApplicationCode);
			AssertEquals(transaction.CPT_Type + " transaction.CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND, transaction.CPT_Status);
			AssertEquals(transaction.CPT_Type + " transaction.CPT_Type is in list", true, messageTypes.Contains(transaction.CPT_Type));
			AssertEquals(transaction.CPT_Type + " transaction.CPT_TransactionID", mrn, transaction.CPT_TransactionID);
			AssertEquals(transaction.CPT_Type + " transaction.CPT_ParentID", parentID, transaction.CPT_ParentID);
			AssertEquals(transaction.CPT_Type + " transaction.CPT_ParentTableCode", parentTablePrefix, transaction.CPT_ParentTableCode);
		}
		AssertContainsExactElementsInAnyOrder("All transaction.CPT_Type are correct", messageTypes, newTransactions.Select(x => x.CPT_Type));
	}

	void AssertNewInboxListMessages(ZString[] urls)
	{
		var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
		messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
		var newRequestMessages = NewFactory().Load<EDIMessage>(messagesQuery);

		AssertEquals("Messages count is correct", urls.Length, newRequestMessages.Length);

		var newRequestMessagesText = new List<ZString>();
		foreach (EDIMessage message in newRequestMessages)
		{
			AssertRequestEDIMessageCreated(message, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
			AssertEquals(message.EM_MessageType + " message.EM_MessageType", DeclarationMessageTypeList.Codes.InboxPendingList, message.EM_MessageType);

			var url = urls.FirstOrDefault(x => message.EM_MessageText.Contains(x));
			AssertMultilineASCIIEquals(message.EM_MessageType + " message.EM_MessageText", GetExpectedNewInboxMessageBodyText(url), message.EM_MessageText);
		}
	}

	void AssertRequestEDIMessageCreated(EDIMessage message, string messageSubType = "")
	{
		AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
		AssertEquals("message.EM_MessageSubType", messageSubType, message.EM_MessageSubType);
		AssertEquals("message.EM_IsTestMessage", true, message.EM_IsTestMessage);
		AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		AssertEquals("message.EM_ApplicationReference", BuilderHelperTest.CertificateName, message.EM_ApplicationReference);
		AssertNull("message doesn't have interchange", message.Interchange);
	}

	ZString GetExpectedNewInboxMessageBodyText(ZString url) => ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<ListaDecV4Ent tipoRespuesta=""{0}"" xmlns=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/li/ListaDecV4Ent.xsd"">
  <declarante>
    <NifDeclarante>{1}</NifDeclarante>
    <NombreDeclarante>{2}</NombreDeclarante>
  </declarante>
</ListaDecV4Ent>
  </soapenv:Body>
</soapenv:Envelope>", url, DeclarantId, DeclarantName);

	const string DeclarantId = "NIF22222222";
	const string DeclarantName = "Declarant Full Name";

	#endregion

	#region Set As Failed From Transmission

	[RequiresSTA]
	public void TestSetAsFailedFromTransmission()
	{
		var confirmationMessageText = "Are you sure you want to set this Entry as Failed from Transmission?";

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_MessageStatus = "AAA";
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();

				var unitTestUserNotificationInstance = UnitTestUserNotification.Instance;
				var setEntryAsFailedMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Set Entry as Failed From Transmission");

				CombineAssertions(() =>
				{
					setEntryAsFailedMenuItem.PerformClick();
					AssertEquals("Should not have message asking to confirm the action when report selected has message status not SNT", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));
					AssertEquals("NctsHeader has the same message status as before because it was not SNT", "AAA", nctsHeader.MovementHeader.BM_MessageStatus);

					nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
					Factory.Save();
					unitTestUserNotificationInstance.ClearMessagesAndAnswers();
					unitTestUserNotificationInstance.AddAnswer(ZDialogResult.Cancel);
					setEntryAsFailedMenuItem.PerformClick();
					AssertEquals("Should have message asking to confirm the action (when cancel)", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));
					AssertEquals("NctsHeader has the same message status as before", "SNT", nctsHeader.MovementHeader.BM_MessageStatus);

					unitTestUserNotificationInstance.ClearMessagesAndAnswers();
					unitTestUserNotificationInstance.AddOKAnswer();
					setEntryAsFailedMenuItem.PerformClick();
					AssertEquals("Should have message asking to confirm the action (when ok)", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));
					AssertEquals("NctsHeader has message status SNT and the confirmation was accepted so the message status is changed", "Entry was set to Failed from Transmission", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("NctsHeader has new message status", "FAL", nctsHeader.MovementHeader.BM_MessageStatus);

					var mainFactoryChangeSet = Factory.GetChanges();
					var mainFactoryHasChanges = mainFactoryChangeSet.GetChangedObjects().Any() || mainFactoryChangeSet.GetAddedObjects().Any();
					AssertEquals("After pressing OK in the pop up the change will not be saved. Does Main Factory have changes?", true, mainFactoryHasChanges);
				});
			}
		}
	}

	#endregion

	#region Capture From Customs

	public void TestCaptureFromCustoms_TransitQuery_Validations()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var capCusMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Capture from Customs");

				CombineAssertions(() =>
				{
					capCusMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after broker not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.BH_CustomsProfile = ZString.Empty;
					nctsHeader.Factory.Save();
					capCusMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					capCusMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared invalid", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					capCusMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared not authorized", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					capCusMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestCaptureFromCustoms_TransitQuery_PreSave()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
		nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
		nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var menuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Capture from Customs");

			CombineAssertions(() =>
			{
				menuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before Send Cancellation", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	public void TestCaptureFromCustoms_TransitQuery()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var capCusMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Capture from Customs");

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					capCusMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.TransitNcts5Query, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Make TNN
	public void TestMakeTNNClick_Validations()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.Factory.Save();

		ZFormModaliser.ShowDialogsInTest = false;

		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var makeTNNMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Make TNN for this Arrival");

			CombineAssertions(() =>
			{
				makeTNNMenuItem.PerformClick();
				AssertEquals("Message informing MRN is needed", "MRN is mandatory to create a TNN declaration.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared after mrn not filled", UnitTestUserNotification.Instance.LastMessage.Text);
				nctsHeader.ArrivalMrnFromUser = "AH3";
				nctsHeader.Factory.Save();

				var newFactory = NewFactory();
				var departureWithSameMRN = newFactory.New<NctsHeader>();
				departureWithSameMRN.BH_JobReference = "NCTXX000002";
				departureWithSameMRN.BH_HeaderType = NctsMovementType.Codes.Departure;
				departureWithSameMRN.MovementReferenceEntryNumber.CE_EntryNum = "AH3";
				newFactory.Save();

				makeTNNMenuItem.PerformClick();
				AssertEquals("Message informing departure with same MRN found.", "The NCTXX000002 Departure is registered with the MRN AH3 and the TNN declaration should not be required.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestMakeTNNClick_PreSave()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMrnFromUser = "AH3";
		nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var makeTNNMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Make TNN for this Arrival");

			CombineAssertions(() =>
			{
				makeTNNMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before Make TNN", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	public void TestMakeTNNClick()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMrnFromUser = "AH3";
		nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
		staff.GS_IsSystemAccount = false;
		nctsHeader.Factory.Save();

		var acceptedDateTest = new ZDateTime(2022, 09, 01);
		var clearanceDateTest = new ZDateTime(2022, 09, 02);

		var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(nctsHeader);
		tnnDataCodeInfo.AcceptanceDate = acceptedDateTest;
		tnnDataCodeInfo.ClearanceDate = clearanceDateTest;

		ZFormModaliser.ShowDialogsInTest = false;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		var nctsMessagingMenuProviders = new KeyObjectHandleDictionaryObject
		{
			{ Core.Constants.CountryCodes.Spain, new TestObjectHandle(new Phase5MessagingMenuProviderForTest(nctsHeader, false, false, tnnDataCodeInfo: tnnDataCodeInfo)) }
		};

		using (ObjectFactory.Substitute("NCTSMessagingMenuProviders", nctsMessagingMenuProviders))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			nctsMovementForm.Show();
			var makeTNNMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Make TNN for this Arrival");
			var mainTabControl = nctsMovementForm.FindSingle<ZTemplateTabControl>("MainTabControl");

			CombineAssertions(() =>
			{
				AssertEquals("TNNArrival is false", false, nctsHeader.ESNctsHeader.CEN_TNNArrival);
				AssertNull("TNNTabPage is not added to the main control when no TNN exists", mainTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Name == "TNNTabPage"));

				makeTNNMenuItem.PerformClick();
				AssertEquals("Successfully created Notifications", "TNN successfully created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("TNNArrival set to true", true, nctsHeader.ESNctsHeader.CEN_TNNArrival);
				AssertNotNull("TNNTabPage is not added to the main control when TNN exist", mainTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Name == "TNNTabPage"));

				var (_, headerFound) = nctsHeader.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
				var departureHeader = (NctsHeader)headerFound;

				AssertNotNull("Departure is available", departureHeader);
				AssertEquals("TNNArrival set to true", true, departureHeader.ESNctsHeader.CEN_TNNArrival);
				AssertEquals("Declaration is Departure", "D", departureHeader.BH_HeaderType);
				AssertEquals("Same MRN", "AH3", departureHeader.MovementReferenceNumber);
				AssertEquals("Phase 5", "NC5", departureHeader.BH_ApplicationCode);
				AssertEquals("Acceptance date in EntryNumber", acceptedDateTest, departureHeader.AcceptanceDate);
				AssertEquals("Clearance date in EntryNumber", clearanceDateTest, departureHeader.ClearanceDate);

				var depMovement = departureHeader.MovementHeader;
				AssertEquals("Phase set to TNN", "TNN", depMovement.BM_Phase);
				AssertEquals("Expected Broker populated on save as current user is not SystemAccount", "AH", depMovement.BM_GS_NKCusAgent);
				AssertEquals("Certificate populated", "TESTCERT1", departureHeader.BH_CustomsProfile);
			});
		}
	}

	#endregion

	#region Load Data for Unloading

	public void TestLoadDataForUnloadingClick_Validations_WithoutDepartureWithSameMRN()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			CreateEntryNumber(nctsHeader);
			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;

			using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var loadDataMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Load Data for Unloading");

				CombineAssertions(() =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
					loadDataMenuItem.PerformClick();
					AssertEquals("Should have message asking if the user wants to send a TQU, should be last message when answer is No", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSendTQUMessagePopUpText));

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after broker not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					loadDataMenuItem.PerformClick();
					AssertEquals("Should have message asking if the user wants to send a TQU, should not be last message when answer is Yes", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSendTQUMessagePopUpText));
					AssertEquals("Message informing representative and certificate are needed", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after broker not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.BH_CustomsProfile = ZString.Empty;
					nctsHeader.Factory.Save();
					loadDataMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					loadDataMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared invalid", UnitTestUserNotification.Instance.LastMessage.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					loadDataMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared not authorized", UnitTestUserNotification.Instance.LastMessage.Text);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					nctsHeader.Reload();
					loadDataMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	public void TestLoadDataForUnloadingClick_PreSave()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		CreateEntryNumber(nctsHeader);
		nctsHeader.Factory.Save();

		nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
		nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var loadDataMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Load Data for Unloading");

			CombineAssertions(() =>
			{
				loadDataMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before Load Data for Unloading", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	public void TestLoadDataForUnloadingClick_WithDepartureWithSameMRN_LoadFromDeparture_FinalPeriod()
	{
		TestLoadDataForUnloadingClick(
			nctsHeaderDeparture => AddDataToDepartureForUnloading(nctsHeaderDeparture),
			nctsHeaderDeparture =>
				{
					AssertContainsExactElementsInAnyOrder(
						"transportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
						new (ZShort, ZString, ZString, ZString, ZString)[]
						{
							(1, "DEC", "20", "wagon", "GB")
						},
						GetArrivalHeaderTransportInfos(nctsHeader));

					var containers = nctsHeader.ArrivalHeaderContainers;
					AssertContainsExactElementsInAnyOrder("containers (BC_SequenceNumber, BC_UnloadedState, BC_ContainerNum, BC_Mode)",
																new (ZShort, ZString, ZString, ZString)[]
																	{
																		(1, "DEC", "CONT1", "CNT"),
																		(2, "DEC", "CONT2", "CNT")
																	}, containers.Select(x => (x.BC_SequenceNumber, x.BC_UnloadedState, x.BC_ContainerNum, x.BC_Mode)));

					var containerWithSeq1 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
					var container1Seal = containerWithSeq1.Seals[0];
					AssertEquals("Container with seq 1, only seal's BK_SequenceNumber", (ZShort)1, container1Seal.BK_SequenceNumber);
					AssertEquals("Container with seq 1, only seal's BK_UnloadingState", "DEC", container1Seal.BK_UnloadingState);
					AssertEquals("Container with seq 1, only seal's BK_SealNumber", "Seal1", container1Seal.BK_SealNumber);

					var containerWithSeq2 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 2);
					var container2Seals = containerWithSeq2.Seals;
					AssertContainsExactElementsInAnyOrder("container2Seals (BK_SequenceNumber, BK_UnloadingState, BK_SealNumber)",
															new (ZShort, ZString, ZString)[]
															{
																(1, "DEC", "Seal2"),
																(2, "DEC", "Seal3"),
																(3, "DEC", "Seal4")
															}, container2Seals.Select(x => (x.BK_SequenceNumber, x.BK_UnloadingState, x.BK_SealNumber)));

					AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, MovementDetail.B9_UnloadedState, B0_Weight)",
																new (ZString, ZString, ZDecimal, ZString)[]
																{
																	("1", "DEC", 20m, "KG")
																}, nctsHeader.Bills.Select(x => (x.MovementDetail.B9_SeqNo, x.MovementDetail.B9_UnloadedState, x.B0_Weight, x.B0_WeightUQ)));

					var bill1GoodsItems = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1").ArrivalGoodsItems;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
															new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
															{
																(1, 1, "DEC", "QQQ", "88888888", 40, "KG", 10, "DG", "departuredescription"),
																(2, 2, "DEC", "WWW", "99999999", 15, "KL", 11, "L", "departuredescription2")
															}, bill1GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));

					var bill1GoodsItem1 = bill1GoodsItems.First(x => x.BY_LineNo == 1);
					var bill1GoodsItem1Packages = bill1GoodsItem1.Packages;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
															new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
															{
																(1, "DEC", "AA", 5, "departuremarks1", ZString.Empty, ZString.Empty, ZString.Empty),
																(2, "DEC", "BB", 20, "departuremarks2", ZString.Empty, ZString.Empty, ZString.Empty)
															}, bill1GoodsItem1Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

					var bill1GoodsItem1Package1Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Package1ContainersPivots.Container", new[] { containerWithSeq1 }, bill1GoodsItem1Package1Containers);
					var bill1GoodsItem1Package2Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Package2ContainersPivots.Container", new[] { containerWithSeq1 }, bill1GoodsItem1Package2Containers);

					var bill1GoodsItem1SupportingDocuments = bill1GoodsItem1.SupportingDocuments;
					AssertContainsExactElementsInAnyOrder("billGoodsItemSupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "A001", "departuredoc1"),
																(2, "DEC", "A002", "departuredoc2")
															}, bill1GoodsItem1SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1GoodsItem1TransportDocuments = bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem1TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "B001", "departuretra1"),
																(2, "DEC", "B002", "departuretra2")
															}, bill1GoodsItem1TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var header1TransportDocuments = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
					AssertContainsExactElementsInAnyOrder("header1TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(4, "DEC", "B003", "departuretraHeader1")
															}, header1TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1TransportDocuments = nctsHeader.Bills[0].AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
					AssertContainsExactElementsInAnyOrder("bill1TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(5, "DEC", "B004", "departuretraBill1")
															}, bill1TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1GoodsItem1ReferenceDocuments = bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem1ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "C001", "departureref1"),
																(2, "DEC", "C002", "departureref2")
															}, bill1GoodsItem1ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var headerReferenceDocuments = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
					AssertContainsExactElementsInAnyOrder("headerReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(4, "DEC", "C003", "departurerefHeader1")
															}, headerReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1ReferenceDocuments = nctsHeader.Bills[0].AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
					AssertContainsExactElementsInAnyOrder("bill1ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(5, "DEC", "C004", "departurerefBill1")
															}, bill1ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1GoodsItem2 = bill1GoodsItems.First(x => x.BY_LineNo == 2);
					var bill1GoodsItem2Packages = bill1GoodsItem2.Packages;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
															new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
															{
																(1, "DEC", "AA", 5, "departuremarks1", ZString.Empty, ZString.Empty, ZString.Empty),
																(2, "DEC", "BB", 20, "departuremarks2", ZString.Empty, ZString.Empty, ZString.Empty)
															}, bill1GoodsItem2Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

					var bill1GoodsItem2Package1Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package1ContainersPivots.Container", new[] { containerWithSeq2 }, bill1GoodsItem2Package1Containers);
					var bill1GoodsItem2Package2Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package2ContainersPivots.Container", new[] { containerWithSeq2 }, bill1GoodsItem2Package2Containers);

					var bill1GoodsItem2SupportingDocuments = bill1GoodsItem2.SupportingDocuments;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2SupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "A001", "departuredoc1"),
																(2, "DEC", "A002", "departuredoc2")
															}, bill1GoodsItem2SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1GoodsItem2TransportDocuments = bill1GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "B001", "departuretra1"),
																(2, "DEC", "B002", "departuretra2")
															}, bill1GoodsItem2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var header2TransportDocuments = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
					AssertContainsExactElementsInAnyOrder("header2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(4, "DEC", "B003", "departuretraHeader1")
															}, header2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1_1GoodsItem2TransportDocuments = nctsHeader.Bills[0].AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
					AssertContainsExactElementsInAnyOrder("bill1_1GoodsItem2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(5, "DEC", "B004", "departuretraBill1")
															}, bill1_1GoodsItem2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1GoodsItem2ReferenceDocuments = bill1GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "C001", "departureref1"),
																(2, "DEC", "C002", "departureref2")
															}, bill1GoodsItem2ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var header3ReferenceDocuments = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
					AssertContainsExactElementsInAnyOrder("header3ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(4, "DEC", "C003", "departurerefHeader1")
															}, header3ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1_2GoodsItem2ReferenceDocuments = nctsHeader.Bills[0].AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
					AssertContainsExactElementsInAnyOrder("bill1_2GoodsItem2ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(5, "DEC", "C004", "departurerefBill1")
															}, bill1_2GoodsItem2ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
				});
	}

	[RequiresSTA]
	public void TestLoadDataForUnloadingClick_WithDepartureWithSameMRN_LoadFromDeparture_NoFinalPeriod()
	{
		TestLoadDataForUnloadingClick(
			nctsHeaderDeparture => AddDataToDepartureForUnloading(nctsHeaderDeparture),
			nctsHeaderDeparture =>
				{
					AssertContainsExactElementsInAnyOrder(
						"transportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
						new (ZShort, ZString, ZString, ZString, ZString)[]
						{
							(1, "DEC", "20", "wagon", "GB")
						},
						GetArrivalHeaderTransportInfos(nctsHeader));

					var containers = nctsHeader.ArrivalHeaderContainers;
					AssertContainsExactElementsInAnyOrder("containers (BC_SequenceNumber, BC_UnloadedState, BC_ContainerNum, BC_Mode)",
																new (ZShort, ZString, ZString, ZString)[]
																	{
																		(1, "DEC", "CONT1", "CNT"),
																		(2, "DEC", "CONT2", "CNT")
																	}, containers.Select(x => (x.BC_SequenceNumber, x.BC_UnloadedState, x.BC_ContainerNum, x.BC_Mode)));

					var containerWithSeq1 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
					var container1Seal = containerWithSeq1.Seals[0];
					AssertEquals("Container with seq 1, only seal's BK_SequenceNumber", (ZShort)1, container1Seal.BK_SequenceNumber);
					AssertEquals("Container with seq 1, only seal's BK_UnloadingState", "DEC", container1Seal.BK_UnloadingState);
					AssertEquals("Container with seq 1, only seal's BK_SealNumber", "Seal1", container1Seal.BK_SealNumber);

					var containerWithSeq2 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 2);
					var container2Seals = containerWithSeq2.Seals;
					AssertContainsExactElementsInAnyOrder("container2Seals (BK_SequenceNumber, BK_UnloadingState, BK_SealNumber)",
															new (ZShort, ZString, ZString)[]
															{
																(1, "DEC", "Seal2"),
																(2, "DEC", "Seal3"),
																(3, "DEC", "Seal4")
															}, container2Seals.Select(x => (x.BK_SequenceNumber, x.BK_UnloadingState, x.BK_SealNumber)));

					AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, MovementDetail.B9_UnloadedState, B0_Weight)",
																new (ZString, ZString, ZDecimal, ZString)[]
																{
																	("1", "DEC", 20m, "KG")
																}, nctsHeader.Bills.Select(x => (x.MovementDetail.B9_SeqNo, x.MovementDetail.B9_UnloadedState, x.B0_Weight, x.B0_WeightUQ)));

					var bill1GoodsItems = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1").ArrivalGoodsItems;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
															new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
															{
																(1, 1, "DEC", "QQQ", "88888888", 40, "KG", 10, "DG", "departuredescription"),
																(2, 2, "DEC", "WWW", "99999999", 15, "KL", 11, "L", "departuredescription2")
															}, bill1GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));

					var bill1GoodsItem1 = bill1GoodsItems.First(x => x.BY_LineNo == 1);
					var bill1GoodsItem1Packages = bill1GoodsItem1.Packages;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
															new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
															{
																(1, "DEC", "AA", 5, "departuremarks1", ZString.Empty, ZString.Empty, ZString.Empty),
																(2, "DEC", "BB", 20, "departuremarks2", ZString.Empty, ZString.Empty, ZString.Empty)
															}, bill1GoodsItem1Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

					var bill1GoodsItem1Package1Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Package1ContainersPivots.Container", new[] { containerWithSeq1 }, bill1GoodsItem1Package1Containers);
					var bill1GoodsItem1Package2Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Package2ContainersPivots.Container", new[] { containerWithSeq1 }, bill1GoodsItem1Package2Containers);

					var bill1GoodsItem1SupportingDocuments = bill1GoodsItem1.SupportingDocuments;
					AssertContainsExactElementsInAnyOrder("billGoodsItemSupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "A001", "departuredoc1"),
																(2, "DEC", "A002", "departuredoc2")
															}, bill1GoodsItem1SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1GoodsItem1TransportDocuments = bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem1TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "B001", "departuretra1"),
																(2, "DEC", "B002", "departuretra2"),
																(4, "DEC", "B003", "departuretraHeader1"),
																(5, "DEC", "B004", "departuretraBill1")
															}, bill1GoodsItem1TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1GoodsItem1ReferenceDocuments = bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem1ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "C001", "departureref1"),
																(2, "DEC", "C002", "departureref2"),
																(4, "DEC", "C003", "departurerefHeader1"),
																(5, "DEC", "C004", "departurerefBill1")
															}, bill1GoodsItem1ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1GoodsItem2 = bill1GoodsItems.First(x => x.BY_LineNo == 2);
					var bill1GoodsItem2Packages = bill1GoodsItem2.Packages;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)",
															new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
															{
																(1, "DEC", "AA", 5, "departuremarks1", ZString.Empty, ZString.Empty, ZString.Empty),
																(2, "DEC", "BB", 20, "departuremarks2", ZString.Empty, ZString.Empty, ZString.Empty)
															}, bill1GoodsItem2Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

					var bill1GoodsItem2Package1Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package1ContainersPivots.Container", new[] { containerWithSeq2 }, bill1GoodsItem2Package1Containers);
					var bill1GoodsItem2Package2Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package2ContainersPivots.Container", new[] { containerWithSeq2 }, bill1GoodsItem2Package2Containers);

					var bill1GoodsItem2SupportingDocuments = bill1GoodsItem2.SupportingDocuments;
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2SupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "A001", "departuredoc1"),
																(2, "DEC", "A002", "departuredoc2")
															}, bill1GoodsItem2SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1GoodsItem2TransportDocuments = bill1GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "B001", "departuretra1"),
																(2, "DEC", "B002", "departuretra2"),
																(4, "DEC", "B003", "departuretraHeader1"),
																(5, "DEC", "B004", "departuretraBill1")
															}, bill1GoodsItem2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					var bill1GoodsItem2ReferenceDocuments = bill1GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
					AssertContainsExactElementsInAnyOrder("bill1GoodsItem2ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
																(1, "DEC", "C001", "departureref1"),
																(2, "DEC", "C002", "departureref2"),
																(4, "DEC", "C003", "departurerefHeader1"),
																(5, "DEC", "C004", "departurerefBill1")
															}, bill1GoodsItem2ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
				},
			useNC5TP: true);
	}

	public void TestLoadDataForUnloadingClick_WithDepartureHouseConsignmentTransportInfos_LoadTransportInfosFromDepartureHouseConsignment()
	{
		TestLoadDataForUnloadingClick(
			nctsHeaderDeparture => AddDataToDepartureForUnloading(nctsHeaderDeparture, addConsignmentTransportDetails: true, addHouseConsignmentTransportDetails: true),
			nctsHeaderDeparture =>
			{
				AssertEquals(
					"Arrival Consignment transportInfos not filled from Departure Consignment when Departure House Consignment transportInfos exist",
					0,
					GetArrivalHeaderTransportInfos(nctsHeader).Count());

				AssertContainsExactElementsInAnyOrder(
					"Arrival House Consignment transportInfos filled from Departure House Consignment",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "20", "b-wagon", "DE"),
					},
					GetArrivalBillTransportInfos(nctsHeader.Bills[0]));
			});
	}

	[RequiresSTA]
	public void TestLoadDataForUnloadingClick_WithNoDepartureHouseConsignmentTransportInfos_LoadTransportInfosFromDepartureConsignment()
	{
		TestLoadDataForUnloadingClick(
			nctsHeaderDeparture => AddDataToDepartureForUnloading(nctsHeaderDeparture, addConsignmentTransportDetails: true, addHouseConsignmentTransportDetails: false),
			nctsHeaderDeparture =>
			{
				AssertContainsExactElementsInAnyOrder(
					"Arrival Consignment transportInfos filled from Departure Consignment",
					new (ZShort, ZString, ZString, ZString, ZString)[]
					{
						(1, "DEC", "20", "wagon", "GB"),
					},
					GetArrivalHeaderTransportInfos(nctsHeader));

				AssertEquals(
					"Arrival House Consignment transportInfos not filled from Departure Consignment",
					0,
					GetArrivalBillTransportInfos(nctsHeader.Bills[0]).Count());
			});
	}

	void TestLoadDataForUnloadingClick(Action<NctsHeader> departureMovementSetup, Action<NctsHeader> arrivalMovementAssertions, bool useNC5TP = false)
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			CreateEntryNumber(nctsHeader);

			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			CreateEntryNumber(nctsHeaderDeparture);

			departureMovementSetup(nctsHeaderDeparture);

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var nctsMessagingMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Spain, new TestObjectHandle(new Phase5MessagingMenuProviderForTest(nctsHeader, false, false, departureSelected: true)) }
			};

			using (ObjectFactory.Substitute("NCTSMessagingMenuProviders", nctsMessagingMenuProviders))
			using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: useNC5TP))
			{
				nctsMovementForm.Show();
				var loadDataMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Load Data for Unloading");

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					loadDataMenuItem.PerformClick();

					AssertEquals("Data has been loaded from departure correctly", "Data loaded correctly. Please, save the job before changing any value.", UnitTestUserNotification.Instance.LastMessage.Text);

					arrivalMovementAssertions(nctsHeaderDeparture);
				});
			}
		}
	}

	void AddDataToDepartureForUnloading(
		NctsHeader nctsHeader,
		bool addConsignmentTransportDetails = true,
		bool addHouseConsignmentTransportDetails = false)
	{
		if (addConsignmentTransportDetails)
		{
			var departureMovement = nctsHeader.MovementHeader;
			departureMovement.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			departureMovement.BM_TransportAtDepartureType = "20";
			departureMovement.BM_TransportAtDeparture = "wagon";
			departureMovement.BM_RN_NKTransportAtDepartureCountry = "GB";
		}

		var cont1 = nctsHeader.DepartureHeaderContainers.AddNew();
		cont1.BC_Mode = "CNT";
		cont1.BC_ContainerNum = "CONT1";
		cont1.Seal1 = "Seal1";
		var cont2 = nctsHeader.DepartureHeaderContainers.AddNew();
		cont2.BC_Mode = "CNT";
		cont2.BC_ContainerNum = "CONT2";
		cont2.Seal1 = "Seal2";
		cont2.Seal2 = "Seal3";
		cont2.AdditionalSeals.AddNew().BK_SealNumber = "Seal4";

		var bill = nctsHeader.Bills.AddNew();
		bill.B0_Weight = 20m;
		bill.B0_WeightUQ = "KG";

		if (addHouseConsignmentTransportDetails)
		{
			bill.TransportTypeAtDeparture = "20";
			bill.TransportAtDeparture = "b-wagon";
			bill.TransportCountryAtDeparture = "DE";
		}

		var goodsItem1 = bill.GoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		goodsItem1.BY_DeclarationGoodsItemNumber = 1;
		goodsItem1.BY_Description = "departuredescription";
		goodsItem1.BY_CusC4Number = "QQQ";
		goodsItem1.BY_HarmonisedTariff = "88888888";
		goodsItem1.BY_GrossWeight = 20;
		goodsItem1.BY_GrossWeightUnit = "KG";
		goodsItem1.BY_NetWeight = 10;
		goodsItem1.BY_NetWeightUnit = "DG";
		AddPackagesToGoodsItemDeparture(goodsItem1, cont1);
		AddSupportingDocumentsToGoodsItemDeparture(goodsItem1);
		AddTransportDocumentsToGoodsItemDeparture(goodsItem1);
		AddReferenceDocumentsToGoodsItemDeparture(goodsItem1);
		AddPreviousDocumentToGoodsItemDeparture(goodsItem1, 40);

		var goodsItem2 = bill.GoodsItems.AddNew();
		goodsItem2.BY_LineNo = 2;
		goodsItem2.BY_DeclarationGoodsItemNumber = 2;
		goodsItem2.BY_Description = "departuredescription2";
		goodsItem2.BY_CusC4Number = "WWW";
		goodsItem2.BY_HarmonisedTariff = "99999999";
		goodsItem2.BY_GrossWeight = 15;
		goodsItem2.BY_GrossWeightUnit = "KL";
		goodsItem2.BY_NetWeight = 11;
		goodsItem2.BY_NetWeightUnit = "L";
		AddPackagesToGoodsItemDeparture(goodsItem2, cont2);
		AddSupportingDocumentsToGoodsItemDeparture(goodsItem2);
		AddTransportDocumentsToGoodsItemDeparture(goodsItem2);
		AddReferenceDocumentsToGoodsItemDeparture(goodsItem2);
		AddPreviousDocumentToGoodsItemDeparture(goodsItem2, 100, "ZZZZ");

		var supDoc1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
		supDoc1.CSI_Code = "A003";
		supDoc1.CSI_ReferenceNumber = "departuredocHeader1";

		var supDoc2 = bill.SupportingDocuments.AddNew();
		supDoc2.CSI_Code = "A004";
		supDoc2.CSI_ReferenceNumber = "departuredocBill1";

		supDoc1.CSI_LineNo = 4;
		supDoc2.CSI_LineNo = 5;

		var transpDoc1 = nctsHeader.AdditionalDocuments.AddNew();
		transpDoc1.CSI_Code = "B003";
		transpDoc1.CSI_SubType = "TRA";
		transpDoc1.CSI_ReferenceNumber = "departuretraHeader1";

		var transpDoc2 = bill.AdditionalDocuments.AddNew();
		transpDoc2.CSI_Code = "B004";
		transpDoc2.CSI_SubType = "TRA";
		transpDoc2.CSI_ReferenceNumber = "departuretraBill1";

		transpDoc1.CSI_LineNo = 4;
		transpDoc2.CSI_LineNo = 5;

		var refDoc1 = nctsHeader.AdditionalDocuments.AddNew();
		refDoc1.CSI_Code = "C003";
		refDoc1.CSI_SubType = "REF";
		refDoc1.CSI_ReferenceNumber = "departurerefHeader1";

		var refDoc2 = bill.AdditionalDocuments.AddNew();
		refDoc2.CSI_Code = "C004";
		refDoc2.CSI_SubType = "REF";
		refDoc2.CSI_ReferenceNumber = "departurerefBill1";

		refDoc1.CSI_LineNo = 4;
		refDoc2.CSI_LineNo = 5;

		void AddPackagesToGoodsItemDeparture(NctsDepartureCargoDesc goodsItem, NctsDepartureHeaderContainer container)
		{
			goodsItem.IsVehicles = false;

			var pack1 = goodsItem.Packages.AddNew();
			pack1.B5_SequenceNumber = 1;
			pack1.B5_UnitType = "AA";
			pack1.B5_UnitCount = 5;
			pack1.B5_MarksAndNumbers = "departuremarks1";
			pack1.ContainersPivot.AddPivotFor(container);

			var pack2 = goodsItem.Packages.AddNew();
			pack2.B5_SequenceNumber = 2;
			pack2.B5_UnitType = "BB";
			pack2.B5_UnitCount = 20;
			pack2.B5_MarksAndNumbers = "departuremarks2";
			pack2.ContainersPivot.AddPivotFor(container);
		}

		void AddSupportingDocumentsToGoodsItemDeparture(NctsDepartureCargoDesc goodsItem)
		{
			var supDoc1 = goodsItem.SupportingDocuments.AddNew();
			supDoc1.CSI_LineNo = 1;
			supDoc1.CSI_Code = "A001";
			supDoc1.CSI_ReferenceNumber = "departuredoc1";

			var supDoc2 = goodsItem.SupportingDocuments.AddNew();
			supDoc2.CSI_LineNo = 2;
			supDoc2.CSI_Code = "A002";
			supDoc2.CSI_ReferenceNumber = "departuredoc2";
		}

		void AddTransportDocumentsToGoodsItemDeparture(NctsDepartureCargoDesc goodsItem)
		{
			var transpDoc1 = goodsItem.AdditionalInfos.AddNew();
			transpDoc1.CSI_Code = "B001";
			transpDoc1.CSI_SubType = "TRA";
			transpDoc1.CSI_ReferenceNumber = "departuretra1";
			transpDoc1.CSI_LineNo = 1;

			var transpDoc2 = goodsItem.AdditionalInfos.AddNew();
			transpDoc2.CSI_Code = "B002";
			transpDoc2.CSI_SubType = "TRA";
			transpDoc2.CSI_ReferenceNumber = "departuretra2";
			transpDoc2.CSI_LineNo = 2;
		}

		void AddReferenceDocumentsToGoodsItemDeparture(NctsDepartureCargoDesc goodsItem)
		{
			var refDoc1 = goodsItem.AdditionalInfos.AddNew();
			refDoc1.CSI_Code = "C001";
			refDoc1.CSI_SubType = "REF";
			refDoc1.CSI_ReferenceNumber = "departureref1";

			var refDoc2 = goodsItem.AdditionalInfos.AddNew();
			refDoc2.CSI_Code = "C002";
			refDoc2.CSI_SubType = "REF";
			refDoc2.CSI_ReferenceNumber = "departureref2";

			refDoc1.CSI_LineNo = 1;
			refDoc2.CSI_LineNo = 2;
		}

		void AddPreviousDocumentToGoodsItemDeparture(NctsDepartureCargoDesc goodsItem, ZDecimal quantity, string code = "N337", string unit = "KGM")
		{
			var prevDoc = goodsItem.PreviousDocuments.AddNew();
			prevDoc.CSI_Code = code;
			prevDoc.CSI_Quantity = quantity;
			prevDoc.CSI_UnitOfQuantity = unit;
		}
	}

	public void TestLoadDataForUnloadingClick_WithDepartureWithSameMRN_LoadFromTQU()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			CreateEntryNumber(nctsHeader);
			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			CreateEntryNumber(nctsHeaderDeparture);

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var nctsMessagingMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Spain, new TestObjectHandle(new Phase5MessagingMenuProviderForTest(nctsHeader, false, false, departureSelected: false)) }
			};

			using (ObjectFactory.Substitute("NCTSMessagingMenuProviders", nctsMessagingMenuProviders))
			using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var loadDataMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Load Data for Unloading");

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					loadDataMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BH_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.ArrivalMovementHeader.BM_MessageStatus);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.TransitNcts5Query, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestLoadDataForUnloadingClick_WithoutDepartureWithSameMRN()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			CreateEntryNumber(nctsHeader);
			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			UnitTestUserNotification.Instance.AddYesAnswer();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var loadDataMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Load Data for Unloading");

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					loadDataMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to send a TQU, should not be last message when answer is Yes", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSendTQUMessagePopUpText));
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BH_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.ArrivalMovementHeader.BM_MessageStatus);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.TransitNcts5Query, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Send TNN

	[RequiresSTA]
	public void TestSendToCustomsCore_TNN()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			CreateEntryNumber(nctsHeader);
			nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
			var nctsHeaderTNN = Factory.New<NctsHeader>();
			nctsHeaderTNN.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderTNN.ESNctsHeader.CEN_TNNArrival = true;
			CreateEntryNumber(nctsHeaderTNN);
			var tnnMovement = nctsHeaderTNN.MovementHeader;
			tnnMovement.BM_Phase = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

			nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = tnnMovement.PK;

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration))
			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsArrivalMovementForm);

				var tnnTabPage = nctsArrivalMovementForm.FindSingle<ZTabPage>("TNNTabPage");
				AssertNotNull("TNNTabPage is added to the main control when TNN exist", tnnTabPage);

				var declarationTypeDropEdit = FindDeclarationTypeDropEdit(tnnTabPage);
				AssertNotNull("declarationTypeDropEdit is not null", declarationTypeDropEdit);
				AssertEquals("Expected not ReadOnly declarationTypeDropEdit", false, declarationTypeDropEdit.ReadOnly);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.ArrivalMovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is TNN", ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData, nctsHeader.ArrivalMovementHeader.BM_Phase);
					AssertEquals("Expected ReadOnly declarationTypeDropEdit", true, declarationTypeDropEdit.ReadOnly);

					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Send Unloading Remarks

	public void TestSendToCustomsCore_UnloadingRemarks()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMrnFromUser = GoodsDescription;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			CreateEntryNumber(nctsHeader);

			nctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods, shouldEdit: true))
			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsArrivalMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.ArrivalMovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 044", ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks, nctsHeader.ArrivalMovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);
				});
			}
		}
	}

	#endregion

	#region Download TAD

	public void TestDownloadTAD_Validations()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			CreateEntryNumber(nctsHeader);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var downloadTADMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Download TAD (Transit Accompanying Document)");

				CombineAssertions(() =>
				{
					downloadTADMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after broker not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
					nctsHeader.BH_CustomsProfile = ZString.Empty;
					nctsHeader.Factory.Save();
					downloadTADMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate is empty", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert not declared", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.BH_CustomsProfile = "INVALID";
					nctsHeader.Factory.Save();
					downloadTADMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but certificate declared is invalid", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared invalid", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
					nctsHeader.Factory.Save();
					downloadTADMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared after cert declared not authorized", UnitTestUserNotification.Instance.LastMessage.Text);
					nctsHeader.Reload();
					downloadTADMenuItem.PerformClick();
					AssertEquals("Message informing representative and certificate are needed when representative is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", WrongBrokerOrCertificatePopUpText, UnitTestUserNotification.Instance.LastMessage.Text);

					using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertNull("User Notification Last Message was cleared after cert declared not authorized without save", UnitTestUserNotification.Instance.LastMessage.Text);
						nctsHeader.Reload();
						downloadTADMenuItem.PerformClick();
						AssertEquals("Message informing csv clearance need to be declared to be able to send a document request", "Cannot download TAD when Clearance Number is empty.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				});
			}
		}
	}

	public void TestDownloadTAD_PreSave()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
		nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
		CreateEntryNumber(nctsHeader);

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var downloadTADMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Download TAD (Transit Accompanying Document)");

			CombineAssertions(() =>
			{
				downloadTADMenuItem.PerformClick();
				AssertEquals("Should have message asking to save the declaration before Download TAD (Transit Accompanying Document)", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	[RequiresSTA]
	public void TestDownloadTAD_Click_NoDocumentNeeded()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "20ES00999830001277";
			nctsHeader.ClearanceEntryNumber.CE_EntryNum = "ABCDEFGHIJKLMNOP";

			var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], "20ES00999830001277_NCTS_AEAT_DAT.pdf", "CLR");
			nctsHeader.Factory.Save();
			docManagerInfo.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var downloadTADMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Download TAD (Transit Accompanying Document)");

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					downloadTADMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("All Documents for AEAT already exist for the entry so nothing will be sent", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertNull("EDIMessage was not created for the ncts header", nctsHeader.Messages.LastOutgoingMessage);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestDownloadTAD_Click_AllDocumentsNeeded()
	{
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
			nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "20ES00999830001277";
			nctsHeader.ClearanceEntryNumber.CE_EntryNum = "ABCDEFGHIJKLMNOP";

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var downloadTADMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Download TAD (Transit Accompanying Document)");

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					downloadTADMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "1 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.EsDocumentRequest, msg.EM_MessageType);
				});
			}
		}
	}

	#endregion

	#region Into Temporary Storage

	public void TestIntoTemporaryStorageClick()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			SetUpTariffAndRates();

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.BH_JobReference = "NCT00000001";
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;
			nctsHeader.SummaryEntryNumber.CE_EntryNum = "99982000174";
			nctsHeader.SummaryEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 02, 02, 01, 00);
			nctsHeader.MovementReferenceEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			arrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2022, 02, 15, 16, 43, 27);
			arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
			arrivalMovementHeader.BM_NoChangesToReport = false;
			var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = "ArrivalLoc";
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;
			var provider = premises.NumberProvider;
			_ = provider.CustomsNumbers.AddNew();
			var wrapper1 = provider.CustomsNumberWrappers[0];
			wrapper1.IsActive = true;
			wrapper1.SN_MinimumValue = 1;
			wrapper1.SN_Count = 1;

			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
			cusPermitHeader.CPH_Number = "GUARANTEEREF";
			cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
			cusPermitHeader.CPH_Type = "TST";
			cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
			var guarantee = arrivalMovementHeader.GuaranteesForArrival.AddNew();
			guarantee.PW_BondNumber = "GUARANTEEREF";
			guarantee.PW_RX_NKCurrency = "USD";
			var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = "OBL";
			transaction.CPL_Reference = "REF";

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.MovementDetail.B9_SeqNo = "1";
			bill1.MovementDetail.B9_UnloadedState = "NEW";

			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem1.SetValues("NEW", 1, 1, "0304798000", "CUSCODE1", "Description1", 2);

			var pack1 = goodsItem1.Packages.AddNew();
			pack1.SetValues("DEC", "BX", "marks1", 5, 2);

			var bill2 = nctsHeader.Bills.AddNew();
			bill2.MovementDetail.B9_SeqNo = "2";
			bill2.MovementDetail.B9_UnloadedState = "DIF";

			var goodsItem2 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem2.SetValues("DIF", 2, 4, "222222", "CUSCODE2", "Description2", 5, "0304798000", "CUSCODE2DIF", "Description2DIF", 10);

			var pack2 = goodsItem2.Packages.AddNew();
			pack2.SetValues("NEW", "FR", "marks4", 5, 0, vin: "VINCODE4", brand: "BRAND4", model: "MODEL4");

			var bill3 = nctsHeader.Bills.AddNew();
			bill3.MovementDetail.B9_SeqNo = "3";
			bill3.MovementDetail.B9_UnloadedState = "MIS";

			var goodsItem3 = bill3.ArrivalGoodsItems.AddNew();
			goodsItem3.SetValues("NEW", 5, 7, "555555", "CUSCODE5", "Description5", 2);

			var pack3 = goodsItem3.Packages.AddNew();
			pack3.SetValues("NEW", "FR", "marks9", 5, 2, vin: "VINCODE9", model: "MODEL9");

			var bill4 = nctsHeader.Bills.AddNew();
			bill4.MovementDetail.B9_SeqNo = "4";
			bill4.MovementDetail.B9_UnloadedState = "DEC";

			var goodsItem4 = bill4.ArrivalGoodsItems.AddNew();
			goodsItem4.SetValues("DEC", 1, 2, "444444", "CUSCODE4", "Description4", 0);

			var pack4 = goodsItem4.Packages.AddNew();
			pack4.SetValues("DEC", "CT", "marks4", 4, 3);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var menuItem = (ZMenuItem)nctsArrivalMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

				var actualGoodsItems = nctsHeader.GetGoodsItems().ToArray();
				AssertEquals("[PRE-CONDITION] GoodsItems Count", 4, actualGoodsItems.Length);
				AssertEquals("[PRE-CONDITION] GoodsItems[0] LiabilityAmount", 148.6m, actualGoodsItems[0].LiabilityAmount);
				AssertEquals("[PRE-CONDITION] GoodsItems[1] LiabilityAmount", 148.6m, actualGoodsItems[1].LiabilityAmount);
				AssertEquals("[PRE-CONDITION] GoodsItems[2] LiabilityAmount", 0m, actualGoodsItems[2].LiabilityAmount);
				AssertEquals("[PRE-CONDITION] GoodsItems[3] LiabilityAmount", 0m, actualGoodsItems[3].LiabilityAmount);
				CombineAssertions(() =>
				{
					const string expectedMessage = "Temporary Storage data created successfully.";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", nctsHeader.Factory);
					menuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", nctsHeader.Factory);
					AssertEquals(message: "The data has been created",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
					AssertEquals("BM_Phase value when Temporary Storage data created successfully", ESNcts5ArrivalPhaseList.Codes.TemporaryStorageActivated, nctsHeader.ArrivalMovementHeader.BM_Phase);

					var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, "99982000174");
					var regHeaders = nctsHeader.Factory.Load<CusTempStorageRegHeader>(query);
					AssertEquals("regHeaders created", 1, regHeaders.Length);

					var regHeader = regHeaders[0];
					AssertEquals("regHeader.SRH_AppCode", "ADT", regHeader.SRH_AppCode);
					AssertEquals("regHeader.SRH_Reference", "99982000174", regHeader.SRH_Reference);
					AssertEquals("regHeader.SRH_ArrivalDate", new ZDateTime(2023, 01, 01), regHeader.SRH_ArrivalDate);
					AssertEquals("regHeader.SRH_PresentationDate", new ZDateTime(2023, 01, 01, 02, 01, 00), regHeader.SRH_PresentationDate);
					AssertEquals("regHeader.SRH_PreviousReferenceType", "NCTS5", regHeader.SRH_PreviousReferenceType);
					AssertEquals("regHeader.SRH_PreviousReference", "1234567890", regHeader.SRH_PreviousReference);
					AssertEquals("regHeader.SRH_Status", "OPN", regHeader.SRH_Status);
					AssertEquals("regHeader.SRH_SRP_Premises", premises.PK, regHeader.SRH_SRP_Premises);

					var regHeaderGuarantee = regHeader.Guarantee;
					AssertEquals("regHeaderGuarantee.PW_BondNumber", "GUARANTEEREF", regHeaderGuarantee.PW_BondNumber);
					AssertEquals("regHeaderGuarantee.PW_BondAmount", 297.2m, regHeaderGuarantee.PW_BondAmount);
					AssertEquals("regHeaderGuarantee.PW_RX_NKCurrency", "USD", regHeaderGuarantee.PW_RX_NKCurrency);
					AssertEquals("regHeaderGuarantee.PW_CPH_Guarantee", cusPermitHeader.PK, regHeaderGuarantee.PW_CPH_Guarantee);

					var regLines = regHeader.CusTempStorageRegLines;

					AssertContainsExactElementsInAnyOrder("regLines SRL_LineNumber",
															new ZInt[] { 1, 2, 3 }, regLines.Select(x => x.SRL_LineNumber));
					AssertContainsExactElementsInAnyOrder("regLines (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZString, ZString, ZString, ZString, ZString, ZString, ZDate)[]
															{
															("BX", "marks1", "ESEORI1234", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("FR", "VINCODE4:BRAND4:MODEL4", "ESEORI1234", "KGM", "OPN", "TER", new ZDate(2023, 04, 01)),
															("CT", "marks4", "ESEORI1234", "KGM", "OPN", "TER", new ZDate(2023, 04, 01))
															}, regLines.Select(x => (x.SRL_PackageType, x.SRL_PackageMarks, x.SRL_GoodsOwnerIdentifier, x.SRL_GrossWeightUQ, x.SRL_CustomsStatus, x.SRL_UnionStatus, x.SRL_LimitDate)));

					AssertRegLineWithOnePivot(regLines, "marks1", 2, 5, 148.6m, 99001, "0304798000", "CUSCODE1", "Description1");

					AssertRegLineWithOnePivot(regLines, "VINCODE4:BRAND4:MODEL4", 10, 5, 148.6m, 4, "0304798000", "CUSCODE2DIF", "Description2DIF");

					AssertRegLineWithOnePivot(regLines, "marks4", 3, 4, ZDecimal.Zero, 2, "444444", "CUSCODE4", "Description4");

					var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery());
					AssertEquals("regLineTransactions created", 3, regLineTransactions.Length);

					var regLineItemPivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery());
					AssertEquals("regLineItemPivots created", 3, regLineItemPivots.Length);

					var regLineItems = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>(new ZQuery());
					AssertEquals("regLineItems created", 3, regLineItems.Length);
				});
			}
		}
	}

	void AssertRegLineWithOnePivot(CusTempStorageRegLineCollection regLines, ZString marks, ZDecimal grossWeight, ZInt packageQty, ZDecimal bondAmount, ZInt itemNumber, ZString tariff, ZString cusCode, ZString description)
	{
		var regLine = regLines.First(x => x.SRL_PackageMarks == marks);
		var regLineTransactions = Factory.Load<CusTempStorageRegLineTransaction>(new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_SRL, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Transactions created", 1, regLineTransactions.Length);
		AssertRegLineTransaction("regLine with marks " + marks + " Transactions[0]", regLineTransactions[0], grossWeight, packageQty, bondAmount);
		var regLinePivots = Factory.Load<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>(new ZQuery(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, regLine.PK));
		AssertEquals("regLine with marks " + marks + " Pivots created", 1, regLinePivots.Length);
		AssertRegLineItemPivotAndItem("regLine with marks " + marks + " Pivot1", regLinePivots[0], grossWeight, itemNumber, tariff, cusCode, description);
	}

	void AssertRegLineTransaction(ZString assertMessage, CusTempStorageRegLineTransaction transaction, ZDecimal grossWeight, ZInt packageQty, ZDecimal bondAmount)
	{
		AssertEquals(assertMessage + ".SRT_GrossWeight", grossWeight, transaction.SRT_GrossWeight);
		AssertEquals(assertMessage + ".SRT_PackageQty", packageQty, transaction.SRT_PackageQty);
		AssertEquals(assertMessage + ".SRT_TransactionType", "OBL", transaction.SRT_TransactionType);
		AssertEquals(assertMessage + ".SRT_InternalReferenceNumber", "NCT00000001", transaction.SRT_InternalReferenceNumber);
		AssertEquals(assertMessage + ".SRT_InternalReferenceType", "TRA", transaction.SRT_InternalReferenceType);
		AssertEquals(assertMessage + ".SRT_TransactionDate", new ZDateTimeOffset(2023, 01, 01, 02, 01, 00), transaction.SRT_TransactionDate);
		AssertEquals(assertMessage + ".SRT_PhysicalInOutDate", new ZDateTimeOffset(2022, 02, 15, 16, 43, 27), transaction.SRT_PhysicalInOutDate);
		AssertEquals(assertMessage + ".SRT_BondAmount", bondAmount, transaction.SRT_BondAmount);
	}

	void AssertRegLineItemPivotAndItem(ZString assertMessage, EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot pivot, ZDecimal grossWeight, ZInt itemNumber, ZString tariff, ZString cusCode, ZString description)
	{
		AssertEquals(assertMessage + ".SRV_GrossWeight", grossWeight, pivot.SRV_GrossWeight);
		var regLineItem = pivot.RegLineItem;
		AssertEquals(assertMessage + ".RegLineItem.SRI_GoodsItemNumber", itemNumber, regLineItem.SRI_GoodsItemNumber);
		AssertEquals(assertMessage + ".RegLineItem.SRI_Tariff", tariff, regLineItem.SRI_Tariff);
		AssertEquals(assertMessage + ".RegLineItem.SRI_CusC4Number", cusCode, regLineItem.SRI_CusC4Number);
		AssertEquals(assertMessage + ".RegLineItem.SRI_GoodsDescription", description, regLineItem.SRI_GoodsDescription);
	}

	public void TestIntoTemporaryStorage()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var houseConsignment = nctsHeader.Bills.AddNew();
		var goodsItem1 = houseConsignment.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 1;
		var package1 = goodsItem1.Packages.AddNew();
		package1.B5_GrossWeight = 1;
		var package2 = goodsItem1.Packages.AddNew();
		package2.B5_GrossWeight = 0;
		nctsHeader.SummaryEntryNumber.CE_EntryNum = "99982000AH3";

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var guarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_Override = true;
		guarantee.PW_BondAmount = 0;
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessage = "Temporary Storage data created successfully.";
				ZFormModaliser.ShowDialogsInTest = false;
				UnitTestUserNotification.Instance.AddYesAnswer();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Nothing done when response is Yes for the first warning (BondAmount 0)",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
				AssertEquals("BM_Phase value when Temporary Storage data not created (for first warning)", ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks, nctsHeader.ArrivalMovementHeader.BM_Phase);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items (for first warning)", UnitTestUserNotification.Instance.LastMessage.Text);

				guarantee.PW_BondAmount = 20;
				UnitTestUserNotification.Instance.AddYesAnswer();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Nothing done when response is Yes for the second warning (Packages with GrossWeight 0)",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
				AssertEquals("BM_Phase value when Temporary Storage data not created (for second warning)", ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks, nctsHeader.ArrivalMovementHeader.BM_Phase);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items (for second warning)", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Functionality is done correctly when response is No for second warning",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
				AssertEquals("BM_Phase value when Temporary Storage data created successfully", ESNcts5ArrivalPhaseList.Codes.TemporaryStorageActivated, nctsHeader.ArrivalMovementHeader.BM_Phase);
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_WarningsWithAnswerNeeded()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var houseConsignment = nctsHeader.Bills.AddNew();
		var goodsItem1 = houseConsignment.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 2;
		var package1 = goodsItem1.Packages.AddNew();
		package1.B5_GrossWeight = 1;
		var package2 = goodsItem1.Packages.AddNew();
		package2.B5_GrossWeight = 0;
		var package3 = goodsItem1.Packages.AddNew();
		package3.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;
		package3.B5_GrossWeight = 0;
		nctsHeader.SummaryEntryNumber.CE_EntryNum = "99982000AH3";

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var guarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_Override = true;
		guarantee.PW_BondAmount = 0;
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessageTSCreated = "Temporary Storage data created successfully.";
				const string expectedMessageBondAmountError = @"If liability amount is 0, no guarantee transaction will be created for the goods that enter the Temporary Storage.
Would you like to cancel this action to check if the data needed to calculate the liability amount has been entered?
(if data is filled and result is 0, please ignore this warning message and press No)";
				const string expectedMessageGrossWeightError = @"There are some package lines with gross mass empty. For these packages, the Goods Item’s gross weight will be automatically apportioned on the Temporary Storage Register.
This action cannot be undone after introducing goods into the Temporary Storage.
Would you like to cancel this action and enter the gross weight for each package line?";

				UnitTestUserNotification.Instance.AddYesAnswer();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Nothing done when response is Yes for the first warning (BondAmount 0)",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageTSCreated));
				AssertEquals(message: "Guatentee's BondAmount is 0",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageBondAmountError));
				AssertEquals(message: "More than 1 package with 1 gross weight empty but no error is shown because we already have the BondAmount error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageGrossWeightError));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items (for first warning)", UnitTestUserNotification.Instance.LastMessage.Text);

				guarantee.PW_BondAmount = 20;
				UnitTestUserNotification.Instance.AddYesAnswer();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Nothing done when response is Yes for the second warning since BondAmount is not 0 (Packages with GrossWeight 0)",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageTSCreated));
				AssertEquals(message: "Guatentee's BondAmount is not 0",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageBondAmountError));
				AssertEquals(message: "More than 1 package with 1 gross weight empty",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageGrossWeightError));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items (for second warning)", UnitTestUserNotification.Instance.LastMessage.Text);

				package2.B5_GrossWeight = 1;
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Functionality is done correctly when there are no warnings",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageTSCreated));
				AssertEquals(message: "Guatentee's BondAmount is not 0",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageBondAmountError));
				AssertEquals(message: "More than 1 package and all of them without gross weight empty",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessageGrossWeightError));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_GoodsItemsDoesNotContainPackageWithGrossMassEmpty()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var houseConsignment = nctsHeader.Bills.AddNew();
		var goodsItem1 = houseConsignment.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 2;
		var package1 = goodsItem1.Packages.AddNew();
		package1.B5_GrossWeight = 1;
		var package2 = goodsItem1.Packages.AddNew();
		package2.B5_GrossWeight = 0;
		var package3 = goodsItem1.Packages.AddNew();
		package3.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;
		package3.B5_GrossWeight = 0;
		nctsHeader.SummaryEntryNumber.CE_EntryNum = "99982000AH3";

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var guarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_Override = true;
		guarantee.PW_BondAmount = 20;
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = @"There are some package lines with gross mass empty. For these packages, the Goods Item’s gross weight will be automatically apportioned on the Temporary Storage Register.
This action cannot be undone after introducing goods into the Temporary Storage.
Would you like to cancel this action and enter the gross weight for each package line?";
				AssertEquals(message: "More than 1 package with 1 gross weight empty",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				package2.B5_GrossWeight = 1;

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "More than 1 package and all of them without gross weight empty",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_GuaranteeLiabilityAmountIsZero()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var houseConsignment = nctsHeader.Bills.AddNew();
		var goodsItem1 = houseConsignment.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 2;
		var package1 = goodsItem1.Packages.AddNew();
		package1.B5_GrossWeight = 1;
		var package2 = goodsItem1.Packages.AddNew();
		package2.B5_GrossWeight = 0;
		var package3 = goodsItem1.Packages.AddNew();
		package3.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;
		package3.B5_GrossWeight = 0;
		nctsHeader.SummaryEntryNumber.CE_EntryNum = "99982000AH3";

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var guarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_Override = true;
		guarantee.PW_BondAmount = 0;
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessage = @"If liability amount is 0, no guarantee transaction will be created for the goods that enter the Temporary Storage.
Would you like to cancel this action to check if the data needed to calculate the liability amount has been entered?
(if data is filled and result is 0, please ignore this warning message and press No)";

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guatentee's BondAmount is 0",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				guarantee.PW_BondAmount = 20;

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guatentee's BondAmount is not 0",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_GrossMassCheckForPackages()
	{
		Factory.SetBulkTypeHelper();

		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
		var guarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_Override = true;
		guarantee.PW_BondAmount = 20;
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";

		var houseConsignment = nctsHeader.Bills.AddNew();
		var goodsItem1 = houseConsignment.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 20;
		var package1 = goodsItem1.Packages.AddNew();
		package1.SetValues(NctsUnloadedStateList.Codes.NEW, "1D", "Mark1", 2, 0);
		var packageBulk1 = goodsItem1.Packages.AddNew();
		packageBulk1.SetValues(NctsUnloadedStateList.Codes.NEW, "VG", "Mark1", 2, 0);

		var package2 = goodsItem1.Packages.AddNew();
		package2.SetValues(NctsUnloadedStateList.Codes.MIS, "1D", "Mark1", 2, 0);
		nctsHeader.SummaryEntryNumber.CE_EntryNum = "99982000AH3";

		var goodsItem2 = houseConsignment.ArrivalGoodsItems.AddNew();
		var package3 = goodsItem2.Packages.AddNew();
		package3.SetValues(NctsUnloadedStateList.Codes.MIS, "1D", "Mark1", 2, 20);

		var package4 = goodsItem2.Packages.AddNew();
		package4.SetValues(NctsUnloadedStateList.Codes.NEW, "1D", "Mark1", 0, 2);

		var packageBulk2 = goodsItem2.Packages.AddNew();
		packageBulk2.SetValues(NctsUnloadedStateList.Codes.NEW, "VG", "Mark1", 0, 2);

		var package5 = goodsItem2.Packages.AddNew();
		package5.SetValues(NctsUnloadedStateList.Codes.NEW, "FR", "Mark1", 0, 20);
		package5.B5_Model = "M";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessagePackages = "When the same packages are used for different goods, real gross mass should be entered in all related package lines.";
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Item with any Package with Gross Weight = 0 AND (Another Package Line in other Goods Item with same type and marks and unit count empty)",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessagePackages));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Packages", UnitTestUserNotification.Instance.LastMessage.Text);

				goodsItem2.BY_GrossWeight = 5;
				goodsItem2.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Item with no Weight 0 with any Package with Gross Weight = 0 AND (Another Package Line in other Goods Item with same type and marks and unit count empty)",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessagePackages));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Packages", UnitTestUserNotification.Instance.LastMessage.Text);

				goodsItem1.BY_GrossWeight = 0;
				goodsItem1.Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Item with any Weight Not Zero with any Package with Gross Weight = 0 AND (Goods items Gross Mass = 0)",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessagePackages));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_GrossMassCheckForPackages_WithoutErrorWhenAnotherPack()
	{
		Factory.SetBulkTypeHelper();

		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		var guarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var houseConsignment = nctsHeader.Bills.AddNew();
		var goodsItem1 = houseConsignment.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 20;
		var package1 = goodsItem1.Packages.AddNew();
		package1.SetValues(NctsUnloadedStateList.Codes.NEW, "1D", "Mark1", 2, 0);
		var packageBulk1 = goodsItem1.Packages.AddNew();
		packageBulk1.SetValues(NctsUnloadedStateList.Codes.NEW, "VG", "Mark1", 2, 0);

		var package2 = goodsItem1.Packages.AddNew();
		package2.SetValues(NctsUnloadedStateList.Codes.MIS, "1D", "Mark1", 2, 0);
		nctsHeader.SummaryEntryNumber.CE_EntryNum = "99982000AH3";

		var goodsItem2 = houseConsignment.ArrivalGoodsItems.AddNew();
		var package3 = goodsItem2.Packages.AddNew();
		package3.SetValues(NctsUnloadedStateList.Codes.MIS, "1D", "Mark1", 2, 20);

		var package4 = goodsItem2.Packages.AddNew();
		package4.SetValues(NctsUnloadedStateList.Codes.NEW, "1D", "Mark1", 0, 2);

		var packageBulk2 = goodsItem2.Packages.AddNew();
		packageBulk2.SetValues(NctsUnloadedStateList.Codes.NEW, "VG", "Mark1", 0, 2);

		var package5 = goodsItem2.Packages.AddNew();
		package5.SetValues(NctsUnloadedStateList.Codes.NEW, "FR", "Mark1", 0, 20);
		package5.B5_Model = "M";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessagePackages = "When the same packages are used for different goods, real gross mass should be entered in all related package lines.";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				package4.B5_UnitType = "1B";
				package4.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Item with any Package with Gross Weight = 0 AND (Another Package Line in other Goods Item with DIFFERENT type and marks and unit count empty)",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessagePackages));

				package4.B5_UnitType = "1D";
				package4.B5_UnitCount = 2;
				package4.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Item with any Package with Gross Weight = 0 AND (Another Package Line in other Goods Item with same type and marks and unit count not empty)",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessagePackages));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_GrossMassCheckForPackages_WithoutErrorWhenItemMassNotZero()
	{
		Factory.SetBulkTypeHelper();

		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		var guarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_BondAmount = 20;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var houseConsignment = nctsHeader.Bills.AddNew();
		var goodsItem1 = houseConsignment.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 20;
		var package1 = goodsItem1.Packages.AddNew();
		package1.SetValues(NctsUnloadedStateList.Codes.NEW, "1D", "Mark1", 2, 0);
		var packageBulk1 = goodsItem1.Packages.AddNew();
		packageBulk1.SetValues(NctsUnloadedStateList.Codes.NEW, "VG", "Mark1", 2, 0);

		var package2 = goodsItem1.Packages.AddNew();
		package2.SetValues(NctsUnloadedStateList.Codes.MIS, "1D", "Mark1", 2, 0);
		nctsHeader.SummaryEntryNumber.CE_EntryNum = "99982000AH3";

		var goodsItem2 = houseConsignment.ArrivalGoodsItems.AddNew();
		var package3 = goodsItem2.Packages.AddNew();
		package3.SetValues(NctsUnloadedStateList.Codes.MIS, "1D", "Mark1", 2, 20);

		var package4 = goodsItem2.Packages.AddNew();
		package4.SetValues(NctsUnloadedStateList.Codes.NEW, "1D", "Mark1", 0, 2);

		var packageBulk2 = goodsItem2.Packages.AddNew();
		packageBulk2.SetValues(NctsUnloadedStateList.Codes.NEW, "VG", "Mark1", 0, 2);

		var package5 = goodsItem2.Packages.AddNew();
		package5.SetValues(NctsUnloadedStateList.Codes.NEW, "FR", "Mark1", 0, 20);
		package5.B5_Model = "M";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				const string expectedMessagePackages = "When the same packages are used for different goods, real gross mass should be entered in all related package lines.";

				package4.B5_UnitType = "1D";
				package4.B5_UnitCount = 2;
				goodsItem1.Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Item with any Package with Gross Weight = 0 AND (Goods items Gross Mass != 0)",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessagePackages));
			});
		}
	}

	public void TestIntoTemporaryStorage_AtLeastOnePackageWithStatusNotMIS()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
		cusPermitHeader.CPH_Number = "GUARANTEEREF";
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
		cusPermitHeader.CPH_Type = "TST";
		cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
		var guarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_Override = true;
		guarantee.PW_BondAmount = 20;
		var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
		transaction.CPL_TransactionType = "OBL";
		transaction.CPL_Reference = "REF";

		var houseConsignment = nctsHeader.Bills.AddNew();
		houseConsignment.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;
		var goodsItem1 = houseConsignment.ArrivalGoodsItems.AddNew();
		goodsItem1.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;
		var package = goodsItem1.Packages.AddNew();
		package.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;

		var goodsItem2 = houseConsignment.ArrivalGoodsItems.AddNew();
		goodsItem2.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;
		var package1 = goodsItem2.Packages.AddNew();
		package1.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;
		var package2 = goodsItem2.Packages.AddNew();
		package2.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;
		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();

				const string expectedMessage = "There are no good items available to enter the Temporary Storage. Please, check if action 'NCTS/Load Data for Unloading' has been previously triggered.";
				AssertEquals(message: "Any Package with Status != MIS doesn't exist in Goods Items",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Packages", UnitTestUserNotification.Instance.LastMessage.Text);

				package2.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;
				package2.Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Any Package with Status != MIS exists in Goods Items, but its goods item and house are MIS",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Packages", UnitTestUserNotification.Instance.LastMessage.Text);

				houseConsignment.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;
				goodsItem1.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;
				package.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;
				package.Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Any Package with Status != MIS exists in Goods Items and its goods item and house are != MIS",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_GuaranteNumberExistsAndIsValid()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;
		var houseConsignment = nctsHeader.Bills.AddNew();
		var goodsItem1 = houseConsignment.ArrivalGoodsItems.AddNew();
		var package = goodsItem1.Packages.AddNew();
		package.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var arrivalGuarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
		arrivalGuarantee.PW_BondNumber = "GUARANTEEREF";
		arrivalGuarantee.PW_Override = true;
		arrivalGuarantee.PW_BondAmount = 20;

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "Guarantee Nº (GUARANTEEREF) does not exist in Maintain/Customs/Customs Files/Customs Guarantees module or is not valid.";
				AssertEquals(message: "Guarantee doesn't exist",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				var cusPermitHeader = Factory.New<CusGuaranteeHeader>();
				cusPermitHeader.CPH_Number = "GUARANTEEREF";
				cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				cusPermitHeader.CPH_OH_PermitHolder = orgHeader.PK;
				cusPermitHeader.CPH_Type = "XX";
				cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(1);
				arrivalGuarantee.PW_BondNumber = ZString.Empty;
				arrivalGuarantee.PW_BondNumber = "GUARANTEEREF";
				arrivalGuarantee.PW_BondAmount = 20;
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee type is not TST",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				cusPermitHeader.CPH_Type = "TST";
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee start date is in the future",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				cusPermitHeader.CPH_StartDate = ZDate.Today.AddDays(-1);
				cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee end date is in the past",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				cusPermitHeader.CPH_EndDate = ZDate.Today.AddDays(1);
				cusPermitHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee has no opening balance transaction",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				var transaction = cusPermitHeader.OpeningCusGuaranteeLineTransactions.AddNew();
				transaction.CPL_TransactionType = "OBL";
				transaction.CPL_Reference = "REF";
				transaction.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee exists and is valid",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_GoodsItemsAreNotInTemporaryStorage()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var guarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
		guarantee.PW_BondNumber = "GUARANTEEREF";
		guarantee.PW_Override = true;
		guarantee.PW_BondAmount = 20;
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;

		var houseConsignment = nctsHeader.Bills.AddNew();
		var goodsItem1 = houseConsignment.ArrivalGoodsItems.AddNew();
		var package = goodsItem1.Packages.AddNew();
		package.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;
		nctsHeader.SummaryEntryNumber.CE_EntryNum = "99982000AH3";

		var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
		tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		tempStorageHeader.SRH_Reference = "99982000AH3";

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				const string expectedMessage = "Goods Items from Summary Declaration 99982000AH3 are already in the Temporary Storage.";
				AssertEquals(message: "Goods Items are already in Temporary Storage",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Items", UnitTestUserNotification.Instance.LastMessage.Text);

				tempStorageHeader.SRH_Reference = "DifferentReference";
				tempStorageHeader.Factory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Goods Items are not already in Temporary Storage",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_GuaranteeWithReferenceAndLiabilityAmount()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "Desc";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.SRP_CustomsLocation = "ES009999AH3";
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;

		Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();

				const string expectedMessage = "To enter goods into the Temporary Storage, a liability amount for a related Guarantee must be supplied.";
				AssertEquals(message: "No guarantee declared (all fields empty)",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				var arrivalGuarantee = nctsHeader.ArrivalMovementHeader.GuaranteesForArrival.AddNew();
				arrivalGuarantee.PW_BondNumber = "AAA";
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "No liability amount declared in the guarantee (BondNumber is not empty), no error",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				arrivalGuarantee.PW_BondNumber = ZString.Empty;
				arrivalGuarantee.PW_Override = true;
				arrivalGuarantee.PW_BondAmount = 20;
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "No reference declared in the guarantee (BondAmount is not empty)",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Guarantee", UnitTestUserNotification.Instance.LastMessage.Text);

				arrivalGuarantee.PW_BondNumber = "AAA";
				Factory.Save();
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Guarantee has bondNumber declared",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	public void TestIntoTemporaryStorage_ArrivalGoodsLocationExistsInPremises()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";

		nctsHeader.Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();

				const string expectedMessage = "Temporary Storage Location (ES009999AH3) does not exist in Maintain/Customs/Customs Files/Temporary Storage Premises module.";
				const string expectedMessage2 = "The associated Premises to the Temporary Storage Location (ES009999AH3) has no active numbering configuration or it has no available numbers.";

				AssertEquals(message: "Arrival Goods Location doesn't exist in premises",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Location", UnitTestUserNotification.Instance.LastMessage.Text);

				var newFactory = new BusinessObjectFactory();
				var premises = newFactory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
				premises.SRP_Code = "AH3";
				premises.SRP_Description = "Desc";
				var orgHeader = newFactory.New<OrgHeader>();
				orgHeader.OH_Code = "AH3";
				var orgAddress = newFactory.New<OrgAddress>();
				orgAddress.OA_OH = orgHeader.PK;
				orgAddress.OA_Address1 = "Address";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;
				premises.SRP_CustomsLocation = "ES009999AH3";
				newFactory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Arrival Goods Location exists in premises",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				AssertEquals(message: "Premises NumberProvider has not active wrapper",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

				var provider = premises.NumberProvider;
				_ = provider.CustomsNumbers.AddNew();
				var wrapper1 = provider.CustomsNumberWrappers[0];
				wrapper1.IsActive = true;
				wrapper1.SN_MinimumValue = 1;
				wrapper1.SN_Count = 1;
				newFactory.Save();
				_ = wrapper1.StmNums.GenerateNextCustomsNumber(newFactory);

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Premises NumberProvider has active wrapper without available numbers",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Premises", UnitTestUserNotification.Instance.LastMessage.Text);

				var otherFactory = new BusinessObjectFactory();
				var reloadedPremises = otherFactory.Load<EU.TemporaryStorage.Business.CusTempStorageRegPremises>(premises.PK);
				reloadedPremises.NumberProvider.CustomsNumberWrappers[0].IsActive = false;

				_ = provider.CustomsNumbers.AddNew();
				var wrapper2 = provider.CustomsNumberWrappers[1];
				wrapper2.IsActive = true;
				wrapper2.SN_MinimumValue = 1;
				wrapper2.SN_Count = 1;
				newFactory.Save();
				otherFactory.Save();

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Premises NumberProvider has active wrapper with available numbers",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage2));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_Mutex()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ES009999AH3";

		nctsHeader.Factory.Save();

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				_ = provider.Mutex.Lock();
				intoTempStorageMenuItem.PerformClick();
				provider.Mutex.Unlock();

				const string expectedMessage = "is already in the process of creating a Temporary Storage Register Header.\r\nYou should be able to access this option when the person has saved the record. Please try later.";

				AssertEquals(message: "Mutex already locked",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Last Message was cleared after error for Goods Location", UnitTestUserNotification.Instance.LastMessage.Text);

				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Mutex not already locked",
							 expected: false,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			});
		}
	}

	[RequiresSTA]
	public void TestIntoTemporaryStorage_PreSave()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;

		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (var nctsMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var intoTempStorageMenuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

			CombineAssertions(() =>
			{
				intoTempStorageMenuItem.PerformClick();
				AssertEquals(message: "Should have message asking to save the declaration before Into Temporary Storage",
							 expected: true,
							 actual: UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldSaveJobPopUpText));
			});
		}
	}

	#endregion

	#region View Temporary Storage Register

	public void TestViewTemporaryStorageRegisterClick()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.SummaryEntryNumber.CE_EntryNum = "1234567890";

			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
			arrivalMovementHeader.BM_NoChangesToReport = false;

			var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = "ArrivalLoc";
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
			tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			tempStorageHeader.SRH_Reference = "1234567890";
			tempStorageHeader.SRH_SRP_Premises = premises.PK;
			nctsHeader.Factory.Save();

			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var viewTempStorageRegistermenuItem = (ZMenuItem)nctsArrivalMovementForm.FindMenuItem_ForTest("View TS Register");

				CombineAssertions(() =>
				{
					viewTempStorageRegistermenuItem.PerformClick();
					var tempStorageRegisterForm = ZFormModaliser.LastFormShownDialogForTest;

					AssertType<TempStorageRegisterForm>(tempStorageRegisterForm);
					AssertNotEquals("Temporary Storage Register form is not the same as the NCTS Arrival Movement form", nctsArrivalMovementForm, tempStorageRegisterForm);
				});
			}
		}
	}

	public void TestViewTemporaryStorageRegisterClick_NoTemporaryStorageRegister()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.BH_JobReference = "NCT00000001";
			nctsHeader.SummaryEntryNumber.CE_EntryNum = "1234567890";

			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
			arrivalMovementHeader.BM_NoChangesToReport = false;
			nctsHeader.Factory.Save();

			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				const string expectedMessage = "No Entry in the Temporary Register found for (1234567890) in (ArrivalLoc)";

				nctsArrivalMovementForm.Show();
				var viewTempStorageRegistermenuItem = (ZMenuItem)nctsArrivalMovementForm.FindMenuItem_ForTest("View TS Register");

				CombineAssertions(() =>
				{
					viewTempStorageRegistermenuItem.PerformClick();
					AssertEquals(message: "Error when no corresponding record found in Temporary Storage Premises for Summary Entry Number (1234567890) in Goods Location (ArrivalLoc)",
								 expected: expectedMessage,
								 actual: UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
					premises.SRP_Type = "ADT";
					premises.SRP_CustomsLocation = "ArrivalLoc";
					premises.SRP_Code = "X";
					premises.SRP_Description = "DESC";
					var orgHeader = Factory.New<OrgHeader>();
					orgHeader.OH_Code = "AAA";
					var orgAddress = Factory.New<OrgAddress>();
					orgAddress.OA_OH = orgHeader.PK;
					orgAddress.OA_Address1 = "Address";
					premises.SRP_OA_PremisesAddress = orgAddress.PK;
					nctsHeader.Factory.Save();

					viewTempStorageRegistermenuItem.PerformClick();
					AssertEquals(message: "Error when no correspoding record found in Temporary Storage Register for Summary Entry Number (1234567890) in Goods Location (ArrivalLoc)",
								 expected: expectedMessage,
								 actual: UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var tempStorageHeader = Factory.New<CusTempStorageRegHeader>();
					tempStorageHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
					tempStorageHeader.SRH_Reference = "0987654321";
					tempStorageHeader.SRH_SRP_Premises = premises.PK;
					nctsHeader.Factory.Save();

					viewTempStorageRegistermenuItem.PerformClick();
					AssertEquals(message: "Error when no matching record found in Temporary Storage Register for Summary Entry Number (1234567890) in Goods Location (ArrivalLoc)",
								 expected: expectedMessage,
								 actual: UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}

	#endregion

	#region Guarantee Transaction

	public void TestIntoTemporaryStorageClick_Transaction()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			CreateArrivalForGuaranteeTransaction();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var menuItem = (ZMenuItem)nctsArrivalMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", nctsHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, nctsHeader.ArrivalMovementHeader?.GuaranteesForArrival.FirstOrDefault()?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					menuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", nctsHeader.Factory);
					AssertNull("Warning was not display", UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(x => x.WasWarning));

					AssertTransactionIsCreated(-50.0m);
				});
			}
		}
	}

	[TestDate(2003, 02, 01, 0, 0, 0)]
	public void TestIntoTemporaryStorageClick_TransactionBillMIS()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			CreateArrivalForGuaranteeTransaction(billStatus: "MIS");

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var menuItem = (ZMenuItem)nctsArrivalMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", nctsHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, nctsHeader.ArrivalMovementHeader?.GuaranteesForArrival.FirstOrDefault()?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					menuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", nctsHeader.Factory);

					AssertEquals("With all Bill MIS, no transaction is created", 0, nctsHeader.ArrivalMovementHeader?.GuaranteesForArrival.FirstOrDefault()?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
				});
			}
		}
	}

	[TestDate(2003, 02, 01, 0, 0, 0)]
	public void TestIntoTemporaryStorageClick_TransactionNoLiabilityAmount()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			CreateArrivalForGuaranteeTransaction(liabilityAmount: 0.0m);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var menuItem = (ZMenuItem)nctsArrivalMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", nctsHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, nctsHeader.ArrivalMovementHeader?.GuaranteesForArrival.FirstOrDefault()?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					menuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", nctsHeader.Factory);

					AssertEquals("With Liability amount 0, no transaction is created", 0, nctsHeader.ArrivalMovementHeader?.GuaranteesForArrival.FirstOrDefault()?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
				});
			}
		}
	}

	[TestDate(2003, 02, 01, 0, 0, 0)]
	public void TestIntoTemporaryStorageClick_TransactionWarning()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		{
			CreateArrivalForGuaranteeTransaction(liabilityAmount: 5000.5m);

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (var nctsArrivalMovementForm = new Phase5ArrivalMovementForm(nctsHeader))
			{
				nctsArrivalMovementForm.Show();
				var menuItem = (ZMenuItem)nctsArrivalMovementForm.FindMenuItem_ForTest("Into Temporary Storage");

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.AddYesAnswer();
					nctsHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before clicking", nctsHeader.Factory);
					AssertEquals("[PreReq] no transactions", 0, nctsHeader.ArrivalMovementHeader?.GuaranteesForArrival.FirstOrDefault()?.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == "TRA"));
					menuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After clicking", nctsHeader.Factory);
					var warning = UnitTestUserNotification.Instance.PreviousMessages.FirstOrDefault(x => x.WasWarning);
					AssertNotNull("Warning exists", warning);
					AssertEquals("Warning was display", "Guarantee Nº (Test1) has not enough remaining balance (1000.00) to create the guarantee transaction (5000.50). It will be created anyway.", warning?.Text);

					AssertTransactionIsCreated(-5000.5m);
				});
			}
		}
	}

	void AssertTransactionIsCreated(decimal expectedTranValue)
	{
		var transactions = nctsHeader.ArrivalMovementHeader?.GuaranteesForArrival.FirstOrDefault()?.CusGuarantee.GetTransactions().Where(x => x.CPL_TransactionType == "TRA");
		var transaction = transactions.FirstOrDefault();
		AssertEquals("With at least one Bill not MIS and Liability Amount not 0, transaction is created", 1, transactions.Count());
		AssertEquals("Transaction Date", new ZDateTime(2023, 01, 01, 02, 01, 00), transaction.CPL_TransactionDate);
		AssertEquals("Transaction Type", "TRA", transaction.CPL_TransactionType);
		AssertEquals("Reference", "99982000174", transaction.CPL_Reference);
		AssertEquals("Value", expectedTranValue, transaction.CPL_TranValue);
		AssertEquals("Comment", "NCTS Arrival ArrivalTest. MRN: 1234567890", transaction.CPL_Comment);
		AssertEquals("Status", "CON", transaction.CPL_TransactionStatus);
	}

	#endregion

	#region Inventory Management

	#region DPT

	public void TestSendToCustoms_DPT_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods();

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005");

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithoutN337doc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference");

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(setCorrectPremisesInHeader: false);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs."));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithoutRegLineItem()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(prevDocLineNo: 2);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing there is no line item associated", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again."));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithVINError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing expected vin is not in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again."));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithPackageError_NotBulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing there is not enough pack qty in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again."));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithPackageError_Bulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing there is not enough pack qty in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again."));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithGrossWeightError_AnswerYes()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddYesAnswer();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing there is not enough gross weight in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 16.000000\n\nDo you want to cancel this declaration to check?"));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithGrossWeightError_AnswerNo()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing there is not enough gross weight in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 16.000000\n\nDo you want to cancel this declaration to check?"));

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -5m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -5m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(prevDocReference: FormattedPrevDocReference);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -5m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -22m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: PrevDocReference);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -5m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -22m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPT_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods();

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5Departure))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5Departure, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -5m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -22m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	#endregion

	#region DPN

	[RequiresSTA]
	public void TestSendToCustoms_DPN_TemporaryStorageRegisterNotEnabled()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods();

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithEmptyGoodsLocation()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(locationInEntry: ZString.Empty);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(locationInPremises: "9999000005");

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithoutN337doc()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(shouldAddDoc: false);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithoutRegHeader()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: "reference");

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertEquals("The message has been created and sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithoutPremises()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(setCorrectPremisesInHeader: false);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: Goods in TSD Number 99994000128 are not stored in location 9999000002 so this declaration might be rejected by Customs. The correct location should be 9999000005.\n\nPlease, set the correct location before submitting this declaration to Customs."));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithoutRegLineItem()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(prevDocLineNo: 2);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing there is no line item associated", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: There is no item line 2 in the Temporary Storage for TSD Number 99994000128. Please, correct data and send again."));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithVINError()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(packageVin: "AAAA", transactionGrossWeight: 5m);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing expected vin is not in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: VIN VIN1 is not present in the Temporary Storage under TSD Number 99994000128, Item 1. Please, correct data and send again."));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithPackageError_NotBulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(packageQtyNotBulk: 2, transactionGrossWeight: 5m);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing there is not enough pack qty in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 9 BX. Please, correct data and send again."));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithPackageError_Bulk()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(bulkPackageTypeForRegLine: "V0", transactionGrossWeight: 5m);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing there is not enough pack qty in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001: There is not enough quantity of goods in the Temporary Storage for TSD Number 99994000128, Item 1: 0 VG. Please, correct data and send again."));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithGrossWeightError_AnswerYes()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddYesAnswer();
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing there is not enough gross weight in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 16.000000\n\nDo you want to cancel this declaration to check?"));

					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ShouldEditMessagePopUpText));
					AssertNotEquals("The message has not been created nor sent", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NctsHeader message status has not changed, BM_MessageStatus", ZString.Empty, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status has not changed", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
					AssertEquals("NctsHeader has no messages", 0, nctsHeader.Messages.Count);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_WithGrossWeightError_AnswerNo()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(transactionGrossWeight: 5m);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing there is not enough gross weight in temp storage", true,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("ES00001:\nThere might not be enough Gross Weight 22 for TSD Number 99994000128, Item 1.\nRemaining Gross Weight in the Temporary Storage: 16.000000\n\nDo you want to cancel this declaration to check?"));

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -5m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -5m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefShorterThan18()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(prevDocReference: FormattedPrevDocReference);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -5m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -22m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	[RequiresSTA]
	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefLongerThan18_WithoutFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(regHeaderReference: PrevDocReference);

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -5m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -22m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	public void TestSendToCustoms_DPN_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_DocRefLongerThan18_WithFormatting()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods();

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 170", ESNctsMovementHeaderTransactionStatusList.Codes.Presentation, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has a new transaction", 3, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has a new transaction", 2, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has a new transaction", 2, regLine3.CusTempStorageRegLineTransactions.Count);

					AssertNewTransactionToReserveGoods(regLine1, -1, -5m);
					AssertNewTransactionToReserveGoods(regLine2, 0, -22m);
					AssertNewTransactionToReserveGoods(regLine3, -9, -6m);
				});
			}
		}
	}

	#endregion

	[RequiresSTA]
	public void TestSendToCustoms_DPD_TemporaryStorageEnabledWithPremisesWithN337docWithRegHeaderWithPremises_NothingIsDone()
	{
		var registry = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (nctsHeader, regLineTransaction, regLine1, regLine2, regLine3) = SetUpDataForForReserveTemporaryStorageGoods(customsStatus: "");

			nctsHeader.Factory.Save();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			using (SubstituteNctsMessagingMenuProvider(nctsHeader, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration))
			using (var nctsMovementForm = new Phase5DepartureMovementForm(nctsHeader))
			{
				nctsMovementForm.Show();
				var sendToCustomsMenuItem = FindSendToCustomsMenuItem(nctsMovementForm);

				CombineAssertions(() =>
				{
					nctsHeader.Factory.Save();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestHelper.CheckFactoryHasNoPendingChanges("Before sending", nctsHeader.Factory);
					sendToCustomsMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After sending", nctsHeader.Factory);
					AssertEquals("Should have message with error informing Premises is not associated to RegHeader", false,
						UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please, set the correct location before submitting this declaration to Customs."));

					AssertEquals("NctsHeader message status has changed, BM_MessageStatus", LogicalStatusList.Codes.Sent, nctsHeader.MovementHeader.BM_MessageStatus);
					AssertEquals("NctsHeader phase status is 015", ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
					var msg = nctsHeader.Messages.LastOutgoingMessage;
					AssertNotNull("EDIMessage was created for the ncts header", msg);
					AssertEquals("New message's type is correct", DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, msg.EM_MessageType);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", msg.EM_MessageText);

					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);

					AssertEquals("regLine1 has no new transactions", 2, regLine1.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine2 has no new transactions", 1, regLine2.CusTempStorageRegLineTransactions.Count);
					AssertEquals("regLine3 has no new transactions", 1, regLine3.CusTempStorageRegLineTransactions.Count);
				});
			}
		}
	}

	const string EntryReference = "ES00001";
	const string LocationInEntry = "9999000002";
	const string PrevDocCode = "N337";
	const string PrevDocReference = "24ES00999980001282";
	const string FormattedPrevDocReference = "99994000128";

	(NctsHeader nctsHeader, CusTempStorageRegLineTransaction regLineTransaction, CusTempStorageRegLine regLine1, CusTempStorageRegLine regLine2, CusTempStorageRegLine regLine3)
		SetUpDataForForReserveTemporaryStorageGoods(bool shouldAddDoc = true, bool setCorrectPremisesInHeader = true, string prevDocCode = PrevDocCode, string prevDocReference = PrevDocReference, int prevDocLineNo = 1,
		string locationInEntry = LocationInEntry, string locationInPremises = LocationInEntry, string regHeaderReference = FormattedPrevDocReference, string packageVin = "VIN1", int packageQtyNotBulk = 9, decimal transactionGrossWeight = 40m, string bulkPackageTypeForRegLine = "VG", string customsStatus = "PRE")
	{
		Factory.SetBulkTypeHelper();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
		nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
		nctsHeader.MovementHeader.Guarantees.AddNew();
		nctsHeader.MovementHeader.BM_CustomsStatus = customsStatus;
		nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
		nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

		var bill1 = nctsHeader.Bills.AddNew();

		var goodsItem1 = bill1.GoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 11m;
		goodsItem1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
		goodsItem1.IsVehicles = true;

		var vehicle1 = goodsItem1.Packages.AddNew();
		vehicle1.B5_PackageID = "VIN1";

		var goodsItem2 = bill1.GoodsItems.AddNew();
		goodsItem2.BY_GrossWeight = 11m;
		goodsItem2.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
		goodsItem2.IsVehicles = false;

		var package1 = goodsItem2.Packages.AddNew();
		package1.B5_MarksAndNumbers = "marks";
		package1.B5_UnitType = "BX";
		package1.B5_UnitCount = 9;

		var package2 = goodsItem2.Packages.AddNew();
		package2.B5_MarksAndNumbers = "bulk gas marks";
		package2.B5_UnitType = "VG";
		package2.B5_UnitCount = 0;

		nctsHeader.MovementHeader.BM_PaperlessInbondNum = EntryReference;

		if (shouldAddDoc)
		{
			var previousDoc1 = goodsItem1.PreviousDocuments.AddNew();
			previousDoc1.CSI_Code = prevDocCode;
			previousDoc1.CSI_ReferenceNumber = prevDocReference;
			previousDoc1.CSI_ItemNumber = prevDocLineNo;

			var previousDoc2 = goodsItem2.PreviousDocuments.AddNew();
			previousDoc2.CSI_Code = prevDocCode;
			previousDoc2.CSI_ReferenceNumber = prevDocReference;
			previousDoc2.CSI_ItemNumber = prevDocLineNo;
		}

		nctsHeader.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier = locationInEntry;

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		var premises1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises1.SRP_Type = "ADT";
		premises1.SRP_CustomsLocation = locationInPremises;
		premises1.SRP_Code = "X";
		premises1.SRP_Description = "DESC";
		premises1.SRP_OA_PremisesAddress = orgAddress.PK;
		var premises2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises2.SRP_Type = "ADT";
		premises2.SRP_CustomsLocation = "9999000005";
		premises2.SRP_Code = "A";
		premises2.SRP_Description = "DESC2";
		premises2.SRP_OA_PremisesAddress = orgAddress.PK;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = regHeaderReference;
		regHeader.SRH_SRP_Premises = setCorrectPremisesInHeader ? premises1.PK : premises2.PK;
		var regLine1 = Factory.New<CusTempStorageRegLine>();
		regLine1.SRL_LineNumber = 1;
		regLine1.SRL_CustomsStatus = "OPN";
		regLine1.SRL_PackageType = "FR";
		regLine1.SRL_PackageMarks = packageVin;
		regLine1.SRL_SRH = regHeader.PK;
		var regLineTransactionPND1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransactionPND1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransactionPND1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransactionPND1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransactionPND1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		var regLineTransaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction1.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		regLineTransaction1.SRT_PackageQty = 10;
		regLineTransaction1.SRT_GrossWeight = 5m;

		var regLine2 = Factory.New<CusTempStorageRegLine>();
		regLine2.SRL_LineNumber = 2;
		regLine2.SRL_CustomsStatus = "OPN";
		regLine2.SRL_PackageType = bulkPackageTypeForRegLine;
		regLine2.SRL_SRH = regHeader.PK;
		var regLineTransaction2 = regLine2.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_InternalReferenceNumber = EntryReference;
		regLineTransaction2.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		regLineTransaction2.SRT_PackageQty = 10;
		regLineTransaction2.SRT_GrossWeight = transactionGrossWeight;

		var regLine3 = Factory.New<CusTempStorageRegLine>();
		regLine3.SRL_LineNumber = 3;
		regLine3.SRL_CustomsStatus = "OPN";
		regLine3.SRL_PackageType = "BX";
		regLine3.SRL_SRH = regHeader.PK;
		var regLineTransaction3 = regLine3.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction3.SRT_InternalReferenceNumber = "AAAAA";
		regLineTransaction3.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		regLineTransaction3.SRT_PackageQty = packageQtyNotBulk;
		regLineTransaction3.SRT_GrossWeight = 6m;

		var regLineItem = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItem>();
		regLineItem.SRI_GoodsItemNumber = 1;

		var regLineItemPivot1 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot1.SRV_SRL_Line = regLine1.PK;

		var regLineItemPivot2 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot2.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot2.SRV_SRL_Line = regLine2.PK;

		var regLineItemPivot3 = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>();
		regLineItemPivot3.SRV_SRI_Item = regLineItem.PK;
		regLineItemPivot3.SRV_SRL_Line = regLine3.PK;

		return (nctsHeader, regLineTransactionPND1, regLine1, regLine2, regLine3);
	}

	void AssertNewTransactionToReserveGoods(CusTempStorageRegLine regLine, ZInt expectedPackQty, ZDecimal expectedGrossWeight, string expectedComment = "")
	{
		var lineNum = regLine.SRL_LineNumber;
		var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_GrossWeight == expectedGrossWeight);
		AssertNotNull("Line " + lineNum + " has new transaction", transaction);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionType correct", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_TransactionStatus correct", CusTempStorageRegLineTransactionStatusList.Codes.Pending, transaction.SRT_TransactionStatus);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceNumber correct", EntryReference, transaction.SRT_InternalReferenceNumber);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_InternalReferenceType correct", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration, transaction.SRT_InternalReferenceType);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_PackageQty correct", expectedPackQty, transaction.SRT_PackageQty);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_GrossWeight correct", expectedGrossWeight, transaction.SRT_GrossWeight);
		AssertEquals("Line " + lineNum + " has new transaction with SRT_Comments correct", expectedComment, transaction.SRT_Comments);
	}

	#endregion

	#region Test helpers

	void AssertVisibleMenuItems(params string[] expectedMenuItems)
	{
		provider.RefreshMenu();
		AssertContainsExactElementsInExactOrder(
			expectedMenuItems,
			menuItems.Where(x => x.Visible).Select(x => x.Text));
	}

	IDisposable SubstituteNctsMessagingMenuProvider(
		NctsHeader header,
		string messageType,
		bool hasInvalidCertificate = false,
		bool shouldEdit = false,
		bool? isResending = null)
	{
		var menuProvider = new KeyObjectHandleDictionaryObject
			{
				{
					Core.Constants.CountryCodes.Spain,
					new TestObjectHandle(new Phase5MessagingMenuProviderForTest(
						header,
						hasInvalidCertificate,
						shouldEdit,
						messageType: messageType,
						isResending: isResending))
				}
			};
		return ObjectFactory.Substitute("NCTSMessagingMenuProviders", menuProvider);
	}

	ZMenuItem FindSendToCustomsMenuItem(Phase5DepartureMovementForm form) =>
		(ZMenuItem)form.FindMenuItem_ForTest("Send to Customs");

	ZMenuItem FindSendToCustomsMenuItem(Phase5ArrivalMovementForm form) =>
		(ZMenuItem)form.FindMenuItem_ForTest("Send to Customs");

	ZTabPage FindMainTabPage(Phase5DepartureMovementForm form) =>
		form.FindSingle<ZTabPage>("MainTabPage");

	ZDropEdit FindDeclarationTypeDropEdit(Phase5DepartureMovementForm form) =>
		form.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");

	ZDropEdit FindDeclarationTypeDropEdit(ZTabPage tabPage) =>
		tabPage.FindSingle<ZDropEdit>("DeclarationTypeDropEdit");

	#endregion

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.FillWithValidTestData();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		org = Factory.NewWithValidTestData<OrgHeader>();
		org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1234", "ES");
		var address = org.Addresses.AddNew();
		address.OA_Address1 = "Address";
		org.Contacts.AddNew();

		provider = new Phase5MessagingMenuProvider(nctsHeader);
		menuItems = provider.CreateMenuItems();
	}
	NctsHeader nctsHeader;
	OrgHeader org;
	Phase5MessagingMenuProvider provider;
	IEnumerable<ZMenuItem> menuItems;

	public GlbStaff Staff => staff ?? (staff = Factory.GetStaffAccount());
	GlbStaff staff;

	NctsHeader GetNctsDeparture(string customsStatus = "")
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.FillWithValidTestData();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.BM_CustomsStatus = customsStatus;
		nctsHeader.MovementHeader.BM_GS_NKCusAgent = Staff.GS_Code;
		nctsHeader.BH_CustomsProfile = BuilderHelperTest.CertificateName;

		Factory.Save();

		provider = new Phase5MessagingMenuProvider(nctsHeader);
		menuItems = provider.CreateMenuItems();

		return nctsHeader;
	}

	void CreateCusEntryNumber(NctsHeader header, ZString entryNum, ZDateTime issueDate)
	{
		var newEntryNumber = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = entryNum;
		newEntryNumber.CE_IssueDate = issueDate;
		newEntryNumber.CE_ExpiryDate = ZDateTime.Empty;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
		Factory.Save();
	}

	void CreateArrivalForGuaranteeTransaction(string billStatus = "NEW", decimal liabilityAmount = 50.0m)
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.BH_JobReference = "ArrivalTest";
		nctsHeader.SummaryEntryNumber.CE_EntryNum = "99982000174";
		nctsHeader.SummaryEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "1234567890";

		var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ArrivalLoc";
		arrivalMovementHeader.BM_NoChangesToReport = false;

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		var guarantee = Factory.New<CusGuaranteeHeader>();
		guarantee.CPH_Number = "Test1";
		guarantee.CPH_OH_PermitHolder = orgHeader.PK;
		guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		guarantee.CPH_SubType = "1";
		guarantee.CPH_StartDate = ZDate.BrettsBirthday;
		guarantee.CPH_Balance = 1000.0m;
		guarantee.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
		guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		var nctsGuarantee = arrivalMovementHeader.SingleGuaranteeForArrival;
		nctsGuarantee.PW_BondNumber = "Test1";
		nctsGuarantee.PW_Override = true;
		nctsGuarantee.PW_BondAmount = liabilityAmount;

		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "ArrivalLoc";
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		var provider = premises.NumberProvider;
		_ = provider.CustomsNumbers.AddNew();
		var wrapper1 = provider.CustomsNumberWrappers[0];
		wrapper1.IsActive = true;
		wrapper1.SN_MinimumValue = 1;
		wrapper1.SN_Count = 1;

		var bill1 = nctsHeader.Bills.AddNew();
		bill1.MovementDetail.B9_SeqNo = "1";
		bill1.MovementDetail.B9_UnloadedState = billStatus;

		var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
		goodsItem1.SetValues("NEW", 1, 1, "111111", "CUSCODE1", "Description1", 2);

		var pack1 = goodsItem1.Packages.AddNew();
		pack1.SetValues("DEC", "BX", "marks1", 5, 2);

		nctsHeader.Factory.Save();
	}

	void SetUpTariffAndRates()
	{
		var countrycode = Core.Constants.CountryCodes.Spain;

		var helper = new UniversalReferenceTestDataHelper(Factory);

		helper.CreateRefCusTaxOrFeeType("VAT");

		var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(countrycode, "test country", parentDataGrouping);
		var tariffType = helper.CreateNewOrGetExistingTariffType(countrycode, Universal.Constants.TariffTypes.Import);
		tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
		Factory.Save();

		var tariff = helper.CreateTariff(countrycode, tariffType.PK, "0304798000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");

		var rateTypeDuty = helper.CreateCusRateType(countrycode, "DTY");
		var rateCodeDuty = helper.CreateCusRateCode(Factory, "A00", rateTypeDuty.PK);
		var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", countrycode, "EUN");

		var rateDuty = helper.CreateRefCusRate(tariff.PK, rateCodeDuty.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);

		var tradeGroup = helper.CreateTradeGroup(countrycode, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(tradeGroup, "EU");
		helper.AddCountry(tradeGroup, countrycode);
		helper.CreateCusApplicability(rateDuty.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var rateTypeCountervailing = helper.CreateCusRateType(countrycode, Customs.Universal.Constants.RateTypes.Countervailing, ensureDataGroupingExists: false);
		var rateCodeCountervailing = helper.CreateCusRateCode(Factory, "RC1", rateTypeCountervailing.PK);
		var testRateCountervailing = helper.CreateRate(tariff, rateCodeCountervailing.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "24.3 * [FLAT]");

		var rateTypeAntiDumping = helper.CreateCusRateType(countrycode, Customs.Universal.Constants.RateTypes.AntiDumping, ensureDataGroupingExists: false);
		var rateCodeAntiDumping = helper.CreateCusRateCode(Factory, "RC1", rateTypeAntiDumping.PK);
		var testRateAntiDumping = helper.CreateRate(tariff, rateCodeAntiDumping.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "4.3 * [FLAT]");

		helper.CreateCusApplicability(testRateCountervailing, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC01");
		helper.CreateCusApplicability(testRateAntiDumping, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "Additional Code");
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, "Default Rate", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

		var lastMonth = ZDateTime.Today.AddMonths(-1);
		var nextMonth = ZDateTime.Today.AddMonths(1);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC01", "EU AC01", lastMonth, nextMonth)
			.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC02", "EU AC02", lastMonth, nextMonth)
			.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);

		Factory.Save();
	}

	IEnumerable<(ZShort, ZString, ZString, ZString, ZString)> GetArrivalBillTransportInfos(Business.NctsBill bill) =>
		GetTransportMeansCollection(bill.ArrivalTransportInfos);

	IEnumerable<(ZShort, ZString, ZString, ZString, ZString)> GetArrivalHeaderTransportInfos(NctsHeader header) =>
		GetTransportMeansCollection(header.ArrivalMovementHeader.ArrivalTransportInfos);

	IEnumerable<(ZShort, ZString, ZString, ZString, ZString)> GetTransportMeansCollection(IArrivalCusTransportMeansCollection<Business.ArrivalCusTransportMeans> transportMeans) =>
		transportMeans.Select(x =>
			(x.TPM_SequenceNumber,
			x.TPM_TransportState,
			x.TPM_TypeOfIdentification,
			x.TPM_IdentificationNumber,
			x.TPM_RN_NKTransportNationality))
		.ToArray();

	const string GoodsDescription = "Description";

	const string NoSendingOptionsAvailablePopUpText = "No sending options available.";

	const string WrongBrokerOrCertificatePopUpText = "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.";

	const string ShouldSaveJobPopUpText = "The Job has not yet been saved. Do you want to save and proceed?";

	const string ShouldEditMessagePopUpText = "Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?";

	const string ShouldSendTQUMessagePopUpText = "A Query Message (TQU) will be sent to Customs to obtain the Departure Data for the Unloading.\r\nDo you want to send that message? Please note existing data in Unloading Remarks could be overwritten.";

	const string ShouldSendTQUMessageToSynchronizePopUpText = "A Query Message (TQU) will be sent to Customs to Synchronize Pre-Declaration data.\r\nDo you want to send that message? Please note that existing data will be overwritten, but Consignee and Consignor will not be updated, you must do it manually if changed.";

	const string CannotSendNotificationOfGoodsPopupText = "Cannot send Notification of Goods. Departure declaration must not be sent already, and must have Pre-Lodged Customs Status.";

	const string CannotSendAmendmentPopupText = "Cannot send Amendment. Departure declaration must not be sent already, and must have Pre-Lodged Customs Status.";

	const string CannotSendCancellationPopupText = "Cannot send Cancellation. Departure declaration must not be sent already, and must have Pre-Lodged or Pending Acceptance Customs Status.";

	const string CannotSendAnnexesPopupText = "Cannot send Annexes. Departure declaration must not be sent already, and must have Decision to Control Customs Status.";

	class Phase5MessagingMenuProviderForTest : Phase5MessagingMenuProvider
	{
		public Phase5MessagingMenuProviderForTest(NctsHeader header, ZBool hasInvalidCertificate, ZBool shouldEdit, TnnDataCodeInfo tnnDataCodeInfo = null, bool departureSelected = true, ZString? messageType = null, bool? isResending = null) : base(header)
		{
			this.shouldEdit = shouldEdit;
			this.hasInvalidCertificate = hasInvalidCertificate;
			this.tnnDataCodeInfo1 = tnnDataCodeInfo;
			this.departureSelected = departureSelected;
			this.messageType = messageType;
			this.isResending = isResending;
		}
		readonly ZBool shouldEdit;
		readonly ZBool hasInvalidCertificate;
		readonly TnnDataCodeInfo tnnDataCodeInfo1;
		readonly ZBool departureSelected;
		readonly ZString? messageType;
		readonly bool? isResending;

		protected override ESNctsCommonMessagingMenuProvider GetESNctsCommonMessagingMenuProvider(SendingType sendingType = SendingType.None, ZForm parentForm = null) => new ESNctsCommonMessagingMenuProviderForEditTest(Header, hasInvalidCertificate, shouldEdit, sendingType, messageType, parentForm);

		protected override TnnDataCodeInfo LoadNewTnnDataCodeInfo(NctsHeader header) => tnnDataCodeInfo1;

		protected override LoadDataForUnloadingSelectorForm GetLoadDataForUnloadingSelectorForm() => new LoadDataForUnloadingSelectorFormForTest(departureSelected);

		protected override bool IsResending(CusInBondMoveHeader movementHeader) => isResending ?? base.IsResending(movementHeader);
	}

	class ESNctsCommonMessagingMenuProviderForEditTest : ESNctsCommonMessagingMenuProvider
	{
		public ESNctsCommonMessagingMenuProviderForEditTest(NctsHeader header, ZBool hasInvalidCertificate, ZBool shouldEdit, SendingType sendingType, ZString? messageType = null, ZForm parentForm = null) : base(header, sendingType, parentForm: parentForm)
		{
			this.shouldEdit = shouldEdit;
			this.hasInvalidCertificate = hasInvalidCertificate;
			this.messageType = messageType;
		}
		readonly ZBool shouldEdit;
		readonly ZBool hasInvalidCertificate;
		readonly ZString? messageType;

		protected override MessageEditForm GetMessageEditForm() => new MessageEditFormForTest();
		protected override bool GetShouldEditMessagePopUpResponse(DialogResult defaultValue) => base.GetShouldEditMessagePopUpResponse(shouldEdit ? DialogResult.Yes : DialogResult.No);
		protected override bool HasInvalidCertificate() => hasInvalidCertificate && base.HasInvalidCertificate();
		protected override MessageSendingForm GetMessageSendingForm(NctsHeaderMessageSendingObjectParent sendingObjectParent) => new SendFormForTest(sendingObjectParent, messageType);
	}

	class MessageEditFormForTest : MessageEditForm
	{
		public MessageEditFormForTest() : base() { }

		public override (ZString, ZBool) EditMessage(ZString messageText) => (messageText.Replace(GoodsDescription, "AAAAAAAAAAAAA"), true);
	}

	class LoadDataForUnloadingSelectorFormForTest : LoadDataForUnloadingSelectorForm
	{
		public LoadDataForUnloadingSelectorFormForTest(ZBool departureSelected)
			: base()
		{
			DepartureRadioButton.Checked = departureSelected;
		}
	}

	class SendFormForTest : MessageSendingForm
	{
		public SendFormForTest(NctsHeaderMessageSendingObjectParent sendingObjectParent, ZString? messageType = null)
			: base(sendingObjectParent)
		{
			foreach (NctsHeaderMessageSendingObject sendingObject in sendingObjectParent.SendingObjectsCollection)
			{
				if (messageType.HasValue)
				{
					sendingObject.MessageType = messageType.Value;
				}
				sendingObject.ReasonForCancellation = "Reason for\r\nCancellation";
			}
		}
	}
}
