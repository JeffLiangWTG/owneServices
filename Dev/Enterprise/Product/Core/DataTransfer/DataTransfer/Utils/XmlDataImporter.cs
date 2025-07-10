using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer.Business
{
	public class XmlDataImporter : DataImporter
	{
		public XmlDataImporter(IValueObjectDataAdapter adapter)
		{
			this.Adapter = adapter;
		}

		public XmlDataImporter(BusinessObjectFactoryProvider factoryProvider, IValueObjectDataAdapter adapter)
			: base(factoryProvider)
		{
			this.Adapter = adapter;
		}

		public readonly IValueObjectDataAdapter Adapter;

		public virtual bool CanImportXmlCollection(string rootCollectionElementName, string collectionXml)
		{
			return (Adapter.RootCollectionElementName == rootCollectionElementName);
		}

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			Adapter.FileName = attachmentFileName;
			additionalTransactionActions = Array.Empty<ITransactionParticipant>();
			NotificationBuffer buffer = new NotificationBuffer(notifications);

			ImportXml(dataReader, FactoryProvider, buffer);

			return !HasFatalErrors(buffer);
		}

		protected override bool HasFatalErrors(NotificationBuffer buffer)
		{
			return base.HasFatalErrors(buffer) || buffer.ContainsNotificationType(ErrorType.XmlSchemaValidation) || buffer.ContainsNotificationType(ErrorType.DataErrorPreventSave);
		}

		protected virtual void ImportXml(TextReader reader, BusinessObjectFactoryProvider factoryProvider, INotifications notifications)
		{
			ImportedBusinessObjectCollection collection = new ImportedBusinessObjectCollection(factoryProvider.Current);
			XmlValueObjectSerializer serialiser = GetSerializer();
			serialiser.ImportXmlData(((StreamReader)reader).BaseStream, Adapter, collection, factoryProvider, notifications);
			ImportedBusinessObjects = collection.ToArray<BusinessObject>();
			SetEDIInterchange(serialiser.ImportContext);
			AfterImportXml(collection, notifications);
		}

		protected virtual void AfterImportXml(IBusinessObjectCollection bizObjsImported, INotifications notifications)
		{
		}

		internal protected virtual XmlValueObjectSerializer GetSerializer()
		{
			return new XmlValueObjectSerializer(Adapter.ValueObjectType);
		}

#if DEBUG
		public
#else
		protected
#endif
		class ImportedBusinessObjectCollection : BusinessObjectCollection<BusinessObject>
		{
			public ImportedBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pK)
			{
				return typeof(MasterFiles.Business.OrgHeader);
			}
		}
	}
}
