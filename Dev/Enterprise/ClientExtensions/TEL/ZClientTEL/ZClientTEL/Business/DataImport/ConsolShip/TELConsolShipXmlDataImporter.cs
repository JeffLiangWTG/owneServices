using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.TEL.Definition;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.TEL.Import
{
	internal class TELConsolShipXmlDataImporter : DataImporter
	{
		public override bool CheckEnvironmentValid(CargoWise.EntityFramework.BusinessObjectFactory factory, INotifications notifications)
		{
			ZBool result = ZBool.True;

			if (TELDataRegistry.Instance.ConsolShipImportNotificationGroupPK.IsEmpty)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, "Please set the Registry > TEL Client Extensions > Import of Consols and Shipments > Notification Group"));
				result = ZBool.False;
			}

			if (TELDataRegistry.Instance.ConsolShipManifestEmailSubjectIdentifier.IsEmpty)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, "Please set the Registry > TEL Client Extensions > Import of Consols and Shipments > Email Subject Identifier"));
				result = ZBool.False;
			}

			return result;
		}

		protected override bool ImportDataToFactoryCore(TextReader reader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			additionalTransactionActions = System.Array.Empty<ITransactionParticipant>();
			NotificationBuffer buffer = new NotificationBuffer(notifications);

			TELConsolShipXmlDocument document = new TELConsolShipXmlDocument((StreamReader)reader, buffer);
			IValueObject[] externalXmlManifests = document.ConvertToValueObjects();

			Xsd.Consol[] consols = new TELConsolShipXmlConverter().Convert(externalXmlManifests);

			for (int i = 0; i < consols.Length; i++)
			{
				Xsd.Consol xsdConsol = consols[i];

				Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
				interchange.InterchangeInfo.EDIOrganisation.OwnerCode = TELConstants.ServiceTask.Import.TELSendingAgentCode;

				ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider.Current, interchange, notifications);
				Adapter.CreateOrUpdateFromValueObject(xsdConsol, importContext);
				FactoryProvider.Current.Save();
			}
			return true;
		}

		#region Implementation

		ForwardingConsolValueObjectDataAdapter Adapter
		{
			get { return adapter ?? (adapter = new ForwardingConsolValueObjectDataAdapter()); }
		}
		ForwardingConsolValueObjectDataAdapter adapter;
		#endregion
	}
}

