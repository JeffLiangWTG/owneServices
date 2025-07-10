using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class K84ReportMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestNotificationGroupForDaily()
		{
			#region Interchange String

			const string interchangeString = @"
UNB+UNOA:3+INETCECPT+YUSAIRXPN+101231:0605+258++++++1'
UNG+CUSDEC+NOTICE+U10207V1+101231:0605+258+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
BGM+++9'
DTM+137:20101230:102'
RFF+ABP:10207'
UNS+D'
DMS+K10'
DTM+130:20101231:102'
DTM+353:20101230:102'
NAD+VC+0497'
LIN+++000000373'
MOA+1:771178'
MOA+161:771178'
MOA+128:771178'
LIN+++000000384'
MOA+155:488'
MOA+1:13024'
MOA+161:13512'
MOA+128:13512'
LIN+++000000395'
MOA+1:86610'
MOA+161:86610'
MOA+128:86610'
DMS+K40'
DTM+130:20110131:102'
MOA+155:488'
MOA+1:870812'
MOA+161:871300'
MOA+128:871300'
UNS+S'
UNT+30+1'
UNE+1+258'
UNZ+1+258'";

			#endregion

			var newGroup1 = Factory.New<GlbGroup>();
			newGroup1.GG_Code = "NG1";
			var newStaff1 = newGroup1.Staff.AddNew();
			newStaff1.GS_Code = "NS1";
			newStaff1.GS_LoginName = "NS1";
			newStaff1.GS_EmailAddress = "ns1@cargowise.com";
			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup1.PK.ToGuid());
			Factory.Save();

			var ediMessage = ProcessMessage(CreateMessage(interchangeString));
			var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Daily K84 Accounting Notice for 30-Dec-10");
			AssertNotNull(email);
			AssertEquals("One recipient", 1, email.CCRecipients.Count);
			AssertEquals(User.PostMasterUserName, "ns1@cargowise.com", email.CCRecipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var newGroup2 = Factory.New<GlbGroup>();
			newGroup2.GG_Code = "NG2";
			var newStaff2 = newGroup2.Staff.AddNew();
			newStaff2.GS_Code = "NS2";
			newStaff2.GS_LoginName = "NS2";
			newStaff2.GS_EmailAddress = "ns2@cargowise.com";
			CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup2.PK.ToGuid());
			Factory.Save();

			ediMessage = ProcessMessage(CreateMessage(interchangeString));
			email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Daily K84 Accounting Notice for 30-Dec-10");
			AssertNotNull(email);
			AssertEquals("One recipient", 1, email.CCRecipients.Count);
			AssertEquals(User.PostMasterUserName, "ns2@cargowise.com", email.CCRecipients[0].Email);
		}

		public void TestNotificationGroupForMonthly()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+110330:0607+567++++++1'
