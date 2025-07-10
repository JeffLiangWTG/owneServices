using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public interface ICustomProperty
	{
		string Identifier { get; }
		object GetValue(BusinessObject parent);
		bool TrySetValue(BusinessObject parent, object value);
		void Validate(BusinessObject parent);

		IEnumerable<ICustomProperty> RelatedProperties { get; }
		DynamicBusinessObjectProperty Info { get; }
		ICustomColumnDefinition CustomColumnDefinition { get; }
		bool IsDeleted { get; }
	}

	public interface ICustomPropertyContainer
	{
		IEnumerable<ICustomProperty> CustomProperties { get; }
	}
}
