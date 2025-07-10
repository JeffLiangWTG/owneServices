using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;

namespace Enterprise.DataTransfer.Integration
{
	public interface IDataImporter
	{
		void ImportFromEmails(ZDateTime emailsReceivedSince, INotifications notifications, ISourceInfo sourceInfo);
		bool ImportData(string attachmentFileName, INotifications notifications, ISourceInfo sourceInfo);
		bool ImportData(TextReader dataReader, string attachmentFileName, INotifications notifications, ISourceInfo sourceInfo);
		bool ImportDataToFactory(TextReader dataReader, string attachmentFileName, INotifications notifications, ISourceInfo sourceInfo, out ITransactionParticipant[] additionalTransactionActions);
		bool CheckEnvironmentValid(BusinessObjectFactory factory, INotifications notifications);
		BusinessObject[] ImportedBusinessObjects { get; }
	}
}
