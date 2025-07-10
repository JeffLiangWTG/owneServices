using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IApportionmentListing
	{
		void UpdateOrCreateCost(AccChargeCode chargeCode, OrgHeader creditor, decimal amount);
	}
}
