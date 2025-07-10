using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCargoRecordLoaderAndCreator
	{
		public AirCargoRecordLoaderAndCreator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public BusinessObject[] LoadOrCreateRecords(ZString flightNumber, ZDateTime arrivalDate, ZString mawbNumber, ZString hawbNumber, ZShort numberOfPackages, ZString portOfDischarge, ZString goodsReceiptPlace, ZString sendersReference, ZString transhipmentNumber, CMRCUSRESMessage message)
		{
			var records = new List<BusinessObject>();
			if (!sendersReference.IsEmpty)
			{
				var dbOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWBBase));
				dbOnlyQuery.AddToFilter(CusHAWBSchema.CS_MessageReference, sendersReference);

				var hawbs = CusHAWBBase.LoadAllFromQuery(dbOnlyQuery, factory);
				if (hawbs.Length > 0)
				{
					if (hawbNumber.IsEmpty)
					{
						var hawb = GetRelevantHAWBForCTOs(hawbs, mawbNumber, transhipmentNumber, flightNumber, arrivalDate, portOfDischarge, goodsReceiptPlace) as BusinessObject;
						if (hawb != null)
						{
							records.Add(hawb);
							(hawb as ITransitWarehouseSyncEventParent)?.RegisterSyncEvent(mawbNumber, hawbNumber);
						}
					}
					else
					{
						var hawb = GetRelevantHAWBUsingOurSendersReference(hawbs, hawbNumber, numberOfPackages, transhipmentNumber, flightNumber, arrivalDate, mawbNumber, portOfDischarge, goodsReceiptPlace, message.EM_MessageNum) as BusinessObject;
						if (hawb != null)
						{
							records.Add(hawb);
							(hawb as ITransitWarehouseSyncEventParent)?.RegisterSyncEvent(mawbNumber, hawbNumber);
						}
					}
				}
			}

			if (records.Count == 0)
			{
				var mawbQuery = new ZDBOnlyQuery(typeof(CusHAWBBase));
				mawbQuery.AddToFilter(CusHAWBSchema.CS_HAWB, mawbNumber);
				var mawbDateSubQuery = new ZDBOnlySubQuery(typeof(CusMAWBBase), CusHAWBSchema.CS_CM);
				mawbDateSubQuery.AddToFilter(CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
				mawbDateSubQuery.AddToFilter(JoinCondition.Or, CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
				mawbQuery.AddSubQuery(mawbDateSubQuery, JoinCondition.And);
				var hawbs = CusHAWBBase.LoadAllFromQuery(mawbQuery, factory);
				if (hawbs.Length > 0 && hawbNumber.IsEmpty)
				{
					var hawb = GetRelevantHAWBForCTOs(hawbs, mawbNumber, transhipmentNumber, flightNumber, arrivalDate, portOfDischarge, goodsReceiptPlace) as BusinessObject;
					if (hawb != null)
					{
						records.Add(hawb);
						(hawb as ITransitWarehouseSyncEventParent)?.RegisterSyncEvent(mawbNumber, hawbNumber);
					}
				}
				else
				{
					records.AddRange(GetUnknownRecords(mawbNumber, hawbNumber, numberOfPackages, transhipmentNumber, flightNumber, arrivalDate, portOfDischarge, goodsReceiptPlace, message));
				}
			}

			return records.ToArray();
		}

		#region Implementation

		#region ICusUnderbondDependentCollectionParentFilters

		BusinessObject[] GetUnknownRecords(ZString mawbNumber, ZString hawbNumber, short numberOfPackages, ZString transhipmentNumber, ZString flightNumber, ZDateTime arrivalDate, ZString portOfDischarge, ZString goodsReceiptPlace, CMRCUSRESMessage message)
		{
			var records = new List<BusinessObject>();
			BusinessObject underbondsParent = null;
			CusMAWB subMAWB = null;
			var mawbLoader = new CusMAWBBase.Loader(factory);
			var arrivalDateToCompare = ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value);
			foreach (CusMAWB mawb in mawbLoader.FindMatchingMAWBs(mawbNumber))
			{
				if (mawb.CM_ArrivalDate == ZDateTime.Empty || mawb.CM_ArrivalDate > arrivalDateToCompare)
				{
					underbondsParent = mawb;
					subMAWB = mawb.CM_MasterHouseBill == hawbNumber ? mawb : subMAWB;
					SetMAWBInfo(mawb, flightNumber, arrivalDate, mawbNumber, portOfDischarge, goodsReceiptPlace, message.EM_MessageNum);
				}
			}

			if (subMAWB != null)
			{
				records.Add(subMAWB);
				if (!hawbNumber.IsEmpty && !mawbNumber.IsEmpty)
				{
					var hawb = FindHAWB(mawbNumber, hawbNumber);
					if (hawb != null)
					{
						SetHAWBInfo(hawb, numberOfPackages, transhipmentNumber);
						records.Add(hawb);
						hawb.RegisterSyncEvent(mawbNumber, hawbNumber);
					}
				}
			}
			else
			{
				if (!hawbNumber.IsEmpty && !mawbNumber.IsEmpty)
				{
					var hawb = FindHAWB(mawbNumber, hawbNumber);
					if (hawb != null)
					{
						SetHAWBInfo(hawb, numberOfPackages, transhipmentNumber);
						underbondsParent = hawb;
					}
				}

				if (underbondsParent == null)
				{
					if (!mawbNumber.IsEmpty)
					{
						var query = new ZQuery(CusUnderbondSchema.C4_MAWB, mawbNumber);
						var dateQuery = new ZQuery(CusUnderbondSchema.C4_ArrivalDate, ZDateTime.Empty);
						dateQuery.AddToFilter(JoinCondition.Or, CusUnderbondSchema.C4_ArrivalDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
						query.AddToFilter(dateQuery);
						var ubmCreator = new AirCargoUnderbondLoaderOrCreator(factory);
						var underbond =
							ubmCreator.FindPartUnderbondByFlightNumber(factory.Load<CusUnderbond>(query), flightNumber, arrivalDate) ??
							ubmCreator.CreateRecord(mawbNumber, hawbNumber, flightNumber, arrivalDate, portOfDischarge, numberOfPackages, "", "", goodsReceiptPlace, "", IsCTOCARST(message));
						if (underbond != null)
						{
							if (!hawbNumber.IsEmpty)
							{
								var outturnFilter = new ZQuery(CusOutturnSchema.C5_C4_Underbond, underbond.PK);
								outturnFilter.AddToFilter(JoinCondition.And, CusOutturnSchema.C5_HouseBill, SQLComparisonOperator.Equal, hawbNumber);
								var outturn = factory.LoadTop1<CusOutturn>(outturnFilter) ?? underbond.Outturns.AddNew();
								if (outturn != null)
								{
									outturn.C5_HouseBill = hawbNumber;
									if (numberOfPackages > 0 && (underbond.OutturnStatus.Code.IsEmpty || underbond.OutturnStatus.Code == CMRBaseStatuses.Codes.NotSent))
									{
										outturn.C5_PackagesOutturned = numberOfPackages;
									}

									if (outturn.C5_OutturnResultType.IsEmpty)
									{
										outturn.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
									}

									if (message != null)
									{
										outturn.Messages.Add(message);
									}

									outturn.RegisterSyncEvent(mawbNumber, hawbNumber);
								}
							}
							else if (message != null)
							{
								underbondsParent = underbond;
							}
						}
					}
				}

				if (underbondsParent != null)
				{
					records.Add(underbondsParent);
					var carstMessage = message as CMRCARSTMessage;
					if (!hawbNumber.IsEmpty && underbondsParent is CusMAWB && carstMessage != null)
					{
						carstMessage.EM_ApplicationReference = mawbNumber;
						carstMessage.EM_MessageOwner = hawbNumber.Left(CMRCARSTMessage.Schema.EM_MessageOwnerMaxLength);
						carstMessage.IsUnmatchedHouseReportRequired = true;
					}
				}
			}

			return records.ToArray();
		}

		ICusUnderbondDependentCollectionParent GetRelevantHAWBForCTOs(CusHAWBBase[] hAWBs, ZString mAWBNumber, ZString transhipmentNumber, ZString flightNumber, ZDateTime arrivalDate, ZString portOfDischarge, ZString goodsReceiptPlace)
		{
			ICusUnderbondDependentCollectionParent result = null;
			foreach (CusHAWBBase currentHAWB in hAWBs)
			{
				SetHAWBInfo(currentHAWB, 0, transhipmentNumber);
				if (currentHAWB.MAWB != null)
				{
					if (!arrivalDate.IsEmpty)
					{
						currentHAWB.MAWB.CM_ArrivalDate = arrivalDate;
					}

					if (!flightNumber.IsEmpty)
					{
						currentHAWB.MAWB.CM_FlightNo = flightNumber;
					}

					if (!portOfDischarge.IsEmpty)
					{
						currentHAWB.MAWB.CM_RL_NKDischargePort = portOfDischarge;
					}
				}

				result = currentHAWB;
			}

			return result;
		}

		ICusUnderbondDependentCollectionParent GetRelevantHAWBUsingOurSendersReference(CusHAWBBase[] hAWBs, ZString hAWBNumber, short numberOfPackages, ZString transhipmentNumber, ZString flightNumber, ZDateTime arrivalDate, ZString mAWBNumber, ZString portOfDischarge, ZString goodsReceiptPlace, ZString messageNum)
		{
			ICusUnderbondDependentCollectionParent result = null;
			foreach (CusHAWBBase currentHAWB in hAWBs)
			{
				using (currentHAWB.GetValidationSuspender())
				{
					CusMAWBBase mAWB = currentHAWB.MAWB;

					if (mAWB != null && mAWB.CM_MAWB == mAWBNumber)
					{
						SetHAWBInfo(currentHAWB, numberOfPackages, transhipmentNumber);
						if (!mAWB.CM_IsCTOMAWB)
						{
							SetAllRelatedCusMAWBINfo(mAWB, flightNumber, arrivalDate, portOfDischarge, goodsReceiptPlace, messageNum);
						}
						else
						{
							SetMAWBInfo(mAWB, flightNumber, arrivalDate, mAWBNumber, portOfDischarge, goodsReceiptPlace, messageNum);
						}

						if ((bool)Env.Registry.RawRegistry.AUCAutoSendUnderbondOnCARST.Value)
						{
							ICusUnderbondUnionCollectionParent underbondParent = mAWB;
							if (underbondParent != null)
							{
								CMRAutoUnderbondSender.CheckUnderbondsAndSend(underbondParent.AllUnderbonds.Cast<CusUnderbond>());
							}
						}

						result = currentHAWB;
					}
				}
			}

			return result;
		}

		bool IsCTOCARST(CMRCUSRESMessage message)
		{
			var carstMessage = message as CMRCARSTMessage;
			if (carstMessage != null)
			{
				return new AirCTOExpectedArrivalCusUnderbondFactory().CompanyHasCTOWithPremiseID(carstMessage.PremiseID, true);
			}

			return false;
		}

		#endregion

		#region UpdateRecords
		public void SetAllRelatedCusMAWBINfo(CusMAWBBase mAWB, ZString flightNumber, ZDateTime arrivalDate, ZString portOfDischarge, ZString goodsReceiptPlace, ZString messageNum)
		{
			var mawbLoader = new CusMAWBBase.Loader(factory);
			var arrivalDateToCompare = ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value);
			foreach (CusMAWBBase mawb in mawbLoader.FindMatchingMAWBs(mAWB.CM_MAWB))
			{
				if (mawb.CM_ArrivalDate == ZDateTime.Empty || mawb.CM_ArrivalDate > arrivalDateToCompare)
				{
					SetMAWBInfo(mawb, flightNumber, arrivalDate, mAWB.CM_MAWB, portOfDischarge, goodsReceiptPlace, messageNum);
				}
			}
		}

		public void SetMAWBInfo(CusMAWBBase mAWB, ZString flightNumber, ZDateTime arrivalDate, ZString mAWBNumber, ZString portOfDischarge, ZString goodsReceiptPlace, ZString messageNum)
		{
			bool fightDetailChanged = false;
			bool lodgedUnderbondExists = false;
			foreach (Customs.Business.CusUnderbond underbond in mAWB.AllUnderbonds)
			{
				if (underbond.UnderbondStatus.Code == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived ||
				underbond.UnderbondStatus.Code == CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived)
				{
					lodgedUnderbondExists = true;
					break;
				}
			}
			if (!lodgedUnderbondExists)
			{
				if (!arrivalDate.IsEmpty && mAWB.CM_ArrivalDate != arrivalDate)
				{
					mAWB.CM_ArrivalDate = arrivalDate;
					fightDetailChanged = true;
				}
				if (!flightNumber.IsEmpty && mAWB.CM_FlightNo != flightNumber)
				{
					mAWB.CM_FlightNo = flightNumber;
					fightDetailChanged = true;
				}
				if (!portOfDischarge.IsEmpty && mAWB.CM_RL_NKDischargePort != portOfDischarge)
				{
					mAWB.CM_RL_NKDischargePort = portOfDischarge;
					fightDetailChanged = true;
				}
				CusMAWB mawbAsCusMAWB = mAWB as CusMAWB;
				if (mawbAsCusMAWB != null && !goodsReceiptPlace.IsEmpty && mawbAsCusMAWB.DischargeCTOID != goodsReceiptPlace)
				{
					mawbAsCusMAWB.DischargeCTOID = goodsReceiptPlace;
					fightDetailChanged = true;
				}
				if (fightDetailChanged)
				{
					UpdateMasterUnderbondRecord(mAWB, flightNumber, arrivalDate, portOfDischarge);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					mAWB.Logs.AddNew(AutoEvents.EditedARecord, "Flight Details changed based on CARST " + messageNum);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		void UpdateMasterUnderbondRecord(CusMAWBBase mAWB, ZString flightNumber, ZDateTime arrivalDate, ZString portOfDischarge)
		{
			ZQuery filter = new ZQuery();
			UpdateUnderbondRecord(mAWB, flightNumber, arrivalDate, portOfDischarge, filter);
		}

		void UpdateUnderbondRecord(CusMAWBBase mAWB, ZString flightNumber, ZDateTime arrivalDate, ZString portOfDischarge, ZQuery filter)
		{
			CusUnderbond[] foundUnderbonds = (CusUnderbond[])mAWB.AllUnderbonds.Find(filter);
			if (foundUnderbonds.Length > 0)
			{
				foundUnderbonds[0].C4_ArrivalDate = arrivalDate;
				foundUnderbonds[0].C4_FlightNo = flightNumber;

				if (foundUnderbonds[0].C4_IsMoveFromDischarge)
				{
					foundUnderbonds[0].C4_RL_NKDischargePort = portOfDischarge;
				}
			}
		}

		/// <summary>
		/// Searches for the CusHAWB matching the specified MAWB and HAWB numbers
		/// </summary>
		/// <param name="mawbNumber">The MAWB number to match</param>
		/// <param name="hawbNumber">The HAWB number to match</param>
		/// <returns>The matching CusHAWB if found; null otherwise</returns>
		CusHAWBBase FindHAWB(ZString mawbNumber, ZString hawbNumber)
		{
			var filter = new ZDBOnlyQuery(typeof(CusHAWBBase));
			filter.AddToFilter(CusHAWBSchema.CS_HAWB, hawbNumber);

			ZDBOnlySubQuery cusMAWBQuery = new ZDBOnlySubQuery(typeof(CusMAWBBase), CusHAWBSchema.CS_CM);
			cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_MAWB, mawbNumber);
			var datePart = new ZQuery(CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
			datePart.AddToFilter(JoinCondition.Or, CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
			cusMAWBQuery.AddToFilter(datePart);

			filter.AddSubQuery(cusMAWBQuery, JoinCondition.And);
			return CusHAWBBase.LoadFromQuery(filter, factory);
		}

		/// <summary>
		/// Updates number of packages and transhipment number of the specified CusHAWBBase
		/// </summary>
		/// <param name="hawb">The CusHAWBBase to update</param>
		/// <param name="numberOfPackages">New value of the number of packages</param>
		/// <param name="transhipmentNumber">New value of the transhipment number</param>
		protected void SetHAWBInfo(CusHAWBBase hawb, short numberOfPackages, ZString transhipmentNumber)
		{
			if (!transhipmentNumber.IsEmpty)
			{
				hawb.CS_TranshipmentEntryNum = transhipmentNumber;
			}
			if (numberOfPackages > 0)
			{
				hawb.CS_PiecesManifested = numberOfPackages;
			}
		}

		#endregion

		readonly BusinessObjectFactory factory;

		#endregion
	}
}
