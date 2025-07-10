using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class PayableOrderDocAddressValidation : JobDocAddressValidation
	{
		public PayableOrderDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}
	}
}
