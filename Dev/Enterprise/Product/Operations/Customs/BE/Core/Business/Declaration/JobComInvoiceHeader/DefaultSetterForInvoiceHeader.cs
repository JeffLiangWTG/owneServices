namespace Enterprise.Customs.BE.Business.Declaration;

public class DefaultSetterForInvoiceHeader : EU.Business.DefaultSetterForInvoiceHeader
{
	public DefaultSetterForInvoiceHeader(JobComInvoiceHeader child, JobDeclaration declaration) : base(child, declaration)
	{
	}

	protected new JobDeclaration declaration => (JobDeclaration)base.declaration;

	protected override void DefaultForNewElementCore()
	{
		base.DefaultForNewElementCore();
		if (newElement is JobComInvoiceHeader invoice)
		{
			invoice.ZG_AgreedPlaceCode = declaration.ZG_AgreedPlaceCode;
		}
	}
}
