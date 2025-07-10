namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GroupInvoiceChargeValidation : EU.Business.Declaration.GroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge charge) : base(charge)
		{ }

		//TODO (Joo's comment)
		//I suggest we should add a validation to the invoice line valuation adjustment code field if a certain charge is missing or invalid. Otherwise be conscious of the performance if you go through each invoice line from here.  

		// DJC comment - fine, but not part of stage 1
	}
}
