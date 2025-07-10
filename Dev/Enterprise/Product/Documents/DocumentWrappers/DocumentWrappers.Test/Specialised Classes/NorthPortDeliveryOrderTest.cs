using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using FreightContainer = Enterprise.Freight.Business.CommonContainer;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(NorthPortDeliveryOrder))]
	sealed class NorthPortDeliveryOrderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMarksAndNumbers()
		{
			AssertEquals("Marks and numbers is empty", ZString.Empty, NPWrapper.MarksAndNumbers);

			var marksAndNumberNote1 = Shipment.Notes.AddNew();
			marksAndNumberNote1.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumberNote1.ST_ParentID = Shipment.PK;
			marksAndNumberNote1.ST_Table = Shipment.TableName;
			marksAndNumberNote1.ST_NoteDataAsText = "Marks and numbers Line One\nLine Two";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumberHeight, 4);
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			AssertEquals("Marks and numbers", "Marks and numbers\nLine One\nLine Two", NPWrapper.MarksAndNumbers);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumberHeight, 1);
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			AssertEquals("Marks and numbers", "Please see attached", NPWrapper.MarksAndNumbers);
		}

		public void TestGoodsDescription()
		{
			AssertEquals("Goods Description is empty", ZString.Empty, NPWrapper.GoodsDescription);

			var detailedDescription = Shipment.Notes.AddNew();
			detailedDescription.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			detailedDescription.ST_ParentID = Shipment.PK;
			detailedDescription.ST_Table = Shipment.TableName;
			detailedDescription.ST_NoteDataAsText = "GOODS DESCRIPTIONS THAT EXCEEDS";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionHeight, 4);
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			AssertEquals("GoodsDescriptionHeight", "GOODS DESCRIPTIONS\nTHAT EXCEEDS", NPWrapper.GoodsDescription);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionHeight, 1);
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			AssertEquals("GoodsDescriptionHeight", "Please see attached", NPWrapper.GoodsDescription);
		}

		#region Package Count

		public void TestGetPackageCountNonMasterBOL()
		{
			Shipment.JS_OuterPacks = 0;
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count", ZString.Empty, bOL.GetPackageCountTestMethod());

			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = ZString.Empty;
			AssertEquals("Package count with no UQ", "120\r\n", bOL.GetPackageCountTestMethod());

			Shipment.JS_F3_NKPackType = "PLT";
			AssertEquals("Package count with UQ", "120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			var shipPackLine = (PackLine)Shipment.OuterPackLines[0];
			shipPackLine.JL_PackageCount = 120;
			FreightContainer container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			shipPackLine.SetContainer(Consol, container);
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 200;
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 100;
			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 100 Pallet(s)\r\n and 20 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());

			var shipPackLine2 = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine2.JL_PackageCount = 120;
			FreightContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			shipPackLine2.SetContainer(Consol, container2);
			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 100 Pallet(s)\r\n and 20 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());
		}

		public void TestGetPackageCountForMasterBOLForOneSubShipment()
		{
			Shipment.JS_OuterPacks = 0;
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count", ZString.Empty, bOL.GetPackageCountTestMethod());

			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = ZString.Empty;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with no UQ", "120\r\n", bOL.GetPackageCountTestMethod());

			Shipment.JS_F3_NKPackType = "PLT";
			AssertEquals("Package count with UQ", "120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = "PLT";

			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			FreightContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			container1.JC_ContainerNum = "Container 1";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			ForwardingShipment subShipment1 = Consol.Shipments.AddNew();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 120;
			shipPackLine.SetContainer(Consol, container1);
			subShipment1.UpdateShipmentFromOuterPackLines();
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 200;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 100;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 100 Pallet(s)\r\n and 20 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());

			var shipPackLine2 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine2.JL_PackageCount = 120;
			FreightContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			shipPackLine2.SetContainer(Consol, container2);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 100 Pallet(s)\r\n and 20 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());
		}

		public void TestGetPackageCountForMasterBOLForTwoSubShipments()
		{
			Shipment.JS_OuterPacks = 0;
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count", ZString.Empty, bOL.GetPackageCountTestMethod());

			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = ZString.Empty;
			AssertEquals("Package count with no UQ", "120\r\n", bOL.GetPackageCountTestMethod());

			Shipment.JS_F3_NKPackType = "PLT";
			AssertEquals("Package count with UQ", "120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = "PLT";

			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			FreightContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			container1.JC_ContainerNum = "Container 1";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			Shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.CoLoadMaster;

			ForwardingShipment subShipment1 = Consol.Shipments.AddNew();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 100;
			shipPackLine.SetContainer(Consol, container1);

			ForwardingShipment subShipment2 = Consol.Shipments.AddNew();
			subShipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine2 = (PackLine)subShipment2.OuterPackLines.AddNew();
			shipPackLine2.JL_PackageCount = 20;
			shipPackLine2.SetContainer(Consol, container1);

			subShipment1.UpdateShipmentFromOuterPackLines();
			subShipment2.UpdateShipmentFromOuterPackLines();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 200;
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 50;
			AssertEquals("Package count with UQ", "STC 70 Pallet(s)\r\n and 50 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());

			var shipPackLine3 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine3.JL_PackageCount = 120;
			FreightContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			shipPackLine3.SetContainer(Consol, container2);
			AssertEquals("Package count with UQ", "STC 70 Pallet(s)\r\n and 50 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());

			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Package count with UQ", "120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());
		}

		#endregion

		public void TestWeight()
		{
			AssertEquals("Weight", ZString.Empty, NPWrapper.Weight);

			Shipment.JS_ActualWeight = 123.00M;
			Shipment.JS_UnitOfWeight = "KG";
			Assert("Weight is not empty", !NPWrapper.Weight.IsEmpty);

			Shipment.JS_UnitOfWeight = "G";
			ZString expectedResult = NPWrapper.Weight;
			Assert("Weight is not empty", !expectedResult.IsEmpty);
			Assert("Weight has two lines", expectedResult.Split('\n').Length > 2);
		}

		public void TestVolume()
		{
			AssertEquals("Volume", ZString.Empty, NPWrapper.Volume);

			Shipment.JS_ActualVolume = 234.23M;
			Shipment.JS_UnitOfVolume = "M3";
			Assert("Volume is not empty", !NPWrapper.Volume.IsEmpty);

			Shipment.JS_UnitOfVolume = "CF";
			ZString expectedResult = NPWrapper.Volume;
			Assert("Volume is not empty", !expectedResult.IsEmpty);
			Assert("Volume has two lines", expectedResult.Split('\n').Length > 2);
		}

		public void TestContainerNumAndSize()
		{
			AssertEquals("Container Number and Size is empty", ZString.Empty, NPWrapper.ContainerNumAndSize);

			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			ForwardingConsol arrivalConsol = Shipment.Consols.AddNew();
			Transport arrivalTransport = arrivalConsol.Transports[0];
			arrivalTransport.JW_RL_NKLoadPort = "SGSIN";
			arrivalTransport.JW_RL_NKDiscPort = "CNSHA";

			FreightContainer arrivalContainer1 = arrivalConsol.Containers.AddNew();
			arrivalContainer1.JC_ContainerNum = "CONTAINER";
			packLine1.SetContainer(arrivalConsol, arrivalContainer1);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			AssertEquals("Container Number and Size", "CONTAINER", NPWrapper.ContainerNumAndSize);

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery());
			arrivalContainer1.JC_RC = containerCode.PK;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			AssertEquals("Container Number and Size", "CONTAINER" + "/" + containerCode.RC_Code, NPWrapper.ContainerNumAndSize);

			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			FreightContainer arrivalContainer2 = arrivalConsol.Containers.AddNew();
			arrivalContainer2.JC_ContainerNum = "CONTAINER2";
			packLine2.SetContainer(arrivalConsol, arrivalContainer2);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			AssertEquals("Container Number and Size", "Please see attached", NPWrapper.ContainerNumAndSize);
		}

		public void TestDateOfArrival()
		{
			AssertEquals("DateOfArrival is empty", ZDateTime.Empty, NPWrapper.DateOfArrival);

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			Shipment.Consols.RemoveAll();
			ForwardingConsol arrivalConsol = Shipment.Consols.AddNew();
			Transport transport = arrivalConsol.Transports[0];

			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "CNSHA";
			AssertEquals("DateOfArrival is empty", ZDateTime.Empty, NPWrapper.DateOfArrival);

			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			transport.JW_ATA = ZDateTime.Empty;
			transport.JW_ETA = DateTime.Today;
			AssertEquals("DateOfArrival", arrivalConsol.JK_JX_JB_E_ARV, NPWrapper.DateOfArrival);

			transport.JW_ATA = DateTime.Today.AddDays(3);
			AssertEquals("DateOfArrival", arrivalConsol.JK_JX_JB_A_ARV, NPWrapper.DateOfArrival);
		}

		public void TestFreightCharge()
		{
			AssertEquals("Freight Charge is empty", ZString.Empty, NPWrapper.FreightCharge);
		}

		public void TestWarehouseNumber()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Malaysia);
			try
			{
				DocForwardingConsol docForwardingConsol = DocForwardingConsol.New(Factory, Consol.PK);
				Transport transport = Consol.Transports[0];

				OrgHeader consolUnpackDepot = CreateDepotOrgWithWarehouseNum("ConsolUnpack");
				OrgHeader consolPackDepot = CreateDepotOrgWithWarehouseNum("ConsolPack");
				OrgHeader shipmentUnpackDepot = CreateDepotOrgWithWarehouseNum("ShipmentUnpack");
				OrgHeader shipmentPackDepot = CreateDepotOrgWithWarehouseNum("ShipmentPack");

				Consol.JK_RL_NKDischargePort = "MYPKG";
				Consol.JK_RL_NKLoadPort = "AUMEL";
				Consol.JK_OA_UnpackDepotAddress = consolUnpackDepot.MainAddress.PK;
				AssertEquals("Consol should be import for the test", true, docForwardingConsol.IsImportConsol);
				AssertEquals("Consol should be import for the test", false, docForwardingConsol.IsExportConsol);
				AssertEquals("Unpack warehouse on consol returned for import", "ConsolUnpack", NPWrapper.WarehouseNumber);

				Consol.JK_RL_NKDischargePort = "AUMEL";
				Consol.JK_RL_NKLoadPort = "MYPKG";
				Consol.JK_OA_PackDepotAddress = consolPackDepot.MainAddress.PK;
				AssertEquals("Consol should be export for the test", false, docForwardingConsol.IsImportConsol);
				AssertEquals("Consol should be export for the test", true, docForwardingConsol.IsExportConsol);
				AssertEquals("Pack warehouse on consol returned for export", "ConsolPack", NPWrapper.WarehouseNumber);

				Consol.JK_RL_NKDischargePort = "MYPKG";
				Consol.JK_RL_NKLoadPort = "AUMEL";
				Shipment.JS_OA_ImportReleaseDepot = shipmentUnpackDepot.MainAddress.PK;
				AssertEquals("Consol should be import for the test", true, docForwardingConsol.IsImportConsol);
				AssertEquals("Consol should be import for the test", false, docForwardingConsol.IsExportConsol);
				AssertEquals("Unpack warehouse on shipment returned for import", "ShipmentUnpack", NPWrapper.WarehouseNumber);

				Consol.JK_RL_NKDischargePort = "AUMEL";
				Consol.JK_RL_NKLoadPort = "MYPKG";
				Shipment.JS_OA_ExportReceivingDepot = shipmentPackDepot.MainAddress.PK;
				AssertEquals("Consol should be export for the test", false, docForwardingConsol.IsImportConsol);
				AssertEquals("Consol should be export for the test", true, docForwardingConsol.IsExportConsol);
				AssertEquals("Pack warehouse on shipment returned for export", "ShipmentPack", NPWrapper.WarehouseNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		OrgHeader CreateDepotOrgWithWarehouseNum(ZString warehouseNum)
		{
			var depot = Factory.New<OrgHeader>();
			depot.OH_FullName = "Depot";

			OrgCusCode warehouseNumCode = depot.CustomsCodes.AddNew();
			warehouseNumCode.OK_CodeType = MalaysiaOrgCusCodeInfo.OrgCusCodes.CFSBondedPackUnpack;
			warehouseNumCode.OK_CustomsRegNo = warehouseNum;
			warehouseNumCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Malaysia;

			return depot;
		}

		public void TestPortOperatorAndSCN()
		{
			EnsureJobSailingAttachedToConsol();
			OrgAddress address = AddAddressAndCustomsCodes();
			Consol.JK_OA_ArrivalCTOAddress = address.PK;
			RefCountry malaysia = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Malaysia);
			OrgCusCode portOperatorCode = address.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.ControlledPremisesID, malaysia);

			Consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			portOperatorCode.OK_CustomsRegNo = "KCT";
			Consol.Schedule.Destination.JB_ArrivalReference = "ArrivalSCN";

			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			AssertEquals("KCT:ArrivalSCN", NPWrapper.PortOperatorAndSCN);
		}

		void EnsureJobSailingAttachedToConsol()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "MYPKG";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "Voyage";

			AssertNotNull("Sailing should exist on the consol now for test", Consol.Schedule);
		}

		public void TestCCC()
		{
			OrgAddress address = AddAddressAndCustomsCodes();
			Consol.SetDefaultShippingLineAddress(address.Header);
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			AssertEquals("CCC", NPWrapper.CCC);
		}

		public void TestExchangeRates()
		{
			AssertEquals("No exchange rates", "", NPWrapper.ExchangeRates);

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = Shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_JobNum = "S99991999";
			AssertEquals("No exchange rates yet", "", NPWrapper.ExchangeRates);

			var rate1 = job.ExchangeRates.AddNew();
			var rate2 = job.ExchangeRates.AddNew();
			var rate3 = job.ExchangeRates.AddNew();

			ZString currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			rate1.JF_RX_NKRateCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			rate1.JF_BaseRate = 0.888M;
			rate2.JF_RX_NKRateCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			rate2.JF_BaseRate = 0.123M;
			rate3.JF_RX_NKRateCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			rate3.JF_BaseRate = 0.900M;
			AssertEquals("Exchange rates", currency + " 0.888, " + currency + " 0.123, " + currency + " 0.9", NPWrapper.ExchangeRates);
		}

		#region NorthPort Container Shipping Note
		public void TestExportTranshipmentFromExportConsol()
		{
			ZString branchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "MYPKG";

			ShipmentWrapper.SetDocumentDirectionForTesting("ARV");
			AssertEquals("Vessel is blank", "", NPWrapper.ExportTranshipmentVessel);
			AssertEquals("VoyageFlight no is blank", "", NPWrapper.ExportTranshipmentVoyage);
			AssertNull("Port of Loading is null", NPWrapper.ExportTranshipmentPortOfLoading);
			AssertNull("Port of Discharge is null", NPWrapper.ExportTranshipmentPortOfDischarge);

			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "NZAKL";

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "MYPEN";
			transport.JW_Vessel = "IMPORT VESSEL";
			transport.JW_VoyageFlight = "123";

			var consol2 = Shipment.Consols.AddNew();

			Transport transport2 = consol2.Transports[0];
			transport2.JW_RL_NKLoadPort = "MYPEN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "EXPORT VESSEL";
			transport2.JW_VoyageFlight = "321";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting("ARV");
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			AssertEquals("Vessel", "EXPORT VESSEL", NPWrapper.ExportTranshipmentVessel);
			AssertEquals("Voyage", "321", NPWrapper.ExportTranshipmentVoyage);
			AssertEquals("Port of Loading is MYPEN", "MYPEN", NPWrapper.ExportTranshipmentPortOfLoading.Code);
			AssertEquals("Port of Discharge is NZAKL", "NZAKL", NPWrapper.ExportTranshipmentPortOfDischarge.Code);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = branchPort;
		}

		public void TestExportTranshipmentFromTranshipPlan()
		{
			ShipmentWrapper.SetTemplateConstants(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection, "ARV" }, { DocumentEngineIntegration.Constants.TemplateDefined.ContactType, ContactType.Consignor.Code } });
			ZString branchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "MYPKG";

			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "NZAKL";

			Consol.JK_RL_NKLoadPort = "USLAX";
			Consol.JK_RL_NKDischargePort = "MYPEN";

			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = "IMPORT VESSEL";
			transport.JW_VoyageFlight = "123";

			AssertEquals("Vessel is blank", "", NPWrapper.ExportTranshipmentVessel);
			AssertEquals("VoyageFlight no is blank", "", NPWrapper.ExportTranshipmentVoyage);
			AssertNull("Port of Loading is null", NPWrapper.ExportTranshipmentPortOfLoading);
			AssertNull("Port of Discharge is null", NPWrapper.ExportTranshipmentPortOfDischarge);

			Transport transport1 = Shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "MYPEN";
			transport1.JW_RL_NKDiscPort = "NZCHC";
			transport1.JW_Vessel = "VESSEL 123";
			transport1.JW_VoyageFlight = "123";

			Transport transport2 = Shipment.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "NZAAA";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "VESSEL 321";
			transport2.JW_VoyageFlight = "321";

			AssertEquals("Vessel", "VESSEL 123", NPWrapper.ExportTranshipmentVessel);
			AssertEquals("VoyageFlight", "123", NPWrapper.ExportTranshipmentVoyage);
			AssertEquals("Port of Loading is MYPEN", "MYPEN", NPWrapper.ExportTranshipmentPortOfLoading.Code);
			AssertEquals("Port of Discharge is NZCHC", "NZCHC", NPWrapper.ExportTranshipmentPortOfDischarge.Code);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = branchPort;
		}

		public void TestImportTranshipmentDetails()
		{
			ShipmentWrapper.SetDocumentDirectionForTesting("ARV");
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			Shipment.Consols.RemoveAndDeleteAll();
			AssertEquals("Empty import details", "", NPWrapper.ImportTranshipmentDetails);
			ZString branchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "MYPKG";

			Shipment.JS_OuterPacks = 10;
			Shipment.JS_F3_NKPackType = "PLT";
			Shipment.JS_GoodsDescription = "Goods Descriptions";
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "NZAKL";

			Consol = Shipment.Consols.AddNew();
			Consol.JK_RL_NKLoadPort = "USLAX";
			Consol.JK_RL_NKDischargePort = "MYPEN";
			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = "EXPORT VESSEL";
			transport.JW_VoyageFlight = "123";

			var consol2 = Shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "MYPEN";
			consol2.JK_RL_NKDischargePort = "NZAKL";
			Transport transport2 = consol2.Transports[0];
			transport2.JW_Vessel = "IMPORT VESSEL";
			transport2.JW_VoyageFlight = "321";

			ZString importDetails = "T/S EX. IMPORT VESSEL V321\n";
			ZString shipmentDetails = "10 Pallet(s)\r\nGoods Descriptions\n\n";
			AssertEquals("Import Consol details", importDetails, NPWrapper.ImportTranshipmentDetails);
			AssertEquals("Transhipment Goods Details", shipmentDetails + importDetails, NPWrapper.TranshipmentGoodsDetails);

			Shipment.JS_HouseBill = "HBL000";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting("ARV");
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);
			importDetails = "T/S EX. IMPORT VESSEL V321\nBill Of Lading No. HBL000\n";

			AssertEquals("Import Consol details", importDetails, NPWrapper.ImportTranshipmentDetails);
			AssertEquals("Transhipment Goods Details", shipmentDetails + importDetails, NPWrapper.TranshipmentGoodsDetails);

			var consolContainer1 = (FreightContainer)Consol.Containers.AddNew();
			consolContainer1.JC_ContainerNum = "CONT1";
			var consolContainer2 = (FreightContainer)Consol.Containers.AddNew();
			consolContainer2.JC_ContainerNum = "CONT2";

			var consol2Container1 = (FreightContainer)consol2.Containers.AddNew();
			consol2Container1.JC_ContainerNum = "CONT3";
			var consol2Container2 = (FreightContainer)consol2.Containers.AddNew();
			consol2Container2.JC_ContainerNum = "CONT4";

			var line1 = (PackLine)Shipment.OuterPackLines.AddNew();
			var line2 = (PackLine)Shipment.OuterPackLines.AddNew();

			line1.SetContainer(Consol, consolContainer1);
			line2.SetContainer(Consol, consolContainer2);
			line1.SetContainer(consol2, consol2Container1);
			line2.SetContainer(consol2, consol2Container2);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting("ARV");
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);

			importDetails = "T/S EX. IMPORT VESSEL V321\nBill Of Lading No. HBL000\nContainer No. CONT3, CONT4";
			AssertEquals("Import Consol details", "T/S EX. IMPORT VESSEL V321\nBill Of Lading No. HBL000\nContainer No. CONT3, CONT4", NPWrapper.ImportTranshipmentDetails);
			AssertEquals("Transhipment Goods Details", shipmentDetails + importDetails, NPWrapper.TranshipmentGoodsDetails);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = branchPort;
		}
		#endregion

		#region Implementation

		ForwardingShipment Shipment;
		DocForwardingShipment ShipmentWrapper;
		ForwardingConsol Consol;
		NorthPortDeliveryOrder NPWrapper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NorthPortDeliveryOrder(ShipmentWrapper);
		}

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			Shipment = Factory.New<ForwardingShipment>();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			Consol = Shipment.Consols.AddNew();
			NPWrapper = new NorthPortDeliveryOrder(ShipmentWrapper);

			base.SetUp();
		}

		OrgAddress AddAddressAndCustomsCodes()
		{
			var unpackDepot = Factory.New<OrgHeader>();
			var address = unpackDepot.Addresses.AddNew();
			var cCD = unpackDepot.CustomsCodes.AddNew();
			cCD.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cCD.OK_RN_NKCodeCountry = "AU";
			cCD.OK_CustomsRegNo = "CCP";

			var cSC = unpackDepot.CustomsCodes.AddNew();
			cSC.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			cSC.OK_RN_NKCodeCountry = "MY";
			cSC.OK_CustomsRegNo = "CSC";

			var cCP = unpackDepot.CustomsCodes.AddNew();
			cCP.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cCP.OK_RN_NKCodeCountry = "MY";
			cCP.OK_CustomsRegNo = "CCP";

			var cCC = unpackDepot.CustomsCodes.AddNew();
			cCC.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cCC.OK_RN_NKCodeCountry = "MY";
			cCC.OK_CustomsRegNo = "CCC";
			return address;
		}

		#endregion
	}
}
