using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC225CGuaranteeReferenceProvider
	{
		public static CC225CGuaranteeReferenceProvider Create(GuaranteeReferenceType09 xmlObject) => new CC225CGuaranteeReferenceProvider(xmlObject);

		public CC225CGuaranteeReferenceProvider(GuaranteeReferenceType09 xmlObject)
		{
			guaranteeReference = Argument.NotNull(xmlObject, nameof(xmlObject));
		}
		readonly GuaranteeReferenceType09 guaranteeReference;

		public ZInt SequenceNumber => ZInt.ParseSafe(guaranteeReference.SequenceNumber, ZInt.Zero);

		public ZString GRN => guaranteeReference.Grn;

		public ZString Currency => guaranteeReference.Currency;

		public ZDecimal ReferenceAmount => guaranteeReference.ReferenceAmount.GetValueOrDefault(0);

		public ZDecimal PercentageOfReferenceAmount => ZDecimal.ParseSafe(guaranteeReference.PercentageOfReferenceAmount, ZDecimal.Zero);

		public ZDecimal GuaranteeAmount => guaranteeReference.GuaranteeAmount.GetValueOrDefault(0);

		public ZInt NumberOfCertificates => ZInt.ParseSafe(guaranteeReference.NumberOfCertificates, ZInt.Zero);

		public ZDate ValidityDate => guaranteeReference.ValidityDate.ConvertToZDate();

		public ZDate InvalidityDate => guaranteeReference.InvalidityDate.ConvertToZDate();

		public ZString InvalidityReasonCode => guaranteeReference.InvalidityReasonCode;

		public ZBool RestrictedUseSuspendedGoods => guaranteeReference.RestrictedUseSuspendedGoods == Flag.Item1;

		public ZString CustomOfficeOfGuaranteeReferenceNumber => guaranteeReference.CustomsOfficeOfGuarantee?.ReferenceNumber ?? ZString.Empty;
	}
}
