using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class UCC6GuaranteeWrapper : IGuarantee
	{
		UCC6GuaranteeWrapper(string deferredPayment)
		{
			this.deferredPayment = deferredPayment;
		}

		readonly string deferredPayment;
		const string DeferredPaymentGuaranteeType = "1";

		public ICollection<IGuaranteeReference> GuaranteeReference => guaranteeReference ?? (guaranteeReference = GetGuaranteeReferenceCollection());
		ICollection<IGuaranteeReference> guaranteeReference;

		ICollection<IGuaranteeReference> GetGuaranteeReferenceCollection()
		{
			Collection<IGuaranteeReference> result = null;

			if (!string.IsNullOrEmpty(deferredPayment))
			{
				result = new Collection<IGuaranteeReference>() { UCC6GuaranteeReferenceWrapper.New(deferredPayment) };
			}
			return result;
		}

		public string GuaranteeType => DeferredPaymentGuaranteeType;

		public static UCC6GuaranteeWrapper New(string deferredPayment) => string.IsNullOrEmpty(deferredPayment) ? null : new UCC6GuaranteeWrapper(deferredPayment);
	}
}
