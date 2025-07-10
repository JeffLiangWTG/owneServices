using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	[TestedType(typeof(JobConversationMessageCollection))]
	sealed class JobConversationMessageCollectionTest : ActiveBusinessObjectCollectionTestCase<JobConversationMessageCollection>
	{
		[TestDate(2016, 1, 2, 3, 4, 5)]
		public void TestCreateNewSetsTheDate()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var messages = new JobConversationMessageCollection(Factory, conversation);

			var newMessage = messages.AddNew();

			AssertEquals(new ZDateTime(2016, 1, 2, 3, 4, 5), newMessage.JCM_PostedTimeUtc);
		}

		public void TestAddNewMessage()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var messages = new JobConversationMessageCollection(Factory, conversation);

			var participant = conversation.Participants.AddNewParticipant(Factory.NewWithValidTestData<GlbStaff>());

			var newMessage = messages.AddNew(participant, "G'Day mate", false);

			AssertEquals(participant.PK, newMessage.JCM_JCP_Participant);
			AssertEquals("G'Day mate", newMessage.JCM_Body);
			AssertEquals(false, newMessage.JCM_IsInternal);
		}

		public void TestAddNewMessage_InvalidBody()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var messages = new JobConversationMessageCollection(Factory, conversation);

			var participant = conversation.Participants.AddNewParticipant(Factory.NewWithValidTestData<GlbStaff>());

			AssertExceptionThrown<ArgumentException>(() => messages.AddNew(participant, null, false));
			AssertExceptionThrown<ArgumentException>(() => messages.AddNew(participant, string.Empty, false));
		}

		public void TestAddNewMessage_NullParticipant()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var messages = new JobConversationMessageCollection(Factory, conversation);

			var message = messages.AddNew(null, "Hello", false);

			AssertEquals(message.Body, "Hello");
			Assert("No parent indicates that this is a system message", message.IsSystemMessage);
		}

		public void TestAddNewMessage_JCM_IsSystem()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var messages = new JobConversationMessageCollection(Factory, conversation);

			var systemGeneratedMessage = messages.AddNew(null, "Agrajag", isSystem: true);
			var nonSystemGeneratedMessage = messages.AddNew(null, "Not again");

			AssertEquals(true, systemGeneratedMessage.JCM_IsSystem);
			AssertEquals(false, nonSystemGeneratedMessage.JCM_IsSystem);
		}

		public void TestCopyFromBroadcastCopiesDataCorrectly()
		{
			var workItemConversation = Factory.NewWithValidTestData<JobConversation>();
			var incidentConversation = Factory.NewWithValidTestData<JobConversation>();

			var workItemConversationMessageCollection = new JobConversationMessageCollection(Factory, workItemConversation);
			var incidentConversationMessageCollection = new JobConversationMessageCollection(Factory, incidentConversation);

			workItemConversationMessageCollection.AddNew(null, "First", false);
			workItemConversationMessageCollection.AddNew(null, "Second", false);

			AssertEquals(0, incidentConversationMessageCollection.Count);

			incidentConversationMessageCollection.CopyFromBroadcast(workItemConversationMessageCollection);

			AssertEquals(2, incidentConversationMessageCollection.Count);
		}

		public void TestCopyFromBroadcastDoesNotCopySystemAndInternalMessages()
		{
			var workItemConversation = Factory.NewWithValidTestData<JobConversation>();
			var incidentConversation = Factory.NewWithValidTestData<JobConversation>();

			var workItemConversationMessageCollection = new JobConversationMessageCollection(Factory, workItemConversation);
			var incidentConversationMessageCollection = new JobConversationMessageCollection(Factory, incidentConversation);

			workItemConversationMessageCollection.AddNew(null, "First", false);
			workItemConversationMessageCollection.AddNew(null, "Second", false);
			workItemConversationMessageCollection.AddNew(null, "Important system message", false, true);
			workItemConversationMessageCollection.AddNew(null, "Super secure internal message, not for clients", true);

			AssertEquals(0, incidentConversationMessageCollection.Count);

			incidentConversationMessageCollection.CopyFromBroadcast(workItemConversationMessageCollection);

			AssertEquals(2, incidentConversationMessageCollection.Count);
		}

		public void TestAddNewMessageJCM_Body_LongString()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var messages = new JobConversationMessageCollection(Factory, conversation);
			var participant = conversation.Participants.AddNewParticipant(Factory.NewWithValidTestData<GlbStaff>());
			var newMessage = messages.AddNew(participant, LongString, false);

			Factory.Save();

			var message = new BusinessObjectFactory().Load<JobConversationMessage>(newMessage.PK);
			AssertEquals(participant.PK, message.JCM_JCP_Participant);
			AssertContains("CargoWise.EntityFramework.ZSaveException:", message.JCM_Body);
			Assert(message.JCM_Body.Length <= 42000);
		}

		protected override JobConversationMessageCollection GetCollectionToTest()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();

			return new JobConversationMessageCollection(Factory, conversation);
		}

		#region LongString Property

		string LongString => @"
CargoWise.EntityFramework.ZSaveException: 
** Error Saving Record **

Tablename: JobConversationMessage
PK: bbb3a2a8-c548-4057-9c4b-252777776db7
RowState: Added
Factory validation suspended: False
Factory name for debugging: 
Business object around row = Enterprise.Client.EDI.IncidentManager.Business.EdiJobConversationMessage
Business object validation suspended: 0
Business object is marking as needing validation suspended: False
Business object light validation is enabled: False
Business object additional info: 

Inner Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"". The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.

 --->CargoWise.EntityFramework.ZDataException: Error from Data layer: TableName = JobConversationMessage, PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7, RowState = Added
JCM_PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
		JCM_Body = Thread - Topic: Update on Incident:
			CS00886826 - TLX Crashed - entries around
that time posted  $0
Thread - Index: AQHWjVRoTjIYJKmCmEmPaS4VPsfO46ltlBxw
  Date: Fri, 18 Sep 2020 01:14:36 + 0000
Message - ID: < MEAPR01MB368840247114287023C70B05B13F0@MEAPR01MB3688.ausprd01.prod.outlook.com >
   References: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
			In - Reply - To: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
						 Accept - Language: en - US
Content - Language: en - US
X - MS - Has - Attach: yes
X - MS - TNEF - Correlator:
Authentication - Results - Original: wisetechglobal.com;
			dkim = none(message not

signed) header.d = none;
			wisetechglobal.com;
			dmarc = none action = none
 header.from = goldstartransport.com.au;
			x - originating - ip: [49.255.17.178]
x - ms - publictraffictype: Email
X - MS - Office365 - Filtering - Correlation - Id: ec8f0c81 - 3614 - 4da0 - afd9 - 08d85b70e898
x - ms - traffictypediagnostic: MEXPR01MB1975:| SY3PR01MB1020:
x - microsoft - antispam - prvs: < MEXPR01MB19752C3734A8CDA9522DCC93B13F0@MEXPR01MB1975.ausprd01.prod.outlook.com >
	   x - ms - oob - tlc - oobclassifiers: OLM:
			131;
OLM:
			131;
			x - ms - exchange - senderadcheck: 1
X - Microsoft - Antispam - Untrusted: BCL:
			0;
			X - Microsoft - Antispam - Message - Info - Original: N4pG1sGvd227gFebpC2hhHltHmvdC0yU1syBOIwo4WAV6Cd8B / N297kh2ma29ZBR8NFtMatslb1aszHocKi5yflWmZrrHKJpZWiH6i99QKp95 / 6YoRFMe0E + I1UAENQPXqYyTJBoT8kUMq0ZXKogyW6q1L5K9LpE8 + umdZq9T + piy12oysSgsHVWuX7R4pobKRIrVD0qvh5CHw5DGutHGFz2Q + SQM / yFZ862gwMzL4welV8WgaooxGfkSTB39iNmvElkjSWTI9sAWyis21hsyFrvsuMFuKdnJATfM598E63Ge9yXhNVsx1WlO0yCYCQPf7kHxJKaP54zAFCUb16YXQ ==
						  X - Forefront - Antispam - Report - Untrusted: CIP:
			255.255.255.255;
CTRY:
			;
LANG:
			en;
SCL:
			1;
SRV:
			;
IPV:
			NLI;
SFV:
			NSPM;
H:
			MEAPR01MB3688.ausprd01.prod.outlook.com;
PTR:
			;
CAT:
			NONE;
SFS:
			(396003)(39850400004)(366004)(136003)(346002)(376002)(9686003)(5660300002)(55016002)(15650500001)(83380400001)(33656002)(8936002)(316002)(99936003)(478600001)(53546011)(6506007)(86362001)(7696005)(52536014)(186003)(8676002)(6916009)(26005)(66446008)(64756008)(66556008)(66476007)(66576008)(2906002)(71200400001)(66946007)(76116006);
DIR:
			OUT;
