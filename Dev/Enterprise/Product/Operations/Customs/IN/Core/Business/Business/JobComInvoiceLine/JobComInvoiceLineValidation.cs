namespace Enterprise.Customs.IN.Business;

public class JobComInvoiceLineValidation : AutoINJobComInvoiceLineValidation
{
	public JobComInvoiceLineValidation(JobComInvoiceLine parent)
		: base(parent)
	{
	}

	protected new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateAccessoryDescription();
	}

	public void ValidateAccessoryDescription()
	{
		ValidateCalculatedProperty(Parent.AccessoryDescriptionInfo);
	}

	protected virtual void CheckAccessoryDescription()
	{
	}
}
