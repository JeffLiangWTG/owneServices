using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC037CIndividualGuaranteeByGuarantorProvider
	{
		public CC037CIndividualGuaranteeByGuarantorProvider(IndividualGuaranteeByGuarantorType individualGuaranteeByGuarantor)
		{
			this.individualGuaranteeByGuarantor = Argument.NotNull(individualGuaranteeByGuarantor, nameof(individualGuaranteeByGuarantor));
		}
		readonly IndividualGuaranteeByGuarantorType individualGuaranteeByGuarantor;

		public ZDecimal GuaranteeAmount => individualGuaranteeByGuarantor.GuaranteeAmount;

		public ZString Currency => individualGuaranteeByGuarantor.Currency;
	}
}