SFP:
			1101;
			X - MS - Exchange - AntiSpam - MessageData - Original: rAo5ssL + Pgi4JSmm5PLPsrdk0UdvYXSJS5nk6fbhgaPlWnBmoDA2VcICQ8b8Yx7 + mR9ONa9UQjL88ZdRU02wvIwVOVzo9G3hcQQMGCsLp4iaKqJAwJFCgBthd0nstpLIymvLBT9AjJvy2yrINuwAfopjeGnaaOTixjKBObsnDy1f9NTdTwviT8q5iUrjXGQe9wNVCRyBL7g7 + B6QQBi + KbOC7uWt4uAT4ay0w3ZmaodNj0R8uzNVBdGrN5J6 + NELWoJ8yBLgRMBo5zSnIQFG9RYXohGIzWExzs7EE5qhRRRomMyjZC59RS1Bs3h7sm3F2Cqja9RQRc2HrkmdcbQQVK2Ay9t5STu69OEHu9SL9Z0BMT0wUsVi0tq0IMZCcoMAzdsqvQIvOpxppM7g8qUeiGmCXhRSEU + kht2xn5pw6H9C / 7nsnzMHyoFrhPFm8c / wgw75016aRVOKTsc0SJlggPyneo9Gxod6G14IPeeZvBPdKRwgWWQTYdxIIHpOSdVtm8e + 06EsizFChLZf + QWk4GUt5ZU0e6fW6sJCB0Pz8KXVPKTwgH / jS0GejQ21M7dOBY03Igya1NI7U1NnU0A + t0abIFIiFovShHesxH7eTVovekFuVGaXFHLZDHCxX9u / DMYqtq9lu8m0k05RoUOpFg ==
										x - ms - exchange - transport - forked: True
Content - Type: multipart / mixed;
			boundary = ""_008_MEAPR01MB368840247114287023C70B05B13F0MEAPR01MB3688ausp_""
X - MS - Exchange - Transport - CrossTenantHeadersStamped: MEXPR01MB1975
X - CLX - Shades: Deliver
X - CLX - Response: 1TFkXGxoaHxEKTHoXHhIRCllEF215H1tiRnMeYGEBEQpYWBdlZlxCeGFEe2l BchEKeE4XaVoYRkBzfWBMaH0RCnlMF2V4e28bRUsfZEhSEQpDSBcHGBwdEQpDWRcHGx8bEQpDSR caBBoaGhEKWU0Xbk9GQ1xPWBEKX1kXGBgbEQpfTRdnZnIRCllJFxpxGhAadwYYGhtxGBoYEBp3B
	 hgaBhoRClleF2hjeREKSUYXXUNZT15PSUJNRkVIS0Z1QkVZXk9OEQpDThduT0dZEmRdHV8aSHwc UG1AWVxoW0sYB0NMG0sSfB9yUBEKWFwXHwQaBBgbHgUbGgQbGhoEHhIEGxMQGx4aHxoRCl5ZF31 HSwVoEQpNXBceHRoRCkxaF2lrTWsRCkxGF29ra2tsa2sRCkJPF2l+SWkYGnxLG11PEQpDWhceGg
 QbGh0EGxkdBBIeEQpCXhcbEQpCXBcaEQpCRRdgQngZTklmWkJSUhEKQk4XaVoYRkBzfWBMaH0RC kJMF2VmXEJ4YUR7aUFyEQpCbBdtZUxfcHtQSEJsZREKQkAXZWFtckJTQRt7SXwRCkJYF2tsRm5P S09ORE5cEQpaWBcYEQp5QxdnQlNNWRlYQXhaQxEKcGcXbHx7ZWdtfVhGQ1sQGRoRCnBoF21BQ1l
 kAUQaXX19EBkaEQpwaBdjR1NickhwSEh9RRAZGhEKcGgXZBpyZUJ7elgYE24QGRoRCnBoF2xPR0 gaH0VLbBJZEBsbHhEKcGgXY2kFTk1kW1IeehMQGRoRCnBnF2hFElB5engeeFBvEBkaEQpwbBdhW k1cYBNHbxhYUBAZGhEKcEwXbENsGm1IfH9temcQGxgZEQptfhcaEQpYTRdLESA =
X - Proofpoint - Virus - Version: vendor = fsecure engine = 2.50.10434:6.0.235,18.0.687
 definitions = 2020 - 09 - 17_20:2020 - 09 - 16,2020 - 09 - 17 signatures = 0
X - Proofpoint - Spam - Details: rule = notspam policy = default score = 0 lowpriorityscore = 0 phishscore = 0
 mlxlogscore = 999 priorityscore = 48 bulkscore = 0 suspectscore = 0 spamscore = 0
 malwarescore = 0 adultscore = 0 clxscore = 1005 impostorscore = 0 mlxscore = 0
 classifier = clx:Deliver adjust = 0 reason = mlx scancount = 2
 engine = 8.12.0 - 2006250000 definitions = main - 2009180010
Return - Path: donna.strohfeldt @goldstartransport.com.au


   Exception


JCM_IsBroadcast = False
JCM_IsInternal = False
JCM_IsLocal = False
JCM_IsSystem = False
JCM_JCC_Conversation = 09315bc9 - 8c7b - 48cc - 9af5 - b1bca8d57595
JCM_JCP_Participant = 5ebe963f - 943a - 42fe - 9389 - ca6429be6c55
JCM_Language = EN
JCM_PostedTimeUtc = 18 / 09 / 2020 1:24:13 AM
	JCM_Score = 0
InnerException Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'. --->System.Data.SqlClient.SqlException: The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.
   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)
   at System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean & dataReady)
   at System.Data.SqlClient.SqlCommand.RunExecuteNonQueryTds(String methodName, Boolean async, Int32 timeout, Boolean asyncWrite)
   at System.Data.SqlClient.SqlCommand.InternalExecuteNonQuery(TaskCompletionSource`1 completion, String methodName, Boolean sendToPipe, Int32 timeout, Boolean & usedCache, Boolean asyncWrite, Boolean inRetry)
   at System.Data.SqlClient.SqlCommand.ExecuteNonQuery()
   at CargoWise.Data.NonQueryCommandRunner.ExecuteCore(IDbCommand command)
   at CargoWise.Data.DbCommand.ExecuteCore(CommandRunner cmdRunner)
   at CargoWise.Data.DbCommand.Execute(CommandRunner cmdRunner)
   at CargoWise.Data.DbCommand.System.Data.IDbCommand.ExecuteNonQuery()
   at CargoWise.EntityFramework.ZSaveCommand.ExecuteStandardPart()
   -- - End of inner exception stack trace-- -
	at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)
   at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)
   at CargoWise.EntityFramework.ZSqlSaver.SaveRows(IList`1 rows)
   at CargoWise.EntityFramework.ZSaver.Save()
   at CargoWise.EntityFramework.ZAccessor.Save()
   at CargoWise.EntityFramework.RowFactory.CargoWise.Integration.ITransactionParticipant.SaveInTransaction()
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()
   -- - End of inner exception stack trace-- -
	at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()
   at System.Linq.Enumerable.WhereSelectArrayIterator`2.MoveNext()
   at System.Linq.Buffer`1..ctor(IEnumerable`1 source)
   at System.Linq.Enumerable.ToArray[TSource](IEnumerable`1 source)
   at CargoWise.EntityFramework.TransactionCoordinator.SaveInTransactions()
   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] factories)
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.SaveMailFactory(MailItem mailItem)
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.ProcessMailItem(MailItem mailItem)
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.CreateAndProcessMailItem(Email email)
   at Enterprise.Client.EDI.Mail.ServiceTasks.EmailProcessorServiceProvider.ProcessEmail(Email email)


CargoWise.EntityFramework.ZSaveException: 
**Error Saving Record **

Tablename: JobConversationMessage
PK: bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
	   RowState: Added
	   Factory validation suspended: False
	   Factory name for debugging: 
Business object around row = Enterprise.Client.EDI.IncidentManager.Business.EdiJobConversationMessage
	   Business object validation suspended: 0
	   Business object is marking as needing validation suspended: False
	   Business object light validation is enabled: False
	   Business object additional info: 


	   Inner Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.
	   

		--->CargoWise.EntityFramework.ZDataException: Error from Data layer: TableName = JobConversationMessage, PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7, RowState = Added
	   JCM_PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
	   JCM_Body = Thread - Topic: Update on Incident: CS00886826 - TLX Crashed - entries around
	   
		that time posted  $0
	   Thread - Index: AQHWjVRoTjIYJKmCmEmPaS4VPsfO46ltlBxw
	   Date: Fri, 18 Sep 2020 01:14:36 + 0000
	   Message - ID: < MEAPR01MB368840247114287023C70B05B13F0@MEAPR01MB3688.ausprd01.prod.outlook.com >
	   References: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
	   In - Reply - To: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
	   Accept - Language: en - US
	   Content - Language: en - US
	   X - MS - Has - Attach: yes
	   X - MS - TNEF - Correlator:
Authentication - Results - Original: wisetechglobal.com; dkim = none(message not
	   
		signed) header.d = none;
			wisetechglobal.com;
			dmarc = none action = none
 header.from = goldstartransport.com.au;
			x - originating - ip: [49.255.17.178]
x - ms - publictraffictype: Email
X - MS - Office365 - Filtering - Correlation - Id: ec8f0c81 - 3614 - 4da0 - afd9 - 08d85b70e898
x - ms - traffictypediagnostic: MEXPR01MB1975:| SY3PR01MB1020:
x - microsoft - antispam - prvs: < MEXPR01MB19752C3734A8CDA9522DCC93B13F0@MEXPR01MB1975.ausprd01.prod.outlook.com >
	   x - ms - oob - tlc - oobclassifiers: OLM:
			131;
OLM:

signed) header.d = none;
			wisetechglobal.com;
			dmarc = none action = none
 header.from = goldstartransport.com.au;
			x - originating - ip: [49.255.17.178]
