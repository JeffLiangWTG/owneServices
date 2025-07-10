using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.IO;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class LocalCartageDataImporter : DataImporter
	{
		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			bool result = false;
			additionalTransactionActions = System.Array.Empty<ITransactionParticipant>();

			TextReader resetReader = null;
			Xsd.XmlInterchange xmlInterchange = null;
			using (TextReaderResetter resetter = TextReaderResetter.New(dataReader))
			{
				xmlInterchange = Xsd.XmlInterchange.ReadInterchangeOnly(resetter.Reader, notifications);
				resetReader = resetter.Reset();
			}

			if (xmlInterchange != null)
			{
				if (xmlInterchange.InterchangeInfo.Target.Type == Xsd.InterchangeInfoTargetType.LocalCartageBooking)
				{
					result = BookingImporter.ImportData(resetReader, attachmentFileName, notifications, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.LocalCartageImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, attachmentFileName));
				}
				else if (xmlInterchange.InterchangeInfo.Target.Type == Xsd.InterchangeInfoTargetType.LocalCartageStatus)
				{
					result = StatusImporter.ImportData(resetReader, attachmentFileName, notifications, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.LocalCartageImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, attachmentFileName));
				}
			}
			return result;
		}

		#region BookingImporter

		LocalCartageBookingImporter BookingImporter
		{
			get
			{
				if (fBookingImporter == null)
				{
					fBookingImporter = GetNewBookingImporter();
				}
				return fBookingImporter;
			}
		}
		LocalCartageBookingImporter fBookingImporter;

#if DEBUG
		protected virtual
#endif
 LocalCartageBookingImporter GetNewBookingImporter()
		{
			return new LocalCartageBookingImporter();
		}

		#endregion

		#region StatusImporter

		LocalCartageStatusImporter StatusImporter
		{
			get
			{
				if (fStatusImporter == null)
				{
					fStatusImporter = GetNewStatusImporter();
				}
				return fStatusImporter;
			}
		}
		LocalCartageStatusImporter fStatusImporter;

#if DEBUG
		protected virtual
#endif
 LocalCartageStatusImporter GetNewStatusImporter()
		{
			return new LocalCartageStatusImporter();
		}

		#endregion
	}
}
