using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSEAOUTRMessage : CMRCUSRESMessage
	{
		public CMRSEAOUTRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference(SendersReference);

			result = CusSCAHouse.Load(Factory, reference);
			if (result == null)
			{
				result = CusOutturnHeader.LoadFromSendersReference(Factory, reference);
			}

			if (result == null)
			{
				result = base.GetWrappedObject();
			}

			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.SEAOUT;
		}

		protected override CMRCUSRESMessage LinkOrCloneMessageCore(EDIMessageCollection messages)
		{
			CMRCUSRESMessage result = base.LinkOrCloneMessageCore(messages);// this sets EM_LinkedObject
			if (EM_LinkedObject is CusOutturnHeader)
			{
				CMRStatusRecalculationSuspender.ResumeStatusRecalculation(Factory);
				CloneToAllRelevantOutturnLines(messages);
				CMRStatusRecalculationSuspender.SuspendStatusRecalculation(Factory);
			}
			return result;
		}

		void CloneToAllRelevantOutturnLines(EDIMessageCollection messages)
		{
			foreach (var outturn in GetAllRelevantOutturnLines(messages))
			{
				var clonedMessage = (CMRSEAOUTRMessage)Clone();
				clonedMessage.EM_Status = EDIMessage.Status.Received;
				outturn.Messages.Add(clonedMessage);
				// Default status to "NOT". It's temparory solution to stop defaulting status when open the form. Outturn status calculation needs to be refactored.
				((ICalculatedCusStatusCalculator)outturn.MessageStatusCalculator).DeriveStatusIfEmptyWithMessages();
			}
		}

		internal IEnumerable<DepotCusOutturn> GetAllRelevantOutturnLines(EDIMessageCollection messages)
		{
			var outturnMessage = GetOutgoingMessageByBGMRefAndVersion(messages, CMRMessage.CMRMessageTypes.SEAOUT) as CMRMessage;
			if (outturnMessage != null)
			{
				List<ZString> keys = new List<ZString>();
				foreach (CUSCARLineKey key in outturnMessage.LineKeys())
				{
					string keyString = key.ToString();
					if (!keys.Contains(keyString))
					{
						keys.Add(keyString);
					}
				}
				foreach (DepotCusOutturn outturn in ((CusOutturnHeader)EM_LinkedObject).Outturns)
				{
					if (keys.Contains(outturn.CusOutturnKey.ToString()))
					{
						yield return outturn;
					}
				}
			}
		}

		public bool IsFullyAccepted
		{
			get
			{
				return GetStatusDescription().Contains("ACCEPTED WITHOUT ERRORS");
			}
		}

		public bool IsFullyRejected
		{
			get
			{
				return GetStatus().Contains("REJECTED");
			}
		}

		public CMRSEAOUTRLine MatchingLine(ZString containerNumber, ZString masterBill, ZString houseBill)
		{
			if (CUSRES != null)
			{
				foreach (SegmentGroup4 group4 in CUSRES.Group4)
				{
					CMRSEAOUTRLine result = new CMRSEAOUTRLine(group4);
					if (result.ContainerNumber == containerNumber && result.HouseBillNumber == houseBill && result.MasterBillNumber == masterBill &&
								!result.FTXText.Contains("REPORT NOT FOUND FOR CHANGE/DELETE"))
					{
						return result;
					}
				}
			}
			return null;
		}
	}
}
