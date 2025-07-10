using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(UNDGSubstanceWrapper))]
	internal class UNDGSubstanceWrapperTest : Base.Testing.GenericWrapperTest
	{
		#region Properties

		public void TestIMOClass()
		{
			UNDGSubstance dgSubstance = Factory.New<UNDGSubstance>();
			dgSubstance.DG_Code = "MRT";
			dgSubstance.DG_Class = "1.2A";

			UNDGDataItem dataItem = Factory.New<UNDGDataItem>();

			UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			AssertEquals("No IMO Class or DG Class", ZString.Empty, wrapper.IMOClass);

			dataItem.DI_DG = dgSubstance.PK;
			AssertEquals("Falls back to DG Class for empty IMO Class", "1.2A", wrapper.IMOClass);

			dataItem.DI_IMOClass = "1.3B";
			AssertEquals("IMO Class taken from DI_IMOClass", "1.3B", wrapper.IMOClass);
		}

		public void TestTechnicalName()
		{
			UNDGSubstance dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_Code = "MRT";

			UNDGDataItem parentDataItem = Factory.New<UNDGDataItem>();

			UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);
			AssertEquals(ZString.Empty, wrapper.TechnicalName);

			parentDataItem.DI_DG = dGSubstance.PK;
			AssertEquals(ZString.Empty, wrapper.TechnicalName);

			parentDataItem.DI_TechnicalName = "TN";
			AssertEquals("TN", wrapper.TechnicalName);

			wrapper.TechnicalName = "override";
			AssertEquals("override", wrapper.TechnicalName);
		}

		public void TestEMSCode()
		{
			UNDGSubstance dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_Code = "MRT";
			dGSubstance.DG_UNNO = "MRT";
			dGSubstance.DG_Standard = "ADN";
			dGSubstance.DG_EMS = "F-E,S-E";

			UNDGDataItem dGItem = Factory.New<UNDGDataItem>();
			dGItem.DI_DG = dGSubstance.PK;

			UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(dGItem, Factory);
			AssertEquals("F-E,S-E", wrapper.EMSCode);

			dGSubstance.DG_EMS = "F-A,S-I";
			dGItem = Factory.New<UNDGDataItem>();
			dGItem.DI_DG = dGSubstance.PK;
			wrapper = new UNDGSubstanceWrapper(dGItem, Factory);
			AssertEquals("F-A,S-I", wrapper.EMSCode);
		}

		public void TestMarinePollutantWarningWithParent()
		{
			UNDGSubstance dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_Code = "MRT";
			dGSubstance.DG_UNNO = "MRT";

			UNDGDataItem parentDataItem = Factory.New<UNDGDataItem>();

			UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);
			AssertEquals(ZString.Empty, wrapper.MarinePollutantWarning);

			parentDataItem.DI_DG = dGSubstance.PK;

			dGSubstance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code;
			parentDataItem.DI_MPMarinePollutant = "T";
			AssertEquals("MARINE POLLUTANT", wrapper.MarinePollutantWarning);

			parentDataItem.DI_MPMarinePollutant = "";
			AssertEquals(ZString.Empty, wrapper.MarinePollutantWarning);
		}

		public void TestMarinePollutantWarningWhenTransportedByBarge_SeaLeg() => TestMarinePollutantWarningWhenTransportedByBarge(Constants.TransportModes.Sea);
		public void TestMarinePollutantWarningWhenTransportedByBarge_InlandWaterwayLeg() => TestMarinePollutantWarningWhenTransportedByBarge(Constants.TransportModes.InlandWaterwayTransport);

		void TestMarinePollutantWarningWhenTransportedByBarge(string bargeTransportMode)
		{
			var dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_Code = "MRT";
			dGSubstance.DG_UNNO = "MRT";
			dGSubstance.DG_Standard = "ADN";

			var parentDataItem = Factory.New<UNDGDataItem>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";

			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(parentDataItem);

			var barge = Factory.LoadTop1<RefVessel>(new ZQuery());
			barge.RV_VesselType = "BA";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = bargeTransportMode;
			transport.JW_RL_NKLoadPort = "NLRTM";
			transport.JW_RL_NKDiscPort = "DEHAM";
			transport.JW_Vessel = barge.RV_Name;

			var packageWrapper = new PackageWrapperFromFreightPackage(packline, Factory);
			var wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);
			wrapper.ContainingPackage = packageWrapper;

			AssertEquals(ZString.Empty, wrapper.MarinePollutantWarning);

			parentDataItem.DI_DG = dGSubstance.PK;

			dGSubstance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code;
			parentDataItem.DI_MPMarinePollutant = "T";
			AssertEquals("MARINE POLLUTANT / ENVIRONMENTALLY HAZARDOUS", wrapper.MarinePollutantWarning);

			var seaVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.PK, SQLComparisonOperator.NotEqual, barge.PK));
			var seaLeg = shipment.Transports.AddNew();
			seaLeg.JW_TransportMode = "SEA";
			seaLeg.JW_RL_NKLoadPort = "DEHAM";
			seaLeg.JW_RL_NKDiscPort = "FRCAL";
			seaLeg.JW_Vessel = seaVessel.RV_Name;

			AssertEquals("ENVIRONMENTALLY HAZARDOUS is not appended for shipments with a mixture of barge and sea legs", "MARINE POLLUTANT", wrapper.MarinePollutantWarning);
		}

		public void TestSubLabel2()
		{
			UNDGDataItem parentDataItem = Factory.New<UNDGDataItem>();

			UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);
			AssertEquals("", wrapper.SubLabel2);

			UNDGSubstance dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_SubLabel2 = "L2";
			dGSubstance.DG_Code = "123";
			dGSubstance.DG_UNNO = "123";

			parentDataItem.DI_DG = dGSubstance.PK;

			wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);
			AssertEquals("L2", wrapper.SubLabel2);
		}

		public void TestHasPickedNameBasedOnLanguageConfiguration()
		{
			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_Code = "XXX";
			undgSubstance.DG_UNNO = "XXX";
			undgSubstance.DG_Variant = "";
			undgSubstance.DG_PSN = "english name";

			var parentDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			parentDataItem.DI_DG = undgSubstance.PK;

			Factory.Save();

			var wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);
			AssertEquals("english name", wrapper.LocalName);

			var undgAttributeNamePtBr = Factory.NewWithValidTestData<ViewUNDGAttribute>();
			undgAttributeNamePtBr.DA_Type = ViewUNDGAttributeLookups.TypeConstants.ProperShippingName;
			undgAttributeNamePtBr.DA_DG = undgSubstance.PK;
			undgAttributeNamePtBr.DA_Language = SharedConstants.Languages.PortugueseBrazil;
			undgAttributeNamePtBr.DA_Descriptor = "brazilian language name";
			undgSubstance.Names.Add(undgAttributeNamePtBr);

			var undgAttributeNameGerman = Factory.NewWithValidTestData<ViewUNDGAttribute>();
			undgAttributeNameGerman.DA_Type = ViewUNDGAttributeLookups.TypeConstants.ProperShippingName;
			undgAttributeNameGerman.DA_DG = undgSubstance.PK;
			undgAttributeNameGerman.DA_Language = SharedConstants.Languages.German;
			undgAttributeNameGerman.DA_Descriptor = "german language name";
			undgSubstance.Names.Add(undgAttributeNameGerman);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Language = SharedConstants.Languages.German;
			company.GC_OH_OrgProxy = orgProxy.PK;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "UKB";
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.OH_Language = SharedConstants.Languages.German;
			branch.GB_OH_OrgProxy = branchProxy.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("german language name", wrapper.LocalName);
			}

			orgProxy.OH_Language = SharedConstants.Languages.PortugueseBrazil;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("brazilian language name", wrapper.LocalName);
			}
		}

		public void TestNamesPropertyHasOnlyPSNAttributes()
		{
			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_Code = "ANY";
			undgSubstance.DG_PSN = "english name";

			var parentDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			parentDataItem.DI_DG = undgSubstance.PK;

			var undgAttributeNamePtBr = Factory.NewWithValidTestData<ViewUNDGAttribute>();
			undgAttributeNamePtBr.DA_Type = ViewUNDGAttributeLookups.TypeConstants.ProperShippingName;
			undgAttributeNamePtBr.DA_DG = undgSubstance.PK;
			undgAttributeNamePtBr.DA_Language = SharedConstants.Languages.PortugueseBrazil;
			undgAttributeNamePtBr.DA_Descriptor = "brazilian language name";
			undgSubstance.Names.Add(undgAttributeNamePtBr);

			var undgAttributeNameGerman = Factory.NewWithValidTestData<ViewUNDGAttribute>();
			undgAttributeNameGerman.DA_Type = ViewUNDGAttributeLookups.TypeConstants.QualifyingDescriptiveText;
			undgAttributeNameGerman.DA_DG = undgSubstance.PK;
			undgAttributeNameGerman.DA_Language = SharedConstants.Languages.German;
			undgAttributeNameGerman.DA_Descriptor = "should not be a name";
			undgSubstance.Names.Add(undgAttributeNameGerman);

			var wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);
			var names = wrapper.Names;

			Assert(!names.Any(o => o.DA_Type != ViewUNDGAttributeLookups.TypeConstants.ProperShippingName));
			Assert(names.Contains(undgAttributeNamePtBr));
		}

		#region TestSummary

		public void TestSummary_ActivateIsCombustibleForDGItemsEnabled()
		{
			TestSummary(true);
		}

		public void TestSummary_ActivateIsCombustibleForDGItemsDisabled()
		{
			TestSummary(false);
		}

		void TestSummary(bool activateIsCombustibleForDGItems)
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, activateIsCombustibleForDGItems))
			{
				UNDGSubstance dGSubstance = Factory.New<UNDGSubstance>();
				dGSubstance.DG_UNNO = "123";
				dGSubstance.DG_Variant = "";
				dGSubstance.DG_PSN = "SPL";
				dGSubstance.DG_PG = "";
				dGSubstance.DG_Class = "1.0";
				dGSubstance.DG_SubLabel1 = "5";
				dGSubstance.DG_SubLabel2 = "3";
				dGSubstance.DG_FlashPoint = "45.0";
				dGSubstance.DG_MP = "";
				dGSubstance.DG_Code = "123";

				UNDGDataItem parentDataItem = Factory.New<UNDGDataItem>();
				parentDataItem.DI_DG = dGSubstance.PK;

				UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);

				AssertEquals("UN123, SPL, class 1.0 (5, 3), (45.0C c.c.)", wrapper.Summary);

				dGSubstance.DG_Class = "";
				dGSubstance.DG_PG = "28";
				AssertEquals("UN123, SPL, class 1.0 (5, 3), PG 28, (45.0C c.c.)", wrapper.Summary);

				dGSubstance.DG_Class = "1.0";
				dGSubstance.DG_FlashPoint = "";
				parentDataItem.DI_IsCombustible = false;
				parentDataItem.DI_DGFlashPoint = 0m;
				AssertEquals("UN123, SPL, class 1.0 (5, 3), PG 28", wrapper.Summary);

				parentDataItem.DI_IsCombustible = true;
				parentDataItem.DI_DGFlashPoint = 45m;
				AssertEquals("UN123, SPL, class 1.0 (5, 3), PG 28, (45C c.c.)", wrapper.Summary);

				dGSubstance.DG_MP = "C";
				parentDataItem.DI_MPMarinePollutant = "T";
				AssertEquals("UN123, SPL, class 1.0 (5, 3), PG 28, (45C c.c.), MARINE POLLUTANT", wrapper.Summary);

				UNDGDataItem parentDataItem2 = Factory.New<UNDGDataItem>();
				parentDataItem2.DI_DG = dGSubstance.PK;
				parentDataItem2.DI_IsCombustible = true;
				parentDataItem2.DI_DGFlashPoint = 45m;
				wrapper = new UNDGSubstanceWrapper(parentDataItem2, Factory);
				AssertEquals("UN123, SPL, class 1.0 (5, 3), PG 28, (45C c.c.)", wrapper.Summary);

				parentDataItem2.DI_TechnicalName = "TN";
				AssertEquals("UN123, SPL (TN), class 1.0 (5, 3), PG 28, (45C c.c.)", wrapper.Summary);

				parentDataItem2.DI_MPMarinePollutant = "S";
				AssertEquals("UN123, SPL (TN), class 1.0 (5, 3), PG 28, (45C c.c.), MARINE POLLUTANT", wrapper.Summary);

				parentDataItem2.DI_IsLimitedQuantity = true;
				AssertEquals("UN123, SPL (TN), class 1.0 (5, 3), PG 28, (45C c.c.), MARINE POLLUTANT, LTD QTY", wrapper.Summary);

				var cfrDGSubstance = CreateCFRSubstance("3507", "", "6.1", "7");

				Factory.Save();

				parentDataItem2 = Factory.New<UNDGDataItem>();
				parentDataItem2.DI_DG = cfrDGSubstance.PK;
				parentDataItem2.DI_IsCombustible = true;
				parentDataItem2.DI_DGFlashPoint = 45m;
				parentDataItem2.DI_IsLimitedQuantity = true;

				wrapper = new UNDGSubstanceWrapper(parentDataItem2, Factory);

				AssertContains("Limited quantity radioactive material", wrapper.Summary);
			}
		}

		public void TestSummary_WithRIDSubstance()
		{
			var ridSubstance = Factory.New<UNDGSubstanceRID>();
			ridSubstance.RID_UNNO = "007";
			ridSubstance.RID_SpecialProvisions = "W2";
			ridSubstance.RID_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E1;
			ridSubstance.RID_Class = "1";
			Factory.Save();

			var dgItem = Factory.New<ForwardingUNDGDataItem>();
			dgItem.DI_DGVolume = 0.01m;
			dgItem.DI_UnitOfVolume = Constants.Volume.Litre;
			dgItem.DI_DGWeight = 1000m;
			dgItem.DI_UnitOfWeight = "KG";
			dgItem.DI_PackageCount = 1;
			dgItem.DI_F3_NKPackType = "PLT";
			dgItem.LinkDefault(ridSubstance);
			var wrapper = new UNDGSubstanceWrapper(dgItem, Factory);

			AssertEquals("UN007, class 1, Net Weight: 1000 KG, MILITARY CONSIGNMENT, DANGEROUS GOODS IN EXCEPTED QUANTITIES: 1 PLT", wrapper.Summary);
		}

		public void TestSummary_WithRIDClass1Weight()
		{
			var ridSubstance = Factory.New<UNDGSubstanceRID>();
			ridSubstance.RID_UNNO = "007";
			ridSubstance.RID_Class = "1";
			Factory.Save();

			var dgItem = Factory.New<ForwardingUNDGDataItem>();
			dgItem.LinkDefault(ridSubstance);
			var wrapper = new UNDGSubstanceWrapper(dgItem, Factory);
			AssertEquals("UN007, class 1", wrapper.Summary);

			dgItem.Subs.DG_Class = "2";
			dgItem.DI_DGWeight = 1000m;
			dgItem.DI_UnitOfWeight = "KG";
			wrapper = new UNDGSubstanceWrapper(dgItem, Factory);
			AssertEquals("UN007, class 2", wrapper.Summary);

			dgItem.Subs.DG_Class = "1";
			dgItem.DI_DGWeight = 1000m;
			dgItem.DI_UnitOfWeight = "KG";
			wrapper = new UNDGSubstanceWrapper(dgItem, Factory);
			AssertEquals("UN007, class 1, Net Weight: 1000 KG", wrapper.Summary);

			dgItem.DI_DGWeight = 1000m;
			dgItem.DI_UnitOfWeight = "LB";
			wrapper = new UNDGSubstanceWrapper(dgItem, Factory);
			AssertEquals("UN007, class 1, Net Weight: 453.592 KG", wrapper.Summary);
		}

		public void TestSummary_WithRIDClass1SpecialProvisionsW2()
		{
			var ridSubstance = Factory.New<UNDGSubstanceRID>();
			ridSubstance.RID_UNNO = "007";
			ridSubstance.RID_SpecialProvisions = "W2";
			ridSubstance.RID_Class = "2";
			Factory.Save();

			var dgItem = Factory.New<UNDGDataItem>();
			dgItem.LinkDefault(ridSubstance);
			var wrapper = new UNDGSubstanceWrapper(dgItem, Factory);
			AssertEquals("UN007, class 2", wrapper.Summary);

			dgItem.Subs.DG_Class = "1";
			AssertEquals("UN007, class 1, MILITARY CONSIGNMENT", wrapper.Summary);
		}

		public void TestSummary_WithRIDExceptedQuantities()
		{
			var ridSubstance = Factory.New<UNDGSubstanceRID>();
			ridSubstance.RID_UNNO = "007";
			ridSubstance.RID_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E1;
			ridSubstance.RID_LQ2MaxAmtUQ = Constants.Volume.Litre;
			ridSubstance.RID_Class = "1";
			Factory.Save();

			var dgItem = Factory.New<ForwardingUNDGDataItem>();
			dgItem.LinkDefault(ridSubstance);
			dgItem.DI_DGVolume = 1m;
			dgItem.DI_UnitOfVolume = Constants.Volume.Litre;
			dgItem.DI_PackageCount = 1;
			dgItem.DI_F3_NKPackType = "PLT";
			var wrapper = new UNDGSubstanceWrapper(dgItem, Factory);
			AssertEquals("UN007, class 1", wrapper.Summary);

			dgItem.DI_DGVolume = 0.01m;
			wrapper = new UNDGSubstanceWrapper(dgItem, Factory);
			AssertEquals("UN007, class 1, DANGEROUS GOODS IN EXCEPTED QUANTITIES: 1 PLT", wrapper.Summary);
		}

		public void TestSummary_WithDetailsTechnicalName()
		{
			UNDGSubstance dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_UNNO = "123";
			dGSubstance.DG_Variant = "";
			dGSubstance.DG_PSN = "SPL";
			dGSubstance.DG_PG = "";
			dGSubstance.DG_Class = "1.0";
			dGSubstance.DG_SubLabel1 = "5";
			dGSubstance.DG_SubLabel2 = "3";
			dGSubstance.DG_FlashPoint = "45.0";
			dGSubstance.DG_MP = "";

			PackLine line = Factory.New<PackLine>();
			UNDGDataItem dGDataItem = line.UNDGs.AddNew();
			dGDataItem.DI_DG = dGSubstance.PK;
			dGDataItem.DI_IsCombustible = true;
			dGDataItem.DI_DGFlashPoint = 33m;

			UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(dGDataItem, Factory);
			wrapper.TechnicalName = "Methyl";
			AssertEquals("UN123, SPL (Methyl), class 1.0 (5, 3), (33C c.c.)", wrapper.Summary);
		}

		public void TestSummary_WithPSAGroup_Origin()
		{
			var dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_UNNO = "123";
			dGSubstance.DG_Variant = "";
			dGSubstance.DG_PSN = "SPL";
			dGSubstance.DG_PG = "";
			dGSubstance.DG_Class = "1.0";
			dGSubstance.DG_SubLabel1 = "5";
			dGSubstance.DG_SubLabel2 = "3";
			dGSubstance.DG_FlashPoint = "45.0";
			dGSubstance.DG_MP = "";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "SGAYC";

			var line = shipment.OuterPackLines.AddNew();
			var dGDataItem = line.UNDGs.AddNew();
			dGDataItem.DI_DG = dGSubstance.PK;
			dGDataItem.DI_IsCombustible = true;
			dGDataItem.DI_DGFlashPoint = 33m;

			var reference = Factory.New<UNDGCountryReference>();
			reference.DCR_HasFlashPointLower = false;
			reference.DCR_HasFlashPointUpper = true;
			reference.DCR_FlashPointUpperCentigrade = 50m;
			reference.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference.DCR_Type = "PSA";
			reference.DCR_Code = "1S";
			dGSubstance.UNDGCountryReferences.Add(reference);

			var wrapper = new UNDGSubstanceWrapper(dGDataItem, Factory);
			AssertEquals("UN123, SPL, class 1.0 (5, 3), (33C c.c.), PSA Group: 1S", wrapper.Summary);
		}

		public void TestSummary_WithPSAGroup_Destination()
		{
			var dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_UNNO = "123";
			dGSubstance.DG_Variant = "";
			dGSubstance.DG_PSN = "SPL";
			dGSubstance.DG_PG = "";
			dGSubstance.DG_Class = "1.0";
			dGSubstance.DG_SubLabel1 = "5";
			dGSubstance.DG_SubLabel2 = "3";
			dGSubstance.DG_FlashPoint = "45.0";
			dGSubstance.DG_MP = "";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKDestination = "SGAYC";

			var line = shipment.OuterPackLines.AddNew();
			var dGDataItem = line.UNDGs.AddNew();
			dGDataItem.DI_DG = dGSubstance.PK;
			dGDataItem.DI_IsCombustible = true;
			dGDataItem.DI_DGFlashPoint = 33m;

			var reference = Factory.New<UNDGCountryReference>();
			reference.DCR_HasFlashPointLower = false;
			reference.DCR_HasFlashPointUpper = true;
			reference.DCR_FlashPointUpperCentigrade = 50m;
			reference.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference.DCR_Type = "PSA";
			reference.DCR_Code = "1S";
			dGSubstance.UNDGCountryReferences.Add(reference);

			var wrapper = new UNDGSubstanceWrapper(dGDataItem, Factory);
			AssertEquals("UN123, SPL, class 1.0 (5, 3), (33C c.c.), PSA Group: 1S", wrapper.Summary);
		}

		public void TestSummary_ShowsCorrectNumberPrefix()
		{
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = "8000";
			undgSubstance.DG_Mode = Constants.TransportModes.Air;

			var undgDataItem = Factory.New<UNDGDataItem>();
			undgDataItem.DI_DG = undgSubstance.PK;

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);

			AssertEquals("When substance number is 8000 and substance mode is AIR, prefix should be ID.", "ID8000", wrapper.Summary);

			undgSubstance.DG_UNNO = "6969";
			undgDataItem.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance);
			AssertEquals("When substance number is not 8000 prefix should be UN.", "UN6969", wrapper.Summary);
		}

		public void TestSummary_LimitedQuantity()
		{
			UNDGSubstance dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_Code = "MRT";

			UNDGDataItem parentDataItem = Factory.New<UNDGDataItem>();
			UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);
			AssertEquals(false, wrapper.IsLimitedQuantity);
			AssertNotContains(" LTD QTY", wrapper.Summary);

			parentDataItem.DI_IsLimitedQuantity = true;
			AssertEquals(true, wrapper.IsLimitedQuantity);
			AssertContains(" LTD QTY", wrapper.Summary);
		}

		public void TestSummary_LimitedQuantity_ContainsRadioactiveMateriaForLimitedQuantityDescription_WhenStandardIsCFRAndOneOfItsOtherClassesIsSeven()
		{
			var cfrSubstance = Factory.NewWithValidTestData<UNDGSubstanceCFR>();
			cfrSubstance.CFR_UNNO = "3507";
			cfrSubstance.CFR_PrimaryClass = "6.1";
			cfrSubstance.CFR_SecondaryClass = "7";
			cfrSubstance.CFR_TertiaryClass = "8";

			Factory.Save();

			var parentDataItem = Factory.New<UNDGDataItem>();
			parentDataItem.DI_DG = cfrSubstance.PK;
			parentDataItem.LinkDefault(cfrSubstance);
			parentDataItem.DI_IsLimitedQuantity = true;

			var wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);

			AssertEquals(true, wrapper.IsLimitedQuantity);
			AssertContains("Limited quantity radioactive material", wrapper.Summary);
		}

		public void TestSummary_RadioactiveLabelCategoryForCFRSubstances()
		{
			var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
			cfrSubstance.CFR_UNNO = "1234";
			cfrSubstance.CFR_PrimaryClass = RadioactiveConstants.RadioactiveClass;

			Factory.Save();

			var dataItem = Factory.New<ForwardingUNDGDataItem>();
			dataItem.DI_DG = cfrSubstance.PK;
			dataItem.LinkDefault(cfrSubstance);
			dataItem.DI_RadioactiveLabelCategory = RadioactiveLabelCategoryList.Codes.WhiteI;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);

			AssertContains("Radioactive Label Category should be in the summary", "RADIOACTIVE WHITE-I LABEL", wrapper.Summary);
		}

		#endregion

		#region TestSummaryWithPacks

		public void TestSummaryWithPacks()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "123";
			substance.DG_Variant = "";
			substance.DG_PSN = "SPL";
			substance.DG_PG = "";
			substance.DG_Class = "1.0";
			substance.DG_SubLabel1 = "5";
			substance.DG_SubLabel2 = "3";
			substance.DG_FlashPoint = "45.0";
			substance.DG_MP = "";
			substance.DG_Code = "526";

			var item = Factory.New<UNDGDataItem>();
			item.DI_DG = substance.PK;
			item.DI_PackageCount = 30;
			item.DI_F3_NKPackType = Constants.PkgUnit.Bottle;

			var wrapper = new UNDGSubstanceWrapper(item, Factory);
			AssertEquals("UN526, SPL, class 1.0 (5, 3), (45.0C c.c.), 30 BOT", wrapper.SummaryWithPacks);

			item.DI_PackageCount = 0;
			item.DI_F3_NKPackType = ZString.Empty;
			wrapper = new UNDGSubstanceWrapper(item, Factory);
			AssertEquals("UN526, SPL, class 1.0 (5, 3), (45.0C c.c.)", wrapper.SummaryWithPacks);

			item.DI_PackageCount = 3;
			item.DI_F3_NKPackType = ZString.Empty;
			wrapper = new UNDGSubstanceWrapper(item, Factory);
			AssertEquals("UN526, SPL, class 1.0 (5, 3), (45.0C c.c.)", wrapper.SummaryWithPacks);
		}

		#endregion

		#region TestSummaryWithContainingPackageID

		public void TestSummaryWithContainingPackageID()
		{
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = "123";
			undgSubstance.DG_Variant = "";
			undgSubstance.DG_PSN = "SPL";
			undgSubstance.DG_PG = "";
			undgSubstance.DG_Class = "1.0";
			undgSubstance.DG_SubLabel1 = "5";
			undgSubstance.DG_SubLabel2 = "3";
			undgSubstance.DG_FlashPoint = "45.0";
			undgSubstance.DG_MP = "";

			var emptyPackage = Factory.New<PkgPackage>();
			emptyPackage.KP_PackageQty = 0;
			emptyPackage.KP_F3_NKPackType = "";

			var package = Factory.New<PkgPackage>();
			package.KP_PackageQty = 1;
			package.KP_F3_NKPackType = Constants.PkgUnit.Box;

			UNDGDataItem undgDataItem = package.UNDGs.AddNew();
			undgDataItem.DI_DG = undgSubstance.PK;
			undgDataItem.DI_IsCombustible = true;
			undgDataItem.DI_DGFlashPoint = 33m;

			var undgWrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			AssertEquals("With no containing Package.", "UN123, SPL, class 1.0 (5, 3), (33C c.c.)", undgWrapper.SummaryWithContainingPackageID);

			var emptyPackageWrapper = new PackageWrapperFromPkgPackage(emptyPackage, Factory);
			undgWrapper.ContainingPackage = emptyPackageWrapper;
			AssertEquals("With empty containing Package.", "UN123, SPL, class 1.0 (5, 3), (33C c.c.)", undgWrapper.SummaryWithContainingPackageID);

			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			undgWrapper.ContainingPackage = packageWrapper;
			AssertEquals("With containing Package without PackageID.", "UN123, SPL, class 1.0 (5, 3), (33C c.c.), in 1x BOX", undgWrapper.SummaryWithContainingPackageID);

			package.KP_PackageID = "PACKAGE123";
			AssertEquals("With containing Package without PackageID.", "UN123, SPL, class 1.0 (5, 3), (33C c.c.), in 1x BOX PACKAGE123", undgWrapper.SummaryWithContainingPackageID);

			package.KP_PackageQty = 0;
			undgWrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			AssertEquals("With containing Package without PackageID.", "UN123, SPL, class 1.0 (5, 3), (33C c.c.)", undgWrapper.SummaryWithContainingPackageID);
		}

		public void TestSummaryWithContainingPackageID_WithParentContainer()
		{
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = "123";
			undgSubstance.DG_Variant = "";
			undgSubstance.DG_PSN = "SPL";
			undgSubstance.DG_PG = "";
			undgSubstance.DG_Class = "1.0";
			undgSubstance.DG_SubLabel1 = "5";
			undgSubstance.DG_SubLabel2 = "3";
			undgSubstance.DG_FlashPoint = "45.0";
			undgSubstance.DG_MP = "";

			var packageJob = Factory.New<PkgPackageJob>();
			var containerPackage = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			containerPackage.KP_PackageID = "TestContainer";
			containerPackage.KP_PackageQty = 1;

			var innerPackage = containerPackage.Packages.AddNew(Constants.PkgUnit.Box);
			innerPackage.KP_PackageQty = 5;

			var undgDataItem1 = containerPackage.UNDGs.AddNew();
			undgDataItem1.DI_DG = undgSubstance.PK;
			undgDataItem1.DI_IsCombustible = true;
			undgDataItem1.DI_DGFlashPoint = 10m;

			var undgDataItem2 = innerPackage.UNDGs.AddNew();
			undgDataItem2.DI_DG = undgSubstance.PK;
			undgDataItem2.DI_IsCombustible = true;
			undgDataItem2.DI_DGFlashPoint = 50m;

			var undgWrapper1 = new UNDGSubstanceWrapper(undgDataItem1, Factory);
			undgWrapper1.ContainingPackage = new PackageWrapperFromPkgPackage(containerPackage, Factory);
			AssertEquals("With containing package, containing package is a container.", "UN123, SPL, class 1.0 (5, 3), (10C c.c.), in 1x CNT TestContainer", undgWrapper1.SummaryWithContainingPackageID);

			var undgWrapper2 = new UNDGSubstanceWrapper(undgDataItem2, Factory);
			undgWrapper2.ContainingPackage = new PackageWrapperFromPkgPackage(innerPackage, Factory);
			AssertEquals("With containing package, containing package is inside a container.", "UN123, SPL, class 1.0 (5, 3), (50C c.c.), in 5x BOX, in container TestContainer", undgWrapper2.SummaryWithContainingPackageID);
		}

		#endregion

		#region TestSummaryADRTunnelCodes

		public void TestSummaryADRTunnelCodes()
		{
			var undgSubstanceADR = Factory.NewWithValidTestData<UNDGSubstanceADR>();
			undgSubstanceADR.ADR_UNNO = "1234";
			undgSubstanceADR.ADR_Variant = "a";
			undgSubstanceADR.ADR_TransportCategory = "2 (B)";
			undgSubstanceADR.ADR_PG = "PG";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var transportLeg1 = shipment.Transports.AddNew();
			var transportLeg2 = shipment.Transports.AddNew();

			transportLeg1.JW_RL_NKLoadPort = "FR24D";
			transportLeg1.JW_RL_NKDiscPort = "GENAB";
			transportLeg1.JW_TransportMode = Constants.TransportModes.Road;

			transportLeg2.JW_RL_NKLoadPort = "GENAB";
			transportLeg2.JW_RL_NKDiscPort = "NZAKL";
			transportLeg2.JW_TransportMode = Constants.TransportModes.Air;

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var pivot = undgDataItem.UNDGSubstancePivotCollection.AddNew();
			pivot.DP_Standard = "ADR";
			pivot.DP_ParentId = undgDataItem.PK;
			pivot.DP_ParentTableCode = "DI";
			pivot.DP_UNNO = "1234";
			pivot.DP_Variant = "a";
			pivot.DP_IsDefault = true;

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			AssertEquals("UN1234, PG PG, (B [MOST RESTRICTIVE])", wrapper.Summary);
		}

		public void TestSummaryADRTunnelCodes_MostRestrictiveCode()
		{
			var undgSubstanceADR1 = Factory.NewWithValidTestData<UNDGSubstanceADR>();
			undgSubstanceADR1.ADR_UNNO = "1234";
			undgSubstanceADR1.ADR_Variant = "a";
			undgSubstanceADR1.ADR_TransportCategory = "1 (B)";
			undgSubstanceADR1.ADR_PG = "PG";

			var undgSubstanceADR2 = Factory.NewWithValidTestData<UNDGSubstanceADR>();
			undgSubstanceADR2.ADR_UNNO = "1234";
			undgSubstanceADR2.ADR_Variant = "b";
			undgSubstanceADR2.ADR_TransportCategory = "2 (C)";
			undgSubstanceADR2.ADR_PG = "PG";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var transportLeg1 = shipment.Transports.AddNew();
			var transportLeg2 = shipment.Transports.AddNew();

			transportLeg1.JW_RL_NKLoadPort = "FR24D";
			transportLeg1.JW_RL_NKDiscPort = "GENAB";
			transportLeg1.JW_TransportMode = Constants.TransportModes.Road;

			transportLeg2.JW_RL_NKLoadPort = "GENAB";
			transportLeg2.JW_RL_NKDiscPort = "NZAKL";
			transportLeg2.JW_TransportMode = Constants.TransportModes.Air;

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem1 = packline.UNDGs.AddNew();
			var pivot1 = undgDataItem1.UNDGSubstancePivotCollection.AddNew();
			pivot1.DP_Standard = "ADR";
			pivot1.DP_ParentId = undgDataItem1.PK;
			pivot1.DP_ParentTableCode = "DI";
			pivot1.DP_UNNO = "1234";
			pivot1.DP_Variant = "a";
			pivot1.DP_IsDefault = true;

			var undgDataItem2 = packline.UNDGs.AddNew();
			var pivot2 = undgDataItem2.UNDGSubstancePivotCollection.AddNew();
			pivot2.DP_Standard = "ADR";
			pivot2.DP_ParentId = undgDataItem2.PK;
			pivot2.DP_ParentTableCode = "DI";
			pivot2.DP_UNNO = "1234";
			pivot2.DP_Variant = "b";
			pivot2.DP_IsDefault = true;

			var wrapper1 = new UNDGSubstanceWrapper(undgDataItem1, Factory);
			AssertEquals("B should be shown", "UN1234, PG PG, (B [MOST RESTRICTIVE])", wrapper1.Summary);

			var wrapper2 = new UNDGSubstanceWrapper(undgDataItem2, Factory);
			AssertEquals("C shouldn't be shown", "UN1234, PG PG", wrapper2.Summary);
		}

		public void TestSummaryADRTunnelCodes_ApplicableCountriesOnly_LoadAndDiscInvalid()
			=> SummaryADRTunnelCodes_ApplicableCountriesOnlyHelper("AUSYD", "AUBNE", false);
		public void TestSummaryADRTunnelCodes_ApplicableCountriesOnly_DiscInvalid()
			=> SummaryADRTunnelCodes_ApplicableCountriesOnlyHelper("AL123", "AUSYD", false);
		public void TestSummaryADRTunnelCodes_ApplicableCountriesOnly_LoadInvalid()
			=> SummaryADRTunnelCodes_ApplicableCountriesOnlyHelper("AUSYD", "AD123", false);
		public void TestSummaryADRTunnelCodes_ApplicableCountriesOnly_Valid()
			=> SummaryADRTunnelCodes_ApplicableCountriesOnlyHelper("AL123", "AD123", true);
		public void TestSummaryADRTunnelCodes_ApplicableCountriesOnly_Valid_Armenia_Nigeria()
			=> SummaryADRTunnelCodes_ApplicableCountriesOnlyHelper("AMEVN", "NGABV", true);
		public void TestSummaryADRTunnelCodes_ApplicableCountriesOnly_Valid_Nigeria_Uzbekistan()
			=> SummaryADRTunnelCodes_ApplicableCountriesOnlyHelper("NGABV", "UZTAS", true);

		void SummaryADRTunnelCodes_ApplicableCountriesOnlyHelper(string loadPort, string discPort, bool containsCode)
		{
			var undgSubstanceADR = Factory.NewWithValidTestData<UNDGSubstanceADR>();
			undgSubstanceADR.ADR_UNNO = "1234";
			undgSubstanceADR.ADR_Variant = "a";
			undgSubstanceADR.ADR_TransportCategory = "1 (B)";
			undgSubstanceADR.ADR_PG = "PG";
			Factory.Save();
			var shipment1 = Factory.New<ForwardingShipment>();
			var transportLeg1 = shipment1.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = loadPort;
			transportLeg1.JW_RL_NKDiscPort = discPort;
			transportLeg1.JW_TransportMode = Constants.TransportModes.Road;
			var packline = shipment1.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var pivot = undgDataItem.UNDGSubstancePivotCollection.AddNew();
			pivot.DP_Standard = "ADR";
			pivot.DP_ParentId = undgDataItem.PK;
			pivot.DP_ParentTableCode = "DI";
			pivot.DP_UNNO = "1234";
			pivot.DP_Variant = "a";
			pivot.DP_IsDefault = true;
			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			if (containsCode)
			{
				AssertEquals("UN1234, PG PG, (B [MOST RESTRICTIVE])", wrapper.Summary);
			}
			else
			{
				AssertEquals("UN1234, PG PG", wrapper.Summary);
			}
		}

		public void TestSummaryADRTunnelCodes_InvalidTransportCategory()
		{
			var undgSubstanceADR = Factory.NewWithValidTestData<UNDGSubstanceADR>();
			undgSubstanceADR.ADR_UNNO = "1234";
			undgSubstanceADR.ADR_Variant = "a";
			undgSubstanceADR.ADR_TransportCategory = "123412 asd e";
			undgSubstanceADR.ADR_PG = "PG";

			Factory.Save();
			var shipment1 = Factory.New<ForwardingShipment>();
			var transportLeg1 = shipment1.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AL123";
			transportLeg1.JW_RL_NKDiscPort = "AD123";
			transportLeg1.JW_TransportMode = Constants.TransportModes.Road;
			var packline = shipment1.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var pivot = undgDataItem.UNDGSubstancePivotCollection.AddNew();
			pivot.DP_Standard = "ADR";
			pivot.DP_ParentId = undgDataItem.PK;
			pivot.DP_ParentTableCode = "DI";
			pivot.DP_UNNO = "1234";
			pivot.DP_Variant = "a";
			pivot.DP_IsDefault = true;

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			AssertEquals("UN1234, PG PG", wrapper.Summary);
		}

		public void TestSummaryADRTunnelCodes_InvalidTransportCategoryCode()
		{
			var undgSubstanceADR = Factory.NewWithValidTestData<UNDGSubstanceADR>();
			undgSubstanceADR.ADR_UNNO = "1234";
			undgSubstanceADR.ADR_Variant = "a";
			undgSubstanceADR.ADR_TransportCategory = "1 (F)";
			undgSubstanceADR.ADR_PG = "PG";

			Factory.Save();
			var shipment1 = Factory.New<ForwardingShipment>();
			var transportLeg1 = shipment1.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AL123";
			transportLeg1.JW_RL_NKDiscPort = "AD123";
			transportLeg1.JW_TransportMode = Constants.TransportModes.Road;
			var packline = shipment1.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			var pivot = undgDataItem.UNDGSubstancePivotCollection.AddNew();
			pivot.DP_Standard = "ADR";
			pivot.DP_ParentId = undgDataItem.PK;
			pivot.DP_ParentTableCode = "DI";
			pivot.DP_UNNO = "1234";
			pivot.DP_Variant = "a";
			pivot.DP_IsDefault = true;

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			AssertEquals("UN1234, PG PG", wrapper.Summary);
		}

		public void TestSummeryADRTunnelCodes_ShouldNotThrowException_WhenUNDGSubstanceIsNull()
		{
			var undgSubstanceADR = Factory.NewWithValidTestData<UNDGSubstanceADR>();
			undgSubstanceADR.ADR_UNNO = "1234";
			undgSubstanceADR.ADR_Variant = "a";
			undgSubstanceADR.ADR_TransportCategory = "1 (B)";
			undgSubstanceADR.ADR_PG = "PG";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var transportLeg = shipment.Transports.AddNew();

			transportLeg.JW_RL_NKLoadPort = "AL123";
			transportLeg.JW_RL_NKDiscPort = "AD123";
			transportLeg.JW_TransportMode = Constants.TransportModes.Road;

			var packline = shipment.OuterPackLines.AddNew();
			var firstUndgDataItem = packline.UNDGs.AddNew();
			firstUndgDataItem.DI_IMOClass = "3";

			var secondUndgDataItem = packline.UNDGs.AddNew();
			var pivot = secondUndgDataItem.UNDGSubstancePivotCollection.AddNew();
			pivot.DP_Standard = "ADR";
			pivot.DP_ParentId = secondUndgDataItem.PK;
			pivot.DP_ParentTableCode = "DI";
			pivot.DP_UNNO = "1234";
			pivot.DP_Variant = "a";
			pivot.DP_IsDefault = true;

			Factory.Save();

			var wrapper = new UNDGSubstanceWrapper(secondUndgDataItem, Factory);
			ZString result = null;
			AssertNoExceptionThrown(() => result = wrapper.Summary);
			AssertEquals("UN1234, PG PG, (B [MOST RESTRICTIVE])", result);
		}

		#endregion

		#region StorageCategorySummary

		public void Test_StorageCategorySummary()
		{
			var dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_UNNO = "123";

			var shipment = Factory.New<ForwardingShipment>();

			var line = shipment.OuterPackLines.AddNew();
			var dGDataItem = line.UNDGs.AddNew();
			dGDataItem.DI_DG = dGSubstance.PK;

			var reference = Factory.New<UNDGCountryReference>();
			dGSubstance.UNDGCountryReferences.Add(reference);
			var pivot = (UNDGCountryReferencePivot)dGSubstance.UNDGCountryReferences.GetRelationshipBusinessObject(reference);
			pivot.DCP_StorageInstruction = "TBC";

			var wrapper = new UNDGSubstanceWrapper(dGDataItem, Factory);
			AssertEquals("TBC", wrapper.StorageCategorySummary);
		}

		#endregion

		#region TankStorageRetentionTrayRequired

		public void Test_TankStorageRetentionTray()
		{
			var dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_UNNO = "123";

			var shipment = Factory.New<ForwardingShipment>();

			var line = shipment.OuterPackLines.AddNew();
			var dGDataItem = line.UNDGs.AddNew();
			dGDataItem.DI_DG = dGSubstance.PK;

			var reference = Factory.New<UNDGCountryReference>();
			dGSubstance.UNDGCountryReferences.Add(reference);
			var pivot = (UNDGCountryReferencePivot)dGSubstance.UNDGCountryReferences.GetRelationshipBusinessObject(reference);

			var wrapper = new UNDGSubstanceWrapper(dGDataItem, Factory);

			pivot.DCP_TankStorageInstructionRetentionTray = true;
			AssertEquals(true, wrapper.TankStorageRetentionTrayRequired);

			pivot.DCP_TankStorageInstructionRetentionTray = false;
			AssertEquals(false, wrapper.TankStorageRetentionTrayRequired);
		}

		#endregion

		#endregion

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (UNDGSubstanceWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.UNNumber", ZString.Empty, wrapperEmpty.UNNumber);
			AssertEquals("wrapperEmpty.UNNumberWithVariant", ZString.Empty, wrapperEmpty.UNNumberWithVariant);
			AssertEquals("wrapperEmpty.ProperShippingName", ZString.Empty, wrapperEmpty.ProperShippingName);
			AssertEquals("wrapperEmpty.IMOClass", ZString.Empty, wrapperEmpty.IMOClass);
			AssertEquals("wrapperEmpty.SubLabel1", ZString.Empty, wrapperEmpty.SubLabel1);
			AssertEquals("wrapperEmpty.SubLabel2", ZString.Empty, wrapperEmpty.SubLabel2);
			AssertEquals("wrapperEmpty.PackingGroup", ZString.Empty, wrapperEmpty.PackingGroup);
			AssertEquals("wrapperEmpty.PackingInstructions", ZString.Empty, wrapperEmpty.PackingInstructions);
			AssertEquals("wrapperEmpty.FlashPoint", ZString.Empty, wrapperEmpty.FlashPoint);
			AssertEquals("wrapperEmpty.MarinePollutantWarning", ZString.Empty, wrapperEmpty.MarinePollutantWarning);
			AssertEquals("wrapperEmpty.IsLimitedQuantity", ZBool.False, wrapperEmpty.IsLimitedQuantity);
			AssertEquals("wrapperEmpty.Packages.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.Packages.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.TankStorageRetentionTrayRequired", ZBool.False, wrapperEmpty.TankStorageRetentionTrayRequired);
			AssertEquals("wrapperEmpty.StorageCategorySummary", ZString.Empty, wrapperEmpty.StorageCategorySummary);
		}

		#endregion

		#region TestWrapperMappingFull

		public void TestWrapperMappingFull()
		{
			var dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_UNNO = "123";
			dGSubstance.DG_Variant = "A";
			dGSubstance.DG_PSN = "SHP NAME";
			dGSubstance.DG_Class = "1.0";
			dGSubstance.DG_SubLabel1 = "LB1";
			dGSubstance.DG_SubLabel2 = "LB2";
			dGSubstance.DG_PG = "PG";
			dGSubstance.DG_PackIns = "PI";
			dGSubstance.DG_FlashPoint = "2.0";
			dGSubstance.DG_MP = "X";

			var dGItem = Factory.New<UNDGDataItem>();
			dGItem.DI_DG = dGSubstance.PK;
			dGItem.DI_DGWeight = 100m;
			dGItem.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			dGItem.DI_DGVolume = 200m;
			dGItem.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			dGItem.DI_IsLimitedQuantity = true;
			dGItem.DI_PackageCount = 33;
			dGItem.DI_F3_NKPackType = Core.Constants.PkgUnit.Bottle;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Contact Name";
			dGItem.DI_OC_DGContact = contact.PK;

			var packLine = Factory.New<PackLine>();
			packLine.JL_PackageCount = 13;
			packLine.JL_F3_NKPackType = "PLT";

			var wrapperFull = new UNDGSubstanceWrapper(dGItem, Factory);
			wrapperFull.ContainingPackage = new PackageWrapperFromFreightPackage(packLine, Factory);
			AssertEquals("wrapperFull.ToString()", "123A", wrapperFull.ToString());
			AssertEquals("wrapperFull.UNNumber", "123", wrapperFull.UNNumber);
			AssertEquals("wrapperFull.UNNumberWithVariant", "123A", wrapperFull.UNNumberWithVariant);
			AssertEquals("wrapperFull.ProperShippingName", "SHP NAME", wrapperFull.ProperShippingName);
			AssertEquals("wrapperFull.IMOClass", "1.0", wrapperFull.IMOClass);
			AssertEquals("wrapperFull.SubLabel1", "LB1", wrapperFull.SubLabel1);
			AssertEquals("wrapperFull.SubLabel2", "LB2", wrapperFull.SubLabel2);
			AssertEquals("wrapperFull.PackingGroup", "PG", wrapperFull.PackingGroup);
			AssertEquals("wrapperFull.PackingInstructions", "PI", wrapperFull.PackingInstructions);
			AssertEquals("wrapperFull.FlashPoint", "2.0", wrapperFull.FlashPoint);
			AssertEquals("wrapperFull.MarinePollutantWarning", "MARINE POLLUTANT", wrapperFull.MarinePollutantWarning);
			AssertEquals("wrapperFull.Weight", "100.000 KG", wrapperFull.Weight.ToString());
			AssertEquals("wrapperFull.Volume", "200.000 D3", wrapperFull.Volume.ToString());
			AssertEquals("wrapperFull.ContainingPackage", "13 PLT", wrapperFull.ContainingPackage.ToString());
			AssertEquals("wrapperFull.IsLimitedQuantity", true, wrapperFull.IsLimitedQuantity);
			AssertEquals("wrapperFull.DGContact", "Contact Name", wrapperFull.DGContact.FullName);
			AssertEquals("wrapperFull.Packages.ValueAndUnitCodeBlankIfZero", "33 BOT", wrapperFull.Packages.ValueAndUnitCodeBlankIfZero);
		}

		#endregion

		#region ADN Standard

		public void TestSummaryContainsLQ_ADN()
		{
			var adnSubstance = Factory.New<UNDGSubstanceADN>();
			adnSubstance.ADN_UNNO = "1234";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "FR123";
			transport.JW_RL_NKDiscPort = "FR432";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_VesselType = Constants.VesselType.Barge;
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_TransportMode = Constants.TransportModes.Sea;

			var packline = shipment.OuterPackLines.AddNew();

			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_DG = adnSubstance.PK;
			dataItem.LinkDefault(adnSubstance);
			dataItem.DI_IsLimitedQuantity = true;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			AssertContains(" LTD QTY", wrapper.Summary);
		}

		public void TestSummaryContainsExceptedQuantity_ADN()
		{
			var adnSubstance = Factory.New<UNDGSubstanceADN>();
			adnSubstance.ADN_UNNO = "007";
			adnSubstance.ADN_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E1;
			adnSubstance.ADN_LQ2MaxAmtUQ = Constants.Volume.Litre;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "FR123";
			transport.JW_RL_NKDiscPort = "FR432";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_VesselType = Constants.VesselType.Barge;
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_TransportMode = Constants.TransportModes.Sea;

			var packline = shipment.OuterPackLines.AddNew();

			var dgItem = packline.UNDGs.AddNew();
			dgItem.LinkDefault(adnSubstance);

			dgItem.DI_DGVolume = 1m;
			dgItem.DI_UnitOfVolume = Constants.Volume.Litre;
			dgItem.DI_PackageCount = 1;
			dgItem.DI_F3_NKPackType = "PLT";
			dgItem.DI_DGVolume = 0.01m;

			var wrapper = new UNDGSubstanceWrapper(dgItem, Factory);

			AssertEquals("UN007, DANGEROUS GOODS IN EXCEPTED QUANTITIES: 1 PLT", wrapper.Summary);
		}

		#endregion

		#region CFR Standard

		#region CFRHighwayRouteControlledQuantityComponent

		public void TestSummaryContainsHRCQ()
		{
			var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
			cfrSubstance.CFR_UNNO = "1234";
			cfrSubstance.CFR_PrimaryClass = RadioactiveConstants.RadioactiveClass;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();

			var packline = shipment.OuterPackLines.AddNew();

			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_DG = cfrSubstance.PK;
			dataItem.LinkDefault(cfrSubstance);
			dataItem.DI_IsHighwayRouteControlledQuantity = false;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			AssertNotContains("HRCQ", wrapper.Summary);

			dataItem.DI_IsHighwayRouteControlledQuantity = true;

			wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			AssertContains("HRCQ", wrapper.Summary);
		}

		#endregion

		#region CFRLimitedQuantityComponent

		public void TestCFRSummary_ID8000_AlwaysHasLimitedQuantityLabel()
		{
			var expectedLabel = " Limited Quantity";
			var packline = GetPackLine();
			var substance_ID8000 = CreateCFRSubstance("8000");
			var substance_UN1234 = CreateCFRSubstance("1234");

			var dataItem = AddCFRSubstanceToPackline(packline, substance_ID8000);
			dataItem.DI_IsLimitedQuantity = false;

			var wrapper = GetUNDGSubstanceWrapper(packline, dataItem);
			AssertContains("ID8000 should always show Limited Quantity, even if unticked", expectedLabel, wrapper.Summary);

			dataItem.DI_IsLimitedQuantity = true;

			wrapper = GetUNDGSubstanceWrapper(packline, dataItem);
			AssertContains("ID8000 should always show Limited Quantity, even if unticked", expectedLabel, wrapper.Summary);

			var someOtherDataItem = AddCFRSubstanceToPackline(packline, substance_UN1234);
			someOtherDataItem.DI_IsLimitedQuantity = false;

			wrapper = GetUNDGSubstanceWrapper(packline, someOtherDataItem);
			AssertNotContains("Shouldn't contain the label if Limited Quantity is false and not ID8000", expectedLabel, wrapper.Summary);
		}

		#endregion

		#region CFRResidueLastContainedComponent

		public void TestCFRSummary_ResidueLastContained()
		{
			var expectedLabel = " RESIDUE: Last Contained * * *";
			var packline = GetPackLine();
			var substance_UN1234 = CreateCFRSubstance("1234");
			var dataItem = AddCFRSubstanceToPackline(packline, substance_UN1234);
			var wrapper = GetUNDGSubstanceWrapper(packline, dataItem);

			AssertNotContains("Pre-Condition", expectedLabel, wrapper.Summary);

			dataItem.DI_IsResidueLastContained = true;
			wrapper = GetUNDGSubstanceWrapper(packline, dataItem);

			AssertContains("Should contain the residue label because it's ticked", expectedLabel, wrapper.Summary);

			dataItem.DI_IsResidueLastContained = false;
			wrapper = GetUNDGSubstanceWrapper(packline, dataItem);

			AssertNotContains(expectedLabel, wrapper.Summary);
		}

		#endregion

		#region CFRMaterialFormDescriptionComponent 

		public void TestSummaryContainsDescription()
		{
			var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
			cfrSubstance.CFR_UNNO = "1234";
			cfrSubstance.CFR_PSN = "Random PSN";
			cfrSubstance.CFR_PrimaryClass = "7";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_DG = cfrSubstance.PK;
			dataItem.LinkDefault(cfrSubstance);
			dataItem.DI_MaterialFormDescription = "abcd";

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			AssertContains("abcd", wrapper.Summary);

			dataItem.DI_MaterialFormDescription = ZString.Empty;

			wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			AssertNotContains("abcd", wrapper.Summary);
		}

		#endregion

		#region CFRFisileExcepted

		public void TestSummaryContainsFissileExcepted()
		{
			var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
			cfrSubstance.CFR_UNNO = "1234";
			cfrSubstance.CFR_PrimaryClass = RadioactiveConstants.RadioactiveClass;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_DG = cfrSubstance.PK;
			dataItem.LinkDefault(cfrSubstance);
			dataItem.DI_IsFissileExcepted = false;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			AssertNotContains("Fissile Excepted", wrapper.Summary);

			dataItem.DI_IsFissileExcepted = true;
			wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			AssertContains("Fissile Excepted", wrapper.Summary);
		}

		#endregion

		#endregion

		#region TestSummary_StandardIsAdr

		public void TestSummary_StandardIsAdr()
		{
			var dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_UNNO = "1230";
			dGSubstance.DG_MP = "";
			dGSubstance.DG_Code = "XXX";

			var parentDataItem = Factory.New<UNDGDataItem>();
			parentDataItem.DI_DG = dGSubstance.PK;
			parentDataItem.DI_IsLimitedQuantity = true;

			var wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);
			AssertContains(" LTD QTY", wrapper.Summary);
			AssertNotContains(" LQ", wrapper.Summary);
		}

		#endregion

		#region TestLtdQty

		public void TestLtdQty()
		{
			var dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_Code = "MRT";

			var parentDataItem = Factory.New<UNDGDataItem>();
			parentDataItem.DI_DG = dGSubstance.PK;
			parentDataItem.DI_IsLimitedQuantity = true;

			var wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);
			AssertContains(" LTD QTY", wrapper.Summary);
			AssertNotContains(" LQ", wrapper.LtdQty);

			var cfrDGSubstance = CreateCFRSubstance("3507", "", "6.1", "7");

			Factory.Save();

			parentDataItem = Factory.New<UNDGDataItem>();
			parentDataItem.DI_DG = cfrDGSubstance.PK;
			parentDataItem.DI_IsLimitedQuantity = true;

			wrapper = new UNDGSubstanceWrapper(parentDataItem, Factory);

			AssertEquals(true, wrapper.IsLimitedQuantity);
			AssertContains("Limited quantity radioactive material", wrapper.Summary);
		}

		#endregion

		#region TestWrapperForNOS

		public void TestWrapperForNOS()
		{
			var shipment = Factory.New<Integration.Forwarding.IForwardingShipment>() as CommonShipment;
			var packLine = shipment.OuterPackLines.AddNew();
			var dataItem = packLine.UNDGs.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "2395";
			substance.DG_Code = "2395";
			substance.DG_PSN = "Isobutyryl chloride";
			dataItem.DI_DG = substance.PK;

			var wrapperFull = new UNDGSubstanceWrapper(dataItem, Factory);
			wrapperFull.ContainingPackage = new PackageWrapperFromFreightPackage(packLine, Factory);

			AssertEquals("UN2395, Isobutyryl chloride", wrapperFull.Summary);

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "0190";
			substance.DG_Code = "0190";
			substance.DG_PSN = "Samples, explosive";
			var attribute = substance.QualifyingDescriptiveTexts.AddNew();
			attribute.DA_Descriptor = "other than initiating explosives";
			attribute.DA_Language = "EN";
			dataItem.DI_DG = substance.PK;
			wrapperFull = new UNDGSubstanceWrapper(dataItem, Factory);
			wrapperFull.ContainingPackage = new PackageWrapperFromFreightPackage(packLine, Factory);
			AssertEquals("UN0190, Samples, explosive", wrapperFull.Summary);

			dataItem.DI_IsNotOtherwiseSpecified = true;
			AssertEquals("UN0190, Samples, explosive, n.o.s., other than initiating explosives", wrapperFull.Summary);

			substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_UNNO = "3535";
			substance.DG_Code = "3535a";
			substance.DG_PSN = "Toxic solid, flammable, inorganic";
			attribute = substance.QualifyingDescriptiveTexts.AddNew();
			attribute.DA_Descriptor = "A5";
			attribute.DA_Language = "EN";
			dataItem.DI_DG = substance.PK;
			wrapperFull = new UNDGSubstanceWrapper(dataItem, Factory);
			wrapperFull.ContainingPackage = new PackageWrapperFromFreightPackage(packLine, Factory);
			AssertEquals("UN3535, Toxic solid, flammable, inorganic", wrapperFull.Summary);

			dataItem.DI_IsNotOtherwiseSpecified = true;
			AssertEquals("UN3535, Toxic solid, flammable, inorganic, n.o.s.", wrapperFull.Summary);
		}

		#endregion

		#region Implementations

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new UNDGSubstanceWrapper(null, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
UNDGSubstance                     (Default Field: UNNumberWithVariant)
======================================================================
Name                                    Type
----------------------------------------------------------------------
DGContact                               Contact
ContainingPackage                       Package
Packages                                PackQTY
Volume                                  Volume
Weight                                  Weight
EMSCode                                 String
FlashPoint                              String
IMOClass                                String
IsLimitedQuantity                       Bool
LocalName                               String
LtdQty                                  String
MarinePollutantWarning                  String
PackingGroup                            String
PackingInstructions                     String
ProperShippingName                      String
PSAGroup                                String
PSAGroupWithLabel                       String
StorageCategorySummary                  String
SubLabel1                               String
SubLabel2                               String
Summary                                 String
SummaryWithContainingPackageID          String
SummaryWithEMSCode                      String
SummaryWithPacks                        String
TankStorageRetentionTrayRequired        Bool
TechnicalName                           String
UNNumber                                String
UNNumberWithVariant                     String
Variant                                 String
Variation                               String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"ContainingPackage : 
DGContact : 
Packages : 
Registry : (No Default Field Value Available on Registry)
Volume : 
Weight :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			UNDGSubstance dGSubstance = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			UNDGDataItem dGItem = Factory.New<UNDGDataItem>();
			dGItem.DI_DG = dGSubstance.PK;
			return new UNDGSubstanceWrapper(dGItem, Factory);
		}

		UNDGSubstanceCFR CreateCFRSubstance(string unno, string variant = "", string primaryClass = "", string secondaryClass = "")
		{
			var substanceQuery = new ZQuery();
			substanceQuery.AddToFilter(UNDGSubstanceSchema.DG_UNNO, unno);
			substanceQuery.AddToFilter(UNDGSubstanceSchema.DG_Variant, variant);
			substanceQuery.AddToFilter(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR);

			UNDGSubstanceCFR substance;

			if (Factory.LoadTop1<UNDGSubstance>(substanceQuery)?.StandardSubstance is UNDGSubstanceCFR loadedSubstance)
			{
				substance = loadedSubstance;
			}
			else
			{
				var cfrSubstance = Factory.New<UNDGSubstanceCFR>();
				cfrSubstance.CFR_UNNO = unno;
				cfrSubstance.CFR_Variant = variant;

				substance = cfrSubstance;
			}

			substance.CFR_PrimaryClass = primaryClass;
			substance.CFR_SecondaryClass = secondaryClass;
			Factory.Save();

			return substance;
		}

		UNDGDataItem AddCFRSubstanceToPackline(ForwardingPackLine packline, UNDGSubstanceCFR substance)
		{
			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_DG = substance.PK;
			dataItem.LinkDefault(substance);

			Factory.Save();

			return dataItem;
		}

		ForwardingPackLine GetPackLine(string transportMode = Constants.TransportModes.Air)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;

			var packline = shipment.OuterPackLines.AddNew();

			return packline;
		}

		UNDGSubstanceWrapper GetUNDGSubstanceWrapper(ForwardingPackLine packline, UNDGDataItem item)
		{
			var packageWrapper = new PackageWrapperFromFreightPackage(packline, Factory);
			var wrapper = new UNDGSubstanceWrapper(item, Factory);
			wrapper.ContainingPackage = packageWrapper;

			return wrapper;
		}

		#endregion
	}
}
