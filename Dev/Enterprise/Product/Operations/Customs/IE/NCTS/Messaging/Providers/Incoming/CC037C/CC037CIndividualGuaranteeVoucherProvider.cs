using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC037CIndividualGuaranteeVoucherProvider
	{
		public CC037CIndividualGuaranteeVoucherProvider(IndividualGuaranteeVoucherType individualGuaranteeVoucher)
		{
			this.individualGuaranteeVoucher = Argument.NotNull(individualGuaranteeVoucher, nameof(individualGuaranteeVoucher));
		}
		readonly IndividualGuaranteeVoucherType individualGuaranteeVoucher;

		public ZDate IssueDate => new ZDate(individualGuaranteeVoucher.IssueDate);

		public ZDate ExpiryDate => new ZDate(individualGuaranteeVoucher.ExpiryDate);

		public ZString CopyGiven => individualGuaranteeVoucher.CopyGiven.GetXmlEnumAttributeValue();

		public ZString TIRCarnet => individualGuaranteeVoucher.TirCarnet.GetXmlEnumAttributeValue();

		public ZDecimal VoucherAmount => individualGuaranteeVoucher.VoucherAmount ?? ZDecimal.Zero;

		public ZString Currency => individualGuaranteeVoucher.Currency;
	}
}