x - ms - publictraffictype: Email
X - MS - Office365 - Filtering - Correlation - Id: ec8f0c81 - 3614 - 4da0 - afd9 - 08d85b70e898
x - ms - traffictypediagnostic: MEXPR01MB1975:| SY3PR01MB1020:
x - microsoft - antispam - prvs: < MEXPR01MB19752C3734A8CDA9522DCC93B13F0@MEXPR01MB1975.ausprd01.prod.outlook.com >
	   x - ms - oob - tlc - oobclassifiers: OLM:
			131;
OLM:
			131;
			x - ms - exchange - senderadcheck: 1
X - Microsoft - Antispam - Untrusted: BCL:
			0;
			X - Microsoft - Antispam - Message - Info - Original: N4pG1sGvd227gFebpC2hhHltHmvdC0yU1syBOIwo4WAV6Cd8B / N297kh2ma29ZBR8NFtMatslb1aszHocKi5yflWmZrrHKJpZWiH6i99QKp95 / 6YoRFMe0E + I1UAENQPXqYyTJBoT8kUMq0ZXKogyW6q1L5K9LpE8 + umdZq9T + piy12oysSgsHVWuX7R4pobKRIrVD0qvh5CHw5DGutHGFz2Q + SQM / yFZ862gwMzL4welV8WgaooxGfkSTB39iNmvElkjSWTI9sAWyis21hsyFrvsuMFuKdnJATfM598E63Ge9yXhNVsx1WlO0yCYCQPf7kHxJKaP54zAFCUb16YXQ ==
						  X - Forefront - Antispam - Report - Untrusted: CIP:
			255.255.255.255;
CTRY:
			;
LANG:
			en;
SCL:
			1;
SRV:
			;
IPV:
			NLI;
SFV:
			NSPM;
H:
			MEAPR01MB3688.ausprd01.prod.outlook.com;
PTR:
			;
CAT:
			NONE;
SFS:
			(396003)(39850400004)(366004)(136003)(346002)(376002)(9686003)(5660300002)(55016002)(15650500001)(83380400001)(33656002)(8936002)(316002)(99936003)(478600001)(53546011)(6506007)(86362001)(7696005)(52536014)(186003)(8676002)(6916009)(26005)(66446008)(64756008)(66556008)(66476007)(66576008)(2906002)(71200400001)(66946007)(76116006);
DIR:
			OUT;
SFP:
			1101;
			X - MS - Exchange - AntiSpam - MessageData - Original: rAo5ssL + Pgi4JSmm5PLPsrdk0UdvYXSJS5nk6fbhgaPlWnBmoDA2VcICQ8b8Yx7 + mR9ONa9UQjL88ZdRU02wvIwVOVzo9G3hcQQMGCsLp4iaKqJAwJFCgBthd0nstpLIymvLBT9AjJvy2yrINuwAfopjeGnaaOTixjKBObsnDy1f9NTdTwviT8q5iUrjXGQe9wNVCRyBL7g7 + B6QQBi + KbOC7uWt4uAT4ay0w3ZmaodNj0R8uzNVBdGrN5J6 + NELWoJ8yBLgRMBo5zSnIQFG9RYXohGIzWExzs7EE5qhRRRomMyjZC59RS1Bs3h7sm3F2Cqja9RQRc2HrkmdcbQQVK2Ay9t5STu69OEHu9SL9Z0BMT0wUsVi0tq0IMZCcoMAzdsqvQIvOpxppM7g8qUeiGmCXhRSEU + kht2xn5pw6H9C / 7nsnzMHyoFrhPFm8c / wgw75016aRVOKTsc0SJlggPyneo9Gxod6G14IPeeZvBPdKRwgWWQTYdxIIHpOSdVtm8e + 06EsizFChLZf + QWk4GUt5ZU0e6fW6sJCB0Pz8KXVPKTwgH / jS0GejQ21M7dOBY03Igya1NI7U1NnU0A + t0abIFIiFovShHesxH7eTVovekFuVGaXFHLZDHCxX9u / DMYqtq9lu8m0k05RoUOpFg ==
										x - ms - exchange - transport - forked: True
Content - Type: multipart / mixed;
			boundary = ""_008_MEAPR01MB368840247114287023C70B05B13F0MEAPR01MB3688ausp_""
X - MS - Exchange - Transport - CrossTenantHeadersStamped: MEXPR01MB1975
X - CLX - Shades: Deliver
X - CLX - Response: 1TFkXGxoaHxEKTHoXHhIRCllEF215H1tiRnMeYGEBEQpYWBdlZlxCeGFEe2l BchEKeE4XaVoYRkBzfWBMaH0RCnlMF2V4e28bRUsfZEhSEQpDSBcHGBwdEQpDWRcHGx8bEQpDSR caBBoaGhEKWU0Xbk9GQ1xPWBEKX1kXGBgbEQpfTRdnZnIRCllJFxpxGhAadwYYGhtxGBoYEBp3B
	 hgaBhoRClleF2hjeREKSUYXXUNZT15PSUJNRkVIS0Z1QkVZXk9OEQpDThduT0dZEmRdHV8aSHwc UG1AWVxoW0sYB0NMG0sSfB9yUBEKWFwXHwQaBBgbHgUbGgQbGhoEHhIEGxMQGx4aHxoRCl5ZF31 HSwVoEQpNXBceHRoRCkxaF2lrTWsRCkxGF29ra2tsa2sRCkJPF2l+SWkYGnxLG11PEQpDWhceGg
 QbGh0EGxkdBBIeEQpCXhcbEQpCXBcaEQpCRRdgQngZTklmWkJSUhEKQk4XaVoYRkBzfWBMaH0RC kJMF2VmXEJ4YUR7aUFyEQpCbBdtZUxfcHtQSEJsZREKQkAXZWFtckJTQRt7SXwRCkJYF2tsRm5P S09ORE5cEQpaWBcYEQp5QxdnQlNNWRlYQXhaQxEKcGcXbHx7ZWdtfVhGQ1sQGRoRCnBoF21BQ1l
 kAUQaXX19EBkaEQpwaBdjR1NickhwSEh9RRAZGhEKcGgXZBpyZUJ7elgYE24QGRoRCnBoF2xPR0 gaH0VLbBJZEBsbHhEKcGgXY2kFTk1kW1IeehMQGRoRCnBnF2hFElB5engeeFBvEBkaEQpwbBdhW k1cYBNHbxhYUBAZGhEKcEwXbENsGm1IfH9temcQGxgZEQptfhcaEQpYTRdLESA =
X - Proofpoint - Virus - Version: vendor = fsecure engine = 2.50.10434:6.0.235,18.0.687
 definitions = 2020 - 09 - 17_20:2020 - 09 - 16,2020 - 09 - 17 signatures = 0
X - Proofpoint - Spam - Details: rule = notspam policy = default score = 0 lowpriorityscore = 0 phishscore = 0
 mlxlogscore = 999 priorityscore = 48 bulkscore = 0 suspectscore = 0 spamscore = 0
 malwarescore = 0 adultscore = 0 clxscore = 1005 impostorscore = 0 mlxscore = 0
 classifier = clx:Deliver adjust = 0 reason = mlx scancount = 2
 engine = 8.12.0 - 2006250000 definitions = main - 2009180010
Return - Path: donna.strohfeldt @goldstartransport.com.au


   Exception


JCM_IsBroadcast = False
JCM_IsInternal = False
JCM_IsLocal = False
JCM_IsSystem = False
JCM_JCC_Conversation = 09315bc9 - 8c7b - 48cc - 9af5 - b1bca8d57595
JCM_JCP_Participant = 5ebe963f - 943a - 42fe - 9389 - ca6429be6c55
JCM_Language = EN
JCM_PostedTimeUtc = 18 / 09 / 2020 1:24:13 AM
	JCM_Score = 0
InnerException Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'. --->System.Data.SqlClient.SqlException: The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.
   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)
   at System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean & dataReady)
   at System.Data.SqlClient.SqlCommand.RunExecuteNonQueryTds(String methodName, Boolean async, Int32 timeout, Boolean asyncWrite)
   at System.Data.SqlClient.SqlCommand.InternalExecuteNonQuery(TaskCompletionSource`1 completion, String methodName, Boolean sendToPipe, Int32 timeout, Boolean & usedCache, Boolean asyncWrite, Boolean inRetry)
   at System.Data.SqlClient.SqlCommand.ExecuteNonQuery()
   at CargoWise.Data.NonQueryCommandRunner.ExecuteCore(IDbCommand command)
   at CargoWise.Data.DbCommand.ExecuteCore(CommandRunner cmdRunner)
   at CargoWise.Data.DbCommand.Execute(CommandRunner cmdRunner)
   at CargoWise.Data.DbCommand.System.Data.IDbCommand.ExecuteNonQuery()
   at CargoWise.EntityFramework.ZSaveCommand.ExecuteStandardPart()
   -- - End of inner exception stack trace-- -
	at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)
   at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)
   at CargoWise.EntityFramework.ZSqlSaver.SaveRows(IList`1 rows)
   at CargoWise.EntityFramework.ZSaver.Save()
   at CargoWise.EntityFramework.ZAccessor.Save()
   at CargoWise.EntityFramework.RowFactory.CargoWise.Integration.ITransactionParticipant.SaveInTransaction()
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()
   -- - End of inner exception stack trace-- -
	at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()
   at System.Linq.Enumerable.WhereSelectArrayIterator`2.MoveNext()
   at System.Linq.Buffer`1..ctor(IEnumerable`1 source)
   at System.Linq.Enumerable.ToArray[TSource](IEnumerable`1 source)
   at CargoWise.EntityFramework.TransactionCoordinator.SaveInTransactions()
   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] factories)
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.SaveMailFactory(MailItem mailItem)
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.ProcessMailItem(MailItem mailItem)
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.CreateAndProcessMailItem(Email email)
   at Enterprise.Client.EDI.Mail.ServiceTasks.EmailProcessorServiceProvider.ProcessEmail(Email email)


