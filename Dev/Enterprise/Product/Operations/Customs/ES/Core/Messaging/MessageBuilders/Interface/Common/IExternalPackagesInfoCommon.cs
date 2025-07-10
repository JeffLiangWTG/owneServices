using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IExternalPackagesInfoCommon
	{
		ZLong NumberOfPackages { get; }
		ZString PackageType { get; }
		IReadOnlyCollection<ZString> Tags { get; }
	}
}
