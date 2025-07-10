using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AIROUTRMessageProcessor : BaseAirCargoMessageProcessor
	{
		public AIROUTRMessageProcessor(LoggingInformation logger) : base(logger, CMRMessage.CMRMessageTypes.AIROUT, "Air Waybill Outturn Report Response - (AIROUTR)")
		{
		}

		protected const string AcceptedWithErrorsString = "ACCEPTED WITH ERRORS";

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return statusType.Contains(AcceptedString); }
		}

		protected override string DoProcessingReturningStatus(Messaging.Business.EDIMessage message)
		{
			var result = base.DoProcessingReturningStatus(message);

			if (HasCustomsError)
			{
				if (HasAmendDeleteNonExistantLineError)
				{
					CreateNonExistantLineErrorLog();
				}
				else if (AcceptedWithErrors)
				{
					var underBond = incomingMessage.EM_LinkedObject as CusUnderbond;
					if (underBond != null)
					{
						// go through the message finding each error
						// search for respective outturn using MWB HWB combination or simply HWB if MWB/HWB not found
						foreach (SegmentGroup4 group4 in cUSRES.Group4)
						{
							foreach (FTXSegment fTX in group4.FTX)
							{
								ZString ftx = fTX.TextLiteral.FreeTextValue1;
								if (ftx.Contains("MWB=") || ftx.Contains("HWB="))
								{
									var mawb = ZString.Empty;
									int i = ftx.IndexOf("MWB=");
									if (i >= 0)
									{
										mawb = ftx.SubstringSafe(i + 4);
										i = mawb.IndexOf(",");
										if (i >= 0)
										{
											mawb = mawb.SubstringSafe(0, i);
										}
									}

									var hawb = ZString.Empty;
									int j = ftx.LastIndexOf("HWB=");
									if (j >= 0)
									{
										hawb = ftx.SubstringSafe(j + 4);
									}

									var outturnQuery = new ZQuery(CusOutturnSchema.C5_MasterBill, mawb);
									outturnQuery.AddToFilter(CusOutturnSchema.C5_HouseBill, hawb);
									Customs.Business.CusOutturn[] outturns = (Customs.Business.CusOutturn[])underBond.Outturns.Find(outturnQuery);
									if (outturns.Length > 0)
									{
										outturns[0].C5_MessageStatus = CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected;
									}
									else
									{
										outturnQuery = new ZQuery(CusOutturnSchema.C5_HouseBill, hawb);
										outturns = (Customs.Business.CusOutturn[])underBond.Outturns.Find(outturnQuery);
										if (outturns.Length > 0)
										{
											outturns[0].C5_MessageStatus = CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected;
										}
										else
										{
											foreach (Customs.Business.CusOutturn outturn in underBond.Outturns)
											{
												if (outturn.C5_ParentTableCode == CusHAWBSchema.Constants.Prefix && outturn.C5_ParentID.IsValid && !hawb.IsEmpty)
												{
													var houseBill = outturn.Factory.Load<CusHAWB>(outturn.C5_ParentID);
													if (houseBill != null)
													{
														if (houseBill.CS_HAWB == hawb)
														{
															outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected;
															break;
														}
													}
												}
												else if (outturn.C5_ParentTableCode == CusMAWBSchema.Constants.Prefix && outturn.C5_ParentID.IsValid && !mawb.IsEmpty)
												{
													var masterBill = outturn.Factory.Load<CusMAWB>(outturn.C5_ParentID);
													if (masterBill != null)
													{
														if (masterBill.CM_MAWB == mawb)
														{
															outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected;
															break;
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}

			return result;
		}

		protected override bool DoAdditionalProcessing()
		{
			var underBond = incomingMessage.EM_LinkedObject as CusUnderbond;
			if (underBond != null && underBond.OutturnStatus != null)
			{
				if (underBond.OutturnStatus.Code == CMRBaseStatuses.Codes.WithdrawalAccepted)
				{
					underBond.C4_Outurned = ZDate.Empty;    // Outurn has been withdrawn - reset Outurned date
				}
			}

			return base.DoAdditionalProcessing();
		}

		#region LogManagerProcessing

		bool AcceptedWithErrors
		{
			get { return statusDescription.Contains(AcceptedWithErrorsString); }
		}

		bool HasCustomsError
		{
			get
			{
				var result = false;
				foreach (SegmentGroup4 group4 in cUSRES.Group4)
				{
					if (IsErrorResponse(group4.ERC))
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		bool IsErrorResponse(ERCSegmentMessageSection eRCSection)
		{
			bool result = false;
			foreach (ERCSegment eRC in eRCSection)
			{
				if (eRC.ApplicationErrorDetail.ApplicationErrorIdentification == "ERROR")
				{
					result = true;
					break;
				}
			}

			return result;
		}

		bool HasAmendDeleteNonExistantLineError
		{
			get
			{
				var result = false;
				foreach (SegmentGroup4 group4 in cUSRES.Group4)
				{
					if (IsAmendDeleteError(group4.ERC) && IsAmendDeleteDesc(group4.FTX))
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		bool IsAmendDeleteError(ERCSegmentMessageSection eRCSection)
		{
			bool result = false;
			foreach (ERCSegment eRC in eRCSection)
			{
				if (eRC.ApplicationErrorDetail.ApplicationErrorIdentification == "CG1535")
				{
					result = true;
					break;
				}
			}

			return result;
		}

		bool IsAmendDeleteDesc(FTXSegmentMessageSection fTXSection)
		{
			bool result = false;
			foreach (FTXSegment fTX in fTXSection)
			{
				if (fTX.TextLiteral.FreeTextValue1.Contains("ATTEMPTED TO AMEND/DELETE A LINE THAT DOESN'T EXIST"))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		void CreateNonExistantLineErrorLog()
		{
			if (incomingMessage.EM_LinkedObject != null)
			{
				CusUnderbond underBond = incomingMessage.EM_LinkedObject as CusUnderbond;
				if (underBond != null)
				{
					underBond.CusUnderbondOutturnLogManager.AddANewHasNonExistantLineAtCustomsLog(nonExistantLineReference);
				}
			}
		}
		internal const string nonExistantLineReference = "Outturn not accepted. Outturn in an inconsistent state. Withdraw the Outturn and resend as an Original.";

		#endregion
	}
}
