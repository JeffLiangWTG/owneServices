using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing.ContainerWrappers
{
	sealed class DocContainerTest : TestCaseWithFactory
	{
		#region Collections

		public void TestShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			var freightContainer = consol.Containers.AddNew();

			ContainerWrapper = DocContainer.New(freightContainer, Factory);
			AssertEquals("AllocatedShipments contains 0 shipments", 0, ContainerWrapper.Shipments.Count);

			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, freightContainer);

			var packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, freightContainer);

			var shipment2 = consol.Shipments.AddNew();
			var packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.SetContainer(consol, freightContainer);

			ContainerWrapper = DocContainer.New(freightContainer, Factory);
			AssertEquals("AllocatedShipments contains 2 shipments", 2, ContainerWrapper.Shipments.Count);
			Assert("AllocatedShipments contains shipment1", ContainerWrapper.Shipments.ContainsWrappedObject(shipment1.PK));
			Assert("AllocatedShipments contains shipment2", ContainerWrapper.Shipments.ContainsWrappedObject(shipment2.PK));
		}

		public void TestAllocatedShipmentPackLine()
		{
			var consol = Factory.New<ForwardingConsol>();
			var freightContainer = consol.Containers.AddNew();

			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, freightContainer);

			ContainerWrapper = DocContainer.New(freightContainer, shipment1, Factory);
			AssertEquals("AllocatedShipmentPackLine", 1, ContainerWrapper.AllocatedShipmentPackLine.Count);

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "A", "IMO").First();

			var packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, freightContainer);
			packLine2.UNDGs.AddNew().DI_DG = subs.PK;
			AssertEquals("AllocatedShipmentPackLine", 2, ContainerWrapper.AllocatedShipmentPackLine.Count);

			AssertEquals("Dangerous goods first", packLine2, ContainerWrapper.AllocatedShipmentPackLine[0].WrappedObject);
			AssertEquals(packLine1, ContainerWrapper.AllocatedShipmentPackLine[1].WrappedObject);

			var shipment2 = consol.Shipments.AddNew();
			var packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.SetContainer(consol, freightContainer);

			// First shipment
			AssertEquals("AllocatedShipmentPackLine", 2, ContainerWrapper.AllocatedShipmentPackLine.Count);

			// Second shipment
			ContainerWrapper = DocContainer.New(freightContainer, shipment2, Factory);
			AssertEquals("AllocatedShipmentPackLine", 1, ContainerWrapper.AllocatedShipmentPackLine.Count);
		}

		public void TestPackLines()
		{
			var consol = Factory.New<ForwardingConsol>();
			var freightContainer = consol.Containers.AddNew();

			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, freightContainer);

			ContainerWrapper = DocContainer.New(freightContainer, shipment1, Factory);
			AssertEquals("AllocatedShipmentPackLine", 1, ContainerWrapper.PackLines.Count);

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "A", "IMO").First();

			var packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, freightContainer);
			packLine2.UNDGs.AddNew().DI_DG = subs.PK;
			AssertEquals("AllocatedShipmentPackLine", 2, ContainerWrapper.PackLines.Count);

			var shipment2 = consol.Shipments.AddNew();
			var packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.SetContainer(consol, freightContainer);

			AssertEquals("AllocatedShipmentPackLine", 3, ContainerWrapper.PackLines.Count);

			AssertEquals("Dangerous goods first", packLine2, ContainerWrapper.PackLines[0].WrappedObject);
			AssertEquals(packLine1, ContainerWrapper.PackLines[1].WrappedObject);
			AssertEquals(packLine3, ContainerWrapper.PackLines[2].WrappedObject);
		}

		public void TestAddressesWithWareHousing()
		{
			AddContainerToConsolAndShipment();
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("AddressesWithWareHousing should be", 0, ContainerWrapper.AddressesWithWareHousing.Count);

			var consignor = Factory.New<OrgHeader>();
			Shipment.ConsignorPK = consignor.PK;

			var j1PickUpAddress = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress j1J2ExporterAddress = consignor.Addresses.AddNew();
			var j2DeliverToAddress = Factory.NewWithValidTestData<OrgAddress>();

			FreightContainer.JC_OA_DepartureContainerYardAddress = j1PickUpAddress.PK;
			Shipment.ConsignorPickupAddress.E2_OA_Address = j1J2ExporterAddress.PK;
			Consol.JK_OA_DepartureCTOAddress = j2DeliverToAddress.PK;

			AssertEquals("AddressesWithWareHousing should be", 0, ContainerWrapper.AddressesWithWareHousing.Count);

			j1PickUpAddress.OA_ForkLift = ZBool.True;
			j1J2ExporterAddress.OA_DockLeveler = ZBool.True;
			j2DeliverToAddress.OA_DockLeveler = ZBool.True;
			AssertEquals("AddressesWithWareHousing should have 3", 3, ContainerWrapper.AddressesWithWareHousing.Count);

			System.Collections.Hashtable hashtable = new System.Collections.Hashtable();

			hashtable.Add(j1PickUpAddress.PK, j1PickUpAddress);
			hashtable.Add(j1J2ExporterAddress.PK, j1J2ExporterAddress);
			hashtable.Add(j2DeliverToAddress.PK, j2DeliverToAddress);
			foreach (DocDocAddress dAddress in ContainerWrapper.AddressesWithWareHousing)
			{
				Assert("is in col", hashtable.ContainsKey(((JobDocAddress)dAddress.WrappedObject).Address.PK));
			}

			Consol.JK_OA_DepartureCTOAddress = j1J2ExporterAddress.PK;
			AssertEquals("AddressesWithWareHousing has 2 same, should have 2, not 3", 2, ContainerWrapper.AddressesWithWareHousing.Count);
		}

		#endregion

		#region IMO and UNDG
		public void TestIMOClassAndUNDG_Num()
		{
			var consol = Factory.New<ForwardingConsol>();
			var freightContainer = consol.Containers.AddNew();

			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, freightContainer);
			ContainerWrapper = DocContainer.New(freightContainer, shipment1, Factory);
			AssertEquals("UNDG empty", "", ContainerWrapper.UNDG_Num);
			AssertEquals("IMO class empty", "", ContainerWrapper.IMOClass);

			var uNDG1 = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.DG_UNNO, "0005"));
			ZQuery uNDGWithVariantFilter = new ZQuery(UNDGSubstanceSchema.DG_UNNO, "0004");
			uNDGWithVariantFilter.AddToFilter(UNDGSubstanceSchema.DG_Variant, "a");
			var uNDG2 = Factory.LoadTop1<UNDGSubstance>(uNDGWithVariantFilter);

			packLine1.UNDGs.AddNew().DI_DG = uNDG1.PK;
			AssertEquals("UNDG not empty", "0005", ContainerWrapper.UNDG_Num);
			AssertEquals("IMO Class not empty", "1.1F", ContainerWrapper.IMOClass);

			var packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, freightContainer);
			packLine2.UNDGs.AddNew().DI_DG = uNDG1.PK;
			ContainerWrapper = DocContainer.New(freightContainer, shipment1, Factory);
			AssertEquals("UNDG not empty", "0005", ContainerWrapper.UNDG_Num);
			AssertEquals("IMO class not empty", "1.1F", ContainerWrapper.IMOClass);

			packLine2.UNDGs.AddNew().DI_DG = uNDG2.PK;
			AssertEquals("IMO class not empty", "1.1F, 1.1D", ContainerWrapper.IMOClass);
			AssertEquals("UNDG not empty", "0005, 0004a", ContainerWrapper.UNDG_Num);
		}

		#endregion

		#region Wrapper Fields

		public void TestShipment()
		{
			AssertNull("Shipment", ContainerWrapper.Shipment);

			var shipment = Factory.New<ForwardingShipment>();
			ContainerWrapper = DocContainer.New(FreightContainer, shipment, Factory);
			AssertNotNull("Shipment", ContainerWrapper.Shipment);
			AssertEquals("Shipment is of type DocForwardingShipment", typeof(DocForwardingShipment), ContainerWrapper.Shipment.GetType());
		}

		public void TestShipmentDirection()
		{
			AssertNull("Shipment", ContainerWrapper.Shipment);

			var shipment = Factory.New<ForwardingShipment>();
			ContainerWrapper = DocContainer.New(FreightContainer, shipment, Factory);
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Shipment document direction is same as container wrapper", nameof(DocumentDirection.ARV), ContainerWrapper.Shipment.DocumentDirection);
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Shipment document direction is same as container wrapper", nameof(DocumentDirection.DEP), ContainerWrapper.Shipment.DocumentDirection);
		}

		public void TestDepartureTransport()
		{
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertNull("DepartureTransport should be null.", ContainerWrapper.DepartureTransport);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			CommonContainer freightContainer = consol.Containers.AddNew();
			PackLine packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, freightContainer);
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			PackLine packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, freightContainer);
			OrgHeader pickupTransportCo = Factory.NewWithValidTestData<OrgHeader>();
			pickupTransportCo.OH_FullName = "Pickup Cartage TransportCo";
			shipment1.DocsAndCartage.PickupCartageCoPK = pickupTransportCo.PK;

			ContainerWrapper = DocContainer.New(freightContainer, Factory);
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertNull("DepartureTransport should be null.", ContainerWrapper.DepartureTransport);

			shipment2.DocsAndCartage.PickupCartageCoPK = pickupTransportCo.PK;
			ContainerWrapper = DocContainer.New(freightContainer, Factory);
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("DepartureTransport should be pickup cartage because departure leg transport and consol departure CFS transport are both empty.", pickupTransportCo.OH_FullName, ContainerWrapper.DepartureTransport.Name);

			OrgHeader departureCFSTransportCo = Factory.NewWithValidTestData<OrgHeader>();
			departureCFSTransportCo.OH_FullName = "Departure CFS TransportCo";
			consol.JK_OA_DeparturePackCFSTransportAddress = departureCFSTransportCo.MainAddress.PK;
			ContainerWrapper = DocContainer.New(freightContainer, Factory);
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("DepartureTransport should be consol departure CFS transport because departure leg transport is empty.", departureCFSTransportCo.OH_FullName, ContainerWrapper.DepartureTransport.Name);

			OrgHeader departLegTransportCo = Factory.NewWithValidTestData<OrgHeader>();
			departLegTransportCo.OH_FullName = "Departure Leg TransportCo";
			freightContainer.OriginCFSDeparture.TransportCoPK = departLegTransportCo.PK;
			ContainerWrapper = DocContainer.New(freightContainer, Factory);
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("DepartureTransport should be departure leg transport.", departLegTransportCo.OH_FullName, ContainerWrapper.DepartureTransport.Name);

			AssertEquals("DepartureTransport is of type DocOrganisation.", typeof(DocOrganisation), ContainerWrapper.DepartureTransport.GetType());
		}

		#endregion

		#region Total Volume and Unit

		public void TestTotalVolumeAndUnit()
		{
			AddContainerToConsolAndShipment();

			var pack1 = Shipment.OuterPackLines[0];

			var pack2 = Shipment.OuterPackLines.AddNew();
			pack2.SetContainer(FreightContainer.PK);

			pack1.JL_ActualVolume = 2;
			pack1.JL_ActualVolumeUQ = "M3";

			pack2.JL_ActualVolume = 3;
			pack2.JL_ActualVolumeUQ = "M3";

			AssertEquals(5M, ContainerWrapper.TotalVolume);
			AssertEquals("M3", ContainerWrapper.TotalVolumeUnit);

			pack2.JL_ActualVolumeUQ = "L";

			AssertEquals(2.003M, ContainerWrapper.TotalVolume);
			AssertEquals("M3", ContainerWrapper.TotalVolumeUnit);

			pack1.JL_ActualVolumeUQ = "CF";
			pack2.JL_ActualVolumeUQ = "CF";

			AssertEquals(5M, ContainerWrapper.TotalVolume);
			AssertEquals("CF", ContainerWrapper.TotalVolumeUnit);
		}

		#endregion

		#region ZGuid Fields

		public void TestDeparturePackAddressOrg()
		{
			AssertEquals("DeparturePackAddressOrg", FreightContainer.JC_Calc_DeparturePackAddressOrg, ContainerWrapper.DeparturePackAddressOrg);
		}

		public void TestDepartureCTOAddressOrg()
		{
			AssertEquals("DepartureCTOAddressOrg", FreightContainer.JC_Calc_DepartureCTOAddressOrg, ContainerWrapper.DepartureCTOAddressOrg);
		}

		public void TestDepartureContainerParkAddressOrg()
		{
			AssertEquals("DepartureContainerParkAddressOrg", FreightContainer.JC_Calc_DepartureContainerYardAddressOrg, ContainerWrapper.DepartureContainerParkAddressOrg);
		}

		public void TestArrivalUnpackAddressOrg()
		{
			AssertEquals("ArrivalUnpackAddressOrg", FreightContainer.JC_Calc_ArrivalUnpackAddressOrg, ContainerWrapper.ArrivalUnpackAddressOrg);
		}

		public void TestArrivalCTOAddressOrg()
		{
			AssertEquals("ArrivalCTOAddressOrg", FreightContainer.JC_Calc_ArrivalCTOAddressOrg, ContainerWrapper.ArrivalCTOAddressOrg);
		}

		public void TestArrivalContainerParkAddressOrg()
		{
			AssertEquals("ArrivalContainerParkAddressOrg", FreightContainer.JC_Calc_ArrivalContainerYardAddressOrg, ContainerWrapper.ArrivalContainerParkAddressOrg);
		}

		#endregion

		#region ZString Fields

		public void TestDeparturePackAddressCode()
		{
			AssertEquals("DeparturePackAddressCode", FreightContainer.JC_Calc_DeparturePackAddressCode, ContainerWrapper.DeparturePackAddressCode);
		}

		public void TestDepartureCTOAddressCode()
		{
			AssertEquals("DepartureCTOAddressCode", FreightContainer.JC_Calc_DepartureCTOAddressCode, ContainerWrapper.DepartureCTOAddressCode);
		}

		public void TestDepartureContainerParkAddressCode()
		{
			AssertEquals("DepartureContainerParkAddressCode", FreightContainer.JC_Calc_DepartureContainerYardAddressCode, ContainerWrapper.DepartureContainerParkAddressCode);
		}

		public void TestArrivalUnpackAddressCode()
		{
			AssertEquals("ArrivalUnpackAddressCode", FreightContainer.JC_Calc_ArrivalUnpackAddressCode, ContainerWrapper.ArrivalUnpackAddressCode);
		}

		public void TestArrivalCTOAddressCode()
		{
			AssertEquals("ArrivalCTOAddressCode", FreightContainer.JC_Calc_ArrivalCTOAddressCode, ContainerWrapper.ArrivalCTOAddressCode);
		}

		public void TestArrivalContainerParkAddressCode()
		{
			AssertEquals("ArrivalContainerParkAddressCode", FreightContainer.JC_Calc_ArrivalContainerYardAddressCode, ContainerWrapper.ArrivalContainerParkAddressCode);
		}

		public void TestContainerTempSign()
		{
			FreightContainer.JC_SetPointTemp = 0m;
			AssertEquals("ContainerTempSign", "+", ContainerWrapper.ContainerTempSign);

			FreightContainer.JC_SetPointTemp = 1m;
			AssertEquals("ContainerTempSign", "+", ContainerWrapper.ContainerTempSign);

			FreightContainer.JC_SetPointTemp = -1m;
			AssertEquals("ContainerTempSign", "-", ContainerWrapper.ContainerTempSign);
		}

		public void TestContainerTemp()
		{
			FreightContainer.JC_SetPointTemp = 0m;
			AssertEquals("ContainerTemp", "0", ContainerWrapper.ContainerTemp);

			FreightContainer.JC_SetPointTemp = -1m;
			AssertEquals("ContainerTemp", "1", ContainerWrapper.ContainerTemp);

			FreightContainer.JC_SetPointTemp = -10.5m;
			AssertEquals("ContainerTemp", "10.5", ContainerWrapper.ContainerTemp);

			FreightContainer.JC_SetPointTemp = 1m;
			AssertEquals("ContainerTemp", "1", ContainerWrapper.ContainerTemp);

			FreightContainer.JC_SetPointTemp = 10.5m;
			AssertEquals("ContainerTemp", "10.5", ContainerWrapper.ContainerTemp);
		}

		public void TestAuthorisationReleaseClause()
		{
			AssertEquals("AuthorisationReleaseClause", Env.Registry.AuthorisationReleaseClause, ContainerWrapper.AuthorisationReleaseClause);
		}

		public void TestHaulier()
		{
			AssertEquals("Haulier should be empty", ZString.Empty, ContainerWrapper.Haulier);
			AddContainerToConsolAndShipment();

			OrgHeader importCartage = Factory.NewWithValidTestData<OrgHeader>();
			importCartage.OH_FullName = "Import Cartage co.";
			Shipment.DocsAndCartage.DeliveryCartageCoPK = importCartage.PK;
			AssertEquals("Should return import cartage co", importCartage.OH_FullName, ContainerWrapper.Haulier);

			OrgCusCode code1 = importCartage.CustomsCodes.AddNew();
			code1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			code1.OK_CustomsRegNo = "REG1111";

			OrgCusCode code2 = importCartage.CustomsCodes.AddNew();
			code2.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			code2.OK_CustomsRegNo = "REG2222";

			AssertEquals("Should return pickup cartage co", importCartage.OH_FullName + " CR#: REG2222", ContainerWrapper.Haulier);

			Shipment.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;
			AssertEquals("Should return blank", "", ContainerWrapper.Haulier);

			Consol.JK_OA_ArrivalUnpackCFSTransportAddress = importCartage.MainAddress.PK;
			AssertEquals("Should return consol cartage company if shipment not specified", importCartage.OH_FullName + " CR#: REG2222", ContainerWrapper.Haulier);
		}

		public void TestForwardingInstructionPackingDetails()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			packLine1.JL_PackageCount = 2;
			packLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine1.JL_Description = "Description";
			packLine1.JL_RH_NKCommodityCode = "HAZ";
			packLine1.JL_ActualWeight = 22.3m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;
			packLine2.JL_PackageCount = 14;
			packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine2.JL_Description = "Description";
			packLine2.JL_RH_NKCommodityCode = "GEN";
			packLine2.JL_ActualWeight = 22.4m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_FreightMode = FreightConstants.OuterPackType;
			packLine3.JL_PackageCount = 25;
			packLine3.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine3.JL_Description = "Description";
			packLine3.JL_RH_NKCommodityCode = "GEN";
			packLine3.JL_ActualWeight = 11.1m;
			packLine3.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			PackLine packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.JL_FreightMode = FreightConstants.OuterPackType;
			packLine4.JL_PackageCount = 2;
			packLine4.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine4.JL_Description = "Description1";
			packLine4.JL_RH_NKCommodityCode = "GEN";
			packLine4.JL_ActualWeight = 18.3m;
			packLine4.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			PackLine packLine5 = shipment.OuterPackLines.AddNew();
			packLine5.JL_FreightMode = FreightConstants.OuterPackType;
			packLine5.JL_PackageCount = 1;
			packLine5.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine5.JL_Description = "Description";
			packLine5.JL_RH_NKCommodityCode = "GEN";
			packLine5.JL_ActualWeight = 10.8m;
			packLine5.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			PackLine packLine6 = shipment.OuterPackLines.AddNew();
			packLine6.JL_FreightMode = FreightConstants.OuterPackType;
			packLine6.JL_PackageCount = 4;
			packLine6.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine6.JL_Description = "Description";
			packLine6.JL_RH_NKCommodityCode = "GEN";
			packLine6.JL_ActualWeight = 11.8m;
			packLine6.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			PackLine packLine7 = shipment.OuterPackLines.AddNew();
			packLine7.JL_FreightMode = FreightConstants.OuterPackType;
			packLine7.JL_PackageCount = 8;
			packLine7.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine7.JL_Description = "Description";
			packLine7.JL_RH_NKCommodityCode = "GEN";
			packLine7.JL_ActualWeight = 4.5m;
			packLine7.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;

			PackLine packLine8 = shipment.OuterPackLines.AddNew();
			packLine8.JL_FreightMode = FreightConstants.OuterPackType;
			packLine8.JL_PackageCount = 2;
			packLine8.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine8.JL_Description = "Description2";
			packLine8.JL_RH_NKCommodityCode = "GEN";
			packLine8.JL_ActualWeight = 1.1m;
			packLine8.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			UNDGSubstance undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = "2";
			undgSubstance.DG_Variant = "b";
			undgSubstance.DG_PSN = "PSN";
			undgSubstance.DG_Class = "1.1";
			undgSubstance.DG_PG = "PG";

			UNDGSubstance undgSubstance2 = Factory.New<UNDGSubstance>();
			undgSubstance2.DG_UNNO = "3";
			undgSubstance2.DG_Variant = "b";
			undgSubstance2.DG_PSN = "A Very Long Proper Shipping Name that goes until Zayden turns 13 and then we'll see what happens";
			undgSubstance2.DG_Class = "1.1";
			undgSubstance2.DG_PG = "PG";

			packLine1.UNDGs.AddNew().DI_DG = undgSubstance.PK;
			packLine8.UNDGs.AddNew().DI_DG = undgSubstance2.PK;

			packLine1.Containers.Add(FreightContainer);
			packLine2.Containers.Add(FreightContainer);
			packLine3.Containers.Add(FreightContainer);
			packLine4.Containers.Add(FreightContainer);
			packLine5.Containers.Add(FreightContainer);
			packLine6.Containers.Add(FreightContainer);
			packLine7.Containers.Add(FreightContainer);
			packLine8.Containers.Add(FreightContainer);

			DocContainer docContainer = DocContainer.New(FreightContainer, shipment, Factory);

			ZString packingDetails = docContainer.ForwardingInstructionPackingDetails;

			ZStringBuilder expectedPackLines = new ZStringBuilder();
			expectedPackLines.Append(" 2 PLT - Description - HAZ - UN2, PSN, class 1.1, PG PG - 22.3 KG");
			expectedPackLines.Append(" 2 KEG - Description2 - GEN - UN3, A Very Long Proper Shipping Name that goes until Zayden");
			expectedPackLines.Append("39 PLT - Description - GEN - 33.5 KG");
			expectedPackLines.Append(" 2 PLT - Description1 - GEN - 18.3 KG");
			expectedPackLines.Append(" 5 KEG - Description - GEN - 22.6 KG");
			expectedPackLines.Append(" 8 KEG - Description - GEN - 4.5 LB");

			AssertMultilineASCIIEquals("Forwarding instruction details by container", expectedPackLines.ToStringWithNewLineBetweenAppends(), packingDetails);
		}

		public void TestTotalAllocatedShipmentPackagesPackType()
		{
			AddContainerToConsolAndShipment();
			Consol.AutomaticallyUpdatePackLineContainers = false;
			PackLine.JL_PackageCount = 40;
			PackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			AssertEquals("TotalAllocatedShipmentPackages", 40, ContainerWrapper.TotalAllocatedShipmentPackages);
			AssertEquals("TotalAllocatedShipmentPackagesPackType", Core.Constants.PkgUnit.Pallet, ContainerWrapper.TotalAllocatedShipmentPackagesPackType);

			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 60;
			PackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine2.SetContainer(Consol, FreightContainer);

			AssertEquals("TotalAllocatedShipmentPackages", 100, ContainerWrapper.TotalAllocatedShipmentPackages);
			AssertEquals("TotalAllocatedShipmentPackagesPackType", Core.Constants.PkgUnit.Pallet, ContainerWrapper.TotalAllocatedShipmentPackagesPackType);

			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 60;
			packLine3.JL_F3_NKPackType = Core.Constants.PkgUnit.Carton;
			packLine3.SetContainer(Consol, FreightContainer);
			AssertEquals("TotalAllocatedShipmentPackagesPackType", Core.Constants.PkgUnit.Piece, ContainerWrapper.TotalAllocatedShipmentPackagesPackType);

			var shipment2 = Consol.Shipments.AddNew();
			var packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.JL_PackageCount = 134;
			packLine4.JL_F3_NKPackType = Core.Constants.PkgUnit.Case;
			packLine4.SetContainer(Consol, FreightContainer);

			//First Shipment
			AssertEquals("TotalAllocatedShipmentPackagesPackType for shipment 1", Core.Constants.PkgUnit.Piece, ContainerWrapper.TotalAllocatedShipmentPackagesPackType);

			//Second Shipment
			ContainerWrapper = DocContainer.New(FreightContainer, shipment2, Factory);
			AssertEquals("TotalAllocatedShipmentPackagesPackType for shipment 2", Core.Constants.PkgUnit.Case, ContainerWrapper.TotalAllocatedShipmentPackagesPackType);
		}

		#endregion

		#region ZDateTime Fields

		public void TestBookingCutOffDate()
		{
			AddContainerToConsolAndShipment();
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("BookingCutOffDate", ZDateTime.Empty, ContainerWrapper.BookingCutOffDate);

			Transport transport = Shipment.Consols[0].Transports[0];
			transport.JW_TerminalCutOff = ZDateTime.Today;
			AssertEquals("BookingCutOffDate", ZDateTime.Today, ContainerWrapper.BookingCutOffDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("BookingCutOffDate", ZDateTime.Empty, ContainerWrapper.BookingCutOffDate);
			transport.JW_DepotCutOff = ZDateTime.Today;
			AssertEquals("BookingCutOffDate", ZDateTime.Today, ContainerWrapper.BookingCutOffDate);
		}

		public void TestPickUpDate()
		{
			AddContainerToConsolAndShipment();
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("PickUpDate", ZDateTime.Empty, ContainerWrapper.PickUpDate);
			Shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Today;
			AssertEquals("PickUpDate", ZDateTime.Today, ContainerWrapper.PickUpDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("PickUpDate", ZDateTime.Empty, ContainerWrapper.PickUpDate);
		}

		public void TestAvailableDate()
		{
			AddContainerToConsolAndShipment();

			Shipment.DocsAndCartage.JP_FCLAvailable = ZDateTime.Today.AddDays(3);
			Shipment.DocsAndCartage.JP_LCLAvailable = ZDateTime.Today.AddDays(4);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("AvailableDate on shipment", Shipment.DocsAndCartage.JP_FCLAvailable, ContainerWrapper.AvailableDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("AvailableDate on shipment", Shipment.DocsAndCartage.JP_LCLAvailable, ContainerWrapper.AvailableDate);

			FreightContainer.JC_FCLAvailable = ZDateTime.Today;
			FreightContainer.JC_LCLAvailable = ZDateTime.Today.AddDays(1);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("AvailableDate", FreightContainer.JC_FCLAvailable, ContainerWrapper.AvailableDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("AvailableDate", FreightContainer.JC_LCLAvailable, ContainerWrapper.AvailableDate);
		}

		public void TestAvailableDateForConsolContainer()
		{
			AddContainerToConsol();

			CommonContainer container = Consol.Containers[0];

			container.JC_FCLAvailable = ZDateTime.Today.AddDays(3);
			container.JC_LCLAvailable = ZDateTime.Today.AddDays(4);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("CTO AvailableDate", ZDateTime.Today.AddDays(3), ContainerWrapper.AvailableDate);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Depot AvailableDate", ZDateTime.Today.AddDays(4), ContainerWrapper.AvailableDate);
		}

		public void TestStorageCommenceDate()
		{
			AddContainerToConsolAndShipment();

			Shipment.DocsAndCartage.JP_FCLStorageCommences = ZDateTime.Today.AddDays(3);
			Shipment.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.Today.AddDays(4);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("StorageCommenceDate on shipment", Shipment.DocsAndCartage.JP_FCLStorageCommences, ContainerWrapper.StorageCommenceDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("StorageCommenceDate on shipment", Shipment.DocsAndCartage.JP_LCLStorageCommences, ContainerWrapper.StorageCommenceDate);

			FreightContainer.JC_ArrivalCTOStorageStartDate = ZDateTime.Today;
			FreightContainer.JC_LCLStorageCommences = ZDateTime.Today.AddDays(1);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("StorageCommenceDate", FreightContainer.JC_ArrivalCTOStorageStartDate, ContainerWrapper.StorageCommenceDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("StorageCommenceDate", FreightContainer.JC_LCLStorageCommences, ContainerWrapper.StorageCommenceDate);
		}

		public void TestStorageCommenceDateForConsolContainer()
		{
			AddContainerToConsol();

			CommonContainer container = Consol.Containers[0];

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(3);
			container.JC_LCLStorageCommences = ZDateTime.Today.AddDays(4);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("CTO StorageCommencesDate", ZDateTime.Today.AddDays(3), ContainerWrapper.StorageCommenceDate);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Depot StorageCommenceDate", ZDateTime.Today.AddDays(4), ContainerWrapper.StorageCommenceDate);
		}

		public void TestCutOffOrAvailableDate()
		{
			AddContainerToConsolAndShipment();
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			Transport transport = Shipment.Consols[0].Transports[0];
			transport.JW_TerminalCutOff = ZDateTime.Today;
			FreightContainer.JC_FCLAvailable = ZDateTime.Today.AddDays(1);

			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("CutOffOrAvailableDate", ZDateTime.Today, ContainerWrapper.CutOffOrAvailableDate);

			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("CutOffOrAvailableDate", ZDateTime.Today.AddDays(1), ContainerWrapper.CutOffOrAvailableDate);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			transport.JW_DepotCutOff = ZDateTime.Today.AddDays(2);
			FreightContainer.JC_LCLAvailable = ZDateTime.Today.AddDays(3);

			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("CutOffOrAvailableDate", ZDateTime.Today.AddDays(2), ContainerWrapper.CutOffOrAvailableDate);

			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("CutOffOrAvailableDate", ZDateTime.Today.AddDays(3), ContainerWrapper.CutOffOrAvailableDate);

			CommonContainer cont = Factory.New<CommonContainer>();
			ForwardingShipment ship = Factory.New<ForwardingShipment>();
			DocContainer docCont = DocContainer.New(cont, ship, Factory);
			docCont.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			ZDateTime cutOffDate = ZDateTime.Today.AddDays(7);
			origin.JA_DGCutOff = cutOffDate;
			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_DepotCutOff = cutOffDate;
			ship.JS_JX = sailing.PK;
			AssertEquals("CutOffOrAvailableDate", cutOffDate, docCont.CutOffOrAvailableDate);
		}

		public void TestPickupOrStorageCommenceDate()
		{
			AddContainerToConsolAndShipment();
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Today;
			FreightContainer.JC_ArrivalCTOStorageStartDate = ZDateTime.Today.AddDays(1);

			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today, ContainerWrapper.PickupOrStorageCommenceDate);
			AssertEquals("FCL DEP Heading", "PICKUP DATE", ContainerWrapper.PickupOrStorageCommenceDateHeading);

			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today.AddDays(1), ContainerWrapper.PickupOrStorageCommenceDate);
			AssertEquals("FCL ARV Heading", "STORAGE STARTS", ContainerWrapper.PickupOrStorageCommenceDateHeading);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			FreightContainer.JC_LCLStorageCommences = ZDateTime.Today.AddDays(2);

			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Empty, ContainerWrapper.PickupOrStorageCommenceDate);
			AssertEquals("LCL DEP Heading", "PICKUP DATE", ContainerWrapper.PickupOrStorageCommenceDateHeading);

			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("PickupOrStorageCommenceDate", ZDateTime.Today.AddDays(2), ContainerWrapper.PickupOrStorageCommenceDate);
			AssertEquals("LCL ARV Heading", "STORAGE STARTS", ContainerWrapper.PickupOrStorageCommenceDateHeading);
		}

		public void TestIDocCartageAdviceDates()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonContainer container = Factory.New<CommonContainer>();

			shipment.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2011, 4, 18);
			shipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2011, 4, 20);
			shipment.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2011, 4, 22);
			shipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2011, 4, 25);

			Transport shipmentTransport1 = shipment.Transports.AddNew();
			shipmentTransport1.JW_ETD = new ZDateTime(2011, 2, 1);
			shipmentTransport1.JW_ETA = new ZDateTime(2011, 2, 15);

			Transport shipmentTransport2 = shipment.Transports.AddNew();
			shipmentTransport2.JW_ETD = new ZDateTime(2011, 1, 1);
			shipmentTransport2.JW_ETA = new ZDateTime(2011, 1, 15);
			shipmentTransport2.JW_TerminalCutOff = new ZDateTime(2011, 1, 18);
			shipmentTransport2.JW_DepotCutOff = new ZDateTime(2011, 1, 20);
			shipmentTransport2.JW_TerminalReceivalCommences = new ZDateTime(2011, 1, 22);
			shipmentTransport2.JW_DepotReceivalCommences = new ZDateTime(2011, 1, 25);

			Transport shipmentTransport3 = shipment.Transports.AddNew();
			shipmentTransport3.JW_ETD = new ZDateTime(2011, 4, 1);
			shipmentTransport3.JW_ETA = new ZDateTime(2011, 4, 15);

			Transport shipmentTransport4 = shipment.Transports.AddNew();
			shipmentTransport4.JW_ETD = new ZDateTime(2011, 3, 1);
			shipmentTransport4.JW_ETA = new ZDateTime(2011, 3, 15);

			DocContainer wrapper = DocContainer.New(container, shipment, Factory);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 18), wrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 18), wrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 22), wrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 22), wrapper.CartageStorageCommenceDate);

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 20), wrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 20), wrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 25), wrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 25), wrapper.CartageStorageCommenceDate);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);

			Transport consolTransport1 = consol.Transports[0];
			consolTransport1.JW_ETD = new ZDateTime(2011, 2, 1);
			consolTransport1.JW_ETA = new ZDateTime(2011, 2, 15);

			Transport consolTransport2 = consol.Transports.AddNew();
			consolTransport2.JW_ETD = new ZDateTime(2011, 1, 1);
			consolTransport2.JW_ETA = new ZDateTime(2011, 1, 15);
			consolTransport2.JW_TerminalCutOff = new ZDateTime(2011, 5, 18);
			consolTransport2.JW_DepotCutOff = new ZDateTime(2011, 5, 20);
			consolTransport2.JW_TerminalReceivalCommences = new ZDateTime(2011, 5, 22);
			consolTransport2.JW_DepotReceivalCommences = new ZDateTime(2011, 5, 25);

			Transport consolTransport3 = consol.Transports.AddNew();
			consolTransport3.JW_ETD = new ZDateTime(2011, 4, 1);
			consolTransport3.JW_ETA = new ZDateTime(2011, 4, 15);
			consolTransport3.JW_TerminalAvailabilityDate = new ZDateTime(2011, 8, 18);
			consolTransport3.JW_DepotAvailabilityDate = new ZDateTime(2011, 8, 20);
			consolTransport3.JW_TerminalStorageDate = new ZDateTime(2011, 8, 22);
			consolTransport3.JW_DepotStorageDate = new ZDateTime(2011, 8, 25);

			Transport consolTransport4 = consol.Transports.AddNew();
			consolTransport4.JW_ETD = new ZDateTime(2011, 3, 1);
			consolTransport4.JW_ETA = new ZDateTime(2011, 3, 15);

			wrapper = DocContainer.New(container, Factory);
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 5, 18), wrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 8, 18), wrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 5, 22), wrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 8, 22), wrapper.CartageStorageCommenceDate);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 5, 20), wrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 8, 20), wrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 5, 25), wrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 8, 25), wrapper.CartageStorageCommenceDate);
		}

		#endregion

		#region ZInt Fields

		public void TestTotalAllocatedShipmentPackages()
		{
			AddContainerToConsolAndShipment();
			Consol.AutomaticallyUpdatePackLineContainers = false;
			PackLine.JL_PackageCount = 40;
			AssertEquals("TotalAllocatedShipmentPackages", 40, ContainerWrapper.TotalAllocatedShipmentPackages);

			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 60;
			packLine2.SetContainer(Consol, FreightContainer);

			AssertEquals("TotalAllocatedShipmentPackages", 100, ContainerWrapper.TotalAllocatedShipmentPackages);

			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 12;
			if (FreightContainer.PackLines.Contains(packLine3))
			{
				FreightContainer.PackLines.Remove(packLine3);
			}

			AssertEquals("TotalAllocatedShipmentPackages", 100, ContainerWrapper.TotalAllocatedShipmentPackages);

			var shipment2 = Consol.Shipments.AddNew();
			var packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.JL_PackageCount = 134;
			packLine4.SetContainer(Consol, FreightContainer);

			//First Shipment
			AssertEquals("TotalAllocatedShipmentPackages", 100, ContainerWrapper.TotalAllocatedShipmentPackages);

			//Second Shipment
			ContainerWrapper = DocContainer.New(FreightContainer, shipment2, Factory);
			AssertEquals("TotalAllocatedShipmentPackages", 134, ContainerWrapper.TotalAllocatedShipmentPackages);
		}

		#endregion

		#region Implementation

		ForwardingContainer FreightContainer;
		DocContainer ContainerWrapper;
		IDocServicesParent ServicesParentWrapper;
		PackLine PackLine;
		ForwardingShipment Shipment;
		ForwardingConsol Consol;

		protected override void SetUp()
		{
			FreightContainer = Factory.New<ForwardingContainer>();
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			ServicesParentWrapper = ContainerWrapper;

			base.SetUp();
		}

		void AddContainerToConsolAndShipment()
		{
			Consol = Factory.New<ForwardingConsol>();
			Consol.Containers.Add(FreightContainer);

			Shipment = Consol.Shipments.AddNew();

			PackLine = Shipment.OuterPackLines.AddNew();
			PackLine.SetContainer(Consol, FreightContainer);
			ContainerWrapper = DocContainer.New(FreightContainer, Shipment, Factory);
		}

		void AddContainerToConsol()
		{
			Consol = Factory.New<ForwardingConsol>();
			Consol.Containers.Add(FreightContainer);

			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
		}
		#endregion

		#region IDocServiceParent Members

		public void TestConsolNumber()
		{
			var container1 = Factory.New<CommonContainer>();
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonConsol consol1 = shipment.Consols.AddNew();
			ServicesParentWrapper = DocContainer.New(container1, Factory);
			AssertEquals("Consol Number should be nothing", ZString.Empty, ServicesParentWrapper.ConsolNumber);

			consol1.Containers.Add(container1);
			AssertEquals("Consol Number", consol1.JK_UniqueConsignRef, ServicesParentWrapper.ConsolNumber);
		}

		public void TestGoodsDescription()
		{
			AssertEquals("", ServicesParentWrapper.GoodsDescription);
		}

		public void TestPackages()
		{
			AssertEquals("Packages", ContainerWrapper.TotalPackLinePackages.ToString(), ServicesParentWrapper.Packages);
		}

		public void TestWeight()
		{
			AssertEquals(FreightContainer.JC_GrossWeight.ToString(), ServicesParentWrapper.Weight);
		}

		public void TestVolume()
		{
			AssertEquals("", ServicesParentWrapper.Volume);
		}

		public void TestWeightUnit()
		{
			AssertEquals(FreightContainer.JC_GrossWeightUQ, ServicesParentWrapper.WeightUnit);
		}

		public void TestVolumeUnit()
		{
			AssertEquals("", ServicesParentWrapper.VolumeUnit);
		}

		public void TestMasterBillNum()
		{
			var container1 = Factory.New<CommonContainer>();
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonConsol consol1 = shipment.Consols.AddNew();
			DocContainer container1Wrapper = DocContainer.New(container1, Factory);
			ServicesParentWrapper = DocContainer.New(container1, Factory);

			AssertEquals("MasterBill should be nothing", ZString.Empty, ServicesParentWrapper.MasterBillNum);

			consol1.Containers.Add(container1);

			AssertEquals("MasterBill should be Consols'", consol1.JK_MasterBillNum, ServicesParentWrapper.MasterBillNum);
		}

		public void TestMasterBillHeading()
		{
			var container1 = Factory.New<CommonContainer>();
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonConsol consol1 = shipment.Consols.AddNew();
			DocContainer container1Wrapper = DocContainer.New(container1, Factory);
			ServicesParentWrapper = DocContainer.New(container1, Factory);

			AssertEquals("MasterBill Heading should be nothing", ZString.Empty, ServicesParentWrapper.MasterBillHeading);

			consol1.Containers.Add(container1);

			AssertEquals("Housebill Heading should be Consols", "MASTER", ServicesParentWrapper.MasterBillHeading);
		}

		public void TestHouseBill()
		{
			AssertEquals("Housebill should be empty", ZString.Empty, ServicesParentWrapper.HouseBill);

			AddContainerToConsolAndShipment();
			AssertEquals("Housebill should be HOUSE44554", Shipment.JS_HouseBill, ServicesParentWrapper.HouseBill);
		}

		public void TestHouseBillHeading()
		{
			AssertEquals("Housebill heading should be empty", ZString.Empty, ServicesParentWrapper.HouseBillHeading);

			AddContainerToConsolAndShipment();
			AssertEquals("Housebill should be shipments'", "", ServicesParentWrapper.HouseBillHeading);
		}

		public void TestTransportInfo()
		{
			ForwardingContainer container1 = Factory.New<ForwardingContainer>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol1 = shipment.Consols.AddNew();
			DocContainer container1Wrapper = DocContainer.New(container1, Factory);
			ServicesParentWrapper = DocContainer.New(container1, Factory);

			AssertEquals("TransportInfo should be nothing", ZString.Empty, ServicesParentWrapper.TransportInfo);

			consol1.Containers.Add(container1);

			ZString vesselVoy = (consol1.Vessel != null) ? consol1.Vessel.RV_Code + " / " : "";
			vesselVoy += (consol1.Voyage != null) ? consol1.Voyage.JV_VoyageFlight + " / " : "";

			AssertEquals("Transport Info", vesselVoy, ServicesParentWrapper.TransportInfo);
		}

		public void TestETAandETD()
		{
			ForwardingContainer container1 = Factory.New<ForwardingContainer>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol1 = shipment.Consols.AddNew();
			DocContainer container1Wrapper = DocContainer.New(container1, Factory);
			ServicesParentWrapper = DocContainer.New(container1, Factory);

			AssertEquals("ETA Should be empty", ZDateTime.Empty, ServicesParentWrapper.ETA);
			AssertEquals("ETD Should be empty", ZDateTime.Empty, ServicesParentWrapper.ETD);

			consol1.Containers.Add(container1);
			ServicesParentWrapper = DocContainer.New(container1, Factory);

			AssertEquals("ETA should be two days from today", consol1.JK_JX_JB_E_ARV, ServicesParentWrapper.ETA);
			AssertEquals("ETD should be today", consol1.JK_JX_JA_E_DEP, ServicesParentWrapper.ETD);
		}

		public void TestContainerNumbers()
		{
			AssertEquals(FreightContainer.JC_ContainerNum, ServicesParentWrapper.ContainerNumbers);
		}

		public void TestContext()
		{
			AssertEquals("Context should be Container", "CONTAINER", ServicesParentWrapper.Context);
		}

		public void TestOwnerRefAndOrderRef()
		{
			var container1 = Factory.New<CommonContainer>();
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_AgentsReference = "222AAA";
			ServicesParentWrapper = DocContainer.New(container1, Factory);
			AssertEquals("OwnerRefAndOrderRef should be nothing", ZString.Empty, ServicesParentWrapper.OwnerRefAndOrderRef);

			consol1.Containers.Add(container1);
			AssertEquals("Should return Agents Reference", "222AAA", ServicesParentWrapper.OwnerRefAndOrderRef);
		}

		public void TestOwnerRefAndOrderRefHeading()
		{
			var container1 = Factory.New<CommonContainer>();
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonConsol consol1 = shipment.Consols.AddNew();
			ServicesParentWrapper = DocContainer.New(container1, Factory);
			AssertEquals("OwnerRefAndOrderRefHeading should be nothing", ZString.Empty, ServicesParentWrapper.OwnerRefAndOrderRefHeading);

			consol1.Containers.Add(container1);
			AssertEquals("OwnerRefAndOrderRefHeading", "AGENTS REFERENCE", ServicesParentWrapper.OwnerRefAndOrderRefHeading);
		}

		#endregion

		#region Not Cleared by Agent

		public void TestNotClearedByAgent()
		{
			FreightDataRegistry.Instance.NotClearedByAgentStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "~~~IIIIII");

			AssertEquals("", ContainerWrapper.NotClearedByAgentNumber);
			AssertEquals(ZDateTime.Empty, ContainerWrapper.NotClearedByAgentIssueDate);
			AssertEquals(ZDateTime.Empty, ContainerWrapper.NotClearedByAgentExpiryDate);

			Consol = Factory.New<ForwardingConsol>();
			Consol.Containers.Add(FreightContainer);
			Shipment = Consol.Shipments.AddNew();

			CusEntryNumber num = Shipment.CusEntryNumbers.AddNew();
			num.CE_EntryType = CusEntryNumberTypes.EU.T1;
			num.CE_EntryNum = "asdasd";
			num.CE_IssueDate = new ZDateTime(2008, 10, 9);
			num.CE_ExpiryDate = new ZDateTime(2008, 10, 15);

			AssertEquals("", ContainerWrapper.NotClearedByAgentNumber);
			AssertEquals(ZDateTime.Empty, ContainerWrapper.NotClearedByAgentIssueDate);
			AssertEquals(ZDateTime.Empty, ContainerWrapper.NotClearedByAgentExpiryDate);

			Shipment.CusEntryNumbers.RemoveAndDeleteAll();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Germany);

			AssertEquals("", ContainerWrapper.NotClearedByAgentNumber);
			AssertEquals(ZDateTime.Empty, ContainerWrapper.NotClearedByAgentIssueDate);
			AssertEquals(ZDateTime.Empty, ContainerWrapper.NotClearedByAgentExpiryDate);

			num = Shipment.CusEntryNumbers.AddNew();
			num.CE_EntryType = CusEntryNumberTypes.EU.T1;
			num.CE_EntryNum = "asdasd";
			num.CE_IssueDate = new ZDateTime(2008, 10, 9);
			num.CE_ExpiryDate = new ZDateTime(2008, 10, 15);

			AssertEquals("", ContainerWrapper.NotClearedByAgentNumber);
			AssertEquals(ZDateTime.Empty, ContainerWrapper.NotClearedByAgentIssueDate);
			AssertEquals(ZDateTime.Empty, ContainerWrapper.NotClearedByAgentExpiryDate);

			Consol.Containers.Remove(FreightContainer);
			AddContainerToConsolAndShipment();

			num = Shipment.CusEntryNumbers.AddNew();
			num.CE_EntryType = CusEntryNumberTypes.EU.T1;
			num.CE_EntryNum = "asdasd";
			num.CE_IssueDate = new ZDateTime(2008, 10, 9);
			num.CE_ExpiryDate = new ZDateTime(2008, 10, 15);

			AssertEquals("asdasd", ContainerWrapper.NotClearedByAgentNumber);
			AssertEquals(new ZDateTime(2008, 10, 9), ContainerWrapper.NotClearedByAgentIssueDate);
			AssertEquals(new ZDateTime(2008, 10, 15), ContainerWrapper.NotClearedByAgentExpiryDate);
		}

		#endregion
	}
}
