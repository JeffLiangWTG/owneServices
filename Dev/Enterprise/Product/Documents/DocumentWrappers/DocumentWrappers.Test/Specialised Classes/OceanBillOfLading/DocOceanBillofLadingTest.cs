using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ChargesDisplayTypes = Enterprise.Freight.Agency.Business.OBLChargesDisplayMode.Codes;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocOceanBillOfLading))]
	sealed class DocOceanBillofLadingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBillClause()
		{
			OrgHeader principal1 = Factory.NewWithValidTestData<OrgHeader>();
			AgencyRegistry.Instance.BillOfLadingClause(principal1).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Blaticus1");

			Shipment.JS_OH_DeliveryAgent = principal1.PK;

			AssertEquals("Blaticus1", DocOBL.BillClause);

			DocumentNote note = DocumentNote.LoadNote(Shipment);
			note.SetSystemDefinedFieldValue("Bill Clause", "Override");
			AssertEquals("Override", DocOBL.BillClause);
		}

		public void TestDefaultBillClause()
		{
			OrgHeader principal1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader principal2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader principal3 = Factory.NewWithValidTestData<OrgHeader>();

			AgencyRegistry.Instance.BillOfLadingClause(principal1).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Blaticus1");
			AgencyRegistry.Instance.BillOfLadingClause(principal2).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Blaticus2");

			Shipment.JS_OH_DeliveryAgent = principal1.PK;
			AssertEquals("Blaticus1", DocOBL.DefaultBillClause);

			Shipment.JS_OH_DeliveryAgent = principal2.PK;
			AssertEquals("Blaticus2", DocOBL.DefaultBillClause);

			Shipment.JS_OH_DeliveryAgent = principal3.PK;
			AssertEquals("", DocOBL.DefaultBillClause);

			Shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			AssertEquals("", DocOBL.DefaultBillClause);
		}

		public void TestShipments_PackageCount()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines.AddNew();

			AssertEquals("FCL: PackageCount from OuterPackLines", "2", DocOBL.Shipments[0].PackageCount);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			Shipment.TopLevelPacks.AddNew();
			Shipment.TopLevelPacks.AddNew();
			Shipment.TopLevelPacks.AddNew();

			AssertEquals("TopLevelPacksMode: PackageCount from TopLevelPacks", "3", DocOBL.Shipments[0].PackageCount);
		}

		public void TestShipments_GoodDiscriptionMarksAndNumbers()
		{
			Shipment.JS_MarksAndNumbers = "Mark And Numbers";
			Shipment.DetailedGoodsDescriptionNoteText = "Detailed Goods Description NoteText";

			AssertEquals("MarksAndNumbers should be uppercase", "MARK AND NUMBERS", DocOBL.Shipments[0].MarksAndNumbers);
			AssertEquals("GoodsDescription should be uppercase", "DETAILED GOODS DESCRIPTION NOTETEXT", DocOBL.Shipments[0].GoodsDescription);
		}

		public void TestWeight()
		{
			shipment = null;
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			AgencyShipmentContainer container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "C1";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;

			AgencyShipmentPackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeight = 1050M;

			container.JC_Calc_NetWeight = 1150M;

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "C2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 3;
			packLine.JL_ActualWeight = 2050M;

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "C3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container.JC_IsEmptyContainer = ZBool.True;

			AssertEquals("Weight", "3200 KG", DocOBL.Weight);
			AssertEquals("GrossWeight", "12050 KG", DocOBL.GrossWeight);

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 3M;

			AssertEquals("Weight", "6200 KG", DocOBL.Weight);
			AssertEquals("GrossWeight", "15050 KG", DocOBL.GrossWeight);

			shipment = null;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeight = 2M;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 3;
			packLine.JL_ActualWeight = 3M;

			Shipment.UpdateShipmentFromOuterPackLines();

			AssertEquals("Weight", "5000 KG", DocOBL.Weight);
			AssertEquals("GrossWeight", "5000 KG", DocOBL.GrossWeight);

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 10M;

			AssertEquals("Weight", "5000 KG", DocOBL.Weight);
			AssertEquals("GrossWeight", "5000 KG", DocOBL.GrossWeight);

			Shipment.UpdateShipmentFromOuterPackLines();

			AssertEquals("Weight", "15000 KG", DocOBL.Weight);
			AssertEquals("GrossWeight", "15000 KG", DocOBL.GrossWeight);
		}

		public void TestWeight_Imperial()
		{
			shipment = null;
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;

			AgencyShipmentContainer container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "C1";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;

			AgencyShipmentPackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeight = 1050M;
			packLine.JL_ActualWeightUQ = "LB";

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "C2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 3;
			packLine.JL_ActualWeight = 2050M;
			packLine.JL_ActualWeightUQ = "KG";

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "C3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container.JC_IsEmptyContainer = ZBool.True;

			Constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.UseImperialUnits, "Y");
			ShipmentWrapper.SetTemplateConstants(Constants);

			AssertEquals("Weight", "5569.476 LB", DocOBL.Weight);
			AssertEquals("GrossWeight", "25080.387 LB", DocOBL.GrossWeight);
		}

		public void TestWeight_NoConversion()
		{
			shipment = null;
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;

			AgencyShipmentPackLine packLine0 = Shipment.OuterPackLines[0];
			packLine0.JL_ActualWeight = 100M;
			packLine0.JL_ActualWeightUQ = "LB";

			AgencyShipmentContainer container1 = Shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "C1";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container1.JC_GrossWeightUQ = "LB";

			AgencyShipmentPackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(container1.PK);
			packLine1.JL_PackageCount = 2;
			packLine1.JL_ActualWeight = 1050M;
			packLine1.JL_ActualWeightUQ = "LB";

			AgencyShipmentContainer container2 = Shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = "C2";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container2.JC_GrossWeightUQ = "LB";

			AgencyShipmentPackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(container2.PK);
			packLine2.JL_PackageCount = 3;
			packLine2.JL_ActualWeight = 2050M;
			packLine2.JL_ActualWeightUQ = "LB";

			AgencyShipmentContainer container3 = Shipment.RealContainers.AddNew();
			container3.JC_ContainerNum = "C3";
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container3.JC_IsEmptyContainer = ZBool.True;
			container3.JC_GrossWeightUQ = "LB";

			Constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ConvertUnits, "N");
			Constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.UseImperialUnits, "Y");
			ShipmentWrapper.SetTemplateConstants(Constants);

			AssertEquals("Weight - LBs", "3200 LB", DocOBL.Weight);
			AssertEquals("GrossWeight - LBs", "12050 LB", DocOBL.GrossWeight);

			container1.JC_GrossWeightUQ = "KG";
			container2.JC_GrossWeightUQ = "KG";
			container3.JC_GrossWeightUQ = "KG";

			packLine0.JL_ActualWeightUQ = "KG";
			packLine1.JL_ActualWeightUQ = "KG";
			packLine2.JL_ActualWeightUQ = "KG";

			AssertEquals("Weight - KG", "3200 KG", DocOBL.Weight);
			AssertEquals("GrossWeight - KG", "12050 KG", DocOBL.GrossWeight);

			container2.JC_GrossWeightUQ = "LB";

			AssertEquals("Weight - mixed units", "4585.316 LB", DocOBL.Weight);
			AssertEquals("GrossWeight - mixed units", "20542.589 LB", DocOBL.GrossWeight);

			shipment.JS_PackingMode = "LCL";
			shipment.JS_ActualWeight = 123.45M;
			shipment.JS_UnitOfWeight = "T";

			AssertEquals("Shipment - LCL", "123.45 T", DocOBL.Weight);
		}

		public void TestVolume()
		{
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			Shipment.JS_ActualVolume = 0.5;

			AssertEquals("Volume", "0.5 M3", DocOBL.Volume);

			Shipment.JS_UnitOfVolume = Core.Constants.Volume.Litre;
			Shipment.JS_ActualVolume = 15000;

			AssertEquals("Volume", "15 M3", DocOBL.Volume);

			Shipment.JS_ActualVolume = 0;
			AssertEquals("Empty Volume", ZString.Empty, DocOBL.Volume);
		}

		public void TestVolume_Imperial()
		{
			Constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.UseImperialUnits, "Y");
			ShipmentWrapper.SetTemplateConstants(Constants);

			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			Shipment.JS_ActualVolume = 0.5;

			AssertEquals("Volume", "17.657 CF", DocOBL.Volume);

			Shipment.JS_UnitOfVolume = Core.Constants.Volume.Litre;
			Shipment.JS_ActualVolume = 15000;

			AssertEquals("Volume", "529.72 CF", DocOBL.Volume);

			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			Shipment.JS_ActualVolume = 298;

			AssertEquals("Volume", "298 CF", DocOBL.Volume);

			Shipment.JS_ActualVolume = 0;
			AssertEquals("Empty Volume", ZString.Empty, DocOBL.Volume);
		}

		public void TestVolume_NoConversion()
		{
			Constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ConvertUnits, "N");
			ShipmentWrapper.SetTemplateConstants(Constants);

			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			Shipment.JS_ActualVolume = 0.5;

			AssertEquals("M3", "0.5 M3", DocOBL.Volume);

			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicYards;

			AssertEquals("CY", "0.5 CY", DocOBL.Volume);
		}

		public void TestIsSeaWaybill()
		{
			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.ExpressBofL;
			AssertEquals("ReleaseType", "Bill of Lading", DocOBL.Title);
			AssertEquals("IsSeaWaybill", false, DocOBL.IsSeaWaybill);

			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			AssertEquals("ReleaseType", "Sea Waybill", DocOBL.Title);
			AssertEquals("IsSeaWaybyll", true, DocOBL.IsSeaWaybill);
		}

		public void TestOceanBillNumber()
		{
			AssertEquals("", DocOBL.OceanBillNumber);

			Shipment.JS_HouseBill = "Blaticus";
			AssertEquals("BLATICUS", DocOBL.OceanBillNumber);
		}

		public void TestVesselNameVoyage()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Blaticus";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			Shipment.JS_JX = sailing.PK;

			AssertEquals("Vessel", "Blaticus", DocOBL.VesselName);
			AssertEquals("Voyage", "1234", DocOBL.VoyageNumber);
		}

		public void TestNotifyParty()
		{
			AssertEquals("Notify Party", "", DocOBL.NotifyParty);

			Env.Registry.NotifyPartyDefaultText = "Default Text";
			AssertEquals("Notify Party", "Default Text", DocOBL.NotifyParty);

			Shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			Shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = "CompanyName";
			Shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "Address1";
			Shipment.NotifyPartyDocumentaryAddress.E2_Address2 = "Address2";
			Shipment.NotifyPartyDocumentaryAddress.E2_City = "City";

			const string expectedText =
				"COMPANYNAME\n" +
				"ADDRESS1\n" +
				"ADDRESS2\n" +
				"CITY" +
				"\n";

			AssertMultilineASCIIEquals("Notify Party", expectedText + Env.CurrentCompany.Country.Description.ToUpper(), DocOBL.NotifyParty);
		}

		public void TestOriginAndDestination()
		{
			AssertEquals("Origin", null, DocOBL.OriginPort);
			AssertEquals("Destination", null, DocOBL.DestinationPort);

			Shipment.JS_RL_NKOrigin = "AUMEL";
			Shipment.JS_RL_NKDestination = "NLAMS";
			AssertEquals("Origin", "AUMEL", DocOBL.OriginPort.Code);
			AssertEquals("Destination", "NLAMS", DocOBL.DestinationPort.Code);
		}

		public void TestLoadAndDischarge()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Blaticus";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "GBLON";
			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];

			AssertEquals("Load", null, DocOBL.LoadPort);
			AssertEquals("Discharge", null, DocOBL.DischargePort);

			Shipment.JS_JX = sailing.PK;

			AssertEquals("Load", "AUBNE", DocOBL.LoadPort.Code);
			AssertEquals("Discharge", "GBLON", DocOBL.DischargePort.Code);
		}

		public void TestFreightPayableAtPort()
		{
			Shipment.JS_RL_NKOrigin = "AUCNS";
			Shipment.JS_RL_NKDestination = "NLAMS";

			AssertEquals("payable at", null, DocOBL.FreightPayableAtPort);

			Shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;
			AssertEquals("payable at", "AUCNS", DocOBL.FreightPayableAtPort.Code);

			Shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;
			AssertEquals("payable at", "NLAMS", DocOBL.FreightPayableAtPort.Code);
		}

		public void TestPuertoRicoTaxReleaseStatus()
		{
			Shipment.JS_RL_NKOrigin = "AUCNS";
			Shipment.JS_RL_NKDestination = "NLAMS";

			var container1 = Shipment.RealContainers.AddNew();
			var container2 = Shipment.RealContainers.AddNew();
			var container3 = Shipment.RealContainers.AddNew();

			AssertEquals(ZString.Empty, DocOBL.PuertoRicoTaxReleaseStatus);

			container1.JC_ContainerImportDORelease = "Not Rlsd";
			AssertEquals(ZString.Empty, DocOBL.PuertoRicoTaxReleaseStatus);

			container2.JC_ContainerImportDORelease = "Not Rlsd:1";
			container3.JC_ContainerImportDORelease = "NOT RLSD:2";
			AssertEquals("NOT RELEASED", DocOBL.PuertoRicoTaxReleaseStatus);

			container2.JC_ContainerImportDORelease = "RELEASED";
			AssertEquals(ZString.Empty, DocOBL.PuertoRicoTaxReleaseStatus);

			container1.JC_ContainerImportDORelease = "Released:121116";
			container3.JC_ContainerImportDORelease = "released";
			AssertEquals("RELEASED", DocOBL.PuertoRicoTaxReleaseStatus);
		}

		public void TestIncludeContainersInDetailSection()
		{
			Shipment.JS_MarksAndNumbers =
				"Marks and Numbers Line 1\n" +
				"Marks and Numbers Line 2\n" +
				"Marks and Numbers Line 3\n" +
				"";

			Shipment.DetailedGoodsDescriptionNoteText =
				"Goods Description Line 1\n" +
				"Goods Description Line 2\n" +
				"Goods Description Line 3\n" +
				"";

			AgencyShipmentContainer container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2200m;

			AgencyShipmentPackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500m;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 2.5m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 10000m;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 7.5m;

			container.JC_Calc_NetWeight = 15000m;

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 8;

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100032";
			container.JC_SealNum = "Seal3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_TareWeight = 2300m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 10m;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 777m;

			Shipment.UpdateShipmentFromOuterPackLines();

			Constants["MarksAndNumbersWidth"] = 20;
			Constants["MarksAndNumbersAndGoodsDescriptionGap"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GoodsDescriptionAndGrossWeightGap"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["GrossWeightAndMeasurementGap"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerNumberAndSealGap"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerSealAndTypeGap"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerTypeAndWeightGap"] = 1;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerWeightAndTareGap"] = 1;
			Constants["ContainerGrossWidth"] = 11;
			Constants["ContainerTareAndGrossGap"] = 1;
			Constants["ContainerTareWidth"] = 11;
			Constants["ContainerGrossAndVolumeGap"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerVolumeAndPackagesGap"] = 1;
			Constants["ContainerPackagesWidth"] = 11;

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 4;
			Constants["ShowDetailHeadingInMainBody"] = "Y";
			Constants["IncludeContainersInMarksAndNumbersSection"] = "Y";
			Constants["ShowContainerHeadingInMainBody"] = "Y";

			const string expectedDetailHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume\n" +
				"";

			const string expectedDetailBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"MARKS AND NUMBERS    GOODS DESCRIPTION        40000 KG    18.777 M3\n" +
				"LINE 1               LINE 1                                        \n" +
				"MARKS AND NUMBERS    GOODS DESCRIPTION                             \n" +
				"";

			const string expectedDetailBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"LINE 2               LINE 2                                        \n" +
				"MARKS AND NUMBERS    GOODS DESCRIPTION                             \n" +
				"LINE 3               LINE 3                                        \n" +
				"";

			const string expectedContainerHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Cn. No      Seal  Type        Net (kg)   Tare (kg)  Gross (kg) Volume (M3)       Packs\n" +
				"";

			const string expectedContainerBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"FAKE4100011 Seal  20GP           15000        2200       17200          10      24 PKG\n" +
				"FAKE4100027 Seal2 40RE           15000        3600       18600           8       8 PLT\n" +
				"";

			const string expectedContainerBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"FAKE4100032 Seal3 20RE               0        2300        2300                        \n" +
				"-           -     -              10000           -       10000       0.777      20 PKG\n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);

			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocOBL.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Container Heading", expectedContainerHeading, DocOBL.ContainersSectionHeader);

			AssertMultilineASCIIEquals("Detail Body",
				expectedDetailHeading +
				expectedDetailBody1,
				DocOBL.FormedPages[0].MainBodyDetailsSection);

			AssertMultilineASCIIEquals("Container Body", "", DocOBL.FormedPages[0].MainBodyContainersSection);

			AssertMultilineASCIIEquals("FollowOn",
				expectedDetailHeading +
				expectedDetailBody2 +
				"\n" +
				expectedContainerHeading +
				expectedContainerBody1 +
				expectedContainerBody2,
				ZString.Join("\n", DocOBL.FollowOnSection));

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 11;

			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocOBL.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Container Heading", expectedContainerHeading, DocOBL.ContainersSectionHeader);

			AssertMultilineASCIIEquals("Detail Body",
				expectedDetailHeading +
				expectedDetailBody1 +
				expectedDetailBody2 +
				"\n" +
				expectedContainerHeading +
				expectedContainerBody1,
				DocOBL.FormedPages[0].MainBodyDetailsSection);

			AssertMultilineASCIIEquals("Container Body", "", DocOBL.FormedPages[0].MainBodyContainersSection);

			AssertMultilineASCIIEquals("FollowOn",
				expectedContainerHeading +
				expectedContainerBody2,
				ZString.Join("\n", DocOBL.FollowOnSection));
		}

		public void TestIncludeExtraSectionInDetailSection()
		{
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Shipment.JS_RL_NKOrigin = "NLAMS";
			Shipment.JS_RL_NKDestination = "AUBNE";

			Shipment.JS_MarksAndNumbers =
				"Marks and Numbers Line 1\n" +
				"Marks and Numbers Line 2\n" +
				"Marks and Numbers Line 3\n" +
				"";

			Shipment.DetailedGoodsDescriptionNoteText =
				"Goods Description Line 1\n" +
				"Goods Description Line 2\n" +
				"Goods Description Line 3\n" +
				"";

			shipment.JS_ActualWeight = shipment.OuterPackLines.TotalWeight;
			shipment.JS_ActualVolume = shipment.OuterPackLines.TotalVolume;

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 4;
			Constants["MarksAndNumbersWidth"] = 20;
			Constants["MarksAndNumbersAndGoodsDescriptionGap"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GoodsDescriptionAndGrossWeightGap"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["GrossWeightAndMeasurementGap"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;
			Constants["ShowDetailHeadingInMainBody"] = "Y";
			Constants["IncludeExtraSectionInMarksAndNumbersSection"] = "Y";
			Constants["ShowExtraSectionHeadingInMainBody"] = "Y";

			Constants["NoOfExtraSectionRows"] = 6;
			Constants["ExtraSection-Count"] = 3;

			Constants["ExtraSection-Heading1"] = "Heading 1";
			Constants["ExtraSection-Path1"] = "Plain Text";
			Constants["ExtraSection-LeftPadding1"] = 0;
			Constants["ExtraSection-Width1"] = 10;

			Constants["ExtraSection-Heading2"] = "Heading Two";
			Constants["ExtraSection-Path2"] = "<BillOfLading.OriginPort.PortName> -> <BillOfLading.DestinationPort.PortName>";
			Constants["ExtraSection-LeftPadding2"] = 1;
			Constants["ExtraSection-Width2"] = 20;

			Constants["ExtraSection-Heading3"] = "Third Heading";
			Constants["ExtraSection-Path3"] = "Line1\r\nLine2\r\nLine3";
			Constants["ExtraSection-LeftPadding3"] = 1;
			Constants["ExtraSection-Width3"] = 15;

			const string expectedDetailHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume\n" +
				"";

			const string expectedDetailBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"MARKS AND NUMBERS    GOODS DESCRIPTION                             \n" +
				"LINE 1               LINE 1                                        \n" +
				"MARKS AND NUMBERS    GOODS DESCRIPTION                             \n" +
				"";

			const string expectedDetailBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"LINE 2               LINE 2                                        \n" +
				"MARKS AND NUMBERS    GOODS DESCRIPTION                             \n" +
				"LINE 3               LINE 3                                        \n" +
				"";

			const string expectedExtraSectionHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Heading 1  Heading Two          Third Heading  \n" +
				"";

			const string expectedExtraSectionBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Plain Text Amsterdam ->         Line1          \n" +
				"           Brisbane             Line2          \n" +
				"";

			const string expectedExtraSectionBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"                                Line3          \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);

			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocOBL.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Extra Section Heading", expectedExtraSectionHeading, DocOBL.ExtraSectionHeader);

			AssertMultilineASCIIEquals("Detail Body",
				expectedDetailHeading +
				expectedDetailBody1,
				DocOBL.FormedPages[0].MainBodyDetailsSection);

			AssertMultilineASCIIEquals("Extra Section Body", "", DocOBL.FormedPages[0].MainBodyExtraSection);

			AssertMultilineASCIIEquals("FollowOn",
				expectedDetailHeading +
				expectedDetailBody2 +
				"\n" +
				expectedExtraSectionHeading +
				expectedExtraSectionBody1 +
				expectedExtraSectionBody2,
				ZString.Join("\n", DocOBL.FollowOnSection));

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 11;

			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocOBL.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Extra Section Heading", expectedExtraSectionHeading, DocOBL.ExtraSectionHeader);

			AssertMultilineASCIIEquals("Detail Body",
				expectedDetailHeading +
				expectedDetailBody1 +
				expectedDetailBody2 +
				"\n" +
				expectedExtraSectionHeading +
				expectedExtraSectionBody1,
				DocOBL.FormedPages[0].MainBodyDetailsSection);

			AssertMultilineASCIIEquals("Container Body", "", DocOBL.FormedPages[0].MainBodyExtraSection);

			AssertMultilineASCIIEquals("FollowOn",
				expectedExtraSectionHeading +
				expectedExtraSectionBody2,
				ZString.Join("\n", DocOBL.FollowOnSection));
		}

		public void TestDetailsSection()
		{
			Shipment.JS_MarksAndNumbers =
				"Marks and Numbers Line 1\n" +
				"Marks and Numbers Line 2\n" +
				"Marks and Numbers Line 3\n" +
				"";

			Shipment.DetailedGoodsDescriptionNoteText =
				"Goods Description Line 1\n" +
				"Goods Description Line 2\n" +
				"Goods Description Line 3\n" +
				"";

			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			Shipment.JS_ActualWeight = 5.5m;

			Shipment.JS_UnitOfVolume = Core.Constants.Volume.Litre;
			Shipment.JS_ActualVolume = 2500;

			Constants["MarksAndNumbersLeftPadding"] = 0;
			Constants["MarksAndNumbersWidth"] = 20;
			Constants["GoodsDescLeftPadding"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GrossWeightLeftPadding"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["VolumeMeasurementLeftPadding"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 3;
			Constants["ShowDetailHeadingInMainBody"] = "N";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume\n" +
				"";

			const string expectedBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"MARKS AND NUMBERS    GOODS DESCRIPTION         5500 KG       2.5 M3\n" +
				"LINE 1               LINE 1                                        \n" +
				"MARKS AND NUMBERS    GOODS DESCRIPTION                             \n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"LINE 2               LINE 2                                        \n" +
				"MARKS AND NUMBERS    GOODS DESCRIPTION                             \n" +
				"LINE 3               LINE 3                                        \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody, DocOBL.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", true, DocOBL.HasFollowOnSection);

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 7;
			Constants["ShowDetailHeadingInMainBody"] = "Y";

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody + expectedFollowOn, DocOBL.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestContainersSection()
		{
			AgencyShipmentContainer container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2200;

			AgencyShipmentPackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 2.5;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 7.5;

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 8;

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100032";
			container.JC_SealNum = "Seal3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_TareWeight = 2300m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 111;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 888;

			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 1;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 11;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 11;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerPackagesLeftPadding"] = 1;
			Constants["ContainerPackagesWidth"] = 11;

			Constants["NumberOfContainerRows"] = 4;
			Constants["ShowContainerHeadingInMainBody"] = "Y";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Cn. No      Seal  Type        Net (kg)   Tare (kg)  Gross (kg) Volume (M3)       Packs\n" +
				"";

			const string expectedBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"FAKE4100011 Seal  20GP           11500        2200       13700          10      24 PKG\n" +
				"FAKE4100027 Seal2 40RE           15000        3600       18600           8       8 PLT\n" +
				"FAKE4100032 Seal3 20RE               0        2300        2300                        \n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"-           -     -             111000           -      111000       0.888       8 PKG\n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, DocOBL.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", true, DocOBL.HasFollowOnSection);

			Constants["ShowContainerHeadingInMainBody"] = "N";

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn, DocOBL.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestContainersSection_MultiPage()
		{
			AgencyShipmentContainer container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2200;

			AgencyShipmentPackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 2.5;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 7.5;

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 8;

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100032";
			container.JC_SealNum = "Seal3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_TareWeight = 2300m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 111;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 888;

			Constants["UseMultiPage"] = "Y";
			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 1;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 11;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 11;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerPackagesLeftPadding"] = 1;
			Constants["ContainerPackagesWidth"] = 11;

			Constants["NumberOfContainerRows"] = 4;
			Constants["ShowContainerHeadingInMainBody"] = "Y";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Cn. No      Seal  Type        Net (kg)   Tare (kg)  Gross (kg) Volume (M3)       Packs\n" +
				"";

			const string expectedBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"FAKE4100011 Seal  20GP           11500        2200       13700          10      24 PKG\n" +
				"FAKE4100027 Seal2 40RE           15000        3600       18600           8       8 PLT\n" +
				"FAKE4100032 Seal3 20RE               0        2300        2300                        \n" +
				"";

			const string expectedBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"-           -     -             111000           -      111000       0.888       8 PKG\n" +
				"";

			AssertEquals("should have 2 formed pages", 2, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Body1", expectedHeading + expectedBody1, DocOBL.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("Body2", expectedHeading + expectedBody2, DocOBL.FormedPages[1].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestContainersSection_Interleave()
		{
			AgencyShipmentContainer container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2200;

			AgencyShipmentPackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 1";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 2.5;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0440", "", "IMO").First().PK;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2305", "", "IMO").First().PK;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 2";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 7.5;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0138", "", "IMO").First().PK;

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 3";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 8;

			container = Shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100032";
			container.JC_SealNum = "Seal3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_TareWeight = 2300m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = "Detailed Description 4";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 111;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 888;

			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 1;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 11;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 11;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerPackagesLeftPadding"] = 1;
			Constants["ContainerPackagesWidth"] = 11;

			Constants["PackRefNumberColumnWidth"] = 0;
			Constants["PackLengthColumnWidth"] = 0;
			Constants["PackWidthColumnWidth"] = 0;
			Constants["PackHeightColumnWidth"] = 0;
			Constants["PackAreaColumnWidth"] = 0;

			Constants["PackDescriptionColumnLeftPadding"] = 3;
			Constants["PackDescriptionColumnWidth"] = 23;
			Constants["PackDescriptionColumnIndex"] = 1;
			Constants["PackWeightColumnWidth"] = 11;
			Constants["PackWeightColumnIndex"] = 2;
			Constants["PackUNDGColumnWidth"] = 23;
			Constants["PackUNDGColumnIndex"] = 3;
			Constants["PackVolumeColumnLeftPadding"] = 1;
			Constants["PackVolumeColumnWidth"] = 11;
			Constants["PackVolumeColumnIndex"] = 4;
			Constants["PackCountColumnWidth"] = 11;
			Constants["PackCountColumnIndex"] = 5;

			Constants["NumberOfContainerRows"] = 8;
			Constants["ShowContainerHeadingInMainBody"] = "Y";
			Constants["InterleavePacksAndContainers"] = "Y";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Cn. No      Seal  Type        Net (kg)   Tare (kg)  Gross (kg) Volume (M3)       Packs\n" +
				"";

			const string expectedBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"FAKE4100011 Seal  20GP           11500        2200       13700          10      24 PKG\n" +
				"   Detailed Description 1         1500 UN0440, CHARGES,                2.5           4\n" +
				"                                       SHAPED, class 1.4D                             \n" +
				"                                       UN2305,                                        \n" +
				"                                       NITROBENZENESULPHONIC                          \n" +
				"                                       ACID, class 8, PG II                           \n" +
				"   Detailed Description 2        10000 UN0138, MINES, class            7.5          20\n" +
				"";

			const string expectedBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"                                       1.2D                                           \n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"FAKE4100027 Seal2 40RE           15000        3600       18600           8       8 PLT\n" +
				"   Detailed Description 3        15000                                   8           8\n" +
				"FAKE4100032 Seal3 20RE               0        2300        2300                        \n" +
				"-           -     -             111000           -      111000       0.888       8 PKG\n" +
				"   Detailed Description 4       111000                               0.888           8\n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody1, DocOBL.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedBody2 + expectedFollowOn, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", true, DocOBL.HasFollowOnSection);

			Constants["ShowContainerHeadingInMainBody"] = "N";

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody1 + expectedBody2, DocOBL.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", true, DocOBL.HasFollowOnSection);
		}

		public void TestExtraSection()
		{
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Shipment.JS_RL_NKOrigin = "NLAMS";
			Shipment.JS_RL_NKDestination = "AUBNE";

			Constants["NoOfExtraSectionRows"] = 3;
			Constants["ExtraSection-Count"] = 3;
			Constants["ShowExtraSectionHeadingInMainBody"] = "Y";

			Constants["ExtraSection-Heading1"] = "Heading 1";
			Constants["ExtraSection-Path1"] = "Plain Text";
			Constants["ExtraSection-LeftPadding1"] = 0;
			Constants["ExtraSection-Width1"] = 10;

			Constants["ExtraSection-Heading2"] = "Heading Two";
			Constants["ExtraSection-Path2"] = "<BillOfLading.OriginPort.PortName> -> <BillOfLading.DestinationPort.PortName>";
			Constants["ExtraSection-LeftPadding2"] = 1;
			Constants["ExtraSection-Width2"] = 20;

			Constants["ExtraSection-Heading3"] = "Third Heading";
			Constants["ExtraSection-Path3"] = "Line1\r\nLine2\r\nLine3";
			Constants["ExtraSection-LeftPadding3"] = 1;
			Constants["ExtraSection-Width3"] = 15;

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Heading 1  Heading Two          Third Heading  \n" +
				"";

			const string expectedBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Plain Text Amsterdam ->         Line1          \n" +
				"           Brisbane             Line2          \n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"                                Line3          \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ExtraSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, DocOBL.FormedPages[0].MainBodyExtraSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", true, DocOBL.HasFollowOnSection);

			Constants["ShowExtraSectionHeadingInMainBody"] = "N";

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ExtraSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn, DocOBL.FormedPages[0].MainBodyExtraSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestChargesSection_MultiPage()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = ChargesDisplayTypes.AllCharges;

			Job header = GetNewHeader(Shipment);
			AddCharge(header, "FRT", 1000);
			AddCharge(header, "OLAB", 500, AgencyInvoiceTypesList.Codes.LocalPrePaid);
			AddCharge(header, "DLAB", 750);

			Constants["UseMultiPage"] = "Y";
			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargesDiscriptionLeftPadding"] = 0;
			Constants["ChargesDiscriptionWidth"] = 25;
			Constants["ChargeDescriptionCaption"] = "Charge Desc";
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 8;
			Constants["CollectChargesColumnCaption"] = "CCT";
			Constants["CollectCurrencyColumnLeftPadding"] = 1;
			Constants["CollectCurrencyColumnWidth"] = 3;
			Constants["CollectCurrencyColumnCaption"] = "CX";
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 8;
			Constants["PrepaidChargesColumnCaption"] = "PPD";
			Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
			Constants["PrepaidCurrencyColumnWidth"] = 3;
			Constants["PrepaidCurrencyColumnCaption"] = "PX";

			Constants["ShowChargesHeadingInMainBody"] = "Y";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Charge Desc                    CCT CX       PPD PX \n" +
				"";

			const string expectedBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"International Freight     1,000.00 ERN             \n" +
				"Origin Labour Charges                    500.00 ERN\n" +
				"";

			const string expectedBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Destination Labour          750.00 ERN             \n" +
				"Charges                                            \n" +
				"";

			AssertEquals("should have 2 formed pages", 2, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Body1", expectedHeading + expectedBody1, DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("Body2", expectedHeading + expectedBody2, DocOBL.FormedPages[1].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestChargesSection_AllCharges()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = ChargesDisplayTypes.AllCharges;

			Job header = GetNewHeader(Shipment);
			AddCharge(header, "FRT", 1000);
			AddCharge(header, "OLAB", 500, AgencyInvoiceTypesList.Codes.LocalPrePaid);
			AddCharge(header, "DLAB", 750);

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargesDiscriptionLeftPadding"] = 0;
			Constants["ChargesDiscriptionWidth"] = 25;
			Constants["ChargeDescriptionCaption"] = "Charge Desc";
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 8;
			Constants["CollectChargesColumnCaption"] = "CCT";
			Constants["CollectCurrencyColumnLeftPadding"] = 1;
			Constants["CollectCurrencyColumnWidth"] = 3;
			Constants["CollectCurrencyColumnCaption"] = "CX";
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 8;
			Constants["PrepaidChargesColumnCaption"] = "PPD";
			Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
			Constants["PrepaidCurrencyColumnWidth"] = 3;
			Constants["PrepaidCurrencyColumnCaption"] = "PX";

			Constants["ShowChargesHeadingInMainBody"] = "Y";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Charge Desc                    CCT CX       PPD PX \n" +
				"";

			const string expectedBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"International Freight     1,000.00 ERN             \n" +
				"Origin Labour Charges                    500.00 ERN\n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Destination Labour          750.00 ERN             \n" +
				"Charges                                            \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", true, DocOBL.HasFollowOnSection);

			Constants["NumberOfCollectChargesRows"] = 5;
			Constants["ShowChargesHeadingInMainBody"] = "N";

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn, DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestChargesSection_AllCharges_Codes()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = ChargesDisplayTypes.AllCharges;

			Job header = GetNewHeader(Shipment);
			AddCharge(header, "FRT", 1000);
			AddCharge(header, "OLAB", 500, AgencyInvoiceTypesList.Codes.LocalPrePaid);
			AddCharge(header, "DLAB", 750);

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargesDiscriptionWidth"] = 0;
			Constants["ChargeCodeIndex"] = 1;
			Constants["ChargeCodeLeftPadding"] = 0;
			Constants["ChargeCodeColumnWidth"] = 12;
			Constants["ChargeCodeCaption"] = "Code";
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 8;
			Constants["CollectChargesColumnCaption"] = "CCT";
			Constants["CollectCurrencyColumnLeftPadding"] = 1;
			Constants["CollectCurrencyColumnWidth"] = 3;
			Constants["CollectCurrencyColumnCaption"] = "CX";
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 8;
			Constants["PrepaidChargesColumnCaption"] = "PPD";
			Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
			Constants["PrepaidCurrencyColumnWidth"] = 3;
			Constants["PrepaidCurrencyColumnCaption"] = "PX";

			Constants["ShowChargesHeadingInMainBody"] = "Y";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Code              CCT CX       PPD PX \n" +
				"";

			const string expectedBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"FRT          1,000.00 ERN             \n" +
				"OLAB                        500.00 ERN\n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"DLAB           750.00 ERN             \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", true, DocOBL.HasFollowOnSection);

			Constants["NumberOfCollectChargesRows"] = 5;
			Constants["ShowChargesHeadingInMainBody"] = "N";

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn, DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestChargesSection_CollectCharges()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = ChargesDisplayTypes.CollectCharges;

			Job header = GetNewHeader(Shipment);
			AddCharge(header, "FRT", 1000);
			AddCharge(header, "OLAB", 500, AgencyInvoiceTypesList.Codes.LocalPrePaid);
			AddCharge(header, "DLAB", 750);

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargesDiscriptionLeftPadding"] = 0;
			Constants["ChargesDiscriptionWidth"] = 25;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 8;
			Constants["CollectCurrencyColumnLeftPadding"] = 1;
			Constants["CollectCurrencyColumnWidth"] = 3;
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 8;
			Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
			Constants["PrepaidCurrencyColumnWidth"] = 3;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Charge Description         Collect                 \n" +
				"";

			const string expectedBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"International Freight     1,000.00 ERN             \n" +
				"Destination Labour          750.00 ERN             \n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Charges                                            \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", true, DocOBL.HasFollowOnSection);

			Constants["NumberOfCollectChargesRows"] = 5;
			Constants["ShowChargesHeadingInMainBody"] = "N";

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn, DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestChargesSection_PrepaidCharges()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = ChargesDisplayTypes.PrepaidCharges;

			Job header = GetNewHeader(Shipment);
			AddCharge(header, "FRT", 1000, AgencyInvoiceTypesList.Codes.LocalPrePaid);
			AddCharge(header, "OLAB", 500);
			AddCharge(header, "DLAB", 750, AgencyInvoiceTypesList.Codes.LocalPrePaid);

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargesDiscriptionLeftPadding"] = 0;
			Constants["ChargesDiscriptionWidth"] = 25;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 8;
			Constants["CollectCurrencyColumnLeftPadding"] = 1;
			Constants["CollectCurrencyColumnWidth"] = 3;
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 8;
			Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
			Constants["PrepaidCurrencyColumnWidth"] = 3;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Charge Description                      Prepaid    \n" +
				"";

			const string expectedBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"International Freight                  1,000.00 ERN\n" +
				"Destination Labour                       750.00 ERN\n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Charges                                            \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", true, DocOBL.HasFollowOnSection);

			Constants["NumberOfCollectChargesRows"] = 5;
			Constants["ShowChargesHeadingInMainBody"] = "N";

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn, DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestChargesSection_AsAgreed()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = ChargesDisplayTypes.AsAgreed;

			Job header = GetNewHeader(Shipment);
			AddCharge(header, "FRT", 10000);
			AddCharge(header, "OLAB", 500);
			AddCharge(header, "DLAB", 750);

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargesDiscriptionLeftPadding"] = 0;
			Constants["ChargesDiscriptionWidth"] = 25;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 12;
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 12;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", "", DocOBL.ChargesSectionHeader.Trim());
			AssertMultilineASCIIEquals("Body", "As Agreed", DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestChargesSection_NoCharges()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = ChargesDisplayTypes.NoCharges;

			Job header = GetNewHeader(Shipment);
			AddCharge(header, "FRT", 10000);
			AddCharge(header, "OLAB", 500);
			AddCharge(header, "DLAB", 750);

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargesDiscriptionLeftPadding"] = 0;
			Constants["ChargesDiscriptionWidth"] = 25;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 12;
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 12;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", "", DocOBL.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", "", DocOBL.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", DocOBL.FollowOnSection));
			AssertEquals("HasFollowOn", false, DocOBL.HasFollowOnSection);
		}

		public void TestDetailSectionColumnOrder()
		{
			Constants["VolumeMeasurementLeftPadding"] = 0;
			Constants["VolumeMeasurementWidth"] = 12;
			Constants["GrossWeightLeftPadding"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["GoodsDescLeftPadding"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["MarksAndNumbersLeftPadding"] = 1;
			Constants["MarksAndNumbersWidth"] = 20;

			Constants["VolumeMeasurementIndex"] = 1;
			Constants["GrossWeightIndex"] = 2;
			Constants["GoodsDescIndex"] = 3;
			Constants["MarksAndNumbersIndex"] = 4;

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"      Volume     Gross Wt Goods Description    Marks & Numbers     \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.DetailsSectionHeader);
		}

		public void TestContainerSectionColumnOrder()
		{
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

			Constants["ContainerPackagesLeftPadding"] = 0;
			Constants["ContainerPackagesWidth"] = 11;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 11;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 11;
			Constants["ContainerWeightLeftPadding"] = 1;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerNumberLeftPadding"] = 1;
			Constants["ContainerNumberWidth"] = 11;

			Constants["ContainerPackagesIndex"] = 1;
			Constants["ContainerVolumeIndex"] = 2;
			Constants["ContainerGrossIndex"] = 3;
			Constants["ContainerTareIndex"] = 4;
			Constants["ContainerWeightIndex"] = 5;
			Constants["ContainerTypeIndex"] = 6;
			Constants["ContainerSealIndex"] = 7;
			Constants["ContainerNumberIndex"] = 8;

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"      Packs Volume (M3)  Gross (kg)   Tare (kg)    Net (kg) Type     Seal  Cn. No     \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ContainersSectionHeader);
		}

		public void TestChargeSectionColumnOrder()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = ChargesDisplayTypes.AllCharges;

			Job header = GetNewHeader(Shipment);
			AddCharge(header, "FRT", 10000);
			AddCharge(header, "OLAB", 500, AgencyInvoiceTypesList.Codes.LocalPrePaid);
			AddCharge(header, "DLAB", 750);

			Constants["PrepaidChargesColumnLeftPadding"] = 0;
			Constants["PrepaidChargesColumnWidth"] = 12;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 12;
			Constants["ChargeDescriptionLeftPadding"] = 1;
			Constants["ChargesDiscriptionWidth"] = 25;

			Constants["PrepaidCurrencyColumnIndex"] = 1;
			Constants["PrepaidChargesColumnIndex"] = 2;
			Constants["CollectCurrencyColumnIndex"] = 3;
			Constants["CollectChargesColumnIndex"] = 4;
			Constants["ChargeDescriptionIndex"] = 5;

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"         Prepaid          Collect Charge Description       \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, DocOBL.ChargesSectionHeader);
		}

		public void TestDisplayContainers()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(true, ((IFormedPagesSupporter)DocOBL).DisplayContainers);

			foreach (string mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				Shipment.JS_PackingMode = mode;
				AssertEquals(false, ((IFormedPagesSupporter)DocOBL).DisplayContainers);
			}
		}

		public void TestTopLevelPacks()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment.ShippingContainers.AddNew();
			Shipment.ShippingContainers.AddNew();

			AssertEquals(false, Shipment.IsTopLevelPacksMode);
			AssertEquals(0, DocOBL.TopLevelPacks.Count);

			foreach (string mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				Shipment.JS_PackingMode = mode;
				Shipment.ShippingContainers.RemoveAll();
				Shipment.ShippingContainers.AddNew();
				Shipment.ShippingContainers.AddNew();

				AssertEquals(true, Shipment.IsTopLevelPacksMode);
				AssertEquals(2, DocOBL.TopLevelPacks.Count);
			}
		}

		public void TestRORSection()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			Shipment.JS_MarksAndNumbers = "Marks Line 1";
			Shipment.DetailedGoodsDescriptionNoteText = "Description Line 1";

			Shipment.JS_ActualWeight = 1522m;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			Shipment.JS_ActualVolume = 6000.48m;
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			//var container1 = Shipment.ShippingContainers.AddNew();
			var container1 = Shipment.ShippingContainers[0];
			container1.JC_ContainerNum = "VIN NUMBER 1";
			container1.JC_ContainerCount = 1;

			container1.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;
			container1.JC_GrossVolume = 6000m;

			container1.JC_GrossWeightUQ = Core.Constants.Weight.Tonnes;
			container1.JC_GrossWeight = 1.5m;

			container1.JC_TotalUnitOfMeasure = Core.Constants.Length.Kilometres;
			container1.JC_TotalLength = 0.01;
			container1.JC_TotalWidth = 0.02;
			container1.JC_TotalHeight = 0.03;

			var container2 = Shipment.ShippingContainers.AddNew();
			container2.JC_ContainerCount = 4;

			container2.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;
			container2.JC_GrossVolume = 0.48m;

			container2.JC_GrossWeightUQ = Core.Constants.Weight.Grams;
			container2.JC_GrossWeight = 22000m;

			container2.JC_TotalUnitOfMeasure = Core.Constants.Length.Millimetres;
			container2.JC_TotalLength = 400;
			container2.JC_TotalWidth = 500;
			container2.JC_TotalHeight = 600;

			Constants["MarksAndNumbersWidth"] = 20;
			Constants["MarksAndNumbersAndGoodsDescriptionGap"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GoodsDescriptionAndGrossWeightGap"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["GrossWeightAndMeasurementGap"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;

			Constants["PackRefNumberColumnWidth"] = 20;
			Constants["PackCountColumnWidth"] = 5;
			Constants["PackWeightColumnWidth"] = 11;
			Constants["PackVolumeColumnWidth"] = 11;
			Constants["PackLengthColumnWidth"] = 10;
			Constants["PackWidthColumnWidth"] = 10;
			Constants["PackHeightColumnWidth"] = 10;
			Constants["PackAreaColumnCaption"] = "Area (m2)";
			Constants["PackAreaColumnWidth"] = 10;

			const string expectedDetailHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume\n" +
				"";

			const string expectedDetailBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"MARKS LINE 1         DESCRIPTION LINE 1        1522 KG   6000.48 M3\n" +
				"";

			const string expectedRORHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9   10
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"VIN/Serial           Count Weight (KG) Volume (M3) Length (M)  Width (M) Height (M)  Area (m2)\n" +
				"";

			const string expectedRORBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9   10
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"VIN NUMBER 1             1        1500        6000     10.000     20.000     30.000    200.000\n" +
				"";

			const string expectedRORBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9   10
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"                         4          22        0.48      0.400      0.500      0.600      0.800\n" +
				"";

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 5;
			Constants["ShowDetailHeadingInMainBody"] = "Y";
			Constants["IncludeRORInMarksAndNumbersSection"] = "Y";
			Constants["ShowRORHeadingInMainBody"] = "Y";
			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocOBL.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedRORHeading + expectedRORBody1, DocOBL.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("ROR Heading", expectedRORHeading, DocOBL.PackRORSectionHeader);
			AssertMultilineASCIIEquals("ROR Body", ZString.Empty, DocOBL.FormedPages[0].PackRORSection);
			AssertMultilineASCIIEquals("FollowOn", expectedRORHeading + expectedRORBody2, ZString.Join("\n", DocOBL.FollowOnSection));

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 6;
			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocOBL.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedRORHeading + expectedRORBody1 + expectedRORBody2, DocOBL.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("ROR Heading", expectedRORHeading, DocOBL.PackRORSectionHeader);
			AssertMultilineASCIIEquals("ROR Body", ZString.Empty, DocOBL.FormedPages[0].PackRORSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", DocOBL.FollowOnSection));

			Constants["ShowRORHeadingInMainBody"] = "N";
			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocOBL.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedRORBody1 + expectedRORBody2, DocOBL.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("ROR Heading", expectedRORHeading, DocOBL.PackRORSectionHeader);
			AssertMultilineASCIIEquals("ROR Body", ZString.Empty, DocOBL.FormedPages[0].PackRORSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", DocOBL.FollowOnSection));

			Constants["ShowRORHeadingInMainBody"] = "Y";
			Constants["IncludeRORInMarksAndNumbersSection"] = "N";
			AssertEquals("Should only have 1 formed page", 1, DocOBL.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocOBL.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody, DocOBL.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("ROR Heading", expectedRORHeading, DocOBL.PackRORSectionHeader);
			AssertMultilineASCIIEquals("ROR Body", expectedRORHeading + expectedRORBody1 + expectedRORBody2, DocOBL.FormedPages[0].PackRORSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", DocOBL.FollowOnSection));
		}

		public void TestBOLClause()
		{
			ZString registryBOLClause = "This is the registry bol clause";
			ZString notesAdditionalBillClauses = "These are the additional bill clauses from the notes";

			OrgHeader principal = Factory.LoadTop1<OrgHeader>(new ZQuery());

			AssertEquals("Precondition", ZString.Empty, DocOBL.BOLClause);

			Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.AdditionalBillClauses.Description, notesAdditionalBillClauses);
			AssertEquals(notesAdditionalBillClauses, DocOBL.BOLClause);

			AgencyRegistry.Instance.BillOfLadingClause(principal).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryBOLClause);
			AssertEquals("Principal not set on shipment", notesAdditionalBillClauses, DocOBL.BOLClause);

			Shipment.JS_OH_DeliveryAgent = principal.PK;
			AssertEquals(registryBOLClause + "\r\n\r\n" + notesAdditionalBillClauses, DocOBL.BOLClause);

			Shipment.Notes.RemoveAndDeleteAll();
			AssertEquals(registryBOLClause, DocOBL.BOLClause);
		}

		#region Implementation

		static Job GetNewHeader(IJobHeaderParent parent)
		{
			Job result = parent.Factory.NewJobForTesting<Job>();
			result.JH_ParentID = parent.PK;
			result.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(parent.TableName);
			result.Parent = parent;
			return result;
		}

		static JobCharge AddCharge(Job header, ZString chargeCode, ZDecimal localSellAmt)
		{
			return AddCharge(header, chargeCode, localSellAmt, AgencyInvoiceTypesList.Codes.LocalCollect);
		}

		static JobCharge AddCharge(Job header, ZString chargeCode, ZDecimal localSellAmt, ZString invoiceType)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			JobCharge charge = header.Charges.AddNew();
			charge.JR_AC = header.Factory.LoadTop1<AccChargeCode>(filter).PK;
			charge.JR_LocalSellAmt = localSellAmt;
			charge.JR_InvoiceType = invoiceType;
			return charge;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return DocOBL;
		}

		public BillOfLading Shipment
		{
			get { return shipment ?? (shipment = Factory.New<BillOfLading>()); }
		}
		BillOfLading shipment;

		public Dictionary<string, object> Constants
		{
			get { return constants ?? (constants = new Dictionary<string, object>()); }
		}
		Dictionary<string, object> constants;

		public DocAgencyShipment ShipmentWrapper
		{
			get
			{
				DocAgencyShipment wrapper = DocAgencyShipment.New(Shipment, Factory);

				if (constants != null)
				{
					wrapper.SetTemplateConstants(constants);
				}

				return wrapper;
			}
		}

		public DocOceanBillOfLading DocOBL
		{
			get { return ShipmentWrapper.BillOfLading; }
		}

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			base.SetUp();
		}

		#endregion
	}
}
