using System.Collections.Generic;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

[assembly: MailSubscriber(typeof(Enterprise.Client.Wow.WowDataImporter))]
namespace Enterprise.Client.Wow
{
	public class WowDataImporter : DataImporter
	{
		public WowDataImporter()
		{
			FactoryProvider.Current.RefreshEnabled = false;
		}

		protected WowDataImporter(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		public bool ImportData(StreamReader message, INotifications notify)
		{
			string fileNameForEdiTrack = "Message" + ZDateTime.Now.ToString(WowConstants.EdiTrackEmailDateFormat) + ".csv";
			return ImportData(message, fileNameForEdiTrack, notify, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ClientSpecifiedImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, fileNameForEdiTrack));
		}

		#region DataImporter Overrides

		protected override bool ImportDataToFactoryCore(TextReader textReader, string attachmentFileName, INotifications notify, out ITransactionParticipant[] transactionActions)
		{
			using (StaticCurrentFetcher.Instance.GetTemporaryCachingHolder())
			{
				RegistryOptions wowImporterItemOptions = WowDataRegistry.Instance.WOWImporterItem.Options;
				WowDataRegistry.Instance.WOWImporterItem.Options = RegistryOptions.Default;
				VirtualMemoryStream virtualStream = new VirtualMemoryStream();
				try
				{
					StreamReader reader = SaveStreamAndRemoveEmptyLines(textReader, virtualStream);

					transactionActions = System.Array.Empty<ITransactionParticipant>();

					bool result = IsWowCsvFile(reader, notify);

					CsvImportFile csvFile = CsvImportFileFactory.TryCreate(FactoryProvider, reader, notify);
					if (csvFile != null && csvFile.ShouldUpdateBusinessData)
					{
						csvFile.VerifyAndUpdateBusinessDataAfterBatchDownload(notify);
					}

					if (result)
					{
						List<ITransactionParticipant> actions = new List<ITransactionParticipant>();

						// SaveInTransactionAction to save the factory that has the imported data
						actions.Add(FactoryProvider.Current);
						// SaveInTransactionAction to save the data to the MI output directory
						if (!WowDataRegistry.Instance.ManagingImportsOutputDirectory.IsEmpty)
						{
							string fullMIOutputFile = Path.Combine(WowDataRegistry.Instance.ManagingImportsOutputDirectory, Path.GetFileName(attachmentFileName));
							MIFileSaver = new UniqueFileNameSaver(reader, fullMIOutputFile);
							actions.Add(MIFileSaver);
						}

						transactionActions = actions.ToArray();
					}
					return result;
				}
				finally
				{
					WowDataRegistry.Instance.WOWImporterItem.Options = wowImporterItemOptions;
					virtualStream.Close();
				}
			}
		}

		static StreamReader SaveStreamAndRemoveEmptyLines(TextReader reader, Stream stream)
		{
			StreamWriter writer = new StreamWriter(stream);

			while (true)
			{
				string line = reader.ReadLine();
				if (line == null)
				{
					break;
				}

				line = TrimBlankLineAndNewLine(line);
				if (line != null)
				{
					writer.WriteLine(line);
				}
			}

			writer.Flush();
			stream.Position = 0;
			return new StreamReader(stream);
		}

		static bool IsWowCsvFile(StreamReader reader, INotifications notify)
		{
			bool result = false;

			if (reader.BaseStream.Length > 0)
			{
				CsvImportFile file = CsvImportFileFactory.TryCreate(new BusinessObjectFactoryProvider(), reader, notify);
				result = (file != null);
			}

			return result;
		}

		static string TrimBlankLineAndNewLine(string line)
		{
			if (!string.IsNullOrEmpty(line.Trim()))
			{
				return line.Replace("\r", "").Replace("\n", "");
			}
			else
			{
				return null;
			}
		}

