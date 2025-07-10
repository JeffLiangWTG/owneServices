using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	public interface IFieldDefaultingStrategy : ICodeDescription
	{
		FieldType DetailFieldType { get; }
		int DetailMaxLength { get; }

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		IList GetBoundCollection(BusinessObjectFactory factory);
		IZType GetDefaultValue(string detail);
	}
}
