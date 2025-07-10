using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Integration
{
	public interface IVisualisationFactoryProvider
	{
		BusinessObjectFactory GetNewEditFactory(string nameForDebug);

		ISecondaryServerConnectionProvider GetSecondaryServerConnectionProvider();
	}
}
