using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading;
using System.Xml;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;
using ICSharpCode.SharpZipLib.Zip;
using NLog;
using ZClientEDI.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Services
{
	public class UpgradePackageUrlGenerator
	{
		public UpgradePackageUrlGenerator(DbConnection connection, NLogWrapper logger)
		{
			this.connection = connection;
			this.logger = logger;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = CreateFactory()); }
		}
		BusinessObjectFactory factory;
		readonly DbConnection connection;
		readonly NLogWrapper logger;

		BusinessObjectFactory CreateFactory()
		{
			BusinessObjectFactory result = connection == null ? new BusinessObjectFactory() : new BusinessObjectFactory(connection);
			result.RefreshEnabled = false;
			return result;
		}

		Guid SessionId { get; set; }
		string LicenceCode { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		public UpgradePackageUrlResponse GetPackageUrl(UpgradePackageUrlRequest request)
		{
			EnsureOnlyRunOnEdiProd();

			SessionId = Guid.NewGuid();
			LicenceCode = request.LicenceCode;

			var timeStart = DateTime.Now;
			var infoMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | Start processing request";
			logger?.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());

			UpgradePackageUrlResponse response = new UpgradePackageUrlResponse() { ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Failed, URL = string.Empty, ErrorMessage = string.Empty };

			try
			{
				// Validate and extract info from request
				int databaseNumber = -1;
				UpgradeInfo upgradeInfo;
				if (!ValidateRequestMessage(request, out databaseNumber, out upgradeInfo))
				{
					response.ErrorMessage = "Bad request: invalid request message";
					var errorMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {response.ErrorMessage}";
					logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
					return response;
				}

				if (request.LicenceCode.Length != 9)
				{
					response.ErrorMessage = "Bad request: invalid licence code";
					var errorMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {response.ErrorMessage}";
					logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
					return response;
				}

				var database = GetDatabaseFromRequest(request.LicenceCode, databaseNumber);
				if (database == null)
				{
					response.ErrorMessage = "Unable to identify database from licence code " + request.LicenceCode;
					var errorMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {response.ErrorMessage}";
					logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
					if (databaseNumber > -1)
					{
						response.ErrorMessage += " / database number " + databaseNumber.ToString(CultureInfo.InvariantCulture);
					}
					return response;
				}

				// Get latest build for database
				if (!new ReleaseRingsList().ContainsCode(database.LD_ReleaseRing))
				{
					response.ErrorMessage = "Unable to identify current release ring from licence code " + request.LicenceCode;
					if (databaseNumber > -1)
					{
						response.ErrorMessage += " / database number " + databaseNumber.ToString(CultureInfo.InvariantCulture);
					}
					var errorMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {response.ErrorMessage}";
					logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
					return response;
				}

				if (database.LD_AvailableUpgradeMethod == UpgradeMethods.Codes.Blocked)
				{
					response.ErrorMessage = "The database does not support HTTP upgrade";
					var errorMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {response.ErrorMessage}";
					logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
					return response;
				}

				infoMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | Start getting latest release build";
				logger?.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());

				ReleaseBuild latestReleaseBuild = null;

				try
				{
					var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.PK, database.LD_LE));
					//deliver cut-off build if it is enabled and the client is WTG-hosted
					var needCutOffBuild =
						EDIDataRegistry.Instance.EnableWeeklyBuildCutOff.Value &&
						EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetBoolFromCode(database.LD_HostedLocation) && !licenceEnterprise.LE_IsInternal;

					var builds = new LatestReleaseBuildsDictionary(Factory);
					builds.Load();

					latestReleaseBuild = builds.GetLatestAvailableBuildForInstalledVersion(database.LD_Product, database.LD_ReleaseRing, new VersionNumber(upgradeInfo.Version), request.IsPatchOnly, takeWeeklyBuildsInsteadOfLatest: needCutOffBuild);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					var errorMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {e}";
					logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.InternalServerError), SessionId.ToString(), ex: e);
					throw;
				}

				infoMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | Getting latest release build completed";
				logger?.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());

				if (latestReleaseBuild == null)
				{
					response.ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Success;
					response.VersionNumber = "0.0.0.0";
					response.ErrorMessage = "No available release build found";
					var warnMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {response.ErrorMessage}";
					logger?.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
					return response;
				}

				if (EDIDataRegistry.Instance.DatabasesRequiredReleaseBuildTesting.Value.ContainsCode(database.LD_DatabaseNumber.ToString())
					&& !latestReleaseBuild.HL_IsTestPassed)
				{
					response.ErrorMessage = $"System requires build to be tested. The latest build {latestReleaseBuild.VersionNumber} has not yet passed integration test.";
					var warnMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {response.ErrorMessage}";
					logger?.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
					return response;
				}

				if (ShouldApplyDownloadSkipOptimization(database, latestReleaseBuild))
				{
					response.URL = string.Empty;
					response.ErrorMessage = $"The package {latestReleaseBuild.VersionNumber} has been rolled out to CargoWise Cloud";
					response.VersionNumber = latestReleaseBuild.VersionNumber.ToString();
					response.ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Success;
					var warnMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {response.ErrorMessage}";
					logger?.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.NoContent), SessionId.ToString());
					return response;
				}

				//Generate upgrade package URL for latest release build
				infoMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | Start generating package URL";
				logger.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());
				var (packageUrl, errorMessage) = GenerateUpgradePackageUrl(database, latestReleaseBuild);
				infoMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | Generating package URL completed: {latestReleaseBuild.VersionNumber} {packageUrl}";
				logger.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());

				if (string.IsNullOrEmpty(packageUrl))
				{
					response.ErrorMessage = string.IsNullOrEmpty(errorMessage) ? "Failed to send package to web server" : errorMessage;
					var errorMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {response.ErrorMessage}";
					logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
					return response;
				}
				else
				{
					LogUpgradeInfo(request, database, latestReleaseBuild);

					response.URL = packageUrl;
					response.VersionNumber = latestReleaseBuild.VersionNumber.ToString();
					response.ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Success;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				response.ResponseClass = UpgradePackageUrlResponse.ResponseClassType.Error;
				response.ErrorMessage = "Unhandled exception: " + ex;

				var company = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany;
				var companyCode = company != null ? company.GC_Code : ZString.Empty;
				ErrorReporter.ReportOnce("Unhandled exception from upgrade web service call", FormattableString.Invariant($"Exception from [Licence Code] {request.LicenceCode} | [Current Version] {request.CurrentVersionNumber}. Current Company: {companyCode}"), ex);

				var errorMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | {response.ErrorMessage}";
				logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.InternalServerError), SessionId.ToString(), ex: ex);
			}
			finally
			{
				var timeSpent = DateTime.Now - timeStart;
				infoMsg = $"UpgradePackageService.GetPackageUrl | {LicenceCode} | Processing request completed in {timeSpent.TotalSeconds} seconds";
				logger.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());
			}

			return response;
		}

		bool ShouldApplyDownloadSkipOptimization(LicenceDatabase database, ReleaseBuild releaseBuild)
		{
			return database.LD_EnablePackageDownloadOptimization && releaseBuild.HL_IsRolledOut;
		}

		#region Validate Request

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public bool ValidateRequestMessage(UpgradePackageUrlRequest request, out int databaseNumber, out UpgradeInfo upgradeInfo)
		{
			bool result = false;
			databaseNumber = -1;
			upgradeInfo = null;

			string requestMessage = "";

			try
			{
				var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
				requestMessage = encoder.Decrypt(request.EncryptedMessage);

				var doc = new XmlDocument();
				doc.LoadXml(requestMessage);

				// <LicenceCode>DDDAAAGPC</LicenceCode>
				var enterpriseCodeNode = doc.SelectNodes("//UpgradePackageRequest/LicenceCode").Cast<XmlNode>().FirstOrDefault();
				if (enterpriseCodeNode != null && enterpriseCodeNode.InnerText == request.LicenceCode)
				{
					result = true;
				}

				// <DatabaseNumber>1111</DatabaseNumber>
				var databaseNumberNode = doc.SelectNodes("//UpgradePackageRequest/DatabaseNumber").Cast<XmlNode>().FirstOrDefault();
				if (databaseNumberNode != null)
				{
					if (!int.TryParse(databaseNumberNode.InnerText, out databaseNumber))
					{
						databaseNumber = 0;
					}
				}

				// <CurrentVersionNumber>73a90a5d-131b-4755-930b-ea0166b90616	1.3.2010.0</CurrentVersionNumber>
				var versionNumberNode = doc.SelectNodes("//UpgradePackageRequest/CurrentVersionNumber").Cast<XmlNode>().FirstOrDefault();
				if (versionNumberNode != null)
				{
					upgradeInfo = UpgradeInfo.FromString(versionNumberNode.InnerText);
				}

				if (!string.IsNullOrWhiteSpace(request.CurrentVersionNumber))
				{
					var upgradeInfoThatMayHaveHigherVersion = UpgradeInfo.FromString(request.CurrentVersionNumber);
					if (upgradeInfo == null || upgradeInfoThatMayHaveHigherVersion.Version > upgradeInfo.Version)
					{
						upgradeInfo = upgradeInfoThatMayHaveHigherVersion;
					}
				}

				var infoMsg = $"UpgradePackageService.ValidateRequestMessage | {request.LicenceCode} | {enterpriseCodeNode?.InnerText} {databaseNumberNode?.InnerText} {versionNumberNode?.InnerText}";
				logger?.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = "Unable to successfully interpret the Upgrade Request Message." + System.Environment.NewLine
					+ "Request Message:" + System.Environment.NewLine + requestMessage
					+ System.Environment.NewLine + System.Environment.NewLine + ex.ToString();

				var errorMsg = $"UpgradePackageService.ValidateRequestMessage | {request.LicenceCode} | {message}";
				logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.InternalServerError), SessionId.ToString(), ex: ex);
				ErrorReporter.ReportOnce("Unable to interpret the Upgrade Request Message", ex.Message, ex);
			}

			return result;
		}

		#endregion

		#region Retrieve Data From Request

		LicenceDatabase GetDatabaseFromRequest(string licenceCode, int databaseNumber)
		{
			LicenceDatabase result;

			string serverCode = licenceCode.Substring(6, 3);
			if (databaseNumber > 0)
			{
				var query = new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, databaseNumber);
				query.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, serverCode);
				result = Factory.LoadTop1<LicenceDatabase>(query);
			}
			else
			{
				string enterpriseCode = licenceCode.Substring(0, 3);
				result = LicenceDatabase.Load(Factory, enterpriseCode, null, serverCode);
			}

			return result;
		}

		EDIOrgHeader GetOrganisationFromRequest(UpgradePackageUrlRequest request)
		{
			string enterpriseCode = request.LicenceCode.Substring(0, 3);
			string companyCode = request.LicenceCode.Substring(3, 3);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EDIOrgHeader));
			ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
			companySubQuery.AddToFilter(LicenceCompanySchema.LC_CompanyCode, companyCode);
			ZDBOnlySubQuery enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
			enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode);

			companySubQuery.AddSubQuery(enterpriseSubQuery, JoinCondition.And);
			query.AddSubQuery(companySubQuery, JoinCondition.And);

			return Factory.LoadTop1<EDIOrgHeader>(query);
		}

		#endregion

		#region Generate Upgrade Package URL

		(string packageUrl, string errorMessage) GenerateUpgradePackageUrl(LicenceDatabase database, ReleaseBuild build)
		{
			var packageUrl = string.Empty;
			var errorMessage = string.Empty;

			var enterpriseCode = database.LicEnterprise?.LE_EnterpriseCode ?? ZString.Empty;
			var packagePublishService = GetPackagePublishServce();

			// Calculate client code from build
			var (isCheckSuccessful, upgradeClientCode) = GetUpgradeClientCodeFromReleaseBuild(build, enterpriseCode);
			if (!isCheckSuccessful)
			{
				return (string.Empty, "Unable to access inactive upgrade package");
			}

			var versionNumber = build.VersionNumber;
			var packageName = ReleaseBuild.GetPackageName(versionNumber);

			// Check if it is non client specific build and package URL exist in cache, if so return URL
			var packageUrlCacheKey = $"PackageUrl_{packageName}";
			if (string.IsNullOrEmpty(upgradeClientCode) && MemoryCache.Default[packageUrlCacheKey] is string cachedPackageUrl && !string.IsNullOrEmpty(cachedPackageUrl))
			{
				return (cachedPackageUrl, string.Empty);
			}

			// Package file not yet on web server, build and send package file to web server
			// Lock is enforced to avoid concurrent accessing to temp files and creating duplicate package files with other webservice requests
			var lockName = string.IsNullOrEmpty(upgradeClientCode) ? "Generic" : "Client";
			var lockObject = GetLockObject(upgradeClientCode);
			bool acquiredLock = false;
			var tempPackageDirectory = Path.Combine(Env.TempPath, enterpriseCode);
			var tempPackageFilePath = string.Empty;
			var startGenZipFile = false;
			var isValidZipFile = false;

			try
			{
				int currentQueueCount = Interlocked.Increment(ref lockQueueCount);
				var infoMsg = $"UpgradePackageService.SendPackage | {LicenceCode} | Requesting lock ({lockName}) - queue size {currentQueueCount}";
				logger?.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());

				const int lockWaitingTimeout = 60 * 1000; // One minute - same as current effective send timeout
				Monitor.TryEnter(lockObject, lockWaitingTimeout, ref acquiredLock);
				if (acquiredLock)
				{
					infoMsg = $"UpgradePackageService.SendPackage | {LicenceCode} | Accquired lock ({lockName})";
					logger?.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());

					var status = packagePublishService.IsPackageAlreadyPublished(packageName, upgradeClientCode);  //Check again before trying to send build package

					if (status == Customs.Business.TriState.True)
					{
						packageUrl = PackagePathGenerator.GenerateHttpPath(upgradeClientCode, packageName);
					}
					else if (status == Customs.Business.TriState.False)
					{
						startGenZipFile = true;
						var builder = GetRuntimePackageBuilder(build, tempPackageDirectory);
						builder.Build(upgradeClientCode, database.IsHostedOnWiseCloud);
						tempPackageFilePath = builder.LastPackagePath;

						using (var zipFile = new ZipFile(tempPackageFilePath, null))
						{
							isValidZipFile = zipFile.TestArchive(true);
						}

						if (isValidZipFile)
						{
							string targetFolder = PackagePathGenerator.GenerateWebServerRootPath(upgradeClientCode);
							infoMsg = FormattableString.Invariant($"UpgradePackageService.SendPackage | {LicenceCode} | Sending Upgrade Package to Web Server: Package Path: {tempPackageFilePath ?? string.Empty} Target Folder: {targetFolder ?? string.Empty}");
							logger?.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());
							var isPublished = packagePublishService.PublishPackage(tempPackageFilePath, targetFolder);
							if (!string.IsNullOrEmpty(packagePublishService.LastErrorMessage))
							{
								var warnMsg = $"UpgradePackageService.SendPackage | {LicenceCode} | {packagePublishService.LastErrorMessage}";
								logger?.AddLog(LogLevel.Warn, warnMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
							}

							if (isPublished)
							{
								packageUrl = PackagePathGenerator.GenerateHttpPath(upgradeClientCode, Path.GetFileName(tempPackageFilePath));
							}
							else
							{
								errorMessage = "Failed to send package to web server";
								var errorMsg = $"UpgradePackageService.SendPackage | {LicenceCode} | {errorMessage}";
								logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
							}
						}
						else
						{
							errorMessage = "Upgrade package file is corrupted";
							var errorMsg = $"UpgradePackageService.SendPackage | {LicenceCode} | {errorMessage}";
							logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest), SessionId.ToString());
						}
					}

					if (string.IsNullOrEmpty(upgradeClientCode))
					{
						MemoryCache.Default.Set(packageUrlCacheKey, packageUrl, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(1) });
					}
				}
			}
			catch (IOException ioe)
			{
				if (startGenZipFile && !isValidZipFile)
				{
					errorMessage = "Upgrade package file is corrupted";
				}
				else
				{
					errorMessage = "Failed to send package to web server: " + ioe.Message;
				}
				var errorMsg = $"UpgradePackageService.SendPackage | {LicenceCode} | {errorMessage}";
				logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.InternalServerError), SessionId.ToString(), ex: ioe);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (startGenZipFile && !isValidZipFile)
				{
					errorMessage = "Upgrade package file is corrupted";
				}
				else
				{
					var key = "Failed to send package to web server";
					errorMessage = key + ": " + e.Message;
					ReportSendPackageError(key, e, build);
				}
			}
			finally
			{
				if (acquiredLock)
				{
					Monitor.Exit(lockObject);
					var infoMsg = $"UpgradePackageService.SendPackage | {LicenceCode} | Released lock ({lockName})";
					logger?.AddLog(LogLevel.Info, infoMsg, ((int)HttpStatusCode.OK), SessionId.ToString());
				}

				Interlocked.Decrement(ref lockQueueCount);

				try
				{
					if (!string.IsNullOrEmpty(tempPackageFilePath) && File.Exists(tempPackageFilePath))
					{
						File.Delete(tempPackageFilePath);
					}

					if (!string.IsNullOrEmpty(tempPackageDirectory) && Directory.Exists(tempPackageDirectory))
					{
						Directory.Delete(tempPackageDirectory);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}

			return (packageUrl, errorMessage);
		}

		protected virtual IPackagePublishService GetPackagePublishServce()
		{
			return new PackagePublishService();
		}

		protected virtual RuntimePackageBuilder GetRuntimePackageBuilder(ReleaseBuild build, string targetPath)
		{
			return new RuntimePackageBuilder(build, targetPath);
		}

		#region Get upgrade client code

		(bool, string) GetUpgradeClientCodeFromReleaseBuild(ReleaseBuild build, string enterpriseCode)
		{
			if (!build.HL_IsActive)
			{
				return (false, string.Empty);
			}

			var cacheKey = $"UpgradeClientCodes_{build.PK}";

			try
			{
				if (!(MemoryCache.Default[cacheKey] is List<string> clientCodes) || clientCodes.Count == 0)
				{
					clientCodes = build.ClientSpecificCodes.ToList();
					MemoryCache.Default.Set(cacheKey, clientCodes, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(12) });
				}
				var upgradeClientCode = clientCodes.Any(x => string.Equals(x, enterpriseCode.ToUpperInvariant())) ? enterpriseCode : string.Empty;
				return (true, upgradeClientCode);
			}
			catch (InvalidOperationException ex) when (ex.Message.Contains("Attempted to get client specific codes from a deleted release build", StringComparison.InvariantCultureIgnoreCase))
			{
				var errorMsg = $"UpgradePackageService.GetUpgradeClientCodeFromReleaseBuild | {LicenceCode} | {ex.Message} {build.VersionNumber}";
				logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.InternalServerError), SessionId.ToString(), ex: ex);
				ReportSendPackageError("Failed to get client specific codes from release build", ex, build);
			}
			catch (IOException ioe) when (ioe.Message.Contains("The network path was not found.", StringComparison.InvariantCultureIgnoreCase))
			{
				var errorMsg = $"UpgradePackageService.GetUpgradeClientCodeFromReleaseBuild | {LicenceCode} | {ioe.Message} {build.HL_PackagePath}";
				logger?.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.InternalServerError), SessionId.ToString(), ex: ioe);
			}

			return (false, null);
		}

		#endregion Get upgrade client code

		#region Locking

		static readonly object lockObjectGeneric = new object();
		static readonly object lockObjectClientExtension = new object();

		[SuppressThreadStaticFieldMessage]
		static volatile int lockQueueCount = 0;

		object GetLockObject(string upgradeClientCode)
		{
			return string.IsNullOrEmpty(upgradeClientCode) ? lockObjectGeneric : lockObjectClientExtension;
		}

		#endregion Locking

		#region Error report for send package

		protected void ReportSendPackageError(string key, Exception e, ReleaseBuild build)
		{
			ZStringBuilder message = new ZStringBuilder(e.Message);
			message.AppendLine();
			message.AppendLine(GetCurrentSystemInfo());
			message.AppendLine($"Build PK = {build?.PK.ToString() ?? "Missing"}");
			message.AppendLine($"Package Path = {build?.HL_PackagePath ?? "Missing"}");
			message.AppendLine($"Product = {build.HL_Product}");
			message.AppendLine($"Release = {build.HL_Release}");
			message.AppendLine($"Display Text = {build.FullDisplayText}");
			ReportSendPackageErrorCore(key, e, message.ToString());
		}

		protected virtual void ReportSendPackageErrorCore(string key, Exception e, string message)
		{
			new WebExceptionReporter().ReportWebException(e, key, message);
		}

		#endregion Error report for send package

		#endregion

		#region Ensure Only Run On EdiProd

		void EnsureOnlyRunOnEdiProd()
		{
			if (!EdiProdDbHelper.IsRunningOnEdiProdDatabase)
			{
				ZStringBuilder message = new ZStringBuilder("This function should only run on ediProd database");
				message.AppendLine();
				message.Append(GetCurrentSystemInfo());

				ErrorReporter.ReportOnce("UpgradeServiceNotRunningOnProd", message.ToString());
			}
		}

		string GetCurrentSystemInfo()
		{
			var ediProdLicenceIdentifier = SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value;
			var systemLicenceIdentifier = Env.CurrentCompany?.GetLicenceCode();
			ZStringBuilder message = new ZStringBuilder();
			message.AppendLine($"ediProdLicenceIdentifier = {ediProdLicenceIdentifier}");
			message.AppendLine($"systemLicenceIdentifier = {systemLicenceIdentifier ?? string.Empty}");

			return message.ToString();
		}

		#endregion

		#region Logging & Notification

