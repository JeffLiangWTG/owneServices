using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ELearningDocument
{
	public interface IELearningDocumentTfIdfRunner
	{
		void Run(CancellationToken token, ILogger logger, IMyAccountClient myAccount, Func<ILogger, IPdfTextExtractor> createPdfExtractor, ITfIdfTransformer tfIdfTransformer);
	}

	public class ELearningDocumentTfIdfRunner : IELearningDocumentTfIdfRunner
	{
		public void Run(CancellationToken token, ILogger logger,  IMyAccountClient myAccount, Func<ILogger, IPdfTextExtractor> createPdfExtractor, ITfIdfTransformer tfIdfTransformer)
		{
			logger?.Log(LogType.Debug, $"started running procedure");

			if (myAccount == null)
			{
				var message = $"{nameof(myAccount)} cannot be null, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(Run)}";
				throw new ArgumentNullException(nameof(myAccount), message);
			}

			if (tfIdfTransformer == null)
			{
				var message = $"{nameof(tfIdfTransformer)} cannot be null, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(Run)}";
				throw new ArgumentNullException(nameof(tfIdfTransformer), message);
			}

			if (createPdfExtractor == null)
			{
				var message = $"{nameof(createPdfExtractor)} cannot be null, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(Run)}";
				throw new ArgumentNullException(nameof(createPdfExtractor), message);
			}
			var factory = new BusinessObjectFactory(Db.Connection);
			var importer = new ELearningDocumentDescriptionImporter(factory);
			foreach (var description in importer.GetDescriptions().ToList())
			{
				if (string.IsNullOrWhiteSpace(description.url))
				{
					var message = $"skip item, url is empty in {description}, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(Run)}";
					logger?.Log(LogType.Warning, message);
					continue;
				}

				if (!ValidUrl(logger, description.url))
				{
					var message = $"skip item, url failed validation. url in {description}, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(Run)}";
					logger?.Log(LogType.Warning, message);
					continue;
				}

				byte[] file = GetFileContent(logger, description, myAccount);
				if (file == null)
				{
					var message = $"skip item, file content is  empty for: {description}, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(Run)}";
					logger?.Log(LogType.Warning, message);
					continue;
				}
				var content = createPdfExtractor(logger).GetText(file);
				if (string.IsNullOrWhiteSpace(content))
				{
					var message = $"file is empty for path:{description.url}, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(Run)}";
					logger?.Log(LogType.Warning, message);
					continue;
				}

				CreateRecord(logger, description, factory, tfIdfTransformer.Transform(content, logger));
				token.ThrowIfCancellationRequested();
			}
			factory.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		void CreateRecord(ILogger logger, (string url, ZGuid id) description, BusinessObjectFactory factory, (IEnumerable<double> tfidf, IEnumerable<int> termFrequency) transformationResult)
		{
			var obj = factory.New<ELearningDocumentTfIdf>();
			obj.DocumentLastModified = DateTime.UtcNow;
			obj.ELearningDocumentDescriptionRefKey = description.id;
			obj.Tf = new ZBlob(transformationResult.termFrequency.Select(d => (double)d).ToList().SelectMany(BitConverter.GetBytes).ToArray());
			obj.TfIdf = new ZBlob(transformationResult.tfidf.ToList().SelectMany(BitConverter.GetBytes).ToArray());
		}

		byte[] GetFileContent(ILogger logger, (string url, ZGuid id) description, IMyAccountClient myAccount)
		{
			byte[] result = null;
			var sharedFolderFilePath = string.Empty;
			try
			{
				sharedFolderFilePath = myAccount.UrlToNetworkPath(description.url);
				if (string.IsNullOrWhiteSpace(sharedFolderFilePath))
				{
					var message = $"{nameof(sharedFolderFilePath)} is empty or null for original record:{description}, check logs for details, unable to download file, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(GetFileContent)}";
					logger?.Log(LogType.Warning, message);
					return result;
				}

				result = myAccount.DownloadFile(sharedFolderFilePath);
				if (result == null || result.Length == 0)
				{
					var message = $"file is empty or error occured, original record: {description}, {nameof(sharedFolderFilePath)}: {sharedFolderFilePath}, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(GetFileContent)}";
					logger?.Log(LogType.Warning, message);
				}
				return result;
			}
			catch (UnauthorizedAccessException uae)
			{
				var message =
					$"access denied to file: {sharedFolderFilePath}, original record: {description}, error:{uae}, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(GetFileContent)}";
				logger?.Log(LogType.Error, message);
			}
			catch (FileNotFoundException fnf)
			{
				var message =
					$"file not found {sharedFolderFilePath}, original record: {description}, error:{fnf}, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(GetFileContent)}";
				logger?.Log(LogType.Error, message);
			}
			catch (IOException ioe)
			{
				var message = $"unable to download file {nameof(sharedFolderFilePath)}: {sharedFolderFilePath}, original record: {description}, error:{ioe}, class:{nameof(ELearningDocumentTfIdfRunner)}, method:{nameof(GetFileContent)}";
				logger?.Log(LogType.Error, message);
			}

			return result;
		}

		bool ValidUrl(ILogger logger, string url)
		{
			logger?.Log(LogType.Information, $"start validation of url:{url}");
			return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
		}
	}
}
