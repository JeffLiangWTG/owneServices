using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class GuaranteeReferenceProvider : IGuaranteeReference
{
	readonly CommonGuarantee guarantee;
	public GuaranteeReferenceProvider(CommonGuarantee guarantee, int sequence)
	{
		this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		this.SequenceNumber = sequence;
	}

	public int SequenceNumber { get; }

	public string GRN => guarantee.PW_BondNumber;

	public string AccessCode => guarantee.PW_Password;

	public decimal AmountToBeCovered => guarantee.PW_BondAmount;

	public string Currency => guarantee.PW_RX_NKCurrency;

	public string CCQualifier => Constants.Qualifiers.NietVanToepassing;

	public string OtherGuaranteeReference => guarantee.PW_GuaranteeDescription;

	public string CustomsOfficeOfGuaranteeReferenceNumber => guarantee.PW_BondFiledPort;
}
