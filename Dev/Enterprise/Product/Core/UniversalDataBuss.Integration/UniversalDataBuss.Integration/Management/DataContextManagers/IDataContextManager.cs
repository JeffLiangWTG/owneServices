using System;
using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataContextManager : IEntityID
	{
		void Init(BusinessObject parent);

		string DefaultOutputDirectory { get; }

		IUniversalXmlSchema SchemaOverride { get; }

		BusinessObject LoadBusinessObjectFromDataSource(ITopLevelDataObject topLevelDataObject, IDataSourceDataObject dataSource, BusinessObjectFactory factory, IXmlImportLogger logger);

		BusinessObject LoadBusinessObjectFromDataTarget(ITopLevelDataObject topLevelDataObject, IDataTargetDataObject dataTarget, BusinessObjectFactory factory, IXmlImportLogger logger);

		BusinessObject[] LoadBusinessObjectsFromDataTarget(ITopLevelDataObject topLevelDataObject, IDataTargetDataObject dataTarget, BusinessObjectFactory factory, IXmlImportLogger logger);

		/// <summary>
		/// The Type of the top level business object created/updated.
		/// E.g. typeof(ForwardingShipment) for the ForwardingShipmentDataContextManager
		/// </summary>
		Type TopLevelBusinessObjectType { get; }
	}
}
