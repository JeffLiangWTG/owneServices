using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSRecordLoaderAndCreator : BaseRecordLoaderAndCreator
	{
		#region Constructors

		public CFSRecordLoaderAndCreator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region GetOrCreateCFSRecord

		public ICusUnderbondDependentCollectionParent GetOrCreateCFSRecord(ICMRDepotMessageLine line)
		{
			CARSTRecord dontCare;
			return GetOrCreateCFSRecord(line, out dontCare);
		}

		public ICusUnderbondDependentCollectionParent GetOrCreateCFSRecord(ICMRDepotMessageLine line, out CARSTRecord carstRecord)
		{
			carstRecord = null;

			ICusUnderbondDependentCollectionParent result = null;

			SetOrgProxyFromPremiseID(line.Parent.OurPremiseID);

			CFSContainer container;
			CFSShipment shipment = FindShipment(line, out container);
			CFSLoadListConsol consol = null;

			if (shipment == null)
			{
				result = FindFreeStandingCusSCAHousePivot(line);
			}

			if (result == null)
			{
				if (container == null)
				{
					container = FindContainer(line);
				}

				if (shipment != null)
				{
					foreach (CFSLoadListConsol aConsol in shipment.Consols)
					{
						if (DoesConsolHaveTransportMatching(aConsol, line))
						{
							consol = aConsol;
							break;
						}
					}
				}
				else if (container != null)
				{
					consol = container.Consol;
				}
				else
				{
					consol = FindConsol(line);
					if (consol != null && consol.Shipments.Count > 0 && line.HouseBillNumber.IsEmpty)// This is for directs (i.e. no house bill)
					{
						shipment = consol.Shipments[0];
					}
				}

				if (consol != null && !line.HouseBillNumber.IsEmpty && line.Parent.MessageType == CMRDepotMessageType.Status)
				{
					if (shipment == null && AUCustomsDataRegistry.Instance.CreateCFSShipmentWhenCustomsStatusReceivedForUnknownShipment.Value)
					{
						shipment = consol.Shipments.AddNew();
						shipment.ConsigneePK = GetOrCreateUnknownConsignee().PK;
					}

					if (shipment != null)
					{
						if (shipment.JS_OuterPacks == 0)
						{
							shipment.JS_OuterPacks = line.NumberOfPackages;
						}

						if (shipment.JS_OuterPacks.IsEmpty)
						{
							shipment.JS_F3_NKPackType = SeaCargoUtilities.ConvertCMRPackageTypeToPkgUnit(line.PackageType);
						}

						if (shipment.JS_PackingMode.IsEmpty)
						{
							shipment.JS_PackingMode = line.ContainerMode;
						}

						if (shipment.JS_HouseBill.IsEmpty)
						{
							shipment.JS_HouseBill = line.HouseBillNumber.Left(shipment.JS_HouseBillInfo.MaxLength);
						}

						if (shipment.JS_GoodsDescription.IsEmpty)
						{
							shipment.JS_GoodsDescription = line.GoodsDescription.Left(shipment.JS_GoodsDescriptionInfo.MaxLength);
						}

						if (shipment.JS_MarksAndNumbers.IsEmpty)
						{
							shipment.JS_MarksAndNumbers = line.MarksAndNumbers.Left(shipment.JS_MarksAndNumbersInfo.MaxLength);
						}

						if (shipment.JS_ActualWeight == 0 && line.ActualWeight != 0)
						{
							shipment.JS_ActualWeight = line.ActualWeight;
							shipment.JS_UnitOfWeight = line.WeightUnits;
						}
						if (shipment.JS_ActualVolume == 0 && line.ActualVolume != 0)
						{
							shipment.JS_ActualVolume = line.ActualVolume;
							shipment.JS_UnitOfVolume = line.VolumeUnits;
						}
					}
				}

				if (shipment != null && line.ContainerMode != CMRImportCargoTypes.Codes.FullContainerLoad && line.ContainerMode != CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills)
				{
					if (shipment.OuterPackLines.Count == 0 && !shipment.IsMasterShipmentRepresentingAllChildShipments)
					{
						shipment.OuterPackLines.AddNew();
						shipment.OuterPackLines[0].JL_PackageCount = line.NumberOfPackages;
						shipment.OuterPackLines[0].JL_F3_NKPackType = SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(line.PackageType);
					}
					if (container != null && consol != null && shipment.OuterPackLines.Count == 1)
					{
						if (shipment.OuterPackLines[0].GetContainer(consol) != container)
						{
							if (consol.Containers.Contains(container))
							{
								shipment.OuterPackLines[0].SetContainer(consol, container);
							}
							else
							{
								carstRecord = new CARSTRecord()
								{
									Shipment = shipment,
									Consol = consol,
									Container = container,
									Line = line
								};
							}
						}
					}
					result = CFSShipmentWrapper.Load(shipment);
				}
				else if (container != null &&
					(line.HouseBillNumber.IsEmpty
					|| line.ContainerMode == CMRImportCargoTypes.Codes.FullContainerLoad
					|| line.ContainerMode == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills))
				{
					result = CFSContainerWrapper.Load(container);
				}
			}

			//CreateDepotPlaceHolder
			if (result == null)
			{
				result = CreateCusSCADepotHolder(line);
			}

			return result;
		}

		#endregion

		#region Implementation

		#region Create CusSCADepot* Holder

		protected internal ICusUnderbondDependentCollectionParent CreateCusSCADepotHolder(ICMRDepotMessageLine line)
		{
			ICusUnderbondDependentCollectionParent result = null;

			if (!line.ContainerNumber.IsEmpty)
			{
				CusSCADepotContainer container = LoadOrCreateCusSCADepotContainer(line);
				result = container;

				if (!line.HouseBillNumber.IsEmpty)
				{
					result = LoadOrCreateCusSCADepotHouseFromContainer(line, container);
				}
			}
			else
			{
				result = LoadOrCreateCusSCADepotHouse(line);
			}

			return result;
		}

		#region LoadOrCreateCusSCADepotContainer

		protected CusSCADepotContainer LoadOrCreateCusSCADepotContainer(ICMRDepotMessageLine line)
		{
			var container = CusSCADepotContainer.Load(Factory, line.ContainerNumber, line.Parent.VoyageNumber, CMRMessage.CMRMessageTypes.UBMREQR, line.Parent.LloydsNumber);

			if (container == null)
			{
				container = Factory.New<CusSCADepotContainer>();
				container.CJ_Status = CMRMessage.CMRMessageTypes.UBMREQR;
				container.CJ_ContainerNumber = line.ContainerNumber;
				container.CJ_LloydsNumber = line.Parent.LloydsNumber;
				container.CJ_Voyage = line.Parent.VoyageNumber;
				container.CJ_PackageCount = line.NumberOfPackages;
			}

			return container;
		}

		#endregion

		#region LoadOrCreateCusSCADepotHouseFromContainer

		protected CusSCADepotHouse LoadOrCreateCusSCADepotHouseFromContainer(ICMRDepotMessageLine line, CusSCADepotContainer container)
		{
			CusSCADepotHouse house = container.HouseBills.Find(line.HouseBillNumber);

			if (house == null)
			{
				house = container.HouseBills.AddNew();
				house.CX_Status = CMRMessage.CMRMessageTypes.UBMREQR;
				house.CX_HouseBill = line.HouseBillNumber;
				house.CX_PackageCount = line.NumberOfPackages;
			}

			return house;
		}

		#endregion

		#region LoadOrCreateCusSCADepotHouse

		protected CusSCADepotHouse LoadOrCreateCusSCADepotHouse(ICMRDepotMessageLine line)
		{
			var house = CusSCADepotHouse.Load(Factory, line.HouseBillNumber, line.OceanBillNumber, CMRMessage.CMRMessageTypes.UBMREQR, line.Parent.LloydsNumber, line.Parent.VoyageNumber);

			if (house == null)
			{
				var container = Factory.New<CusSCADepotContainer>();
				container.CJ_LloydsNumber = line.Parent.LloydsNumber;
				container.CJ_Voyage = line.Parent.VoyageNumber;
				house = container.HouseBills.AddNew();

				switch (line.Parent.MessageType)
				{
					case CMRDepotMessageType.Status:
						house.CX_Status = CMRMessage.CMRMessageTypes.CARST;
						break;
					default:
					case CMRDepotMessageType.ExpectedArrival:
						house.CX_Status = CMRMessage.CMRMessageTypes.UBMREQR;
						break;
				}

				house.CX_HouseBill = line.HouseBillNumber.IsEmpty ? line.OceanBillNumber : line.HouseBillNumber;
				house.CX_PackageCount = line.NumberOfPackages;
			}

			return house;
		}

		#endregion

		#endregion

		#region Finding Records

		internal CusSCAPivot FindFreeStandingCusSCAHousePivot(ICMRDepotMessageLine line)
		{
			if (!line.ContainerNumber.IsEmpty && !line.HouseBillNumber.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(CusSCAPivot));
				var containerSubQuery = new ZDBOnlySubQuery(typeof(CusSCAContainer), CusSCAPivotSchema.CV_CN);
				containerSubQuery.AddToFilter(CusSCAContainerSchema.CN_ContainerNumber, line.ContainerNumber);
				query.AddSubQuery(containerSubQuery, JoinCondition.And);
				var houseSubQuery = new ZDBOnlySubQuery(typeof(CusSCAHouse), CusSCAPivotSchema.CV_CA);
				houseSubQuery.AddToFilter(CusSCAHouseSchema.CA_HouseBill, line.HouseBillNumber);
				var oceanBillSubQuery = new ZDBOnlySubQuery(typeof(CusSCAOceanBill), CusSCAOceanBillSchema.PK);
				oceanBillSubQuery.AddToFilter(CusSCAOceanBillSchema.CB_ParentId, null);
				oceanBillSubQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
				oceanBillSubQuery.AddToFilter(CusSCAOceanBillSchema.CB_LloydsIMO, line.Parent.LloydsNumber);
				oceanBillSubQuery.AddToFilter(CusSCAOceanBillSchema.CB_Voyage, line.Parent.VoyageNumber);
				houseSubQuery.AddSubQuery(CusSCAHouseSchema.CA_CB, oceanBillSubQuery, JoinCondition.And);
				query.AddSubQuery(houseSubQuery, JoinCondition.And);
				return Factory.LoadTop1<CusSCAPivot>(query);
			}
			return null;
		}

		protected CFSContainer FindContainer(ICMRDepotMessageLine line)
		{
			CFSContainer result = null;
			if (!line.ContainerNumber.IsEmpty)
			{
				CFSContainer firstContainerForVesselVoyage = null;
				CFSContainer firstContainerForVesselVoyageWithLine = null;
				ZQuery filter = new ZQuery(JobContainerSchema.JC_ContainerNum, line.ContainerNumber);
				filter.AddToFilter(JobContainerSchema.JC_IsCFSRegistered, true);
				CFSContainer[] containers = (CFSContainer[])Factory.Load(typeof(CFSContainer), filter);
				foreach (CFSContainer container in containers)
				{
					var consol = container.Consol;
					if (consol != null)
					{
						var consolContainsThisLine = line.HouseBillNumber.IsEmpty || consol.Shipments.Cast<CFSShipment>().Any(x => x.JS_HouseBill == line.HouseBillNumber);
						if (DoesConsolHaveTransportMatching(consol, line))
						{
							if (consolContainsThisLine && line.OceanBillNumber == consol.JK_MasterBillNum)
							{
								result = container;
								break;
							}
							else if (firstContainerForVesselVoyageWithLine == null && consolContainsThisLine)
							{
								firstContainerForVesselVoyageWithLine = container;
							}
							else if (firstContainerForVesselVoyage == null || line.OceanBillNumber == consol.JK_MasterBillNum)
							{
								firstContainerForVesselVoyage = container;
							}
						}
					}
				}

				if (result == null)
				{
					result = firstContainerForVesselVoyageWithLine;
				}

				if (result == null)
				{
					result = firstContainerForVesselVoyage;
				}
			}
			return result;
		}

		static bool DoesConsolHaveTransportMatching(CFSLoadListConsol consol, ICMRDepotMessageLine line)
		{
			return consol.Transports.Cast<Transport>().Any(x =>
				x.Vessel != null &&
				x.Vessel.RV_LloydsNumber == line.Parent.LloydsNumber &&
				x.JW_VoyageFlight == line.Parent.VoyageNumber);
		}

		protected CFSLoadListConsol FindConsol(ICMRDepotMessageLine line)
		{
			CFSLoadListConsol result = null;
			if (!line.OceanBillNumber.IsEmpty)
			{
				CFSLoadListConsol firstConsolForVesselVoyage = null;
				ZQuery filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, line.OceanBillNumber);
				filter.AddToFilter(JobConsolSchema.JK_IsCFS, true);
				CFSLoadListConsol[] consols = (CFSLoadListConsol[])Factory.Load(typeof(CFSLoadListConsol), filter);

				foreach (CFSLoadListConsol consol in consols)
				{
					var consolContainsThisLine = line.HouseBillNumber.IsEmpty || consol.Shipments.Any(x => ((CFSShipment)x).JS_HouseBill == line.HouseBillNumber);

					if (DoesConsolHaveTransportMatching(consol, line))
					{
						if (consolContainsThisLine)
						{
							result = consol;
							break;
						}
						else if (firstConsolForVesselVoyage == null)
						{
							firstConsolForVesselVoyage = consol;
						}
					}
				}
				if (result == null)
				{
					result = firstConsolForVesselVoyage;
				}
			}
			return result;
		}

		protected internal CFSShipment FindShipment(ICMRDepotMessageLine line, out CFSContainer container1)
		{
			var possibleResults = new Dictionary<CFSShipment, CFSContainer>();
			if (!line.HouseBillNumber.IsEmpty)
			{
				ZQuery filter = new ZQuery(JobShipmentSchema.JS_HouseBill, line.HouseBillNumber);
				filter.AddToFilter(JobShipmentSchema.JS_IsCFSRegistered, true);
				CFSShipment[] shipments = (CFSShipment[])Factory.Load(typeof(CFSShipment), filter);
				foreach (CFSShipment shipment in shipments)
				{
					foreach (CFSLoadListConsol consol in shipment.Consols)
					{
						if (DoesConsolHaveTransportMatching(consol, line))
						{
							var container = (CFSContainer)consol.Containers.FirstOrDefault(x => ((CFSContainer)x).JC_ContainerNum == line.ContainerNumber);
							if (!possibleResults.ContainsKey(shipment))
							{
								possibleResults.Add(shipment, container);
							}
							break;
						}
					}
				}
			}

			var result = possibleResults.FirstOrDefault(x => x.Value != null);
			if (result.Key == null && possibleResults.Count > 0)
			{
				result = possibleResults.First();
			}

			container1 = result.Value;
			return result.Key;
		}

		protected OrgHeader GetOrCreateUnknownConsignee()
		{
			ZQuery unknownConsigneeFilter = new ZQuery(OrgHeaderSchema.OH_FullName, "Unknown Consignee");
			var result = Factory.LoadTop1<OrgHeader>(unknownConsigneeFilter);
			if (result == null)
			{
				result = Factory.New<OrgHeader>();
				result.OH_FullName = "Unknown Consignee";
				result.OH_RL_NKClosestPort = ProxyBranch.GB_RL_NKHomePort;
				result.MainAddress.OA_Address1 = "*** Not on file ***";
				result.MainAddress.OA_City = "Unknown";
				result.OH_IsConsignee = true;
			}
			return result;
		}

		#endregion

		#region EnterpriseModeFromCustomsMode

		protected ZString EnterpriseModeFromCustomsMode(ZString customsMode)
		{
			ZString result = customsMode;
			if (customsMode == CMRCargoTypes.Codes.BreakBulk)
			{
				result = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			}

			return result;
		}

		#endregion

		#endregion
	}
}
