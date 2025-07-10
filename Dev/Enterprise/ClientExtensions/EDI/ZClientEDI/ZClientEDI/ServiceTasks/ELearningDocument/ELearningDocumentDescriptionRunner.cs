using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ELearningDocument
{
	public interface IELearningDocumentDescriptionRunner
	{
		void Run(CancellationToken token, IRecentPdfUpdatesApiClient prdUpdatesClient);
	}
	public class ELearningDocumentDescriptionRunner : IELearningDocumentDescriptionRunner
	{
		readonly ILogger logger;
		public ELearningDocumentDescriptionRunner(ILogger logger)
		{
			this.logger = logger;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public void Run(CancellationToken token, IRecentPdfUpdatesApiClient prdUpdatesClient)
		{
			if (prdUpdatesClient == null)
			{
				var message = $"{nameof(prdUpdatesClient)} cannot be null";
				throw new ArgumentNullException(nameof(prdUpdatesClient), message);
			}

			logger?.Log(LogType.Debug, $"starting running task");
			var factory = new BusinessObjectFactory(Db.Connection);
			var importer = new ELearningDocumentDescriptionImporter(factory);
			prdUpdatesClient.DownloadListOfChanges(importer.LastUpdateDate()).ToList().ForEach(doc =>
			{
				var record = !importer.RecordExists(Guid.Parse(doc.Id)) ? importer.Create() : importer.Load(Guid.Parse(doc.Id));
				record.Url = doc.Url;
				record.DocumentLastModified = DateTime.UtcNow;
				record.DocumentType = doc.Type;
				record.Title = doc.Title;
				token.ThrowIfCancellationRequested();
			});
			factory.Save();
		}
	}
}
