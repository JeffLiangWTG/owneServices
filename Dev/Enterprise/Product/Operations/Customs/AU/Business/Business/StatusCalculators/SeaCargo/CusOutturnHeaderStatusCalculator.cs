using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderStatusCalculator : CMRStatusCalculator<CusOutturnHeader>
	{
		public CusOutturnHeaderStatusCalculator(CusOutturnHeader header)
			: base(header)
		{
			WarnIfInInconsistentStateOrOriginalFailed();
		}

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.SEAOUT };

		protected internal override ZPropertyInfo StatusInfo => Parent.C6_MessageStatusInfo;

		protected internal override bool InterestedInPendingMessages => true;

		protected override void DeriveStatusCore()
		{
			base.DeriveStatusCore();
			ReceiveCargoAtDepot();

			var outturnStatus = (ZString)StatusInfo.Value;

			if (StatusInfo.HasChanges)
			{
				if (outturnStatus == CMRBaseStatuses.Codes.OriginalAccepted ||
					outturnStatus == CMRBaseStatuses.Codes.AmendmentAccepted ||
					outturnStatus == CMRBaseStatuses.Codes.WithdrawalAccepted)
				{
					UpdateLastMessageDateForOutturns(outturnStatus);
				}
			}

			if (outturnStatus == CMRBaseStatuses.Codes.AwaitingResponseToOriginal ||
				outturnStatus == CMRBaseStatuses.Codes.AwaitingResponseToAmendment ||
				outturnStatus == CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal ||
				outturnStatus == CMRBaseStatuses.Codes.NotSent)
			{
				Parent.CusUnderbondOutturnLogManager.CancelAllOutturnLogs();
			}

			WarnIfInInconsistentStateOrOriginalFailed();
		}

		void ReceiveCargoAtDepot()
		{
			if (Parent.IsInDatabase && CMRUtilities.MessageStatusChangedToAccepted(StatusInfo))
			{
				var lastOutturnResponse = (CMRSEAOUTRMessage)Parent.Messages.GetLastMessage(EDIMessage.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEAOUT, EDIMessage.Direction.Receive);
				if (lastOutturnResponse != null)
				{
					var acceptedOutturns = lastOutturnResponse.GetAllRelevantOutturnLines(Parent.Messages)
						.Where(x => !x.C5_MasterBill.IsEmpty && !x.C5_HouseBill.IsEmpty && !x.C5_ContainerNumber.IsEmpty && x.OutturnStatus == CMRMessage.CMRMessageStatusDescription.ACCEPTED);

					foreach (var sameOceanBillOutturns in acceptedOutturns.GroupBy(x => x.C5_MasterBill).Select(x => x.ToList()))
					{
						var houseQuery = new ZDBOnlyQuery(typeof(CusSCAHouse));
						houseQuery.AddToFilter(CusSCAHouseSchema.CA_HouseBill, sameOceanBillOutturns.Select(x => x.C5_HouseBill));
						var oceanQuery = new ZDBOnlySubQuery(typeof(CusSCAOceanBill), CusSCAOceanBillSchema.PK);
						oceanQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
						oceanQuery.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, sameOceanBillOutturns[0].C5_MasterBill);
						oceanQuery.AddToFilter(CusSCAOceanBillSchema.CB_LloydsIMO, Parent.C6_LloydsIMO);
						oceanQuery.AddToFilter(CusSCAOceanBillSchema.CB_Voyage, Parent.C6_VoyageNum);
						houseQuery.AddSubQuery(CusSCAHouseSchema.CA_CB, oceanQuery, JoinCondition.And);

						foreach (var house in Factory.Load<CusSCAHouse>(houseQuery))
						{
							foreach (CusSCAPivot pivot in house.Pivot)
							{
								var outturn = sameOceanBillOutturns.FirstOrDefault(x => x.C5_HouseBill.EqualsIgnoringCase(house.CA_HouseBill) && x.C5_ContainerNumber.EqualsIgnoringCase(pivot.CN_ContainerNumber));
								if (outturn != null)
								{
									pivot.OutturnCargo(outturn);
								}
							}
						}
					}
				}
			}
		}

		void UpdateLastMessageDateForOutturns(ZString statusCode)
		{
			var messageDate = ZDateTime.Empty;
			if (statusCode == CMRBaseStatuses.Codes.OriginalAccepted ||
				statusCode == CMRBaseStatuses.Codes.AmendmentAccepted)
			{
				messageDate = ZDateTime.Today;
			}

			foreach (CusOutturn outturn in Parent.Outturns)
			{
				if (statusCode == CMRBaseStatuses.Codes.WithdrawalAccepted || outturn.C5_MessageStatus != CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected)
				{
					if (outturn.C5_LastMessageDate.IsEmpty || messageDate.IsEmpty)
					{
						outturn.C5_LastMessageDate = messageDate;
					}
				}
			}
		}

		protected void WarnIfInInconsistentStateOrOriginalFailed()
		{
			if (Parent.Calculator != null)
			{
				if (Parent.HasSplitMessageOriginalRejectedLog)
				{
					Parent.OutturnStatus.DescriptionInfo.AddMessageError("Outturn in error - correct error & resend.\r\nThe previous Outturn message had to be split into multiple messages as it exceeded the maximum number of lines allowed by Customs.\r\nThe first of the split messages has been rejected by Customs.\r\n\r\nYou therefore you need to correct the error and resend the outturn as an original again.");
					Parent.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalRejected;
				}
				else if (Parent.HasSplitMessageFailedLog)
				{
					Parent.OutturnStatus.DescriptionInfo.AddMessageError("Outturn Rejected - correct error & resend.\r\nThe previous Outturn message had to be split into multiple messages as it exceeded the maximum number of lines allowed by Customs.\r\nOne of the split messages has been rejected leaving the Outturn in an inconsistent state with Customs.\r\n\r\nYou need to fix the error noted by Customs and then resend the Outturn again.\r\n\r\nIf the Outturn continues to be rejected, you will need to withdraw the current outturn and resend the fixed outturn as an original again.");
					Parent.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentRejected;
				}
				else if (Parent.HasNonExistantLineAtCustomsLog)
				{
					Parent.OutturnStatus.DescriptionInfo.AddMessageError("Inconsistent Outturn State - withdraw & resend original.\r\nThe previous Outturn message was rejected trying to amend or delete lines that do not exist at Customs.\r\nThe Outturn is therefore in an inconsistent state to that recorded at Customs.\r\n\r\nYou need to withdraw the current outturn and then resend as an original.");
					Parent.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentRejected;
				}

				Parent.OutturnStatus.DescriptionInfo.RefreshBinding();
			}
		}
	}
}
