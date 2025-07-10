using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;

namespace Enterprise.DataTransfer.GUI.Testing
{
	sealed class DataImporterForTest : DataImporter
	{
		protected override bool ImportDataToFactoryCore(
			TextReader dataReader, string attachmentFileName,
			INotifications notifications, out ITransactionParticipant[] transactionActions)
		{
			if (DoOnImport != null)
			{
				DoOnImport(this);
			}

			transactionActions = null;
			return false;
		}

		public Action<DataImporterForTest> DoOnImport { get; set; }
		protected override IMailFilter GetMailItemFilter() => QueryMailFilter.AllQueuedItems_ForTesting;
	}
}
