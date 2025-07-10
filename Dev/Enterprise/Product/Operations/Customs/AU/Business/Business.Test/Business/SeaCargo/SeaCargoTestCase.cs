using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class SeaCargoTestCase : TestCaseWithFactory
	{
		#region Implementation

		#region Constants

		protected const string OceanBill1 = "SUDU400014772110";
		protected const string PrincipalID1 = "C065301902";
		protected const string VesselName1 = "ADMIRALENGRACHT";
		protected const string Voyage1 = "442";
		protected const string RL_NKPortOfDischarge1 = "AUSYD";
		protected const string RL_NKPortOfLoading1 = "NZAKL";
		protected const string Vessel1 = "AUSTRALIAN STAR";

		protected const string OceanBill2 = "SUDU400014772111";
		protected const string PrincipalID2 = "C845624152";
		protected const string VesselName2 = "AMERICA STAR";
		protected const string Voyage2 = "5";
		protected const string RL_NKPortOfDischarge2 = "AUMEL";
		protected const string RL_NKPortOfLoading2 = "SGSIN";
		protected const string Vessel2 = "AUSTRALIAN ALLIANCE";

		protected const string HouseBillNumber1 = "HB00001";
		protected const string HouseBillNumber2 = "HB00002";
		protected const string HouseBillNumber3 = "HB00003";

		protected const string ClientID1 = "C000871607";
		protected const string ClientID2 = "C000871617";

		protected const string ContainerNumber1 = "FSCU6400235";
		protected const string ContainerNumber2 = "FDSU6400235";
		protected const string ContainerNumber3 = "FOPU6400235";
		protected const string ContainerNumber4 = "HJPD6412335";
		protected const string ContainerNumber5 = "PSPU9412335";
		protected const string ContainerNumber6 = "IJVJ6412335";
		protected const string ContainerNumber7 = "YNPB6412335";
		protected const string ContainerNumber8 = "CTRL0000017";

		#endregion

		#region Object Types

		protected virtual Type ConsolType
		{
			get
			{
				return typeof(ForwardingConsol);
			}
		}

		#endregion

		#region Variables

		protected CusSCAHouse houseBill;

		#endregion

		protected CusSCAOceanBill CreateTestOriginalOceanBill()
		{
			CusSCAOceanBill result = CreateOceanBill();

			CusSCAHouse houseBill1 = CreateHouseBill(result, HouseBillNumber1);
			CusSCAHouse houseBill2 = CreateHouseBill(result, HouseBillNumber2);
			CusSCAHouse houseBill3 = CreateHouseBill(result, HouseBillNumber3);

			CreatePivot(houseBill1, ContainerNumber1);

			CreatePivot(houseBill2, ContainerNumber2);
			CreatePivot(houseBill2, ContainerNumber3);

			CreatePivot(houseBill3, ContainerNumber3);
			CreatePivot(houseBill3, ContainerNumber1);
			return result;
		}

		protected CusSCAOceanBill CreateOceanBill()
		{
			CusSCAOceanBill result = Factory.New<CusSCAOceanBill>();
			result.CB_OceanBill = OceanBill1;
			result.CB_PrincipalID = PrincipalID1;
			result.CB_Voyage = Voyage1;
			result.CB_RL_NKPortOfDischarge = RL_NKPortOfDischarge1;
			result.CB_RL_NKPortOfLoading = RL_NKPortOfLoading1;
			result.CB_VesselName = TestVessel.RV_Code;
			result.CB_OH_ShippingLine = CreateShippingLine("Shipping Line").PK;

			return result;
		}

		RefVessel fTestVessel;
		protected RefVessel TestVessel
		{
			get
			{
				if (fTestVessel == null)
				{
					fTestVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, Vessel1));
					if (fTestVessel == null)
					{
						fTestVessel = RefVessel.New(Factory);
						fTestVessel.RV_Code = Vessel1;
						fTestVessel.RV_LloydsNumber = "8610033";
					}
				}
				return fTestVessel;
			}
		}

		protected CusSCAHouse CreateHouseBill(CusSCAOceanBill oceanBill, string houseBillNumber)
		{
			CusSCAHouse result = oceanBill.HouseBills.AddNew();
			result.CA_HouseBill = houseBillNumber;

			result.CA_RN_NKGoodsOrigin = "NZ";
			result.CA_RL_NK_PortOfDestination = "AUBNE";
			result.CA_RL_NK_PortOfOrigin = "NZAKL";
			result.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
			result.CA_ConsigneeName = "TRITEC PTY LTD";
			result.CA_ConsigneeAddress1 = "C O NEW CENTURY PACKING";
			result.CA_ConsigneeAddress2 = "UNIT 5 370 NUDGEE ROAD";
			result.CA_ConsigneeSuburb = "HENDRA QLD AUSTRALIA";
			result.CA_ConsigneePostcode = "4011";
			result.CA_ConsignorName = "HUHTAMAKI VAN LEER  NZ  LTD";
			result.CA_ConsignorAddress1 = "FLEXIBLE PACKAGING DIVISION";
			result.CA_ConsignorAddress2 = "PRIVATE BAG 93 002";
			result.CA_ConsignorSuburb = "NEW LYNN AUCKLAND";
			result.CA_NotifyName = "TRITEC PTY LTD";
			result.CA_NotifyAddress1 = "C O NEW CENTURY PACKING";
			result.CA_NotifyAddress2 = "UNIT 5 370 NUDGEE ROAD";
			result.CA_NotifySuburb = "HENDRA QLD AUSTRALIA";
			result.CA_NotifyPostcode = "4011";

			return result;
		}

		protected CusSCAContainer FindContainerOnOceanBill(CusSCAOceanBill oceanBill, string containerNumber)
		{
			CusSCAContainer result = null;
			foreach (CusSCAContainer container in oceanBill.Containers)
			{
				if (container.CN_ContainerNumber == containerNumber)
				{
					result = container;
					break;
				}
			}
			return result;
		}

		protected CusSCAContainer EnsureOceanBillContainer(CusSCAOceanBill oceanBill, string containerNumber)
		{
			CusSCAContainer result = FindContainerOnOceanBill(oceanBill, containerNumber);
			if (result == null)
			{
				result = oceanBill.Containers.AddNew();
				result.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
				result.CN_RC_NKContainerType = "40GP";
				result.CN_ShipperOwnedContainer = false;
				result.CN_SealNumber = "89644";
				result.CN_ContainerNumber = containerNumber;
			}
			return result;
		}

		protected CusSCAPivot CreatePivot(CusSCAHouse houseBill, string containerNumber)
		{
			var container = EnsureOceanBillContainer(houseBill.OceanBill, containerNumber);

			var result = houseBill.Pivot.AddNew();
			result.CV_CN = container.PK;
			result.CV_PackageCount = 40;
			result.CV_PackageType = "BX";
			result.CV_MarksAndNumbers = "MARKSANDNUMBERS";
			result.CV_GoodsDescription = "STC 40 BAGS OF PLASTIC REGRIND";
			result.CV_WeightUQ = Core.Constants.Weight.Kilograms;
			result.CV_Weight = 23900.00m;
			result.CV_Volume = 68.000m;

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetContainerISOType();
		}

		protected void SetContainerISOType()
		{
			var container40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container40GP.RC_ISOType = "4000";

			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container20GP.RC_ISOType = "2000";
		}

		protected const string REFMESSAGENUMBERHOLDER = @"<<REFMESSAGENUMBERHOLDER>>";
		protected const string HouseBill1Acknowledgement = @"UNH+77600+CUSRES:002:912:UN'BGM+961++9:200402091346:203+9+ZZ:802*8610033*HFRD'LOC+12:AUSYD'RFF+ACW:<<REFMESSAGENUMBERHOLDER>>'UNT+5+77600'";
		protected const string HouseBill2Acknowledgement = @"UNH+77600+CUSRES:002:912:UN'BGM+961++9:200402091346:203+9+ZZ:802*8610033*HFRD'LOC+12:AUSYD'RFF+ACW:<<REFMESSAGENUMBERHOLDER>>'UNT+5+77600'";
		protected const string DuplicateContainerRejection = @"UNH+71400+CUSRES:002:912:UN'BGM+963+<<REFMESSAGENUMBERHOLDER>>+9:200402041744:203+9+ZZ:802*8610033*342'ERP+2:" + ContainerNumber7 + ":0'ERC+R:SCA:95+078:SCA:95'UNT+5+71400'";
		protected const string InvalidLloydsNumberRejection = @"UNH+72900+CUSRES:002:912:UN'BGM+963+<<REFMESSAGENUMBERHOLDER>>+9:200402051456:203+9+ZZ:802*8610033*93'ERP+2:" + ContainerNumber2 + "*C782074702:0'ERP+2:" + ContainerNumber3 + ":0'ERP+2:" + ContainerNumber5 + ":0'ERP+2:" + ContainerNumber6 + ":0'ERC+R:SCA:95+078:SCA:95'UNT+6+72900'";
		protected const string RejectEntireMessageRejection = @"UNH+41300+CUSRES:002:912:UN'BGM+963+<<REFMESSAGENUMBERHOLDER>>+9:200402201521:203+9+ZZ:802*40600'ERP+2:<<REFMESSAGENUMBERHOLDER>>:0'ERC+R:SCA:95+249:SCA:95'UNT+5+41300'";

		#region Organisation

		protected string houseBillDestinationPort = "AUMEL";
		protected string oceanBillDischargePort = "AUSYD";
		protected string arrivalCTOPremiseCode = "9823N";
		protected string localDepotPremiseCode = "9815C";

		protected OrgHeader ArrivalCTO
		{
			get
			{
				if (fArrivalCTO == null)
				{
					fArrivalCTO = CreateCTO("Arrival CTO");
					fArrivalCTO.MainAddress.LocalControlledPremisesID = arrivalCTOPremiseCode;
				}
				return fArrivalCTO;
			}
		}
		OrgHeader fArrivalCTO;

		protected OrgHeader UnpackDepot
		{
			get
			{
				if (fUnpackDepot == null)
				{
					fUnpackDepot = CreateDepot("Local Depot");
					fUnpackDepot.MainAddress.LocalControlledPremisesID = localDepotPremiseCode;
				}
				return fUnpackDepot;
			}
		}
		OrgHeader fUnpackDepot;

		protected OrgHeader CreateOrganisation(ZString name, ZString uNLOCO)
		{
			return CreateOrganisation(name, uNLOCO, Factory);
		}

		protected OrgHeader CreateOrganisation(ZString name, ZString uNLOCO, BusinessObjectFactory factory)
		{
			OrgHeader result = factory.New<OrgHeader>();
			result.OH_FullName = name;
			result.OH_RL_NKClosestPort = uNLOCO;
			result.MainAddress.OA_Address1 = new ZString("Address " + name).SubstringSafe(0, 50);
			return result;
		}

		protected OrgHeader CreateConsignee(ZString name)
		{
			return CreateConsignee(name, Factory);
		}

		protected OrgHeader CreateConsignee(ZString name, BusinessObjectFactory factory)
		{
			OrgHeader result = CreateOrganisation(name, "AUSYD", factory);
			result.OH_IsConsignee = true;
			return result;
		}

		protected OrgHeader CreateConsignor(ZString name)
		{
			return CreateConsignor(name, Factory);
		}

		protected OrgHeader CreateConsignor(ZString name, BusinessObjectFactory factory)
		{
			OrgHeader result = CreateOrganisation(name, "SGSIN", factory);
			result.OH_IsConsignor = true;
			return result;
		}

		protected const string DepotCode = "F1029";
		protected const string CTOCode = "CTO12";

		protected OrgHeader CreateDepot(ZString depotName)
		{
			OrgHeader result = CreateOrganisation(depotName, "AUSYD");
			result.OH_IsUnpackDepot = true;
			result.MainAddress.LocalControlledPremisesID = DepotCode;
			return result;
		}

		protected OrgHeader CreateCTO(ZString cTOName)
		{
			OrgHeader result = CreateOrganisation(cTOName, "AUSYD");
			result.OH_IsSeaCTO = true;
			result.MainAddress.LocalControlledPremisesID = CTOCode;
			return result;
		}

		protected const string ShippingLineABN = "61852570977";
		protected OrgHeader CreateShippingLine(ZString name)
		{
			OrgHeader result = CreateOrganisation(name, "AUSYD");
			result.OH_IsShippingLine = true;
			result.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, ClientID2);
			result.LocalBusinessRegNo = ShippingLineABN;
			return result;
		}

		protected OrgHeader CreateReceivingForwarder(ZString name, ZString clientID)
		{
			OrgHeader result = CreateOrganisation(name, "AUSYD");
			result.OH_IsForwarder = true;
			result.LocalManifestID = clientID;
			return result;
		}

		protected OrgHeader CreateReceivingForwarder(ZString name)
		{
			return CreateReceivingForwarder(name, ClientID1);
		}

		#endregion

		protected CommonContainer AddContainerToConsol(CommonConsol consol, ZString containerNumber)
		{
			CommonContainer result = consol.Containers.AddNew();
			result.JC_ContainerNum = containerNumber;
			result.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			return result;
		}

		protected CommonShipment AddShipmentToConsol(CommonConsol consol, ZString houseBill)
		{
			CommonShipment result = consol.Shipments.AddNew();
			result.JS_HouseBill = houseBill;
			result.ConsigneePK = CreateConsignee("CEE" + houseBill).PK;
			result.ConsignorPK = CreateConsignor("COR" + houseBill).PK;
			result.JS_OuterPacks = 10;
			result.JS_F3_NKPackType = "BOX";
			result.JS_ActualVolume = 1.0m;
			result.JS_ActualWeight = 1000.0m;
			result.JS_GoodsDescription = "Description of goods";
			result.JS_MarksAndNumbers = "NIL MARKS";
			return result;
		}

		protected CommonShipment AddCoLoadMasterToConsol(CommonConsol consol, ZString houseBill, ZString clientID)
		{
			CommonShipment result = AddShipmentToConsol(consol, houseBill);
			result.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			result.ConsigneePK = CreateReceivingForwarder("CoLoadMasterForwarder", clientID).PK;
			return result;
		}

		protected CommonShipment AddUltimateHouseBillToConsol(CommonConsol consol, CommonShipment coLoadMasterShipment, ZString houseBill)
		{
			CommonShipment result = AddShipmentToConsol(consol, houseBill);
			coLoadMasterShipment.CoLoadShipments.Add(result);
			return result;
		}

		protected CommonConsol CreateSeaConsol(Type consolType)
		{
			CommonConsol result = (CommonConsol)Factory.New(consolType);
			result.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			result.JK_RL_NKLoadPort = "SGSIN";
			result.JK_RL_NKDischargePort = "AUSYD";

			Transport transport = result.Transports[0];
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.NotEqual, ZString.Empty)).RV_Code;
			transport.JW_VoyageFlight = Voyage1;
			transport.JW_ETA = ZDateTime.Today.AddDays(1);

			AssertNotNull("Failed To Create Sailing", result.Schedule);

			result.SetDefaultReceivingForwarderAddress(CreateReceivingForwarder("Test Recieving Forwarder"));
			result.SetDefaultShippingLineAddress(CreateShippingLine("Test Shipping Line"));

			return result;
		}

		protected CommonConsol CreateSeaConsol()
		{
			return CreateSeaConsol(ConsolType);
		}

		protected CommonConsol CreateFCLConsol(Type consolType)
		{
			CommonConsol result = CreateSeaConsol(consolType);
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;

			return result;
		}

		protected CommonConsol CreateFCLConsol()
		{
			return CreateFCLConsol(ConsolType);
		}

		protected CommonConsol CreateLCLConsol()
		{
			CommonConsol result = CreateSeaConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.LCL;
			return result;
		}

		protected CommonConsol CreateBuyersConsol()
		{
			CommonConsol result = CreateSeaConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
			return result;
		}

		protected CommonConsol CreateGroupageConsol()
		{
			CommonConsol result = CreateSeaConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Groupage;
			return result;
		}

		protected CommonConsol CreateBulkConsol()
		{
			CommonConsol result = CreateSeaConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			return result;
		}

		protected CommonConsol CreateBreakBulkConsol()
		{
			CommonConsol result = CreateSeaConsol();
			result.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			return result;
		}

		protected void EnsureValidHouse(CusSCAHouse houseBill, ZString houseBillNum)
		{
			houseBill.CA_HouseBill = houseBillNum;
			houseBill.CA_OA_ConsigneeAddress = CreateConsignee(new ZString("C'NEE " + houseBill).SubstringSafe(0, 50)).MainAddress.PK;
			houseBill.CA_OA_ConsignorAddress = CreateConsignor(new ZString("C'NOR " + houseBill).SubstringSafe(0, 50)).MainAddress.PK;
			houseBill.CA_OH_Notify = houseBill.CA_OH_Consignee;
			houseBill.CA_RL_NK_PortOfOrigin = "USLAX";
			houseBill.CA_RL_NK_PortOfDestination = "AUSYD";
			houseBill.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.Collect;
			houseBill.CA_RN_NKGoodsOrigin = "US";
			houseBill.RunPreSaveValidation();
			Assert("Precondition: Housebill should not have any message errors "
				+ houseBill.Notifications.GetMessageErrors().ToUniqueMessageListString()
				+ houseBill.Notifications.GetErrors().ToUniqueMessageListString(),
				!houseBill.Notifications.HasMessageErrors() && !houseBill.Notifications.HasErrors());
		}

		protected void EnsureValidContainer(CusSCAContainer container, ZString containerNum)
		{
			container.CN_ContainerNumber = containerNum;
			container.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.CN_RC_NKContainerType = "40GP";
			container.RunPreSaveValidation();
			Assert("Precondition: Container should not have any message errors "
				+ container.Notifications.GetMessageErrors().ToUniqueMessageListString()
				+ container.Notifications.GetErrors().ToUniqueMessageListString(),
				!container.Notifications.HasMessageErrors() && !container.Notifications.HasErrors());
		}

		protected void EnsureValidPivot(CusSCAPivot pivot, ZString pivotDescription)
		{
			pivot.CV_GoodsDescription = pivotDescription;
			pivot.CV_PackageCount = 10;
			pivot.CV_MarksAndNumbers = "Nil Marks";
			pivot.CV_PackageType = CMRPackageTypes.Codes.Box;
			pivot.CV_Weight = 1000m;
			pivot.CV_Volume = 1m;
			pivot.RunPreSaveValidation();
			Assert("Precondition: Pivot should not have any message errors "
				+ pivot.Notifications.GetMessageErrors().ToUniqueMessageListString()
				+ pivot.Notifications.GetErrors().ToUniqueMessageListString(),
								!pivot.Notifications.HasMessageErrors() && !pivot.Notifications.HasErrors());
		}

		protected void EnsureValidUnderbond(CusUnderbond underbond, ZString originPremiseID, ZString destinationPresmiseID)
		{
			EnsureValidUnderbond(underbond, originPremiseID, destinationPresmiseID, false);
		}

		protected void EnsureValidUnderbond(CusUnderbond underbond, ZString originPremiseID, ZString destinationPresmiseID, bool isMoveFromDischarge)
		{
			underbond.C4_OriginPremiseID = originPremiseID;
			underbond.C4_DestinationPremiseID = destinationPresmiseID;
			underbond.C4_IsMoveFromDischarge = isMoveFromDischarge;
			underbond.C4_ModeOfMovement = Core.Constants.TransportModes.Road;
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			underbond.RunPreSaveValidation();
			Assert("Precondition: Underbond should not have any message errors "
				+ underbond.Notifications.GetMessageErrors().ToUniqueMessageListString()
				+ underbond.Notifications.GetErrors().ToUniqueMessageListString(),
								!underbond.Notifications.HasMessageErrors() && !underbond.Notifications.HasErrors());
		}

		#endregion
	}
}