CargoWise.EntityFramework.ZSaveException: 
**Error Saving Record **

Tablename: JobConversationMessage
PK: bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
	   RowState: Added
	   Factory validation suspended: False
	   Factory name for debugging: 
Business object around row = Enterprise.Client.EDI.IncidentManager.Business.EdiJobConversationMessage
	   Business object validation suspended: 0
	   Business object is marking as needing validation suspended: False
	   Business object light validation is enabled: False
	   Business object additional info: 


	   Inner Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.
	   

		--->CargoWise.EntityFramework.ZDataException: Error from Data layer: TableName = JobConversationMessage, PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7, RowState = Added
	   JCM_PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
	   JCM_Body = Thread - Topic: Update on Incident: CS00886826 - TLX Crashed - entries around
	   
		that time posted  $0
	   Thread - Index: AQHWjVRoTjIYJKmCmEmPaS4VPsfO46ltlBxw
	   Date: Fri, 18 Sep 2020 01:14:36 + 0000
	   Message - ID: < MEAPR01MB368840247114287023C70B05B13F0@MEAPR01MB3688.ausprd01.prod.outlook.com >
	   References: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
	   In - Reply - To: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
	   Accept - Language: en - US
	   Content - Language: en - US
	   X - MS - Has - Attach: yes
	   X - MS - TNEF - Correlator:
Authentication - Results - Original: wisetechglobal.com; dkim = none(message not
	   
		signed) header.d = none;
			wisetechglobal.com;
			dmarc = none action = none
 header.from = goldstartransport.com.au;
			x - originating - ip: [49.255.17.178]
x - ms - publictraffictype: Email
X - MS - Office365 - Filtering - Correlation - Id: ec8f0c81 - 3614 - 4da0 - afd9 - 08d85b70e898
x - ms - traffictypediagnostic: MEXPR01MB1975:| SY3PR01MB1020:
x - microsoft - antispam - prvs: < MEXPR01MB19752C3734A8CDA9522DCC93B13F0@MEXPR01MB1975.ausprd01.prod.outlook.com >
	   x - ms - oob - tlc - oobclassifiers: OLM:
			131;
OLM:
signed) header.d = none;
			wisetechglobal.com;
			dmarc = none action = none
 header.from = goldstartransport.com.au;
			x - originating - ip: [49.255.17.178]
x - ms - publictraffictype: Email
X - MS - Office365 - Filtering - Correlation - Id: ec8f0c81 - 3614 - 4da0 - afd9 - 08d85b70e898
x - ms - traffictypediagnostic: MEXPR01MB1975:| SY3PR01MB1020:
x - microsoft - antispam - prvs: < MEXPR01MB19752C3734A8CDA9522DCC93B13F0@MEXPR01MB1975.ausprd01.prod.outlook.com >
	   x - ms - oob - tlc - oobclassifiers: OLM:
			131;
OLM:
			131;
			x - ms - exchange - senderadcheck: 1
X - Microsoft - Antispam - Untrusted: BCL:
			0;
			X - Microsoft - Antispam - Message - Info - Original: N4pG1sGvd227gFebpC2hhHltHmvdC0yU1syBOIwo4WAV6Cd8B / N297kh2ma29ZBR8NFtMatslb1aszHocKi5yflWmZrrHKJpZWiH6i99QKp95 / 6YoRFMe0E + I1UAENQPXqYyTJBoT8kUMq0ZXKogyW6q1L5K9LpE8 + umdZq9T + piy12oysSgsHVWuX7R4pobKRIrVD0qvh5CHw5DGutHGFz2Q + SQM / yFZ862gwMzL4welV8WgaooxGfkSTB39iNmvElkjSWTI9sAWyis21hsyFrvsuMFuKdnJATfM598E63Ge9yXhNVsx1WlO0yCYCQPf7kHxJKaP54zAFCUb16YXQ ==
						  X - Forefront - Antispam - Report - Untrusted: CIP:
			255.255.255.255;
CTRY:
			;
LANG:
			en;
SCL:
			1;
SRV:
			;
IPV:
			NLI;
SFV:
			NSPM;
H:
			MEAPR01MB3688.ausprd01.prod.outlook.com;
PTR:
			;
CAT:
			NONE;
SFS:
			(396003)(39850400004)(366004)(136003)(346002)(376002)(9686003)(5660300002)(55016002)(15650500001)(83380400001)(33656002)(8936002)(316002)(99936003)(478600001)(53546011)(6506007)(86362001)(7696005)(52536014)(186003)(8676002)(6916009)(26005)(66446008)(64756008)(66556008)(66476007)(66576008)(2906002)(71200400001)(66946007)(76116006);
DIR:
			OUT;
SFP:
			1101;
			X - MS - Exchange - AntiSpam - MessageData - Original: rAo5ssL + Pgi4JSmm5PLPsrdk0UdvYXSJS5nk6fbhgaPlWnBmoDA2VcICQ8b8Yx7 + mR9ONa9UQjL88ZdRU02wvIwVOVzo9G3hcQQMGCsLp4iaKqJAwJFCgBthd0nstpLIymvLBT9AjJvy2yrINuwAfopjeGnaaOTixjKBObsnDy1f9NTdTwviT8q5iUrjXGQe9wNVCRyBL7g7 + B6QQBi + KbOC7uWt4uAT4ay0w3ZmaodNj0R8uzNVBdGrN5J6 + NELWoJ8yBLgRMBo5zSnIQFG9RYXohGIzWExzs7EE5qhRRRomMyjZC59RS1Bs3h7sm3F2Cqja9RQRc2HrkmdcbQQVK2Ay9t5STu69OEHu9SL9Z0BMT0wUsVi0tq0IMZCcoMAzdsqvQIvOpxppM7g8qUeiGmCXhRSEU + kht2xn5pw6H9C / 7nsnzMHyoFrhPFm8c / wgw75016aRVOKTsc0SJlggPyneo9Gxod6G14IPeeZvBPdKRwgWWQTYdxIIHpOSdVtm8e + 06EsizFChLZf + QWk4GUt5ZU0e6fW6sJCB0Pz8KXVPKTwgH / jS0GejQ21M7dOBY03Igya1NI7U1NnU0A + t0abIFIiFovShHesxH7eTVovekFuVGaXFHLZDHCxX9u / DMYqtq9lu8m0k05RoUOpFg ==
										x - ms - exchange - transport - forked: True
Content - Type: multipart / mixed;
			boundary = ""_008_MEAPR01MB368840247114287023C70B05B13F0MEAPR01MB3688ausp_""
X - MS - Exchange - Transport - CrossTenantHeadersStamped: MEXPR01MB1975
X - CLX - Shades: Deliver
X - CLX - Response: 1TFkXGxoaHxEKTHoXHhIRCllEF215H1tiRnMeYGEBEQpYWBdlZlxCeGFEe2l BchEKeE4XaVoYRkBzfWBMaH0RCnlMF2V4e28bRUsfZEhSEQpDSBcHGBwdEQpDWRcHGx8bEQpDSR caBBoaGhEKWU0Xbk9GQ1xPWBEKX1kXGBgbEQpfTRdnZnIRCllJFxpxGhAadwYYGhtxGBoYEBp3B
	 hgaBhoRClleF2hjeREKSUYXXUNZT15PSUJNRkVIS0Z1QkVZXk9OEQpDThduT0dZEmRdHV8aSHwc UG1AWVxoW0sYB0NMG0sSfB9yUBEKWFwXHwQaBBgbHgUbGgQbGhoEHhIEGxMQGx4aHxoRCl5ZF31 HSwVoEQpNXBceHRoRCkxaF2lrTWsRCkxGF29ra2tsa2sRCkJPF2l+SWkYGnxLG11PEQpDWhceGg
 QbGh0EGxkdBBIeEQpCXhcbEQpCXBcaEQpCRRdgQngZTklmWkJSUhEKQk4XaVoYRkBzfWBMaH0RC kJMF2VmXEJ4YUR7aUFyEQpCbBdtZUxfcHtQSEJsZREKQkAXZWFtckJTQRt7SXwRCkJYF2tsRm5P S09ORE5cEQpaWBcYEQp5QxdnQlNNWRlYQXhaQxEKcGcXbHx7ZWdtfVhGQ1sQGRoRCnBoF21BQ1l
 kAUQaXX19EBkaEQpwaBdjR1NickhwSEh9RRAZGhEKcGgXZBpyZUJ7elgYE24QGRoRCnBoF2xPR0 gaH0VLbBJZEBsbHhEKcGgXY2kFTk1kW1IeehMQGRoRCnBnF2hFElB5engeeFBvEBkaEQpwbBdhW k1cYBNHbxhYUBAZGhEKcEwXbENsGm1IfH9temcQGxgZEQptfhcaEQpYTRdLESA =
