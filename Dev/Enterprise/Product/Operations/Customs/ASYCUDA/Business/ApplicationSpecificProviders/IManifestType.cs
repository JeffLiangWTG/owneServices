using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface IManifestType : ICodeDescription
	{
		IEnumerable<string> ApplicableTransportModes { get; }
		IEnumerable<string> ApplicableManifestStyles { get; }
		MessageLevel MessageLevel { get; }
		CodeDescriptionPairList ManifestNatures { get; }
		bool Enabled { get; }
	}
}
