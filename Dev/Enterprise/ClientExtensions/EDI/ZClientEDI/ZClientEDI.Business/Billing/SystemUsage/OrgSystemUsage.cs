using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class OrgSystemUsage : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgSystemUsage(OrganisationBill bill, SystemUsage usage)
			: base(bill != null ? bill.Factory : null)
		{
			Bill = bill;
			Usage = usage;
		}

		public OrganisationBill Bill { get; private set; }
		public SystemUsage Usage { get; private set; }
	}

	public class OrgSystemUsageCollection : NonPersistentBusinessObjectCollection<OrgSystemUsage>
	{
		public static OrgSystemUsageCollection CreateAndBuild(OrganisationBillCollection bills)
		{
			var result = new OrgSystemUsageCollection(bills);
			result.Rebuild();
			return result;
		}

		public OrgSystemUsageCollection(OrganisationBillCollection bills)
			: base(bills != null ? bills.Factory : null)
		{
			this.bills = bills;
		}

		public void Rebuild()
		{
			RemoveAll();

			if (bills != null)
			{
				foreach (OrganisationBill bill in bills)
				{
					foreach (SystemUsage usage in bill.SystemUsages)
					{
						Add(new OrgSystemUsage(bill, usage));
					}
				}
			}
		}

		readonly OrganisationBillCollection bills;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgSystemUsage(null, null);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}

