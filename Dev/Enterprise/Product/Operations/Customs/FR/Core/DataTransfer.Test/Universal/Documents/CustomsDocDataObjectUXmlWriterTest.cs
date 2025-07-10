using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.UniversalDataBuss.DataObjects;
using DataContext = Enterprise.Freight.Forwarding.Documents.DataContext;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Documents.Testing
{
	public class CustomsDocDataObjectUXmlWriterTest : TestCaseWithFactory
	{
		public void TestGetDataObject()
		{
			var declaration = Factory.New<JobDeclaration>();

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "Container1";
			var commonContainer = Factory.New<CommonContainer>();
			commonContainer.JC_IsNonOperativeReefer = true;
			cusContainer.CO_JC = commonContainer.PK;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var carrierWrapper = new DemandeDeTracingBuilder(entryHeader.TRCDetailsProvider, entryHeader.Containers.Select(x => x.JobContainer).ToArray(), DemandeDeTracingDirection.Import).Build();

			var writer = new CustomsDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.Instance, CreateMockDocument(carrierWrapper, DataContext.FRPortsTrackingRequestTRC)) as UniversalDataBuss.DataObjects.Universal.Shipment;

			AssertNotNull("DataObject has been produced", dataObject);
		}

		IDocument CreateMockDocument(DocDataObject docDataObject, string dataContext)
		{
			var data = docDataObject.MakeDynamic();
			var document = new DummyDocument();
			document.Data = data;
			document.DataContext = dataContext;

			return document;
		}
	}
}
