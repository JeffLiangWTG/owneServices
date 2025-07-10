using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business;

public class AuthorizationHeaderProvider
{
	public AuthorizationHeaderProvider(IAuthorizationHeaderDataProvider authorisationDataProvider, BusinessObjectFactory factory)
	{
		this.authorisationDataProvider = Argument.NotNull(authorisationDataProvider, nameof(authorisationDataProvider));
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	readonly IAuthorizationHeaderDataProvider authorisationDataProvider;
	readonly BusinessObjectFactory factory;

	public CusAuthorisationHeader Authorization
	{
		get
		{
			if (authorizationCachedProperty == null)
			{
				authorizationCachedProperty = new CachedProperty<CusAuthorisationHeader>(factory, () => GetAuthorizationHeader());
			}
			return authorizationCachedProperty.Value;
		}
	}
	CachedProperty<CusAuthorisationHeader> authorizationCachedProperty;

	CusAuthorisationHeader GetAuthorizationHeader()
	{
		var authorizationNumber = authorisationDataProvider.AuthorizationNumber;
		var authorizationTypes = authorisationDataProvider.AuthorizationTypes;
		var holderPK = authorisationDataProvider.HolderPk;

		if (!authorizationNumber.IsEmpty && authorizationTypes.Any() && !holderPK.IsEmpty)
		{
			return CusAuthorisationHeader.Loader
				.GetAuthorisations(factory, Core.Constants.CountryCodes.Italy, authorizationTypes.ToArray(), ZDate.Today, holderPK)
				.FirstOrDefault(x => x.CPH_Number == authorizationNumber);
		}
		return null;
	}
}
