using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IJobPackLinePackage
	{
		ZGuid PK { get; }

		ZGuid JPP_JL_PackLine { get; }
	}
}
