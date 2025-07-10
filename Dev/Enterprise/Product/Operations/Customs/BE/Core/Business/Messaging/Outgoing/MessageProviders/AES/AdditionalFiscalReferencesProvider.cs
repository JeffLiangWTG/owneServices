using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class AdditionalFiscalReferencesProvider : IAdditionalFiscalReference
{
	public AdditionalFiscalReferencesProvider(CusFiscalReference fiscalReference, int sequence)
	{
		this.fiscalReference = Argument.NotNull(fiscalReference, nameof(fiscalReference));
		SequenceNumber = sequence.ToString();
	}

	readonly CusFiscalReference fiscalReference;

	public string Role => fiscalReference.CFR_Code;

	public string VatIdentificationNumber => fiscalReference.Owner?.GetUnprefixedEORI();

	public string SequenceNumber { get; }
}
