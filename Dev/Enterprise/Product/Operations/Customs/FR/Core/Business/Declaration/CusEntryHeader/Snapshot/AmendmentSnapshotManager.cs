using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.FR.Business.Declaration;

public class AmendmentSnapshotManager : Customs.Business.AmendmentSnapshotManager
{
	public AmendmentSnapshotManager(CusEntryHeader entryHeader, ZString messageType) : base(entryHeader, messageType)
	{
	}

	protected override ZInt GetVersionNumberCore()
	{
		var entrySeqNumber = ((CusEntryHeader)entryHeader).CH_SequenceNumber;
		return entrySeqNumber.IsEmpty ? 1 : entrySeqNumber ;
	}

	protected override Stream TakeSnapshotCore()
	{
		var shipment = entryHeader.Declaration.GetUniversalShipment();
		foreach (var invoice in shipment.CommercialInfo.CommercialInvoiceCollection)
		{
			invoice.CommercialInvoiceLineCollection.Content = CollectionContent.Complete;
		}
		var selfDataTarget = DataContextFactory.NewDataTarget();
		var dataSource = shipment.DataContext.DataSourceCollection.First();
		selfDataTarget.Type = dataSource.Type;
		selfDataTarget.Key = dataSource.Key;
		shipment.DataContext.AddDataTarget(selfDataTarget);

		var stream = (SubStreamableStream)new MemoryStream();
		ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

		return stream;
	}

	protected override void RestoreEntryHeaderCore(SnapshotRevertingStrategy strategy, IXmlImportLogger logger)
	{
		var universalFactory = new UniversalObjectFactory();
		var uxmlReaderLogger = RestoreEntryHeaderInMemoryOnly(strategy, universalFactory);
		universalFactory.SaveAtEndOfImport(uxmlReaderLogger);
	}

	internal XmlSessionTracker RestoreEntryHeaderInMemoryOnly(SnapshotRevertingStrategy strategy, UniversalObjectFactory universalFactory)
	{
		var factory = entryHeader.Factory;
		var provider = factory.GetUniversalCustomsDataObjectProvider(Core.Constants.CountryCodes.France);
		using var reader = LastSnapshot.GetCES_SnapshotXmlReader();
		var result = reader.ReadToEnd();
		var shipment = new Shipment();
		using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(result)))
		{
			ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, Logger);
		}
		var uxmlReaderLogger = new XmlSessionTracker(new SimpleLogger());
		var universalShipmentReader = provider.GetNewJobDeclarationDataObjectReader(shipment, uxmlReaderLogger, universalFactory, null);
		var snapshotReader = (ISnapshotReader)universalShipmentReader;
		using (snapshotReader.TemporarilyMarkJobDeclarationReaderIsBeingUsedForSnapshotReverting((CusEntryHeader)entryHeader, strategy, Logger))
		{
			universalShipmentReader.ReadIntoTopLevelBusinessObject();
		}
		return uxmlReaderLogger;
	}
}
