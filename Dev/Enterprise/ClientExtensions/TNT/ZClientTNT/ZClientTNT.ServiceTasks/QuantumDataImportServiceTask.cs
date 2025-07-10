using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Client.TNT;
using Enterprise.Client.TNT.ServiceTasks;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	TNTConstants.QuantumFileImportSrvTaskCode,
	"Quantum Files Import",
	"CSP",
	typeof(QuantumDataImportServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia,
	MinimumPeriod = "30seconds",
	DefaultScheduleRunEvery = "30seconds"
	)]
namespace Enterprise.Client.TNT.ServiceTasks
{
	class QuantumDataImportServiceTask : TNTServiceTask
	{
		protected override void Execute(NotificationBuffer notify, CancellationToken token)
		{
			notify.Notify(new InfoNotification("Searching " + TNTDataRegistry.Instance.QuantumFileSourceDirectory + " for Quantum files to process..."));
			DirectoryInfo info = new DirectoryInfo(TNTDataRegistry.Instance.QuantumFileSourceDirectory);
			FileInfo[] files = info.GetFiles();

			if (files.Length > 0)
			{
				notify.Notify(new InfoNotification(files.Length.ToString(CultureInfo.InvariantCulture) + " file(s) found in Quantum directory"));
				foreach (FileInfo file in files)
				{
					token.ThrowIfCancellationRequested();
					if (IsQuantumX1File(file))
					{
						NotificationBuffer inner = new NotificationBuffer(notify);
						try
						{
							inner.Notify(new InfoNotification("Importing Data from file: " + file.Name));

							using (StreamReader fileDataStream = new StreamReader(file.FullName, Encoding.ASCII))
							{
								DataImporter.ImportData(fileDataStream, file.Name, inner, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ClientSpecifiedImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, file.Name));

								if (inner.HasErrors)
								{
									inner.Notify(new ErrorNotification(ErrorType.Error, "Error(s) occurred while importing data from file: " + file.Name));
									RenameFile(file);
								}
								MoveFileToProcessDirectory(file);
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							inner.Notify(new ErrorNotification(ErrorType.Error, "Error(s) occurred while importing data from file: " + file.Name + " " + ex.Message));
							RenameFile(file);
						}
					}
				}
			}
			else
			{
				notify.Notify(new InfoNotification("	...no files found for processing at this time"));
			}

			notify.Notify(new InfoNotification("EDN reply - Start"));
			RunEDNReturnCatchingExceptions(notify);
			notify.Notify(new InfoNotification("EDN reply - End"));
		}

		void MoveFileToProcessDirectory(FileInfo file)
		{
			ProcessedFileMover mover = new ProcessedFileMover(TNTDataRegistry.Instance.QuantumFileProcessedDirectory);
			mover.Move(file);
		}

		void RenameFile(FileInfo file)
		{
			try
			{
				int extensionIndx = file.Name.LastIndexOf('.');
				ZString fileName = (extensionIndx != -1) ? new ZString(file.Name).SubstringSafe(0, extensionIndx + 1) : new ZString(file.Name + ".");
				ZString errorFileFullName = Path.Combine(TNTDataRegistry.Instance.QuantumFileSourceDirectory, fileName + "err");
				File.Copy(file.FullName, errorFileFullName, true);
				file.Delete();
			}
			catch (IOException) { }
		}

		void RunEDNReturnCatchingExceptions(NotificationBuffer notify)
		{
			NotificationBuffer inner = new NotificationBuffer(notify);

			inner.Notify(new InfoNotification("Extracting EDN return values..."));
			try
			{
				EDNExporter exporter = new EDNExporter(new NotificationBuffer(notify));
				exporter.Run();
				inner.Notify(new InfoNotification("EDN Return Complete."));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				inner.Notify(new InfoNotification("Errors occurred while exporting EDN Data: " + ex.Message));
			}
		}

		TNTDataImporter DataImporter
		{
			get { return dataImporter ?? (dataImporter = new TNTDataImporter(true)); }
		}
		TNTDataImporter dataImporter;

		bool IsQuantumX1File(FileInfo file)
		{
			string exit1Pattern = @"^\w{3}.X1\.[0-9]{8}\.[0-9]{6}" + QuantumFile.Extension + "$";
			bool result = Regex.IsMatch(file.Name, exit1Pattern, RegexOptions.IgnoreCase);
			return result;
		}

		protected override bool IsEnvironmentValid
		{
			get
			{
				return
					!TNTDataRegistry.Instance.QuantumFileProcessedDirectory.IsEmpty && Directory.Exists(TNTDataRegistry.Instance.QuantumFileProcessedDirectory) &&
					!TNTDataRegistry.Instance.QuantumFileSourceDirectory.IsEmpty && Directory.Exists(TNTDataRegistry.Instance.QuantumFileSourceDirectory) &&
					!TNTDataRegistry.Instance.TNTReplyDirectory.IsEmpty && Directory.Exists(TNTDataRegistry.Instance.TNTReplyDirectory);
			}
		}

		protected override ZString RegistriesNotSetErrMesg
		{
			get { return ErrorMessage; }
		}

		internal const string ErrorMessage = "Quantum Files Import Registry Items are not set or invalid. Please verify the values in Admin-> Registry-> TNT Client Extensions-> Quantum Files";
	}
}
