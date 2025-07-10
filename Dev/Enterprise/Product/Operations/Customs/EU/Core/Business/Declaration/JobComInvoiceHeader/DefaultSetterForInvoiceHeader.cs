using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class DefaultSetterForInvoiceHeader : Customs.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(JobComInvoiceHeader child, JobDeclaration declaration)
			: base(child, declaration)
		{
		}

		protected new JobDeclaration declaration => (JobDeclaration)base.declaration;

		protected override void DefaultForNewElementCore()
		{
			base.DefaultForNewElementCore();
			DefaultIncoTermsDetails();
		}

		protected override void SetDefaultsForAdditionalInvoiceCore(BaseJobComInvoiceHeader previousInvoice)
		{
			base.SetDefaultsForAdditionalInvoiceCore(previousInvoice);
			newElement.JZ_ValuationCode = previousInvoice.JZ_ValuationCode;
		}

		protected override void SetDefaultIncoTerm(BaseJobComInvoiceHeader previousInvoice)
		{
			newElement.JZ_IncoTerm = declaration.JE_ShipmentIncoTerm;
		}

		protected void DefaultIncoTermsDetails()
		{
			if (newElement is JobComInvoiceHeader euInvoice)
			{
				euInvoice.JZ_IncoTermPlace = declaration.JE_ShipmentIncoTermPlace;
				euInvoice.ZG_AgreedPlaceCode = declaration.IsUCC6
					? declaration.EUD_AgreedPlaceCode
					: declaration.ZG_AgreedPlaceCode;
			}
		}
	}
}
