using CargoWise.Customs.IT.MessageContracts.Declaration.Import;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class FiscalReferenceWrapper : IFiscalReference
{
	public FiscalReferenceWrapper(string identificationNumber, string role)
	{
		this.identificationNumber = identificationNumber;
		this.role = role;
	}

	readonly string identificationNumber;
	readonly string role;

	public string IdentificationNumber => identificationNumber;

	public string Role => role;
}
