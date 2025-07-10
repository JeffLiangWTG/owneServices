using Enterprise.Customs.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Documents
{
	public class CustomsDocDataObjectUXmlWriter : ICustomsDocDataObjectUXmlWriter
	{
		public ITopLevelDataObject GetDataObject(IDataObjectWriterStrategy dataObjectWriterStrategy, DocumentVisualizer.Core.IDocument document)
		{
			var manager = new Freight.Forwarding.Documents.DataTransfer.DataWritingManager(dataObjectWriterStrategy);
			var dataContext = document?.DataContext ?? string.Empty;
			var docDataObject = document?.Data?.Value as DocDataObject;

			switch (dataContext)
			{
				case DataContext.FRPortsTrackingRequestTRC:
					if (docDataObject is DemandeDeTracing demandeDeTracing)
					{
						var writer = new DemandeDeTracingDataObjectWriter(manager);
						return writer.GetDataObject(demandeDeTracing);
					}
					break;
			}

			return null;
		}
	}
}
