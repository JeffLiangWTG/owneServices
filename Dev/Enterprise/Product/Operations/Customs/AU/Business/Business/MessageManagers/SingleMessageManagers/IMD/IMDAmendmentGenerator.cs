using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDAmendmentGenerator : CMRAmendmentGenerator
	{
		public IMDAmendmentGenerator(CusEntryHeader entryHeader, CMRAmendmentWithdrawalReason reason)
			: base(entryHeader)
		{
			EntryHeader = entryHeader;
			Reason = reason;
		}

		protected readonly CusEntryHeader EntryHeader;
		internal readonly CMRAmendmentWithdrawalReason Reason;

		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo)
		{
			BaseImportMessageBuilder result = null;
			if (!bizo.IsDeleted)
			{
				CusEntryHeader entryHeader = bizo as CusEntryHeader;
				if (entryHeader.Declaration.IsSAC)
				{
					result = new SACMessageBuilder(entryHeader, CMRMessageTypes.Amendment);
				}
				else
				{
					result = new IMDMessageBuilder(entryHeader, CMRMessageTypes.Amendment);
				}
				result.AmendmentWithdrawalReason = Reason;
			}
			return result;
		}

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo) => (bizo as CusEntryHeader).Messages;

		protected override ZPropertyInfo[] UniqueIdentifierInfos => throw new NotSupportedException("We do not go though this message to detect a unique key change");

		protected override bool UniqueIdentifierBeingChanged => false;
	}
}
