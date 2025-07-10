using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.Data
{
	public class CTIDataImporter : DataImporter
	{
		protected override bool ImportDataToFactoryCore(
			TextReader data, string attachmentFileName,
			INotifications notify, out ITransactionParticipant[] additionalTransactionActions)
		{
			additionalTransactionActions = System.Array.Empty<ITransactionParticipant>();
			CarotransManifestDataFileReader reader = new CarotransManifestDataFileReader(data.ReadToEnd());

			IValueObjectDataAdapter adapter = new ForwardingConsolValueObjectDataAdapter();

			MainFormConsolCollection collection = new MainFormConsolCollection(FactoryProvider.Current);
			Xsd.ConsolCollection consols = reader.ProcessImportManifest();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();

			foreach (Xsd.Consol consol in consols)
			{
				ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, interchange, notify);
				adapter.CreateOrUpdateFromValueObject(consol, importContext);
			}

			return true;
		}
	}
}
