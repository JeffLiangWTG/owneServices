using System.Collections;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class JXCMessageImporter : DataImporter
	{
		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			bool result;

			JXCRecord[] records = ReadRecordsFromReader(dataReader);
			additionalTransactionActions = System.Array.Empty<ITransactionParticipant>();
			JXCMessageProcessor messageProcessor = MessageProcessorFactory.NewProcessor(records, notifications);
			if (messageProcessor == null)
			{
				result = false;
				fLastMessageCategoryImported = JXCConstants.MessageCategories.Unknown;
			}
			else
			{
				result = messageProcessor.ProcessRecords(FactoryProvider, notifications);
				fLastMessageCategoryImported = JXCConstants.MessageTypes.GetMessageCategoryFromMessageType(messageProcessor.MessageType);
			}

			return result;
		}

		protected override void OnAfterImportData(NotificationBuffer buffer, bool sucessfullyImported)
		{
			base.OnAfterImportData(buffer, sucessfullyImported);
			if (!sucessfullyImported)
			{
				DisposeNewlyCreatedJobs();
			}
		}

		void DisposeNewlyCreatedJobs()
		{
			ZQuery filter = new ZQuery();
			filter.FetchOnlyFromLocalCache = true;
			Job[] jobs = (Job[])FactoryProvider.Current.Load(typeof(Job), filter);
			foreach (Job job in jobs)
			{
				job.Dispose();
			}
		}

		public JXCConstants.MessageCategories LastMessageCategoryImported
		{
			get { return fLastMessageCategoryImported; }
		}

		#region Implementation

		JXCRecord[] ReadRecordsFromReader(TextReader dataReader)
		{
			ArrayList result = new ArrayList();

			string line = dataReader.ReadLine();
			while (line != null)
			{
				if (!string.IsNullOrEmpty(line))
				{
					JXCRecord record = RecordFactory.NewRecord(line);
					if (record != null)
					{
						result.Add(record);
					}
				}
				line = dataReader.ReadLine();
			}

			return (JXCRecord[])result.ToArray(typeof(JXCRecord));
		}

		JXCMessageProcessorFactory MessageProcessorFactory
		{
			get
			{
				if (fMessageProcessorFactory == null)
				{
					fMessageProcessorFactory = GetNewJXCMessageProcessorFactory();
				}
				return fMessageProcessorFactory;
			}
		}

		JXCRecordFactory RecordFactory
		{
			get
			{
				if (fRecordFactory == null)
				{
					fRecordFactory = new JXCRecordFactory();
				}
				return fRecordFactory;
			}
		}

		protected virtual
 JXCMessageProcessorFactory GetNewJXCMessageProcessorFactory()
		{
			return new JXCMessageProcessorFactory();
		}

		JXCMessageProcessorFactory fMessageProcessorFactory;
		JXCRecordFactory fRecordFactory;
		protected JXCConstants.MessageCategories fLastMessageCategoryImported;

		#endregion
	}
}
