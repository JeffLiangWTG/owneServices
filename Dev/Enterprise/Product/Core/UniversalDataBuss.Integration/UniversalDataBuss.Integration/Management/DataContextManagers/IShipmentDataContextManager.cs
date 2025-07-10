using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IShipmentDataContextManager : IDataContextManager
	{
		bool UseIncomingShipmentData(ITopLevelDataObject topLevelDataObject, IXmlImportLogger logger, IUniversalObjectFactory factory);

		IKeysResult GetKeysForBlockingParallelImport(ITopLevelDataObject topLevelDataObject, IXmlImportLogger logger, IUniversalObjectFactory factory);

		bool ManagesShipments { get; }

		ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager);

		void DefaultDataTargetFromRecipientRole(IDataContextDataObject dataContext, IXmlSessionTracker importSessionLogger);

		BusinessObject FindExistingBusinessObjectForIncomingShipment(ITopLevelDataObject topLevelDataObject, IXmlImportLogger logger, IUniversalObjectFactory factory);
	}

	public interface IShipmentDataContextManagerInternal
	{
		ITopLevelDataObjectReader GetShipmentDataObjectReader(ITopLevelDataObject universalShipment, IXmlImportLogger logger, IUniversalObjectFactory factory);
	}
}
