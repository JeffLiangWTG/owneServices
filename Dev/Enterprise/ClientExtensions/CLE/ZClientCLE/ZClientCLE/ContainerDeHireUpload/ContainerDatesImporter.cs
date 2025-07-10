using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.CLE
{
	internal class ContainerDatesImporter : DataImporter
	{
		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			additionalTransactionActions = System.Array.Empty<ITransactionParticipant>();

			int processedLines = 0;
			string currentLine;

			NotificationBuffer notify = new NotificationBuffer(notifications);
			notify.Notify(new InfoNotification("Starting to process the lines..."));

			while ((currentLine = dataReader.ReadLine()) != null)
			{
				ContainerDatesFlatFileDataRow row = new ContainerDatesFlatFileDataRow(currentLine);
				RowImporter.Import(row, notify, ExceptionBuffer);
				processedLines++;
				if (processedLines > 100)
				{
					processedLines = 0;
					FactoryProvider.SaveCurrentAndCreateNew();
				}
			}
			return !notify.HasErrors;
		}

		protected override void OnAfterImportData(NotificationBuffer buffer, bool sucessfullyImported)
		{
			ExceptionBuffer.CreateExceptionReport(buffer);
		}

		#region RowImporter

		ContainerDatesFlatFileDataRowImporter RowImporter
		{
			get { return rowImporter ?? (rowImporter = new ContainerDatesFlatFileDataRowImporter(FactoryProvider)); }
		}
		ContainerDatesFlatFileDataRowImporter rowImporter;

		#endregion

		#region ExceptionBuffer

		ContainerDatesExceptionBuffer ExceptionBuffer
		{
			get { return exceptionBuffer ?? (exceptionBuffer = new ContainerDatesExceptionBuffer()); }
		}
		ContainerDatesExceptionBuffer exceptionBuffer;

		#endregion
	}
}
