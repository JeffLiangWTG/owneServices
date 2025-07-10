using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class EdecDeclarantDataProvider : IEdecDeclarant
{
	public static EdecDeclarantDataProvider New(JobDeclaration declaration) => declaration == null ? null : new EdecDeclarantDataProvider(declaration);

	public EdecDeclarantDataProvider(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
		address = declaration.Forwarder?.MainAddress ?? declaration.DeclarantAddress;
	}
	readonly JobDeclaration declaration;
	readonly OrgAddress address;

	const string UndefinedTraderIdentificationNumber = "-";
	const string UndefinedDeclarantNumber = "0";

	public string TraderIdentificationNumber => (GlbCompany.CurrentCompany?.GC_CustomsRegistrationNo).ReturnDefaultValueIfNullOrEmpty(UndefinedTraderIdentificationNumber);

	public string DeclarantNumber => (declaration.CHDPassword?.GP_UserID).ReturnDefaultValueIfNullOrEmpty(UndefinedDeclarantNumber);

	public string Name => address?.Header?.OH_FullName ?? string.Empty;

	public string Street => address?.OA_Address1 ?? string.Empty;

	public string PostalCode => address?.OA_PostCode ?? string.Empty;

	public string City => address?.OA_City ?? string.Empty;

	public string Country => address?.OA_RN_NKCountryCode ?? string.Empty;
}
