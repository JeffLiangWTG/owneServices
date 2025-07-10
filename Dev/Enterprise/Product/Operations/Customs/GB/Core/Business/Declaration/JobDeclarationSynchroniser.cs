using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class JobDeclarationSynchroniser : EU.Business.Declaration.JobDeclarationSynchroniser
	{
		// NB see also CusHAWB.SynchroniseToDeclaration for the same operation in standalone, without the locking of fields.

		public JobDeclarationSynchroniser(BaseJobDeclaration destination)
			: base(destination)
		{
			declaration = (JobDeclaration)destination;
		}

		// To consider for the future - synch truck ID from consol
		//protected override  IZType GetVesselFromTransportLeg(Freight.Business.Transport leg)
		//{
		//	return leg != null && (leg.JW_TransportMode == Core.Constants.TransportModes.Sea || leg.JW_TransportMode == Core.Constants.TransportModes.Road) ? leg.JW_Vessel : ZString.Empty;
		//}

		CusHAWB SourceHawb
		{
			get
			{
				if (sourceHawb == null && !hasLookedForSourceHawb)
				{
					declaration.DetermineHawbForSynching(Source, out sourceHawb);
					if (sourceHawb != null)
					{
						sourceMawb = sourceHawb.MAWB;
					}
					hasLookedForSourceHawb = true;
				}
				return sourceHawb;
			}
		}

		protected override ZPropertyInfo[] GetDateOfArrivalRelatedInfos()
		{
			var infos = base.GetDateOfArrivalRelatedInfos().ToList();
			if (SourceHawb != null && sourceMawb != null && !sourceMawb.CM_ArrivalDate.IsEmpty)
			{
				infos.Add(sourceMawb.CM_ArrivalDateInfo);
			}
			return infos.ToArray();
		}

		protected override IZType GetDateOfArrival()
		{
			IZType result = base.GetDateOfArrival();
			if (SourceHawb != null && sourceMawb != null)
			{
				result = sourceMawb.CM_ArrivalDate.IsEmpty ? result : sourceMawb.CM_ArrivalDate;
			}
			return result;
		}

		protected override void AddPacksSynchroniser()
		{
			// This funny syntax is needed because you can't just link CS_PiecesExpectedInfo (ZShort) and JE_TotalNumberOfPacksInfo (ZInt) - the assignment explodes.
			if (SourceHawb != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalNoOfPacksInfo,
															delegate
															{ return SourceHawb.CS_PiecesManifested.ToZInt(); },
															() => new ZPropertyInfo[] { SourceHawb.CS_PiecesManifestedInfo })
								 );

				var packTypeSyncroniser = new FieldSynchroniser(Destination.JE_TotalNoOfPacksPackTypeInfo, Source.JS_F3_NKPackTypeInfo);
				packTypeSyncroniser.Format += PackTypeSyncroniser_Format;
				Synchronisers.Add(packTypeSyncroniser);
			}
			else
			{
				base.AddPacksSynchroniser();
			}
		}

		protected override void HookWeight()
		{
			if (SourceHawb != null)
			{
				Synchronisers.Add(new FieldSynchroniser(declaration.JE_TotalWeightInfo, SourceHawb.CS_WeightInfo));
				Synchronisers.Add(new FieldSynchroniser(declaration.JE_TotalWeightUnitInfo, SourceHawb.CS_WeightUQInfo));
			}
			else
			{
				base.HookWeight();
			}
		}

		protected override void HookGoodsDescription()
		{
			if (SourceHawb != null)
			{
				Synchronisers.Add(new FieldSynchroniser(declaration.JE_GoodsDescriptionInfo, SourceHawb.CS_GoodsDescriptionInfo));
			}
			else
			{
				base.HookGoodsDescription();
			}
		}

		protected override void HookPortsOfLoadingAndArrival()
		{
			Integration.Customs.GB.CCSUK.ICcsukCusAwbBase iCusAwbBase = null;
			if (SourceHawb != null)
			{
				iCusAwbBase = SourceHawb.CS_IsMasterHouse && sourceMawb != null ? sourceMawb as Integration.Customs.GB.CCSUK.ICcsukCusAwbBase
														: SourceHawb as Integration.Customs.GB.CCSUK.ICcsukCusAwbBase;
			}
			if (iCusAwbBase != null)
			{
				RefUNLOCO portOfDestination = null;
				if (!iCusAwbBase.AirportOfDestination.IsEmpty)
				{
					portOfDestination = RefUNLOCO.LoadFromIATA(declaration.Factory, iCusAwbBase.AirportOfDestination);
				}
				Synchronisers.Add(new FieldSynchroniser(declaration.JE_RL_NKPortOfArrivalInfo,
														delegate
														{ return portOfDestination == null ? iCusAwbBase.AirportOfDestination : portOfDestination.RL_Code; },
														() => new ZPropertyInfo[] { iCusAwbBase.AirportOfDestinationInfo }));
				Synchronisers.Add(new FieldSynchroniser(declaration.JE_RL_NKPortOfLoadingInfo, iCusAwbBase.AirportOfOriginInfo));
			}
			else
			{
				base.HookPortsOfLoadingAndArrival();
			}
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!SyncChangesDetected)
			{
				var iCusAwbBase = SourceHawb as Integration.Customs.GB.CCSUK.ICcsukCusAwbBase;
				if (iCusAwbBase != null)
				{
					Synchronisers.Add(new FieldSynchroniser(declaration.JE_SubLocationOfGoodsInfo,
																delegate
																{ return new ZString(PortConverter.IataToChief(iCusAwbBase.CargoTerminalOperatorAirport, declaration.Factory).PadLeft(3) + iCusAwbBase.CargoTerminalOperator.PadRight(3)); },
																() => new ZPropertyInfo[] { SourceHawb.CS_WarehouseLocationInfo }));

					Synchronisers.Add(new FieldSynchroniser(declaration.JE_LocationOfGoodsInfo,
																delegate
																{ return new ZString(PortConverter.IataToChief(iCusAwbBase.CargoTerminalOperatorAirport, declaration.Factory).PadLeft(3)); },
																() => new ZPropertyInfo[] { SourceHawb.CS_WarehouseLocationInfo }));

					Synchronisers.Add(new FieldSynchroniser(declaration.JE_LocationOtherInformationInfo,
															delegate
															{ return new CcsUkToCdsLocationConverter(SourceHawb.CS_WarehouseLocation).CalculateCdsLocation(declaration.Factory); },
																	() => new ZPropertyInfo[] { SourceHawb.CS_WarehouseLocationInfo }));

					Synchronisers.Add(new FieldSynchroniser(declaration.JE_CustomsProfileInfo, iCusAwbBase.AgentBadgeInfo));
				}
			}
		}

		protected override ZString GetHouseBillOfSpecificShipment(IBillDetails shipment)
		{
			// We need to use hte HAWB's house bill number, not the Shipment's, because the may not match for CCSUK jobs.
			return
				(declaration != null && SourceHawb != null && declaration.Shipment != null && declaration.Shipment == SourceHawb.Shipment && declaration.Shipment == shipment)
				? SourceHawb.CS_HAWB
				: base.GetHouseBillOfSpecificShipment(shipment);
		}

		CusHAWB sourceHawb;
		CusMAWB sourceMawb;
		bool hasLookedForSourceHawb;
		readonly JobDeclaration declaration;
	}
}
