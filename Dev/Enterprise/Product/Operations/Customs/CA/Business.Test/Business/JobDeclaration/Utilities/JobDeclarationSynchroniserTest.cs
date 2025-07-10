using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;
using RefCusCodeListAttributes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.Attributes;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationSynchroniserTest : Customs.Business.Testing.JobDeclarationSynchroniserTest
	{
		protected override void SetupForDetection(ForwardingShipment shipment, ZString origin, ZString destination)
		{
			base.SetupForDetection(shipment, origin, destination);
			shipment.JS_F3_NKPackType = CustomsUnitOfMeasureList.ConvertStockUnitsToCustomsUnits(shipment.JS_F3_NKPackType, Factory, true);
		}

		public void TestCusEntryNumbersSynchroniser()
		{
			var number1 = shipment.Numbers.AddNew();
			number1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number1.CE_EntryNum = "1111111";

			var number2 = shipment.Numbers.AddNew();
			number2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			number2.CE_EntryNum = "2222222";

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(2, declaration.AdditionalReferenceNumbers.Count);

			AssertEquals(number1.CE_EntryNum, declaration.AdditionalReferenceNumbers[0].CE_EntryNum);
			AssertEquals(number1.CE_EntryType, declaration.AdditionalReferenceNumbers[0].CE_EntryType);

			AssertEquals(number2.CE_EntryNum, declaration.AdditionalReferenceNumbers[1].CE_EntryNum);
			AssertEquals(number2.CE_EntryType, declaration.AdditionalReferenceNumbers[1].CE_EntryType);

			number2.CE_EntryNum = "3333333";

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(2, declaration.AdditionalReferenceNumbers.Count);
			AssertEquals(number2.CE_EntryNum, declaration.AdditionalReferenceNumbers[1].CE_EntryNum);

			number2.CE_EntryNum = "4444444";

			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			declaration2.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(2, declaration2.AdditionalReferenceNumbers.Count);
			AssertEquals(number1.CE_EntryNum, declaration2.AdditionalReferenceNumbers[0].CE_EntryNum);
			AssertEquals(number1.CE_EntryType, declaration2.AdditionalReferenceNumbers[0].CE_EntryType);
			AssertEquals(number2.CE_EntryNum, declaration2.AdditionalReferenceNumbers[1].CE_EntryNum);
			AssertEquals(number2.CE_EntryType, declaration2.AdditionalReferenceNumbers[1].CE_EntryType);
		}

		public void TestJE_RL_NKPortOfLoadingSynchronisation()
		{
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(false, declaration.IsImport);
			AssertEquals(true, declaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(testHelper.CATOR.Code, declaration.JE_RL_NKPortOfLoading);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNull(consol.Transports.ImportTransport);
			AssertEquals("", declaration.JE_RL_NKPortOfLoading);

			consol.JK_RL_NKLoadPort = testHelper.AUBNE.Code;
			consol.JK_RL_NKDischargePort = testHelper.CAVAR.Code;
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertNotNull(consol.Transports.ImportTransport);
			AssertEquals(testHelper.AUBNE.Code, declaration.JE_RL_NKPortOfLoading);
		}

		public override void TestLoadAndDischargeSyncroniseToCorrectTransport_Export()
		{
			Assert("This test is currently not applicable for CA", true);
		}

		public void TestCA_PortOfExitSynchronisation()
		{
			var exportTransport = consol.Transports.ExportTransport;
			AssertNotNull(exportTransport);
			declaration.CA_PortOfExit = ZString.Empty;
			testHelper.Consignor.OH_RL_NKClosestPort = testHelper.AUBNE.Code;
			testHelper.Consignee.OH_RL_NKClosestPort = testHelper.CATOR.Code;
			shipment.JS_RL_NKOrigin = testHelper.AUBNE.Code;
			shipment.JS_RL_NKDestination = testHelper.CATOR.Code;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			Factory.Save();

			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);

			AssertEquals(false, declaration.CA_PlaceOfReportInfo.ReadOnly);
			AssertEquals(ZString.Empty, declaration.CA_PortOfExit);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals(false, declaration.CA_PortOfExitInfo.ReadOnly);
			AssertEquals(testHelper.CATOR.Code, exportTransport.JW_RL_NKLoadPort);
			AssertEquals(officeCATOR.ZZD_Code, declaration.CA_PortOfExit);

			declaration.CA_PortOfExit = ZString.Empty;
			exportTransport.JW_RL_NKLoadPort = testHelper.CAVAR.Code;
			AssertEquals(officeCAVAR.ZZD_Code, declaration.CA_PortOfExit);

			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			shipment.Consols.RemoveAndDeleteAll();
			declaration.CA_PortOfExit = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = testHelper.CATOR.Code;
			AssertEquals(officeCATOR.ZZD_Code, declaration.CA_PortOfExit);

			declaration.CA_PortOfExit = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = testHelper.AUSYD.Code;
			AssertEquals(ZString.Empty, declaration.CA_PortOfExit);
		}

		public void TestCA_PlaceOfReportSynchronisation()
		{
			CACustomsDataRegistry.Instance.DefaultPlaceOfReport.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "");
			var exportTransport = consol.Transports.ExportTransport;
			AssertNotNull(exportTransport);
			declaration.CA_PlaceOfReport = ZString.Empty;
			testHelper.Consignor.OH_RL_NKClosestPort = testHelper.AUBNE.Code;
			testHelper.Consignee.OH_RL_NKClosestPort = testHelper.CATOR.Code;
			shipment.JS_RL_NKOrigin = testHelper.AUBNE.Code;
			shipment.JS_RL_NKDestination = testHelper.CATOR.Code;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);

			AssertEquals(false, declaration.CA_PlaceOfReportInfo.ReadOnly);
			AssertEquals(ZString.Empty, declaration.CA_PlaceOfReport);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals(false, declaration.CA_PlaceOfReportInfo.ReadOnly);
			AssertEquals(testHelper.CATOR.Code, exportTransport.JW_RL_NKLoadPort);
			AssertEquals(officeCATOR.ZZD_Code, declaration.CA_PlaceOfReport);

			declaration.CA_PlaceOfReport = ZString.Empty;
			exportTransport.JW_RL_NKLoadPort = testHelper.CAVAR.Code;
			AssertEquals(officeCAVAR.ZZD_Code, declaration.CA_PlaceOfReport);

			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			Enterprise.Freight.Business.ChildEditableService.SetState(Factory, Enterprise.Freight.Integration.ChildEditableServiceStates.Shipment);
			shipment.Consols.RemoveAndDeleteAll();
			declaration.CA_PlaceOfReport = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = testHelper.CATOR.Code;
			AssertEquals(officeCATOR.ZZD_Code, declaration.CA_PlaceOfReport);

			declaration.CA_PlaceOfReport = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = testHelper.AUSYD.Code;
			AssertEquals(ZString.Empty, declaration.CA_PlaceOfReport);

			declaration.CA_PlaceOfReport = ZString.Empty;
			CACustomsDataRegistry.Instance.DefaultPlaceOfReport.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0818");
			declaration.JE_RL_NKPortOfLoading = testHelper.CATOR.Code;
			AssertEquals("0818", declaration.CA_PlaceOfReport);

			declaration.CA_PlaceOfReport = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = testHelper.AUSYD.Code;
			AssertEquals("0818", declaration.CA_PlaceOfReport);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_PlaceOfReport = "0010";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Place of report is not lost when import job syncronised", "0010", declaration.CA_PlaceOfReport);
		}

		public void TestJE_OH_ForwarderIsSetFromCurrentBranchOrgPK()
		{
			var org = GlbBranch.CurrentBranch.OrgProxy;
			org.OH_IsForwarder = ZBool.True;
			declaration.JE_OH_Forwarder = ZGuid.Empty;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(org.PK, declaration.JE_OH_Forwarder);

			declaration.JE_OH_Forwarder = ZGuid.NewZGuid();
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertNotEquals(org.PK, declaration.JE_OH_Forwarder);

			org.OH_IsForwarder = ZBool.False;
			declaration.JE_OH_Forwarder = ZGuid.Empty;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(ZGuid.Empty, declaration.JE_OH_Forwarder);
		}

		public void TestTransportDocumentNumber()
		{
			SetTransportMode(Core.Constants.TransportModes.Air);
			consol.JK_BookingReference = "BK123";
			consol.JK_MasterBillNum = "MWB123";
			shipment.JS_HouseBill = "HWB123";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("MWB-123", declaration.CA_TransportDocumentNumber);

			consol.JK_MasterBillNum = "MWB321";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("MWB-123", declaration.CA_TransportDocumentNumber);
		}

		public void TestTransportModeFormatting_RoadAndRail()
		{
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			SetTransportMode(Core.Constants.TransportModes.Road);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Road: FCL -> CNT", "CNT", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Road: LCL -> CNT", "CNT", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("Road: BCN -> CNT", "CNT", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FTL;
			AssertEquals("Road: FTL -> BBK", "BBK", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LTL;
			AssertEquals("Road: LTL -> BBK", "BBK", declaration.JE_ContainerMode);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Rail: FCL -> CNT", "CNT", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Rail: LCL -> CNT", "CNT", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("Rail: BCN -> CNT", "CNT", declaration.JE_ContainerMode);
		}

		public override void TestSeaTransportModeFormattingForImport()
		{
			shipment.ConsigneePK = testHelper.Consignor.PK;
			shipment.ConsignorPK = testHelper.Consignee.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			SetTransportMode(Core.Constants.TransportModes.Sea);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("Sea: BLK -> BLK", "BLK", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("Sea: LQD -> LQD", "LQD", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Sea: BBK -> BBK", "BBK", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("Sea: ROR -> ROR", "ROR", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("Sea: FCL -> CNT", "CNT", declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("Sea: LCL -> CNT", "CNT", declaration.JE_ContainerMode);

			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Loose;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.TransportModes.Air, declaration.JE_ContainerMode);
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.ULD;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.TransportModes.Air, declaration.JE_ContainerMode);
		}

		public void TestGetConvertedPackType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OuterPacks = 100;
			shipment.JS_F3_NKPackType = "PKG";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Pack UQ", "PK", declaration.JE_TotalNoOfPacksPackType);

			declaration.CA_ServiceOption = "";
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Pack UQ", "PKG", declaration.JE_TotalNoOfPacksPackType);
		}

		protected override void AssertDetectEnabledForUniversalXml(ZString messageType, ZString origin, ZString destination, ZString loadPort, ZString dischargePort, ZString firstArrival, ZBool overrideFreightDefaults)
		{
			SetupRegistriesForDetechEnabledForUniversalXml(messageType);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = "BCN";
			shipment.JS_HouseBill = "HS12345678";
			var number = shipment.Numbers.AddNew();
			number.CE_EntryType = "CCN";
			number.CE_EntryNum = "CCN12345678";
			var consol = shipment.Consols.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			declaration.JE_MessageType = messageType;
			declaration.JE_OverrideFreightDefaults = false;
			SetupForDetection(shipment, origin, destination);
			SetupForDetection(consol, loadPort, dischargePort, firstArrival);
			SetupPackingDetail(shipment, consol);
			declaration.ShipmentSynchroniser.Synchronise(true);
			Factory.Save();
			declaration.ShipmentSynchroniser.DetectEnabled = true;
			AssertEquals("declaration.ShipmentSynchroniser.SyncChangesDetected", false, declaration.ShipmentSynchroniser.SyncChangesDetected);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("No changes should be detected", false, declaration.ShipmentSynchroniser.SyncChangesDetected);
			declaration.ShipmentSynchroniser.SetEnabled(false, true);
			declaration.JE_JS = ZGuid.Empty;
			declaration.Delete();
			Factory.Save();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			declaration.JE_MessageType = messageType;
			var ccn = declaration.CargoControlNumbers.AddNew();
			ccn.CY_CargoControlNumber = "CCN12345678";
			ClearDataForSynchronisationDetection(declaration);
			((IBusinessObjectInternals)declaration).Row[JobDeclaration.Schema.JE_OverrideFreightDefaults] = false;
			Factory.Save();
			declaration.ShipmentSynchroniser.DetectEnabled = true;
			AssertEquals("declaration.ShipmentSynchroniser.SyncChangesDetected", false, declaration.ShipmentSynchroniser.SyncChangesDetected);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Changes should be detected", true, declaration.ShipmentSynchroniser.SyncChangesDetected);
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			using (SynchroniserDetectionHelper.SetupEnableDetectionForTesting())
			{
				declaration.ShipmentSynchroniser.Synchronise(true);
				var unchangedData = SynchroniserDetectionHelper.GetInfosNotChanged(ShouldIgnoreInfoForDetection);
				if (!unchangedData.IsEmpty)
				{
					Fail("Test data is required for the following synchronise fields:\r\n" + unchangedData);
				}
				var missingData = SynchroniserDetectionHelper.GetSynchroniserWithData();
				if (!missingData.IsEmpty)
				{
					Fail("Test data is required for the following synchronisers:\r\n" + missingData);
				}
			}
			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			Factory.Save();
			var message = GetQueuedUniversalShipmentMessage(shipment);
			message.EM_MessageText = message.EM_MessageText.
						Replace(consol.JK_UniqueConsignRef, "").
						Replace(consol.JK_UniqueConsignRef, "").
						Replace(shipment.JS_UniqueConsignRef, "").
						Replace(declaration.JE_DeclarationReference, "").
						Replace("DataSource", "DataTarget").
						Replace("1130", "1140");
			Factory.Save();
			var logger = new ServiceTaskLogForTesting();
			ProcessUniversalMessage(message, logger);
			var newShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, shipment.JS_HouseBill.Replace("1130", "1140")));
			var newDeclaration = (JobDeclaration)newShipment.DeclarationForDocuments;
			AssertEquals("newDeclaration.JE_OverrideFreightDefaults", overrideFreightDefaults, newDeclaration.JE_OverrideFreightDefaults);
		}

		void SetTransportMode(ZString transportMode)
		{
			consol.JK_TransportMode = transportMode;
			shipment.JS_TransportMode = transportMode;
		}

		protected override bool ShouldIgnoreInfoForDetection(ZPropertyInfo info)
		{
			switch (info.Name)
			{
				case JobDeclaration.Schema.CA_PortOfExit:
				case JobDeclaration.Schema.CA_PlaceOfReport:
					var declarationInfo = info.BizObj as AddInfoJobDeclaration;
					if (declarationInfo != null)
					{
						var declaration = declarationInfo.Parent;
						return declaration != null && !declaration.IsExport;
					}
					return false;
				case CargoControlNumber.Schema.CY_CargoControlNumber:
					return true;
				default:
					return base.ShouldIgnoreInfoForDetection(info);
			}
		}

		protected override void ClearDataForSynchronisationDetection(BaseJobDeclaration declaration)
		{
			base.ClearDataForSynchronisationDetection(declaration);
			var caDeclaration = (JobDeclaration)declaration;
			caDeclaration.CA_PortOfExit = ZString.Empty;
			caDeclaration.CA_PlaceOfReport = ZString.Empty;
		}

		protected override void SetupRegistriesForDetechEnabledForUniversalXml(ZString messageType)
		{
			base.SetupRegistriesForDetechEnabledForUniversalXml(messageType);
			if (messageType == JobMessageTypeList.Codes.Export)
			{
				CACustomsDataRegistry.Instance.DefaultPlaceOfReport.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0497");
			}
		}

		protected override void SetUp()
		{
			testHelper = new DeclarationTestHelper(Factory, false);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.CustomsOffice, "CustomsOffice");

			officeCATOR = helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Canada, RefCusCodeListTypes.CustomsOffice, "0001", testHelper.CATOR.RL_PortName, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCATOR.PK, RefCusCodeListAttributes.Province, testHelper.CATOR.CountryStates.RW_Code);

			officeCAVAR = helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Canada, RefCusCodeListTypes.CustomsOffice, "0002", testHelper.CAVAR.RL_PortName, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCAVAR.PK, RefCusCodeListAttributes.Province, testHelper.CAVAR.CountryStates.RW_Code);

			Factory.Save();

			base.SetUp();

			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
			
			declaration = Factory.New<JobDeclaration>();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = testHelper.CATOR.Code;
			consol.JK_RL_NKDischargePort = testHelper.AUSYD.Code;
			shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = testHelper.Consignee.PK;
			shipment.ConsignorPK = testHelper.Consignor.PK;
			declaration.JE_JS = shipment.PK;
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		new JobDeclaration declaration;
		ForwardingConsol consol;
		ForwardingShipment shipment;
		DeclarationTestHelper testHelper;
		IDisposable asecSetup;
		RefCusCodeList officeCATOR;
		RefCusCodeList officeCAVAR;
	}
}
