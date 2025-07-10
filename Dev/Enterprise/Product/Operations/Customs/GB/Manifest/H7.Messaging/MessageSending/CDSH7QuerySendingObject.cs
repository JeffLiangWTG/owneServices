using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public abstract class CDSH7QuerySendingObject : BaseCDSQuerySendingObject
	{
		public CDSH7QuerySendingObject(AsycudaBill asycudaBill)
		{
			bill = asycudaBill;
		}

		protected readonly AsycudaBill bill;

		protected override DataContextType GetContextTypeCore() => DataContextType.AsycudaBill;

		protected override ZString GetContextReferenceCore() => bill.Header.AMA_JobReference + bill.ABL_BillNumber;

		protected override ZString GetCredentialKeyCore()
		{
			return bill.Header.CredentialsKey;
		}
	}

	public class CDSH7QueryMRNSendingObject : CDSH7QuerySendingObject
	{
		public CDSH7QueryMRNSendingObject(AsycudaBill asycudaBill, string mrnQueryType) : base(asycudaBill)
		{
			if (mrnQueryType != H7QueryTypeList.Codes.MRNSummary && mrnQueryType != H7QueryTypeList.Codes.MRNSnapshot)
			{
				throw new ArgumentException("Only MRN query types are supported.");
			}

			queryType = mrnQueryType;
		}

		readonly string queryType;

		protected override EntryNumberSet GetEntryNumberSetBySource(ZGuid sourcePK) => new EntryNumberSet(sourcePK, bill.MovementReferenceNumber);

		protected override ZString EntryNumberType { get { return CDSDISQueryHelper.Constants.EntryNumberTypes.MRN; } }

		protected override ZString NotificationType { get { return queryType == H7QueryTypeList.Codes.MRNSummary ? CDSDISQueryHelper.Constants.NotificationTypes.Status : CDSDISQueryHelper.Constants.NotificationTypes.Full; } }
	}

	public class CDSH7QueryDUCRSendingObject : CDSH7QuerySendingObject
	{
		public CDSH7QueryDUCRSendingObject(AsycudaBill asycudaBill) : base(asycudaBill)
		{
		}

		protected override EntryNumberSet GetEntryNumberSetBySource(ZGuid sourcePK)
		{
			var entryNumber =  bill.PreviousDocuments.FirstOrDefault(d => d.CSI_Code == PreviousDocumentCodeListCDS.Codes.DeclarationUniqueConsignmentReferenceDucr).CSI_ReferenceNumber;
			return new EntryNumberSet(sourcePK, entryNumber);
		}

		protected override ZString EntryNumberType { get { return CDSDISQueryHelper.Constants.EntryNumberTypes.DUCR; } }

		protected override ZString NotificationType { get { return CDSDISQueryHelper.Constants.NotificationTypes.Status; } }
	}

	public class CDSH7QueryUCRSendingObject : CDSH7QuerySendingObject
	{
		public CDSH7QueryUCRSendingObject(AsycudaBill asycudaBill) : base(asycudaBill)
		{
		}

		protected override EntryNumberSet GetEntryNumberSetBySource(ZGuid sourcePK) => new EntryNumberSet(sourcePK, bill.ABL_UCRNumber);

		protected override ZString EntryNumberType { get { return CDSDISQueryHelper.Constants.EntryNumberTypes.UCR; } }

		protected override ZString NotificationType { get { return CDSDISQueryHelper.Constants.NotificationTypes.Status; } }
	}
}
