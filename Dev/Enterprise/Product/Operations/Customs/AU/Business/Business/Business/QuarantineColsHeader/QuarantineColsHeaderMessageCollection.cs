using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineColsHeaderMessageCollection : EDIMessageCollection
	{
		public QuarantineColsHeaderMessageCollection(QuarantineColsHeader master)
			: base(master, new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.COLS))
		{
		}

		public QuarantineColsHeaderMessageCollection(QuarantineColsHeader master, ZString messageStatus, bool isIncluded)
			: base(master, FilterByApplicationCodeAndStatus(messageStatus, isIncluded))
		{
		}

		static ZQuery FilterByApplicationCodeAndStatus(ZString messageStatus, bool isIncluded)
		{
			var filter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.COLS);
			if (isIncluded)
			{
				filter.AddToFilter(EDIMessageSchema.EM_Status, messageStatus);
			}
			else
			{
				filter.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, messageStatus);
			}
			return filter;
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			if (child is EDIMessage message && message.EM_LinkUniqueID.IsEmpty)
			{
				base.SetCollectionRelationships(child);
			}
		}
	}
}
