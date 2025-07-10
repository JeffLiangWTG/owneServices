using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Business;

public class LocalReferenceNumberStrategy : IMessageNumberStrategy
{
	public LocalReferenceNumberStrategy(IDbConnected fountainConnection)
	{
		this.fountainConnection = Argument.NotNull(fountainConnection, nameof(fountainConnection));
		licenseCode = GetLicenseCode();
	}

	readonly IDbConnected fountainConnection;
	readonly string licenseCode;

	public string GetMessageReferenceNumber()
	{
		var year = ZDate.Today.Year.ToString("0000");
		var numberFountain = GetNumberFountain(year);
		var messageReferenceNumber = numberFountain.GetNextFormatted(fountainConnection);

		return FormattableString.Invariant($"{year}{licenseCode}{messageReferenceNumber}");
	}

	INumberFountainProxy GetNumberFountain(string year) => Env.NumberFountains.ITMessageLocalReferenceNumber(year);

	static string GetLicenseCode()
	{
		var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
		return registrationKey.EnterpriseCode + registrationKey.ServerCode;
	}
}
