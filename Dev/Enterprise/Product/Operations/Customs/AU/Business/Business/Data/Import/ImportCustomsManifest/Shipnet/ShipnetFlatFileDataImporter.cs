using CargoWise.ComponentModel;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ShipnetFlatFileDataImporter : FlatFileDataImporter
	{
		public ShipnetFlatFileDataImporter(CusSeaManTranHead tranHead)
			: base(tranHead)
		{
			this.tranHead = tranHead;
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			CusImportManifestValueObjectDataAdapter dataAdapter = new CusImportManifestValueObjectDataAdapter(false);
			ValueObjectImportContext context = new ValueObjectImportContext(FactoryProvider, notifications);
			dataAdapter.ImportFromValueObject(tranHead, (Xsd.CusImportManifest)xSD, context);

			tranHead.OceanBills.Load();
			foreach (CusSeaManArrivalPort port in tranHead.Arrivals)
			{
				port.CargoLines.Load();
			}
			return true;
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.CusImportManifest();
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new ShipnetFlatFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new ShipnetFlatFileFormat(); }
		}

		readonly CusSeaManTranHead tranHead;
	}
}
