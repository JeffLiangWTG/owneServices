using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.ExcelTemplates;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromDeclaration))]
	public class FreightWrapperFromDeclarationTest : FreightWrapperTest
	{
		public void TestNewWrapperType()
		{
			var testCases = new (string countryCode, string expectedDeclarationType, string expectedDocDeclarationType)[]
			{
				(Core.Constants.CountryCodes.Eritrea, "Enterprise.Customs.Business.BaseJobDeclaration", "Enterprise.DocumentWrappers.Customs.General.DocDeclaration"),
				(Core.Constants.CountryCodes.Australia, "Enterprise.Customs.AU.Declaration.Business.JobDeclaration", "Enterprise.DocumentWrappers.Customs.AU.DocDeclaration"),
				(Core.Constants.CountryCodes.UnitedArabEmirates, "Enterprise.Customs.AE.Business.JobDeclaration", "Enterprise.Customs.AE.Business.DocDeclaration"),
				(Core.Constants.CountryCodes.Brazil, "Enterprise.Customs.BR.Business.JobDeclaration", "Enterprise.Customs.BR.Business.DocDeclaration"),
				(Core.Constants.CountryCodes.Canada, "Enterprise.Customs.CA.Business.JobDeclaration", "Enterprise.Customs.CA.Business.DocDeclaration"),
				(Core.Constants.CountryCodes.NewZealand, "Enterprise.Customs.NZ.Business.Declaration.JobDeclaration", "Enterprise.DocumentWrappers.Customs.NZ.DocDeclaration"),
				(Core.Constants.CountryCodes.Switzerland, "Enterprise.Customs.CH.Business.JobDeclaration", "Enterprise.Customs.CH.Business.DocDeclaration"),
				(Core.Constants.CountryCodes.Singapore, "Enterprise.Customs.SG.V4.Business.JobDeclaration", "Enterprise.Customs.SG.V4.Business.DocDeclaration"),
				(Core.Constants.CountryCodes.UnitedStates, "Enterprise.Customs.US.Business.JobDeclaration", "Enterprise.Customs.US.DocumentWrappers.DocDeclaration"),
				(Core.Constants.CountryCodes.SouthAfrica, "Enterprise.Customs.ZA.Business.JobDeclaration", "Enterprise.Customs.ZA.Business.DocumentWrappers.DocDeclaration"),
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(testCase.countryCode))
					{
						var declaration = Factory.New<BaseJobDeclaration>();
						var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
						var wrapperType = wrapper.GetType();
						var declarationType = wrapperType.GenericTypeArguments[0].FullName;
						var docDeclarationType = wrapperType.GenericTypeArguments[1].FullName;
						AssertEquals($"{testCase.countryCode} DeclarationType", testCase.expectedDeclarationType, declarationType);
						AssertEquals($"{testCase.countryCode} DocDeclarationType", testCase.expectedDocDeclarationType, docDeclarationType);
					}
				}
			});
		}

		public void TestCustomsEntryNumber()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "Test1234";
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Test1234", wrapper.CustomsEntryNumber);
		}

		public void TestFullHandlingInstructions()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with no care");
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Handle with no care", wrapper.FullHandlingInstructions);
		}

		public void TestFullCartageInstructions()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Pick it up");
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Pick it up", wrapper.FullCartageInstructions);
		}

		public void TestMasterBillAndHouseBill()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "08155555555";
			declaration.JE_HouseBill = "H9083";

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Template MasterBill", "08155555555", wrapper.MasterBill);
			AssertEquals("Template HouseBill", "H9083", wrapper.HouseBill);

			declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "08155555555";
			declaration["JE_MasterBillIssuerSCAC"] = new ZString("APLU");
			declaration.JE_HouseBill = "H9083";
			declaration["JE_HouseBillIssuerSCAC"] = new ZString("AAAC");
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("US import Sea & rail dec should have SCAC prepended to the master bill number", "APLU08155555555", wrapper.MasterBill);
			AssertEquals("US import Sea & rail dec should have SCAC prepended to the house bill number", "AAACH9083", wrapper.HouseBill);
		}

		public void TestBusinessObjectToLogAgainst()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var emptyWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(declaration, (emptyWrapper as IBODocDataProvider).BusinessObjectToLogAgainst);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00023456";

			var shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(shipment, (emptyWrapper as IBODocDataProvider).BusinessObjectToLogAgainst);
		}

		public void TestGetMasterBill()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00023456";

			var shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("MasterBill", "", wrapper.MasterBill);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals("MasterBill", "", wrapper.MasterBill);

			consol.JK_MasterBillNum = "OB99304583";
			AssertEquals("MasterBill should fall back to Consol value when declaration is overridden but Dec.Masterbill is empty", "OB99304583", wrapper.MasterBill);

			declaration.JE_MasterBill = "DecMawb";
			AssertEquals("MasterBill should use Dec.Masterbill when freight details overriden & has value", "DecMawb", wrapper.MasterBill);
		}

		public void TestGetOriginAndDestinationFallBackToFreight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Origin", ZString.Empty, wrapper.Origin.Location.UNLOCO);
			AssertEquals("ETD", ZDateTime.Empty, wrapper.Origin.Date);
			AssertEquals("Destination", ZString.Empty, wrapper.Destination.Location.UNLOCO);
			AssertEquals("ETA", ZDateTime.Empty, wrapper.Destination.Date);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_E_DEP = new ZDateTime(2011, 1, 1);
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_E_ARV = new ZDateTime(2011, 1, 3);

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Origin", "AUSYD", wrapper.Origin.Location.UNLOCO);
			AssertEquals("ETD", new ZDateTime(2011, 1, 1), wrapper.Origin.Date);
			AssertEquals("Destination", "USCHI", wrapper.Destination.Location.UNLOCO);
			AssertEquals("ETA", new ZDateTime(2011, 1, 3), wrapper.Destination.Date);

			declaration.JE_RL_NKOrigin = "";
			declaration.JE_DateAtOrigin = new ZDateTime(2011, 1, 2);
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2011, 1, 4);

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Origin", "AUSYD", wrapper.Origin.Location.UNLOCO);
			AssertEquals("ETD", new ZDateTime(2011, 1, 1), wrapper.Origin.Date);
			AssertEquals("Destination", "USLAX", wrapper.Destination.Location.UNLOCO);
			AssertEquals("ETA", new ZDateTime(2011, 1, 4), wrapper.Destination.Date);
		}

		public void TestGetHouseBillFallbackToFreight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("HouseBill", ZString.Empty, wrapper.HouseBill);

			shipment.JS_HouseBill = "H123";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("HouseBill fallbacks to shipment", "H123", wrapper.HouseBill);

			declaration.JE_HouseBill = "H124";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("HouseBill fallbacks to shipment", "H124", wrapper.HouseBill);
		}

		public void TestGetOuterPackFallbackToFreight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("OuterPack", "0 PLT", wrapper.ShipmentOuterPacksQty.ValueAndUnitCode);

			shipment.JS_OuterPacks = 500;
			shipment.JS_F3_NKPackType = "PCK";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("OuterPack", "500 PCK", wrapper.ShipmentOuterPacksQty.ValueAndUnitCode);

			declaration.JE_TotalNoOfPacksPackType = "PCK";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("OuterPack", "500 PCK", wrapper.ShipmentOuterPacksQty.ValueAndUnitCode);

			declaration.JE_TotalNoOfPacks = 501;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("OuterPack", "501 PCK", wrapper.ShipmentOuterPacksQty.ValueAndUnitCode);
		}

		public void TestTotalWeightFallbackToFreight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Weight", "0.000 KG", wrapper.Weight.ValueAndUnitCode);

			shipment.JS_ManifestedWeight = 500;
			shipment.JS_ActualWeight = 500;
			shipment.JS_DocumentedWeight = 500;
			shipment.JS_UnitOfWeight = "KG";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Weight", "500.000 KG", wrapper.Weight.ValueAndUnitCode);

			declaration.JE_TotalWeightUnit = "KG";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Weight", "500.000 KG", wrapper.Weight.ValueAndUnitCode);

			declaration.JE_TotalWeight = 501;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Weight", "501.000 KG", wrapper.Weight.ValueAndUnitCode);
		}

		public void TestTotalVolumeFallbackToFreight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Volume", "0.000 M3", wrapper.Volume.ValueAndUnitCode);

			shipment.JS_ManifestedVolume = 500;
			shipment.JS_ActualVolume = 500;
			shipment.JS_DocumentedVolume = 500;
			shipment.JS_UnitOfWeight = Core.Constants.Volume.CubicMetres;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Volume", "500.000 M3", wrapper.Volume.ValueAndUnitCode);

			declaration.JE_TotalVolumeUnit = Core.Constants.Volume.CubicMetres;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Volume", "500.000 M3", wrapper.Volume.ValueAndUnitCode);

			declaration.JE_TotalVolume = 501;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Volume", "501.000 M3", wrapper.Volume.ValueAndUnitCode);
		}

		public void TestContainersFallbackToFreight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Containers", 0, wrapper.Containers.Count);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX1234561";
			shipment.OuterPackLines.AddNew().JL_Calc_ContainerNumber = "CRUX1234561";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Containers", 1, wrapper.Containers.Count);
			AssertEquals("Container number", "CRUX1234561", wrapper.Containers[0].ContainerNo);

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "CRUX1234562";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Containers", 1, wrapper.Containers.Count);
			AssertEquals("Container number", "CRUX1234562", wrapper.Containers[0].ContainerNo);
		}

		public void TestPackagesFallBackToFreight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Packages", 0, wrapper.Packages.Count);

			var shipmentPack = shipment.OuterPackLines.AddNew();
			shipmentPack.JL_PackageCount = 20;
			shipmentPack.JL_F3_NKPackType = "PCK";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Packages", 1, wrapper.Packages.Count);
			AssertEquals("Quantity", 20m, wrapper.Packages[0].Packages.Value);
			AssertEquals("Unit", "PCK", wrapper.Packages[0].Packages.Unit.Code);

			var customsPack = declaration.Packages.AddNew();
			customsPack.CW_PackType = "PCK";
			customsPack.CW_PackQty = 21;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Packages", 1, wrapper.Packages.Count);
			AssertEquals("Quantity", 21m, wrapper.Packages[0].Packages.Value);
			AssertEquals("Unit", "PCK", wrapper.Packages[0].Packages.Unit.Code);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestGoodsAvailableAtFallsBackToShipmentThenConsol()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var emptyWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00023456";

			var shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.GoodsAvailableAt.CompanyName", "", wrapper.GoodsAvailableAt.CompanyName);

			var orgAtConsol = Factory.New<OrgHeader>();
			orgAtConsol.OH_FullName = "Org at Consol";
			consol.JK_OA_UnpackDepotAddress = orgAtConsol.MainAddress.PK;

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.GoodsAvailableAt.CompanyName", "Org at Consol", wrapper.GoodsAvailableAt.CompanyName);

			var orgAtShipment = Factory.New<OrgHeader>();
			orgAtShipment.OH_FullName = "Org at Shipment";
			shipment.JS_OA_ImportReleaseDepot = orgAtShipment.MainAddress.PK;

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.GoodsAvailableAt.CompanyName", "Org at Shipment", wrapper.GoodsAvailableAt.CompanyName);

			var orgAtDeclaration = Factory.New<OrgHeader>();
			orgAtDeclaration.OH_FullName = "Org at Declaration";
			shipment.JS_OA_ImportReleaseDepot = orgAtDeclaration.MainAddress.PK;

			declaration.DepotDocAddress.OrganisationPK = orgAtDeclaration.PK;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.GoodsAvailableAt.CompanyName", "Org at Declaration", wrapper.GoodsAvailableAt.CompanyName);
		}

		public void TestDeliveryAgentFallsBackToShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(true, wrapper.DeliveryAgent.Organisation.IsNull);
			AssertEquals(ZDateTime.Empty, wrapper.DeliveryCartageAdvised);
			AssertEquals(ZDateTime.Empty, wrapper.DeliveryGoodsDelivered);
			AssertEquals(ZDateTime.Empty, wrapper.DeliveryRequiredBy);

			var deliveryAgent = Factory.New<OrgHeader>();
			var deliveryAgentAddress = deliveryAgent.Addresses.AddNewMainAddress();
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = deliveryAgentAddress.PK;

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2008, 9, 11);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2009, 9, 11);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2010, 9, 11);

			AssertEquals(deliveryAgent.PK, wrapper.DeliveryAgent.Organisation.PK);
			AssertEquals(new ZDateTime(2008, 9, 11), wrapper.DeliveryCartageAdvised);
			AssertEquals(new ZDateTime(2009, 9, 11), wrapper.DeliveryGoodsDelivered);
			AssertEquals(new ZDateTime(2010, 9, 11), wrapper.DeliveryRequiredBy);
		}

		public void TestImportAgentFallsBackToShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(true, wrapper.ImportAgent.Organisation.IsNull);

			var importAgent = Factory.New<OrgHeader>();
			shipment.JS_OH_DeliveryAgent = importAgent.PK;

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(importAgent.PK, wrapper.ImportAgent.Organisation.PK);
		}

		public void TestBarcodeDetails_CS00126784()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "BS2342342";
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Code-[DEC]\r\nUniqueID-[BS2342342]", wrapper.BarcodePrerequisitesForTestingONLY);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S2343923423";
			declaration.JE_JS = shipment.PK;

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Code-[SHP]\r\nUniqueID-[S2343923423]", wrapper.BarcodePrerequisitesForTestingONLY);
		}

		public void TestCorrectHeadingsAndNumbersAreReturned()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var emptyWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00023456";

			var shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;

			var fullWrapperDeclaration = FreightWrapperFromDeclaration.New(declaration, Factory);

			AssertEquals("fullWrapper.ConsolRoutes.Count", 1, fullWrapperDeclaration.ConsolRoutes.Count);
			AssertEquals("fullWrapper.JobNumberHeading", "Shipment", fullWrapperDeclaration.JobNumberHeading);
			AssertEquals("fullWrapper.JobNumber", ZString.Empty, fullWrapperDeclaration.JobNumber);
			AssertEquals("fullWrapper.SecondaryHeading", "Consol", fullWrapperDeclaration.SecondaryHeading);
			AssertEquals("fullWrapper.SecondaryNumber", "C00023456", fullWrapperDeclaration.SecondaryNumber);
		}

		public override void TestJobHeaderBranchLogo()
		{
			GlbBranch currentBranch = GlbBranch.CurrentBranch;

			GlbBranch anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, currentBranch.PK.ToGuid(), Guid.Empty, new Bitmap(1, 1));
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, anotherBranch.PK.ToGuid(), Guid.Empty, new Bitmap(2, 2));

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = currentBranch.PK;

			FreightWrapper wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Logo should be Current Default Branches Logo", new Size(1, 1), wrapper.CompanyLogo.Size);

			declaration.JE_GB = anotherBranch.PK;
			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Logo should be Another Branches Logo", new Size(2, 2), wrapper.CompanyLogo.Size);

			JobHeader jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_GB = currentBranch.PK;
			jobHeader.JH_ParentID = declaration.PK;
			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Logo should NOW be Current Default Branches Logo again.", new Size(1, 1), wrapper.CompanyLogo.Size);
		}

		public void TestNotifyPartyWorksForEmptyDeclarationBOAndShipmentBONotifyPartyDocumentaryAddresses()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.NotifyParty.CompanyName should be empty string", ZString.Empty, wrapper.NotifyParty.CompanyName);

			declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			OrgHeader importerOrg = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importerOrg.PK;
			OrgContact orgContact = importerOrg.Contacts.AddNew();
			OrgDocument orgContactDocument = orgContact.Documents.AddNew();
			orgContactDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;
			importerOrg.OH_FullName = "IMPORTERER";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.NotifyParty.CompanyName should be from declaration.importer", "IMPORTERER", wrapper.NotifyParty.CompanyName);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			JobDocAddress shipmentNotify = shipment.NotifyPartyDocumentaryAddress;
			shipmentNotify.E2_AddressOverride = true;
			shipmentNotify.E2_CompanyName = "NOTIFY SHIPMENT";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.NotifyParty.CompanyName should be from Shipment", "NOTIFY SHIPMENT", wrapper.NotifyParty.CompanyName);

			JobDocAddress declarationNotify = declaration.NotifyPartyDocumentaryAddress;
			declarationNotify.E2_AddressOverride = true;
			declarationNotify.E2_CompanyName = "NOTIFY DECLARATION";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.NotifyParty.CompanyName should be from declaration", "NOTIFY DECLARATION", wrapper.NotifyParty.CompanyName);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.NotifyParty.CompanyName should be from declaration", "NOTIFY DECLARATION", wrapper.NotifyParty.CompanyName);
		}

		public void TestExportReceivingDepotAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			FreightWrapper wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingDepotAddress.Address);

			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO DEPARTUREDEPOTADDRESS 55";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTUREDEPOTADDRESS 55\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);

			OrgAddress shipmentExportReceivingDepot = Factory.New<OrgAddress>();
			shipmentExportReceivingDepot.OA_Address1 = "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66";
			shipmentExportReceivingDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = shipmentExportReceivingDepot.PK;

			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with ShipmentBO.ExportReceivingDepot", "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress declarationBODepotDocAddress = org.Addresses.AddNew();
			declarationBODepotDocAddress.OA_Address1 = "DECLARATIONBO DEPOTDOCADDRESS 55";
			declarationBODepotDocAddress.OA_RN_NKCountryCode = "AU";
			declaration.DepotDocAddress.E2_OA_Address = declarationBODepotDocAddress.PK;

			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with DeclarationBO.DepotDocAddress, ContactType.Depot", "DECLARATIONBO DEPOTDOCADDRESS 55\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);
		}

		public void TestExportReceivingCTOAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			FreightWrapper wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingCTOAddress.Address);

			OrgAddress consolBODepartureCTOAddress = Factory.New<OrgAddress>();
			consolBODepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolBODepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_DepartureCTOAddress = consolBODepartureCTOAddress.PK;

			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivingCTOAddress.Address);

			OrgAddress declarationBOCTODocAddress = Factory.New<OrgAddress>();
			declarationBOCTODocAddress.OA_Address1 = "DECLARATIONBO CTODOCADDRESS 44";
			declarationBOCTODocAddress.OA_RN_NKCountryCode = "AU";
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = declarationBOCTODocAddress.PK;

			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with DeclarationBO.ContainerTerminalOperatorDocAddress", "DECLARATIONBO CTODOCADDRESS 44\nAUSTRALIA", wrapper.ExportReceivingCTOAddress.Address);
		}

		public void TestExportReceivalAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			FreightWrapper wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivalAddress.Address);

			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO PACKDEPOTADDRESS 88";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			OrgAddress consolDepartureCTOAddress = Factory.New<OrgAddress>();
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with ConsolBO.PackDepotAddress", "CONSOLBO PACKDEPOTADDRESS 88\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			OrgAddress shipmentExportReceivingDepot = Factory.New<OrgAddress>();
			shipmentExportReceivingDepot.OA_Address1 = "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66";
			shipmentExportReceivingDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = shipmentExportReceivingDepot.PK;

			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with ShipmentBO.ExportReceivingDepot", "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress declarationBODepotDocAddress = org.Addresses.AddNew();
			declarationBODepotDocAddress.OA_Address1 = "DECLARATIONBO DEPOTDOCADDRESS 55";
			declarationBODepotDocAddress.OA_RN_NKCountryCode = "AU";
			declaration.DepotDocAddress.E2_OA_Address = declarationBODepotDocAddress.PK;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			OrgContact ctoContact = org.Contacts.AddNew();
			ctoContact.OC_ContactName = "Mickey Mouse";
			OrgDocument ctoContactDoc = ctoContact.Documents.AddNew();
			ctoContactDoc.OD_DocumentGroup = ContactType.CTO.Code;
			ctoContactDoc.OD_DefaultContact = true;

			OrgContact depotContact = org.Contacts.AddNew();
			depotContact.OC_ContactName = "Elmer Fudd";
			OrgDocument depotContactDoc = depotContact.Documents.AddNew();
			depotContactDoc.OD_DocumentGroup = ContactType.Depot.Code;
			depotContactDoc.OD_DefaultContact = true;

			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with DeclarationBO.DepotDocAddress, ContactType.Depot", "DECLARATIONBO DEPOTDOCADDRESS 55\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);
			AssertEquals("Should return AddressWrapper with DeclarationBO.DepotDocAddress, ContactType.Depot", "Elmer Fudd", wrapper.ExportReceivalAddress.ContactName);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Should return AddressWrapper with DeclarationBO.DepotDocAddress, ContactType.CTO", "DECLARATIONBO DEPOTDOCADDRESS 55\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);
			AssertEquals("Should return AddressWrapper with DeclarationBO.DepotDocAddress, ContactType.CTO", "Mickey Mouse", wrapper.ExportReceivalAddress.ContactName);
		}

		public void TestPickupDeliveryConfirmations()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			PackLine packline = shipment.OuterPackLines.AddNew();
			ForwardingContainer container = consol.Containers.AddNew();
			packline.SetContainer(consol, container);
			CommonPickupDeliveryConfirm deliveryLooseConfirmation = shipment.DeliveryConfirms.AddNew();

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);

			AssertEquals("wrapper.PickupDeliveryConfirmations", 1, wrapper.PickupDeliveryConfirmations.Count);
		}

		#region TestBarcode

		public void TestTextBarcode_WithAndWithoutShipment()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_DeclarationReference = "B000001";

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			((IDocTypeCode)wrapper).DocTypeCode = "CAD";

			AssertEquals("No shipment attached, barcode comes from the declaration.", "^DEC=B000001;CAD;|", wrapper.BarcodeText);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);

			AssertEquals("Precondition: Origin", "", shipment.JS_RL_NKOrigin);
			AssertEquals("Precondition: Destination", "", shipment.JS_RL_NKDestination);
			AssertEquals("Precondition: Housebill", "", shipment.JS_HouseBill);

			AssertEquals("Missing data in shipment, BarcodeTextForFont property is empty", ZString.Empty, wrapper.BarcodeText);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_HouseBill = "12345678901234567890";
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);

			AssertEquals("Missing data in shipment, BarcodeTextForFont property is empty", ZString.Empty, wrapper.BarcodeText);

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			((IDocTypeCode)wrapper).DocTypeCode = "CAD";

			AssertEquals("Shipment attached, All data valid and barcode comes from the shipment", "[EDICADSYDLAX12345678901234567890]", wrapper.BarcodeText);
		}

		#endregion

		public void TestMultipleConsols()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			FreightWrapperFromDeclaration wrapper0 = (FreightWrapperFromDeclaration)FreightWrapper.New(declaration, Factory)[0];
			AssertNull(wrapper0.Consol);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			declaration.JE_JS = shipment.PK;
			ForwardingConsol consol = shipment.Consols.AddNew();

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_ETD = new ZDateTime(2008, 1, 1);
			transport.JW_ATD = new ZDateTime(2008, 1, 2);
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETA = new ZDateTime(2008, 1, 3);
			transport.JW_ATA = new ZDateTime(2008, 1, 4);

			FreightWrapperFromDeclaration wrapper1 = (FreightWrapperFromDeclaration)FreightWrapper.New(declaration, Factory)[0];
			AssertEquals(consol, wrapper1.Consol);

			ForwardingConsol consol2 = shipment.Consols.AddNew();
			Transport transport2 = consol2.Transports.MostInterestingTransport;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_ETD = new ZDateTime(2008, 1, 5);
			transport2.JW_ATD = new ZDateTime(2008, 1, 6);
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_ETA = new ZDateTime(2008, 1, 7);
			transport2.JW_ATA = new ZDateTime(2008, 1, 8);

			FreightWrapperFromDeclaration wrapper2 = (FreightWrapperFromDeclaration)FreightWrapper.New(shipment, transport2, Factory)[0];
			AssertEquals(consol2, wrapper2.Consol);

			FreightWrapperFromDeclaration wrapper3 = (FreightWrapperFromDeclaration)FreightWrapper.New(shipment, transport, Factory)[0];
			AssertEquals(consol, wrapper3.Consol);

			shipment.CurrentConsolForDocuments = consol2;

			FreightWrapperFromDeclaration wrapper4 = (FreightWrapperFromDeclaration)FreightWrapper.New(declaration, Factory)[0];
			wrapper4.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals(consol2, wrapper4.Consol);

			FreightWrapperFromDeclaration wrapper5 = (FreightWrapperFromDeclaration)FreightWrapper.New(declaration, Factory)[0];
			wrapper5.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("If not transport leg is specified, wrapper should default to current consol on the shipment", consol2, wrapper5.Consol);

			shipment.CurrentConsolForDocuments = null;

			FreightWrapperFromDeclaration wrapper6 = (FreightWrapperFromDeclaration)FreightWrapper.New(declaration, Factory)[0];
			wrapper6.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("If neither transport leg nor current consol is specifed, calls back to DepartureConsolForDocuments with the departure direction", consol, wrapper6.Consol);
		}

		public void TestMasterBillHeading()
		{
			BaseJobDeclaration declaration1 = Factory.New<BaseJobDeclaration>();
			FreightWrapper wrapper1 = FreightWrapper.New(declaration1, Factory)[0];

			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			FreightWrapper wrapper2 = FreightWrapper.New(declaration2, Factory)[0];

			BaseJobDeclaration declaration3 = Factory.New<BaseJobDeclaration>();
			FreightWrapper wrapper3 = FreightWrapper.New(declaration3, Factory)[0];

			declaration1.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("wrapper.HouseBillHeading", "MAWB", wrapper1.MasterBillHeading);

			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("wrapper.HouseBillHeading", "Ocean Bill Of Lading", wrapper2.MasterBillHeading);

			declaration3.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("wrapper.HouseBillHeading", "Master Bill", wrapper3.MasterBillHeading);
		}

		public void TestHouseBillHeading()
		{
			BaseJobDeclaration declaration1 = Factory.New<BaseJobDeclaration>();
			FreightWrapper wrapper1 = FreightWrapper.New(declaration1, Factory)[0];

			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			FreightWrapper wrapper2 = FreightWrapper.New(declaration2, Factory)[0];

			BaseJobDeclaration declaration3 = Factory.New<BaseJobDeclaration>();
			FreightWrapper wrapper3 = FreightWrapper.New(declaration3, Factory)[0];

			BaseJobDeclaration declaration4 = Factory.New<BaseJobDeclaration>();
			FreightWrapper wrapper4 = FreightWrapper.New(declaration4, Factory)[0];

			declaration1.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("wrapper.HouseBillHeading", "HAWB", wrapper1.HouseBillHeading);

			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("wrapper.HouseBillHeading", "House Bill Of Lading", wrapper2.HouseBillHeading);

			declaration3.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("wrapper.HouseBillHeading", "House Bill", wrapper3.HouseBillHeading);

			declaration4.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals("wrapper.HouseBillHeading", "Parcel Post Numbers", wrapper4.HouseBillHeading);
		}

		public void TestGBDeclarationCanAccessGBProperties()
		{
			Enterprise.Customs.EU.Business.Declaration.JobDeclaration declaration = Factory.New<Enterprise.Customs.EU.Business.Declaration.JobDeclaration>();
			declaration.ZG_CTStatusID = "ZXC";
			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=GenericFreightJob";
					workSheet[4, 0] = "#SectionBody";
					workSheet[5, 1] = "<Declaration.ZG_CTStatusID>";
					workSheet[6, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(wrapper, excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface xlInterface = new ExcelInterface())
					{
						xlInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = xlInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", "{B}-[ZXC]", workSheet.ToString());
					}
				}
			}
		}

		Report GetNewReport(IBODocDataProvider topLevelDataSource, ExcelTemplate excelTemplate)
		{
			DocumentCommand stmMenuItem = Factory.New<DocumentCommand>();
			DocumentPack pack = new DocumentPack(stmMenuItem);
			return new Report(pack, excelTemplate, topLevelDataSource, "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
		}

		public void TestSDFFieldsWorkFromStandaloneDeclaration()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("", wrapper.GetDocDataValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, ""));

			DocumentNote docNote = DocumentNote.LoadNote(declaration);
			docNote.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, "YEEEEEHOOOO!!!");
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("YEEEEEHOOOO!!!", wrapper.GetDocDataValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, ""));
		}

		public void TestSDFFieldsWorkWhenLinkedToShipment()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("", wrapper.GetDocDataValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, ""));

			DocumentNote docNote = DocumentNote.LoadNote(shipment);
			docNote.SetSystemDefinedFieldValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, "YEEEEEHOOOO!!!");
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("YEEEEEHOOOO!!!", wrapper.GetDocDataValue(DocBaseJobDeclaration.SDFields.AdditionalPaymentTerms, ""));
		}

		public override void TestInsuranceRelatedAddressFields()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			JobDocAddress shipmentInsuredBy = shipment.InsuredByDocAddress;
			shipmentInsuredBy.E2_AddressOverride = true;
			shipmentInsuredBy.E2_CompanyName = "INSUREDBY SHIPMENT";

			JobDocAddress shipmentAssuredParty = shipment.AssuredPartyDocAddress;
			shipmentAssuredParty.E2_AddressOverride = true;
			shipmentAssuredParty.E2_CompanyName = "ASSUREDPARTY SHIPMENT";

			JobDocAddress shipmentClaimsPayableBy = shipment.ClaimsPayableByDocAddress;
			shipmentClaimsPayableBy.E2_AddressOverride = true;
			shipmentClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY SHIPMENT";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY SHIPMENT";

			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.InsuredBy.CompanyName", "INSUREDBY SHIPMENT", wrapper.InsuredBy.CompanyName);
			AssertEquals("wrapper.AssuredParty.CompanyName", "ASSUREDPARTY SHIPMENT", wrapper.AssuredParty.CompanyName);
			AssertEquals("wrapper.ClaimsPayableBy.CompanyName", "CLAIMSPAYABLEBY SHIPMENT", wrapper.ClaimsPayableBy.CompanyName);
			AssertEquals("wrapper.SurveyReportParty.CompanyName", "SURVEYREPORTPARTY SHIPMENT", wrapper.SurveyReportParty.CompanyName);

			JobDocAddress declarationInsuredBy = declaration.InsuredByDocAddress;
			declarationInsuredBy.E2_AddressOverride = true;
			declarationInsuredBy.E2_CompanyName = "INSUREDBY DECLARATION";

			JobDocAddress declarationAssuredParty = declaration.AssuredPartyDocAddress;
			declarationAssuredParty.E2_AddressOverride = true;
			declarationAssuredParty.E2_CompanyName = "ASSUREDPARTY DECLARATION";

			JobDocAddress declarationClaimsPayableBy = declaration.ClaimsPayableByDocAddress;
			declarationClaimsPayableBy.E2_AddressOverride = true;
			declarationClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY DECLARATION";

			JobDocAddress declarationSurveyReportParty = declaration.SurveyReportPartyDocAddress;
			declarationSurveyReportParty.E2_AddressOverride = true;
			declarationSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY DECLARATION";

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.InsuredBy.CompanyName", "INSUREDBY DECLARATION", wrapper.InsuredBy.CompanyName);
			AssertEquals("wrapper.AssuredParty.CompanyName", "ASSUREDPARTY DECLARATION", wrapper.AssuredParty.CompanyName);
			AssertEquals("wrapper.ClaimsPayableBy.CompanyName", "CLAIMSPAYABLEBY DECLARATION", wrapper.ClaimsPayableBy.CompanyName);
			AssertEquals("wrapper.SurveyReportParty.CompanyName", "SURVEYREPORTPARTY DECLARATION", wrapper.SurveyReportParty.CompanyName);
		}

		public void TestOrderLines()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();

			var order1 = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			var orderLine2 = order1.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;

			var order2 = Factory.NewWithValidTestData<Order>();
			var orderLine3 = order2.OrderLines.AddNew();
			orderLine3.JO_LineNo = 3;

			var order3 = Factory.NewWithValidTestData<Order>();
			var orderLine4 = order3.OrderLines.AddNew();
			orderLine4.JO_LineNo = 4;
			declaration.AttachedOrders.Add(order3);

			shipment.AttachedOrders.Add(order1);
			shipment.AttachedOrders.Add(order2);
			declaration.AttachedOrders.Add(order3);

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(1, wrapper.Orders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { 4 }, wrapper.OrderLines.Cast<OrderLineWrapper>().Select(x => (int)x.LineNumber));

			declaration.JE_JS = shipment.PK;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(2, wrapper.Orders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3 }, wrapper.OrderLines.Cast<OrderLineWrapper>().Select(x => (int)x.LineNumber));
		}

		public void TestPrimaryAddressesDefaultToFirstAvailableShipmentOrDocumentaryOrOrgHeaderMainAddresses()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_OH_Importer = GetOrgHeader("IMPORTER").PK;
			declaration.JE_OH_Supplier = GetOrgHeader("SUPPLIER").PK;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();

			var shipmentConsignor = shipment.ConsignorDocumentaryAddress;
			shipmentConsignor.E2_AddressOverride = true;
			shipmentConsignor.E2_CompanyName = "CONSIGNOR SHIPMENT";

			var shipmentConsignee = shipment.ConsigneeDocumentaryAddress;
			shipmentConsignee.E2_AddressOverride = true;
			shipmentConsignee.E2_CompanyName = "CONSIGNEE SHIPMENT";

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.Consignor.CompanyName", "CONSIGNOR SHIPMENT", wrapper.Consignor.CompanyName);
			AssertEquals("wrapper.Consignee.CompanyName", "CONSIGNEE SHIPMENT", wrapper.Consignee.CompanyName);

			var declarationSupplier = declaration.SupplierDocumentaryAddress;
			declarationSupplier.E2_AddressOverride = true;
			declarationSupplier.E2_CompanyName = "SUPPLIER DECLARATION";

			var declarationImporter = declaration.ImporterDocumentaryAddress;
			declarationImporter.E2_AddressOverride = true;
			declarationImporter.E2_CompanyName = "IMPORTER DECLARATION";

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.Consignor.CompanyName", "CONSIGNOR SHIPMENT", wrapper.Consignor.CompanyName);
			AssertEquals("wrapper.Consignee.CompanyName", "CONSIGNEE SHIPMENT", wrapper.Consignee.CompanyName);

			declaration.JE_JS = ZGuid.Empty;

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.Consignor.CompanyName", "SUPPLIER DECLARATION", wrapper.Consignor.CompanyName);
			AssertEquals("wrapper.Consignee.CompanyName", "IMPORTER DECLARATION", wrapper.Consignee.CompanyName);

			declarationSupplier.Delete();
			declarationImporter.E2_AddressOverride = false;

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.Consignor.CompanyName", "SUPPLIER", wrapper.Consignor.CompanyName);
			AssertEquals("wrapper.Consignee.CompanyName", "IMPORTER", wrapper.Consignee.CompanyName);
		}

		public void TestSupplementaryAddressesFallbackToOrgHeaderOrFreightShipment()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_OH_Importer = GetOrgHeader("IMPORTER").PK;
			declaration.JE_OH_Supplier = GetOrgHeader("SUPPLIER").PK;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();

			var shipmentNotify = shipment.NotifyPartyDocumentaryAddress;
			shipmentNotify.E2_AddressOverride = true;
			shipmentNotify.E2_CompanyName = "NOTIFY SHIPMENT";

			var shipmentBuyer = shipment.BuyerDocAddress;
			shipmentBuyer.E2_AddressOverride = true;
			shipmentBuyer.E2_CompanyName = "BUYER SHIPMENT";

			var shipmentPickup = shipment.ConsignorPickupAddress;
			shipmentPickup.E2_AddressOverride = true;
			shipmentPickup.E2_CompanyName = "PICKUP SHIPMENT";

			var shipmentDelivery = shipment.ConsigneeDeliveryAddress;
			shipmentDelivery.E2_AddressOverride = true;
			shipmentDelivery.E2_CompanyName = "DELIVERY SHIPMENT";

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.NotifyParty.CompanyName", "NOTIFY SHIPMENT", wrapper.NotifyParty.CompanyName);
			AssertEquals("wrapper.Buyer.CompanyName", "BUYER SHIPMENT", wrapper.Buyer.CompanyName);
			AssertEquals("wrapper.PickupAddress.CompanyName", "PICKUP SHIPMENT", wrapper.PickupAddress.CompanyName);
			AssertEquals("wrapper.DeliveryAddress.CompanyName", "DELIVERY SHIPMENT", wrapper.DeliveryAddress.CompanyName);

			var declarationNotify = declaration.NotifyPartyDocumentaryAddress;
			declarationNotify.E2_AddressOverride = true;
			declarationNotify.E2_CompanyName = "NOTIFY DECLARATION";

			var declarationBuyer = declaration.BuyerDocAddress;
			declarationBuyer.E2_AddressOverride = true;
			declarationBuyer.E2_CompanyName = "BUYER DECLARATION";

			var declarationPickup = declaration.SupplierPickupAddress;
			declarationPickup.E2_AddressOverride = true;
			declarationPickup.E2_CompanyName = "PICKUP SHIPMEMT/DECLARATION";

			var declarationDelivery = declaration.ImporterDeliveryAddress;
			declarationDelivery.E2_AddressOverride = true;
			declarationDelivery.E2_CompanyName = "DELIVERY SHIPMENT/DECLARATION";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.NotifyParty.CompanyName", "NOTIFY DECLARATION", wrapper.NotifyParty.CompanyName);
			AssertEquals("wrapper.Buyer.CompanyName", "BUYER DECLARATION", wrapper.Buyer.CompanyName);
			AssertEquals("wrapper.PickupAddress.CompanyName", "PICKUP SHIPMEMT/DECLARATION", wrapper.PickupAddress.CompanyName);
			AssertEquals("wrapper.DeliveryAddress.CompanyName", "DELIVERY SHIPMENT/DECLARATION", wrapper.DeliveryAddress.CompanyName);
			AssertEquals("Shipment & Dec PickupAddress are now same object", shipmentPickup, declarationPickup);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.NotifyParty.CompanyName", "NOTIFY DECLARATION", wrapper.NotifyParty.CompanyName);
			AssertEquals("wrapper.Buyer.CompanyName", "BUYER DECLARATION", wrapper.Buyer.CompanyName);
			AssertEquals("wrapper.PickupAddress.CompanyName", "PICKUP SHIPMEMT/DECLARATION", wrapper.PickupAddress.CompanyName);
			AssertEquals("wrapper.DeliveryAddress.CompanyName", "DELIVERY SHIPMENT/DECLARATION", wrapper.DeliveryAddress.CompanyName);
			AssertEquals("Shipment & Dec DeliveryAddress are now same object", shipmentDelivery, declarationDelivery);
			AssertEquals("wrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, wrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("wrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, wrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("wrapper.ExportReceivalAddress", AddressWrapper.Empty(Factory).Address, wrapper.ExportReceivalAddress.Address);

			declarationPickup.Delete();
			declarationDelivery.Delete();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.DeliveryAddress.CompanyName", "IMPORTER", wrapper.DeliveryAddress.CompanyName);
			AssertEquals("wrapper.PickupAddress.CompanyName", "SUPPLIER", wrapper.PickupAddress.CompanyName);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.DeliveryAddress.CompanyName", "IMPORTER", wrapper.DeliveryAddress.CompanyName);
			AssertEquals("wrapper.PickupAddress.CompanyName", "SUPPLIER", wrapper.PickupAddress.CompanyName);
		}

		public void TestGoodsDescriptionWorksFromExtendedGoodsDescription()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.GoodsDescription", ZString.Empty, wrapper.GoodsDescription);

			declaration.JE_GoodsDescription = "I LOVE CHIPS WITH SAUCE";
			AssertEquals("wrapper.GoodsDescription", "I LOVE CHIPS WITH SAUCE", wrapper.GoodsDescription);

			string longGoodsDescription = @"I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, 
