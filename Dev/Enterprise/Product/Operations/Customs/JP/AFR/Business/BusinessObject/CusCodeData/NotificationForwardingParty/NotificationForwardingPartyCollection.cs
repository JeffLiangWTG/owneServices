
namespace Enterprise.Customs.JP.AFR.Business
{
	public class NotificationForwardingPartyCollection : CusCodeDataWithSequenceNumberLineCollection<NotificationForwardingParty>
	{
		public NotificationForwardingPartyCollection(JPAFRBills master)
			: base(master, NotificationForwardingParty.NFPType)
		{
		}

		public JPAFRBills Bill
		{
			get { return (JPAFRBills)Relationship.Master; }
		}

		protected override bool AllowNew
		{
			get
			{
				var master = Bill;
				return master != null && !master.ShouldSynchronise;
			}
		}
	}
}
