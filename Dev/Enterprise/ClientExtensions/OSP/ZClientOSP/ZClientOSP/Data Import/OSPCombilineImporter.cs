using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.OSP.Data_Import
{
	public class OSPCombilineImporter : DataImporter
	{
		public OSPCombilineImporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		ForwardingConsolValueObjectDataAdapter Adapter
		{
			get { return adapter ?? new ForwardingConsolValueObjectDataAdapter(); }
		}
		readonly ForwardingConsolValueObjectDataAdapter adapter;

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider.Current, interchange, notifications);
			OSPCombilineConverter converter = new OSPCombilineConverter(interchange, FactoryProvider.Current);
			Xsd.Consol consol = converter.Convert(dataReader, notifications);

			Adapter.CreateOrUpdateFromValueObject(consol, importContext);
			FactoryProvider.SaveCurrentAndCreateNew();

			additionalTransactionActions = null;
			return true;
		}
	}
}
