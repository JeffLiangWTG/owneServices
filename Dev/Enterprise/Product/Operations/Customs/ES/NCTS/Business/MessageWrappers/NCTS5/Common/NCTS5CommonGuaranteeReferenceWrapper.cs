using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonGuaranteeReferenceWrapper : INCTSCommonGuaranteeReference
	{
		public NCTS5CommonGuaranteeReferenceWrapper(NctsGuarantee guarantee, ZShort seqNum)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));

			SequenceNumber = seqNum.ToString();
		}
		readonly NctsGuarantee guarantee;

		const string ESCode = "ES";

		public ZString SequenceNumber { get; }

		public ZString GRN => guarantee.PW_BondNumber;

		public ZString AccessCode => guarantee.PW_BondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher
										|| ((guarantee.PW_BondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee
											|| guarantee.PW_BondType == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver)
											&& !guarantee.PW_BondNumber.StartsWith(ESCode))
									? guarantee.PW_Password
									: ZString.Empty;

		public ZDecimal AmountToBeCovered => guarantee.PW_BondAmount;
	}
}