X - Proofpoint - Virus - Version: vendor = fsecure engine = 2.50.10434:6.0.235,18.0.687
 definitions = 2020 - 09 - 17_20:2020 - 09 - 16,2020 - 09 - 17 signatures = 0
X - Proofpoint - Spam - Details: rule = notspam policy = default score = 0 lowpriorityscore = 0 phishscore = 0
 mlxlogscore = 999 priorityscore = 48 bulkscore = 0 suspectscore = 0 spamscore = 0
 malwarescore = 0 adultscore = 0 clxscore = 1005 impostorscore = 0 mlxscore = 0
 classifier = clx:Deliver adjust = 0 reason = mlx scancount = 2
 engine = 8.12.0 - 2006250000 definitions = main - 2009180010
Return - Path: donna.strohfeldt @goldstartransport.com.au


   Exception


JCM_IsBroadcast = False
JCM_IsInternal = False
JCM_IsLocal = False
JCM_IsSystem = False
JCM_JCC_Conversation = 09315bc9 - 8c7b - 48cc - 9af5 - b1bca8d57595
JCM_JCP_Participant = 5ebe963f - 943a - 42fe - 9389 - ca6429be6c55
JCM_Language = EN
JCM_PostedTimeUtc = 18 / 09 / 2020 1:24:13 AM
	JCM_Score = 0
InnerException Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'. --->System.Data.SqlClient.SqlException: The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.
   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)
   at System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean & dataReady)
   at System.Data.SqlClient.SqlCommand.RunExecuteNonQueryTds(String methodName, Boolean async, Int32 timeout, Boolean asyncWrite)
   at System.Data.SqlClient.SqlCommand.InternalExecuteNonQuery(TaskCompletionSource`1 completion, String methodName, Boolean sendToPipe, Int32 timeout, Boolean & usedCache, Boolean asyncWrite, Boolean inRetry)
   at System.Data.SqlClient.SqlCommand.ExecuteNonQuery()
   at CargoWise.Data.NonQueryCommandRunner.ExecuteCore(IDbCommand command)
   at CargoWise.Data.DbCommand.ExecuteCore(CommandRunner cmdRunner)
   at CargoWise.Data.DbCommand.Execute(CommandRunner cmdRunner)
   at CargoWise.Data.DbCommand.System.Data.IDbCommand.ExecuteNonQuery()
   at CargoWise.EntityFramework.ZSaveCommand.ExecuteStandardPart()
   -- - End of inner exception stack trace-- -
	at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)
   at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)
   at CargoWise.EntityFramework.ZSqlSaver.SaveRows(IList`1 rows)
   at CargoWise.EntityFramework.ZSaver.Save()
   at CargoWise.EntityFramework.ZAccessor.Save()
   at CargoWise.EntityFramework.RowFactory.CargoWise.Integration.ITransactionParticipant.SaveInTransaction()
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()
   -- - End of inner exception stack trace-- -
	at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()
   at System.Linq.Enumerable.WhereSelectArrayIterator`2.MoveNext()
   at System.Linq.Buffer`1..ctor(IEnumerable`1 source)
   at System.Linq.Enumerable.ToArray[TSource](IEnumerable`1 source)
   at CargoWise.EntityFramework.TransactionCoordinator.SaveInTransactions()
   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] factories)
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.SaveMailFactory(MailItem mailItem)
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.ProcessMailItem(MailItem mailItem)
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.CreateAndProcessMailItem(Email email)
   at Enterprise.Client.EDI.Mail.ServiceTasks.EmailProcessorServiceProvider.ProcessEmail(Email email)


CargoWise.EntityFramework.ZSaveException: 
**Error Saving Record **

Tablename: JobConversationMessage
PK: bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
	   RowState: Added
	   Factory validation suspended: False
	   Factory name for debugging: 
Business object around row = Enterprise.Client.EDI.IncidentManager.Business.EdiJobConversationMessage
	   Business object validation suspended: 0
	   Business object is marking as needing validation suspended: False
	   Business object light validation is enabled: False
	   Business object additional info: 


	   Inner Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.
	   

		--->CargoWise.EntityFramework.ZDataException: Error from Data layer: TableName = JobConversationMessage, PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7, RowState = Added
	   JCM_PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
	   JCM_Body = Thread - Topic: Update on Incident: CS00886826 - TLX Crashed - entries around
	   
		that time posted  $0
	   Thread - Index: AQHWjVRoTjIYJKmCmEmPaS4VPsfO46ltlBxw
	   Date: Fri, 18 Sep 2020 01:14:36 + 0000
	   Message - ID: < MEAPR01MB368840247114287023C70B05B13F0@MEAPR01MB3688.ausprd01.prod.outlook.com >
	   References: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
	   In - Reply - To: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
	   Accept - Language: en - US
	   Content - Language: en - US
	   X - MS - Has - Attach: yes
	   X - MS - TNEF - Correlator:
Authentication - Results - Original: wisetechglobal.com; dkim = none(message not
	   
		signed) header.d = none;
			wisetechglobal.com;
			dmarc = none action = none
 header.from = goldstartransport.com.au;
			x - originating - ip: [49.255.17.178]
x - ms - publictraffictype: Email
X - MS - Office365 - Filtering - Correlation - Id: ec8f0c81 - 3614 - 4da0 - afd9 - 08d85b70e898
x - ms - traffictypediagnostic: MEXPR01MB1975:| SY3PR01MB1020:
x - microsoft - antispam - prvs: < MEXPR01MB19752C3734A8CDA9522DCC93B13F0@MEXPR01MB1975.ausprd01.prod.outlook.com >
	   x - ms - oob - tlc - oobclassifiers: OLM:
			131;
OLM:
signed) header.d = none;
			wisetechglobal.com;
			dmarc = none action = none
 header.from = goldstartransport.com.au;
			x - originating - ip: [49.255.17.178]
x - ms - publictraffictype: Email
X - MS - Office365 - Filtering - Correlation - Id: ec8f0c81 - 3614 - 4da0 - afd9 - 08d85b70e898
x - ms - traffictypediagnostic: MEXPR01MB1975:| SY3PR01MB1020:
x - microsoft - antispam - prvs: < MEXPR01MB19752C3734A8CDA9522DCC93B13F0@MEXPR01MB1975.ausprd01.prod.outlook.com >
	   x - ms - oob - tlc - oobclassifiers: OLM:
			131;
OLM:
			131;
			x - ms - exchange - senderadcheck: 1
X - Microsoft - Antispam - Untrusted: BCL:
			0;
			X - Microsoft - Antispam - Message - Info - Original: N4pG1sGvd227gFebpC2hhHltHmvdC0yU1syBOIwo4WAV6Cd8B / N297kh2ma29ZBR8NFtMatslb1aszHocKi5yflWmZrrHKJpZWiH6i99QKp95 / 6YoRFMe0E + I1UAENQPXqYyTJBoT8kUMq0ZXKogyW6q1L5K9LpE8 + umdZq9T + piy12oysSgsHVWuX7R4pobKRIrVD0qvh5CHw5DGutHGFz2Q + SQM / yFZ862gwMzL4welV8WgaooxGfkSTB39iNmvElkjSWTI9sAWyis21hsyFrvsuMFuKdnJATfM598E63Ge9yXhNVsx1WlO0yCYCQPf7kHxJKaP54zAFCUb16YXQ ==
						  X - Forefront - Antispam - Report - Untrusted: CIP:
			255.255.255.255;
CTRY:
			;
LANG:
			en;
SCL:
			1;
SRV:
			;
IPV:
			NLI;
SFV:
			NSPM;
H:
			MEAPR01MB3688.ausprd01.prod.outlook.com;
PTR:
			;
CAT:
			NONE;
SFS:
			(396003)(39850400004)(366004)(136003)(346002)(376002)(9686003)(5660300002)(55016002)(15650500001)(83380400001)(33656002)(8936002)(316002)(99936003)(478600001)(53546011)(6506007)(86362001)(7696005)(52536014)(186003)(8676002)(6916009)(26005)(66446008)(64756008)(66556008)(66476007)(66576008)(2906002)(71200400001)(66946007)(76116006);
DIR:
			OUT;
SFP:
			1101;
			X - MS - Exchange - AntiSpam - MessageData - Original: rAo5ssL + Pgi4JSmm5PLPsrdk0UdvYXSJS5nk6fbhgaPlWnBmoDA2VcICQ8b8Yx7 + mR9ONa9UQjL88ZdRU02wvIwVOVzo9G3hcQQMGCsLp4iaKqJAwJFCgBthd0nstpLIymvLBT9AjJvy2yrINuwAfopjeGnaaOTixjKBObsnDy1f9NTdTwviT8q5iUrjXGQe9wNVCRyBL7g7 + B6QQBi + KbOC7uWt4uAT4ay0w3ZmaodNj0R8uzNVBdGrN5J6 + NELWoJ8yBLgRMBo5zSnIQFG9RYXohGIzWExzs7EE5qhRRRomMyjZC59RS1Bs3h7sm3F2Cqja9RQRc2HrkmdcbQQVK2Ay9t5STu69OEHu9SL9Z0BMT0wUsVi0tq0IMZCcoMAzdsqvQIvOpxppM7g8qUeiGmCXhRSEU + kht2xn5pw6H9C / 7nsnzMHyoFrhPFm8c / wgw75016aRVOKTsc0SJlggPyneo9Gxod6G14IPeeZvBPdKRwgWWQTYdxIIHpOSdVtm8e + 06EsizFChLZf + QWk4GUt5ZU0e6fW6sJCB0Pz8KXVPKTwgH / jS0GejQ21M7dOBY03Igya1NI7U1NnU0A + t0abIFIiFovShHesxH7eTVovekFuVGaXFHLZDHCxX9u / DMYqtq9lu8m0k05RoUOpFg ==
										x - ms - exchange - transport - forked: True
