using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.FSH.TsManifest
{
	public class FortuneShippingFlatFileDataImporter : FlatFileDataImporter
	{
		public FortuneShippingFlatFileDataImporter()
		{
		}

		internal protected FortuneShippingFlatFileDataImporter(BusinessObjectFactory factory) : base(new SingleBusinessObjectFactoryProvider(factory))
		{
		}

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notify, out ITransactionParticipant[] additionalTransactionActions)
		{
			base.ImportDataToFactoryCore(dataReader, attachmentFileName, notify, out additionalTransactionActions);
			return true;
		}
		internal bool InternalImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notify, out ITransactionParticipant[] additionalTransactionActions) => ImportDataToFactoryCore(dataReader, attachmentFileName, notify, out additionalTransactionActions);

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			IValueObjectDataAdapter dataAdapter = new ForwardingConsolValueObjectDataAdapter();
			Xsd.ConsolCollection consols = (Xsd.ConsolCollection)xSD;

			OrganisationMatchingWithoutCreatingTemporaryOrgs organisationMatching = new OrganisationMatchingWithoutCreatingTemporaryOrgs(FactoryProvider, Xsd.XmlInterchange.Empty, notifications);
			ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, Xsd.XmlInterchange.Empty, organisationMatching, notifications);

			int count = 0;
			foreach (Xsd.Consol consol in consols)
			{
				dataAdapter.CreateOrUpdateFromValueObject(consol, importContext);
				if ((count % 10) == 0)
				{
					FactoryProvider.SaveCurrentAndCreateNew();
				}
			}
			FactoryProvider.SaveCurrentAndCreateNew();

			return false;
		}
		internal bool InternalExtractToDataAdapter(IValueObject xSD, INotifications notifications) => ExtractToDataAdapter(xSD, notifications);

		class OrganisationMatchingWithoutCreatingTemporaryOrgs : OrganisationMatching
		{
			public OrganisationMatchingWithoutCreatingTemporaryOrgs(BusinessObjectFactoryProvider factoryProvider, Xsd.XmlInterchange interchange, INotifications notifications) : base(factoryProvider, interchange, notifications)
			{
			}

			protected override bool CreateTemporaryOrUnmatchOrgIfNoMatchFound
			{
				get { return false; }
			}
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.ConsolCollection();
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new FortuneShippingFlatFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new FortuneShippingFlatFileFormat(); }
		}
	}
}
