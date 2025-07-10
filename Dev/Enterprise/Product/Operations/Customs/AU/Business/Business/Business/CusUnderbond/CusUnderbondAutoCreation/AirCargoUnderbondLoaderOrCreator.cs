
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCargoUnderbondLoaderOrCreator
	{
		public AirCargoUnderbondLoaderOrCreator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public CusUnderbond CreateRecord(ZString parentID, ZString hAWBNumber, ZString flightNumber, ZDateTime arrivalDate, ZString dischargePort, short piecesManifested, ZString movementReason, ZString modeOfMovement, ZString originPremiseID, ZString destinationPremiseID, bool createStandAlone)
		{
			CusUnderbond underbond = null;

			// If we don't have a HAWB number, find an underbond that matches our MAWB/Flight/Arrival Date
			if (hAWBNumber.IsEmpty)
			{
				CusUnderbond[] underbondsByMAWB = (CusUnderbond[])factory.Load(typeof(CusUnderbond), new ZQuery(CusUnderbondSchema.C4_MAWB, parentID));
				if (underbondsByMAWB.Length > 0)
				{
					underbond = FindPartUnderbondByFlightNumber(underbondsByMAWB, flightNumber, arrivalDate);
				}
			}

			if (underbond == null)
			{
				// Load the MAWB for our MAWB number
				CusMAWB mAWB;
				var mawbs = new CusMAWBBase.Loader(factory).FindMatchingMAWBs(parentID, ZGuid.Empty, false, excludeOld: true).Cast<CusMAWB>();
				mAWB = mawbs.FirstOrDefault(x => x.CM_MasterHouseBill == hAWBNumber);
				if (mAWB == null)
				{
					mAWB = mawbs.FirstOrDefault();
				}

				ICusUnderbondUnionCollectionParent parent = mAWB;

				if (mAWB != null)
				{
					// If we have a HAWB number, load the HAWB and use it instead of our MAWB
					if (!hAWBNumber.IsEmpty)
					{
						CusHAWB hAWB = (CusHAWB)CusHAWBBase.LoadFromQuery(new ZQuery(CusHAWBSchema.CS_HAWB, hAWBNumber), factory);
						if (hAWB != null)
						{
							parent = hAWB;
						}
					}

					// Try to find the right Underbond based on Origin/Destination/Flight/Arrival Date
					foreach (CusUnderbond underbondForUpdate in parent.AllUnderbonds)
					{
						if (underbondForUpdate.C4_DestinationPremiseID == destinationPremiseID && underbondForUpdate.C4_OriginPremiseID == originPremiseID &&
							(underbondForUpdate.C4_FlightNo.IsEmpty || underbondForUpdate.C4_FlightNo == flightNumber) &&
							(underbondForUpdate.C4_ArrivalDate.IsEmpty || underbondForUpdate.C4_ArrivalDate == arrivalDate))
						{
							underbond = underbondForUpdate;
							break;
						}
					}

					// Create an Underbond if we haven't found one.
					if (underbond == null)
					{
						underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)parent).Underbonds.AddNew();
						parent.AllUnderbonds.Load();
					}
				}
				else if (createStandAlone)
				{
					// If we don't have a MAWB, create a stand-alone Underbond record.
					underbond = factory.New<CusUnderbond>();
					underbond.C4_MAWB = parentID;
				}
			}

			if (underbond != null)
			{
				if (!flightNumber.IsEmpty)
				{
					underbond.C4_FlightNo = flightNumber;
				}

				if (!arrivalDate.IsEmpty)
				{
					underbond.C4_ArrivalDate = arrivalDate;
				}

				if (!dischargePort.IsEmpty)
				{
					underbond.C4_RL_NKDischargePort = dischargePort;
				}

				if (piecesManifested > 0)
				{
					underbond.C4_PiecesManifested = piecesManifested;
				}

				if (!movementReason.IsEmpty)
				{
					underbond.C4_MovementReason = movementReason;
				}

				if (!modeOfMovement.IsEmpty)
				{
					underbond.C4_ModeOfMovement = modeOfMovement;
				}

				if (!originPremiseID.IsEmpty)
				{
					underbond.C4_OriginPremiseID = originPremiseID;
				}

				if (!destinationPremiseID.IsEmpty)
				{
					underbond.C4_DestinationPremiseID = destinationPremiseID;
				}

				if (underbond.C4_MovementReason.IsEmpty)
				{
					underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
				}

				if (underbond.C4_ModeOfMovement.IsEmpty)
				{
					underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Road;
				}
			}

			return underbond;
		}

		public CusUnderbond FindPartUnderbondByFlightNumber(CusUnderbond[] underbonds, ZString flightNumber, ZDateTime arrivalDate)
		{
			foreach (CusUnderbond currentUnderbond in underbonds)
			{
				if (currentUnderbond.C4_FlightNo == flightNumber)
				{
					if (!currentUnderbond.C4_ArrivalDate.IsEmpty && currentUnderbond.C4_ArrivalDate.ToShortDateString() == arrivalDate.ToShortDateString())
					{
						return currentUnderbond;
					}
					else if (currentUnderbond.C4_ArrivalDate.IsEmpty)
					{
						return currentUnderbond;
					}
				}
			}
			return null;
		}
	}
}