		public override bool CheckEnvironmentValid(BusinessObjectFactory factory, INotifications notify)
		{
			bool result = true;
			if (!WowDataRegistry.Instance.IsUnmatchedDataItemsAccountValid(factory))
			{
				notify.Notify(new ErrorNotification(WowErrorType.UnmatchOrgNotSet, ""));
				result = false;
			}
			else if (factory.LoadTop1(typeof(GlbGroup), new ZQuery(GlbGroupSchema.GG_Code,
				SQLComparisonOperator.Equal, WowConstants.DataImportNotificationGroupCode)) == null)
			{
				notify.Notify(new ErrorNotification(
					ErrorType.EmailNotifyGroupNotExist, "Use group code '" + WowConstants.DataImportNotificationGroupCode + "'"));
				result = false;
			}
			return result;
		}

		protected override void OnAfterImportFromEmails(NotificationBuffer buffer)
		{
			base.OnAfterImportFromEmails(buffer);
			if (buffer.HasErrors)
			{
				buffer.SendEmail(this.OutgoingMailManager, WowConstants.ErrorNotificationEmailSubject, WowConstants.DataImportNotificationGroupCode);
			}
			if (MIFileSaver != null)
			{
				MIFileSaver.Dispose();
			}
		}

		UniqueFileNameSaver MIFileSaver;

		#endregion

		#region Implementation

		internal IOutgoingMailManager OutgoingMailManager = Env.OutgoingMailManager;

		protected override IMailFilter GetMailItemFilter() => CreateWowMailFilter();

		[MailFilter(MailFilterCodes.WooliesDataImporter)]
		public static IMailFilter CreateWowMailFilter()
		{
			var query = new ZQuery(MailDBItemsSchema.MI_Direction, MailDirection.Receive)
				.AddToFilter(MailDBItemsSchema.MI_Status, MailStatus.Queued)
				.AddToFilter(WowDataRegistry.Instance.FilterEmailImportFilesBySpecificSubject.Value ?
					GetFilterEmailImportFilesBySpecificSubjectFilter() :
					GetLegacyExcludeEmailsSentFromEdiOrCustomsForContingencyFilter());

			return new QueryMailFilter(MailFilterCodes.WooliesDataImporter, query);
		}

		static ZQuery GetFilterEmailImportFilesBySpecificSubjectFilter()
		{
			ZQuery result = new ZQuery();

			ZQuery overseasManifestFilter = new ZQuery();
			overseasManifestFilter.AddToFilter(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.StartsWith, "Woolworths -");
			overseasManifestFilter.AddToFilter(JoinCondition.And, MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, "Etd-");
			result.AddToFilter(overseasManifestFilter, JoinCondition.Or);

			result.AddToFilter(JoinCondition.Or, MailDBItemsSchema.MI_Subject, SQLComparisonOperator.StartsWith, "WW shpmt prealert -");
			result.AddToFilter(JoinCondition.Or, MailDBItemsSchema.MI_Subject, SQLComparisonOperator.StartsWith, "WOOLWORTH SHIPMENT PRE-ALERT FOR");

			return result;
		}

		static ZQuery GetLegacyExcludeEmailsSentFromEdiOrCustomsForContingencyFilter()
		{
			var ccfEmailAddress = Customs.AU.Declaration.Business.SysConfigHelper.Instance.AUCCustomsCCFCurrentEmailAddress;
			ZQuery result = new ZQuery();
			result.AddToFilter(JoinCondition.And, MailDBItemsSchema.MI_Subject, SQLComparisonOperator.NotEqual, "IFTSTA");
			result.AddToFilter(JoinCondition.And, MailDBItemsSchema.MI_Subject, SQLComparisonOperator.NotContains, "Undeliverable:");
			result.AddToFilter(JoinCondition.And, MailDBItemsSchema.MI_Subject, SQLComparisonOperator.NotContains, "out of office reply");
			result.AddToFilter(JoinCondition.And, MailDBItemsSchema.MI_From, SQLComparisonOperator.NotContains, "@" + MessagingConstants.eRouterACSServerName);
			result.AddToFilter(JoinCondition.And, MailDBItemsSchema.MI_From, SQLComparisonOperator.NotContains, ccfEmailAddress.SubstringSafe(ccfEmailAddress.IndexOf('@')));
			return result;
		}

		#endregion
	}
}
