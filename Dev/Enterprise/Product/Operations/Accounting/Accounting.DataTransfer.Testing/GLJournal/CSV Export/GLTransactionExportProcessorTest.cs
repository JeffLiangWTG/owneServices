using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	public class GLTransactionExportProcessorTest : TestCaseWithFactory
	{
		public void TestProcessIncludesCallStackInException()
		{
			PrepareTestData();

			var processor = new GLTransactionExportProcessorForExceptionTest();
			var notify = new NotificationBuffer();

			processor.Process(notify);
			var errors = notify.GetEventsByType(ErrorType.Error);
			AssertNotNull("Exception should include call stack", errors.SingleOrDefault(e => e.Message.StartsWith(@"Error: Error Exporting GL Transactions of Company EDI : Some exception message
   at Enterprise.Accounting.DataTransfer.GLJournals.Testing.GLTransactionExportProcessorTest.GLTransactionExportProcessorForExceptionTest.GetNewGLTransactionBusinessObject()")));
		}

		class GLTransactionExportProcessorForExceptionTest : GLTransactionExportProcessor
		{
			protected override GLTransactionBusinessObject GetNewGLTransactionBusinessObject()
			{
				throw new InvalidOperationException("Some exception message");
			}
		}

		[TestDate(2009, 1, 23, 15, 30, 40)]
		public void TestProcess()
		{
			PrepareTestData();

			GLTransactionExportProcessor processor = GetNewExportProcessor();
			NotificationBuffer notify = new NotificationBuffer();

			AssertEquals("no of email sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			processor.Process(notify);

			ZString expectedFileForCompanyNZ = Path.Combine(NZTestPath, "GLTransactions20090123153040 CNZ batch 1000.csv");
			ZString expectedFileForCompanyAU = Path.Combine(AUTestPath, "GLTransactions20090123153040 " + Env.CurrentCompany.Code + " batch 1000.csv");
			ZString expectedFileForCompanyKR = Path.Combine(KRTestPath, "GLTransactions20090123153040 CKR batch 1000.csv");

			AssertEquals("notification has errors", true, notify.HasErrors);

			AssertContains("Export GL Transactions Process of Company " + Env.CurrentCompany.Code + " Started.", notify.AsString);
			AssertContains("Data exported successfully to the file '" + expectedFileForCompanyAU, notify.AsString);
			AssertContains("Export GL Transactions Process of Company " + Env.CurrentCompany.Code + " Finished.", notify.AsString);

			AssertContains("Export GL Transactions to CSV registry items of Company CUS are not set or invalid. Please verify the values in Admin->Registry->System->Data Export Settings->Export GL Transactions to CSV", notify.AsString);

			AssertEquals("no of email sent", 4, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertEmailWithSubject("GL Transactions of Company CNZ Export Failure");
			AssertEmailWithSubject("GL Transactions of Company " + Env.CurrentCompany.Code + " Export Successfully - Batch Number: 1000");
			AssertEmailWithSubject("GL Transactions of Company CUS Export Failure");
			AssertEmailWithSubject("GL Transactions of Company CKR Export Failure");

			AssertRecipientWithEmail(dummyEmailGroup.Staff[0].GS_EmailAddress);
			AssertRecipientWithEmail(dummyEmailGroup2.Staff[0].GS_EmailAddress);

			if (processor.GetNewGLTransactionExporter(new GLTransactionBusinessObject(Factory), new NotificationBuffer()).IsHighWaterMarkEnabled)
			{
				AssertEquals("High water mark registry value should have been set", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.GetValueWithoutFallback(CompanyNZ.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.GetValueWithoutFallback(CompanyUS.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.GetValueWithoutFallback(CompanyKR.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
			else
			{
				AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.GetValueWithoutFallback(CompanyNZ.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.GetValueWithoutFallback(CompanyUS.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.GetValueWithoutFallback(CompanyKR.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		[TestDate(2009, 1, 23, 15, 30, 40)]
		public void TestProcessCompanyWithoutActiveBranch()
		{
			SetupAllRequiredRegistries(true);

			GlbCompany.CurrentCompany.Branches.ForEach(x => x.GB_IsActive = false);
			GlbCompany.CurrentCompany.Factory.Save();

			var processor = GetNewExportProcessor();
			var notify = new NotificationBuffer();

			AssertEquals("Pre-condition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			processor.Process(notify);

			AssertEquals("Should have error because company has no active branch", true, notify.HasErrors);
			AssertContains($"Cannot process company {Env.CurrentCompany.Code} because it does not have active branch.", notify.AsString);
			AssertEquals("Error notification email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmailWithSubject($"GL Transactions of Company {Env.CurrentCompany.Code} Export Failure");
			AssertRecipientWithEmail(dummyEmailGroup.Staff[0].GS_EmailAddress);
		}

		[TestDate(2009, 1, 23, 15, 30, 39)]
		public void TestMutexLockTryAgain()
		{
			PrepareTestData();

			SystemDataRegistry.Instance.EnableAutomaticGLTransactionsCSVExport.SetValue(CompanyUS.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			List<ZGlobalMutex> otherMutexes = new List<ZGlobalMutex>();
			try
			{
				GLTransactionExportProcessor processor = GLTransactionExportProcessor.New();
				NotificationBuffer notify = new NotificationBuffer();

				var companiesToProcess = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True));

				foreach (GlbCompany company in companiesToProcess)
				{
					var otherMutex = new ZGlobalMutex(MutexIDs.BatchAggregtorRunning, company.PK.ToString());
					Assert(otherMutex.Lock());
					otherMutexes.Add(otherMutex);
				}

				processor.Process(notify);

				ZString expectedFileForCompanyAU = Path.Combine(AUTestPath, "GLTransactions20090123153040 " + Env.CurrentCompany.Code + " batch 1000.csv");

				AssertEquals("Problem not reported as error", false, notify.HasErrors);

				var assertableString = Regex.Replace(notify.AsString, @"since.*$", "", RegexOptions.Multiline).TrimEnd('\n').Split('\n');
				var expected = new[] { "Export GL Transactions Process of Company EDI cannot run now because: GL Account update is currently being run by user 'CargoWise Support' ",
					"Export GL Transactions Process of Company CNZ cannot run now because: GL Account update is currently being run by user 'CargoWise Support' ",
					"Export GL Transactions Process of Company CKR cannot run now because: GL Account update is currently being run by user 'CargoWise Support' ",
					"Export GL Transactions Process of Company EDI cannot run now because: GL Account update is currently being run by user 'CargoWise Support' ",
					"Export GL Transactions Process of Company CNZ cannot run now because: GL Account update is currently being run by user 'CargoWise Support' ",
					"Export GL Transactions Process of Company CKR cannot run now because: GL Account update is currently being run by user 'CargoWise Support' " };

				AssertContainsExactElementsInAnyOrder("Processing for each company can't run due to lock, and one retry is performed for each company", expected, assertableString);
			}
			finally
			{
				foreach (var otherMutex in otherMutexes)
				{
					otherMutex.Unlock();
				}
			}
		}

		[TestDate(2009, 1, 23, 15, 30, 39)]
		public void TestDeadlockTryAgain()
		{
			PrepareTestData();

			AssertEquals("no of email sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			SystemDataRegistry.Instance.EnableAutomaticGLTransactionsCSVExport.SetValue(CompanyUS.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var processor = new GLTransactionExportProcessorForDeadlockTest();

			var notify = new NotificationBuffer();

			processor.Process(notify);

			var msg = notify.AsString;

			var deadlock = msg.IndexOf("deadlock");
			Assert("a database deadlock should occur", deadlock > 0);

			var companyName = msg.LastIndexOf(" Company ", deadlock);
			Assert("company name should not be blank", companyName > 0);

			var companyNameString = msg.Substring(companyName, deadlock - companyName);
			Assert("company name should not be blank", !string.IsNullOrWhiteSpace(companyNameString));

			var retry = msg.IndexOf(companyNameString, companyName + 10);
			Assert("Processing for a company couldn't run due to deadlock, and one retry was performed for the company", retry > 0);

			var emails = Env.OutgoingMailManager.EmailsCreated;

			Assert("emails sent", emails.Count >= 2);

			foreach (var email in emails)
			{
				Assert("deadlock should be mentioned in email body", email.Body.IndexOf("deadlock") > 0);
				Assert("the email subject should be '...Successfully...'", email.Subject.IndexOf("Successfully") > 0);
			}
		}

		#region setup

		void AssertEmailWithSubject(ZString subject)
		{
			bool result = false;

			foreach (EmailDef current in Env.OutgoingMailManager.EmailsCreated)
			{
				if (current.Subject == subject)
				{
					result = true;
					break;
				}
			}

			AssertEquals("email with specified subject exists", true, result);
		}

		void AssertRecipientWithEmail(ZString email)
		{
			foreach (EmailDef current in Env.OutgoingMailManager.EmailsCreated)
			{
				foreach (RecipientDef recipient in current.Recipients)
				{
					if (recipient.Email == email)
					{
						return;
					}
				}
			}

			Fail($"recipient with specified email {email} does NOT exists");
		}

		void PrepareTestData()
		{
			AUTestPath = AddTempFolder(Path.Combine(Env.TempPath, "AU"));
			NZTestPath = AddTempFolder(Path.Combine(Env.TempPath, "NZ"));
			KRTestPath = AddTempFolder(Path.Combine(Env.TempPath, "KR"));
			CompanyNZ = SetupCompanyAndBranch("CNZ", "new zealand", Core.Constants.CountryCodes.NewZealand, "BNZ");
			CompanyUS = SetupCompanyAndBranch("CUS", "us", Core.Constants.CountryCodes.UnitedStates, "BUS");
			CompanyKR = SetupCompanyAndBranch("CKR", "kr", Core.Constants.CountryCodes.KoreaSouth, "BKR");
			SetupAllRequiredRegistries();

			BatchTestHelper.SetControlAccounts();
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PeriodTestHelper.SetupSinglePeriod(200803, new ZDateTime(2008, 3, 1), new ZDateTime(2008, 3, 31, 23, 59, 00), GlbCompany.CurrentCompany.PK);
			PeriodTestHelper.SetupSinglePeriod(200803, new ZDateTime(2008, 3, 1), new ZDateTime(2008, 3, 31, 23, 59, 00), CompanyNZ.PK);
			PeriodTestHelper.SetupSinglePeriod(200803, new ZDateTime(2008, 3, 1), new ZDateTime(2008, 3, 31, 23, 59, 00), CompanyKR.PK);

			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BatchTestHelper.APControlAccount);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BatchTestHelper.ARControlAccount);

			SetupTransactions(CompanyNZ.Branches[0]);
			SetupTransactions(GlbBranch.CurrentBranch);
			SetupTransactions(CompanyKR.Branches[0]);

			TestConnection.ExecuteNonQuery(String.Format("UPDATE dbo.AccChargeCode SET AC_AG_CostAccount = 'AC129D82-B88D-45EE-BCE5-25592F734023', AC_SystemLastEditTimeUtc = GETUTCDATE(), AC_SystemLastEditUser = 'TST' where AC_CODE = 'BOND' OR AC_CODE = 'CLAIM'"));
		}

		void SetupTransactions(GlbBranch loginBranch)
		{
			using (loginBranch.SetAsTemporaryContext())
			{
				TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
				Invoice invoice = (Invoice)testObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.LocalCurrency.CurrentBuyRate);

				invoice.AH_InvoiceAmount = 333m;
				invoice.AH_AG = BatchTestHelper.APControlAccount;
				invoice.AH_PostDate = new ZDateTime(2008, 3, 10);
				invoice.AH_Ledger = "AP";
				invoice.AH_TransactionType = "INV";
				invoice.AH_PostToGL = "Y";

				InvoiceLine invoiceLine = (InvoiceLine)testObjectCreator.CreateInvoiceLine(invoice, GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.LocalCurrency.CurrentBuyRate, 333m);

				invoiceLine.AL_AG = BatchTestHelper.APControlAccount;
				invoiceLine.AL_LineAmount = 333m;
				invoiceLine.AL_OSAmount = 333m;
				invoiceLine.AL_PostDate = new ZDateTime(2008, 3, 10);
				AssertNotNull("reverse should be set when post is set due to new reve recognition", invoiceLine.AL_ReverseDate);

				Factory.Save();
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			TidyUp();
		}

		AccountingPeriodTestHelper PeriodTestHelper
		{
			get { return periodTestHelper ?? (periodTestHelper = new AccountingPeriodTestHelper(Factory)); }
		}
		AccountingPeriodTestHelper periodTestHelper;

		public GlbGroup DummyEmailGroup
		{
			get
			{
				if (dummyEmailGroup == null)
				{
					dummyEmailGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);

					if (dummyEmailGroup.Staff.Count == 0)
					{
						GlbStaff currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
						dummyEmailGroup.Staff.Add(currentStaff);
					}
					dummyEmailGroup.Staff[0].GS_EmailAddress = "teststaff@abc.com";
					Factory.Save();
				}
				return dummyEmailGroup;
			}
		}
		GlbGroup dummyEmailGroup;

		public GlbGroup DummyEmailGroup2
		{
			get
			{
				if (dummyEmailGroup2 == null)
				{
					dummyEmailGroup2 = Factory.NewWithValidTestData<GlbGroup>();

					if (dummyEmailGroup2.Staff.Count == 0)
					{
						GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
						dummyEmailGroup2.Staff.Add(staff);
					}
					dummyEmailGroup2.Staff[0].GS_EmailAddress = "teststaff2@xyz.com";
					Factory.Save();
				}
				return dummyEmailGroup2;
			}
		}
		GlbGroup dummyEmailGroup2;

		void SetupAllRequiredRegistries(bool isConfigureAUCompanyOnly = false)
		{
			SystemDataRegistry.Instance.EnableAutomaticGLTransactionsCSVExport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.GLTransactionsCSVExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AUTestPath);
			SystemDataRegistry.Instance.GLTransCSVExportNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, DummyEmailGroup.PK.ToGuid());

			if (!isConfigureAUCompanyOnly)
			{
				SystemDataRegistry.Instance.EnableAutomaticGLTransactionsCSVExport.SetValue(CompanyNZ.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				SystemDataRegistry.Instance.EnableAutomaticGLTransactionsCSVExport.SetValue(CompanyUS.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				SystemDataRegistry.Instance.EnableAutomaticGLTransactionsCSVExport.SetValue(CompanyKR.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				SystemDataRegistry.Instance.GLTransactionsCSVExportDirectory.SetValue(CompanyNZ.PK.ToGuid(), Guid.Empty, Guid.Empty, NZTestPath);
				SystemDataRegistry.Instance.GLTransactionsCSVExportDirectory.SetValue(CompanyKR.PK.ToGuid(), Guid.Empty, Guid.Empty, KRTestPath);

				SystemDataRegistry.Instance.GLTransCSVExportNotificationGroup.SetValue(CompanyNZ.PK.ToGuid(), Guid.Empty, Guid.Empty, DummyEmailGroup.PK.ToGuid());
				SystemDataRegistry.Instance.GLTransCSVExportNotificationGroup.SetValue(CompanyUS.PK.ToGuid(), Guid.Empty, Guid.Empty, DummyEmailGroup.PK.ToGuid());
				SystemDataRegistry.Instance.GLTransCSVExportNotificationGroup.SetValue(CompanyKR.PK.ToGuid(), Guid.Empty, Guid.Empty, DummyEmailGroup2.PK.ToGuid());
			}
		}

		BatchTestHelper BatchTestHelper
		{
			get { return batchTestHelper ?? (batchTestHelper = new BatchTestHelper(Factory)); }
		}
		BatchTestHelper batchTestHelper;

		List<String> TempTestFolders
		{
			get { return tempTestFolders ?? (tempTestFolders = new List<string>()); }
		}
		List<String> tempTestFolders;

		GlbCompany SetupCompanyAndBranch(ZString companyCode, ZString companyName, ZString countryCode, ZString branchCode)
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_Name = companyName;
			company.GC_RN_NKCountryCode = countryCode;
			company.GC_IsActive = true;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			GlbBranch branch = company.Branches.AddNew();
			branch.GB_Code = branchCode;
			branch.GB_IsActive = true;

			Factory.Save();

			return company;
		}

		string AddTempFolder(string folderFullName)
		{
			TempTestFolders.Add(folderFullName);
			if (!Directory.Exists(folderFullName))
			{
				Directory.CreateDirectory(folderFullName);
			}
			return folderFullName;
		}

		void TidyUp()
		{
			foreach (string folderFullName in TempTestFolders)
			{
				foreach (string fileName in Directory.GetFiles(folderFullName))
				{
					AccountingUtils.DeleteFileSafe(fileName);
				}
				AccountingUtils.DeleteDirectorySafe(folderFullName);
			}
		}

		protected override DbConnection TestConnection
		{
			get { return Db.Connection; }
		}

		protected virtual GLTransactionExportProcessor GetNewExportProcessor()
		{
			return GLTransactionExportProcessor.New();
		}

		protected virtual TimeSpan ExpectedHighWaterMarkBuffer
		{
			get { return new TimeSpan(48, 0, 0); }
		}

		GlbCompany CompanyNZ, CompanyUS, CompanyKR;
		ZString AUTestPath, NZTestPath, KRTestPath;

		class GLTransactionExportProcessorForDeadlockTest : GLTransactionExportProcessor
		{
			protected override IAggregateRunner GetIAggregateRunner()
			{
				var mock = new Mock<IAggregateRunner>();

				mock.Setup(m => m.AggregateResult).Returns("deadlock");
				mock.Setup(m => m.FailedToAquireMutex).Returns(false);
				mock.Setup(m => m.FailedToAquireMutexReason).Returns((string)null);
				mock.Setup(m => m.FailedWithDeadlock).Returns(true);
				mock.Setup(m => m.Aggregate()).Returns(false);

				return mock.Object;
			}
		}

		#endregion
	}
}
