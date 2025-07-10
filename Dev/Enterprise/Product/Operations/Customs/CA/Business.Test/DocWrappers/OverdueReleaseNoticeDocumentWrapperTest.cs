using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class OverdueReleaseNoticeDocumentWrapperTest : K84ReportDocumentWrapperTestCase
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var message = CreateMessageFromInterchangeString(Factory, interchangeString);
			var wrapper = new OverdueReleaseNoticeDocumentWrapper(message);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("OverdueReleaseNoticeDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", message.PK, supporter?.SourceIdentifier);
		}

		#endregion

		#region Interchange String

		const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+110407:0608+594++++++1'
UNG+CUSRES+OVERDUE REPORT+U10207V1+110407:0608+594+UN+S:99B'
UNH+1+CUSRES:S:99B:UN'
BGM+++9'
DTM+137:20110407:102'
RFF+ABP:10207'
RFF+AEA:0495'
RFF+ARA:842957342RM0001'
RFF+TN:000001056:N'
RFF+AFB:94639121438DD'
RFF+AEJ:006:Y'
DTM+204:20110329:102'
RFF+AEA:0497'
RFF+ARA:123241838RM0001'
RFF+TN:400004228:N'
RFF+AFB:3713PARS6542555'
RFF+AEJ:103:'
DTM+204:20101105:102'
RFF+AEA:0497'
RFF+ARA:123241838RM0001'
RFF+TN:400004068:N'
RFF+AFB:37132536987'
RFF+AEJ:090:Y'
DTM+204:20101125:102'
UNS+D'
UNS+S'
UNT+25+1'
UNE+1+594'
UNZ+1+594'";

		#endregion

		public override void TestProperties()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER1";
			K84DailyReportDocumentWrapperTest.CreatedDeclaration(Factory, importer, "B00001113", "10207000001056");
			K84DailyReportDocumentWrapperTest.CreatedDeclaration(Factory, importer, "B00001114", "10207400004228");
			importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER2";
			K84DailyReportDocumentWrapperTest.CreatedDeclaration(Factory, importer, "B0000111X", "10207400004068", ZDateTime.UtcNow.AddMinutes(-1));
			K84DailyReportDocumentWrapperTest.CreatedDeclaration(Factory, importer, "B00001115", "10207400004068", ZDateTime.UtcNow);
			Factory.Save();

			AssertOverdueNotice(interchangeString);
			AssertOverdueNotice(interchangeString.Replace("CUSRES", "CUSDEC"));
		}

		void AssertOverdueNotice(string interchangeString)
		{
			var message = CreateMessageFromInterchangeString(Factory, interchangeString);
			var wrapper = new OverdueReleaseNoticeDocumentWrapper(message);
			AssertEquals("CurrentDate", new ZDateTime(2011, 04, 07), wrapper.CurrentDate);
			AssertEquals("AccountSecurityNumber", "10207", wrapper.AccountSecurityNumber);
			var notices = wrapper.OverdueNotices;
			AssertEquals("OverdueNotices.Count", 3, notices.Count);
			AssertOverdueNotice(notices[0], "0495", "842957342RM0001", "000001056", "94639121438DD", 6, true, new ZDateTime(2011, 03, 29), "B00001113", "IMPORTER1");
			AssertOverdueNotice(notices[1], "0497", "123241838RM0001", "400004228", "3713PARS6542555", 103, false, new ZDateTime(2010, 11, 05), "B00001114", "IMPORTER1");
			AssertOverdueNotice(notices[2], "0497", "123241838RM0001", "400004068", "37132536987", 90, true, new ZDateTime(2010, 11, 25), "B00001115", "IMPORTER2");
		}

		static void AssertOverdueNotice(OverdueReleaseNoticeDocumentWrapper.OverdueNotice notice, string releaseOffice,
			string businessNum, string tranNum, string ccn, int ageInDays, bool isAQ, ZDateTime releaseDate, ZString jobNumber, ZString importerCode)
		{
			AssertEquals("ReleaseOfficeNumber", releaseOffice, notice.ReleaseOffice);
			AssertEquals("ClientBusinessNumber", businessNum, notice.ClientBusinessNumber);
			AssertEquals("TransactionNumber", tranNum, notice.TransactionNumber);
			AssertEquals("LvsIndicator", false, notice.IsLVS);
			AssertEquals("CargoControlNumber", ccn, notice.CargoControlNumber);
			AssertEquals("AgeInDays", ageInDays, notice.AgeInDays);
			AssertEquals("AQIndicator", isAQ, notice.IsAQ);
			AssertEquals("ReleaseDate", releaseDate, notice.ReleaseDate);
			AssertEquals("JobNumber", jobNumber, notice.Declaration?.JE_DeclarationReference ?? ZString.Empty);
			AssertEquals("ImporterCode", importerCode, notice.ImporterCode);
		}

		public void TestPropertiesForCUSDECVersion()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+160608:1234+594++++++1'