I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I LOVE CHIPS WITH SAUCE, I REALLY DO!!!!!";
			StmNote goodsDescriptionNote = declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, longGoodsDescription);
			AssertEquals("wrapper.GoodsDescription", longGoodsDescription, wrapper.GoodsDescription);
		}

		public void TestFieldsForAirConsolThatFallbackToFreight()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_JS = consol.Shipments.AddNew().PK;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillIssueDate = new ZDateTime(2007, 4, 28);
			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.MasterBillIssue", new ZDateTime(2007, 4, 28), wrapper.MasterBillIssue);
			AssertEquals("wrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, wrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("wrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, wrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("wrapper.ExportReceivalAddress", AddressWrapper.Empty(Factory).Address, wrapper.ExportReceivalAddress.Address);
		}

		public void TestFieldsThatFallbackToFreight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var emptyWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("emptyWrapper.ConsolType.Code", Constants.AgentType.Agent, emptyWrapper.ConsolType.Code);
			AssertEquals("emptyWrapper.ConsolContainerMode.Code", ZString.Empty, emptyWrapper.ConsolContainerMode.Code);
			AssertEquals("emptyWrapper.ConsolTransportMode.Code", ZString.Empty, emptyWrapper.ConsolTransportMode.Code);
			AssertEquals("emptyWrapper.BookingReference", ZString.Empty, emptyWrapper.BookingReference);
			AssertEquals("emptyWrapper.ConsolPaymentType", ZString.Empty, emptyWrapper.ConsolPaymentType);
			AssertEquals("emptyWrapper.ConsolCreditor.CompanyName", ZString.Empty, emptyWrapper.ConsolCreditor.CompanyName);

			AssertEquals("emptyWrapper.ReleaseType.Code", ZString.Empty, emptyWrapper.ReleaseType.Code);
			AssertEquals("emptyWrapper.ShippedOnBoardType.Code", "SHP", emptyWrapper.ShippedOnBoardType.Code);
			AssertEquals("emptyWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero", ZString.Empty, emptyWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("emptyWrapper.FreightRate.AmountAndCurrencyCode", ZString.Empty, emptyWrapper.FreightRate.AmountAndCurrencyCode);
			AssertEquals("emptyWrapper.ShippedOnBoardDate", ZDateTime.Empty, emptyWrapper.ShippedOnBoardDate);
			AssertEquals("emptyWrapper.NoCopyBills", 3, emptyWrapper.NoCopyBills);
			AssertEquals("emptyWrapper.NoOriginalBills", 3, emptyWrapper.NoOriginalBills);
			AssertEquals("emptyWrapper.ConsolNumber", ZString.Empty, emptyWrapper.ConsolNumber);
			AssertEquals("emptyWrapper.JobNumberHeading", "Brokerage", emptyWrapper.JobNumberHeading);
			AssertEquals("emptyWrapper.JobNumber", ZString.Empty, emptyWrapper.JobNumber);
			AssertEquals("emptyWrapper.SecondaryHeading", ZString.Empty, emptyWrapper.SecondaryHeading);
			AssertEquals("emptyWrapper.SecondaryNumber", ZString.Empty, emptyWrapper.SecondaryNumber);
			AssertEquals("emptyWrapper.AdditionalTerms", ZString.Empty, emptyWrapper.AdditionalTerms);
			AssertEquals("emptyWrapper.ArrivalReference", ZString.Empty, emptyWrapper.ArrivalReference);
			AssertEquals("emptyWrapper.CTOArrivalBerth", ZString.Empty, emptyWrapper.CTOArrivalBerth);
			AssertEquals("emptyWrapper.ShipmentRoutes[First].Origin.UNLOCO", ZString.Empty, emptyWrapper.ShipmentRoutes["First"].Origin.UNLOCO);
			AssertEquals("emptyWrapper.ShipmentRoutes[First].EstimatedDeparture", ZDateTime.Empty, emptyWrapper.ShipmentRoutes["First"].EstimatedDeparture);
			AssertEquals("emptyWrapper.ShipmentRoutes[First].ActualDeparture", ZDateTime.Empty, emptyWrapper.ShipmentRoutes["First"].ActualDeparture);
			AssertEquals("emptyWrapper.ShipmentRoutes[First].Destination.UNLOCO", ZString.Empty, emptyWrapper.ShipmentRoutes["Last"].Destination.UNLOCO);
			AssertEquals("emptyWrapper.ShipmentRoutes[Last].EstimatedArrival", ZDateTime.Empty, emptyWrapper.ShipmentRoutes["Last"].EstimatedArrival);
			AssertEquals("emptyWrapper.ShipmentRoutes[Last].ActualArrival", ZDateTime.Empty, emptyWrapper.ShipmentRoutes["Last"].ActualArrival);
			AssertEquals("emptyWrapper.UnAllocatedWeight", ZDecimal.Zero, emptyWrapper.UnAllocatedWeight);
			AssertEquals("emptyWrapper.UnAllocatedVolume", ZDecimal.Zero, emptyWrapper.UnAllocatedVolume);
			AssertEquals("emptyWrapper.UnAllocatedPackages", ZInt.Zero, emptyWrapper.UnAllocatedPackages);
			AssertEquals("emptyWrapper.CTOArrival.CompanyName", ZString.Empty, emptyWrapper.CTOArrival.CompanyName);
			AssertEquals("emptyWrapper.LoadingMeters", ZDecimal.Zero, emptyWrapper.LoadingMeters);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00023456";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_AgentType = "PSP";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = "VD_";
			consol.JK_BookingReference = "BOOK_A_HOOKER";
			consol.JK_PrepaidCollect = "CCX";
			consol.JK_OA_CreditorAddress = GetOrgHeader("SLARTY BARDFAST").MainAddress.PK;
			consol.JK_AgentsReference = "C0000001";

			var cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "SHOOT ME";

			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;

			var sailing = Factory.New<JobSailing>();
			var voyage = Factory.New<JobVoyage>();

			var voyageDestination = Factory.New<VoyageDestination>();
			voyageDestination.JB_RL_NKPortOfDischarge = "NZAKL";
			voyageDestination.JB_Berth = "WHARF C3";
			voyageDestination.JB_JV = voyage.PK;

			var voyageOrigin = Factory.New<VoyageOrigin>();
			voyageOrigin.JA_RL_NKPortOfLoading = "AUSYD";

			sailing.JX_JB = voyageDestination.PK;
			sailing.JX_JA = voyageOrigin.PK;

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_JX = sailing.PK;
			transport.JW_TerminalCutOff = new ZDateTime(2005, 4, 15);
			transport.JW_DepotCutOff = new ZDateTime(2006, 7, 29);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0001234";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_UniqueConsignRef = "S00087654";
			shipment.JS_ReleaseType = "NXS";
			shipment.JS_ActualWeight = 45.55m;
			shipment.JS_ActualChargeable = 45.55m;
			shipment.JS_ActualVolume = 38.63;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_LoadingMeters = 10.24m;
			shipment.JS_UnitFreightRate = 123.45m;
			shipment.JS_RX_NKFrtRateCurrency = "AUD";
			shipment.JS_ShippedOnBoard = "SOB";
			shipment.JS_ShippedOnBoardDate = new ZDateTime(2006, 12, 25);
			shipment.JS_NoCopyBills = 5;
			shipment.JS_NoOriginalBills = 6;
			shipment.JS_OuterPacks = 345;
			shipment.JS_AdditionalTerms = "Follow the white rabbit";

			shipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2007, 2, 4);
			shipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2007, 3, 5);

			var packLine = shipment.OuterPackLines[0];
			packLine.JL_ActualWeight = 22.34m;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualVolume = 12.89m;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine.JL_PackageCount = 138;

			declaration.JE_JS = shipment.PK;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			declaration.JE_DateAtOrigin = new ZDateTime(1901, 8, 17);
			declaration.JE_DateAtFinalDestination = new ZDateTime(1901, 8, 21);
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(1901, 8, 17);
			declaration.JE_DateOfArrival = new ZDateTime(1901, 8, 21);

			var fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("fullWrapper.ConsolRoutes.Count", 1, fullWrapper.ConsolRoutes.Count);
			CombineAssertions(delegate
			{
				AssertEquals("fullWrapper.ConsolRoutes[0].Origin.UNLOCO", "AUSYD", fullWrapper.ConsolRoutes[0].Origin.UNLOCO);
				AssertEquals("fullWrapper.ConsolRoutes[0].EstimatedDeparture", new ZDateTime(1901, 8, 17), fullWrapper.ConsolRoutes[0].EstimatedDeparture);
				AssertEquals("fullWrapper.ConsolRoutes[0].ActualDeparture", new ZDateTime(1901, 8, 17), fullWrapper.ConsolRoutes[0].ActualDeparture);
				AssertEquals("fullWrapper.ConsolRoutes[0].Destination.UNLOCO", "NZAKL", fullWrapper.ConsolRoutes[0].Destination.UNLOCO);
				AssertEquals("fullWrapper.ConsolRoutes[0].EstimatedArrival", new ZDateTime(1901, 8, 21), fullWrapper.ConsolRoutes[0].EstimatedArrival);
				AssertEquals("fullWrapper.ConsolRoutes[0].ActualArrival", new ZDateTime(1901, 8, 21), fullWrapper.ConsolRoutes[0].ActualArrival);
			});
			AssertEquals("fullWrapper.ConsolType.Code", "PSP", fullWrapper.ConsolType.Code);
			AssertEquals("fullWrapper.ConsolContainerMode.Code", "VD_", fullWrapper.ConsolContainerMode.Code);
			AssertEquals("fullWrapper.ConsolTransportMode.Code", "SEA", fullWrapper.ConsolTransportMode.Code);
			AssertEquals("fullWrapper.BookingReference", "BOOK_A_HOOKER", fullWrapper.BookingReference);
			AssertEquals("fullWrapper.ConsolPaymentType", "CCX", fullWrapper.ConsolPaymentType);
			AssertEquals("fullWrapper.ConsolCreditor.CompanyName", "SLARTY BARDFAST", fullWrapper.ConsolCreditor.CompanyName);

			AssertEquals("fullWrapper.ReleaseType.Code", "NXS", fullWrapper.ReleaseType.Code);
			AssertEquals("fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero", "38.630 M3", fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.FreightRate.AmountAndCurrencyCode", "123.45 AUD", fullWrapper.FreightRate.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.ShippedOnBoardType.Code", "SOB", fullWrapper.ShippedOnBoardType.Code);
			AssertEquals("fullWrapper.ShippedOnBoardDate", new ZDateTime(2006, 12, 25), fullWrapper.ShippedOnBoardDate);
			AssertEquals("fullWrapper.NoCopyBills", 5, fullWrapper.NoCopyBills);
			AssertEquals("fullWrapper.NoOriginalBills", 6, fullWrapper.NoOriginalBills);
			AssertEquals("fullWrapper.ConsolNumber", "C00023456", fullWrapper.ConsolNumber);
			AssertEquals("fullWrapper.ConsolAgentsReference", "C0000001", fullWrapper.ConsolAgentsReference);
			AssertEquals("fullWrapper.JobNumberHeading", "Shipment", fullWrapper.JobNumberHeading);
			AssertEquals("fullWrapper.JobNumber", "S00087654", fullWrapper.JobNumber);
			AssertEquals("fullWrapper.SecondaryHeading", "Consol", fullWrapper.SecondaryHeading);
			AssertEquals("fullWrapper.SecondaryNumber", "C00023456", fullWrapper.SecondaryNumber);
			AssertEquals("fullWrapper.AdditionalTerms", "Follow the white rabbit", fullWrapper.AdditionalTerms);
			AssertEquals("fullWrapper.ArrivalReference", ZString.Empty, fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.CTOArrivalBerth", "WHARF C3", fullWrapper.CTOArrivalBerth);
			AssertEquals("fullWrapper.UnAllocatedWeight", 23.21m, fullWrapper.UnAllocatedWeight);
			AssertEquals("fullWrapper.UnAllocatedVolume", 25.74m, fullWrapper.UnAllocatedVolume);
			AssertEquals("fullWrapper.UnAllocatedPackages", 207, fullWrapper.UnAllocatedPackages);
			AssertEquals("fullWrapper.CTOArrival.CompanyName", "SHOOT ME", fullWrapper.CTOArrival.CompanyName);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.MainShipToParty.CompanyName", ZString.Empty, fullWrapper.MainShipToParty.CompanyName);
			AssertEquals("fullWrapper.SellingParty.CompanyName", ZString.Empty, fullWrapper.SellingParty.CompanyName);
			AssertEquals("fullWrapper.Consolidator.CompanyName", ZString.Empty, fullWrapper.Consolidator.CompanyName);
			AssertEquals("fullWrapper.StuffingLocation.CompanyName", ZString.Empty, fullWrapper.StuffingLocation.CompanyName);
			AssertEquals("fullWrapper.LoadingMeters", 10.24m, fullWrapper.LoadingMeters);
			AssertEquals("fullWrapper.DeliveryDueDate", ZDateTime.Empty, fullWrapper.DeliveryDueDate);
			AssertEquals("fullWrapper.RevisedDeliveryDueDate", ZDateTimeOffset.Empty, fullWrapper.RevisedDeliveryDueDate);
		}

		public void TestFreightShipmentWrapperWorks()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			FreightWrapperFromDeclaration emptyWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("emptyWrapper.FreightShipment", null, emptyWrapper.FreightShipment);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			FreightWrapperFromDeclaration fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertNotEquals("fullWrapper.FreightShipment", null, fullWrapper.FreightShipment);
		}

		public void TestWrapperMappingImport()
		{
			#region Setup
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();

			declaration.JE_OH_Importer = GetOrgHeader("IMPORTER").PK;
			declaration.JE_OH_Supplier = GetOrgHeader("SUPPLIER").PK;
			declaration.JE_OH_Forwarder = GetOrgHeader("FORWARDER").PK;
			declaration.JE_OH_ShippingLine = GetOrgHeader("SHIPPINGLINE").PK;
			declaration.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERYCARTAGE").PK;
			declaration.DocsAndCartage.PickupCartageCoPK = GetOrgHeader("PICKUPCARTAGE").PK;
			declaration.Branch.GB_OH_OrgProxy = GetOrgHeader("OURBRANCH").PK;

			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "I HATE DOCUMENTS";
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";

			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTO.Addresses[0].PK;

			declaration.JE_ContainerMode = Constants.ContainerModes.FCL;  //Lookups.CargoIdTypeList;
			declaration.JE_TransportMode = Constants.TransportModes.Sea; //Lookups.TransportTypeList;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import; //Lookups.MessageTypeList;
			declaration.JE_MessageSubType = "MST"; //Lookups.MessageSubTypeList;
			declaration.JE_ShipmentIncoTerm = Constants.IncoTerms.DeliveredAtFrontier;
			declaration.JE_RS_NKServiceLevel = "SLV";
			declaration.JE_MessageStatus = "MSA";
			declaration.JE_EntryStatus = "ESA";

			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 5, 5);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 5, 6);
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 5, 7);
			declaration.JE_RL_NKFinalDestination = "NZCHC";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 5, 8);

			declaration.JE_MasterBill = "MASTERME";
			declaration.JE_HouseBill = "HOUSEME";
			declaration.JE_TotalNoOfPacks = 34;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			declaration.JE_TotalWeight = 55.4;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalVolume = 32.45;
			declaration.JE_TotalVolumeUnit = "M3";

			declaration.JE_TotalNoOfPieces = 68;

			declaration.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2006, 1, 1);
			declaration.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2006, 1, 2);
			declaration.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2006, 1, 3);
			declaration.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2006, 1, 4);
			declaration.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2006, 2, 1);
			declaration.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2006, 2, 2);
			declaration.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2006, 2, 3);
			declaration.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2006, 2, 4);

			declaration.DocsAndCartage.JP_CustomAttrib1 = "ONE";
			declaration.DocsAndCartage.JP_CustomAttrib2 = "TWO";
			declaration.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2001, 1, 1);
			declaration.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2002, 2, 2);
			declaration.DocsAndCartage.JP_CustomDecimal1 = 1.1m;
			declaration.DocsAndCartage.JP_CustomDecimal2 = 2.22m;
			declaration.DocsAndCartage.JP_CustomFlag1 = true;
			declaration.DocsAndCartage.JP_CustomFlag2 = false;

			declaration.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2007, 2, 2);
			declaration.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2007, 2, 4);
			declaration.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2007, 3, 3);
			declaration.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2007, 3, 5);

			declaration.JE_AgentsReference = "AGENT_1";

			declaration.HouseBillIssuedDate = new ZDateTime(2005, 4, 8);
			declaration.JE_GoodsDescription = "JILTED LOVERS";
			declaration.JE_OwnerRef = "OWNERS OF";
			declaration.JE_MarksAndNumbers = "BROKEN HEARTS";

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000026";
			container.CO_FCL_LCL_AIR = Constants.ContainerModes.Containerised;

			Bill houseBill = declaration.PrimaryHouseBill;
			AssertNotNull("Precondition: declaration.PrimaryBill should not be null", houseBill.CU_HouseBill);

			BasePackingGroup packingGroup = (declaration.PackingGroups.Count > 0) ? declaration.PackingGroups[0] : declaration.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = houseBill.PK;
			packingGroup.CR_CO_Container = container.PK;

			BasePackage package1 = (declaration.Packages.Count > 0) ? declaration.Packages[0] : declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = packingGroup.PK;
			package1.CW_PackQty = 11;
			package1.CW_PackType = "PK";

			BasePackage package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = packingGroup.PK;
			package2.CW_PackQty = 22;
			package2.CW_PackType = "PK";

			BasePackage package3 = declaration.Packages.AddNew();
			package3.CW_CR_HouseContainer = packingGroup.PK;
			package3.CW_PackQty = 33;
			package3.CW_PackType = "PK";

			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.DeliveredAtFrontier;

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "1ONE1";
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "2TWO2";

			declaration.AttachedOrders.AddNew();
			declaration.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYME").MainAddress.PK;

			declaration.DocsAndCartage.RequiredDocuments.AddNew();
			declaration.Transports.RemoveAndDeleteAll();
			#endregion

			FreightWrapperFromDeclaration fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("fullWrapper.ShipmentContainerMode.Code", Constants.ContainerModes.Loose, fullWrapper.ShipmentContainerMode.Code);
			AssertEquals("fullWrapper.ShipmentTransportMode.Code", Constants.TransportModes.Sea, fullWrapper.ShipmentTransportMode.Code);
			AssertEquals("fullWrapper.ShipmentType.Code", JobMessageTypeList.Codes.Import, fullWrapper.ShipmentType.Code);
			AssertEquals("fullWrapper.IncoTerm.Code", Constants.IncoTerms.DeliveredAtFrontier, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "PPD - Prepaid", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.ServiceLevel.Code", "SLV", fullWrapper.ServiceLevel.Code);
			AssertEquals("fullWrapper.ShipmentStatus.Code", "ESA", fullWrapper.ShipmentStatus.Code);

			AssertEquals("fullWrapper.Consignee.CompanyName", "IMPORTER", fullWrapper.Consignee.CompanyName);
			AssertEquals("fullWrapper.Consignor.CompanyName", "SUPPLIER", fullWrapper.Consignor.CompanyName);
			AssertEquals("fullWrapper.Carrier.CompanyName", "SHIPPINGLINE", fullWrapper.Carrier.CompanyName);
			AssertEquals("fullWrapper.DeliveryAgent.CompanyName", "DELIVERYCARTAGE", fullWrapper.DeliveryAgent.CompanyName);
			AssertEquals("fullWrapper.ImportAgent.CompanyName", "FORWARDER", fullWrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", ZString.Empty, fullWrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ImportBroker.CompanyName", "OURBRANCH", fullWrapper.ImportBroker.CompanyName);
			AssertEquals("fullWrapper.ExportBroker.CompanyName", ZString.Empty, fullWrapper.ExportBroker.CompanyName);
			AssertEquals("fullWrapper.LocalForwarder.CompanyName", "FORWARDER", fullWrapper.LocalForwarder.CompanyName);

			AssertEquals("fullWrapper.NotifyParty", "NOTIFYME", fullWrapper.NotifyParty.CompanyName);

			AssertEquals("fullWrapper.DeliveryAddress.CompanyName", "IMPORTER", fullWrapper.DeliveryAddress.CompanyName);
			AssertEquals("fullWrapper.PickupAddress.CompanyName", "SUPPLIER", fullWrapper.PickupAddress.CompanyName);

			AssertEquals("Port Of Loading UNLOCO", "USLAX", fullWrapper.ShipmentRoutes["First"].Destination.UNLOCO);
			AssertEquals("Port Of Loading ETD", new ZDateTime(2006, 5, 5), fullWrapper.ShipmentRoutes["First"].EstimatedDeparture);
			AssertEquals("Port Of Loading ATD", ZDateTime.Empty, fullWrapper.ShipmentRoutes["First"].ActualDeparture);
			AssertEquals("Port Of Discharge UNLOCO", "NZAKL", fullWrapper.ShipmentRoutes["Last"].Origin.UNLOCO);
			AssertEquals("Port Of Discharge ETA", new ZDateTime(2006, 5, 8), fullWrapper.ShipmentRoutes["Last"].EstimatedArrival);
			AssertEquals("Port Of Discharge ATA", ZDateTime.Empty, fullWrapper.ShipmentRoutes["Last"].ActualArrival);

			AssertEquals("fullWrapper.Origin.Location.UNLOCO", "USDNV", fullWrapper.Origin.Location.UNLOCO);
			AssertEquals("fullWrapper.Origin.EstimatedDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.EstimatedDate);
			AssertEquals("fullWrapper.Origin.ActualDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.Location.UNLOCO", "NZCHC", fullWrapper.Destination.Location.UNLOCO);
			AssertEquals("fullWrapper.Destination.EstimatedDate", new ZDateTime(2006, 5, 8), fullWrapper.Destination.EstimatedDate);
			AssertEquals("fullWrapper.Destination.ActualDate", new ZDateTime(2006, 5, 8), fullWrapper.Destination.ActualDate);

			AssertEquals("fullWrapper.ShipmentOuterPacksQty.Value", 34m, fullWrapper.ShipmentOuterPacksQty.Value);
			AssertEquals("fullWrapper.ShipmentOuterPacksQty.Unit.Code", "PK", fullWrapper.ShipmentOuterPacksQty.Unit.Code);
			AssertEquals("fullWrapper.Volume.Value", 32.450m, fullWrapper.Volume.Value);
			AssertEquals("fullWrapper.Volume.Unit.Code", "M3", fullWrapper.Volume.Unit.Code);
			AssertEquals("fullWrapper.Weight.Value", 55.4m, fullWrapper.Weight.Value);
			AssertEquals("fullWrapper.Weight.Unit.Code", "KG", fullWrapper.Weight.Unit.Code);
			AssertEquals("fullWrapper.ShipmentInnerPacksQty.Value", 68m, fullWrapper.ShipmentInnerPacksQty.Value);
			AssertEquals("fullWrapper.ShipmentInnerPacksQty.Unit.Code", ZString.Empty, fullWrapper.ShipmentInnerPacksQty.Unit.Code);

			ZDateTime dateTimeCreated = declaration.LogsOfDeclarationOrShipment.CreatedDateUtc;
			AssertEquals("fullWrapper.ConsolDateCreated", dateTimeCreated, fullWrapper.ConsolDateCreated);
			AssertEquals("fullWrapper.ShipmentDateCreated", dateTimeCreated, fullWrapper.ShipmentDateCreated);

			AssertEquals("fullWrapper.MasterBill", "MASTERME", fullWrapper.MasterBill);
			AssertEquals("fullWrapper.MasterBillHeading", "Ocean Bill Of Lading", fullWrapper.MasterBillHeading);
			AssertEquals("fullWrapper.HouseBill", "HOUSEME", fullWrapper.HouseBill);
			AssertEquals("fullWrapper.HouseBillHeading", "House Bill Of Lading", fullWrapper.HouseBillHeading);
			AssertEquals("fullWrapper.HBLIssueDate", new ZDateTime(2005, 4, 8), fullWrapper.HouseBillIssue);
			AssertEquals("fullWrapper.GoodsDescription", "JILTED LOVERS", fullWrapper.GoodsDescription);
			AssertEquals("fullWrapper.OrderNumbersWithOwnersReference", "OWNERS OF", fullWrapper.OrderNumbersWithOwnersReference);
			AssertEquals("fullWrapper.OwnerReference", "OWNERS OF", fullWrapper.OwnerReference);
			AssertEquals("fullWrapper.MarksAndNumbers", "BROKEN HEARTS", fullWrapper.MarksAndNumbers);
			AssertEquals("fullWrapper.ExportAgentsReference", ZString.Empty, fullWrapper.ExportAgentsReference);
			AssertEquals("fullWrapper.ImportAgentsReference", "AGENT_1", fullWrapper.ImportAgentsReference);
			AssertEquals("fullWrapper.LocalForwarderReference", ZString.Empty, fullWrapper.LocalForwarderReference);
			AssertEquals("fullWrapper.HBLContainerMode", Constants.ContainerModes.FCL, fullWrapper.HBLContainerMode);

			AssertEquals("fullWrapper.DeliveryCartageAdvised", new ZDateTime(2006, 1, 1), fullWrapper.DeliveryCartageAdvised);
			AssertEquals("fullWrapper.DeliveryFrom", new ZDateTime(2006, 1, 2), fullWrapper.DeliveryFrom);
			AssertEquals("fullWrapper.DeliveryGoodsDelivered", new ZDateTime(2006, 1, 3), fullWrapper.DeliveryGoodsDelivered);
			AssertEquals("fullWrapper.DeliveryRequiredBy", new ZDateTime(2006, 1, 4), fullWrapper.DeliveryRequiredBy);
			AssertEquals("fullWrapper.PickupCartageAdvised", new ZDateTime(2006, 2, 1), fullWrapper.PickupCartageAdvised);
			AssertEquals("fullWrapper.PickupFrom", new ZDateTime(2006, 2, 2), fullWrapper.PickupFrom);
			AssertEquals("fullWrapper.PickupGoodsPickedup", new ZDateTime(2006, 2, 3), fullWrapper.PickupGoodsPickedup);
			AssertEquals("fullWrapper.PickupRequiredBy", new ZDateTime(2006, 2, 4), fullWrapper.PickupRequiredBy);
			AssertEquals("fullWrapper.PickupDateOfReceipt", ZDateTime.Empty, fullWrapper.PickupDateOfReceipt);

			AssertEquals("fullWrapper.CustomAttribute1", "ONE", fullWrapper.CustomAttribute1);
			AssertEquals("fullWrapper.CustomAttribute2", "TWO", fullWrapper.CustomAttribute2);
			AssertEquals("fullWrapper.CustomDate1", new ZDateTime(2001, 1, 1), fullWrapper.CustomDate1);
			AssertEquals("fullWrapper.CustomDate2", new ZDateTime(2002, 2, 2), fullWrapper.CustomDate2);
			AssertEquals("fullWrapper.CustomDecimal1", 1.1m, fullWrapper.CustomDecimal1);
			AssertEquals("fullWrapper.CustomDecimal2", 2.22m, fullWrapper.CustomDecimal2);
			AssertEquals("fullWrapper.CustomFlag1", true, fullWrapper.CustomFlag1);
			AssertEquals("fullWrapper.CustomFlag2", false, fullWrapper.CustomFlag2);

			AssertEquals("fullWrapper.CommercialInvoices.Count", 1, fullWrapper.CommercialInvoices.Count);
			AssertEquals("fullWrapper.CommercialInvoiceLines.Count", 2, fullWrapper.CommercialInvoiceLines.Count);
			AssertEquals("fullWrapper.GoodsValue.Amount", ZDecimal.Zero, fullWrapper.GoodsValue.Amount);
			AssertEquals("fullWrapper.GoodsValue.Currency.Code", ZString.Empty, fullWrapper.GoodsValue.Currency.Code);

			AssertEquals("fullWrapper.Containers.Count", 1, fullWrapper.Containers.Count);
			AssertEquals("fullWrapper.Packages.Count", 3, fullWrapper.Packages.Count);

			AssertEquals("fullWrapper.ConsolRoutes.Count", 1, fullWrapper.ConsolRoutes.Count);
			AssertEquals("fullWrapper.ShipmentRoutes.Count", 3, fullWrapper.ShipmentRoutes.Count);

			AssertEquals("fullWrapper.CustomsEntries.Count", 2, fullWrapper.CustomsEntries.Count);
			AssertEquals("fullWrapper.Orders.Count", 1, fullWrapper.Orders.Count);

			AssertEquals("fullWrapper.FreightJobs.Count", 0, fullWrapper.FreightJobs.Count);
			AssertEquals("fullWrapper.FreightConsolidations.Count", 0, fullWrapper.FreightConsolidations.Count);

			AssertEquals("fullWrapper.RequiredDocuments.Count", 1, fullWrapper.RequiredDocuments.Count);

			AssertEquals("fullWrapper.Services.Count", 0, fullWrapper.Services.Count);

			AssertNull("fullWrapper.DangerousGoodsAdditionalHandlingInformation", fullWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description]);
			AssertNull("fullWrapper.CertificateOfOriginNotes", fullWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description]);
			AssertNull("fullWrapper.PreAlertArrivalNoticeRemarks", fullWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description]);

			AssertEquals("fullWrapper.CaratagePickupMode.Code", ZString.Empty, fullWrapper.CaratagePickupMode.Code);

			AssertEquals("fullWrapper.ArrivalReference", ZString.Empty, fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.CTOArrival.CompanyName", "I HATE DOCUMENTS", fullWrapper.CTOArrival.CompanyName);

			AssertEquals("fullWrapper.ReceivingForwarder.CompanyName", "FORWARDER", fullWrapper.ReceivingForwarder.CompanyName);
			AssertEquals("fullWrapper.SendingForwarder.CompanyName", ZString.Empty, fullWrapper.SendingForwarder.CompanyName);

			AssertOrgWrappersReturnRightTypes(fullWrapper);

			CustomsIncoTermOverrideCollection collection = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.Value;
			collection.AddNew(Constants.IncoTerms.DeliveredAtFrontier, Core.Constants.IncoTerms.DeliveredDutyPaid);
			DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("fullWrapper.IncoTerm.Code", Constants.IncoTerms.DeliveredDutyPaid, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "PPD - Prepaid", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);

			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", "AUSTRALIA", fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivalAddress.Address);

			AssertEquals("fullWrapper.PickupLocation", "USDNV", fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", "NZCHC", fullWrapper.DeliveryLocation.UNLOCO);
			AssertEquals("fullWrapper.FreightPayableAt", ZString.Empty, fullWrapper.FreightPayableAt.UNLOCO);
		}

		public void TestWrapperMappingExport()
		{
			#region Setup
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();

			declaration.JE_OH_Importer = GetOrgHeader("IMPORTER").PK;
			declaration.JE_OH_Supplier = GetOrgHeader("SUPPLIER").PK;
			declaration.JE_OH_Forwarder = GetOrgHeader("FORWARDER").PK;
			declaration.JE_OH_ShippingLine = GetOrgHeader("SHIPPINGLINE").PK;
			declaration.DocsAndCartage.DeliveryCartageCoPK = GetOrgHeader("DELIVERYCARTAGE").PK;
			declaration.Branch.GB_OH_OrgProxy = GetOrgHeader("OURBRANCH").PK;

			declaration.JE_ContainerMode = Constants.ContainerModes.Containerised;  //Lookups.CargoIdTypeList;
			declaration.JE_TransportMode = Constants.TransportModes.Sea; //Lookups.TransportTypeList;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export; //Lookups.MessageTypeList;
			declaration.JE_MessageSubType = "MST"; //Lookups.MessageSubTypeList;
			declaration.JE_ShipmentIncoTerm = Constants.IncoTerms.DeliveredAtFrontier;
			declaration.JE_RS_NKServiceLevel = "SLV";
			declaration.JE_MessageStatus = "MSA";
			declaration.JE_EntryStatus = "ESA";
			declaration.JE_VesselName = "vessel";
			declaration.JE_VoyageFlightNo = "FR001";

			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_DateAtOrigin = new ZDateTime(2006, 5, 5);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_ExportDate = new ZDateTime(2006, 5, 6);
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			declaration.JE_DateOfArrival = new ZDateTime(2006, 5, 7);
			declaration.JE_RL_NKFinalDestination = "NZCHC";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2006, 5, 8);

			declaration.JE_MasterBill = "MASTERME";
			declaration.JE_HouseBill = "HOUSEME";
			declaration.JE_TotalNoOfPacks = 34;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			declaration.JE_TotalWeight = 55.4;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalVolume = 32.45;
			declaration.JE_TotalVolumeUnit = "M3";

			declaration.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2006, 1, 1);
			declaration.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2006, 1, 2);
			declaration.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2006, 1, 3);
			declaration.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2006, 1, 4);
			declaration.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2006, 2, 1);
			declaration.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2006, 2, 2);
			declaration.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2006, 2, 3);
			declaration.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2006, 2, 4);

			declaration.DocsAndCartage.JP_CustomAttrib1 = "ONE";
			declaration.DocsAndCartage.JP_CustomAttrib2 = "TWO";
			declaration.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2001, 1, 1);
			declaration.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2002, 2, 2);
			declaration.DocsAndCartage.JP_CustomDecimal1 = 1.1m;
			declaration.DocsAndCartage.JP_CustomDecimal2 = 2.22m;
			declaration.DocsAndCartage.JP_CustomFlag1 = true;
			declaration.DocsAndCartage.JP_CustomFlag2 = false;

			declaration.JE_AgentsReference = "AGENT_1";

			declaration.HouseBillIssuedDate = new ZDateTime(2005, 4, 8);
			declaration.JE_GoodsDescription = "JILTED LOVERS";
			declaration.JE_OwnerRef = "OWNERS OF";
			declaration.JE_MarksAndNumbers = "BROKEN HEARTS";

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000026";
			container.CO_FCL_LCL_AIR = Constants.ContainerModes.Containerised;

			Bill houseBill = declaration.PrimaryHouseBill;
			AssertNotNull("Precondition: declaration.PrimaryBill should not be null", houseBill.CU_HouseBill);

			BasePackingGroup packingGroup = (declaration.PackingGroups.Count > 0) ? declaration.PackingGroups[0] : declaration.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = houseBill.PK;
			packingGroup.CR_CO_Container = container.PK;

			BasePackage package1 = (declaration.Packages.Count > 0) ? declaration.Packages[0] : declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = packingGroup.PK;
			package1.CW_PackQty = 11;
			package1.CW_PackType = "PK";

			BasePackage package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = packingGroup.PK;
			package2.CW_PackQty = 22;
			package2.CW_PackType = "PK";

			BasePackage package3 = declaration.Packages.AddNew();
			package3.CW_CR_HouseContainer = packingGroup.PK;
			package3.CW_PackQty = 33;
			package3.CW_PackType = "PK";

			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.DeliveredAtFrontier;

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "1ONE1";
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "2TWO2";

			declaration.AttachedOrders.AddNew();
			declaration.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYME").MainAddress.PK;

			declaration.DocsAndCartage.RequiredDocuments.AddNew();

			#endregion

			FreightWrapperFromDeclaration fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("fullWrapper.ShipmentContainerMode.Code", Constants.ContainerModes.Loose, fullWrapper.ShipmentContainerMode.Code);
			AssertEquals("fullWrapper.ShipmentTransportMode.Code", Constants.TransportModes.Sea, fullWrapper.ShipmentTransportMode.Code);
			AssertEquals("fullWrapper.ShipmentType.Code", JobMessageTypeList.Codes.Export, fullWrapper.ShipmentType.Code);
			AssertEquals("fullWrapper.OrderTransportMode.Code", ZString.Empty, fullWrapper.OrderTransportMode.Code);
			AssertEquals("fullWrapper.IncoTerm.Code", Constants.IncoTerms.DeliveredAtFrontier, fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "PPD - Prepaid", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.ServiceLevel.Code", "SLV", fullWrapper.ServiceLevel.Code);
			AssertEquals("fullWrapper.ShipmentStatus.Code", "ESA", fullWrapper.ShipmentStatus.Code);

			AssertEquals("fullWrapper.Consignee.CompanyName", "IMPORTER", fullWrapper.Consignee.CompanyName);
			AssertEquals("fullWrapper.Consignor.CompanyName", "SUPPLIER", fullWrapper.Consignor.CompanyName);
			AssertEquals("fullWrapper.Carrier.CompanyName", "SHIPPINGLINE", fullWrapper.Carrier.CompanyName);
			AssertEquals("fullWrapper.DeliveryAgent.CompanyName", ZString.Empty, fullWrapper.DeliveryAgent.CompanyName);
			AssertEquals("fullWrapper.ImportAgent.CompanyName", ZString.Empty, fullWrapper.ImportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", "FORWARDER", fullWrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ImportBroker.CompanyName", ZString.Empty, fullWrapper.ImportBroker.CompanyName);
			AssertEquals("fullWrapper.ExportBroker.CompanyName", "OURBRANCH", fullWrapper.ExportBroker.CompanyName);
			AssertEquals("fullWrapper.BookingParty.CompanyName", ZString.Empty, fullWrapper.BookingParty.CompanyName);
			AssertEquals("fullWrapper.LocalForwarder.CompanyName", "FORWARDER", fullWrapper.LocalForwarder.CompanyName);

			AssertEquals("fullWrapper.NotifyParty.CompanyName", "NOTIFYME", fullWrapper.NotifyParty.CompanyName);

			AssertEquals("fullWrapper.DeliveryAddress.CompanyName", "IMPORTER", fullWrapper.DeliveryAddress.CompanyName);
			AssertEquals("fullWrapper.PickupAddress.CompanyName", "SUPPLIER", fullWrapper.PickupAddress.CompanyName);

			AssertEquals("fullWrapper.Origin.Location.UNLOCO", "USDNV", fullWrapper.Origin.Location.UNLOCO);
			AssertEquals("fullWrapper.Origin.EstimatedDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.EstimatedDate);
			AssertEquals("fullWrapper.Origin.ActualDate", new ZDateTime(2006, 5, 5), fullWrapper.Origin.ActualDate);
			AssertEquals("fullWrapper.Destination.Location.UNLOCO", "NZCHC", fullWrapper.Destination.Location.UNLOCO);
			AssertEquals("fullWrapper.Destination.EstimatedDate", new ZDateTime(2006, 5, 8), fullWrapper.Destination.EstimatedDate);
			AssertEquals("fullWrapper.Destination.ActualDate", new ZDateTime(2006, 5, 8), fullWrapper.Destination.ActualDate);

			AssertEquals("fullWrapper.ShipmentOuterPacksQty.Value", 34m, fullWrapper.ShipmentOuterPacksQty.Value);
			AssertEquals("fullWrapper.ShipmentOuterPacksQty.Unit.Code", "PK", fullWrapper.ShipmentOuterPacksQty.Unit.Code);
			AssertEquals("fullWrapper.Volume.Value", 32.450m, fullWrapper.Volume.Value);
			AssertEquals("fullWrapper.Volume.Unit.Code", "M3", fullWrapper.Volume.Unit.Code);
			AssertEquals("fullWrapper.Weight.Value", 55.4m, fullWrapper.Weight.Value);
			AssertEquals("fullWrapper.Weight.Unit.Code", "KG", fullWrapper.Weight.Unit.Code);
			AssertEquals("fullWrapper.ShipmentInnerPacksQty.Value", ZDecimal.Zero, fullWrapper.ShipmentInnerPacksQty.Value);
			AssertEquals("fullWrapper.ShipmentInnerPacksQty.Unit.Code", ZString.Empty, fullWrapper.ShipmentInnerPacksQty.Unit.Code);

			ZDateTime dateTimeCreated = declaration.LogsOfDeclarationOrShipment.CreatedDateUtc;
			AssertEquals("fullWrapper.ConsolDateCreated", dateTimeCreated, fullWrapper.ConsolDateCreated);
			AssertEquals("fullWrapper.ShipmentDateCreated", dateTimeCreated, fullWrapper.ShipmentDateCreated);

			AssertEquals("fullWrapper.MasterBill", "MASTERME", fullWrapper.MasterBill);
			AssertEquals("fullWrapper.MasterBillHeading", "Ocean Bill Of Lading", fullWrapper.MasterBillHeading);
			AssertEquals("fullWrapper.HouseBill", "HOUSEME", fullWrapper.HouseBill);
			AssertEquals("fullWrapper.HBLIssueDate", new ZDateTime(2005, 4, 8), fullWrapper.HouseBillIssue);
			AssertEquals("fullWrapper.GoodsDescription", "JILTED LOVERS", fullWrapper.GoodsDescription);
			AssertEquals("fullWrapper.OrderNumbersWithOwnersReference", "OWNERS OF", fullWrapper.OrderNumbersWithOwnersReference);
			AssertEquals("fullWrapper.OwnerReference", "OWNERS OF", fullWrapper.OwnerReference);
			AssertEquals("fullWrapper.MarksAndNumbers", "BROKEN HEARTS", fullWrapper.MarksAndNumbers);
			AssertEquals("fullWrapper.ExportAgentsReference", ZString.Empty, fullWrapper.ExportAgentsReference);
			AssertEquals("fullWrapper.HBLContainerMode", Constants.ContainerModes.Containerised, fullWrapper.HBLContainerMode);
			AssertEquals("fullWrapper.ImportAgentsReference", "AGENT_1", fullWrapper.ImportAgentsReference);
			AssertEquals("fullWrapper.LocalForwarderReference", ZString.Empty, fullWrapper.LocalForwarderReference);

			AssertEquals("fullWrapper.DeliveryCartageAdvised", new ZDateTime(2006, 1, 1), fullWrapper.DeliveryCartageAdvised);
			AssertEquals("fullWrapper.DeliveryFrom", new ZDateTime(2006, 1, 2), fullWrapper.DeliveryFrom);
			AssertEquals("fullWrapper.DeliveryGoodsDelivered", new ZDateTime(2006, 1, 3), fullWrapper.DeliveryGoodsDelivered);
			AssertEquals("fullWrapper.DeliveryRequiredBy", new ZDateTime(2006, 1, 4), fullWrapper.DeliveryRequiredBy);
			AssertEquals("fullWrapper.PickupCartageAdvised", new ZDateTime(2006, 2, 1), fullWrapper.PickupCartageAdvised);
			AssertEquals("fullWrapper.PickupFrom", new ZDateTime(2006, 2, 2), fullWrapper.PickupFrom);
			AssertEquals("fullWrapper.PickupGoodsPickedup", new ZDateTime(2006, 2, 3), fullWrapper.PickupGoodsPickedup);
			AssertEquals("fullWrapper.PickupRequiredBy", new ZDateTime(2006, 2, 4), fullWrapper.PickupRequiredBy);
			AssertEquals("fullWrapper.PickupDateOfReceipt", ZDateTime.Empty, fullWrapper.PickupDateOfReceipt);

			AssertEquals("fullWrapper.CustomAttribute1", "ONE", fullWrapper.CustomAttribute1);
			AssertEquals("fullWrapper.CustomAttribute2", "TWO", fullWrapper.CustomAttribute2);
			AssertEquals("fullWrapper.CustomDate1", new ZDateTime(2001, 1, 1), fullWrapper.CustomDate1);
			AssertEquals("fullWrapper.CustomDate2", new ZDateTime(2002, 2, 2), fullWrapper.CustomDate2);
			AssertEquals("fullWrapper.CustomDecimal1", 1.1m, fullWrapper.CustomDecimal1);
			AssertEquals("fullWrapper.CustomDecimal2", 2.22m, fullWrapper.CustomDecimal2);
			AssertEquals("fullWrapper.CustomFlag1", true, fullWrapper.CustomFlag1);
			AssertEquals("fullWrapper.CustomFlag2", false, fullWrapper.CustomFlag2);

			AssertEquals("fullWrapper.CommercialInvoices.Count", 1, fullWrapper.CommercialInvoices.Count);
			AssertEquals("fullWrapper.CommercialInvoiceLines.Count", 2, fullWrapper.CommercialInvoiceLines.Count);
			AssertEquals("fullWrapper.GoodsValue.Amount", ZDecimal.Zero, fullWrapper.GoodsValue.Amount);
			AssertEquals("fullWrapper.GoodsValue.Currency.Code", ZString.Empty, fullWrapper.GoodsValue.Currency.Code);

			AssertEquals("fullWrapper.Containers.Count", 1, fullWrapper.Containers.Count);
			AssertEquals("fullWrapper.Packages.Count", 3, fullWrapper.Packages.Count);

			AssertEquals("fullWrapper.ConsolRoutes.Count", 1, fullWrapper.ConsolRoutes.Count);
			AssertEquals("fullWrapper.ShipmentRoutes.Count", 1, fullWrapper.ShipmentRoutes.Count);

			AssertEquals("fullWrapper.CustomsEntries.Count", 2, fullWrapper.CustomsEntries.Count);
			AssertEquals("fullWrapper.Orders.Count", 1, fullWrapper.Orders.Count);

			AssertEquals("fullWrapper.FreightJobs.Count", 0, fullWrapper.FreightJobs.Count);
			AssertEquals("fullWrapper.FreightConsolidations.Count", 0, fullWrapper.FreightConsolidations.Count);

			AssertEquals("fullWrapper.RequiredDocuments.Count", 1, fullWrapper.RequiredDocuments.Count);

			AssertEquals("fullWrapper.Services.Count", 0, fullWrapper.Services.Count);

			AssertNull("fullWrapper.DangerousGoodsAdditionalHandlingInformation", fullWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description]);
			AssertNull("fullWrapper.CertificateOfOriginNotes", fullWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description]);
			AssertNull("fullWrapper.PreAlertArrivalNoticeRemarks", fullWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description]);

			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);

			AssertEquals("fullWrapper.ReceivingForwarder.CompanyName", ZString.Empty, fullWrapper.ReceivingForwarder.CompanyName);
			AssertEquals("fullWrapper.SendingForwarder.CompanyName", "FORWARDER", fullWrapper.SendingForwarder.CompanyName);

			AssertOrgWrappersReturnRightTypes(fullWrapper);
		}

		[TestDate(2007, 1, 1)]
		public void TestWrapperExportPickup()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "ABX";
			declaration.JE_DeliveryOrPickupLabourCharge = 13.00m;
			declaration.JE_DeliveryOrPickupLabourTime = ZDateTime.Now.AddHours(26).AddMinutes(15);
			declaration.JE_PickupOrDeliveryTruckWaitCharge = 23.56m;
			declaration.JE_PickupOrDeliveryTruckWaitTime = ZDateTime.Now.AddHours(-34).AddMinutes(-30);
			declaration.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 1;
			OrgAddress depotAddress = Factory.New<OrgAddress>();
			depotAddress.OA_Address1 = "TEST 1 ADDRESS";
			depotAddress.OA_RN_NKCountryCode = "AU";
			declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;

			FreightWrapperFromDeclaration fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("fullWrapper.CaratagePickupMode.Code", "ABX", fullWrapper.CaratagePickupMode.Code);
			AssertEquals("fullWrapper.PickupCFSAddress.Address", "TEST 1 ADDRESS\nAUSTRALIA", fullWrapper.PickupCFSAddress.Address);
			AssertEquals("fullWrapper.PickupInterimReceipt", ZString.Empty, fullWrapper.PickupInterimReceipt);
			AssertEquals("fullWrapper.PickupLabourCharge", 13.00m, fullWrapper.PickupLabourCharge);
			AssertEquals("fullWrapper.PickupLabourTime", "26:15", fullWrapper.PickupLabourTime);
			AssertEquals("fullWrapper.PickupTruckWaitCharge", 23.56m, fullWrapper.PickupTruckWaitCharge);
			AssertEquals("fullWrapper.PickupTruckWaitTime", "-34:30", fullWrapper.PickupTruckWaitTime);
			AssertEquals("fullWrapper.StorgeTime", "1 Days", fullWrapper.StorageTime.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.UnpackCFSAddress.Address", ZString.Empty, fullWrapper.UnpackCFSAddress.Address);
			AssertEquals("fullWrapper.GoodsAvailableAt.Address", "TEST 1 ADDRESS\nAUSTRALIA", fullWrapper.GoodsAvailableAt.Address);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", "TEST 1 ADDRESS\nAUSTRALIA", fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", "TEST 1 ADDRESS\nAUSTRALIA", fullWrapper.ExportReceivalAddress.Address);
			AssertEquals("fullWrapper.PickupLocation", ZString.Empty, fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", ZString.Empty, fullWrapper.DeliveryLocation.UNLOCO);
			AssertEquals("fullWrapper.FreightPayableAt", ZString.Empty, fullWrapper.FreightPayableAt.UNLOCO);
		}

		[TestDate(2007, 1, 1)]
		public void TestWrapperImportPickup()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "ABX";
			declaration.JE_DeliveryOrPickupLabourCharge = 10.00m;
			declaration.JE_DeliveryOrPickupLabourTime = ZDateTime.Now.AddHours(126).AddMinutes(30);
			declaration.JE_PickupOrDeliveryTruckWaitCharge = 435.89m;
			declaration.JE_PickupOrDeliveryTruckWaitTime = ZDateTime.Now.AddHours(-345).AddMinutes(-34);
			declaration.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 19;
			OrgAddress depotAddress = Factory.New<OrgAddress>();
			depotAddress.OA_Address1 = "TEST 2 ADDRESS";
			depotAddress.OA_RN_NKCountryCode = "AU";
			declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;

			FreightWrapperFromDeclaration fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("fullWrapper.CaratagePickupMode.Code", ZString.Empty, fullWrapper.CaratagePickupMode.Code);
			AssertEquals("fullWrapper.PickupCFSAddress.Address", ZString.Empty, fullWrapper.PickupCFSAddress.Address);
			AssertEquals("fullWrapper.PickupInterimReceipt", ZString.Empty, fullWrapper.PickupInterimReceipt);
			AssertEquals("fullWrapper.PickupLabourCharge", ZDecimal.Zero, fullWrapper.PickupLabourCharge);
			AssertEquals("fullWrapper.PickupLabourTime", ZString.Empty, fullWrapper.PickupLabourTime);
			AssertEquals("fullWrapper.PickupTruckWaitCharge", ZDecimal.Zero, fullWrapper.PickupTruckWaitCharge);
			AssertEquals("fullWrapper.PickupTruckWaitTime", ZString.Empty, fullWrapper.PickupTruckWaitTime);
			AssertEquals("fullWrapper.StorgeTime", ZString.Empty, fullWrapper.StorageTime.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.UnpackCFSAddress.Address", "TEST 2 ADDRESS\nAUSTRALIA", fullWrapper.UnpackCFSAddress.Address);
			AssertEquals("fullWrapper.GoodsAvailableAt.Address", "TEST 2 ADDRESS\nAUSTRALIA", fullWrapper.GoodsAvailableAt.Address);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", "TEST 2 ADDRESS\nAUSTRALIA", fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", "TEST 2 ADDRESS\nAUSTRALIA", fullWrapper.ExportReceivingDepotAddress.Address);
		}

		[TestDate(2007, 1, 1)]
		public void TestWrapperFallbackFreightExportPickup()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "ABX";
			shipment.DocsAndCartage.JP_PickupLabourCharge = 15.00m;
			shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Now.AddHours(87).AddMinutes(45);
			shipment.DocsAndCartage.JP_PickupTruckWaitCharge = 13.45m;
			shipment.DocsAndCartage.JP_PickupTruckWaitTime = ZDateTime.Now.AddHours(-87).AddMinutes(-35);
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 59;
			shipment.JS_InterimReceipt = "1.343";
			shipment.JS_A_RCV = new ZDateTime(2007, 2, 2);
			shipment.JS_BookingReference = "A SHIP REF";
			OrgAddress pickupdepotAddress = Factory.New<OrgAddress>();
			pickupdepotAddress.OA_Address1 = "TEST 1 ADDRESS SHIPMENT";
			pickupdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = pickupdepotAddress.PK;
			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST 2 ADDRESS SHIPMENT";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ImportReleaseDepot = unpackdepotAddress.PK;

			FreightWrapperFromDeclaration fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("fullWrapper.CaratagePickupMode.Code", "ABX", fullWrapper.CaratagePickupMode.Code);
			AssertEquals("fullWrapper.PickupCFSAddress.Address", "TEST 1 ADDRESS SHIPMENT\nAUSTRALIA", fullWrapper.PickupCFSAddress.Address);
			AssertEquals("fullWrapper.UnpackCFSAddress.Address", "TEST 2 ADDRESS SHIPMENT\nAUSTRALIA", fullWrapper.UnpackCFSAddress.Address);
			AssertEquals("fullWrapper.GoodsAvailableAt.Address", "TEST 2 ADDRESS SHIPMENT\nAUSTRALIA", fullWrapper.GoodsAvailableAt.Address);
			AssertEquals("fullWrapper.PickupInterimReceipt", "1.343", fullWrapper.PickupInterimReceipt);
			AssertEquals("fullWrapper.ActualReceive", new ZDateTime(2007, 2, 2), fullWrapper.ActualReceive);
			AssertEquals("fullWrapper.PickupLabourCharge", 15.00m, fullWrapper.PickupLabourCharge);
			AssertEquals("fullWrapper.PickupLabourTime", "87:45", fullWrapper.PickupLabourTime);
			AssertEquals("fullWrapper.PickupTruckWaitCharge", 13.45m, fullWrapper.PickupTruckWaitCharge);
			AssertEquals("fullWrapper.PickupTruckWaitTime", "-87:35", fullWrapper.PickupTruckWaitTime);
			AssertEquals("fullWrapper.ShippersReference", "A SHIP REF", fullWrapper.ShippersReference);
			AssertEquals("fullWrapper.StorgeTime", "59 Hours", fullWrapper.StorageTime.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", "TEST 1 ADDRESS SHIPMENT\nAUSTRALIA", fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", "TEST 1 ADDRESS SHIPMENT\nAUSTRALIA", fullWrapper.ExportReceivalAddress.Address);
			AssertEquals("fullWrapper.PickupLocation", ZString.Empty, fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", ZString.Empty, fullWrapper.DeliveryLocation.UNLOCO);
			AssertEquals("fullWrapper.FreightPayableAt", ZString.Empty, fullWrapper.FreightPayableAt.UNLOCO);
		}

		[TestDate(2007, 1, 1)]
		public void TestWrapperFallbackFreightImportPickup()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "ABX";
			shipment.DocsAndCartage.JP_PickupLabourCharge = 8.00m;
			shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Now.AddHours(92);
			shipment.DocsAndCartage.JP_PickupTruckWaitCharge = 3.23m;
			shipment.DocsAndCartage.JP_PickupTruckWaitTime = ZDateTime.Now.AddHours(38).AddMinutes(9);
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 34;
			shipment.JS_InterimReceipt = "TRUYDS";
			shipment.JS_A_RCV = new ZDateTime(2007, 3, 3);
			shipment.JS_BookingReference = "A SHIP REF";
			OrgAddress pickupdepotAddress = Factory.New<OrgAddress>();
			pickupdepotAddress.OA_Address1 = "TEST 1 ADDRESS SHIPMENT";
			pickupdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = pickupdepotAddress.PK;

			FreightWrapperFromDeclaration fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("fullWrapper.CaratagePickupMode.Code", "ABX", fullWrapper.CaratagePickupMode.Code);
			AssertEquals("fullWrapper.PickupCFSAddress.Address", "TEST 1 ADDRESS SHIPMENT\nAUSTRALIA", fullWrapper.PickupCFSAddress.Address);
			AssertEquals("fullWrapper.PickupInterimReceipt", "TRUYDS", fullWrapper.PickupInterimReceipt);
			AssertEquals("fullWrapper.ActualReceive", new ZDateTime(2007, 3, 3), fullWrapper.ActualReceive);
			AssertEquals("fullWrapper.PickupLabourCharge", 8.00m, fullWrapper.PickupLabourCharge);
			AssertEquals("fullWrapper.PickupLabourTime", "92:00", fullWrapper.PickupLabourTime);
			AssertEquals("fullWrapper.PickupTruckWaitCharge", 3.23m, fullWrapper.PickupTruckWaitCharge);
			AssertEquals("fullWrapper.PickupTruckWaitTime", "38:09", fullWrapper.PickupTruckWaitTime);
			AssertEquals("fullWrapper.ShippersReference", "A SHIP REF", fullWrapper.ShippersReference);
			AssertEquals("fullWrapper.StorgeTime", "34 Days", fullWrapper.StorageTime.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", "TEST 1 ADDRESS SHIPMENT\nAUSTRALIA", fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", "TEST 1 ADDRESS SHIPMENT\nAUSTRALIA", fullWrapper.ExportReceivalAddress.Address);
			AssertEquals("fullWrapper.PickupLocation", ZString.Empty, fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", ZString.Empty, fullWrapper.DeliveryLocation.UNLOCO);
			AssertEquals("fullWrapper.FreightPayableAt", ZString.Empty, fullWrapper.FreightPayableAt.UNLOCO);
		}

		public void TestWrapperWarehouse()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.JS_WarehouseLocation = "DARWIN";

			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.WarehouseLocation", "DARWIN", wrapper.WarehouseLocation);
		}

		public void TestDeclarationDocumentWrapperNotNull()
		{
			AssertNotNull("Wrapper.Declaration should not be null", Wrapper.Declaration);
		}

		public override void TestBuyer()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", ZString.Empty, wrapper.Buyer.CompanyNameAndAddress);

			JobDocAddress buyerAddress = declaration.BuyerDocAddress;
			buyerAddress.E2_AddressOverride = true;
			buyerAddress.E2_CompanyName = "FRED'S FLAT TYRES";
			buyerAddress.E2_Address1 = "ADDRESS 1";
			buyerAddress.E2_City = "CITY";
			buyerAddress.E2_State = "STATE";
			buyerAddress.E2_Postcode = "PCODE";

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", "FRED'S FLAT TYRES\nADDRESS 1\nCITY STATE PCODE\nANGUILLA", wrapper.Buyer.CompanyNameAndAddress);
		}

		public override void TestWrapperNotes()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
			declaration.Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "TEST HANDLING INFO");
			declaration.Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "TEST CERTIFICATE OF ORIGIN NOTES");
			declaration.Shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, "TEST PRE ALERT ARRIVAL NOTICE REMARKS");

			FreightWrapperFromDeclaration fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("fullWrapper.DangerousGoodsAdditionalHandlingInformation", "TEST HANDLING INFO", fullWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description].Text);
			AssertEquals("fullWrapper.CertificateOfOriginNotes", "TEST CERTIFICATE OF ORIGIN NOTES", fullWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description].Text);
			AssertEquals("fullWrapper.PreAlertArrivalNoticeRemarks", "TEST PRE ALERT ARRIVAL NOTICE REMARKS", fullWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description].Text);
		}

		public void TestPackagesPullsFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OuterPacks = 1;
			ForwardingPackLine shipmentPackLine = shipment.OuterPackLines[0];
			shipmentPackLine.JL_MarksAndNumbers = "Shipment's Marks And Nos";

			var mock = Factory.NewMoq<BaseJobDeclaration>();
			mock.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			BaseJobDeclaration declaration = mock.Object;
			declaration.JE_JS = shipment.PK;
			BasePackage declarationPackage = declaration.Packages.AddNew();
			declarationPackage.CW_MarksAndNos = "Declaration's Marks And Nos";
			FreightWrapperFromDeclaration wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			PackageWrapperCollection packages = wrapper.Packages;
			AssertEquals("Count", 1, packages.Count);
			AssertEquals(declarationPackage.CW_MarksAndNos, packages[0].MarksAndNumbers);

			mock.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(false);
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			packages = wrapper.Packages;
			AssertEquals("Count", 1, packages.Count);
			AssertEquals(shipmentPackLine.JL_MarksAndNumbers, packages[0].MarksAndNumbers);
		}

		public void TestGetCartageInfo()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(declaration, ((DocBaseWrapper)fullWrapper.CartageInfo.WrappedObject).WrappedObject);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			fullWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals(shipment, fullWrapper.CartageInfo.WrappedObject);

			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			fullWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals(shipment, fullWrapper.CartageInfo.WrappedObject);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			fullWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals(shipment, fullWrapper.CartageInfo.WrappedObject);

			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			fullWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals(shipment, fullWrapper.CartageInfo.WrappedObject);
		}

		public void TestOrderNumbersWithOwnerReferenceOnShipmentDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OwnerRef = "INV-0001";
			var fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Declaration reference when Shipment has no order refs", "INV-0001", fullWrapper.OrderNumbersWithOwnersReference);

			shipment.DocsAndCartage.JP_OrderItemsAsString = "INV-0001, INV-002";
			AssertEquals("Pre-condition test - JS_OrderReferences picked up from Docs & Cartage has no imbedded spaces", "INV-0001,INV-002", shipment.JS_OrderReferences);

			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Shipment order refs only as OwnerRef includes same value", "INV-0001, INV-002", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "Another Reference";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Shipment order refs plus OwnerRef as different value", "INV-0001, INV-002 / Another Reference", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "INV-0001, INV-002";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Should recognise same string with comma", "INV-0001, INV-002", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "INV-002, INV-0003";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Shipment order refs plus OwnerRef when whole OwnerRef not the same as orders", "INV-0001, INV-002 / INV-002, INV-0003", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "0001";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Shipment order refs plus OwnerRef when whole OwnerRef not the same as orders", "INV-0001, INV-002 / 0001", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Shipment order refs only when empty OwnerRef - i.e. no slash(/)", "INV-0001, INV-002", fullWrapper.OrderNumbersWithOwnersReference);
		}

		public void TestOrderNumbersWithOwnerReferenceOnStandAloneDeclaration()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return empty value from stand alone Declaration with no orders and owner reference", "", fullWrapper.OrderNumbersWithOwnersReference);

			var orderItem1 = declaration.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "INV-0001";
			var orderItem2 = declaration.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "INV-002";
			AssertEquals("Pre-condition: Orders as string", "INV-0001,INV-002", declaration.DocsAndCartage.JP_OrderItemsAsString);

			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return order ref values when Declaration has no owner reference", "INV-0001, INV-002", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "INV-0001";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should not include Declaration Owner Reference as it is one of the Order Numbers", "INV-0001, INV-002", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "INV-0001, INV-002";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should not include Declaration Owner Reference, embedded spaces are ignored, details still match orders", "INV-0001, INV-002", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "INV-0001, HVG/Urgent";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should now include Declaration Owner Reference, as details differ from Orders", "INV-0001, INV-002 / INV-0001, HVG/Urgent", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.DocsAndCartage.OrderItems.RemoveAndDeleteAll();
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Declaration Owner Reference when no Orders", "INV-0001, HVG/Urgent", fullWrapper.OrderNumbersWithOwnersReference);
		}

		public void TestOrderNumbersWithOwnersReferenceWithCaseSensitiveOrders()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return empty value from stand alone Declaration with no owner reference", "", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "INV-0001";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Declaration reference when stand-alone dec", "INV-0001", fullWrapper.OrderNumbersWithOwnersReference);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Declaration reference when Shipment has no order refs", "INV-0001", fullWrapper.OrderNumbersWithOwnersReference);

			shipment.DocsAndCartage.JP_OrderItemsAsString = "Inv-0001, inv-002";
			AssertEquals("Pre-condition test - JS_OrderReferences picked up from Docs & Cartage", "Inv-0001,inv-002", shipment.JS_OrderReferences);

			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Shipment order refs only as OwnerRef includes same value, case sensitivity ignored", "Inv-0001, inv-002", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "Another Reference";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Shipment order refs plus OwnerRef as different value", "Inv-0001, inv-002 / Another Reference", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "INV-0001, INV-002";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Should recognise same string with comma", "Inv-0001, inv-002", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "INV-002, INV-0003";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Shipment order refs plus OwnerRef when whole OwnerRef not the same as orders", "Inv-0001, inv-002 / INV-002, INV-0003", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "0001";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Shipment order refs plus OwnerRef when whole OwnerRef not the same as orders", "Inv-0001, inv-002 / 0001", fullWrapper.OrderNumbersWithOwnersReference);

			declaration.JE_OwnerRef = "";
			fullWrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Property should return Shipment order refs only when empty OwnerRef - i.e. no slash(/)", "Inv-0001, inv-002", fullWrapper.OrderNumbersWithOwnersReference);
		}

		public void TestShipmentContainerMode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_ContainerMode = Constants.ContainerModes.Containerised;
			declaration.JE_JS = shipment.PK;

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("If there are no containers, and there is a linked shipment, then use the shipments container mode", shipment.JS_PackingMode, wrapper.ShipmentContainerMode.Code);

			declaration.JE_JS = ZGuid.Empty;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("If there are no containers, and no linked shipment, ShipmentContainerMode is 'LSE'", Constants.ContainerModes.Loose, wrapper.ShipmentContainerMode.Code);

			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Constants.ContainerModes.FCL;
			BaseCusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_FCL_LCL_AIR = Constants.ContainerModes.FCL;
			BaseCusContainer container3 = declaration.CusContainers.AddNew();
			container3.CO_FCL_LCL_AIR = Constants.ContainerModes.FCL;
			BaseCusContainer container4 = declaration.CusContainers.AddNew();
			container4.CO_FCL_LCL_AIR = Constants.ContainerModes.FCL;

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("If there are containers, and they are all FCL, ShipmentContainerMode is the mode from containers", Constants.ContainerModes.FCL, wrapper.ShipmentContainerMode.Code);

			container1.CO_FCL_LCL_AIR = Constants.ContainerModes.LCL;
			container2.CO_FCL_LCL_AIR = Constants.ContainerModes.BreakBulk;
			container3.CO_FCL_LCL_AIR = Constants.ContainerModes.FCLMixedShipper;

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("If there are containers, one of which is BBK, ShipmentContainerMode is “BBK”.", Constants.ContainerModes.BreakBulk, wrapper.ShipmentContainerMode.Code);

			container1.CO_FCL_LCL_AIR = Constants.ContainerModes.FCLMixedShipper;
			container2.CO_FCL_LCL_AIR = Constants.ContainerModes.FCL;
			container3.CO_FCL_LCL_AIR = Constants.ContainerModes.FCLMixedShipper;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("If there are containers, one of which is FCL, ShipmentContainerMode is “FCL”.", Constants.ContainerModes.FCL, wrapper.ShipmentContainerMode.Code);

			container1.CO_FCL_LCL_AIR = Constants.ContainerModes.LCL;
			container2.CO_FCL_LCL_AIR = Constants.ContainerModes.LCL;
			container3.CO_FCL_LCL_AIR = Constants.ContainerModes.LCL;
			container4.CO_FCL_LCL_AIR = Constants.ContainerModes.LCL;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("LCL is the lowest form of life, ShipmentContainerMode is “LCL”.", Constants.ContainerModes.LCL, wrapper.ShipmentContainerMode.Code);
		}

		#region TestContainersWrapper

		public void TestContainersWrapperWhenDeclarationHasSameContainersAsShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "123";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "456";

			var packLine1 = GetPackLine(22.34m, Constants.Weight.Kilograms, 12.89m, Constants.Volume.CubicMetres, 138, Constants.PkgUnit.Pallet);
			packLine1.SetContainer(consol, container1);
			shipment.OuterPackLines.Add(packLine1);

			var packLine2 = GetPackLine(100m, Constants.Weight.Pounds, 20m, Constants.Volume.Litre, 2, Constants.PkgUnit.Bottle);
			shipment.OuterPackLines.Add(packLine2);
			packLine2.SetContainer(consol, container2);

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);

			declaration.CusContainers.AddNew().CO_ContainerNumber = container1.JC_ContainerNum;
			declaration.CusContainers.AddNew().CO_ContainerNumber = container2.JC_ContainerNum;

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Containers count", 2, wrapper.Containers.Count);
			AssertContainerWrapper(wrapper.Containers[0], packLine1, container1.JC_ContainerNum);
			AssertContainerWrapper(wrapper.Containers[1], packLine2, container2.JC_ContainerNum);
		}

		public void TestContainersWrapperWhenDeclarationContainersNotEqualToShipmentContainers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "123";

			var packLine1 = GetPackLine(22.34m, Constants.Weight.Kilograms, 12.89m, Constants.Volume.CubicMetres, 138, Constants.PkgUnit.Pallet);
			packLine1.SetContainer(consol, container1);
			shipment.OuterPackLines.Add(packLine1);

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			declaration.JE_OverrideFreightDefaults = true;

			declaration.CusContainers.AddNew().CO_ContainerNumber = container1.JC_ContainerNum;

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "789";
			cusContainer.CO_Weight = 12.33m;
			cusContainer.CO_WeightUQ = Constants.Weight.Grams;
			ContainerWrapperFromCustomsTest.AddCusPackage(declaration, cusContainer.CO_ContainerNumber, 30, Constants.PkgUnit.Drum, "");

			var expectedPackLine1 = GetPackLine(packLine1.JL_ActualWeight, packLine1.JL_ActualWeightUQ, 0m, string.Empty, 0, Constants.PkgUnit.Package);
			var expectedPackLine2 = GetPackLine(cusContainer.CO_Weight, cusContainer.CO_WeightUQ, 0m, string.Empty, 30, Constants.PkgUnit.Drum);

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Containers count", 2, wrapper.Containers.Count);
			AssertContainerWrapper(wrapper.Containers[0], expectedPackLine1, container1.JC_ContainerNum);
			AssertContainerWrapper(wrapper.Containers[1], expectedPackLine2, cusContainer.CO_ContainerNumber);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "456";

			var packLine2 = GetPackLine(100m, Constants.Weight.Pounds, 20m, Constants.Volume.Litre, 2, Constants.PkgUnit.Bottle);
			shipment.OuterPackLines.Add(packLine2);
			packLine2.SetContainer(consol, container2);

			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Containers count", 2, wrapper.Containers.Count);
			AssertContainerWrapper(wrapper.Containers[0], expectedPackLine1, container1.JC_ContainerNum);
			AssertContainerWrapper(wrapper.Containers[1], expectedPackLine2, cusContainer.CO_ContainerNumber);
		}

		public void TestContainersWrapperForNonShipmentDeclaration()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "123";
			cusContainer.CO_Weight = 12.33m;
			cusContainer.CO_WeightUQ = Constants.Weight.Grams;
			ContainerWrapperFromCustomsTest.AddCusPackage(declaration, cusContainer.CO_ContainerNumber, 30, Constants.PkgUnit.Drum, "HB:HB2");

			var expectedPackLine = GetPackLine(cusContainer.CO_Weight, cusContainer.CO_WeightUQ, 0m, string.Empty, 30, Constants.PkgUnit.Drum);

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("Containers count", 1, wrapper.Containers.Count);
			AssertContainerWrapper(wrapper.Containers[0], expectedPackLine, cusContainer.CO_ContainerNumber);
		}

		#endregion

		public void TestInvoicingJob()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();

			var header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = "11112222";
			header.JH_ParentID = declaration.PK;
			header.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			var debtor1 = Factory.New<OrgHeader>();

			var charge1Debtor1 = header.Charges.AddNew();
			charge1Debtor1.JR_OH_SellAccount = debtor1.PK;

			var charge2Debtor1 = header.Charges.AddNew();
			charge2Debtor1.JR_OH_SellAccount = debtor1.PK;

			var debtor2 = Factory.New<OrgHeader>();

			var charge1Debtor2 = header.Charges.AddNew();
			charge1Debtor2.JR_OH_SellAccount = debtor2.PK;

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { charge1Debtor1.PK, charge2Debtor1.PK, charge1Debtor2.PK },
				FreightWrapperFromDeclaration.New(declaration, Factory).InvoicingJob.Charges.Select(charge => ((DocJobInvoicingJobCharge)charge).ChargePK));

			var localClient = Factory.New<OrgHeader>();
			localClient.Addresses.AddNew(OrgAddressType.Office, true);
			header.LocalChargesPK = localClient.PK;

			var charge1LocalClient = header.Charges.AddNew();
			charge1LocalClient.JR_OH_SellAccount = localClient.PK;

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { charge1LocalClient.PK },
				FreightWrapperFromDeclaration.New(declaration, Factory).InvoicingJob.Charges.Select(charge => ((DocJobInvoicingJobCharge)charge).ChargePK));
		}

		public void TestGetCustomField()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			declaration.JE_JS = shipment.PK;

			string customFieldName = "Custom Property";
			shipment.SetUserDefinedValue(customFieldName, new ZString("XXX"));
			AssertEquals("Precondition:shipment.GetCustomField", "XXX", shipment.GetCustomField(customFieldName, null));

			declaration.SetUserDefinedValue(customFieldName, new ZString("YYY"));
			AssertEquals("Precondition:declaration.GetCustomField", "YYY", declaration.GetCustomField(customFieldName, null));

			var docBuilderDataSource = new DocBuilderDataSource();
			docBuilderDataSource.Brokerage = false;
			DocumentsDataRegistry.Instance.DocBuilderDataSource.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, docBuilderDataSource);
			var wrapper1 = FreightWrapper.New(shipment, Factory)[0];
			Assert(wrapper1 is FreightWrapperFromShipment);
			AssertEquals("FreightWrapperFromShipment.GetCustomField", "XXX", wrapper1.GetCustomField(customFieldName));
			AssertEquals("FreightWrapperFromShipment.GetCustomFieldCodeDescription", "XXX", wrapper1.GetCustomFieldCodeDescription(customFieldName));

			docBuilderDataSource.Brokerage = true;
			DocumentsDataRegistry.Instance.DocBuilderDataSource.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, docBuilderDataSource);
			var wrapper2 = FreightWrapper.New(shipment, Factory)[0];
			Assert(wrapper2 is FreightWrapperFromDeclaration);
			AssertEquals("FreightWrapperFromDeclaration.GetCustomField", "YYY", wrapper2.GetCustomField(customFieldName));
			AssertEquals("FreightWrapperFromDeclaration.GetCustomFieldCodeDescription", "YYY", wrapper2.GetCustomFieldCodeDescription(customFieldName));
		}

		public void TestBusinessObjectForPrintJobForInvoice()
		{
			var invoice = Factory.New<ARInvoice>();
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			declaration.JE_JS = shipment.PK;
			var header = Factory.NewJobForTesting<Job>();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_JobNum = "00001000";
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.AH_JH = header.PK;
			Factory.Save();

			var wrapper = FreightWrapper.New(invoice, Factory)[0];
			Assert(wrapper is FreightWrapperFromDeclaration);
			AssertEquals(invoice, wrapper.BusinessObjectForPrintJob);
		}

		public override void TestTrackingBusinessObjectPK()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);

			AssertEquals("TrackingBusinessObjectPK", declaration.PK, wrapper.TrackingBusinessObjectPK);
		}

		public void TestImportContainerPenalties()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();

			var cusContainer1 = declaration.CusContainers.AddNew();
			var cusContainer2 = declaration.CusContainers.AddNew();
			cusContainer1.ImportPenalties.AddNew();
			cusContainer1.ExportPenalties.AddNew();
			cusContainer2.ImportPenalties.AddNew();
			cusContainer2.ExportPenalties.AddNew();

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("2 penalties from CusContainers", 2, wrapper.ImportContainerPenalties.Count);

			shipment.PickupPenalties.AddNew();
			shipment.DeliveryPenalties.AddNew();
			declaration.JE_JS = shipment.PK;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("1 penalties from Shipment", 1, wrapper.ImportContainerPenalties.Count);
		}

		public void TestExportContainerPenalties()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();

			var cusContainer1 = declaration.CusContainers.AddNew();
			var cusContainer2 = declaration.CusContainers.AddNew();
			cusContainer1.ImportPenalties.AddNew();
			cusContainer1.ExportPenalties.AddNew();
			cusContainer2.ImportPenalties.AddNew();
			cusContainer2.ExportPenalties.AddNew();
			cusContainer2.ExportPenalties.AddNew();

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("3 penalties from CusContainers", 3, wrapper.ExportContainerPenalties.Count);

			shipment.PickupPenalties.AddNew();
			shipment.DeliveryPenalties.AddNew();
			declaration.JE_JS = shipment.PK;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("1 penalty from Shipment", 1, wrapper.ExportContainerPenalties.Count);
		}

		public void TestContainerPenalties()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();

			var cusContainer1 = declaration.CusContainers.AddNew();
			var cusContainer2 = declaration.CusContainers.AddNew();
			cusContainer1.ImportPenalties.AddNew();
			cusContainer1.ExportPenalties.AddNew();
			cusContainer2.ImportPenalties.AddNew();
			cusContainer2.ExportPenalties.AddNew();

			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("4 penalties from CusContainers", 4, wrapper.ContainerPenalties.Count);

			shipment.PickupPenalties.AddNew();
			shipment.DeliveryPenalties.AddNew();
			declaration.JE_JS = shipment.PK;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("2 penalties from Shipment", 2, wrapper.ContainerPenalties.Count);
		}

		public void TestCO2eEmissions()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("No emissions when declaration does not have a related shipment.", 0, wrapper.CO2eEmissions.Count);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(0, wrapper.CO2eEmissions.Count);

			shipment.Transports.AddNew();
			shipment.Transports.AddNew();
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(2, wrapper.CO2eEmissions.Count);
		}

		public override void TestFormattedTotalCO2e()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("No formatted co2e when declaration does not have a related shipment.", ZString.Empty, wrapper.FormattedTotalCO2e);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = consol.Shipments.AddNew();
			shipment.SetTotalCO2e(9.07244m);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			declaration.JE_JS = shipment.PK;
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("9.072", wrapper.FormattedTotalCO2e);

			shipment.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals(ZString.Empty, wrapper.FormattedTotalCO2e);
		}

		public override void TestCO2eCalculationDate()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			var wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals("No CO2e calculation date when declaration does not have a related shipment.", ZDateTime.Empty, wrapper.CO2eCalculationDate);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 100m;
			shipment.SetCO2ePerTonneInKg(200m);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			wrapper = FreightWrapperFromDeclaration.New(declaration, Factory);
			AssertEquals((shipment.GetOrCreateJobCO2e() as JobCO2e).JCO_SystemLastEditTimeUtc, wrapper.CO2eCalculationDate);
		}

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "HBLContainerMode", "CNT" },
					{ "HouseBillHeading", "House Bill Of Lading" },
					{ "JobNumberHeading", "Shipment" },
					{ "MasterBillHeading", "Ocean Bill Of Lading" },
					{ "NoOriginalBills", "3" },
					{ "NoCopyBills", "3" },
					{ "SecondaryHeading", "Consol" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
AssuredParty : ASSUREDPARTY DECLARATION\nANGUILLA
Buyer : BUYER DECLARATION\nANGUILLA
CaratagePickupMode : ABX
Carrier : SHIPPINGLINE\nAUSTRALIA
ChargeableWeight : 38.630 M3
ClaimsPayableBy : CLAIMSPAYABLEBY DECLARATION\nANGUILLA
Consignee : IMPORTER\nAUSTRALIA
Consignor : SUPPLIER\nAUSTRALIA
ConsolContainerMode : VD_
ConsolCreditor : DUMB ASS\nAUSTRALIA
ConsolTransportMode : SEA - Sea Freight
ConsolType : AGT - Agent
CTOArrival : WHY ME AND DOCUMENTS\nAUSTRALIA
DeliveryAddress : IMPORTER\nAUSTRALIA
DeliveryLocation : NZCHC - Christchurch
Destination : NZCHC - Christchurch
ExportAgent : FORWARDER\nAUSTRALIA
ExportBroker : OURBRANCH\nAUSTRALIA
ExportReceivalAddress : TEST 1 ADDRESS\nAUSTRALIA
ExportReceivingDepotAddress : TEST 1 ADDRESS\nAUSTRALIA
FreightRate : 123.45 AUD
GoodsAvailableAt : TEST 1 ADDRESS\nAUSTRALIA
ImportArrivalCTOAddress : WHY ME AND DOCUMENTS\nAUSTRALIA
IncoTerm : DAF
InsuredBy : INSUREDBY SHIPMENT\nANGUILLA
LocalForwarder : FORWARDER\nAUSTRALIA
NotifyParty : NOTIFYME\nAUSTRALIA
OrderTransportMode : 
Origin : USDNV - Dunnville
PickupAddress : SUPPLIER\nAUSTRALIA
PickupAgent : CARTAGE COMPANY\nAUSTRALIA
PickupCFSAddress : TEST 1 ADDRESS\nAUSTRALIA
PickupLocation : USDNV - Dunnville
ReleaseType : NXS
SendingForwarder : FORWARDER\nAUSTRALIA
ServiceLevel : STD - Standard
ShipmentContainerMode : LCL - Less Container Load
ShipmentOuterPacksQty : 34 PK
ShipmentStatus : ESA
ShipmentTransportMode : SEA - Sea
ShipmentType : EXP - Export
ShippedOnBoardType : SHP - Shipped
StorageTime : 1 Days
SurveyReportParty : SURVEYREPORTPARTY SHIPMENT\nANGUILLA
Volume : 32.450 M3
Weight : 55.400 KG";
			}
		}

		static void AssertContainerWrapper(ContainerWrapper containerWrapper, ForwardingPackLine packLine, ZString containerNum)
		{
			AssertEquals("ContainerNo", containerNum, containerWrapper.ContainerNo);
			AssertEquals("WeightGoods.Value", packLine.JL_ActualWeight, containerWrapper.WeightGoods.Value);
			AssertEquals("WeightGoods.Unit", packLine.JL_ActualWeightUQ, containerWrapper.WeightGoods.Unit.Code);
			AssertEquals("VolumeGoods.Value", packLine.JL_ActualVolume, containerWrapper.VolumeGoods.Value);
			AssertEquals("VolumeGoods.Unit", packLine.JL_ActualVolumeUQ, containerWrapper.VolumeGoods.Unit.Code);
			AssertEquals("PackCount.Value", (ZDecimal)packLine.JL_PackageCount, containerWrapper.PackCount.Value);
			AssertEquals("PackCount.Unit", packLine.JL_F3_NKPackType, containerWrapper.PackCount.Unit.Code);
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			((BaseJobDeclaration)WrappedBO).JE_OH_ShippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return FreightWrapperFromDeclaration.New((BaseJobDeclaration)WrappedBO, Factory);
		}

		ForwardingPackLine GetPackLine(decimal weight, string weightUQ, decimal volume, string volumeUQ, int packageCount, string packType)
		{
			var packLine = Factory.New<ForwardingPackLine>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualWeightUQ = weightUQ;
			packLine.JL_ActualVolume = volume;
			packLine.JL_ActualVolumeUQ = volumeUQ;
			packLine.JL_PackageCount = packageCount;
			packLine.JL_F3_NKPackType = packType;
			return packLine;
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
		}

		ZString countryToStartWith;
		protected override void SetUp()
		{
			base.SetUp();
			countryToStartWith = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("AI");
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(countryToStartWith);
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "WHY ME AND DOCUMENTS";
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress depotAddress = Factory.New<OrgAddress>();
			depotAddress.OA_Address1 = "TEST 1 ADDRESS";
			depotAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader cartageCo = Factory.New<OrgHeader>();
			cartageCo.OH_FullName = "CARTAGE COMPANY";
			cartageCo.MainAddress.OA_RN_NKCountryCode = "AU";

			JobSailing sailing = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageDestination voyageDestination = Factory.New<VoyageDestination>();
			VoyageOrigin voyageOrigin = Factory.New<VoyageOrigin>();
			voyageDestination.JB_Berth = "WHARF C3";
			voyageDestination.JB_JV = voyage.PK;
			sailing.JX_JB = voyageDestination.PK;
			sailing.JX_JA = voyageOrigin.PK;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_JX = sailing.PK;
			consol.JK_OA_CreditorAddress = GetOrgHeader("DUMB ASS").MainAddress.PK;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;
			consol.JK_ConsolMode = "VD_";

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = GetOrgHeader("SUPPLIER").PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = GetOrgHeader("IMPORTER").PK;
			shipment.JS_ActualVolume = 38.63;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_UnitFreightRate = 123.45m;
			shipment.JS_RX_NKFrtRateCurrency = "AUD";
			shipment.JS_ReleaseType = "NXS";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;

			declaration.JE_OH_ShippingLine = GetOrgHeader("SHIPPINGLINE").PK;
			declaration.Branch.GB_OH_OrgProxy = GetOrgHeader("OURBRANCH").PK;

			JobDocAddress declarationBuyer = declaration.BuyerDocAddress;
			declarationBuyer.E2_AddressOverride = true;
			declarationBuyer.E2_CompanyName = "BUYER DECLARATION";

			JobDocAddress declarationAssuredParty = declaration.AssuredPartyDocAddress;
			declarationAssuredParty.E2_AddressOverride = true;
			declarationAssuredParty.E2_CompanyName = "ASSUREDPARTY DECLARATION";

			JobDocAddress declarationClaimsPayableBy = declaration.ClaimsPayableByDocAddress;
			declarationClaimsPayableBy.E2_AddressOverride = true;
			declarationClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY DECLARATION";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY SHIPMENT";

			JobDocAddress shipmentInsuredBy = shipment.InsuredByDocAddress;
			shipmentInsuredBy.E2_AddressOverride = true;
			shipmentInsuredBy.E2_CompanyName = "INSUREDBY SHIPMENT";

			declaration.JE_ContainerMode = Constants.ContainerModes.Containerised;  //Lookups.CargoIdTypeList;
			declaration.JE_TransportMode = Constants.TransportModes.Sea; //Lookups.TransportTypeList;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export; //Lookups.MessageTypeList;
			declaration.JE_MessageSubType = "MST"; //Lookups.MessageSubTypeList;
			declaration.JE_ShipmentIncoTerm = Constants.IncoTerms.DeliveredAtFrontier;
			//declaration.JE_RS_NKServiceLevel = "SLV"; This will be ignored by system. If plugged into shipment, it always returns shipment's value and the field disappears from form
			declaration.JE_MessageStatus = "MSA";
			declaration.JE_EntryStatus = "ESA";
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "ABX";
			declaration.JE_OH_Forwarder = GetOrgHeader("FORWARDER").PK;

			declaration.JE_RL_NKOrigin = "USDNV";
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			declaration.JE_RL_NKFinalDestination = "NZCHC";

			declaration.JE_TotalNoOfPacks = 34;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			declaration.JE_TotalWeight = 55.4;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalVolume = 32.45;
			declaration.JE_TotalVolumeUnit = "M3";
			declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;

			declaration.AttachedOrders.AddNew();
			declaration.NotifyPartyDocumentaryAddress.E2_OA_Address = GetOrgHeader("NOTIFYME").MainAddress.PK;
			declaration.DeliveryOrPickupCartageCoPK = cartageCo.PK;
			declaration.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 1;

			return FreightWrapperFromDeclaration.New(declaration, Factory);
		}

		#endregion

	}
}