UNG+CUSDEC+K84++110330:0607+567+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
BGM+++9'
LOC+127+:::0497'
DTM+130:20110330:102'
RFF+ABP:10207'
UNS+D'
DMS+K50'
DTM+137:20110303:102'
MOA+155:113703'
MOA+1:202532'
MOA+161:316235'
DMS+K50'
DTM+137:20110304:102'
MOA+155:7366'
MOA+1:25496'
MOA+161:32862'
DMS+K50'
DTM+137:20110307:102'
MOA+155:45668'
MOA+105:2000'
MOA+4:3000'
MOA+1:96257'
MOA+161:146925'
DMS+K50'
DTM+137:20110308:102'
MOA+155:11'
MOA+1:1861623'
MOA+161:1861634'
DMS+K51'
MOA+155:166748'
MOA+105:2000'
MOA+4:3000'
MOA+1:2185908'
MOA+161:2357656'
DMS+K52++000000395'
DTM+137:20110330:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
DMS+K52++000000396'
DTM+137:20110331:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
DMS+K53'
MOA+155:2000'
MOA+105:4000'
MOA+4:6000'
MOA+1:8000'
MOA+161:20000'
DMS+K54'
DTM+137:20110330:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K54'
DTM+137:20110331:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K56++000000395'
DTM+137:20110330:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K56++000000396'
DTM+137:20110331:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K60++000000395'
DTM+137:20110330:102'
MOA+201:5000'
DMS+K60++000000396'
DTM+137:20110331:102'
MOA+201:5000'
DMS+K61++000000395'
DTM+137:20110330:102'
MOA+201:10000'
DMS+K61++000000396'
DTM+137:20110331:102'
MOA+201:10000'
DMS+K62'
DTM+137:20110330:102'
MOA+201:15000'
DMS+K62'
DTM+137:20110331:102'
MOA+201:15000'
DMS+K65'
MOA+201:10000'
MOA+202:20000'
MOA+208:30000'
MOA+128:60000'
DMS+K66'
PAT+1+2011033099999999'
PCD+16:.375'
PAT+7+2011033199999999'
PCD+15:001.002'
DMS+K70'
DTM+140:20110331:102'
MOA+155:166748'
MOA+105:2000'
MOA+4:3000'
MOA+1:2185908'
MOA+161:2357656'
MOA+128:2357656'
UNS+S'
UNT+38+1'
UNE+1+567'
UNZ+1+567'";

			#endregion

			var newGroup1 = Factory.New<GlbGroup>();
			newGroup1.GG_Code = "NG1";
			var newStaff1 = newGroup1.Staff.AddNew();
			newStaff1.GS_Code = "NS1";
			newStaff1.GS_LoginName = "NS1";
			newStaff1.GS_EmailAddress = "ns1@cargowise.com";
			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup1.PK.ToGuid());
			Factory.Save();

			var ediMessage = ProcessMessage(CreateMessage(interchangeString));
			var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Monthly K84 Accounting Notice for 30-Mar-11");
			AssertNotNull(email);
			AssertEquals("One recipient", 1, email.CCRecipients.Count);
			AssertEquals(User.PostMasterUserName, "ns1@cargowise.com", email.CCRecipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var newGroup2 = Factory.New<GlbGroup>();
			newGroup2.GG_Code = "NG2";
			var newStaff2 = newGroup2.Staff.AddNew();
			newStaff2.GS_Code = "NS2";
			newStaff2.GS_LoginName = "NS2";
			newStaff2.GS_EmailAddress = "ns2@cargowise.com";
			CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup2.PK.ToGuid());
			Factory.Save();

			ediMessage = ProcessMessage(CreateMessage(interchangeString));
			email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Monthly K84 Accounting Notice for 30-Mar-11");
			AssertNotNull(email);
			AssertEquals("One recipient", 1, email.CCRecipients.Count);
			AssertEquals(User.PostMasterUserName, "ns2@cargowise.com", email.CCRecipients[0].Email);
		}

		public void TestNotificationGroupForOverdue()
		{
			#region Interchange String

			const string interchangeString = @"
UNB+UNOA:3+INETCECPT+YUSAIRXPN+110407:0608+594++++++1'
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

			var newGroup1 = Factory.New<GlbGroup>();
			newGroup1.GG_Code = "NG1";
			var newStaff1 = newGroup1.Staff.AddNew();
			newStaff1.GS_Code = "NS1";
			newStaff1.GS_LoginName = "NS1";
			newStaff1.GS_EmailAddress = "ns1@cargowise.com";
			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup1.PK.ToGuid());
			Factory.Save();

			var ediMessage = ProcessMessage(CreateMessage(interchangeString));
			var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Overdue Release Notice for 07-Apr-11");
			AssertNotNull(email);
			AssertEquals("One recipient", 1, email.CCRecipients.Count);
			AssertEquals(User.PostMasterUserName, "ns1@cargowise.com", email.CCRecipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var newGroup2 = Factory.New<GlbGroup>();
			newGroup2.GG_Code = "NG2";
			var newStaff2 = newGroup2.Staff.AddNew();
			newStaff2.GS_Code = "NS2";
			newStaff2.GS_LoginName = "NS2";
			newStaff2.GS_EmailAddress = "ns2@cargowise.com";
			CACustomsDataRegistry.Instance.SendOverdueReportNotificationsToGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGroup2.PK.ToGuid());
			Factory.Save();

			ediMessage = ProcessMessage(CreateMessage(interchangeString));
			email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Overdue Release Notice for 07-Apr-11");
			AssertNotNull(email);
			AssertEquals("One recipient", 1, email.CCRecipients.Count);
			AssertEquals(User.PostMasterUserName, "ns2@cargowise.com", email.CCRecipients[0].Email);
		}

		public void TestDailyK84MessageAfterARLCutOver()
		{
			#region Interchange String

			const string interchangeString = @"
UNB+UNOA:3+INETCECPT+YUSAIRXPN+101231:0605+258++++++1'
UNG+CUSDEC+NOTICE+U10207V1+101231:0605+258+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
BGM+++9'
DTM+137:201625:102'
RFF+ABP:10207'
UNS+D'
DMS+K10'
DTM+130:20160126:102'
DTM+353:20160125:102'
NAD+VC+0497'
LIN+++000000373'
MOA+1:771178'
MOA+161:771178'
MOA+128:771178'
LIN+++000000384'
MOA+155:488'
MOA+1:13024'
MOA+161:13512'
MOA+128:13512'
LIN+++000000395'
MOA+1:86610'
MOA+161:86610'
MOA+128:86610'
DMS+K40'
DTM+130:20110131:102'
MOA+155:488'
MOA+1:870812'
MOA+161:871300'
MOA+128:871300'
UNS+S'
UNT+30+1'
UNE+1+258'
UNZ+1+258'";

			#endregion

			var ediMessage = ProcessMessage(CreateMessage(interchangeString));
			AssertEquals("EM_MessageSubType", K84ReportTypes.Codes.Daily, ediMessage.EM_MessageSubType);
			AssertEquals("CA_K84StatementDate", ZDateTime.Empty, declaration.CA_K84StatementDate);
			AssertEquals("CA_K84AccountingDate", ZDateTime.Empty, declaration.CA_K84AccountingDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDailyK84MessageInterpretationAfterARLCutOver()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+101231:0605+258++++++1'
UNG+CUSDEC+NOTICE+U10207V1+101231:0605+258+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
BGM+++9'
DTM+137:20160125:102'
RFF+ABP:10207'
UNS+D'
DMS+K10'
DTM+130:20160126:102'
DTM+353:20160125:102'
NAD+VC+0497'
LIN+++000001170'
MOA+155:51738'
MOA+105:100'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501223'
MOA+201:200'
MOA+128:1501423'
LIN+++000001147'
MOA+1:13000'
MOA+161:13000'
MOA+128:13000'
LIN+++35'
MOA+155:51738'
MOA+105:200'
MOA+4:1000000'
MOA+1:462385'
MOA+161:1514323'
MOA+201:300'
MOA+128:1514623'
LIN+++000001181'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++35'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++000001192'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++000001238'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++000001227'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
DMS+K36'
MOA+155:155214'
MOA+4:3000000'
MOA+1:1348155'
MOA+161:4503369'
MOA+128:4503369'
DMS+K40'
DTM+130:20110429:102'
MOA+155:258690'
MOA+105:100'
MOA+4:5000000'
MOA+1:2259925'
MOA+161:7518715'
MOA+201:300'
MOA+128:7519015'
UNS+S'
UNT+70+1'
UNE+1+258'
UNZ+1+258'";

			#endregion

			var importer = CreateImporter("1", true, false);
			CreatedDeclaration("e7de0bcb-c1e0-49c8-a2db-5195aec012df", importer, "B00001111", "10207000001170", "", "AB", "0495", billedAmount: 15012.23m);
			CreatedDeclaration("c000dbca-8a93-41c0-8799-a18364c75137", importer, "B00001112", "10207000001147");

			CreatedDeclaration("55f9f668-6206-430f-9d35-9f6b2e84ffd8", null, "B00001113", "10207000001181", JobMessageTypeList.Codes.LowValueShipments, billedAmount: 15011.23m);

			var importer2 = CreateImporter("3", false, true);
			CreatedDeclaration("8f11909a-9e3f-4516-8372-28802554e37f", importer2, "B00001114", "10207000001192", billedAmount: 15011.23m);
			CreatedDeclaration("a458a030-f1f9-4fbc-a173-d30ec925880a", importer2, "B00001115", "10207000001238", billedAmount: 10517.38m);
			CreatedDeclaration("701e7ee6-530d-4726-aa3b-9d2d1ebbdc43", importer2, "B00001116", "10207000001227");

			Factory.Save();

			var initialHeaders = (int)CargoWise.Data.Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.CusStatementHeader");

			var ediMessage = ProcessMessage(CreateMessage(interchangeString, "a70367cd-abf9-4211-a500-ad655ce30ce6"));
			AssertEquals("EM_MessageSubType", K84ReportTypes.Codes.Daily, ediMessage.EM_MessageSubType);
			AssertMessageInterpretation(ediMessage, "K84DailyReportInterpretation.htm", "Daily K84 Accounting Notice for 25-Jan-16", accountingDate: "25-Jan-16", statementDate: "26-Jan-16");

			Factory.Save();
			AssertEquals("no new CusStatementHeaders created", 0, (int)CargoWise.Data.Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.CusStatementHeader") - initialHeaders);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDailyK84MessageInterpretation()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+101231:0605+258++++++1'
UNG+CUSDEC+NOTICE+U10207V1+101231:0605+258+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
BGM+++9'
DTM+137:20160124:102'
RFF+ABP:10207'
UNS+D'
DMS+K10'
DTM+130:20160125:102'
DTM+353:20160124:102'
NAD+VC+0497'
LIN+++000001170'
MOA+155:51738'
MOA+105:100'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501223'
MOA+201:200'
MOA+128:1501423'
LIN+++000001147'
MOA+1:13000'
MOA+161:13000'
MOA+128:13000'
LIN+++35'
MOA+155:51738'
MOA+105:200'
MOA+4:1000000'
MOA+1:462385'
MOA+161:1514323'
MOA+201:300'
MOA+128:1514623'
LIN+++000001181'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++35'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++000001192'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++000001238'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
LIN+++000001227'
MOA+155:51738'
MOA+4:1000000'
MOA+1:449385'
MOA+161:1501123'
MOA+128:1501123'
DMS+K36'
MOA+155:155214'
MOA+4:3000000'
MOA+1:1348155'
MOA+161:4503369'
MOA+128:4503369'
DMS+K40'
DTM+130:20110429:102'
MOA+155:258690'
MOA+105:100'
MOA+4:5000000'
MOA+1:2259925'
MOA+161:7518715'
MOA+201:300'
MOA+128:7519015'
UNS+S'
UNT+70+1'
UNE+1+258'
UNZ+1+258'";

			#endregion

			var importer = CreateImporter("1", true, false);
			CreatedDeclaration("e7de0bcb-c1e0-49c8-a2db-5195aec012df", importer, "B00001111", "10207000001170", "", "AB", "0495", billedAmount: 15012.23m);
			CreatedDeclaration("c000dbca-8a93-41c0-8799-a18364c75137", importer, "B00001112", "10207000001147");

			CreatedDeclaration("55f9f668-6206-430f-9d35-9f6b2e84ffd8", null, "B00001113", "10207000001181", JobMessageTypeList.Codes.LowValueShipments, billedAmount: 15011.23m);

			var importer2 = CreateImporter("3", false, true);
			CreatedDeclaration("8f11909a-9e3f-4516-8372-28802554e37f", importer2, "B00001114", "10207000001192", billedAmount: 15011.23m);
			CreatedDeclaration("a458a030-f1f9-4fbc-a173-d30ec925880a", importer2, "B00001115", "10207000001238", billedAmount: 10517.38m);
			CreatedDeclaration("701e7ee6-530d-4726-aa3b-9d2d1ebbdc43", importer2, "B00001116", "10207000001227");

			Factory.Save();

			var initialHeaders = (int)CargoWise.Data.Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.CusStatementHeader");

			var ediMessage = ProcessMessage(CreateMessage(interchangeString, "a70367cd-abf9-4211-a500-ad655ce30ce6"));
			AssertEquals("EM_MessageSubType", K84ReportTypes.Codes.Daily, ediMessage.EM_MessageSubType);
			AssertMessageInterpretation(ediMessage, "K84DailyReportInterpretation.htm", "Daily K84 Accounting Notice for 24-Jan-16", accountingDate: "24-Jan-16", statementDate: "25-Jan-16");

			Factory.Save();
			AssertEquals("No CusStatementHeaders created", 0, (int)CargoWise.Data.Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.CusStatementHeader") - initialHeaders);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMonthlyK84Message()
		{
			#region Interchange String

			const string interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+110330:0607+567++++++1'
UNG+CUSDEC+K84++110330:0607+567+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
BGM+++9'
LOC+127+:::0497'
DTM+130:20110330:102'
RFF+ABP:10207'
UNS+D'
DMS+K50'
DTM+137:20110303:102'
MOA+155:113703'
MOA+1:202532'
MOA+161:316235'
DMS+K50'
DTM+137:20110304:102'
MOA+155:7366'
MOA+1:25496'
MOA+161:32862'
DMS+K50'
DTM+137:20110307:102'
MOA+155:45668'
MOA+105:2000'
MOA+4:3000'
MOA+1:96257'
MOA+161:146925'
DMS+K50'
DTM+137:20110308:102'
MOA+155:11'
MOA+1:1861623'
MOA+161:1861634'
DMS+K51'
MOA+155:166748'
MOA+105:2000'
MOA+4:3000'
MOA+1:2185908'
MOA+161:2357656'
DMS+K52++000000395'
DTM+137:20110330:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
DMS+K52++000000396'
DTM+137:20110331:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
DMS+K53'
MOA+155:2000'
MOA+105:4000'
MOA+4:6000'
MOA+1:8000'
MOA+161:20000'
DMS+K54'
DTM+137:20110330:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K54'
DTM+137:20110331:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K56++000000395'
DTM+137:20110330:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K56++000000396'
DTM+137:20110331:102'
MOA+155:1000'
MOA+105:2000'
MOA+4:3000'
MOA+1:4000'
MOA+161:10000'
MOA+201:5000'
MOA+128:15000'
DMS+K60++000000395'
DTM+137:20110330:102'
MOA+201:5000'
DMS+K60++000000396'
DTM+137:20110331:102'
MOA+201:5000'
DMS+K61++000000395'
DTM+137:20110330:102'
MOA+201:10000'
DMS+K61++000000396'
DTM+137:20110331:102'
MOA+201:10000'
DMS+K62'
DTM+137:20110330:102'
MOA+201:15000'
DMS+K62'
DTM+137:20110331:102'
MOA+201:15000'
DMS+K65'
MOA+201:10000'
MOA+202:20000'
MOA+208:30000'
MOA+128:60000'
DMS+K66'
PAT+1+2011033099999999'
PCD+16:.375'
PAT+7+2011033199999999'
PCD+15:001.002'
DMS+K70'
DTM+140:20110331:102'
MOA+155:166748'
MOA+105:2000'
MOA+4:3000'
MOA+1:2185908'
MOA+161:2357656'
MOA+128:2357656'
UNS+S'
UNT+38+1'
UNE+1+567'
UNZ+1+567'";

			#endregion

			var importer = CreateImporter("1", false);
			CreatedDeclaration("e7de0bcb-c1e0-49c8-a2db-5195aec012df", importer, "B00001111", "10207000000395");
			CreatedDeclaration("c000dbca-8a93-41c0-8799-a18364c75137", importer, "B00001112", "10207000000396");
			Factory.Save();

			var ediMessage = ProcessMessage(CreateMessage(interchangeString, "07fc855b-eb52-4183-8e08-6416287297d0"));
			AssertEquals("EM_MessageSubType", K84ReportTypes.Codes.Monthly, ediMessage.EM_MessageSubType);
			AssertMessageInterpretation(ediMessage, "K84MonthlyReportInterpretation.htm", "Monthly K84 Accounting Notice for 30-Mar-11");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOverdueReportMessage()
		{
			#region Interchange String

			const string interchangeString = @"
UNB+UNOA:3+INETCECPT+YUSAIRXPN+110407:0608+594++++++1'
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

			AssertProcessOverdueReportMessage(interchangeString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOverdueReportMessageForCUSDECVersion()
		{
			#region Interchange String

			const string interchangeString = @"
UNB+UNOA:3+INETCECPT+YUSAIRXPN+110407:0608+594++++++1'
UNG+CUSDEC+OVERDUE REPORT+U10207V1+110407:0608+594+UN+S:99B'
UNH+1+CUSDEC:S:99B:UN'
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

			AssertProcessOverdueReportMessage(interchangeString);
		}

		void AssertProcessOverdueReportMessage(string interchangeString)
		{
			var importer = CreateImporter("1", false);
			CreatedDeclaration("C0BBD298-0816-4A84-9B12-6111AC02B76B", importer, "B00001113", "10207000001056");
			CreatedDeclaration("7F33B198-22F0-469F-8193-EFAB2DB6EE82", importer, "B00001114", "10207400004228");
			importer = CreateImporter("2", false);
			CreatedDeclaration("E691612C-C71A-430D-B39F-F9C3057EFAA7", importer, "B00001115", "10207400004068");
			Factory.Save();

			var ediMessage = ProcessMessage(CreateMessage(interchangeString, "6160b004-f131-435c-8baf-58063c426f14"));
			AssertEquals("EM_MessageSubType", K84ReportTypes.Codes.Overdue, ediMessage.EM_MessageSubType);
			AssertMessageInterpretation(ediMessage, "OverdueReportInterpretation.htm", "Overdue Release Notice for 07-Apr-11");
		}

		#region Implementation

		void AssertMessageInterpretation(EDIMessage ediMessage, string expectedInterpretationFileName, string expectedSubject, string accountingDate = "", string statementDate = "")
		{
			var expectedInterpretation = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\" + expectedInterpretationFileName);
			if (!string.IsNullOrEmpty(accountingDate))
			{
				expectedInterpretation = expectedInterpretation.Replace("08-Apr-11", accountingDate);
			}

			if (!string.IsNullOrEmpty(statementDate))
			{
				expectedInterpretation = expectedInterpretation.Replace("11-Apr-11", statementDate);
			}

			expectedInterpretation = expectedInterpretation.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}");
			var interpretation = ediMessage.EM_MessageInterpretation.Replace("<tr", "\r\n<tr").Replace("<td", "\t\r\n<td").Replace("<th", "\t\r\n<th");
			AssertMultilineASCIIEquals(ediMessage.EM_MessageSubTypeDescription + " Interpretation", expectedInterpretation, interpretation);
			AssertEmail(expectedSubject, expectedInterpretation.Replace("\r\n<tr", "<tr").Replace("\t\r\n<td", "<td").Replace("\t\r\n<th", "<th"), ediMessage.EM_MessageText.Replace("\r\n", ""), string.Empty);
		}

		OrgHeader CreateImporter(string number, bool security, bool gstDirect = false)
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_Code = "IMPORTER" + number;
			importer.OH_FullName = "Test Importer " + number;
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsImporterDirectPayment = security;
			((OrgImpAddInfo)importer.CountryData.ImpAddInfo).ZO_IsGSTDirectPayment = gstDirect;
			return importer;
		}

		AccChargeCode DSBChargeCode
		{
			get
			{
				if (dSBChargeCode == null)
				{
					dSBChargeCode = Factory.New<AccChargeCode>();
					dSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
					dSBChargeCode.AC_Code = "TST";
					dSBChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
					dSBChargeCode.FillWithValidTestData();

					Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dSBChargeCode.PK.ToGuid());
				}
				return dSBChargeCode;
			}
		}
		AccChargeCode dSBChargeCode;

		void CreatedDeclaration(string pk, OrgHeader importer, ZString jobNumber, ZString transactionNo, string messageType = "", string messageSubType = "", string releaseOffice = "", decimal billedAmount = 0m)
		{
			declaration = Factory.NewWithPrimaryKey<JobDeclaration>(new Guid(pk));
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			if (!string.IsNullOrEmpty(messageType))
			{
				declaration.JE_MessageType = messageType;
			}

			if (!string.IsNullOrEmpty(messageSubType))
			{
				declaration.JE_MessageSubType = messageSubType;
			}

			if (!string.IsNullOrEmpty(releaseOffice))
			{
				declaration.CA_ReleaseOffice = releaseOffice;
			}

			if (importer != null)
			{
				declaration.JE_OH_Importer = importer.PK;
			}

			declaration.JE_DeclarationReference = jobNumber;
			declaration.JE_IsCancelled = false;
			var transactionNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumber.EntryType.CATransactionNumber, Constants.CountryCodes.Canada);
			transactionNumber.CE_EntryNum = transactionNo;

			if (billedAmount > 0m)
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.JH_ParentID = declaration.PK;
				jobHeader.JH_ParentTableCode = "JE";
				var charge1 = Factory.NewWithValidTestData<JobCharge>();
				charge1.JR_JH = jobHeader.PK;
				charge1.JR_AC = DSBChargeCode.PK;
				charge1.JR_LocalSellAmt = billedAmount;
				charge1.JR_OSSellAmt = billedAmount;
				var accTransHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
				accTransHeader.AH_JH = jobHeader.PK;
				accTransHeader.AH_Ledger = "AR";
				accTransHeader.AH_TransactionCategory = "DBT";
				accTransHeader.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
				var accTransLine = Factory.NewWithValidTestData<AccTransactionLines>();
				accTransLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				accTransLine.AL_AH = accTransHeader.PK;
				accTransLine.AL_LineAmount = billedAmount;
				accTransLine.AL_OSAmount = billedAmount;
				accTransLine.AL_AC = DSBChargeCode.PK;
				accTransLine.AL_JH = jobHeader.PK;
				accTransLine.AL_RevRecognitionType = "CUS";
				charge1.JR_AL_ARLine = accTransLine.PK;
			}
		}

		EDIMessage CreateMessage(string interchangeString, string messagePk)
		{
			var tmpMessage = CreateMessage(interchangeString);
			var interchange = tmpMessage.Interchange;
			Factory.NewWithPrimaryKey<EDIMessage>(new Guid(messagePk)).CopyPersistentValuesFrom(tmpMessage);
			tmpMessage.Delete();
			interchange.ContainedMessages.Load();
			return (EDIMessage)interchange.ContainedMessages[0];
		}

		EDIMessage CreateMessage(string interchangeString)
		{
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString.Replace("\r\n", ""), Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAIMP, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			return (EDIMessage)interchange.ContainedMessages[0];
		}

		EDIMessage ProcessMessage(EDIMessage ediMessage)
		{
			var impMessageProcessor = new IMPMessageProcessor(logger);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.K84Report, ediMessage.EM_MessageType);
			return ediMessage;
		}

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();

			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
			declaration = (JobDeclaration)JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "10207";
			declaration.TransactionNumber.SequentialNumber = "00000037";
			declaration.JE_IsCancelled = false;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			Factory.Save();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}

		JobDeclaration declaration;

		#endregion
	}
}
