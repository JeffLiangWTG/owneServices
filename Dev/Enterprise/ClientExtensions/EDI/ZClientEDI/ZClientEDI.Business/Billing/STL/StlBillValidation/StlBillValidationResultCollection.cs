using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlBillValidationResultCollection : NonPersistentBusinessObjectCollection<StlBillValidationResult>
	{
		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new StlBillValidationResult();

		public void PopulateResults(StlBillCollection bills)
		{
			foreach (StlBill bill in bills)
			{
				foreach (var rowNotification in bill.NotificationsIncludingChildren)
				{
					Add(new StlBillValidationResult() { StlBillPK = bill.PK, OrgCode = bill.OrganisationCode, ValidationMessage = rowNotification.Message });
				}
			}
		}
	}
}