UNG+CUSDEC+OVERDUE REPORT+U12310V1+160608:1234+594+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
BGM+++9'
DTM+137:20160608:102'
RFF+ABP:12310'
RFF+AEA:0813'
RFF+ARA:836263228RM0001'
RFF+TN:001209310:N'
RFF+AFB:2130PARS55218127'
RFF+AEJ:007:Y'
DTM+204:20160527:102'
UNS+D'
UNS+S'
UNT+13+1'
UNE+1+594'
UNZ+1+594'";

			#endregion

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER1";
			K84DailyReportDocumentWrapperTest.CreatedDeclaration(Factory, importer, "B00001113", "12310001209310");
			Factory.Save();

			var message = CreateMessageFromInterchangeString(Factory, interchangeString);
			var wrapper = new OverdueReleaseNoticeDocumentWrapper(message);
			AssertEquals("CurrentDate", new ZDateTime(2016, 6, 8), wrapper.CurrentDate);
			AssertEquals("AccountSecurityNumber", "12310", wrapper.AccountSecurityNumber);
			var notices = wrapper.OverdueNotices;
			AssertEquals("OverdueNotices.Count", 1, notices.Count);
			AssertOverdueNotice(notices[0], "0813", "836263228RM0001", "001209310", "2130PARS55218127", 7, true, new ZDateTime(2016, 5, 27), "B00001113", "IMPORTER1");
		}

		public void TestProperties_WrongMessage()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+110407:0608+594++++++1'
UNG+CUSRES+OVERDUE REPORT+U10207V1+110407:0608+594+UN+S:99B'
UNH+1+CUSRES:S:99B:UN'
BGM+++9'
DTM+137:20110407:102'
RFF+ABP:10207'
RFF+ARA:842957342RM0001'
RFF+TN:000001056:N'
RFF+AEJ:006:Y'
DTM+204:20110329:102'
RFF+AEA:0497'
RFF+TN:400004228:N'
RFF+AFB:3713PARS6542555'
RFF+AEJ:103:'
DTM+204:20101105:102'
RFF+AEA:0497'
RFF+ARA:123241838RM0001'
RFF+AFB:37132536987'
UNS+D'
UNS+S'
UNT+19+1'
UNE+1+594'
UNZ+1+594'";

			#endregion

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER1";
			K84DailyReportDocumentWrapperTest.CreatedDeclaration(Factory, importer, "B00001113", "10207000001056");
			K84DailyReportDocumentWrapperTest.CreatedDeclaration(Factory, importer, "B00001114", "10207400004228");
			Factory.Save();

			AssertOverdueNotice_WrongMessage(interchangeString);
			AssertOverdueNotice_WrongMessage(interchangeString.Replace("CUSRES", "CUSDEC"));
		}

		void AssertOverdueNotice_WrongMessage(string interchangeString)
		{
			var message = CreateMessageFromInterchangeString(Factory, interchangeString);
			var wrapper = new OverdueReleaseNoticeDocumentWrapper(message);
			AssertEquals("CurrentDate", new ZDateTime(2011, 04, 07), wrapper.CurrentDate);
			AssertEquals("AccountSecurityNumber", "10207", wrapper.AccountSecurityNumber);
			var notices = wrapper.OverdueNotices;
			AssertEquals("OverdueNotices.Count", 3, notices.Count);
			AssertOverdueNotice(notices[0], "", "842957342RM0001", "000001056", "", 6, true, new ZDateTime(2011, 03, 29), "B00001113", "IMPORTER1");
			AssertOverdueNotice(notices[1], "0497", "", "400004228", "3713PARS6542555", 103, false, new ZDateTime(2010, 11, 05), "B00001114", "IMPORTER1");
			AssertOverdueNotice(notices[2], "0497", "123241838RM0001", "", "37132536987", 0, false, ZDate.Empty, "", "");
		}
	}
}
