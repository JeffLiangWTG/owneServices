using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalXmlContentFilterApplicator
	{
		void RemoveNonExportedCollections(ITopLevelDataObject dataObject, IUniversalActionInfo info);

		public void RemoveNonExportedCollections(ITopLevelDataObject dataObject, IEDIMessageContentFilter contentFilter);

		public void RemoveNonExportedCollections(ITopLevelDataObject dataObject, IMessageProfile messageProfile);

		bool GetShouldUniversalShipmentExcludeCollection(ITopLevelDataObject dataObject, IUniversalActionInfo info, string dataContext);

		void ExportAttachedDocuments(BusinessObject businessObject, ITopLevelDataObject dataObject, IUniversalActionInfo info);

		void ImportAttachedDocuments(BusinessObject businessObject, ITopLevelDataObject dataObject, IXmlImportLogger logger);

		bool ShouldExcludeEmptyElements(IUniversalActionInfo info);
	}
}
