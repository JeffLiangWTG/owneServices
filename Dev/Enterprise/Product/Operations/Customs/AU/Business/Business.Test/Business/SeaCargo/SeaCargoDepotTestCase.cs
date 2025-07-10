using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class SeaCargoDepotTestCase : SeaCargoTestCase
	{
		#region Implementation

		protected const string TestPremiseCode = "T123P";

		protected override Type ConsolType
		{
			get { return typeof(CFSLoadListConsol); }
		}

		protected CFSLoadListConsol GetImportFCLConsol()
		{
			CFSLoadListConsol result = GetConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;
			return result;
		}

		protected CFSLoadListConsol GetImportLCLConsol()
		{
			return GetImportLCLConsol(typeof(CFSLoadListConsol));
		}

		protected CFSLoadListConsol GetImportLCLConsol(Type consolType)
		{
			CFSLoadListConsol result = GetConsol(consolType);
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.LCL;
			return result;
		}

		protected CFSLoadListConsol GetImportBBKConsol()
		{
			CFSLoadListConsol result = GetConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			return result;
		}

		protected CFSLoadListConsol GetImportBLKConsol()
		{
			CFSLoadListConsol result = GetConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			return result;
		}

		protected CFSLoadListConsol GetImportLQDConsol()
		{
			CFSLoadListConsol result = GetConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Liquid;
			return result;
		}

		protected CFSLoadListConsol GetImportGRPConsol()
		{
			CFSLoadListConsol result = GetConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Groupage;
			return result;
		}

		protected CFSLoadListConsol GetImportBuyersConsol()
		{
			CFSLoadListConsol result = GetConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			return result;
		}

		protected CFSLoadListConsol GetConsol()
		{
			return GetConsol(typeof(CFSLoadListConsol));
		}

		protected CFSLoadListConsol GetConsol(Type consolType)
		{
			CFSLoadListConsol result = (CFSLoadListConsol)Factory.New(consolType);
			result.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			Transport transport = result.Transports[0];
			transport.JW_JX = CreateImportSailing();

			return result;
		}

		protected ZString testVoyageNumber = "13";
		protected ZGuid CreateImportSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;
			voyage.JV_VoyageFlight = testVoyageNumber;
			voyage.Origins.AddNew();
			voyage.Origins[0].JA_RL_NKPortOfLoading = "SGSIN";
			voyage.Origins[0].JA_E_DEP = ZDateTime.Today.AddDays(-14);
			voyage.Destinations.AddNew();
			voyage.Destinations[0].JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Destinations[0].JB_E_ARV = ZDateTime.Today.AddDays(-1);
			voyage.GenerateSailings();
			return voyage.Sailings[0].PK;
		}

		protected OrgHeader GetForwarder()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "Test Depot Client - Forwarder";
			result.OH_IsForwarder = true;
			result.MainAddress.OA_Address1 = "Depots Mailing Address";
			result.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			result.LocalManifestID = "C029382919";
			return result;
		}

		protected ZQuery SimulateAFindBoxQueryForCusSCAHouse
		{
			get
			{
				ZQuery result = new ZQuery(CusSCADepotHouseSchema.CX_JS, ZGuid.Empty);
				result.AddToFilter(new ZQuery(CusSCADepotHouseSchema.CX_JS, DBNull.Value), JoinCondition.Or);
				return result;
			}
		}

		protected ZQuery SimulateAFindBoxQueryForCusSCAContainer
		{
			get
			{
				ZQuery result = new ZQuery(CusSCADepotContainerSchema.CJ_JC, ZGuid.Empty);
				result.AddToFilter(new ZQuery(CusSCADepotContainerSchema.CJ_JC, DBNull.Value), JoinCondition.Or);
				return result;
			}
		}

		protected CusSCADepotHouse CreateCusSCADepotHouse(ZString messageType, bool createShipment)
		{
			if (createShipment)
			{
				return CreateCusSCADepotHouse(messageType, Factory.New(typeof(CFSShipment)).PK);
			}
			else
			{
				return CreateCusSCADepotHouse(messageType, ZGuid.Empty);
			}
		}

		protected CusSCADepotHouse CreateCusSCADepotHouse(ZString messageType, ZGuid shipmentPK)
		{
			CusSCADepotHouse result = Factory.New<CusSCADepotHouse>();
			result.CX_Status = messageType;
			result.CX_JS = shipmentPK;
			return result;
		}

		public CusSCADepotContainer CreateCusSCADepotContainer(ZString messageType, bool createContainer)
		{
			if (createContainer)
			{
				return CreateCusSCADepotContainer(messageType, Factory.New(typeof(CFSContainer)).PK);
			}
			else
			{
				return CreateCusSCADepotContainer(messageType, ZGuid.Empty);
			}
		}

		public CusSCADepotContainer CreateCusSCADepotContainer(ZString messageType, ZGuid containerPK)
		{
			CusSCADepotContainer result = Factory.New<CusSCADepotContainer>();
			result.CJ_Status = messageType;
			result.CJ_JC = containerPK;
			return result;
		}

		protected JobSailing CreateSailing(ZString lloydsNumber, ZString voyageNumber)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber));
			if (vessel == null)
			{
				vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, lloydsNumber));
				if (vessel == null)
				{
					vessel = Factory.New<RefVessel>();
					vessel.RV_Name = lloydsNumber;
					vessel.RV_LloydsNumber = lloydsNumber;
				}
			}
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageNumber;
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "SGSIN";
			destination.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			destination.JB_E_ARV = ZDateTime.Today;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		#region Message Processing

		protected void ProcessIncomingUnderbond(CMRUBMREQRMessage message)
		{
			LoggingInformation logger = new LoggingInformation();
			new UBMREQRMessageProcessor(logger).ProcessMessage(message);
		}

		protected void ProcessIncomingCargoStatus(CMRCARSTMessage message)
		{
			LoggingInformation logger = new LoggingInformation();
			new CARSTMessageProcessor(logger).ProcessMessage(message);
		}

		#endregion

		#endregion
	}
}
