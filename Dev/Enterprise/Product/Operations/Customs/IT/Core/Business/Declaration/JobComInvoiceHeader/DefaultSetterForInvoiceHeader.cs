namespace Enterprise.Customs.IT.Business.Declaration;

sealed class DefaultSetterForInvoiceHeader : EU.Business.DefaultSetterForInvoiceHeader
{
	public DefaultSetterForInvoiceHeader(JobComInvoiceHeader child, JobDeclaration declaration) : base(child, declaration)
	{
	}

	protected override void DefaultForNewElementCore()
	{
		base.DefaultForNewElementCore();

		var declaration = (JobDeclaration)base.declaration;
		if (declaration is JobDeclaration itDeclaration && itDeclaration.IsUcc6ExportAndIsShipmentIncoTermOther)
		{
			newElement.JZ_AdditionalTerms = declaration.ZG_AdditionalDeliveryTerms;
		}
	}
}
