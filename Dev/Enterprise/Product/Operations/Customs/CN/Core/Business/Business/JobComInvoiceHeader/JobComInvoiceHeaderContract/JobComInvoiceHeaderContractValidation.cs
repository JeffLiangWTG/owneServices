using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class JobComInvoiceHeaderContractValidation : JobComInvoiceHeaderRefsValidation
	{
		public JobComInvoiceHeaderContractValidation(AutoJobComInvoiceHeaderRefs parent) : base(parent)
		{
		}

		protected override void CheckJ2_ReferenceType()
		{
		}

		protected override void CheckJ2_ReferenceNumber()
		{
			MandatoryValidation.CheckEntered(Parent.J2_ReferenceNumberInfo);
		}
	}
}
