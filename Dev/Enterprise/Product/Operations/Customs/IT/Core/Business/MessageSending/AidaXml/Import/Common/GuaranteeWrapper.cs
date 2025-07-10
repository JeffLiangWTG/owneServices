using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public class GuaranteeWrapper : IGuarantee
{
	public GuaranteeWrapper(GuaranteeForEntryInstruction guaranteeForEntryInstruction)
	{
		this.guaranteeForEntryInstruction = Argument.NotNull(guaranteeForEntryInstruction, nameof(guaranteeForEntryInstruction));
	}

	readonly GuaranteeForEntryInstruction guaranteeForEntryInstruction;

	string IGuarantee.AccessCode => guaranteeForEntryInstruction.PW_Password;

	string IGuarantee.AdditionalReference => guaranteeForEntryInstruction.PW_BondNumber2;

	string IGuarantee.CurrencyCode => guaranteeForEntryInstruction.PW_RX_NKCurrency;

	string IGuarantee.CustomsOffice => guaranteeForEntryInstruction.PW_BondFiledPort;

	decimal IGuarantee.DutyAmount => guaranteeForEntryInstruction.PW_BondAmount;

	string IGuarantee.Grn => guaranteeForEntryInstruction.PW_BondNumber;

	string IGuarantee.HolderIdentification => guaranteeForEntryInstruction.PW_HolderIdentification;
}