Content - Type: multipart / mixed;
			boundary = ""_008_MEAPR01MB368840247114287023C70B05B13F0MEAPR01MB3688ausp_""
X - MS - Exchange - Transport - CrossTenantHeadersStamped: MEXPR01MB1975
X - CLX - Shades: Deliver
X - CLX - Response: 1TFkXGxoaHxEKTHoXHhIRCllEF215H1tiRnMeYGEBEQpYWBdlZlxCeGFEe2l BchEKeE4XaVoYRkBzfWBMaH0RCnlMF2V4e28bRUsfZEhSEQpDSBcHGBwdEQpDWRcHGx8bEQpDSR caBBoaGhEKWU0Xbk9GQ1xPWBEKX1kXGBgbEQpfTRdnZnIRCllJFxpxGhAadwYYGhtxGBoYEBp3B
	 hgaBhoRClleF2hjeREKSUYXXUNZT15PSUJNRkVIS0Z1QkVZXk9OEQpDThduT0dZEmRdHV8aSHwc UG1AWVxoW0sYB0NMG0sSfB9yUBEKWFwXHwQaBBgbHgUbGgQbGhoEHhIEGxMQGx4aHxoRCl5ZF31 HSwVoEQpNXBceHRoRCkxaF2lrTWsRCkxGF29ra2tsa2sRCkJPF2l+SWkYGnxLG11PEQpDWhceGg
 QbGh0EGxkdBBIeEQpCXhcbEQpCXBcaEQpCRRdgQngZTklmWkJSUhEKQk4XaVoYRkBzfWBMaH0RC kJMF2VmXEJ4YUR7aUFyEQpCbBdtZUxfcHtQSEJsZREKQkAXZWFtckJTQRt7SXwRCkJYF2tsRm5P S09ORE5cEQpaWBcYEQp5QxdnQlNNWRlYQXhaQxEKcGcXbHx7ZWdtfVhGQ1sQGRoRCnBoF21BQ1l
 kAUQaXX19EBkaEQpwaBdjR1NickhwSEh9RRAZGhEKcGgXZBpyZUJ7elgYE24QGRoRCnBoF2xPR0 gaH0VLbBJZEBsbHhEKcGgXY2kFTk1kW1IeehMQGRoRCnBnF2hFElB5engeeFBvEBkaEQpwbBdhW k1cYBNHbxhYUBAZGhEKcEwXbENsGm1IfH9temcQGxgZEQptfhcaEQpYTRdLESA =
X - Proofpoint - Virus - Version: vendor = fsecure engine = 2.50.10434:6.0.235,18.0.687
 definitions = 2020 - 09 - 17_20:2020 - 09 - 16,2020 - 09 - 17 signatures = 0
X - Proofpoint - Spam - Details: rule = notspam policy = default score = 0 lowpriorityscore = 0 phishscore = 0
 mlxlogscore = 999 priorityscore = 48 bulkscore = 0 suspectscore = 0 spamscore = 0
 malwarescore = 0 adultscore = 0 clxscore = 1005 impostorscore = 0 mlxscore = 0
 classifier = clx:Deliver adjust = 0 reason = mlx scancount = 2
 engine = 8.12.0 - 2006250000 definitions = main - 2009180010
Return - Path: donna.strohfeldt @goldstartransport.com.au


   Exception


JCM_IsBroadcast = False
JCM_IsInternal = False
JCM_IsLocal = False
JCM_IsSystem = False
JCM_JCC_Conversation = 09315bc9 - 8c7b - 48cc - 9af5 - b1bca8d57595
JCM_JCP_Participant = 5ebe963f - 943a - 42fe - 9389 - ca6429be6c55
JCM_Language = EN
JCM_PostedTimeUtc = 18 / 09 / 2020 1:24:13 AM
	JCM_Score = 0
InnerException Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'. --->System.Data.SqlClient.SqlException: The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.
   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)
   at System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean & dataReady)
   at System.Data.SqlClient.SqlCommand.RunExecuteNonQueryTds(String methodName, Boolean async, Int32 timeout, Boolean asyncWrite)
   at System.Data.SqlClient.SqlCommand.InternalExecuteNonQuery(TaskCompletionSource`1 completion, String methodName, Boolean sendToPipe, Int32 timeout, Boolean & usedCache, Boolean asyncWrite, Boolean inRetry)
   at System.Data.SqlClient.SqlCommand.ExecuteNonQuery()
   at CargoWise.Data.NonQueryCommandRunner.ExecuteCore(IDbCommand command)
   at CargoWise.Data.DbCommand.ExecuteCore(CommandRunner cmdRunner)
   at CargoWise.Data.DbCommand.Execute(CommandRunner cmdRunner)
   at CargoWise.Data.DbCommand.System.Data.IDbCommand.ExecuteNonQuery()
   at CargoWise.EntityFramework.ZSaveCommand.ExecuteStandardPart()
   -- - End of inner exception stack trace-- -
	at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)
   at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)
   at CargoWise.EntityFramework.ZSqlSaver.SaveRows(IList`1 rows)
   at CargoWise.EntityFramework.ZSaver.Save()
   at CargoWise.EntityFramework.ZAccessor.Save()
   at CargoWise.EntityFramework.RowFactory.CargoWise.Integration.ITransactionParticipant.SaveInTransaction()
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()
   -- - End of inner exception stack trace-- -
	at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()
   at System.Linq.Enumerable.WhereSelectArrayIterator`2.MoveNext()
   at System.Linq.Buffer`1..ctor(IEnumerable`1 source)
   at System.Linq.Enumerable.ToArray[TSource](IEnumerable`1 source)
   at CargoWise.EntityFramework.TransactionCoordinator.SaveInTransactions()
   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] factories)
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.SaveMailFactory(MailItem mailItem)
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.ProcessMailItem(MailItem mailItem)
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.CreateAndProcessMailItem(Email email)
   at Enterprise.Client.EDI.Mail.ServiceTasks.EmailProcessorServiceProvider.ProcessEmail(Email email)


CargoWise.EntityFramework.ZSaveException: 
**Error Saving Record **

Tablename: JobConversationMessage
PK: bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
	   RowState: Added
	   Factory validation suspended: False
	   Factory name for debugging: 
Business object around row = Enterprise.Client.EDI.IncidentManager.Business.EdiJobConversationMessage
	   Business object validation suspended: 0
	   Business object is marking as needing validation suspended: False
	   Business object light validation is enabled: False
	   Business object additional info: 


	   Inner Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.
	   

		--->CargoWise.EntityFramework.ZDataException: Error from Data layer: TableName = JobConversationMessage, PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7, RowState = Added
	   JCM_PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
	   JCM_Body = Thread - Topic: Update on Incident: CS00886826 - TLX Crashed - entries around
	   
		that time posted  $0
	   Thread - Index: AQHWjVRoTjIYJKmCmEmPaS4VPsfO46ltlBxw
	   Date: Fri, 18 Sep 2020 01:14:36 + 0000
	   Message - ID: < MEAPR01MB368840247114287023C70B05B13F0@MEAPR01MB3688.ausprd01.prod.outlook.com >
	   References: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
	   In - Reply - To: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
	   Accept - Language: en - US
	   Content - Language: en - US
	   X - MS - Has - Attach: yes
	   X - MS - TNEF - Correlator:
Authentication - Results - Original: wisetechglobal.com; dkim = none(message not
	   
		signed) header.d = none;
			wisetechglobal.com;
			dmarc = none action = none
 header.from = goldstartransport.com.au;
			x - originating - ip: [49.255.17.178]
x - ms - publictraffictype: Email
X - MS - Office365 - Filtering - Correlation - Id: ec8f0c81 - 3614 - 4da0 - afd9 - 08d85b70e898
x - ms - traffictypediagnostic: MEXPR01MB1975:| SY3PR01MB1020:
x - microsoft - antispam - prvs: < MEXPR01MB19752C3734A8CDA9522DCC93B13F0@MEXPR01MB1975.ausprd01.prod.outlook.com >
	   x - ms - oob - tlc - oobclassifiers: OLM:
			131;
OLM:
signed) header.d = none;
			wisetechglobal.com;
			dmarc = none action = none
 header.from = goldstartransport.com.au;
			x - originating - ip: [49.255.17.178]
x - ms - publictraffictype: Email
X - MS - Office365 - Filtering - Correlation - Id: ec8f0c81 - 3614 - 4da0 - afd9 - 08d85b70e898
x - ms - traffictypediagnostic: MEXPR01MB1975:| SY3PR01MB1020:
x - microsoft - antispam - prvs: < MEXPR01MB19752C3734A8CDA9522DCC93B13F0@MEXPR01MB1975.ausprd01.prod.outlook.com >
	   x - ms - oob - tlc - oobclassifiers: OLM:
			131;
OLM:
			131;
			x - ms - exchange - senderadcheck: 1
