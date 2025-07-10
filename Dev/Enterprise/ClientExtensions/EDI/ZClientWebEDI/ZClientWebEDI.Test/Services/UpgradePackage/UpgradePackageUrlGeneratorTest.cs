using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.Testing;
using WTG.DevTools.Definitions;

namespace Enterprise.ZClientWebCargoWiseEDI.Services.Test
{
	public class UpgradePackageUrlGeneratorTest : TestCaseWithFactory
	{
		#region Validate Request Message

		public void TestValidateRequestMessage()
		{
			SetupForRequest();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAASYD", 1000, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616	1.3.2010.0"));
			var encryptedMessage = encoder.Encrypt(message);

			int dbNumber = 0;
			UpgradeInfo upgradeInfo;

			UpgradePackageUrlRequest request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD" };
			AssertEquals(true, Generator.ValidateRequestMessage(request, out dbNumber, out upgradeInfo));
			AssertEquals(1000, dbNumber);
			AssertEquals("1.3.2010.0", upgradeInfo.Version.ToString());

			request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDBBBSYD" };
			AssertEquals(false, Generator.ValidateRequestMessage(request, out dbNumber, out upgradeInfo));

			request = new UpgradePackageUrlRequest() { EncryptedMessage = "Something Invalid", LicenceCode = "DDDAAASYD" };
			AssertEquals(false, Generator.ValidateRequestMessage(request, out dbNumber, out upgradeInfo));
			AssertEquals(typeof(CryptographicException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Unable to interpret the Upgrade Request Message", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestValidateRequestMessage_CalculateCorrectUpgradeInfo()
		{
			SetupForRequest();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			int dbNumber = 0;
			UpgradeInfo upgradeInfo;

			var message = GetRequestMessage("DDDAAASYD", 1000);
			var encryptedMessage = encoder.Encrypt(message);
			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD", CurrentVersionNumber = "73a90a5d-131b-4755-930b-ea0166b90616	1.3.2010.0" };
			AssertEquals("Precondition", true, Generator.ValidateRequestMessage(request, out dbNumber, out upgradeInfo));
			AssertEquals("73a90a5d-131b-4755-930b-ea0166b90616", upgradeInfo.PK.ToString());
			AssertEquals("1.3.2010.0", upgradeInfo.Version.ToString());

			message = GetRequestMessage("DDDAAASYD", 1000, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616	1.3.2011.0"));
			encryptedMessage = encoder.Encrypt(message);
			request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD", CurrentVersionNumber = "73a90a5d-131b-4755-930b-ea0166b90616	1.3.2010.0" };
			AssertEquals("Precondition", true, Generator.ValidateRequestMessage(request, out dbNumber, out upgradeInfo));
			AssertEquals("73a90a5d-131b-4755-930b-ea0166b90616", upgradeInfo.PK.ToString());
			AssertEquals("1.3.2011.0", upgradeInfo.Version.ToString());

			message = GetRequestMessage("DDDAAASYD", 1000, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616	1.3.2009.0"));
			encryptedMessage = encoder.Encrypt(message);
			request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD", CurrentVersionNumber = "73a90a5d-131b-4755-930b-ea0166b90616	1.3.2010.0" };
			AssertEquals("Precondition", true, Generator.ValidateRequestMessage(request, out dbNumber, out upgradeInfo));
			AssertEquals("73a90a5d-131b-4755-930b-ea0166b90616", upgradeInfo.PK.ToString());
			AssertEquals("1.3.2010.0", upgradeInfo.Version.ToString());
		}

		#endregion

		#region EDI restrition

		public void TestRunsOnEdiOnly()
		{
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "xxx");
			SetupForRequest();

			EDIDataRegistry.Instance.HttpGenericBaseUrl = "http://demo.test.com/ediEnterprise/Generic/";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "http://demo.test.com/ediEnterprise/ClientSpecific/";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAATST", 1033);
			var encryptedMessage = encoder.Encrypt(message);

			int upgradeToClientCountBefore = Factory.Load<UpgradesToClient>(new ZQuery()).Length;

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAATST" };
			var logger = new TestLogger();
			var response = Generator.GetPackageUrl(request);

			//assert non edi

			AssertEquals("Should send correct error report", @"This function should only run on ediProd database
ediProdLicenceIdentifier = xxx
systemLicenceIdentifier = EDIEDIDAT
", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			//setup edi
			SetupEdi();

			response = Generator.GetPackageUrl(request);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		#endregion

		#region Get Package URL

		public void TestGetPackageURL()
		{
			SetupForRequest();

			EDIDataRegistry.Instance.HttpGenericBaseUrl = "http://demo.test.com/ediEnterprise/Generic/";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "http://demo.test.com/ediEnterprise/ClientSpecific/";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAATST", 1033);
			var encryptedMessage = encoder.Encrypt(message);

			int upgradeToClientCountBefore = Factory.Load<UpgradesToClient>(new ZQuery()).Length;

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAATST" };
			var response = Generator.GetPackageUrl(request);

			AssertEquals("", response.ErrorMessage);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("http://demo.test.com/ediEnterprise/ClientSpecific/DDD/Package20050704_000000_1_3_2011_23.edp", response.URL);
			AssertEquals("1.3.2011.23", response.VersionNumber);

			message = GetRequestMessage("DDDAAASYD", 1024, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616	1.3.1973.0"));
			encryptedMessage = encoder.Encrypt(message);
			request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD", CurrentVersionNumber = "73a90a5d-131b-4755-930b-ea0166b90616 1.3.1973.0" };
			ErrorReporter.Clear();
			response = Generator.GetPackageUrl(request);

			AssertEquals("Reports argument exception in ValidateRequestMessage", typeof(ArgumentException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Unable to interpret the Upgrade Request Message", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertEquals("", response.ErrorMessage);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("http://demo.test.com/ediEnterprise/Generic/Package20050703_000000_1_3_2010_44.edp", response.URL);
			AssertEquals("1.3.2010.44", response.VersionNumber);

			int upgradeToClientCountAfter = new BusinessObjectFactory().Load<UpgradesToClient>(new ZQuery()).Length;
			AssertEquals("No new added upgrade to client in database", upgradeToClientCountBefore, upgradeToClientCountAfter);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UpgradeSucceeded.Code);
			var logs = Database1.Logs.Find(query);
			AssertEquals(2, logs.Length);
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "Upgrade Web Service Call: [Licence Code] DDDAAASYD | [Latest Download] 1.3.1973.0 | [Sent Version] 1.3.2010.44"));
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "GP Release 2005 Jul 03 patch 44 (v1.3.2010.44) will be sent to this client, SYD via HTP (WebService)"));

			logs = Database2.Logs.Find(query);
			AssertEquals(2, logs.Length);
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "Upgrade Web Service Call: [Licence Code] DDDAAATST | [Latest Download]  | [Sent Version] 1.3.2011.23"));
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "Alpha Release 2005 Jul 04 patch 23 (v1.3.2011.23) will be sent to this client, TST via HTP (WebService)"));

			Generator.GetPackageUrl(request);
			AssertEquals("Reports argument exception in ValidateRequestMessage", typeof(ArgumentException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Unable to interpret the Upgrade Request Message", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			Database1.Logs.GetAllLogs().Reload(true);
			Database2.Logs.GetAllLogs().Reload(true);
			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UpgradeSucceeded.Code);
			logs = Database1.Logs.Find(query);
			AssertEquals("Log duplicates not made when request repeats", 2, logs.Length);
			logs = Database2.Logs.Find(query);
			AssertEquals("Log duplicates not made when request repeats", 2, logs.Length);
		}

