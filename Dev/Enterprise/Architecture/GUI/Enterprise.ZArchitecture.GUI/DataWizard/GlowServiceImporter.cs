using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.Application;
using CargoWise.DataTransfer;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IGlowServiceImporter
	{
		void StartImportAsync(Uri baseUri, IDataTransferMapping mapping, MultipartContent dataContent, GlowLog log);
	}

	class GlowServiceImporter : IGlowServiceImporter
	{
		public void StartImportAsync(Uri baseUri, IDataTransferMapping mapping, MultipartContent dataContent, GlowLog log)
		{
			try
			{
				log.AppendLog(LogType.Info, ResString.GetMultilingualString("455573f7-4714-474a-8f3d-3a4e2c37901b", "Starting file import process..."), 1);

				var clientFactory = ObjectFactory.Get<IGlowServiceClientFactory>();
				var requestURI = baseUri.ToString() + FormattableString.Invariant($"api/datatransfer/import?entityName={mapping.ContextModule}&key={Guid.Empty}&mappingPK={mapping.PK}"); // url address

				using (var glowclient = clientFactory.Create(baseUri))
				using (var request = new HttpRequestMessage(HttpMethod.Post, requestURI) { Content = dataContent })
				using (var response = HttpUtils.Send(glowclient, request, HttpCompletionOption.ResponseHeadersRead))
				{
					HandleResponse(response, mapping, log);
					if (response.IsSuccessStatusCode)
					{
						var body = HttpUtils.ReadAsStream(response);
						using (var streamReader = new StreamReader(body))
						{
							while (!streamReader.EndOfStream)
							{
								var jsonObject = streamReader.ReadLine();
								var responseLog = JsonConvert.DeserializeObject<ProgressLog>(jsonObject);
								if (!responseLog.IsEmpty)
								{
									log.AppendLog(responseLog.Type, responseLog.Message, responseLog.Progress ?? 0);
								}
							}
						}
						if (!log.HasErrors)
						{
							log.AppendLog(LogType.ImportFinished, ResString.GetMultilingualString("5c091c63-ea71-4fe6-8ea9-cc1bc9bcb838", "Data has been imported successfully."), 100);
						}
					}
				}
			}
			finally
			{
				dataContent.Dispose();
			}
		}

		static void HandleResponse(HttpResponseMessage response, IDataTransferMapping mapping, GlowLog logger)
		{
			if (response == null)
			{
				throw new ArgumentNullException(nameof(response));
			}

			if (!response.IsSuccessStatusCode)
			{
				string error = null;

				if (response.StatusCode == HttpStatusCode.RequestEntityTooLarge)
				{
					error = ResString.GetMultilingualString("292580f5-ba50-467d-9e74-4cadb872408e", "The file that you have chosen is too large and could cause performance issues if used in your application. Please choose a smaller file.");
				}
				else if (response.IsProblemDetails())
				{
					try
					{
						var problemDetails = HttpUtils.ReadAsJson<ProblemDetails>(response);
						error = GetErrorFromProblemDetails(mapping, problemDetails);
					}
					catch (JsonReaderException)
					{
					}
				}

				if (string.IsNullOrEmpty(error))
				{
					error = ResString.GetMultilingualString("5DA87F0F-BEBB-4F46-B976-60EAAEC17258", "Response from Glow service contains a non-successful status code {0}.", FormattableString.Invariant($"{(int)response.StatusCode} ({response.ReasonPhrase})"));
				}
				logger.AppendLog(LogType.Error, error, 100);
			}
		}

		static string GetErrorFromProblemDetails(IDataTransferMapping mapping, ProblemDetails problemDetails)
		{
			if (problemDetails == null)
			{
				return null;
			}

			var problemType = problemDetails.Type;
			switch (problemType)
			{
				case ProblemType.BadData:
					var badData = problemDetails.Extensions["badData"]; // key of extension of ProblemDetails
					return ResString.GetMultilingualString("f5c42b15-558c-4dc2-8f5f-35902c08df5b", "Please fix the file and try again. File contains bad data: {0}", badData);

				case ProblemType.FileHasInvalidExcelSheet:
					var sheetName = problemDetails.Extensions["sheetName"];
					return ResString.GetMultilingualString("c7c196c1-7a14-480f-9020-514795bc171f", "No Worksheet with name '{0}' was found in file. Please make sure the file and mapping settings are correct.", sheetName);

				case ProblemType.FileIsEmpty:
					return ResString.GetMultilingualString("ad8dc112-3762-4f01-83d9-b1e701466037", "The selected file contains no data.");

				case ProblemType.FileIsEncrypted:
					return ResString.GetMultilingualString("ed7771a1-15b0-48c9-9ff0-c59f47fbb760", "The selected Excel file is encrypted with a password and cannot be read. Please remove the password before importing.");

				case ProblemType.FileIsInvalidExcel:
					return ResString.GetMultilingualString("8ff40056-8316-41eb-88a2-9f2afca45dc3", "The selected Excel file is invalid or corrupted.");

				case ProblemType.FileTooOld:
					return ResString.GetMultilingualString("553ff81a-f222-434e-8844-45150bef0c97", "The selected Excel file version is not supported. Please select an Excel file with version 97/2000/XP/2003 or superior.");

				case ProblemType.StrictOOXMLExcelFileNotSupported:
					return ResString.GetMultilingualString("ffb46cb5-900b-44fe-abf9-804b17fdfad3", "Your import data file is in an unsupported Excel format. Please save your file in 'Excel Workbook (*.xlsx)' format and then try again.");

				case ProblemType.UnauthorizedMappingPaths:
					return GetFriendlyErrorEntities(mapping, problemDetails);

				default:
					break;
			}

			return null;
		}

		static string GetFriendlyErrorEntities(IDataTransferMapping mapping, ProblemDetails problemDetails)
		{
			var errorMessage = new StringBuilder();
			if (problemDetails.Extensions.TryGetValue("securityErrors", out var securityErrorsObj) &&
				securityErrorsObj is JArray securityErrors)
			{
				errorMessage.AppendLine(ResString.GetMultilingualString("31da831f-93ef-41ed-ae09-8c01a4dd19f5", "Your permissions do not give importing rights to the following entities:"));
				var mappingSecurityResults = securityErrors.ToObject<MappingSecurityResult[]>();
				foreach (var mappingSecurityResult in mappingSecurityResults)
				{
					var pathPrefix = mappingSecurityResult.PathPrefix;
					var mainEntity = string.IsNullOrWhiteSpace(pathPrefix) ? mapping.ContextModule : pathPrefix;
					if (!mappingSecurityResult.IsTableAuthorised)
					{
						errorMessage.AppendLine(mainEntity);
					}

					foreach (var unauthorisedRelation in mappingSecurityResult.UnauthorisedRelations)
					{
						errorMessage.AppendLine(mainEntity + "/" + unauthorisedRelation);
					}
				}
			}
			else
			{
				errorMessage.AppendLine(ResString.GetMultilingualString("3aec3837-ad33-40fd-afaf-a220a6c49053", "You do not have permission to perform this action. Contact your system administrator to be granted appropriate permissions."));
			}
			return errorMessage.ToString().TrimEnd(System.Environment.NewLine.ToCharArray());
		}

		class ProgressLog
		{
			public string Type { get; set; }
			public string Message { get; set; }
			public decimal? Progress { get; set; }
			public bool IsEmpty => string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Message);
		}
	}
}
