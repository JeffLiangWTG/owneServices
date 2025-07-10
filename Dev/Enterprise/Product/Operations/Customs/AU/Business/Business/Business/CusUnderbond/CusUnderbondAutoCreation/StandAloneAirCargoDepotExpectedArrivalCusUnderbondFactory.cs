using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory : ExpectedArrivalCusUnderbondFactory
	{
		protected internal ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParentInternal(CMRUBMREQRMessage message, int lineNumber) => LoadOrCreateUnderbondParent(message, lineNumber);
		protected override ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParent(CMRUBMREQRMessage message, int lineNumber)
		{
			var mawbNumber = message.GetMAWB(lineNumber);
			var hawbNumber = message.GetHAWB(lineNumber);

			if (hawbNumber.IsEmpty)
			{
				LoadOrCreateUnderbondParentForMasterUnderbond(message, lineNumber, mawbNumber);
				return LoadOrCreateUnderbondParentForConsignmentUnderbond(message, lineNumber, mawbNumber, hawbNumber);
			}
			else
			{
				return LoadOrCreateUnderbondParentForConsignmentUnderbond(message, lineNumber, mawbNumber, hawbNumber);
			}
		}

		CusUnderbond FindUnderbond(CMRUBMREQRMessage message, ZString mAWBNumber, ZString hAWBNumber)
		{
			CusUnderbond result = null;

			var underbondFilter = new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, message.UBMSendersReference);
			underbondFilter.AddToFilter(CusUnderbondSchema.C4_ApplicationCode, Customs.Business.CusUnderbondApplicationCodeList.Codes.AUUnderbond);

			var dateEmptyOrMatches = new ZQuery();
			dateEmptyOrMatches.AddToFilter(CusUnderbondSchema.C4_ArrivalDate, ZDateTime.Empty);
			dateEmptyOrMatches.AddToFilter(JoinCondition.Or, CusUnderbondSchema.C4_ArrivalDate, message.ArrivalDate);
			underbondFilter.AddToFilter(dateEmptyOrMatches);

			var foundUnderbond = message.Factory.LoadTop1<CusUnderbond>(underbondFilter);
			if (foundUnderbond != null)
			{
				// For a stand-alone underbond, they won't be linked to a MAWB/HAWB.
				if (foundUnderbond.C4_MAWB == mAWBNumber)
				{
					result = foundUnderbond;
				}
				else if (foundUnderbond.MAWB != null)
				{
					if (foundUnderbond.MAWB.CM_FlightNo == message.FlightNumber && foundUnderbond.MAWB.CM_MAWB == mAWBNumber)
					{
						if (foundUnderbond.HAWBLinked != null && !hAWBNumber.IsEmpty)
						{
							if (foundUnderbond.HAWBLinked.CS_HAWB == hAWBNumber)
							{
								result = foundUnderbond;
							}
						}
						else
						{
							result = foundUnderbond;
						}
					}
				}
			}

			return result;
		}

		protected internal bool IsInterestedInUBMREQRInternal(CMRUBMREQRMessage message) => IsInterestedInUBMREQR(message);
		protected override bool IsInterestedInUBMREQR(CMRUBMREQRMessage message)
		{
			return message.IsAir && (UnderbondForDepot(message) || AirForwardingRecordExists(message));
		}

		protected bool AirForwardingRecordExists(CMRUBMREQRMessage message)
		{
			var mawb = message.GetMAWB(1);
			return !mawb.IsEmpty && new CusMAWBBase.Loader(message.Factory).FindFirstMatchingForwardedMAWB(mawb) != null;
		}

		#region Implementation

		ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParentForConsignmentUnderbond(CMRUBMREQRMessage message, int lineNumber, ZString mawbNumber, ZString hawbNumber)
		{
			var underbond = FindUnderbond(message, mawbNumber, hawbNumber);
			if (underbond != null)
			{
				if (!message.ArrivalDate.IsEmpty)
				{
					underbond.C4_ArrivalDate = message.ArrivalDate;
				}

				if (!message.FlightNumber.IsEmpty)
				{
					underbond.C4_FlightNo = message.FlightNumber;
				}

				if (!message.DestinationID.IsEmpty)
				{
					underbond.C4_DestinationPremiseID = message.DestinationID;
				}

				var numberOfPackages = (short)message.GetNumberOfPackages(lineNumber);
				if (numberOfPackages > 0)
				{
					underbond.C4_PiecesManifested = numberOfPackages;
				}

				if (!message.OriginID.IsEmpty)
				{
					underbond.C4_OriginPremiseID = message.OriginID;
				}

				if (!message.DestinationPort.IsEmpty)
				{
					underbond.C4_RL_NKDischargePort = message.DestinationPort;
				}
			}
			else if (!mawbNumber.IsEmpty)
			{
				var uBMCreator = new AirCargoUnderbondLoaderOrCreator(message.Factory);
				underbond = uBMCreator.CreateRecord(mawbNumber, hawbNumber, message.FlightNumber, message.ArrivalDate, message.DestinationPort, (short)message.GetNumberOfPackages(lineNumber), message.RequestReason, message.ModeOfMovement, message.OriginID, message.DestinationID, true);

				if (underbond != null && underbond.C4_ParentID.IsEmpty && !hawbNumber.IsEmpty)
				{
					var outturnFilter = new ZQuery(CusOutturnSchema.C5_C4_Underbond, underbond.PK);
					outturnFilter.AddToFilter(CusOutturnSchema.C5_HouseBill, hawbNumber);
					outturnFilter.AddToFilter(CusOutturnSchema.C5_ApplicationCode, Customs.Business.CusOutturnApplicationCodeList.Codes.CMR);

					var outturn = message.Factory.LoadTop1<CusOutturn>(outturnFilter);
					if (outturn == null)
					{
						outturn = underbond.Outturns.AddNew();
						outturn.C5_HouseBill = hawbNumber;
					}

					var numberOfPackages = (short)message.GetNumberOfPackages(lineNumber);
					if (numberOfPackages > 0)
					{
						outturn.C5_PackagesOutturned = numberOfPackages;
					}

					message.LinkOrCloneMessage(outturn);
					return null;
				}
			}

			if (underbond != null)
			{
				message.LinkOrCloneMessage(underbond);
				if (!hawbNumber.IsEmpty)
				{
					UpdateAnySuitableNestedCusMAWBs(message, lineNumber, underbond, hawbNumber);
				}
			}

			return underbond;
		}

		void LoadOrCreateUnderbondParentForMasterUnderbond(CMRUBMREQRMessage message, int lineNumber, ZString mawbNumber)
		{
			var mawbs = new CusMAWBBase.Loader(message.Factory).FindMatchingMAWBs(mawbNumber, ZGuid.Empty, false, excludeOld: true);
			foreach (CusMAWB mawb in mawbs)
			{
				UpdateAndLinkToMasterUnderbond(message, lineNumber, mawb);
			}
		}

		void UpdateAndLinkToMasterUnderbond(CMRUBMREQRMessage message, int lineNumber, CusMAWB mawb)
		{
			CusUnderbond underbond = null;
			foreach (CusUnderbond underbondForUpdate in mawb.AllUnderbonds)
			{
				if (underbondForUpdate.C4_DestinationPremiseID == message.DestinationID &&
					underbondForUpdate.C4_OriginPremiseID == message.OriginID &&
					(underbondForUpdate.C4_FlightNo.IsEmpty || underbondForUpdate.C4_FlightNo == message.FlightNumber) &&
					(underbondForUpdate.C4_ArrivalDate.IsEmpty || underbondForUpdate.C4_ArrivalDate == message.ArrivalDate))
				{
					underbond = underbondForUpdate;
					break;
				}
			}

			if (underbond == null)
			{
				underbond = mawb.Underbonds.AddNew();
				mawb.AllUnderbonds.Load();
			}

			if (!message.FlightNumber.IsEmpty)
			{
				underbond.C4_FlightNo = message.FlightNumber;
			}

			if (!message.ArrivalDate.IsEmpty)
			{
				underbond.C4_ArrivalDate = message.ArrivalDate;
			}

			if (!message.DestinationPort.IsEmpty)
			{
				underbond.C4_RL_NKDischargePort = message.DestinationPort;
			}

			if (message.GetNumberOfPackages(lineNumber) > 0)
			{
				underbond.C4_PiecesManifested = message.GetNumberOfPackages(lineNumber);
			}

			if (!message.RequestReason.IsEmpty)
			{
				underbond.C4_MovementReason = message.RequestReason;
			}

			if (!message.ModeOfMovement.IsEmpty)
			{
				underbond.C4_ModeOfMovement = message.ModeOfMovement;
			}

			if (!message.OriginID.IsEmpty)
			{
				underbond.C4_OriginPremiseID = message.OriginID;
			}

			if (!message.DestinationID.IsEmpty)
			{
				underbond.C4_DestinationPremiseID = message.DestinationID;
			}

			if (underbond.C4_DestinationPremiseID.IsEmpty)
			{
				underbond.C4_DestinationPremiseID = GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID;
			}

			if (underbond.C4_MovementReason.IsEmpty)
			{
				underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			}

			if (underbond.C4_ModeOfMovement.IsEmpty)
			{
				underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Road;
			}

			message.LinkOrCloneMessage(underbond);
		}

		void UpdateAnySuitableNestedCusMAWBs(CMRUBMREQRMessage message, int lineNumber, CusUnderbond underbond, ZString hawbNumber)
		{
			var cusMAWB = underbond.LinkedObject as CusMAWB;
			if (cusMAWB != null && cusMAWB.CM_MasterHouseBill == hawbNumber)
			{
				var query = new ZDBOnlyQuery(typeof(CusMAWB));
				query.AddToFilter(new CusMAWBBase.Loader(message.Factory).FindMatchingMAWBsFilter(cusMAWB.CM_MAWB, consolPK: ZGuid.Empty, reloadExistingRows: false, excludeOld: true));
				query.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, SQLComparisonOperator.NotEqual, hawbNumber);
				var queryText = string.Format("{0} IN (SELECT {1} FROM {2} WHERE {3} = @CUSMAWBPK)", CusMAWB.Schema.CM_MasterHouseBill, CusHAWB.Schema.CS_HAWB, CusHAWB.Schema.TableName, CusHAWB.Schema.CS_CM);
				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add(ZSqlParameter.New("@CUSMAWBPK", cusMAWB.PK, CusMAWBSchema.PK));
				query.AddFilterAndZSQLParameterCollection(queryText, sqlParams, JoinCondition.And);
				foreach (CusMAWB nestedCusMAWB in message.Factory.Load<CusMAWB>(query))
				{
					UpdateAndLinkToMasterUnderbond(message, lineNumber, nestedCusMAWB);
				}
			}
		}

		#endregion // Implementation
	}
}
