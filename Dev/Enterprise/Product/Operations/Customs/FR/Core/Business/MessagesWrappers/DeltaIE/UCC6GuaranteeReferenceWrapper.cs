using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	internal class UCC6GuaranteeReferenceWrapper : IGuaranteeReference
	{
		UCC6GuaranteeReferenceWrapper(string deferredPayment)
		{
			this.deferredPayment = deferredPayment;
		}
		readonly string deferredPayment;

		public string AccessCode => null;

		public double AmountToBeCovered => 0.00D;

		public string CcQualifier => null;

		public string CurrencyCode => Core.Constants.CurrencyCodes.France;

		public ICustomsOfficeOfGuarantee CustomsOfficeOfGuarantee => UCC6CustomsOfficeOfGuaranteeWrapper.New();

		public string Grn => grn ?? (grn = deferredPayment);
		string grn;

		public string OtherGuaranteeReference => null;

		public static UCC6GuaranteeReferenceWrapper New(string deferredpayment) =>  new UCC6GuaranteeReferenceWrapper(deferredpayment);
	}
}
