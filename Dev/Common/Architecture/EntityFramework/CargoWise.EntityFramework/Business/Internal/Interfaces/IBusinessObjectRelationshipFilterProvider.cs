using System;

namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectRelationshipFilterProvider
	{
		ZQuery GetFilter(Type relatedBusinesObjectType);
	}
}
