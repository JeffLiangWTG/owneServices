using System;
using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosFileExporterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", NotificationBuffer, Exporter.Notifications);
			AssertEquals("Should be assigned in the constructor", new ZDateTime(2006, 5, 5), Exporter.ExportStartDateTime);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "ExportFilePath has to be specified")]
		public void TestExportToFile_EmptyExportFilePath()
		{
			Exporter.ExportToFile(null);
		}

		public void TestExportToFile_IOException()
		{
			Exporter.IOExceptionToThrowDuringExport = new IOException("MEH MEH");
			Assert("Should return false when IOException thrown", !Exporter.ExportToFile("blah"));
			AssertEquals("The user should be notified when IOException thrown", 1, NotificationBuffer.Events.Length);
			AssertEquals("The user should be notified when IOException thrown", ErrorType.IOError, ((ErrorNotification)NotificationBuffer.Events[0]).ErrorType);
			AssertEquals("The user should be notified when IOException thrown", "MEH MEH", ((INotificationSubscriberNotification)NotificationBuffer.Events[0]).AdditionalInfo);
		}

		public void TestExportToFile()
		{
			string testFile = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			try
			{
				Exporter.ExportToFile(testFile);
				AssertFileContent(testFile);
				AssertNotificationsAndMethodsExecutionOrder();
			}
			finally
			{
				File.Delete(testFile);
			}
		}

		public void TestExportToFile_AccountingDataIsEmpty()
		{
			string testFile = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			try
			{
				Exporter.ShouldExportAccountingData = false;
				Exporter.ExportToFile(testFile);
				AssertFileContent_AccountingDataIsEmpty(testFile);
			}
			finally
			{
				File.Delete(testFile);
			}
		}

		public void TestGetNewCognosAccountsAggregator()
		{
			AssertEquals(typeof(CognosAccountsAggregator), Exporter.GetNewCognosAccountsAggregator_Original().GetType());
		}

		public void TestGetNewNonAccountingCognosLineGenerator()
		{
			AssertEquals(typeof(NonAccountingCognosLineGenerator), Exporter.GetNewNonAccountingCognosLineGenerator_Original().GetType());
		}

		#region Implementation
		void AssertFileContent(string testFile)
		{
			using (StreamReader reader = new StreamReader(testFile))
			{
				string generatedContent = reader.ReadToEnd();
				string expectedContent = @"
Account 101,101,USCOR,AI,BNE,COM,10.00+,AUD,15.00+,SEA
Account 101,101,AUSYD,AE,BNE,COM,20.00+,AUD,25.00+,AFR
Account 101,101REC,,,,,30.00+,,,
Account 102,102,ITMIL,ME,SYD,TEL,500.00+,SGD,1005.00+,NAM
Account 103,103,SGSIN,CHB,MEL,OTH,100.00+,HKD,980.00+,MEA
Account 103,103,AUBNE,MI,BNE,COM,50.00+,AUD,55.00+,SSS
Account 103,103REC,,,,,150.00+,,,
FROM NONACCOUNTING COGNOS LINE".TrimStart();
				AssertMultilineEquals("Should be as expected", expectedContent, generatedContent, '\n');
			}
		}

		void AssertFileContent_AccountingDataIsEmpty(string testFile)
		{
			string generatedContent = File.ReadAllText(testFile);
			string expectedContent = "FROM NONACCOUNTING COGNOS LINE";
			AssertEquals(expectedContent, generatedContent);
		}

		void AssertNotificationsAndMethodsExecutionOrder()
		{
			AssertEquals("There should be 2 InfoNotifications notified", 2, NotificationBuffer.Events.Length);
			AssertInfoNotification(0, "Writing Accounting Data to CSV file", 0);
			AssertInfoNotification(1, "Writing Statistical Data to CSV file", 3);
			AssertEquals("CurrentProgress should be 13%", 13, NotificationBuffer.CurrentProgress);
		}

		void AssertInfoNotification(int eventIndex, string message, int expectedCurrentProgressPercentage)
		{
			INotificationSubscriberNotification expectedNotification = (INotificationSubscriberNotification)NotificationBuffer.Events[eventIndex];
			AssertEquals("Notification should be an InfoNotification", typeof(InfoNotification), expectedNotification.GetType());
			AssertEquals("AdditionalInfo not as expected", message, expectedNotification.AdditionalInfo);
			AssertEquals("CurrentProgressPercentage not as expected", expectedCurrentProgressPercentage, NotificationBuffer.GetProgressPercentageAtEventIndex(eventIndex));
		}

		CognosFileExporterForTest Exporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new CognosFileExporterForTest(new ZDateTime(2006, 5, 5), NotificationBuffer);
				}

				return fExporter;
			}
		}

		CognosNotificationBufferForTest NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new CognosNotificationBufferForTest();
				}

				return fNotificationBuffer;
			}
		}

		CognosFileExporterForTest fExporter;
		CognosNotificationBufferForTest fNotificationBuffer;
		#endregion
		#region class CognosFileExporterForTest
		class CognosFileExporterForTest : CognosFileExporter
		{
			public CognosFileExporterForTest(ZDateTime exportStartDateTime, ICognosNotificationSubscriber notifications) : base(exportStartDateTime, notifications)
			{
			}

			public CognosAccountsAggregator GetNewCognosAccountsAggregator_Original()
			{
				return base.GetNewCognosAccountsAggregator();
			}

			public NonAccountingCognosLineGenerator GetNewNonAccountingCognosLineGenerator_Original()
			{
				return base.GetNewNonAccountingCognosLineGenerator();
			}

			protected override CognosAccountsAggregator GetNewCognosAccountsAggregator()
			{
				return new CognosAccountsAggregatorForTest(ShouldExportAccountingData, ExportStartDateTime, Notifications);
			}

			protected override NonAccountingCognosLineGenerator GetNewNonAccountingCognosLineGenerator()
			{
				return new NonAccountingCognosLineGeneratorForTest(new BusinessObjectFactory(), ExportStartDateTime);
			}

			protected override void ExportToFileCore(ZString exportFilePath)
			{
				if (IOExceptionToThrowDuringExport != null)
				{
					throw IOExceptionToThrowDuringExport;
				}

				base.ExportToFileCore(exportFilePath);
			}

			public IOException IOExceptionToThrowDuringExport;
			public bool ShouldExportAccountingData = true;
		}

		#endregion
		#region class CognosAccountsAggregatorForTest
		class CognosAccountsAggregatorForTest : CognosAccountsAggregator
		{
			public CognosAccountsAggregatorForTest(bool shouldExportAccountingData, ZDateTime exportStartDateTime, ICognosNotificationSubscriber notifications) : base(exportStartDateTime, notifications)
			{
				this.ShouldExportAccountingData = shouldExportAccountingData;
			}

			public override void AggregateToExportTempTable()
			{
				if (ShouldExportAccountingData)
				{
					InsertDummyAccountDescriptorRecordsIntoCognosExportTempTable();
				}
			}

			void InsertDummyAccountDescriptorRecordsIntoCognosExportTempTable()
			{
				CognosAccGLAccountDescriptor account101 = CreateNewAccGLAccountDescriptor("101", "Account 101");
				account101.ExtraInfo.T9_ReconciliationTotalAccount = "101REC";
				CognosAccGLAccountDescriptor account102 = CreateNewAccGLAccountDescriptor("102", "Account 102");
				CognosAccGLAccountDescriptor account103 = CreateNewAccGLAccountDescriptor("103", "Account 103");
				account103.ExtraInfo.T9_ReconciliationTotalAccount = "103REC";
				Factory.Save();
				string commandText = ConstructInsertScript(account101, "USCOR", "AI", "BNE", "COM", 10m, "AUD", 15m, "SEA");
				commandText += ConstructInsertScript(account101, "AUSYD", "AE", "BNE", "COM", 20m, "AUD", 25m, "AFR");
				commandText += ConstructInsertScript(account102, "ITMIL", "ME", "SYD", "TEL", 500m, "SGD", 1005m, "NAM");
				commandText += ConstructInsertScript(account103, "SGSIN", "CHB", "MEL", "OTH", 100m, "HKD", 980m, "MEA");
				commandText += ConstructInsertScript(account103, "AUBNE", "MI", "BNE", "COM", 50m, "AUD", 55m, "SSS");
				Db.Connection.ExecuteNonQuery(commandText);
			}

			string ConstructInsertScript(AccGLAccountDescriptor account, ZString companyCode, ZString mode, ZString branch, ZString businessType, ZDecimal amount, ZString transactionCurrency, ZDecimal transactionAmount, ZString geographical)
			{
				return string.Format(@"
INSERT INTO #CognosExport VALUES('{0}', '{1}', '{2}', '{3}', '{4}', {5}, '{6}', {7}, '{8}')", account.PK, companyCode, mode, branch, businessType, amount, transactionCurrency, transactionAmount, geographical);
			}

			CognosAccGLAccountDescriptor CreateNewAccGLAccountDescriptor(ZString localAccountNumber, ZString description)
			{
				CognosAccGLAccountDescriptor account = Factory.NewWithValidTestData<CognosAccGLAccountDescriptor>();
				account.AJ_LocalAccountNumber = localAccountNumber;
				account.AJ_AccountDescription = description;
				return account;
			}

			readonly bool ShouldExportAccountingData;
			readonly BusinessObjectFactory Factory = new BusinessObjectFactory();
		}

		#endregion
		#region class NonAccountingCognosLineGeneratorForTest
		class NonAccountingCognosLineGeneratorForTest : NonAccountingCognosLineGenerator
		{
			public NonAccountingCognosLineGeneratorForTest(BusinessObjectFactory factory, ZDateTime exportStartDateTime) : base(factory, exportStartDateTime)
			{
			}

			public override string GetLinesAsString()
			{
				return "FROM NONACCOUNTING COGNOS LINE";
			}
		}
		#endregion
	}
}
