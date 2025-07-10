using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IInternalPackageIdentificationCommon
	{
		ZString Tag { get; }
		ZString ElementsType { get; }
		ZLong NumberOfElements { get; }
	}

	public interface IInternalPackagesInfoCommon
	{
		IReadOnlyCollection<IInternalPackageIdentificationCommon> Packages { get; }
	}
}