X - Microsoft - Antispam - Untrusted: BCL:
			0;
			X - Microsoft - Antispam - Message - Info - Original: N4pG1sGvd227gFebpC2hhHltHmvdC0yU1syBOIwo4WAV6Cd8B / N297kh2ma29ZBR8NFtMatslb1aszHocKi5yflWmZrrHKJpZWiH6i99QKp95 / 6YoRFMe0E + I1UAENQPXqYyTJBoT8kUMq0ZXKogyW6q1L5K9LpE8 + umdZq9T + piy12oysSgsHVWuX7R4pobKRIrVD0qvh5CHw5DGutHGFz2Q + SQM / yFZ862gwMzL4welV8WgaooxGfkSTB39iNmvElkjSWTI9sAWyis21hsyFrvsuMFuKdnJATfM598E63Ge9yXhNVsx1WlO0yCYCQPf7kHxJKaP54zAFCUb16YXQ ==
						  X - Forefront - Antispam - Report - Untrusted: CIP:
			255.255.255.255;
CTRY:
			;
LANG:
			en;
SCL:
			1;
SRV:
			;
IPV:
			NLI;
SFV:
			NSPM;
H:
			MEAPR01MB3688.ausprd01.prod.outlook.com;
PTR:
			;
CAT:
			NONE;
SFS:
			(396003)(39850400004)(366004)(136003)(346002)(376002)(9686003)(5660300002)(55016002)(15650500001)(83380400001)(33656002)(8936002)(316002)(99936003)(478600001)(53546011)(6506007)(86362001)(7696005)(52536014)(186003)(8676002)(6916009)(26005)(66446008)(64756008)(66556008)(66476007)(66576008)(2906002)(71200400001)(66946007)(76116006);
DIR:
			OUT;
SFP:
			1101;
			X - MS - Exchange - AntiSpam - MessageData - Original: rAo5ssL + Pgi4JSmm5PLPsrdk0UdvYXSJS5nk6fbhgaPlWnBmoDA2VcICQ8b8Yx7 + mR9ONa9UQjL88ZdRU02wvIwVOVzo9G3hcQQMGCsLp4iaKqJAwJFCgBthd0nstpLIymvLBT9AjJvy2yrINuwAfopjeGnaaOTixjKBObsnDy1f9NTdTwviT8q5iUrjXGQe9wNVCRyBL7g7 + B6QQBi + KbOC7uWt4uAT4ay0w3ZmaodNj0R8uzNVBdGrN5J6 + NELWoJ8yBLgRMBo5zSnIQFG9RYXohGIzWExzs7EE5qhRRRomMyjZC59RS1Bs3h7sm3F2Cqja9RQRc2HrkmdcbQQVK2Ay9t5STu69OEHu9SL9Z0BMT0wUsVi0tq0IMZCcoMAzdsqvQIvOpxppM7g8qUeiGmCXhRSEU + kht2xn5pw6H9C / 7nsnzMHyoFrhPFm8c / wgw75016aRVOKTsc0SJlggPyneo9Gxod6G14IPeeZvBPdKRwgWWQTYdxIIHpOSdVtm8e + 06EsizFChLZf + QWk4GUt5ZU0e6fW6sJCB0Pz8KXVPKTwgH / jS0GejQ21M7dOBY03Igya1NI7U1NnU0A + t0abIFIiFovShHesxH7eTVovekFuVGaXFHLZDHCxX9u / DMYqtq9lu8m0k05RoUOpFg ==
										x - ms - exchange - transport - forked: True
Content - Type: multipart / mixed;
			boundary = ""_008_MEAPR01MB368840247114287023C70B05B13F0MEAPR01MB3688ausp_""
X - MS - Exchange - Transport - CrossTenantHeadersStamped: MEXPR01MB1975
X - CLX - Shades: Deliver
X - CLX - Response: 1TFkXGxoaHxEKTHoXHhIRCllEF215H1tiRnMeYGEBEQpYWBdlZlxCeGFEe2l BchEKeE4XaVoYRkBzfWBMaH0RCnlMF2V4e28bRUsfZEhSEQpDSBcHGBwdEQpDWRcHGx8bEQpDSR caBBoaGhEKWU0Xbk9GQ1xPWBEKX1kXGBgbEQpfTRdnZnIRCllJFxpxGhAadwYYGhtxGBoYEBp3B
	 hgaBhoRClleF2hjeREKSUYXXUNZT15PSUJNRkVIS0Z1QkVZXk9OEQpDThduT0dZEmRdHV8aSHwc UG1AWVxoW0sYB0NMG0sSfB9yUBEKWFwXHwQaBBgbHgUbGgQbGhoEHhIEGxMQGx4aHxoRCl5ZF31 HSwVoEQpNXBceHRoRCkxaF2lrTWsRCkxGF29ra2tsa2sRCkJPF2l+SWkYGnxLG11PEQpDWhceGg
 QbGh0EGxkdBBIeEQpCXhcbEQpCXBcaEQpCRRdgQngZTklmWkJSUhEKQk4XaVoYRkBzfWBMaH0RC kJMF2VmXEJ4YUR7aUFyEQpCbBdtZUxfcHtQSEJsZREKQkAXZWFtckJTQRt7SXwRCkJYF2tsRm5P S09ORE5cEQpaWBcYEQp5QxdnQlNNWRlYQXhaQxEKcGcXbHx7ZWdtfVhGQ1sQGRoRCnBoF21BQ1l
 kAUQaXX19EBkaEQpwaBdjR1NickhwSEh9RRAZGhEKcGgXZBpyZUJ7elgYE24QGRoRCnBoF2xPR0 gaH0VLbBJZEBsbHhEKcGgXY2kFTk1kW1IeehMQGRoRCnBnF2hFElB5engeeFBvEBkaEQpwbBdhW k1cYBNHbxhYUBAZGhEKcEwXbENsGm1IfH9temcQGxgZEQptfhcaEQpYTRdLESA =
X - Proofpoint - Virus - Version: vendor = fsecure engine = 2.50.10434:6.0.235,18.0.687
 definitions = 2020 - 09 - 17_20:2020 - 09 - 16,2020 - 09 - 17 signatures = 0
X - Proofpoint - Spam - Details: rule = notspam policy = default score = 0 lowpriorityscore = 0 phishscore = 0
 mlxlogscore = 999 priorityscore = 48 bulkscore = 0 suspectscore = 0 spamscore = 0
 malwarescore = 0 adultscore = 0 clxscore = 1005 impostorscore = 0 mlxscore = 0
 classifier = clx:Deliver adjust = 0 reason = mlx scancount = 2
 engine = 8.12.0 - 2006250000 definitions = main - 2009180010
Return - Path: donna.strohfeldt @goldstartransport.com.au


   Exception


JCM_IsBroadcast = False
JCM_IsInternal = False
JCM_IsLocal = False
JCM_IsSystem = False
JCM_JCC_Conversation = 09315bc9 - 8c7b - 48cc - 9af5 - b1bca8d57595
JCM_JCP_Participant = 5ebe963f - 943a - 42fe - 9389 - ca6429be6c55
JCM_Language = EN
JCM_PostedTimeUtc = 18 / 09 / 2020 1:24:13 AM
	JCM_Score = 0
