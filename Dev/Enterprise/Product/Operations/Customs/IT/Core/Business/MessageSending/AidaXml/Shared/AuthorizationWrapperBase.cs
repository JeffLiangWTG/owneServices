using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public abstract class AuthorizationWrapperBase : IAuthorization
{
	protected AuthorizationWrapperBase(EU.Business.CusAuthorizationUsage authorization, Func<EU.Business.CusAuthorizationUsage, string> authorizationTypeFunc)
	{
		Argument.NotNull(authorization, nameof(authorization));
		Argument.NotNull(authorizationTypeFunc, nameof(authorizationTypeFunc));

		lazyIdentificationNumber = new Lazy<string>(() => authorization.Owner.GetEoriCode(countryCode: true));
		lazyAuthorizationType = new Lazy<string>(() => authorizationTypeFunc(authorization));
		lazyReferenceNumber = new Lazy<string>(() => authorization.AGC_Number);
	}

	readonly Lazy<string> lazyIdentificationNumber;
	readonly Lazy<string> lazyAuthorizationType;
	readonly Lazy<string> lazyReferenceNumber;

	string IAuthorization.IdentificationNumber => lazyIdentificationNumber.Value;

	string IAuthorization.AuthorizationType => lazyAuthorizationType.Value;

	string IAuthorization.ReferenceNumber => lazyReferenceNumber.Value;
}
