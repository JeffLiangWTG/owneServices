using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC228CGuaranteeReferenceProvider
	{
		public static CC228CGuaranteeReferenceProvider Create(GuaranteeReferenceType10 xmlObject) => new CC228CGuaranteeReferenceProvider(xmlObject);

		public CC228CGuaranteeReferenceProvider(GuaranteeReferenceType10 xmlObject)
		{
			guaranteeReference = Argument.NotNull(xmlObject, nameof(xmlObject));
		}
		readonly GuaranteeReferenceType10 guaranteeReference;

		public ZInt SequenceNumber => ZInt.ParseSafe(guaranteeReference.SequenceNumber, ZInt.Zero);

		public ZString GRN => guaranteeReference.Grn;

		public ZString Currency => guaranteeReference.Currency;

		public ZDecimal GuaranteeAmount => guaranteeReference.GuaranteeAmount;

		public ZDateTime InvalidityDate => guaranteeReference.InvalidityDate == DateTime.MinValue ? ZDateTime.Empty : guaranteeReference.InvalidityDate;

		public ZString CustomOfficeOfGuaranteeReferenceNumber => guaranteeReference.CustomsOfficeOfGuarantee?.ReferenceNumber ?? ZString.Empty;

		public ZDateTime LiabilityLiberationDate => guaranteeReference.LiabilityLiberationDate == DateTime.MinValue ? ZDateTime.Empty : guaranteeReference.LiabilityLiberationDate;
	}
}
