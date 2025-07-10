using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public class FiscalReferenceWrapper : IFiscalReference
{
	public FiscalReferenceWrapper(CusFiscalReference fiscalReference)
	{
		this.fiscalReference = Argument.NotNull(fiscalReference, nameof(fiscalReference));
	}

	readonly CusFiscalReference fiscalReference;

	string IFiscalReference.IdentificationNumber => fiscalReference.CFR_Reference;

	string IFiscalReference.Role => fiscalReference.CFR_Code;
}
