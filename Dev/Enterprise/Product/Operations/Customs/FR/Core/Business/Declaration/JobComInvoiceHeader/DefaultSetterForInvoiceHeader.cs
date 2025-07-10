namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DefaultSetterForInvoiceHeader : EU.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(JobComInvoiceHeader child, JobDeclaration declaration) : base(child, declaration)
		{
		}

		protected new JobDeclaration declaration => (JobDeclaration)base.declaration;

		protected new JobComInvoiceHeader newElement => (JobComInvoiceHeader)base.newElement;

		protected override void DefaultForNewElementCore()
		{
			base.DefaultForNewElementCore();
			newElement.JZ_IncoTerm = newElement.JobDeclaration.JE_ShipmentIncoTerm;
			newElement.JZ_IncoTermPlace = newElement.JobDeclaration.JE_ShipmentIncoTermPlace;
			newElement.ZG_AgreedPlaceCode = newElement.JobDeclaration.ZG_AgreedPlaceCode;
			newElement.ZG_ValuationMethod = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
		}
	}
}
