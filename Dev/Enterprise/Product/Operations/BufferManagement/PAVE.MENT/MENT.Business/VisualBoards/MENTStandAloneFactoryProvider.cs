using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTStandAloneFactoryProvider : IVisualisationFactoryProvider
	{
		public BusinessObjectFactory GetNewEditFactory(string nameForDebug)
		{
			return new BusinessObjectFactory { NameForDebugging = nameForDebug };
		}

		public ISecondaryServerConnectionProvider GetSecondaryServerConnectionProvider()
		{
			return SecondaryServerConnectionProviderProvider.GetProvider();
		}
	}
}
