using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.CarbonEmissions.Integration;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CO2eEmissionWrapperCollection : GenericWrapperCollection<CO2eEmissionWrapper>
	{
		public CO2eEmissionWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CO2eEmissionWrapperCollection(ICO2eLegBasedSupporter supporter, BusinessObjectFactory factory)
			: base(factory)
		{
			if (supporter == null)
			{
				return;
			}

			supporter.Legs?.Cast<ICO2eLegProvider>().ForEach(leg => Add(new CO2eEmissionWrapper(supporter, leg, factory)));
		}
	}
}
