using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class TraderDataProvider : ITrader
{
	public static TraderDataProvider New(JobDeclaration declaration) => declaration == null ? null : new TraderDataProvider(declaration);

	TraderDataProvider(JobDeclaration declaration)
	{
		this.declaration = declaration;
	}
	readonly JobDeclaration declaration;

	public string CommunicationLanguage => declaration.JE_DeclarationLanguage.ReturnNullIfEmpty()?.ToLowerInvariant();

	public string IdentificationNumber => declaration.DeclarantAddress?.Header?.GetBIDNumber();

	public IContactPerson ContactPerson => contactPerson ??= StaffContactPersonDataProvider.New(declaration.CusAgent);
	IContactPerson contactPerson;
}