InnerException Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'. --->System.Data.SqlClient.SqlException: The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.
   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)
   at System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean & dataReady)
   at System.Data.SqlClient.SqlCommand.RunExecuteNonQueryTds(String methodName, Boolean async, Int32 timeout, Boolean asyncWrite)
   at System.Data.SqlClient.SqlCommand.InternalExecuteNonQuery(TaskCompletionSource`1 completion, String methodName, Boolean sendToPipe, Int32 timeout, Boolean & usedCache, Boolean asyncWrite, Boolean inRetry)
   at System.Data.SqlClient.SqlCommand.ExecuteNonQuery()
   at CargoWise.Data.NonQueryCommandRunner.ExecuteCore(IDbCommand command)
   at CargoWise.Data.DbCommand.ExecuteCore(CommandRunner cmdRunner)
   at CargoWise.Data.DbCommand.Execute(CommandRunner cmdRunner)
   at CargoWise.Data.DbCommand.System.Data.IDbCommand.ExecuteNonQuery()
   at CargoWise.EntityFramework.ZSaveCommand.ExecuteStandardPart()
   -- - End of inner exception stack trace-- -
	at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)
   at CargoWise.EntityFramework.ZSqlSaver.SaveSplitRows(IList`1 rows, Int32 startingIndex, Boolean canConsolidateInserts, Boolean isBulkUpdate)
   at CargoWise.EntityFramework.ZSqlSaver.SaveRows(IList`1 rows)
   at CargoWise.EntityFramework.ZSaver.Save()
   at CargoWise.EntityFramework.ZAccessor.Save()
   at CargoWise.EntityFramework.RowFactory.CargoWise.Integration.ITransactionParticipant.SaveInTransaction()
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()
   -- - End of inner exception stack trace-- -
	at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()
   at System.Linq.Enumerable.WhereSelectArrayIterator`2.MoveNext()
   at System.Linq.Buffer`1..ctor(IEnumerable`1 source)
   at System.Linq.Enumerable.ToArray[TSource](IEnumerable`1 source)
   at CargoWise.EntityFramework.TransactionCoordinator.SaveInTransactions()
   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] factories)
   at CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.SaveMailFactory(MailItem mailItem)
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.ProcessMailItem(MailItem mailItem)
   at Enterprise.Client.EDI.Mail.Business.BusinessObjectEmailProcessor`1.CreateAndProcessMailItem(Email email)
   at Enterprise.Client.EDI.Mail.ServiceTasks.EmailProcessorServiceProvider.ProcessEmail(Email email)


CargoWise.EntityFramework.ZSaveException: 
**Error Saving Record **

Tablename: JobConversationMessage
PK: bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
	   RowState: Added
	   Factory validation suspended: False
	   Factory name for debugging: 
Business object around row = Enterprise.Client.EDI.IncidentManager.Business.EdiJobConversationMessage
	   Business object validation suspended: 0
	   Business object is marking as needing validation suspended: False
	   Business object light validation is enabled: False
	   Business object additional info: 


	   Inner Message = The INSERT statement conflicted with the CHECK constraint ""Constraint_JCM_Body"".The conflict occurred in database ""ediprod"", table ""dbo.JobConversationMessage"", column 'JCM_Body'.
	   

		--->CargoWise.EntityFramework.ZDataException: Error from Data layer: TableName = JobConversationMessage, PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7, RowState = Added
	   JCM_PK = bbb3a2a8 - c548 - 4057 - 9c4b - 252777776db7
	   JCM_Body = Thread - Topic: Update on Incident: CS00886826 - TLX Crashed - entries around
	   
		that time posted  $0
	   Thread - Index: AQHWjVRoTjIYJKmCmEmPaS4VPsfO46ltlBxw
	   Date: Fri, 18 Sep 2020 01:14:36 + 0000
	   Message - ID: < MEAPR01MB368840247114287023C70B05B13F0@MEAPR01MB3688.ausprd01.prod.outlook.com >
	   References: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
	   In - Reply - To: < 7a08c043 - 5abe - 4f09 - b10f - a003a0559915@mail.dll >
	   Accept - Language: en - US
	   Content - Language: en - US
	   X - MS - Has - Attach: yes
	   X - MS - TNEF - Correlator:
Authentication - Results - Original: wisetechglobal.com; dkim = none(message not
	   
		signed) header.d = none;
			wisetechglobal.com;
			dmarc = none action = none
 header.from = goldstartransport.com.au;
			x - originating - ip: [49.255.17.178]
x - ms - publictraffictype: Email
X - MS - Office365 - Filtering - Correlation - Id: ec8f0c81 - 3614 - 4da0 - afd9 - 08d85b70e898
x - ms - traffictypediagnostic: MEXPR01MB1975:| SY3PR01MB1020:
x - microsoft - antispam - prvs: < MEXPR01MB19752C3734A8CDA9522DCC93B13F0@MEXPR01MB1975.ausprd01.prod.outlook.com >
	   x - ms - oob - tlc - oobclassifiers: OLM:
			131;
OLM:
			131;
			x - ms - exchange - senderadcheck: 1
X - Microsoft - Antispam - Untrusted: BCL:
			0;
			X - Microsoft - Antispam - Message - Info - Original: N4pG1sGvd227gFebpC2hhHltHmvdC0yU1syBOIwo4WAV6Cd8B / N297kh2ma29ZBR8NFtMatslb1aszHocKi5yflWmZrrHKJpZWiH6i99QKp95 / 6YoRFMe0E + I1UAENQPXqYyTJBoT8kUMq0ZXKogyW6q1L5K9LpE8 + umdZq9T + piy12oysSgsHVWuX7R4pobKRIrVD0qvh5CHw5DGutHGFz2Q + SQM / yFZ862gwMzL4welV8WgaooxGfkSTB39iNmvElkjSWTI9sAWyis21hsyFrvsuMFuKdnJATfM598E63Ge9yXhNVsx1WlO0yCYCQPf7kHxJKaP54zAFCUb16YXQ ==
						  X - Forefront - Antispam - Report - Untrusted: CIP:
			255.255.255.255;
CTRY:
			;
LANG:
			en;
SCL:
			1;
SRV:
			;
IPV:
			NLI;
SFV:
			NSPM;
H:
			MEAPR01MB3688.ausprd01.prod.outlook.com;
PTR:
			;
CAT:
			NONE;
SFS:
			(396003)(39850400004)(366004)(136003)(346002)(376002)(9686003)(5660300002)(55016002)(15650500001)(83380400001)(33656002)(8936002)(316002)(99936003)(478600001)(53546011)(6506007)(86362001)(7696005)(52536014)(186003)(8676002)(6916009)(26005)(66446008)(64756008)(66556008)(66476007)(66576008)(2906002)(71200400001)(66946007)(76116006);
DIR:
			OUT;
SFP:
			1101;
			X - MS - Exchange - AntiSpam - MessageData - Original: rAo5ssL + Pgi4JSmm5PLPsrdk0UdvYXSJS5nk6fbhgaPlWnBmoDA2VcICQ8b8Yx7 + mR9ONa9UQjL88ZdRU02wvIwVOVzo9G3hcQQMGCsLp4iaKqJAwJFCgBthd0nstpLIymvLBT9AjJvy2yrINuwAfopjeGnaaOTixjKBObsnDy1f9NTdTwviT8q5iUrjXGQe9wNVCRyBL7g7 + B6QQBi + KbOC7uWt4uAT4ay0w3ZmaodNj0R8uzNVBdGrN5J6 + NELWoJ8yBLgRMBo5zSnIQFG9RYXohGIzWExzs7EE5qhRRRomMyjZC59RS1Bs3h7sm3F2Cqja9RQRc2HrkmdcbQQVK2Ay9t5STu69OEHu9SL9Z0BMT0wUsVi0tq0IMZCcoMAzdsqvQIvOpxppM7g8qUeiGmCXhRSEU + kht2xn5pw6H9C / 7nsnzMHyoFrhPFm8c / wgw75016aRVOKTsc0SJlggPyneo9Gxod6G14IPeeZvBPdKRwgWWQTYdxIIHpOSdVtm8e + 06EsizFChLZf + QWk4GUt5ZU0e6fW6sJCB0Pz8KXVPKTwgH / jS0GejQ21M7dOBY03Igya1NI7U1NnU0A + t0abIFIiFovShHesxH7eTVovekFuVGaXFHLZDHCxX9u / DMYqtq9lu8m0k05RoUOpFg ==
										x - ms - exchange - transport - forked: True
Content - Type: multipart / mixed;
			boundary = ""_008_MEAPR01MB368840247114287023C70B05B13F0MEAPR01MB3688ausp_""
X - MS - Exchange - Transport - CrossTenantHeadersStamped: MEXPR01MB1975
X - CLX - Shades: Deliver
X - CLX - Response: 1TFkXGxoaHxEKTHoXHhIRCllEF215H1tiRnMeYGEBEQpYWBdlZlxCeGFEe2l BchEKeE4XaVoYRkBzfWBMaH0RCnlMF2V4e28bRUsfZEhSEQpDSBcHGBwdEQpDWRcHGx8bEQpDSR caBBoaGhEKWU0Xbk9GQ1xPWBEKX1kXGBgbEQpfTRdnZnIRCllJFxpxGhAadwYYGhtxGBoYEBp3B
	 hgaBhoRClleF2hjeREKSUYXXUNZT15PSUJNRkVIS0Z1QkVZXk9OEQpDThduT0dZEmRdHV8aSHwc UG1AWVxoW0sYB0NMG0sSfB9yUBEKWFwXHwQaBBgbHgUbGgQbGhoEHhIEGxMQGx4aHxoRCl5ZF31 HSwVoEQpNXBceHRoRCkxaF2lrTWsRCkxGF29ra2tsa2sRCkJPF2l+SWkYGnxLG11PEQpDWhceGg
 QbGh0EGxkdBBIeEQpCXhcbEQpCXBcaEQpCRRdgQngZTklmWkJSUhEKQk4XaVoYRkBzfWBMaH0RC kJMF2VmXEJ4YUR7aUFyEQpCbBdtZUxfcHtQSEJsZREKQkAXZWFtckJTQRt7SXwRCkJYF2tsRm5P S09ORE5cEQpaWBcYEQp5QxdnQlNNWRlYQXhaQxEKcGcXbHx7ZWdtfVhGQ1sQGRoRCnBoF21BQ1l
 kAUQaXX19EBkaEQpwaBdjR1NickhwSEh9RRAZGhEKcGgXZBpyZUJ7elgYE24QGRoRCnBoF2xPR0 gaH0VLbBJZEBsbHhEKcGgXY2kFTk1kW1IeehMQGRoRCnBnF2hFElB5engeeFBvEBkaEQpwbBdhW k1cYBNHbxhYUBAZGhEKcEwXbENsGm1IfH9temcQGxgZEQptfhcaEQpYTRdLESA =
X - Proofpoint - Virus - Version: vendor = fsecure engine = 2.50.10434:6.0.235,18.0.687
 definitions = 2020 - 09 - 17_20:2020 - 09 - 16,2020 - 09 - 17 signatures = 0
X - Proofpoint - Spam - Details: rule = notspam policy = default score = 0 lowpriorityscore = 0 phishscore = 0
 mlxlogscore = 999 priorityscore = 48 bulkscore = 0 suspectscore = 0 spamscore = 0
 malwarescore = 0 adultscore = 0 clxscore = 1005 impostorscore = 0 mlxscore = 0
 classifier = clx:Deliver adjust = 0 reason = mlx scancount = 2
 engine = 8.12.0 - 2006250000 definitions = main - 2009180010
Return - Path: donna.strohfeldt @goldstartransport.com.au

";

		#endregion
	}
}
