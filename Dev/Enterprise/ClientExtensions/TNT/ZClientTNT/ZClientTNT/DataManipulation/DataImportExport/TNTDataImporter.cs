using System.Collections;
using System.IO;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TNT
{
	public class TNTDataImporter : DataImporter
	{
		public TNTDataImporter()
			: base()
		{ }

		public TNTDataImporter(bool isAutoImportProcess)
			: base()
		{
			IsAutoImportProcess = isAutoImportProcess;
		}

		public bool IsAutoImportProcess
		{
			get;
			private set;
		}

		protected override void OnAfterImportData(NotificationBuffer buffer, bool sucessfullyImported)
		{
			base.OnAfterImportData(buffer, sucessfullyImported);
			if (buffer.HasErrors)
			{
				buffer.SendEmail(this.OutgoingMailManager, TNTConstants.ErrorNotificationEmailSubject, TNTConstants.DataImportNotificationGroupCode);
			}
		}

		protected override bool ImportDataToFactoryCore(TextReader dataStream, string attachmentFileName, INotifications notify, out ITransactionParticipant[] transactionActions)
		{
			string data = GetFullDataFileClosingStream((StreamReader)dataStream);
			bool result = false;

			ArrayList transactionActionsList = new ArrayList();

			if (!IsAutoImportProcess)
			{
				string fullFilePath = Path.Combine(TNTDataRegistry.Instance.QuantumFileSourceDirectory, attachmentFileName);
				FileInfo fileInfo = new FileInfo(fullFilePath);
				transactionActionsList.Add(FactoryProvider.Current);
				transactionActionsList.Add(new SaveInTransactionActionProcessedFileMover(fileInfo, TNTDataRegistry.Instance.QuantumFileProcessedDirectory));
				FactoryProvider.CreateNewWithoutSave();
			}

			string[] fileLines = data.Split(new char[] { '\n' });
			fileLines = StringParser.TrimBlankLinesAndNewLines(fileLines);

			QuantumFile file = QuantumFile.FromData(fileLines, attachmentFileName, notify);

			if (file != null && file.QuantumSegments != null)
			{
				int i = 0;
				foreach (QuantumSegment segment in file.QuantumSegments)
				{
					ForwardingConsol segmentConsol = null;
					if (file.RequiresConsolProcessing)
					{
						segmentConsol = segment.Consol.CreateConsol(FactoryProvider.Current, notify);
					}

					if (segment.Shipments.Length > 0)
					{
						for (int counter = 0; counter < segment.Shipments.Length; counter++)
						{
							if (!segment.Shipments[counter].IsSubHousebill)
							{
								segment.ShipmentsNotes[counter].CreateShipmentNoteIfNotExists(FactoryProvider.Current, segment.Shipments[counter].CreateShipment(FactoryProvider.Current, true, notify));
							}
						}
					}
					i++;
					if (i % 10 == 0)
					{
						transactionActionsList.Add(FactoryProvider.Current);
						FactoryProvider.CreateNewWithoutSave();
					}
				}

				result = true;
			}

			transactionActionsList.Add(FactoryProvider.Current);
			FactoryProvider.CreateNewWithoutSave();
			transactionActions = (ITransactionParticipant[])transactionActionsList.ToArray(typeof(ITransactionParticipant)); // So that the factory is garbage collect when it is out of scope
			return result;
		}

		public override bool CheckEnvironmentValid(BusinessObjectFactory factory, INotifications notify)
		{
			bool result = true;
			if (!Directory.Exists(TNTDataRegistry.Instance.QuantumFileSourceDirectory))
			{
				notify.Notify(new ErrorNotification(ErrorType.MissingDataDirectory, "source directory " + TNTDataRegistry.Instance.QuantumFileSourceDirectory));
				result = false;
			}
			if (!Directory.Exists(TNTDataRegistry.Instance.QuantumFileProcessedDirectory))
			{
				notify.Notify(new ErrorNotification(ErrorType.MissingDataDirectory, "processed directory " + TNTDataRegistry.Instance.QuantumFileProcessedDirectory));
				result = false;
			}
			return result;
		}

		#region Implementation

		internal IOutgoingMailManager OutgoingMailManager = Env.OutgoingMailManager;

		protected string GetFullDataFileClosingStream(StreamReader reader)
		{
			string result = reader.ReadToEnd();
			reader.Close();
			return result;
		}

		internal BusinessObjectFactoryProvider FactoryProviderForTest
		{
			get
			{
				return FactoryProvider;
			}
		}

		#endregion
	}
}