		public void TestGetPackageURL_ReleaseRingIsGpcAndGpcIsNotAvailable()
		{
			SetupForRequest();

			var gpcLicenceDatabase = Factory.New<LicenceDatabase>();
			gpcLicenceDatabase.LD_LE = Enterprise.PK;
			gpcLicenceDatabase.LD_ServerCode = "GPC";
			gpcLicenceDatabase.LD_ReleaseRing = ReleaseRings.Codes.GPC;
			gpcLicenceDatabase.LD_DatabaseNumber = 1111;

			var availableBuildGPC = CreateNewReleaseBuild(AvailableBuildGPR.HL_MajorVersion, AvailableBuildGPR.HL_MinorVersion, AvailableBuildGPR.HL_Release, 10, false, ReleaseRings.Codes.GPC);

			Factory.Save();

			AssertEquals("Precondition: AvailableBuildGPC.VersionNumber > AvailableBuildGPR.VersionNumber", false, availableBuildGPC.VersionNumber > AvailableBuildGPR.VersionNumber);

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAAGPC", 1111, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616	1.3.2010.0"));
			var encryptedMessage = encoder.Encrypt(message);
			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAAGPC" };
			var logger = new TestLogger();
			var response = Generator.GetPackageUrl(request);

			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("return GPR as GPC is not available", "1.3.2010.44", response.VersionNumber);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UpgradeSucceeded.Code);
			var logs = gpcLicenceDatabase.Logs.Find(query);
			AssertEquals(2, logs.Length);
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "Upgrade Web Service Call: [Licence Code] DDDAAAGPC | [Latest Download]  | [Sent Version] 1.3.2010.44"));
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "GP Release 2005 Jul 03 patch 44 (v1.3.2010.44) will be sent to this client, GPC via HTP (WebService)"));
		}

		public void TestGetPackageURL_ReleaseRingIsGpcAndGpcIsAvailable()
		{
			SetupForRequest();

			var gpcLicenceDatabase = Factory.New<LicenceDatabase>();
			gpcLicenceDatabase.LD_LE = Enterprise.PK;
			gpcLicenceDatabase.LD_ServerCode = "GPC";
			gpcLicenceDatabase.LD_ReleaseRing = ReleaseRings.Codes.GPC;
			gpcLicenceDatabase.LD_DatabaseNumber = 1111;

			var availableBuildGPC = CreateNewReleaseBuild(AvailableBuildGPR.HL_MajorVersion, AvailableBuildGPR.HL_MinorVersion, AvailableBuildGPR.HL_Release, 80, false, ReleaseRings.Codes.GPC);

			Factory.Save();

			AssertEquals("Precondition: AvailableBuildGPC.VersionNumber > AvailableBuildGPR.VersionNumber", true, availableBuildGPC.VersionNumber > AvailableBuildGPR.VersionNumber);

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAAGPC", 1111, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616	1.3.2010.0"));
			var encryptedMessage = encoder.Encrypt(message);
			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAAGPC" };
			var logger = new TestLogger();
			var response = Generator.GetPackageUrl(request);

			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("return GPC as it is available", "1.3.2010.80", response.VersionNumber);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UpgradeSucceeded.Code);
			var logs = gpcLicenceDatabase.Logs.Find(query);
			AssertEquals("Two upgrades have been sent, total 2 logs", 2, logs.Length);
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "Upgrade Web Service Call: [Licence Code] DDDAAAGPC | [Latest Download]  | [Sent Version] 1.3.2010.80"));
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "GP Candidate 2005 Jul 03 patch 80 (v1.3.2010.80) will be sent to this client, GPC via HTP (WebService)"));
		}

		public void TestGetPackageURL_InvalidRequest()
		{
			SetupForRequest();

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = "Something Invalid", LicenceCode = "DDDBBBSYD" };
			var logger = new TestLogger();
			ErrorReporter.Clear();
			var response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("Bad request: invalid request message", response.ErrorMessage);
			AssertEquals(typeof(CryptographicException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Unable to interpret the Upgrade Request Message", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAASY", 0);
			request.EncryptedMessage = encoder.Encrypt(message);
			request.LicenceCode = "DDDAAASY";
			response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("Bad request: invalid licence code", response.ErrorMessage);

			message = GetRequestMessage("DDDAAASYD", 999);
			request.EncryptedMessage = encoder.Encrypt(message);
			request.LicenceCode = "DDDAAASYD";
			response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("Unable to identify database from licence code DDDAAASYD / database number 999", response.ErrorMessage);

			message = GetRequestMessage("DDDAAAYOU", 1024);
			request.EncryptedMessage = encoder.Encrypt(message);
			request.LicenceCode = "DDDAAAYOU";
			response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("Unable to identify database from licence code DDDAAAYOU / database number 1024", response.ErrorMessage);

			message = GetRequestMessage("DDDAAANEW", 1080);
			request.EncryptedMessage = encoder.Encrypt(message);
			request.LicenceCode = "DDDAAANEW";
			response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("Unable to identify current release ring from licence code DDDAAANEW / database number 1080", response.ErrorMessage);

			message = GetRequestMessage("DDDAAABBB", 1100);
			request.EncryptedMessage = encoder.Encrypt(message);
			request.LicenceCode = "DDDAAABBB";
			response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("The database does not support HTTP upgrade", response.ErrorMessage);
		}

		public void TestGetPackageUrl_DatabaseRequiresBuildTesting()
		{
			SetupForRequest();

			AvailableBuildALP.HL_IsTestPassed = false;
			AvailableBuildALP.HL_TestDateUtc = new ZDateTime(2020, 3, 5);
			AvailableBuildALP.Factory.Save();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAATST", 1033);
			var encryptedMessage = encoder.Encrypt(message);

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAATST" };
			var response = new GeneratorForTest(new NLogWrapperForTest(GetType())).GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("http://www.cargowise.com/ftpmirror/ediEnterprise/ClientSpecific/DDD/Package20050704_000000_1_3_2011_23.edp", response.URL);

			var dbList = new CodeDescriptionPairList();
			dbList.AddPair("1033", "DDDAAATST");
			EDIDataRegistry.Instance.DatabasesRequiredReleaseBuildTesting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dbList);

			response = new GeneratorForTest(new NLogWrapperForTest(GetType())).GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals(string.Empty, response.URL);
			AssertEquals("System requires build to be tested. The latest build 1.3.2011.23 has not yet passed integration test.", response.ErrorMessage);

			AvailableBuildALP.HL_IsTestPassed = true;
			AvailableBuildALP.Factory.Save();
			response = new GeneratorForTest(new NLogWrapperForTest(GetType())).GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("http://www.cargowise.com/ftpmirror/ediEnterprise/ClientSpecific/DDD/Package20050704_000000_1_3_2011_23.edp", response.URL);
		}

		public void TestGetPackageUrl_MultipleLocking()
		{
			SetupForRequest();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var logger = new NLogWrapperForTest(GetType());

			var message1 = GetRequestMessage("DDDAAATST", 1033);
			var encryptedMessage1 = encoder.Encrypt(message1);
			var request1 = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage1, LicenceCode = "DDDAAATST" };

			var message2 = GetRequestMessage("DDDAAASYD", 1024);
			var encryptedMessage2 = encoder.Encrypt(message2);
			var request2 = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage2, LicenceCode = "DDDAAASYD" };

			new GeneratorForTest(logger) { IsPackagePublished = false }.GetPackageUrl(request1);
			new GeneratorForTest(logger) { IsPackagePublished = false }.GetPackageUrl(request2);

			var logtext = string.Join(System.Environment.NewLine, logger.LogEntries);

			AssertContains("Accquired lock (Client)", logtext);
			AssertContains("Accquired lock (Generic)", logtext);
		}

		public void TestGetPackageUrl_ClientCodesCache()
		{
			SetupForRequest();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var logger = new NLogWrapperForTest(GetType());

			var message1 = GetRequestMessage("DDDAAATST", 1033);
			var encryptedMessage1 = encoder.Encrypt(message1);
			var request1 = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage1, LicenceCode = "DDDAAATST" };

			var message2 = GetRequestMessage("DDDAAASYD", 1024);
			var encryptedMessage2 = encoder.Encrypt(message2);
			var request2 = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage2, LicenceCode = "DDDAAASYD" };

			AssertEquals(0, MemoryCache.Default.Count(x => x.Key.StartsWith("UpgradeClientCodes_")));

			new GeneratorForTest(logger) { IsPackagePublished = false }.GetPackageUrl(request1);

			var clientCodeCache1 = MemoryCache.Default[$"UpgradeClientCodes_{AvailableBuildALP.PK}"] as List<string>;
			AssertNotNull(clientCodeCache1);
			AssertEquals(1, clientCodeCache1.Count);
			AssertEquals("DDD", clientCodeCache1[0]);

			new GeneratorForTest(logger) { IsPackagePublished = false }.GetPackageUrl(request2);
			var clientCodeCache2 = MemoryCache.Default[$"UpgradeClientCodes_{AvailableBuildGPR.PK}"] as List<string>;
			AssertNotNull(clientCodeCache2);
		}

		public void TestGetPackageUrl_PackageUrlCache()
		{
			SetupForRequest();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var logger = new NLogWrapperForTest(GetType());

			var message1 = GetRequestMessage("DDDAAATST", 1033);
			var encryptedMessage1 = encoder.Encrypt(message1);
			var request1 = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage1, LicenceCode = "DDDAAATST" };

			var message2 = GetRequestMessage("FFFBBBPRD", 9527);
			var encryptedMessage2 = encoder.Encrypt(message2);
			var request2 = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage2, LicenceCode = "FFFBBBPRD" };

			AssertEquals(0, MemoryCache.Default.Count(x => x.Key.StartsWith("PackageUrl_")));

			new GeneratorForTest(logger) { IsPackagePublished = false }.GetPackageUrl(request1);

			var urlCache = MemoryCache.Default.FirstOrDefault(x => x.Key.StartsWith("PackageUrl_")).Value;
			AssertNull("Client specific URL should not be added to cache", urlCache);

			new GeneratorForTest(logger) { IsPackagePublished = false }.GetPackageUrl(request2);
			urlCache = MemoryCache.Default.FirstOrDefault(x => x.Key.StartsWith("PackageUrl_")).Value;
			AssertNotNull(urlCache);
			AssertEquals("Generic URL should be added to cache", $"http://www.cargowise.com/ftpmirror/ediEnterprise/Generic/{PackageFileName}", urlCache);
		}

		public void TestGetPackageURL_InvalidZipFile()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupForRequest();

			EDIDataRegistry.Instance.HttpGenericBaseUrl = "http://demo.test.com/ediEnterprise/Generic/";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "http://demo.test.com/ediEnterprise/ClientSpecific/";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAASYD", 1024, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616\t1.3.1973.0"));
			var encryptedMessage = encoder.Encrypt(message);

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD" };
			var response = new GeneratorForTest(logger) { IsPackagePublished = false, ShouldBuildBadZipPackage = true }.GetPackageUrl(request);

			AssertEquals("Upgrade package file is corrupted", response.ErrorMessage);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);

			var expectedLogMessages = new string[] {
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Start processing request",
$@"UpgradePackageService.ValidateRequestMessage | DDDAAASYD | DDDAAASYD 1024 73a90a5d-131b-4755-930b-ea0166b90616\t1.3.1973.0",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Start getting latest release build",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Getting latest release build completed",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Start generating package URL",
$@"UpgradePackageService.SendPackage | DDDAAASYD | Requesting lock (Generic) - queue size 1",
$@"UpgradePackageService.SendPackage | DDDAAASYD | Accquired lock (Generic)",
$@"UpgradePackageService.SendPackage | DDDAAASYD | Upgrade package file is corrupted",
$@"UpgradePackageService.SendPackage | DDDAAASYD | Released lock (Generic)",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Generating package URL completed: 1.3.2010.44",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Upgrade package file is corrupted",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Processing request completed" };

			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestGetPackageUrl_ShouldReturnEmptyUrlWhenDownloadSkipOptimizationApplied()
		{
			// Arrange
			SetupForRequest();

			Database1.LD_EnablePackageDownloadOptimization = true;
			AvailableBuildGPR.HL_IsRolledOut = true;
			Factory.Save();

			EDIDataRegistry.Instance.HttpGenericBaseUrl = "http://demo.test.com/ediEnterprise/Generic/";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "http://demo.test.com/ediEnterprise/ClientSpecific/";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAASYD", 1024);
			var encryptedMessage = encoder.Encrypt(message);

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD", IsPatchOnly = false };
			// Act
			var response = Generator.GetPackageUrl(request);

			// Assert
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals($"The package {AvailableBuildGPR.VersionNumber} has been rolled out to CargoWise Cloud", response.ErrorMessage);
		}

		public void TestGetPackageUrl_ShouldReturnUrlWhenDownloadSkipOptimizationDisabled()
		{
			// Arrange
			SetupForRequest();

			Database1.LD_EnablePackageDownloadOptimization = false;
			AvailableBuildGPR.HL_IsRolledOut = true;
			Factory.Save();

			EDIDataRegistry.Instance.HttpGenericBaseUrl = "http://demo.test.com/ediEnterprise/Generic/";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "http://demo.test.com/ediEnterprise/ClientSpecific/";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAASYD", 1024);
			var encryptedMessage = encoder.Encrypt(message);

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD", IsPatchOnly = false };

			// Act
			var response = Generator.GetPackageUrl(request);

			// Assert
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals(EDIDataRegistry.Instance.HttpGenericBaseUrl + "Package20050703_000000_1_3_2010_44.edp", response.URL);
			AssertEquals("", response.ErrorMessage);
		}

		public void TestGetPackageUrl_ShouldReturnUrlWhenReleaseBuildNotRolledOut()
		{
			// Arrange
			SetupForRequest();

			Database1.LD_EnablePackageDownloadOptimization = true;
			AvailableBuildGPR.HL_IsRolledOut = false;
			Factory.Save();

			EDIDataRegistry.Instance.HttpGenericBaseUrl = "http://demo.test.com/ediEnterprise/Generic/";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "http://demo.test.com/ediEnterprise/ClientSpecific/";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAASYD", 1024);
			var encryptedMessage = encoder.Encrypt(message);

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD", IsPatchOnly = false };

			// Act
			var response = Generator.GetPackageUrl(request);

			// Assert
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals(EDIDataRegistry.Instance.HttpGenericBaseUrl + "Package20050703_000000_1_3_2010_44.edp", response.URL);
			AssertEquals("", response.ErrorMessage);
		}
		#endregion

		#region Weekly Build

		public void TestGetPackageUrl_WeeklyBuildEnabled_WtgHostedClient()
		{
			GetPackageUrl_WeeklyBuildCutOff_Helper();
		}

		public void TestGetPackageUrl_WeeklyBuildEnabled_SelfHostedClient()
		{
			GetPackageUrl_WeeklyBuildCutOff_Helper(true);
		}

		public void TestGetPackageUrl_WeeklyBuildEnabled_WtgHostedClient_WeeklyBuildSuperseded()
		{
			GetPackageUrl_WeeklyBuildCutOff_Helper(false, false);
		}

		public void TestGetPackageUrl_WeeklyBuildEnabled_WtgHostedClient_WeeklyBuildAfterCutOffDate()
		{
			GetPackageUrl_WeeklyBuildCutOff_Helper(false, true, true);
		}

		public void TestGetPackageUrl_WeeklyBuildEnabled_WtgHostedClient_WeeklyBuild_WhenInternalHostedSystem()
		{
			GetPackageUrl_WeeklyBuildCutOff_Helper(false, true, true, true);
		}

		void GetPackageUrl_WeeklyBuildCutOff_Helper(bool isSelfHostedClient = false, bool isWeeklyCutOffBuildActive = true, bool isWeeklyBuildAfterCutOffDate = false, bool isInternal = false)
		{
			SetupForRequest(true, !isSelfHostedClient, isInternal);

			Func<LicenceDatabase, bool> isHostedWithCargoWise = (db) => EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetBoolFromCode(db.LD_HostedLocation);

			Assert(isHostedWithCargoWise(Database1) == !isSelfHostedClient);
			Assert(isHostedWithCargoWise(Database2) == !isSelfHostedClient);
			Assert(isHostedWithCargoWise(Database3) == !isSelfHostedClient);
			Assert(isHostedWithCargoWise(Database4) == !isSelfHostedClient);

			if (!isWeeklyCutOffBuildActive)
			{
				AvailableBuildGPRWeeklyCutOff.HL_Superceded = true;
				Factory.Save();
			}

			if (isWeeklyBuildAfterCutOffDate)
			{
				LastBuildCutOffDate = AvailableBuildGPRWeeklyCutOff.HL_ExeVersionDate.ToDateTime().AddMinutes(-1);
				EDIDataRegistry.Instance.WeeklyBuildCutOffTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, LastBuildCutOffDate.ToShortTimeString());
			}

			var versionNumber = Guid.NewGuid().ToString() + "\t1.3.1973.0";
			var message = GetRequestMessage("DDDAAASYD", 1024, UpgradeInfo.FromString(versionNumber));
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var encryptedMessage = encoder.Encrypt(message);
			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD", CurrentVersionNumber = versionNumber };
			var logger = new TestLogger();
			var response = Generator.GetPackageUrl(request);

			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);

			if (isSelfHostedClient || !isWeeklyCutOffBuildActive || isInternal)
			{
				AssertEquals("the lastest active version is expected", AvailableBuildGPR.VersionNumber.ToString(), response.VersionNumber);
			}
			else
			{
				AssertEquals("the weekly cut-off build version is expected", AvailableBuildGPRWeeklyCutOff.VersionNumber.ToString(), response.VersionNumber);
			}
		}

		#endregion

		string GetRequestMessage(string licenceCode, int databaseNumber, UpgradeInfo upgradeInfo = null)
		{
			return UpgradePackageServiceFactory.GetRawMessage(licenceCode, databaseNumber, upgradeInfo ?? new UpgradeInfo(Guid.Empty, 0, 0, 0, 0));
		}

		public void TestGetPackageURL_NoNewerBuildVersionFound_RespondSuccessWithVersionNumberAsZero()
		{
			ErrorReporter.Clear();
			SetupForRequest();

			var versionNumber = Guid.NewGuid().ToString() + "\t99.99.99.99";
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAATST", 1033, UpgradeInfo.FromString(versionNumber));
			var encryptedMessage = encoder.Encrypt(message);
			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAATST", CurrentVersionNumber = versionNumber, IsPatchOnly = true };
			var logger = new TestLogger();
			var response = Generator.GetPackageUrl(request);

			CombineAssertions(() =>
			{
				AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
				AssertEquals("0.0.0.0", response.VersionNumber);
				AssertEquals("No available release build found", response.ErrorMessage);
				AssertEquals(string.Empty, response.URL);
			});
		}

		#region Logging and Error Reporting

		public void TestLogging()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupForRequest();

			EDIDataRegistry.Instance.HttpGenericBaseUrl = "http://demo.test.com/ediEnterprise/Generic/";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "http://demo.test.com/ediEnterprise/ClientSpecific/";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAASYD", 1024, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616\t1.3.1973.0"));
			var encryptedMessage = encoder.Encrypt(message);

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD" };
			var response = new GeneratorForTest(logger) { IsPackagePublished = false }.GetPackageUrl(request);

			AssertEquals("", response.ErrorMessage);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals($"http://demo.test.com/ediEnterprise/Generic/{PackageFileName}", response.URL);

			var expectedLogMessages = new string[] {
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Start processing request",
$@"UpgradePackageService.ValidateRequestMessage | DDDAAASYD | DDDAAASYD 1024 73a90a5d-131b-4755-930b-ea0166b90616\t1.3.1973.0",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Start getting latest release build",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Getting latest release build completed",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Start generating package URL",
$@"UpgradePackageService.SendPackage | DDDAAASYD | Requesting lock (Generic) - queue size 1",
$@"UpgradePackageService.SendPackage | DDDAAASYD | Accquired lock (Generic)",
$@"UpgradePackageService.SendPackage | DDDAAASYD | Sending Upgrade Package to Web Server: Package Path: {Env.TempPath}DDD\{PackageFileName} Target Folder: \\web\updates\ediEnterprise\Generic\",
$@"UpgradePackageService.SendPackage | DDDAAASYD | Released lock (Generic)",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Generating package URL completed: 1.3.2010.44 http://demo.test.com/ediEnterprise/Generic/{PackageFileName}",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Processing request completed" };

			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestLogging_InvalidRequest()
		{
			var logger = new NLogWrapperForTest(GetType());
			var request = new UpgradePackageUrlRequest() { EncryptedMessage = "Something Invalid", LicenceCode = "DDDAAASYD" };
			new GeneratorForTest(logger).GetPackageUrl(request);

			var expectedLogMessages = new string[] {
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Start processing request",
$@"UpgradePackageService.ValidateRequestMessage | DDDAAASYD | Unable to successfully interpret the Upgrade Request Message",
$@"UpgradePackageService.GetPackageUrl | DDDAAASYD | Bad request: invalid request message" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			ErrorReporter.Clear();
		}

		public void TestLogging_UnhandledException()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupForRequest();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAATST", 1033, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616\t1.3.2011.23"));
			var encryptedMessage = encoder.Encrypt(message);

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAATST" };
			var generator = new GeneratorForTest(logger) { ShouldThrowUnhandledException = true };
			var response = generator.GetPackageUrl(request);

			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Error, response.ResponseClass);
			AssertContains("Unhandled exception: System.Exception: Stop working...", response.ErrorMessage);

			AssertEquals("Unhandled exception from upgrade web service call", ErrorReporter.LastKeyReported);
			AssertEquals(FormattableString.Invariant($"Exception from [Licence Code] DDDAAATST | [Current Version] . Current Company: {GlbCompany.CurrentCompany.GC_Code}"), ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();

			var expectedLogMessages = new string[] {
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Start processing request",
$@"UpgradePackageService.ValidateRequestMessage | DDDAAATST | DDDAAATST 1033 73a90a5d-131b-4755-930b-ea0166b90616\t1.3.2011.23",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Start getting latest release build",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Getting latest release build completed",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Start generating package URL",
$@"UpgradePackageService.SendPackage | DDDAAATST | Requesting lock (Client) - queue size 1",
$@"UpgradePackageService.SendPackage | DDDAAATST | Accquired lock (Client)",
$@"UpgradePackageService.SendPackage | DDDAAATST | Released lock (Client)",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Generating package URL completed: 1.3.2011.23 http://www.cargowise.com/ftpmirror/ediEnterprise/ClientSpecific/DDD/Package20050704_000000_1_3_2011_23.edp",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Unhandled exception: System.Exception: Stop working..." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestLogging_SendPackageException_NetworkPathNotFound()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupForRequest();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAATST", 1033, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616\t1.3.2011.23"));
			var encryptedMessage = encoder.Encrypt(message);
			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAATST" };

			var generatorWithEx = new GeneratorForTest(logger) { IsPackagePublished = false, ShouldThrowSendPackageException = true };
			var response = generatorWithEx.GetPackageUrl(request);

			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals(string.Empty, response.URL);
			AssertEquals(null, response.VersionNumber);
			AssertEquals("Failed to send package to web server: The network path was not found.", response.ErrorMessage);

			var expectedLogMessages = new string[] {
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Start processing request",
$@"UpgradePackageService.ValidateRequestMessage | DDDAAATST | DDDAAATST 1033 73a90a5d-131b-4755-930b-ea0166b90616\t1.3.2011.23",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Start getting latest release build",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Getting latest release build completed",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Start generating package URL",
$@"UpgradePackageService.SendPackage | DDDAAATST | Requesting lock (Client) - queue size 1",
$@"UpgradePackageService.SendPackage | DDDAAATST | Accquired lock (Client)",
$@"UpgradePackageService.SendPackage | DDDAAATST | Sending Upgrade Package to Web Server: Package Path: {Env.TempPath}DDD\{PackageFileName} Target Folder: \\web\updates\ediEnterprise\ClientSpecific\DDD",
$@"UpgradePackageService.SendPackage | DDDAAATST | Failed to send package to web server: The network path was not found.",
$@"UpgradePackageService.SendPackage | DDDAAATST | Released lock (Client)",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Generating package URL completed: 1.3.2011.23 " };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestLogging_SendPackageErrorMessage()
		{
			var logger = new NLogWrapperForTest(GetType());
			SetupForRequest();

			EDIDataRegistry.Instance.HttpGenericBaseUrl = "http://demo.test.com/ediEnterprise/Generic/";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "http://demo.test.com/ediEnterprise/ClientSpecific/";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAATST", 1033, UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616\t1.3.2011.23"));
			var encryptedMessage = encoder.Encrypt(message);
			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAATST" };

			var generatorWithEx = new GeneratorForTest(logger) { IsPackagePublished = false, ShouldAddSendPackageErrorMessage = true };
			var response = generatorWithEx.GetPackageUrl(request);

			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertNotEquals(string.Empty, response.URL);

			var expectedLogMessages = new string[] {
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Start processing request",
$@"UpgradePackageService.ValidateRequestMessage | DDDAAATST | DDDAAATST 1033 73a90a5d-131b-4755-930b-ea0166b90616\t1.3.2011.23",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Start getting latest release build",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Getting latest release build completed",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Start generating package URL",
$@"UpgradePackageService.SendPackage | DDDAAATST | Requesting lock (Client) - queue size 1",
$@"UpgradePackageService.SendPackage | DDDAAATST | Accquired lock (Client)",
$@"UpgradePackageService.SendPackage | DDDAAATST | Sending Upgrade Package to Web Server: Package Path: {Env.TempPath}DDD\{PackageFileName} Target Folder: \\web\updates\ediEnterprise\ClientSpecific\DDD",
$@"UpgradePackageService.SendPackage | DDDAAATST | Failed to delete old file. The network path was not found.",
$@"UpgradePackageService.SendPackage | DDDAAATST | Released lock (Client)",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Generating package URL completed: 1.3.2011.23 http://demo.test.com/ediEnterprise/ClientSpecific/DDD/{PackageFileName}",
$@"UpgradePackageService.GetPackageUrl | DDDAAATST | Processing request completed" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		#endregion

		#region Process Legacy Request

		public void TestValidateRequestMessage_LegacyRequest()
		{
			SetupForLegacyRequest();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			string message = GetLegacyRequestMessage("DDDAAASYD");
			string encryptedMessage = encoder.Encrypt(message);

			int dbNumber = 0;
			UpgradeInfo upgradeInfo;

			UpgradePackageUrlRequest request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD" };
			AssertEquals(true, Generator.ValidateRequestMessage(request, out dbNumber, out upgradeInfo));

			request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDBBBSYD" };
			AssertEquals(false, Generator.ValidateRequestMessage(request, out dbNumber, out upgradeInfo));

			request = new UpgradePackageUrlRequest() { EncryptedMessage = "Something Invalid", LicenceCode = "DDDAAASYD" };
			AssertEquals(false, Generator.ValidateRequestMessage(request, out dbNumber, out upgradeInfo));
			AssertEquals(typeof(CryptographicException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Unable to interpret the Upgrade Request Message", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetPackageURL_LegacyRequest()
		{
			SetupForLegacyRequest();

			EDIDataRegistry.Instance.HttpGenericBaseUrl = "http://demo.test.com/ediEnterprise/Generic/";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "http://demo.test.com/ediEnterprise/ClientSpecific/";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetLegacyRequestMessage("DDDAAATST");
			var encryptedMessage = encoder.Encrypt(message);

			int upgradeToClientCountBefore = Factory.Load<UpgradesToClient>(new ZQuery()).Length;

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAATST" };
			var logger = new TestLogger();
			var response = Generator.GetPackageUrl(request);

			AssertEquals("", response.ErrorMessage);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("http://demo.test.com/ediEnterprise/ClientSpecific/DDD/Package20050704_000000_1_3_2011_23.edp", response.URL);
			AssertEquals("1.3.2011.23", response.VersionNumber);

			message = GetLegacyRequestMessage("DDDAAASYD", UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616	1.3.1973.0"));
			encryptedMessage = encoder.Encrypt(message);
			request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAASYD", CurrentVersionNumber = "73a90a5d-131b-4755-930b-ea0166b90616 1.3.1973.0" };
			response = Generator.GetPackageUrl(request);
			ClearErrorReporterXmlMessage();

			AssertEquals("", response.ErrorMessage);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("http://demo.test.com/ediEnterprise/Generic/Package20050703_000000_1_3_2010_44.edp", response.URL);
			AssertEquals("1.3.2010.44", response.VersionNumber);

			int upgradeToClientCountAfter = new BusinessObjectFactory().Load<UpgradesToClient>(new ZQuery()).Length;
			AssertEquals("No new added upgrade to client in database", upgradeToClientCountBefore, upgradeToClientCountAfter);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UpgradeSucceeded.Code);
			var logs = Database1.Logs.Find(query);
			AssertEquals(2, logs.Length);
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "Upgrade Web Service Call: [Licence Code] DDDAAASYD | [Latest Download] 1.3.1973.0 | [Sent Version] 1.3.2010.44"));
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "GP Release 2005 Jul 03 patch 44 (v1.3.2010.44) will be sent to this client, SYD via HTP (WebService)"));

			logs = Database2.Logs.Find(query);
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "Upgrade Web Service Call: [Licence Code] DDDAAATST | [Latest Download]  | [Sent Version] 1.3.2011.23"));
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "Alpha Release 2005 Jul 04 patch 23 (v1.3.2011.23) will be sent to this client, TST via HTP (WebService)"));

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Delivered.Code);
			logs = AvailableBuildALP.Logs.Find(query);
			AssertEquals("One log for ALP build delivery", 1, logs.Length);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Delivered.Code);
			logs = AvailableBuildGPR.Logs.Find(query);
			AssertEquals("One log for GPR build delivery", 1, logs.Length);
		}

		public void TestGetPackageURL_ReleaseRingIsGpcAndGpcIsNotAvailable_LegacyRequest()
		{
			SetupForLegacyRequest();

			LicenceDatabase gpcLicenceDatabase = Organisation.LicCompany.LicDatabases.AddNew();
			gpcLicenceDatabase.LD_ServerCode = "GPC";
			gpcLicenceDatabase.LD_ReleaseRing = ReleaseRings.Codes.GPC;

			var availableBuildGPC = CreateNewReleaseBuild(AvailableBuildGPR.HL_MajorVersion, AvailableBuildGPR.HL_MinorVersion, AvailableBuildGPR.HL_Release, 10, false, ReleaseRings.Codes.GPC);

			Factory.Save();

			AssertEquals("Precondition: AvailableBuildGPC.VersionNumber > AvailableBuildGPR.VersionNumber", false, availableBuildGPC.VersionNumber > AvailableBuildGPR.VersionNumber);

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			string message = GetLegacyRequestMessage("DDDAAAGPC", UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616	1.3.2010.0"));
			string encryptedMessage = encoder.Encrypt(message);
			UpgradePackageUrlRequest request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAAGPC" };

			var logger = new TestLogger();
			UpgradePackageUrlResponse response = Generator.GetPackageUrl(request);

			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("return GPR as GPC is not available", "1.3.2010.44", response.VersionNumber);

			ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UpgradeSucceeded.Code);
			StmALog[] logs = gpcLicenceDatabase.Logs.Find(query);
			AssertEquals("Two upgrades have been sent, total 2 logs", 2, logs.Length);
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "Upgrade Web Service Call: [Licence Code] DDDAAAGPC | [Latest Download]  | [Sent Version] 1.3.2010.44"));
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "GP Release 2005 Jul 03 patch 44 (v1.3.2010.44) will be sent to this client, GPC via HTP (WebService)"));

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Delivered.Code);
			AssertEquals("One log for GPR build delivery", 1, AvailableBuildGPR.Logs.Find(query).Length);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Delivered.Code);
			AssertEquals("No log for GPC build delivery", 0, availableBuildGPC.Logs.Find(query).Length);
		}

		public void TestGetPackageURL_ReleaseRingIsGpcAndGpcIsAvailable_LegacyRequest()
		{
			SetupForLegacyRequest();

			LicenceDatabase gpcLicenceDatabase = Organisation.LicCompany.LicDatabases.AddNew();
			gpcLicenceDatabase.LD_ServerCode = "GPC";
			gpcLicenceDatabase.LD_ReleaseRing = ReleaseRings.Codes.GPC;

			var availableBuildGPC = CreateNewReleaseBuild(AvailableBuildGPR.HL_MajorVersion, AvailableBuildGPR.HL_MinorVersion, AvailableBuildGPR.HL_Release, 80, false, ReleaseRings.Codes.GPC);

			Factory.Save();

			AssertEquals("Precondition: AvailableBuildGPC.VersionNumber > AvailableBuildGPR.VersionNumber", true, availableBuildGPC.VersionNumber > AvailableBuildGPR.VersionNumber);

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			string message = GetLegacyRequestMessage("DDDAAAGPC", UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616	1.3.2010.0"));
			string encryptedMessage = encoder.Encrypt(message);
			UpgradePackageUrlRequest request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAAGPC" };
			var logger = new TestLogger();
			UpgradePackageUrlResponse response = Generator.GetPackageUrl(request);

			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("return GPC as it is available", "1.3.2010.80", response.VersionNumber);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UpgradeSucceeded.Code);
			var logs = gpcLicenceDatabase.Logs.Find(query);
			AssertEquals("Two upgrades have been sent, total 2 logs", 2, logs.Length);
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "Upgrade Web Service Call: [Licence Code] DDDAAAGPC | [Latest Download]  | [Sent Version] 1.3.2010.80"));
			AssertEquals(true, logs.Cast<StmALog>().Any(l => l.SL_Reference == "GP Candidate 2005 Jul 03 patch 80 (v1.3.2010.80) will be sent to this client, GPC via HTP (WebService)"));

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Delivered.Code);
			AssertEquals("One log for GPC build delivery", 1, availableBuildGPC.Logs.Find(query).Length);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Delivered.Code);
			AssertEquals("No log for GPR build delivery", 0, AvailableBuildGPR.Logs.Find(query).Length);
		}

		public void TestGetPackageURL_InvalidRequest_LegacyRequest()
		{
			SetupForLegacyRequest();

			UpgradePackageUrlRequest request = new UpgradePackageUrlRequest() { EncryptedMessage = "Something Invalid", LicenceCode = "DDDBBBSYD" };
			var logger = new TestLogger();
			ErrorReporter.Clear();
			UpgradePackageUrlResponse response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("Bad request: invalid request message", response.ErrorMessage);
			AssertEquals(typeof(CryptographicException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Unable to interpret the Upgrade Request Message", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			string message = GetLegacyRequestMessage("DDDAAASY");
			request.EncryptedMessage = encoder.Encrypt(message);
			request.LicenceCode = "DDDAAASY";
			response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("Bad request: invalid licence code", response.ErrorMessage);

			message = GetLegacyRequestMessage("DDDBBBBAD");
			request.EncryptedMessage = encoder.Encrypt(message);
			request.LicenceCode = "DDDBBBBAD";
			response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("Unable to identify database from licence code DDDBBBBAD", response.ErrorMessage);

			message = GetLegacyRequestMessage("DDDAAANEW");
			request.EncryptedMessage = encoder.Encrypt(message);
			request.LicenceCode = "DDDAAANEW";
			response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("Unable to identify current release ring from licence code DDDAAANEW", response.ErrorMessage);

			message = GetLegacyRequestMessage("DDDAAABBB");
			request.EncryptedMessage = encoder.Encrypt(message);
			request.LicenceCode = "DDDAAABBB";
			response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("The database does not support HTTP upgrade", response.ErrorMessage);

			message = GetLegacyRequestMessage("DDD???BBB");
			request.EncryptedMessage = encoder.Encrypt(message);
			request.LicenceCode = "DDD???BBB";
			response = Generator.GetPackageUrl(request);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("", response.URL);
			AssertEquals("Company code is ignored for loading the database", "The database does not support HTTP upgrade", response.ErrorMessage);
		}

		public void TestExceptionEmailNotification_LegacyRequest()
		{
			SetupForLegacyRequest();

			Generator.IsPackagePublished = false;
			Generator.ShouldThrowUnhandledException = true;

			InitialiseMailManager();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			string message = GetLegacyRequestMessage("DDDAAATST", UpgradeInfo.FromString("73a90a5d-131b-4755-930b-ea0166b90616	1.3.2011.23"));
			string encryptedMessage = encoder.Encrypt(message);

			int upgradeToClientCountBefore = Factory.Load<UpgradesToClient>(new ZQuery()).Length;

			UpgradePackageUrlRequest request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAATST", CurrentVersionNumber = "73a90a5d-131b-4755-930b-ea0166b90616 1.3.2011.23" };
			UpgradePackageUrlResponse response = Generator.GetPackageUrl(request);

			AssertContains("Unhandled exception: System.Exception: Stop working...", response.ErrorMessage);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Error, response.ResponseClass);

			AssertEquals("Unhandled exception from upgrade web service call", ErrorReporter.LastKeyReported);
			AssertEquals(FormattableString.Invariant($"Exception from [Licence Code] DDDAAATST | [Current Version] 73a90a5d-131b-4755-930b-ea0166b90616 1.3.2011.23. Current Company: {GlbCompany.CurrentCompany.GC_Code}"), ErrorReporter.LastMessageReported);
			AssertEquals("Stop working...", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Instance.Clear();
		}

		string GetLegacyRequestMessage(string licenceCode, UpgradeInfo upgradeInfo = null)
		{
			return UpgradePackageServiceFactory.GetRawMessage(licenceCode, upgradeInfo ?? new UpgradeInfo(Guid.Empty, 0, 0, 0, 0));
		}

		void ClearErrorReporterXmlMessage()
		{
			if (ErrorReporter.LastExceptionReported?.GetType() == typeof(ArgumentException) && ErrorReporter.LastKeyReported == "Unable to interpret the Upgrade Request Message")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestGetPackageURL_EscalatedDPRReturnsDPR()
		{
			// Create Client license
			LicenceDatabase clientLicence = CreateClientLicence("GP1", ReleaseRings.Codes.GP1);

			// create initial build entries
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 2, 17, 0, false, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 3, 16, 0, false, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR_old = CreateNewReleaseBuild(16, 4, 6, 0, true, ReleaseRings.Codes.DPR);
			ReleaseBuild releaseDPR = CreateNewReleaseBuild(16, 4, 13, 0, false, ReleaseRings.Codes.DPR);
			Factory.Save();

			var clientBuild = releaseDPR_old;

			// cust requests upgrade, should get DPR
			var response = PerformUpdateRequest(clientLicence, clientBuild);
			AssertEquals(response.ErrorMessage, UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("Has DPR should get next DPR because STD and GP1 are older", "16.4.13.0", response.VersionNumber);
			ClearErrorReporterXmlMessage();
		}

		public void TestGetPackageURL_EscalatedDPRReturnsSTD()
		{
			// Create Client license
			LicenceDatabase clientLicence = CreateClientLicence("GP1", ReleaseRings.Codes.GP1);

			// create initial build entries
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 2, 17, 0, false, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD_old = CreateNewReleaseBuild(16, 3, 16, 0, true, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR_old = CreateNewReleaseBuild(16, 4, 13, 0, true, ReleaseRings.Codes.DPR);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 4, 13, 0, false, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR = CreateNewReleaseBuild(16, 4, 27, 0, false, ReleaseRings.Codes.DPR);
			Factory.Save();

			var clientBuild = releaseDPR_old;

			// cust requests upgrade from DPR, should get STD
			var response = PerformUpdateRequest(clientLicence, clientBuild);
			AssertEquals(response.ErrorMessage, UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("Has DPR should get equivalent STD", "16.4.13.0", response.VersionNumber);
			ClearErrorReporterXmlMessage();
		}

		public void TestGetPackageURL_EscalatedSTDReturnsGP1()
		{
			// Create Client license
			LicenceDatabase clientLicence = CreateClientLicence("GP1", ReleaseRings.Codes.GP1);

			// create build entries as at 22 June 2016
			ReleaseBuild releaseGP1_old = CreateNewReleaseBuild(16, 2, 17, 0, true, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD_old = CreateNewReleaseBuild(16, 5, 11, 0, true, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR_old = CreateNewReleaseBuild(16, 6, 8, 0, true, ReleaseRings.Codes.DPR);
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 5, 11, 0, false, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 6, 8, 0, false, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR = CreateNewReleaseBuild(16, 6, 22, 0, false, ReleaseRings.Codes.DPR);
			Factory.Save();

			ReleaseBuild clientBuild = releaseSTD_old;

			// cust requests upgrade from STD, should get GP1
			var response = PerformUpdateRequest(clientLicence, clientBuild);
			AssertEquals(response.ErrorMessage, UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("Has STD should get next GP1", "16.5.11.0", response.VersionNumber);
			ClearErrorReporterXmlMessage();
		}

		public void TestGetPackageURL_UpgradeGP1ReturnsGP1()
		{
			// Create Client license
			LicenceDatabase clientLicence = CreateClientLicence("GP1", ReleaseRings.Codes.GP1);

			// create build entries as at 22 June 2016
			ReleaseBuild releaseGP1_old = CreateNewReleaseBuild(16, 2, 17, 0, true, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD_old = CreateNewReleaseBuild(16, 5, 11, 0, true, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR_old = CreateNewReleaseBuild(16, 6, 8, 0, true, ReleaseRings.Codes.DPR);
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 5, 11, 0, false, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 6, 8, 0, false, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR = CreateNewReleaseBuild(16, 6, 22, 0, false, ReleaseRings.Codes.DPR);
			Factory.Save();

			ReleaseBuild clientBuild = releaseGP1_old;

			// cust requests upgrade from GP1, should get GP1
			var response = PerformUpdateRequest(clientLicence, clientBuild);
			AssertEquals(response.ErrorMessage, UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("Has GP1 should get next GP1", "16.5.11.0", response.VersionNumber);
			ClearErrorReporterXmlMessage();
		}

		public void TestGetPackageURL_UpgradeSTDReturnsSTD()
		{
			// Create Client license
			LicenceDatabase clientLicence = CreateClientLicence("STD", ReleaseRings.Codes.STD);

			// create build entries as at 22 June 2016
			ReleaseBuild releaseGP1_old = CreateNewReleaseBuild(16, 2, 17, 0, true, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD_old = CreateNewReleaseBuild(16, 5, 11, 0, true, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR_old = CreateNewReleaseBuild(16, 6, 8, 0, true, ReleaseRings.Codes.DPR);
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 5, 11, 0, false, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 6, 8, 0, false, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR = CreateNewReleaseBuild(16, 6, 22, 0, false, ReleaseRings.Codes.DPR);
			Factory.Save();

			ReleaseBuild clientBuild = releaseSTD_old;

			// cust requests upgrade from GP1, should get GP1
			var response = PerformUpdateRequest(clientLicence, clientBuild);
			AssertEquals(response.ErrorMessage, UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals("Has STD should get next STD", "16.6.8.0", response.VersionNumber);
			ClearErrorReporterXmlMessage();
		}

		public void TestErrorReported_UnableToDecodeMessage()
		{
			// submit a malformed XML document
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage("DDDAAATST", 1033, new UpgradeInfo(Guid.Parse("73a90a5d-131b-4755-930b-ea0166b90616"), 1, 3, 3021, 0));
			var malformedXml = message.Substring(20);
			var encryptedMessage = encoder.Encrypt(malformedXml);

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = "DDDAAATST", CurrentVersionNumber = "73a90a5d-131b-4755-930b-ea0166b90616 1.3.3021.0" };
			var logger = new TestLogger();
			ErrorReporter.Clear();
			var response = Generator.GetPackageUrl(request);

			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Failed, response.ResponseClass);
			AssertEquals("Bad request: invalid request message", response.ErrorMessage);

			AssertEquals(typeof(XmlException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Unable to interpret the Upgrade Request Message", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetPackageUrl_UnrecognisedInstalledVersion()
		{
			InitialiseMailManager();

			// Create Client license
			LicenceDatabase clientLicence = CreateClientLicence("GP1", ReleaseRings.Codes.GP1);
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 5, 11, 0, false, ReleaseRings.Codes.GP1);
			Factory.Save();

			var version = "73a90a5d-131b-4755-930b-ea0166b90616	16.6.18.0";
			var licenceCode = "DDDAAAGP1";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var message = GetRequestMessage(licenceCode, 1111, UpgradeInfo.FromString(version));
			var encryptedMessage = encoder.Encrypt(message);

			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = licenceCode, CurrentVersionNumber = version };
			var logger = new TestLogger();
			var response = Generator.GetPackageUrl(request);

			AssertEquals("", response.ErrorMessage);
			AssertEquals(UpgradePackageUrlResponse.ResponseClassType.Success, response.ResponseClass);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		void InitialiseMailManager()
		{
			EDIDataRegistry.Instance.HttpGenericBaseUrl = "http://demo.test.com/ediEnterprise/Generic/";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "http://demo.test.com/ediEnterprise/ClientSpecific/";
		}

		LicenceDatabase CreateClientLicence(string serverCode, string releaseRing)
		{
			var organisation = Factory.NewWithValidTestData<EDIOrgHeader>();
			organisation.OH_Code = "DDDAAAMEL";

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = organisation.PK;

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_LE = enterprise.PK;
			licenceDatabase.LD_ServerCode = serverCode;
			licenceDatabase.LD_ReleaseRing = releaseRing;
			licenceDatabase.LD_DatabaseNumber = 1111;

			return licenceDatabase;
		}

		ReleaseBuild CreateNewReleaseBuild(int majorVersion, int minorVersion, int release, int patch, bool superceded, string releaseStatus)
		{
			var releaseBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			releaseBuild.HL_Product = ProductTypes.Codes.Enterprise;
			releaseBuild.HL_MajorVersion = majorVersion;
			releaseBuild.HL_MinorVersion = minorVersion;
			releaseBuild.HL_Release = release;
			releaseBuild.HL_Patch = patch;
			releaseBuild.HL_Superceded = superceded;
			releaseBuild.HL_ReleaseStatus = releaseStatus;
			ReleaseBuild.SetClientSpecificCodesForTesting(releaseBuild.PK);

			return releaseBuild;
		}

		UpgradePackageUrlResponse PerformUpdateRequest(LicenceDatabase license, ReleaseBuild currentVersion)
		{
			string licenceCode = "DDDAAA" + license.LD_ServerCode;
			int databaseNumber = license.LD_DatabaseNumber;
			var requestVersion = currentVersion != null ? new VersionNumber(currentVersion.HL_MajorVersion, currentVersion.HL_MinorVersion, currentVersion.HL_Release, currentVersion.HL_Patch) : new VersionNumber();

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var versionGuid = Guid.Parse("73a90a5d-131b-4755-930b-ea0166b90616");
			var upgradeInfo = new UpgradeInfo(versionGuid, requestVersion.Major, requestVersion.Minor, requestVersion.Release, requestVersion.Patch);
			var message = GetRequestMessage(licenceCode, databaseNumber, upgradeInfo);
			var encryptedMessage = encoder.Encrypt(message);
			var request = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = licenceCode, CurrentVersionNumber = requestVersion.ToString() };

			return Generator.GetPackageUrl(request);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Generator = new GeneratorForTest(new NLogWrapperForTest(GetType()));
			SetupEdi();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TempBuildFile?.Dispose();
		}

		void SetupForRequest(bool enableWeeklyBuildCutOff = false, bool isCargoWiseHosted = true, bool isInternal = false)
		{
			Organisation = Factory.NewWithValidTestData<EDIOrgHeader>();
			Organisation.OH_Code = "DDDAAAMEL";
			Enterprise = Factory.New<LicenceEnterprise>();
			Enterprise.LE_EnterpriseCode = "DDD";
			Enterprise.LE_OH = Organisation.PK;
			Enterprise.LE_IsInternal = isInternal;

			Database1 = Factory.New<LicenceDatabase>();
			Database1.LD_LE = Enterprise.PK;
			Database1.LD_ServerCode = "SYD";
			Database1.LD_ReleaseRing = ReleaseRings.Codes.GPR;
			Database1.LD_DatabaseNumber = 1024;
			Database1.LD_HostedLocation = isCargoWiseHosted ? "SYD" : "NCW";

			Database2 = Factory.New<LicenceDatabase>();
			Database2.LD_LE = Enterprise.PK;
			Database2.LD_ServerCode = "TST";
			Database2.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			Database2.LD_DatabaseNumber = 1033;
			Database2.LD_HostedLocation = isCargoWiseHosted ? "SYD" : "NCW";

			Database3 = Factory.New<LicenceDatabase>();
			Database3.LD_LE = Enterprise.PK;
			Database3.LD_ServerCode = "NEW";
			Database3.LD_ReleaseRing = "";
			Database3.LD_DatabaseNumber = 1080;
			Database3.LD_HostedLocation = isCargoWiseHosted ? "SYD" : "NCW";

			Database4 = Factory.New<LicenceDatabase>();
			Database4.LD_LE = Enterprise.PK;
			Database4.LD_ServerCode = "BBB";
			Database4.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			Database4.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			Database4.LD_DatabaseNumber = 1100;
			Database4.LD_HostedLocation = isCargoWiseHosted ? "SYD" : "NCW";

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_RL_NKClosestPort = "AUSYD";
			org2.OH_FullName = "My Organisation 2";
			org2.OH_Code = "BBBSYD";

			org2.CreateAndLoadLicenceForOrg();
			org2.LicenceEnterpriseCode = "FFF";
			org2.LicCompany.LC_CompanyCode = "BBB";

			Database5 = org2.LicCompany.LicDatabases.AddNew();
			Database5.LD_ServerCode = "PRD";
			Database5.LD_DatabaseNumber = 9527;
			Database5.LD_ReleaseRing = ReleaseRings.Codes.ALP;

			SetupReleaseBuilds();

			Factory.Save();

			if (enableWeeklyBuildCutOff)
			{
				EDIDataRegistry.Instance.EnableWeeklyBuildCutOff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				EDIDataRegistry.Instance.WeeklyBuildCutOffDay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, LastBuildCutOffDate.DayOfWeek.ToString());
				EDIDataRegistry.Instance.WeeklyBuildCutOffTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, LastBuildCutOffDate.ToShortTimeString());
			}
		}

		void SetupEdi()
		{
			var systemLicenceCode = EnvProxy.Instance.CurrentCompany.GetLicenceCode();
			var systemLicenceCodeWithDummyCompany = systemLicenceCode.Substring(0, 3) + "ZZZ" + systemLicenceCode.Substring(6, 3);
			AssertNotEquals(systemLicenceCode, systemLicenceCodeWithDummyCompany);
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLicenceCodeWithDummyCompany);
		}

		void SetupForLegacyRequest()
		{
			Organisation = Factory.NewWithValidTestData<EDIOrgHeader>();
			Organisation.OH_RL_NKClosestPort = "AUSYD";
			Organisation.OH_FullName = "My Organisation";
			Organisation.OH_Code = "AAASYD";
			Organisation.Addresses.AddNew().OA_Address1 = "Test Address";

			Organisation.CreateAndLoadLicenceForOrg();
			Organisation.LicenceEnterpriseCode = "DDD";
			Organisation.LicCompany.LC_CompanyCode = "AAA";

			Database1 = Organisation.LicCompany.LicDatabases.AddNew();
			Database1.LD_ServerCode = "SYD";
			Database1.LD_ReleaseRing = ReleaseRings.Codes.GPR;

			Database2 = Organisation.LicCompany.LicDatabases.AddNew();
			Database2.LD_ServerCode = "TST";
			Database2.LD_ReleaseRing = ReleaseRings.Codes.ALP;

			Database3 = Organisation.LicCompany.LicDatabases.AddNew();
			Database3.LD_ServerCode = "NEW";
			Database3.LD_ReleaseRing = "";

			Database4 = Organisation.LicCompany.LicDatabases.AddNew();
			Database4.LD_ServerCode = "BBB";
			Database4.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			Database4.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;

			SetupReleaseBuilds();

			Factory.Save();
		}

		void SetupReleaseBuilds()
		{
			CurrentBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			CurrentBuild.HL_Product = ProductTypes.Codes.Enterprise;
			CurrentBuild.HL_MajorVersion = 1;
			CurrentBuild.HL_MinorVersion = 3;
			CurrentBuild.HL_Release = 1973;
			CurrentBuild.HL_Patch = 0;
			ReleaseBuild.SetClientSpecificCodesForTesting(CurrentBuild.PK);

			AvailableBuildALP = Factory.NewWithValidTestData<ReleaseBuild>();
			AvailableBuildALP.HL_Product = ProductTypes.Codes.Enterprise;
			AvailableBuildALP.HL_MajorVersion = 1;
			AvailableBuildALP.HL_MinorVersion = 3;
			AvailableBuildALP.HL_Release = 2011;
			AvailableBuildALP.HL_Patch = 23;
			AvailableBuildALP.HL_Superceded = false;
			AvailableBuildALP.HL_ReleaseStatus = ReleaseRings.Codes.ALP;
			ReleaseBuild.SetClientSpecificCodesForTesting(AvailableBuildALP.PK, "DDD");

			AvailableBuildGPR = Factory.NewWithValidTestData<ReleaseBuild>();
			AvailableBuildGPR.HL_Product = ProductTypes.Codes.Enterprise;
			AvailableBuildGPR.HL_MajorVersion = 1;
			AvailableBuildGPR.HL_MinorVersion = 3;
			AvailableBuildGPR.HL_Release = 2010;
			AvailableBuildGPR.HL_Patch = 44;
			AvailableBuildGPR.HL_Superceded = false;
			AvailableBuildGPR.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			ReleaseBuild.SetClientSpecificCodesForTesting(AvailableBuildGPR.PK);

			//second available GPR - weekly cut off build
			AvailableBuildGPRWeeklyCutOff = Factory.NewWithValidTestData<ReleaseBuild>();
			AvailableBuildGPRWeeklyCutOff.HL_Product = ProductTypes.Codes.Enterprise;
			AvailableBuildGPRWeeklyCutOff.HL_MajorVersion = 1;
			AvailableBuildGPRWeeklyCutOff.HL_MinorVersion = 3;
			AvailableBuildGPRWeeklyCutOff.HL_Release = 2010;
			AvailableBuildGPRWeeklyCutOff.HL_Patch = 41;
			AvailableBuildGPRWeeklyCutOff.HL_Superceded = false;
			AvailableBuildGPRWeeklyCutOff.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			ReleaseBuild.SetClientSpecificCodesForTesting(AvailableBuildGPRWeeklyCutOff.PK);

			//setup exe dates for purpose of testing weekly cut-off builds
			LastBuildCutOffDate = DateTime.Now.AddDays(-1).Date;
			AvailableBuildGPR.HL_ExeVersionDate = LastBuildCutOffDate.AddMinutes(1);
			AvailableBuildGPRWeeklyCutOff.HL_ExeVersionDate = LastBuildCutOffDate.AddMinutes(-1);

			Database1.LD_HL_CurrentRunningVersion = CurrentBuild.PK;
			Database2.LD_HL_CurrentRunningVersion = CurrentBuild.PK;
			Database4.LD_HL_CurrentRunningVersion = CurrentBuild.PK;

			TempBuildFile = TempFile.New();
			var versionExe = Path.Combine(AssemblyLoader.GetBinPath(), ExeFileNames.CargoWiseOneExeForVersionInfo);
			var exeVersionInfo = FileVersionInfo.GetVersionInfo(versionExe);
			PackageFileName = GetPackageFileName(exeVersionInfo);
			using (var zip = new ZipArchive(File.Create(TempBuildFile.Filename), ZipArchiveMode.Create, false))
			{
				zip.CreateEntryFromFile(versionExe, "Distribution/Application/CargoWiseOne.exe");
			}

			CurrentBuild.HL_PackagePath = TempBuildFile.Filename;
			AvailableBuildALP.HL_PackagePath = TempBuildFile.Filename;
			AvailableBuildGPR.HL_PackagePath = TempBuildFile.Filename;
			AvailableBuildGPRWeeklyCutOff.HL_PackagePath = TempBuildFile.Filename;
		}

		string GetPackageFileName(FileVersionInfo fileVersionInfo)
		{
			var regex = new Regex("\\.");
			string[] versionInfo = regex.Split(fileVersionInfo.FileVersion);

			return string.Format(@"Package20{0}{1}{2}_000000_{3}_{4}_{5}_{6}.edp",
				versionInfo[0].PadLeft(2).Replace(" ", "0"),
				versionInfo[1].PadLeft(2).Replace(" ", "0"),
				versionInfo[2].PadLeft(2).Replace(" ", "0"),
				versionInfo[0], versionInfo[1], versionInfo[2], versionInfo[3]);
		}

		TempFile TempBuildFile;
		string PackageFileName;

		LicenceEnterprise Enterprise;
		LicenceDatabase Database1;
		LicenceDatabase Database2;
		LicenceDatabase Database3;
		LicenceDatabase Database4;
		LicenceDatabase Database5;

		EDIOrgHeader Organisation;
		ReleaseBuild CurrentBuild;
		ReleaseBuild AvailableBuildALP;
		ReleaseBuild AvailableBuildGPR;
		ReleaseBuild AvailableBuildGPRWeeklyCutOff;
		DateTime LastBuildCutOffDate;

		GeneratorForTest Generator;

		class GeneratorForTest : UpgradePackageUrlGenerator
		{
			public GeneratorForTest(NLogWrapper logger) : base(null, logger) { }

			public bool ShouldThrowSendPackageException { get; set; }
			public bool ShouldThrowUnhandledException { get; set; }
			public bool ShouldAddSendPackageErrorMessage { get; set; }
			public bool IsPackagePublished { get; set; } = true;
			public int ReportSendPackageErrorCallCount { get; private set; }
			public string LastMessageReported = string.Empty;

			protected override IPackagePublishService GetPackagePublishServce()
			{
				return new PackagePublishServiceForTest() { IsPackagePublished = IsPackagePublished, ShouldThrowException = ShouldThrowSendPackageException, ShouldAddErrorMessage = ShouldAddSendPackageErrorMessage };
			}

			protected override RuntimePackageBuilder GetRuntimePackageBuilder(ReleaseBuild build, string targetPath)
			{
				return new RuntimePackageBuilderForTest(build, targetPath) { ShouldBuildBadZipPackage = this.ShouldBuildBadZipPackage };
			}

			protected override void LogUpgradeInfo(UpgradePackageUrlRequest request, LicenceDatabase database, ReleaseBuild latestReleaseBuild)
			{
				if (ShouldThrowUnhandledException)
				{
					throw new Exception("Stop working...");
				}
				else
				{
					base.LogUpgradeInfo(request, database, latestReleaseBuild);
				}
			}

			protected override void ReportSendPackageErrorCore(string key, Exception e, string message)
			{
				LastMessageReported = message;
				ReportSendPackageErrorCallCount += 1;
			}

			public bool ShouldBuildBadZipPackage { get; set; }
		}

		class RuntimePackageBuilderForTest : RuntimePackageBuilder
		{
			public bool ShouldBuildBadZipPackage { get; set; }

			public RuntimePackageBuilderForTest(ReleaseBuild build, string targetPath)
				: base(build, targetPath)
			{
			}

			public RuntimePackageBuilderForTest(string goodBuildPath, string targetPath)
				: base(goodBuildPath, targetPath)
			{
			}

			public RuntimePackageBuilderForTest(string templatePath, string goodBuildPath, string targetPath)
				: base(templatePath, goodBuildPath, targetPath)
			{
			}

			protected override void ValidateDeployableBuild(FileVersionInfo info)
			{
			}

			protected override void BuildFromMasterPackageBlob(string enterpriseCode, bool isHostedOnWiseCloud)
			{
				base.BuildFromMasterPackageBlob(enterpriseCode, isHostedOnWiseCloud);

				if (ShouldBuildBadZipPackage)
				{
					var bytes = File.ReadAllBytes(LastPackagePath);
					bytes[bytes.Length / 2]++;
					File.WriteAllBytes(LastPackagePath, bytes);
				}
			}
		}

		class PackagePublishServiceForTest : IPackagePublishService
		{
			public bool IsPackagePublished { get; set; }
			public bool ShouldThrowException { get; set; }
			public bool ShouldAddErrorMessage { get; set; }

			public string LastErrorMessage => lastErrorMessage ?? string.Empty;
			string lastErrorMessage;

			public TriState IsPackageAlreadyPublished(string packageName, string clientSpecificCode)
			{
				return IsPackagePublished ? TriState.True : TriState.False;
			}

			public bool PublishPackage(string packagePath, string targetDirectory)
			{
				if (ShouldThrowException)
				{
					throw new IOException("The network path was not found.");
				}
				if (ShouldAddErrorMessage)
				{
					lastErrorMessage = "Failed to delete old file. The network path was not found.";
				}
				return true;
			}
		}

		class TestLogger : ILogger
		{
			public void Log(LogType type, string message)
			{
				LogBuilder.Append(message);
			}

			readonly StringBuilder LogBuilder = new StringBuilder();

			public void Log(LogType type, string message, Exception ex)
			{
				throw new NotImplementedException();
			}

			public override string ToString()
			{
				return LogBuilder.ToString();
			}
		}

		#endregion
	}
}
