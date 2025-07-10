using System;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public interface IDependentBusinessObjectCollection : IBusinessObjectCollectionInternals, IBusinessObjectCollection
	{
		Type ChildType { get; }
		BusinessObject Master { get; }
		SchemaGuidColumn FKSchemaColumnInDependent { get; }
		void Add(BusinessObject businessObject);
	}
}
