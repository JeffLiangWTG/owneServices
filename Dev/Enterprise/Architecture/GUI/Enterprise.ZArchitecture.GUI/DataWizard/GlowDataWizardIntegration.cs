using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;
using WTG.Foundation.Http;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI
{
	public enum IsImportableResult
	{
		Unknown,
		Undecided,
		False,
		True,
		DbMismatch,
		ServiceUrlMissing,
	}

	public static class GlowDataWizardIntegration
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "file extension filters")]
		const string FileDialogFilter = "All supported files|*.xls;*.xlsx;*.txt;*.csv|" +
										"Excel files (*.xls;*.xlsx)|*.xls;*.xlsx|" +
										"Text files (*.txt)|*.txt|" +
										"CSV files (*.csv)|*.csv";

		const string Portal = "GHS/" + FormFactor.Desktop; // url path

		[ThreadSafe]
		static readonly IList<string> validExtensionList = new ReadOnlyCollection<string>(new List<string> { ".xls", ".xlsx", ".txt", ".csv" });// file extension filters

		static bool InvalidExtension(string fileName, GlowLog log)
		{
			var ext = Path.GetExtension(fileName);
			if (validExtensionList.Contains(ext, StringComparer.OrdinalIgnoreCase))
			{
				return false;
			}
			var extensionMessage = ResString.GetMultilingualString("410f4eea-d2ad-4d40-8b22-6060f3744d1d", "File with extension {0} is not supported. Supported file types are: .xls, .xlsx, .csv, .txt", ext);
			log.AppendLog(LogType.Error, extensionMessage, 0);
			return true;
		}

		static bool IsEmptyFile(Stream fileStream, GlowLog log)
		{
			if (fileStream.Length == 0)
			{
				log.AppendLog(LogType.Error, ResString.GetMultilingualString("273e0c72-e69e-4697-a455-109807ef2fc3", "The selected file is empty."), 0);
				return true;
			}
			return false;
		}

		public static string ShowEmbeddedImportWizard(IBusinessObjectCollection collection, IDataTransferMapping mapping, Func<string, INotifications, bool> additionalAction = null)
		{
			if (!ValidateEnvironment(out var message))
			{
				return message;
			}

			var glowServiceUri = GlowRegistry.Instance.GlowServiceUri;
			var baseUri = new Uri(glowServiceUri);

			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.Filter = FileDialogFilter;
				openFileDialog.RestoreDirectory = true;
				var dialogResult = ZFormModaliser.ShowCommonDialogWithoutDispose(openFileDialog);

				if (dialogResult == DialogResult.OK)
				{
					using (var fileStream = openFileDialog.OpenFile())
					{
						var fileName = Path.GetFileName(openFileDialog.UnmappedFileName);
						var log = new GlowLog();
						var sendErrorReport = false;

						var hasErrors = InvalidExtension(fileName, log) || IsEmptyFile(fileStream, log);
						if (!hasErrors)
						{
							GlowResponse<IEnumerable<ImportPreview>> dataRows = null;
							GlowResponse<MappingDataModel> mappingDataModel = null;
							var dataDefinitionName = GlowDataDefinitionReference.FromType(collection.TypeOfElements)?.DataDefinitionName;
							using (var progressReporter = GetProgressFromProvider().CreateProgressReporter(Res.GetString("EBD024E2-AC9C-4545-8299-A3D2C6D47AAF", "Importing Excel File, Parsing File, Running Mappings"), -1, false))
							{
								try
								{
									log.AppendLog(LogType.Info, ResString.GetMultilingualString("3FF31E79-9712-435F-BE1C-E70E0CB75C16", "Start getting data rows"), 0);
									dataRows = GetDataRows(mapping, baseUri, fileStream, fileName, log);
									if (!dataRows.HasError)
									{
										log.AppendLog(LogType.Info, ResString.GetMultilingualString("8DA4D740-F400-4F10-8FE4-D9C7A1CA2B93", "Finish getting data rows"), 0);
										log.AppendLog(LogType.Info, ResString.GetMultilingualString("033EF3AF-3D7B-4B6A-B3F8-041FD6D11DB1", "Start getting mapping data model"), 0);
										mappingDataModel = GetMappingDataModel(mapping, baseUri, dataDefinitionName, log);
										if (!mappingDataModel.HasError)
										{
											log.AppendLog(LogType.Info, ResString.GetMultilingualString("C7B1E503-9D7F-4D3F-A78E-E77262006AE9", "Finish getting mapping data model"), 0);
										}
										else
										{
											sendErrorReport = mappingDataModel.IsReportable;
											hasErrors = true;
										}
									}
									else
									{
										sendErrorReport = dataRows.IsReportable;
										hasErrors = true;
									}
								}
								catch (GlowHttpRequestException e)
								{
									log.AppendLog(LogType.Error, e.Message, 0);
									hasErrors = true;
									sendErrorReport = IsReportableStatus(e.StatusCode);
								}
								catch (AuthorizationFailureException ex) when (!Env.CurrentUser.IsSupportUser)
								{
									log.AppendLog(LogType.Error, ex.Message, 0);
									hasErrors = true;
									if (!TryHandleAuthenticationResult(ex))
									{
										sendErrorReport = true;
									}
								}
							}

							if (dataRows?.Content != null && mappingDataModel?.Content != null)
							{
								using (var progressReporter = GetProgressFromProvider().CreateProgressReporter(Res.GetString("6ABACA43-DE6F-4A35-B54D-3A60179F9677", "Importing Row"), dataRows.Content.Count(), false))
								using (collection.SuspendListChanged())
								{
									log.AppendLog(LogType.Info, Res.GetString("2A0D813B-7C38-4C7B-8A19-6E73324AC701", "Start populating rows"), 0);
									using (Db.DisposableActionForDbConnection())
									{
										hasErrors = !ObjectFactory.Get<IGlowCollectionImporter>().PopulateFromDataRows(collection, dataRows.Content, mappingDataModel.Content, log, progressReporter);
									}
									log.AppendLog(LogType.Info, Res.GetString("18D353C4-85F4-4E95-BBD7-76F4BCD1D5F0", "Finish populating rows"), 0);
								}
							}
						}

						if (hasErrors)
						{
							if (sendErrorReport)
							{
								log.AppendLog(LogType.Error, Res.GetString("4A6B8092-E1F9-4AE8-8F61-8866A012D08F", "An error report of the problem has been sent to CargoWise."), 0);
								ErrorReporter.ReportOnce("GlowImportWizard_FailedImport", log.GetLogs());
							}
							else
							{
								log.AppendLog(LogType.Warning, ResString.GetMultilingualString("dceabb3c-ba86-4f3a-9933-0bce3dfa63c4", "Import has finished with errors."), 100);
							}
						}
						else
						{
							if (additionalAction?.Invoke(openFileDialog.UnmappedFileName, log) ?? true)
							{
								log.AppendLog(LogType.ImportFinished, ResString.GetMultilingualString("d283a615-7640-4879-bf3e-c46705ab60fa", "Data has been imported successfully."), 100);
							}
						}

						ShowResultForm(log);
					}
				}
			}

			return string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "multipart content type")]
		public static string ShowEmbeddedImportWizard(IDataTransferMapping mapping, Action refresh)
		{
			if (!ValidateEnvironment(out var message))
			{
				return message;
			}

			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.Filter = FileDialogFilter;
				openFileDialog.RestoreDirectory = true;
				var dialogResult = ZFormModaliser.ShowCommonDialogWithoutDispose(openFileDialog);

				if (dialogResult == DialogResult.OK)
				{
					using (var fileStream = openFileDialog.OpenFile())
					{
						var fileName = Path.GetFileName(openFileDialog.UnmappedFileName);
						var log = new GlowLog();
						if (!InvalidExtension(fileName, log) && !IsEmptyFile(fileStream, log))
						{
							using (var progressReporter = GetProgressFromProvider().CreateProgressReporter(Res.GetString("73DA06FE-F139-4945-BB58-EDF96E76BC01", "Importing excel file"), -1, false))
							{
								var glowServiceUri = GlowRegistry.Instance.GlowServiceUri;
								using (var dataContent = CreateFileMultipartContent(fileStream, fileName, "application/vnd.ms-excel"))
								{
									var importer = ObjectFactory.Get<IGlowServiceImporter>();
									var baseUri = new Uri(glowServiceUri);
									try
									{
										importer.StartImportAsync(baseUri, mapping, dataContent, log);
										refresh();
									}
									catch (Exception e) when (!e.IsCriticalException() && !(e is DatabaseUpgradeException))
									{
										log.AppendLog(LogType.Error, e.Message, 0);
									}
								}
							}
						}
						ShowResultForm(log);
					}
				}
			}

			return string.Empty;
		}

		static GlowResponse<IEnumerable<ImportPreview>> GetDataRows(IDataTransferMapping mapping, Uri baseUri, Stream fileStream, string fileName, GlowLog log)
		{
			var clientFactory = ObjectFactory.Get<IGlowServiceClientFactory>();
			using (var client = clientFactory.Create(baseUri))
			{
				var result = new GlowResponse<IEnumerable<ImportPreview>>();
				var delimitedRows = GetImportContent(client, mapping, fileStream, fileName, log, baseUri);
				if (!delimitedRows.HasError)
				{
					var response = HttpUtils.PostAsJson(client, FormattableString.Invariant($"api/datatransfer/createpreview?mappingPK={mapping.PK}"), delimitedRows.Content); // service URL
					if (!response.IsSuccessStatusCode)
					{
						var responseAsString = HttpUtils.ReadAsString(response);
						log.AppendLog(LogType.Error, Res.GetString("D9D5DC88-DB6F-4036-83B9-7C92B5DF32CA", "Tried to get data rows but the request was unsuccessful."), 0);
						log.AppendLog(LogType.Error,
							$"Post Request details: baseUri: {baseUri}, mappingPK: {mapping.PK}, " + // error Message for Silent exception
							$"Post Response details: StatusCode: {response.StatusCode}, Content: {responseAsString}", 0);// error Message for Silent exception
						result.HasError = true;
						result.IsReportable = IsReportableStatus(response.StatusCode);
					}
					else
					{
						result.Content = HttpUtils.ReadAsJson<ImportPreview[]>(response);
						if (result.Content == null)
						{
							log.AppendLog(LogType.Error, Res.GetString("F9DCA503-4289-4FE0-A78A-93658F900392", "Tried to get data rows but returned null."), 0);
							log.AppendLog(LogType.Error,
								$"Post Request details: baseUri: {baseUri}, mappingPK: {mapping.PK}, " + // error Message for Silent exception
								$"Post Response details: StatusCode: {response.StatusCode}, Content: null", 0);// error Message for Silent exception
							result.HasError = true;
						}
					}
				}
				else
				{
					result.HasError = delimitedRows.HasError;
					result.IsReportable = delimitedRows.IsReportable;
				}
				return result;
			}
		}

		static GlowResponse<MappingDataModel> GetMappingDataModel(IDataTransferMapping mapping, Uri baseUri, string dataDefinitionName, GlowLog log)
		{
			var clientFactory = ObjectFactory.Get<IGlowServiceClientFactory>();
			using (var client = clientFactory.Create(baseUri))
			{
				var result = new GlowResponse<MappingDataModel>();
				var response = HttpUtils.Get(client, FormattableString.Invariant($"api/datatransfer/MappingDataModel?dataDefinitionName={WebUtility.UrlEncode(dataDefinitionName)}&mappingPK={mapping.PK}")); // service URL
				if (!response.IsSuccessStatusCode)
				{
					HandleError();
				}
				else
				{
					result.Content = HttpUtils.ReadAsJson<MappingDataModel>(response);
					if (result.Content == null)
					{
						log.AppendLog(LogType.Error, Res.GetString("7C9DCA86-AE54-4D19-81DA-5BBDEDBE8259", "Tried to get mapping data model but returned null."), 0);
						log.AppendLog(LogType.Error,
							$"Get Request details: baseUri: {baseUri}, mappingPK: {mapping.PK}, dataDefinitionName: {dataDefinitionName}, " + // error Message for Silent exception
							$"Get Response details: StatusCode: {response.StatusCode}, Content: null", 0);// error Message for Silent exception
						result.HasError = true;
					}
				}
				return result;

				void HandleError()
				{
					var responseAsString = HttpUtils.ReadAsString(response);
					result.HasError = true;

					if (response.IsProblemDetails())
					{
						try
						{
							var problemDetails = HttpUtils.ReadAsJson<ProblemDetails>(response);
							log.AppendLog(LogType.Error, GetErrorMessageFromProblemDetails(problemDetails), 0);
							result.IsReportable = IsReportableProblem(problemDetails);
						}
						catch (JsonReaderException)
						{
							log.AppendLog(LogType.Error, $"Post Response details: StatusCode: {response.StatusCode}, Content: {responseAsString}", 0); // error Message for Silent exception
							result.IsReportable = true;
						}
					}
					else
					{
						log.AppendLog(LogType.Error, Res.GetString("50C47A0D-45C7-4C2C-9B61-6804C89C6587", "Tried to get mapping data model but the request was unsuccessful."), 0);
						log.AppendLog(LogType.Error,
							$"Get Request details: baseUri: {baseUri}, mappingPK: {mapping.PK}, dataDefinitionName: {dataDefinitionName}, " + // error Message for Silent exception
							$"Get Response details: StatusCode: {response.StatusCode}, Content: {responseAsString}", 0);// error Message for Silent exception
						result.IsReportable = IsReportableStatus(response.StatusCode);
					}
				}
			}
		}

		class GlowResponse<T>
		{
			public bool HasError { get; set; }
			public bool IsReportable { get; set; }
			public T Content { get; set; }
		}

		static bool IsReportableStatus(HttpStatusCode statusCode) => statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.NotFound;

		static bool IsReportableProblem(ProblemDetails problemDetails)
		{
			var type = problemDetails.Type;
			var notReportableProblemTypes = new[]
			{
				ProblemType.BadData,
				ProblemType.FileHasInvalidExcelSheet,
				ProblemType.FileIsEmpty,
				ProblemType.FileIsEncrypted,
				ProblemType.FileIsInvalidExcel,
				ProblemType.FileTooOld,
				ProblemType.FileTypeNotSupported,
				ProblemType.InvalidMappingTargetColumn,
				ProblemType.MissingSeparatorError,
				ProblemType.StrictOOXMLExcelFileNotSupported,
			};

			return !notReportableProblemTypes.Contains(type);
		}

		static bool ValidateEnvironment(out string message)
		{
			var glowServiceUri = GlowRegistry.Instance.GlowServiceUri;
			if (glowServiceUri == "/")
			{
				message = GetGlowServiceNotConfiguredMessage();
				return false;
			}

			if (Env.CurrentUser.IsSupportUser)
			{
				message = ResString.GetMultilingualString(
					"8e7dc37c-a100-4229-b375-de4f3230da9c",
					"The CW1 Support login cannot be used when interacting with the Advanced Data Automation Wizard. Please login as an operational user in order to use this feature."
				);
				return false;
			}

			message = null;
			return true;
		}

		static void ShowResultForm(GlowLog log)
		{
			if (log.CountWithoutVerbose > 0)
			{
				using (var importForm = new GlowDataWizardImportForm(log))
				{
					ZFormModaliser.ShowDialogWithoutDispose(importForm);
				}
			}
		}

		static IProgressReporterProvider GetProgressFromProvider()
		{
			return new DefaultProgressReporterProvider(ZForm.ActiveForm);
		}

		public static string ShowImportMappingWizard(Type typeOfElements, IDataTransferMapping mapping)
		{
			if (!ValidateEnvironment(out var message))
			{
				return message;
			}

			var dataDefinition = GlowDataDefinitionReference.FromType(typeOfElements);
			var baseURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var validationErrorMessage = ValidateForGlowImportWizard(dataDefinition, baseURL);
			if (!string.IsNullOrEmpty(validationErrorMessage))
			{
				return validationErrorMessage;
			}

			var mappingPK = mapping?.PK.ToString() ?? string.Empty;
			var mappingPKUrlSuffix = string.IsNullOrEmpty(mappingPK) ? string.Empty : "/" + mappingPK;
			var baseUri = new Uri(baseURL);

			var url = UrlBuilder.GenerateCaptiveSessionURL(baseUri, Portal, FormattableString.Invariant($@"/dataAutomation/importMappingPage/{dataDefinition.DataDefinitionName}{mappingPKUrlSuffix}")); // url address

			WebUrlLauncher.Launch(url.ToString());
			return string.Empty;
		}

		public static bool IsADAWEnabled()
		{
			return GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public static IsImportableResult IsImportable(Type type)
		{
			if (GlowRegistry.Instance.GlowServiceUri == "/")
			{
				return IsImportableResult.ServiceUrlMissing;
			}

			var dataDefinition = GlowDataDefinitionReference.FromType(type);
			if (dataDefinition == null || !IsADAWEnabled())
			{
				return IsImportableResult.False;
			}

			if (GlowRegistry.Instance.ForceImportable.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				return IsImportableResult.True;
			}

			return IsImportable(dataDefinition.DataDefinitionName);
		}

		static IsImportableResult IsImportable(string dataDefinitionName)
		{
			try
			{
				var names = ImportableDataDefinitionNamesProviderForUser.Names;
				if (names != null)
				{
					return names.TryGetValue(dataDefinitionName, out var result)
						? result
						: IsImportableResult.Undecided;
				}

				return IsImportableResult.Unknown;
			}
			catch (GlowConfigurationException)
			{
				return IsImportableResult.DbMismatch;
			}
		}

		static ImportableDataDefinitionNamesProvider ImportableDataDefinitionNamesProviderForUser => Env.CurrentUserContext.GetInstance(() => new ImportableDataDefinitionNamesProvider());

#if DEBUG
		public static void ResetImportableDataDefinitionNames()
		{
			ImportableDataDefinitionNamesProviderForUser.Reset();
		}
#endif

		class ImportableDataDefinitionNamesProvider
		{
			public IReadOnlyDictionary<string, IsImportableResult> Names => names ?? (names = GetNames());

			IReadOnlyDictionary<string, IsImportableResult> GetNames()
			{
				var clientFactory = ObjectFactory.Get<IGlowServiceClientFactory>();
				var glowServiceUri = GlowRegistry.Instance.GlowServiceUri;
				var baseUri = new Uri(glowServiceUri);
				try
				{
					using (var client = clientFactory.Create(baseUri))
					{
						var response = HttpUtils.Get(client, "api/datatransfer/datadefinitionnames");
						if (!response.IsSuccessStatusCode)
						{
							if (!BadConfigurationStatusCodes.Contains(response.StatusCode))
							{
								ErrorReporter.ReportOnce($"GlowDataWizardIntegration_IsImportable failed with StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}.");
							}
							return null;
						}
						return HttpUtils.ReadAsJson<Dictionary<string, bool>>(response).ToDictionary(p => p.Key, p => p.Value ? IsImportableResult.True : IsImportableResult.False);
					}
				}
				catch (GlowConfigurationException)
				{
					throw;
				}
				catch (AuthorizationFailureException ex) when (!Env.CurrentUser.IsSupportUser)
				{
					if (!TryHandleAuthenticationResult(ex))
					{
						ErrorReporter.ReportOnce("GlowDataWizardIntegration_IsImportable_Unauthorized", ex);
					}
				}
				catch (AuthorizationFailureException)
				{
					// this exception is ignored as alot was coming from support users on testrigs
				}
				catch (GlowHttpRequestException ex) when (BadConfigurationStatusCodes.Contains(ex.StatusCode))
				{
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("GlowDataWizardIntegration_IsImportable", ex);
				}

				return null;
			}

#if DEBUG
			public void Reset() => names = null;
#endif

			IReadOnlyDictionary<string, IsImportableResult> names;

			[ThreadSafe]
			static readonly HttpStatusCode[] BadConfigurationStatusCodes = new HttpStatusCode[] { HttpStatusCode.ProxyAuthenticationRequired, HttpStatusCode.BadGateway, HttpStatusCode.GatewayTimeout, HttpStatusCode.InternalServerError, HttpStatusCode.NotFound, HttpStatusCode.ServiceUnavailable, HttpStatusCode.Unauthorized, 0 };
		}

		static bool TryHandleAuthenticationResult(AuthorizationFailureException ex)
		{
			if (ex.AuthenticationResult == AuthenticationResult.LogonDetailsIncorrect
				|| ex.AuthenticationResult == AuthenticationResult.PasswordChangeRequired
				|| ex.AuthenticationResult == AuthenticationResult.LoginDisabled
				|| ex.AuthenticationResult == AuthenticationResult.AccountLocked
				|| ex.AuthenticationResult == AuthenticationResult.ContextChangeRequired)
			{
				ShowADAWNewLoginRequiredDialog();
				return true;
			}
			return false;
		}

		static string ValidateForGlowImportWizard(GlowDataDefinitionReference dataDefinition, string baseURL)
		{
			var isImportableResult = dataDefinition != null ? IsImportable(dataDefinition.DataDefinitionName) : IsImportableResult.False;
			if (isImportableResult == IsImportableResult.Undecided)
			{
				return ResString.GetMultilingualString("626e4c9a-71ae-49dc-92e4-7eedd37684f5", "The Advanced Data Automation Wizard is not available on this module. If you wish to use it, please log an eRequest and WiseTech Global will look at adding this functionality.");
			}
			else if (isImportableResult == IsImportableResult.DbMismatch)
			{
				return ResString.GetMultilingualString("2DD22288-5515-451F-8F2D-8081837F03A5", "Service authentication failed due to an instance mismatch. Ensure that your Glow Services and Glow Portals settings are configured correctly. Please contact your administrator.");
			}
			else if (isImportableResult != IsImportableResult.True)
			{
				return ResString.GetMultilingualString("5618c922-7e34-46bd-aca7-7a1a8a53db0e", "The Advanced Data Automation Wizard is not available on this module.");
			}

			if (string.IsNullOrEmpty(baseURL))
			{
				return GetGlowPortalsNotConfiguredMessage();
			}

			return string.Empty;
		}

		static string GetGlowServiceNotConfiguredMessage()
		{
			return ResString.GetMultilingualString("acdff5d6-8fd5-4884-a7b9-71f428bbd934", @"The Advanced Data Automation Wizard cannot be used due to missing Service URL in the GLOW configuration.
Please contact your System Administrator.");
		}

		static string GetGlowPortalsNotConfiguredMessage()
		{
			return ResString.GetMultilingualString("39b54786-a256-40a2-8f31-7c936c301a30", @"The Advanced Data Automation Wizard cannot be used due to missing Portals URL in the GLOW configuration.
Please contact your System Administrator.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "multipart content type, service URL, error Message for Silent exception")]
		static GlowResponse<IEnumerable<SampleDataLine>> GetImportContent(IGlowServiceClient client, IDataTransferMapping mapping, Stream fileStream, string fileName, GlowLog log, Uri baseUri)
		{
			using (var dataContent = CreateFileMultipartContent(fileStream, fileName, "application/vnd.ms-excel"))
			{
				var baseUrl = "api/datatransfer/sampledata";
				var sheetNameParam = mapping.SheetName.IsNullOrEmpty() ? string.Empty : $"&sheetName={WebUtility.UrlEncode(mapping.SheetName)}";
				var uri = $"{baseUrl}?separator={WebUtility.UrlEncode(mapping.Delimiter)}{sheetNameParam}&processFullStream=true";

				var response = HttpUtils.Post(client, uri, dataContent);
				var result = new GlowResponse<IEnumerable<SampleDataLine>>();
				if (!response.IsSuccessStatusCode)
				{
					HandleError();
					result.HasError = true;
				}
				else
				{
					var sampleData = HttpUtils.ReadAsJson<SampleData>(response);
					if (sampleData == null)
					{
						log.AppendLog(LogType.Error, Res.GetString("8770B3E4-6C66-4861-8FF4-934163A01C26", "Tried to get sample data but was null."), 0);
						log.AppendLog(LogType.Error,
							$"Post Request details: baseUri: {baseUri}, separator: '{mapping.Delimiter}', sheetName: '{mapping.SheetName}', " +
							$"Post Response details: StatusCode: {response.StatusCode}, Content: null", 0);
						result.HasError = true;
					}
					else
					{
						result.Content = sampleData.DelimitedRows.Skip(mapping.StartingRow).Select(r => new SampleDataLine(r.RowIndex, null, CSVUtils.SerializeValues(r.DataValue, mapping.Delimiter)));
					}
				}

				return result;

				void HandleError()
				{
					ProblemDetails problemDetails = null;
					var responseAsString = HttpUtils.ReadAsString(response);
					log.AppendLog(LogType.Error, Res.GetString("D1DB409A-4564-4561-9A1B-DE8C67AB8865", "Tried to get sample data but the request was unsuccessful."), 0);
					if (response.StatusCode == HttpStatusCode.RequestEntityTooLarge)
					{
						log.AppendLog(LogType.Error,
							Res.GetString("1ee9ac73-6dbe-45cd-b44a-8ab5d239a36b",
							"The file uploaded is too large and exceeds the size supported. Please select a smaller file and try again."), 0);
					}
					else
					{
						if (response.IsProblemDetails())
						{
							log.AppendLog(LogType.Error, Res.GetString("71416574-47f4-4f19-8151-8d1279c163f8", "Error when processing sample data"), 0);
							try
							{
								problemDetails = HttpUtils.ReadAsJson<ProblemDetails>(response);
								log.AppendLog(LogType.Error, GetErrorMessageFromProblemDetails(problemDetails), 0);
							}
							catch (JsonReaderException)
							{
								log.AppendLog(LogType.Error, $"Post Response details: StatusCode: {response.StatusCode}, Content: {responseAsString}", 0);
							}
						}
						else
						{
							log.AppendLog(LogType.Error,
								$"Post Request details: baseUri: {baseUri}, separator: '{mapping.Delimiter}', sheetName: '{mapping.SheetName}', " +
								$"Post Response details: StatusCode: {response.StatusCode}, Content: {responseAsString}", 0);
						}
					}
					result.IsReportable = IsReportableStatus(response.StatusCode) && (problemDetails == null || IsReportableProblem(problemDetails));
				}
			}
		}

		static string GetErrorMessageFromProblemDetails(ProblemDetails problemDetails)
		{
			var problemType = problemDetails.Type;
			string message = problemDetails.Detail;
			switch (problemType)
			{
				case ProblemType.MissingSeparatorError:
					message = ResString.GetMultilingualString("f3ed321b-d9b6-49e0-81e7-2531329bd043", "A separator must be provided.");
					break;

				case ProblemType.MissingContentError:
					message = ResString.GetMultilingualString("d7a33349-ab11-46b1-82bd-35bba09b446d", "Request must have multipart content.");
					break;

				case ProblemType.FileTypeNotSupported:
					var fileType = problemDetails.Extensions["fileType"];
					var supportedFileTypes = problemDetails.Extensions["supportedFileTypes"];
					message = ResString.GetMultilingualString("bc1946d8-447f-4006-a445-af1ecae4c992", "File with extension \"{0}\" is not supported. Supported file types are: {1}", fileType, supportedFileTypes);
					break;

				case ProblemType.BadData:
					var badData = problemDetails.Extensions["badData"];
					message = ResString.GetMultilingualString("69a9fc2a-9c80-4585-bdee-033e8e9114c0", "Please fix the file and try again. File contains bad data: {0}", badData);
					break;

				case ProblemType.FileHasInvalidExcelSheet:
					var sheetName = problemDetails.Extensions["sheetName"];
					message = ResString.GetMultilingualString("87eb528a-06c0-4607-ba02-3d341ee6a3cd", "No Worksheet with name '{0}' was found in file. Please make sure the file and mapping settings are correct.", sheetName);
					break;

				case ProblemType.FileIsEncrypted:
					message = ResString.GetMultilingualString("611c1e13-4deb-4973-b7bf-0a24d0955a2b", "The selected Excel file is encrypted with a password and cannot be read. Please remove the password before importing.");
					break;

				case ProblemType.FileTooOld:
					message = ResString.GetMultilingualString("f833c77a-f9b5-4528-bf97-a20e540a7c93", "The selected Excel file version is not supported. Please select an Excel file with version 97/2000/XP/2003 or superior.");
					break;

				case ProblemType.FileIsInvalidExcel:
					message = ResString.GetMultilingualString("8ff40056-8316-41eb-88a2-9f2afca45dc3", "The selected Excel file is invalid or corrupted.");
					break;

				case ProblemType.StrictOOXMLExcelFileNotSupported:
					message = ResString.GetMultilingualString("56151d92-f4d2-4ea1-b3b4-cb91ac7a97d4", "Your import data file is in an unsupported Excel format. Please save your file in 'Excel Workbook (*.xlsx)' format and then try again.");
					break;

				case ProblemType.InvalidMappingTargetColumn:
					var entity = GetTableCaptionFromGlowInterface(problemDetails.Extensions["entity"].ToString());
					var path = problemDetails.Extensions["path"];
					message = ResString.GetMultilingualString("c02ad667-fd51-4a59-9b0a-97ddd478eeea", "{0} does not have an associated property called \"{1}\"", entity, path);
					break;

				default:
					break;
			}

			return message;
		}

		static string GetTableCaptionFromGlowInterface(string interfaceName)
		{
			var tableName = interfaceName;
			var entityType = GlowInterfaceReferenceAttribute.GetGlowInterface(tableName);
			if (entityType != null)
			{
				tableName = DataBoundResourceStrings.GetTableDescriptiveName(entityType.Name.Substring(1));
			}

			return tableName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "multipart constant")]
		static MultipartContent CreateFileMultipartContent(Stream stream, string fileName, string contentType)
		{
			var dataContent = new MultipartFormDataContent();

			var fileContent = new StreamContent(stream);
			fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
			{
				Name = "\"Files[]\"",
				FileName = "\"" + fileName + "\""
			};
			fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
			dataContent.Add(fileContent);

			return dataContent;
		}

		public static void HandleADAWMenuItemPopup(ZMenuItem importWizardMenuItem, Type entityType, Action<IDataTransferMapping> handleShowImportWizard, Action<IDataTransferMapping> handleShowImportMappingWizard)
		{
			importWizardMenuItem.MenuItems.Clear();
			importWizardMenuItem.Click -= ShowADAWNotUnavailableForSupportUserErrorDialog;
			importWizardMenuItem.Click -= ShowADAWRequestDialog;
			importWizardMenuItem.Click -= ShowADAWConfigurationErrorDialog;
			importWizardMenuItem.Click -= ShowNonImportableDialog;

			if (importWizardMenuItem.Caption is ModifiedMultilingualString caption)
			{
				importWizardMenuItem.Caption = caption.Strings.First();
			}

			if (Env.CurrentUser.IsSupportUser)
			{
				importWizardMenuItem.Caption = MultilingualString.Join(" ", importWizardMenuItem.Caption, ResString.GetMultilingualString("fafc7d4b-e012-4fa6-8da6-2318b730c4ad", "(Unavailable for CW1 Support)"));
				importWizardMenuItem.Click += ShowADAWNotUnavailableForSupportUserErrorDialog;
			}
			else
			{
				var isImportable = IsImportable(entityType);
				if (isImportable == IsImportableResult.Undecided)
				{
					importWizardMenuItem.Caption = MultilingualString.Join(" ", importWizardMenuItem.Caption, ResString.GetMultilingualString("BB6AB3C3-AA5A-4335-A56A-BA90B8994C6E", "(Currently Unsupported)"));
					importWizardMenuItem.Click += ShowADAWRequestDialog;
				}
				else if (isImportable == IsImportableResult.Unknown)
				{
					importWizardMenuItem.Caption = MultilingualString.Join(" ", importWizardMenuItem.Caption, ResString.GetMultilingualString("1AEFAE28-98A4-4DF3-BF3A-616986C73202", "(Currently Unavailable)"));
					importWizardMenuItem.Click += ShowADAWConfigurationErrorDialog;
				}
				else if (isImportable == IsImportableResult.False)
				{
					importWizardMenuItem.Caption = MultilingualString.Join(" ", importWizardMenuItem.Caption, ResString.GetMultilingualString("E0F6B8FB-6D15-48B3-A949-042D031E0A60", "(Unavailable)"));
					importWizardMenuItem.Click += ShowNonImportableDialog;
				}
				else
				{
					var mappings = GlowEntityTypeHelper.GetImportMappings(new BusinessObjectFactory(), entityType);
					if (mappings.Any())
					{
						var glowImportWithMappingsMenu = new ZMenuItem(ResString.GetMultilingualString("0d37b4fb-5802-4253-8d7c-3d370a06a90b", "Import File Using"));
						glowImportWithMappingsMenu.MenuItems.AddRange(mappings.Select(m => new ZMenuItem(m.Name, (sender, args) => handleShowImportWizard(m))).ToArray());
						importWizardMenuItem.MenuItems.Add(glowImportWithMappingsMenu);
					}

					var glowMappingMenu = new ZMenuItem(ResString.GetMultilingualString("b86bdee2-ca8c-4a0b-ac58-b49c3dcdbf60", "Manage Mappings"));
					glowMappingMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("62e947bb-9ae9-47b1-9e0e-84dd074a4b7a", "New Mapping"), (sender, args) => handleShowImportMappingWizard(null)));
					glowMappingMenu.MenuItems.AddRange(mappings.Select(m => new ZMenuItem(m.Name, (sender, args) => handleShowImportMappingWizard(m))).ToArray());
					importWizardMenuItem.MenuItems.Add(glowMappingMenu);
				}
			}
		}

		static void ShowADAWNotUnavailableForSupportUserErrorDialog(object sender, EventArgs e) => ShowADAWNotUnavailableForSupportUserErrorDialog();

		static void ShowADAWNotUnavailableForSupportUserErrorDialog()
		{
			Globals.Message.Show(ResString.GetMultilingualString("c453ecc7-80b5-4a2c-8c8d-1a65b680f9ca", "The CW1 Support login cannot be used when interacting with the Advanced Data Automation Wizard. Please login as an operational user in order to use this feature."));
		}

		static void ShowADAWRequestDialog(object sender, EventArgs e) => ShowADAWRequestDialog();

		static void ShowADAWRequestDialog()
		{
			Globals.Message.Show(ResString.GetMultilingualString("a4b504b7-f050-45a8-948d-047bad6a58dd", @"Importing is currently not available for this data type. 
Please raise a CR9 for the relevant Product for this grid with examples of the data to be imported (you can press the F1 key to open a new eRequest)."));
		}

		static void ShowADAWConfigurationErrorDialog(object sender, EventArgs e) => ShowADAWConfigurationErrorDialog();

		static void ShowADAWConfigurationErrorDialog()
		{
			Globals.Message.Show(ResString.GetMultilingualString("93ed2fc9-a48e-4aac-b77b-a8f1bab3133e", @"Advanced Data Automation Wizard is currently not available.
Please contact your CW1 administrator to check that configuration is correct or contact CW1 Support if this error persists."));
		}

		static void ShowNonImportableDialog(object sender, EventArgs e) => ShowNonImportableDialog();

		static void ShowNonImportableDialog()
		{
			Globals.Message.Show(ResString.GetMultilingualString("4f208a55-960f-4967-80d2-70799477ee2a", @"Importing is not available for this data type."));
		}

		static void ShowADAWNewLoginRequiredDialog()
		{
			Globals.Message.Show(ResString.GetMultilingualString("23a8331d-a7d0-4450-b455-f50702aac28b", @"Changes to your credentials have occurred since the last login.
In order to use Advanced Data Automation Wizard, please logout first and login again."));
		}
	}
}
