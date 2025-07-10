using System.Collections.Immutable;
using CargoWise.Types;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business
{
	class MethodOfPaymentHelper
	{
		public static bool RequireDeferralPaymentParty(ZString paymentMethod)
		{
			return DeferredMethodsOfPayment.Contains(paymentMethod);
		}

		public static readonly ImmutableHashSet<ZString> DeferredMethodsOfPayment = ImmutableHashSet.Create<ZString>(MethodOfPaymentTypes.E, MethodOfPaymentTypes.F, MethodOfPaymentTypes.G, MethodOfPaymentTypes.Z);
	}
}
