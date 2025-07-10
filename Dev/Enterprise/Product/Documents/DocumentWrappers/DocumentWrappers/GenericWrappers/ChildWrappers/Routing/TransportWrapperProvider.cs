using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Integration;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class TransportWrapperProvider : ITransportWrapperProvider
	{
		public string GetTransportReference(BusinessObject parent, BusinessObjectFactory factory)
		{
			var result = string.Empty;

			if (parent is BaseJobDeclaration declaration)
			{
				var declarationWrapper = FreightWrapperFromDeclaration.New(declaration, factory);
				var mostInterestingRoutes = declarationWrapper.ConsolRoutes["mostinteresting"];
				if (mostInterestingRoutes != null)
				{
					result = mostInterestingRoutes.Transport.Reference;
				}
			}

			return result;
		}
	}
}