#if DEBUG
		protected virtual
#endif
		void LogUpgradeInfo(UpgradePackageUrlRequest request, LicenceDatabase database, ReleaseBuild latestReleaseBuild)
		{
			var organisation = GetOrganisationFromRequest(request);
			if (organisation != null)
			{
				AddIfNotNew(latestReleaseBuild.Logs, Events.Delivered, organisation.OH_Code);
			}

			var currentVersionNumberFromRequest = request.CurrentVersionNumber;
			string currentVersionNumber = currentVersionNumberFromRequest != null && currentVersionNumberFromRequest.Length > 36
										? currentVersionNumberFromRequest.Substring(36).Trim()
										: currentVersionNumberFromRequest;
			string logMessage = string.Format(CultureInfo.CurrentCulture, "Upgrade Web Service Call: [Licence Code] {0} | [Latest Download] {1} | [Sent Version] {2}",
				request.LicenceCode, currentVersionNumber, latestReleaseBuild.ExeVersion);
			AddIfNotNew(database.Logs, Events.UpgradeSucceeded, logMessage);

			logMessage = string.Format(CultureInfo.CurrentCulture, "{0} (v{1}) will be sent to this client, {2} via {3} (WebService)",
				latestReleaseBuild.ReleaseDisplayText, latestReleaseBuild.ExeVersion, database.LD_ServerCode, UpgradeMethods.Codes.Http);
			AddIfNotNew(database.Logs, Events.UpgradeSucceeded, logMessage);

			Factory.Save();
		}

		void AddIfNotNew(Logs logs, Event @event, string message)
		{
			if (logs.MostRecentLogByEventTime(@event, message) == null)
			{
				logs.AddNew(@event, message);
			}
		}

		#endregion
	}
}
