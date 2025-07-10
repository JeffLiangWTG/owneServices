using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.JAS.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Utilities.Testing
{
	internal class JASUnitConverterTest : TestCaseWithFactory
	{
		public void TestGetJASPackageUnitFromShipmentOuterPacksType()
		{
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Basket, "BSK");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Box, "BOX");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Case, "CAS");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Carton, "CTN");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Container, "CBC");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Crate, "CRT");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Cylinder, "CYL");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Drum, "DRM");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Keg, "KEG");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Package, "PKG");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Pail, "PAL");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Pallet, "PLT");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Piece, "PCS");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Reel, "REL");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Roll, "ROL");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Sheet, "SHT");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Skid, "SKD");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Unit, "UNT");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.BaleCompressed, "BLE");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.BaleUncompressed, "BLE");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.Bag, "BAG");
			AssertGetJASPackageUnitFromShipmentOuterPacksType(Constants.PkgUnit.BulkBag, "BAG");
			AssertGetJASPackageUnitFromShipmentOuterPacksType("ASDLKJASKD", "PLT");
		}

		public void TestGetShipmentOuterPacksTypeFromJASPackageUnit()
		{
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("BSK", Constants.PkgUnit.Basket);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("BOX", Constants.PkgUnit.Box);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("CAS", Constants.PkgUnit.Case);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("CTN", Constants.PkgUnit.Carton);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("CBC", Constants.PkgUnit.Container);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("CRT", Constants.PkgUnit.Crate);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("CYL", Constants.PkgUnit.Cylinder);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("DRM", Constants.PkgUnit.Drum);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("KEG", Constants.PkgUnit.Keg);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("PKG", Constants.PkgUnit.Package);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("PAL", Constants.PkgUnit.Pail);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("PLT", Constants.PkgUnit.Pallet);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("PCS", Constants.PkgUnit.Piece);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("REL", Constants.PkgUnit.Reel);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("ROL", Constants.PkgUnit.Roll);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("SHT", Constants.PkgUnit.Sheet);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("SKD", Constants.PkgUnit.Skid);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("UNT", Constants.PkgUnit.Unit);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("BLE", Constants.PkgUnit.BaleUncompressed);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("BAG", Constants.PkgUnit.Bag);
			AssertGetShipmentOuterPacksTypeFromJASPackageUnit("ALSKJLKASJDOIUQWE", Constants.PkgUnit.Pallet);
		}

		public void TestConvertContainerAndDeliveryModeToJASTypeOfService()
		{
			ForwardingContainer container = Factory.New<ForwardingContainer>();
			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			AssertEquals("CS", UnitConverter.GetJASTypeOfServiceFromContainerAndDeliveryMode(container));
			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
			AssertEquals("CY", UnitConverter.GetJASTypeOfServiceFromContainerAndDeliveryMode(container));
			container.JC_DeliveryMode = "";
			container.JC_JK = Factory.New(typeof(JASForwardingConsol)).PK;
			container.Consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("BB", UnitConverter.GetJASTypeOfServiceFromContainerAndDeliveryMode(container));
			container.Consol.JK_ConsolMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("RR", UnitConverter.GetJASTypeOfServiceFromContainerAndDeliveryMode(container));
			container.Consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			AssertEquals("", UnitConverter.GetJASTypeOfServiceFromContainerAndDeliveryMode(container));
		}

		public void TestAssignJASTypeOfServiceToContainerAndDeliveryMode()
		{
			ForwardingContainer container = Factory.New<ForwardingContainer>();
			container.JC_DeliveryMode = "";
			UnitConverter.AssignJASTypeOfServiceToContainerAndDeliveryMode("CS", container);
			AssertEquals(Core.Constants.DeliveryModes.Codes.CFS_CFS, container.JC_DeliveryMode);
			UnitConverter.AssignJASTypeOfServiceToContainerAndDeliveryMode("CY", container);
			AssertEquals(Core.Constants.DeliveryModes.Codes.CY_CY, container.JC_DeliveryMode);
			UnitConverter.AssignJASTypeOfServiceToContainerAndDeliveryMode("BB", container);
			container.JC_JK = Factory.New(typeof(JASForwardingConsol)).PK;
			container.Consol.JK_ConsolMode = "";
			UnitConverter.AssignJASTypeOfServiceToContainerAndDeliveryMode("BB", container);
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, container.Consol.JK_ConsolMode);
			UnitConverter.AssignJASTypeOfServiceToContainerAndDeliveryMode("RR", container);
			AssertEquals(Core.Constants.ContainerModes.RollOnRollOff, container.Consol.JK_ConsolMode);
		}

		public void TestGetJASContainerType()
		{
			try
			{
				SetupDataForTestContainerMapping();
				AssertGetJASContainerType("20RE", "20RE", "Should be from our container type code");
				AssertGetJASContainerType("40RE", "FORTY", "Should be from the PatternMatchOverride");
				AssertGetJASContainerType("BLAH", "BLUH", "Should be from the PatternMatchOverride");
				AssertGetJASContainerType("40OT", "40OT", "Should be from our container type code");
				AssertGetJASContainerType("20FR", "TWENTY", "Should be from the PatternMatchOverride");
				AssertGetJASContainerType("40FR", "40FR", "Should be from our container type code");
				AssertGetJASContainerType("20PL", "20PL", "Should be from our container type code");
				AssertGetJASContainerType("40PL", "40PL", "Should be from our container type code");
				AssertGetJASContainerType("40HC", "40HC", "Should be from our container type code");
				AssertGetJASContainerType("40REHC", "40REHC", "Should be from our container type code");
				AssertGetJASContainerType("40NOR", "40NOR", "Should be from our container type code");
				AssertGetJASContainerType("20OT", "20OT", "Should be from our container type code");
				AssertGetJASContainerType("YES", "YES", "Should be from our container type code");
			}
			finally
			{
				JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
			}
		}

		public void TestGetRefContainerFromJASContainerType()
		{
			try
			{
				SetupDataForTestContainerMapping();
				AssertGetRefContainerFromJASContainerType("TWENTY", "20FR", "Should be from the PatternMatchOverride", true);
				AssertGetRefContainerFromJASContainerType("FORTY", "40RE", "Should be from the PatternMatchOverride", true);
				AssertGetRefContainerFromJASContainerType("BLUH", "BLAH", "Should be from the PatternMatchOverride", true);
				AssertGetRefContainerFromJASContainerType("20RE", "20RE", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("40RE", "40RE", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("20OT", "20OT", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("40OT", "40OT", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("20FR", "20FR", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("40FR", "40FR", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("20PL", "20PL", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("40PL", "40PL", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("40HC", "40HC", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("40REHC", "40REHC", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("40NOR", "40NOR", "Should match our container type code", true);
				AssertGetRefContainerFromJASContainerType("asdf", "", "Should not be found", false);
			}
			finally
			{
				JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
			}
		}

		void AssertGetJASPackageUnitFromShipmentOuterPacksType(ZString packType, ZString expectedJASPackageUnit)
		{
			AssertEquals(expectedJASPackageUnit, UnitConverter.GetJASPackageUnitFromShipmentOuterPacksType(packType));
		}

		void AssertGetShipmentOuterPacksTypeFromJASPackageUnit(ZString jASPackageUnit, ZString expectedPackType)
		{
			AssertEquals(expectedPackType, UnitConverter.GetShipmentOuterPacksTypeFromJASPackageUnit(jASPackageUnit));
		}

		void AssertGetJASContainerType(ZString containerType, ZString expectedJASContainerType, ZString failureMessage)
		{
			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
			if (refContainer == null)
			{
				refContainer = Factory.New<RefContainer>();
				refContainer.RC_Code = containerType;
			}

			AssertEquals(failureMessage, expectedJASContainerType, UnitConverter.GetJASContainerType(refContainer));
		}

		void AssertGetRefContainerFromJASContainerType(ZString jASContainerType, ZString expectedContainerType, ZString failureMessage, bool shouldBeFound)
		{
			RefContainer refContainer = UnitConverter.GetRefContainerFromJASContainerType(Factory, jASContainerType);
			if (shouldBeFound)
			{
				AssertNotNull(failureMessage, refContainer);
				AssertEquals(failureMessage, expectedContainerType, refContainer.RC_Code);
			}
			else
			{
				AssertNull(failureMessage, refContainer);
			}
		}

		void SetupDataForTestContainerMapping()
		{
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
			JASOrgHeader jASWWOrganisation = JASDataRegistry.Instance.GetJASWWOrganisation(Factory);
			AddContainerMapping(jASWWOrganisation, "20FR", "TWENTY");
			AddContainerMapping(jASWWOrganisation, "40RE", "FORTY");
			AddContainerMapping(jASWWOrganisation, "BLAH", "BLUH");
		}

		void AddContainerMapping(JASOrgHeader jASWWOrganisation, ZString containerType, ZString jASContainerType)
		{
			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
			if (refContainer == null)
			{
				refContainer = Factory.New<RefContainer>();
				refContainer.RC_Code = containerType;
			}

			OrgPatternMatchOverride @override = jASWWOrganisation.CreatePatternMatchOverrideForTest();
			@override.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			@override.OO_LocalGuid = refContainer.PK;
			@override.OO_ForeignCode = jASContainerType;
		}

		JASUnitConverter UnitConverter
		{
			get
			{
				if (fUnitConverter == null)
				{
					fUnitConverter = new JASUnitConverter();
				}

				return fUnitConverter;
			}
		}

		JASUnitConverter fUnitConverter;
	}
}
