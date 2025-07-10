using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	class MultiCompaniesGLJournalFlatFileDataImporterTest : GLJournalFlatFileDataImporterTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestExtractToDataAdapter()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2005, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var testCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "TST");
			periodTestHelper.PostPeriodsForEntireYear(2005, testCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			XmlDocument xmlDoc;
			using (Stream reader = File.OpenRead(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournal.xml"))
			{
				xmlDoc = fDirector.LoadXmlDoc(reader);
			}

			Xsd.GLJournalCollection xsd = fDirector.ExtractJournalNodes(xmlDoc);
			fImporter.ExtractToDataAdapter(xsd, new NotificationBuffer());

			AssertEquals("Should import 2 GL journals.", 2, fImporter.ImportedJournals_ForTestOnly.Count);
			AssertEquals(GlbCompany.CurrentCompany.GC_Code, fImporter.ImportedJournals_ForTestOnly[0].Company.GC_Code);
			AssertEquals("TST", fImporter.ImportedJournals_ForTestOnly[1].Company.GC_Code);
		}

		public void TestImportCSVWithAttribute()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			var chart = TestObjectCreator.CreateAlternateChart("MGT");
			TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "9");
			var glHeader = TestObjectCreator.CreateGLHeader("3330.10.00");
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, false);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, false);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, false);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, false);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, false);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, false);

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			var org = TestObjectCreator.CreateOrgHeader("org", true, true);
			Factory.Save();

			var data = "GLJH,,,GJL,202206,202207,DEPRECIATION OF COMPUTER EQUIPMENT FOR THE MONTH OF JUNE1,ELM,,\r\nGLJL,3330.10.00,,SYD,BRN,20500,AUD,51000,Description1,Org1,ORG,2010.00.01,SEG,2010.00.02,Zorg,TPY,LOC,WEU,STI,SPR\r\nGLJL,1010.10.00,,SYD,BRN,-20500,AUD,51000,Description1,Org1,ORG,2010.00.01,SEG,2010.00.02";
			try
			{
				using (var memoryStream = new MemoryStream())
				using (var streamWriter = new StreamWriter(memoryStream))
				{
					streamWriter.Write(data);
					streamWriter.Flush();
					memoryStream.Position = 0;
					using (TextReader reader = new StreamReader(memoryStream))
					{
						fImporter.ImportDataToFactoryCore(reader, "test.csv", notification, out var additionalTransactionAction);
					}
				}
			}
			finally
			{
				if (fImporter.ImportLockWithHash_ForTestOnly != null)
				{
					fImporter.ImportLockWithHash_ForTestOnly.Dispose();
				}
			}
			Assert(!notification.HasErrors);
			var result = fImporter.LastImportedJournal.Factory.Load<AccTransactionLineDissectionAttribute>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals(6, result.Length);
				AssertEquals(1, result.Where(r => r.ALD_Attribute == "ORG" && r.ALD_AttributeValueID == org.PK && r.ALD_AttributeValue == "").Count());
				AssertEquals(1, result.Where(r => r.ALD_Attribute == "OCG" && r.ALD_AttributeValue == "TPY").Count());
				AssertEquals(1, result.Where(r => r.ALD_Attribute == "LFO" && r.ALD_AttributeValue == "LOC").Count());
				AssertEquals(1, result.Where(r => r.ALD_Attribute == "LFE" && r.ALD_AttributeValue == "WEU").Count());
				AssertEquals(1, result.Where(r => r.ALD_Attribute == "TIC" && r.ALD_AttributeValue == "STI").Count());
				AssertEquals(1, result.Where(r => r.ALD_Attribute == "SPR" && r.ALD_AttributeValue == "SPR").Count());
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 08, 10)]
		public void TestAttachFileToEveryGLJournal()
		{
			var suspendCreateEDocOnSaving = GLJournal.SuspendCreateEDocOnSaving();

			var fileName = "ValidGJLJournal_Aggregation.csv";
			AttachFileToEDocs(fileName);

			AssertEquals(2, fImporter.ImportedJournals_ForTestOnly.Count);

			var newFactory = new BusinessObjectFactory();
			var reloadedJournal1 = (IDocManagerSupport)newFactory.Load<GLJournal>(fImporter.ImportedJournals_ForTestOnly[0].PK);
			var reloadedJournal2 = (IDocManagerSupport)newFactory.Load<GLJournal>(fImporter.ImportedJournals_ForTestOnly[1].PK);
			AssertFileOnEDocs(reloadedJournal1, fileName);
			AssertFileOnEDocs(reloadedJournal2, fileName);

			suspendCreateEDocOnSaving.Dispose();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 08, 10)]
		public void TestAttachFileToEveryGLJournalApprovalRequest()
		{
			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var fileName = "ValidGJLJournal_Aggregation.csv";
			AttachFileToEDocs(fileName);

			var approvalRequests = fImporter.FactoryProvider_ForTestOnly.Current.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, null));
			AssertEquals(2, approvalRequests.Length);
			var approval1 = approvalRequests[0];
			var approval2 = approvalRequests[1];
			AssertEquals(GenApprovalRequestApprovalStatus.Requested, approval1.XP_ApprovalStatus);
			AssertEquals(GenApprovalRequestApprovalStatus.Requested, approval2.XP_ApprovalStatus);

			var newFactory = new BusinessObjectFactory();
			var reloadedJournal1 = (IDocManagerSupport)newFactory.Load<GLJournalApprovalRequest>(approval1.PK);
			var reloadedJournal2 = (IDocManagerSupport)newFactory.Load<GLJournalApprovalRequest>(approval2.PK);
			AssertFileOnEDocs(reloadedJournal1, fileName);
			AssertFileOnEDocs(reloadedJournal2, fileName);
		}

		void AttachFileToEDocs(string fileName)
		{
			var tstCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "TST");
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(2022, tstCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var fileToRead = Path.Combine(BaseSourcePath, $@"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\{fileName}");
			using (var reader = new StreamReader(fileToRead))
			{
				var returnVal = fImporter.ImportData(reader, fileToRead, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}
		}

		void AssertFileOnEDocs(IDocManagerSupport docParent, string fileName)
		{
			var docManagerInfo = docParent.DocManagerInfo;
			AssertEquals("One eDoc is attached.", 1, docManagerInfo.AllEDocs.Count);

			var eDoc = docManagerInfo.AllEDocs[0];
			AssertEquals(DocManagerCodes.GLJournal, eDoc.DocType);
			AssertEquals(fileName, eDoc.FileName);

			var expectedContent = File.ReadAllBytes(Path.Combine(BaseSourcePath, $@"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\{fileName}"));
			AssertEquals("Content length", expectedContent.Length, eDoc.ImageData.Length);
			AssertArrayEqualsByElements(expectedContent, eDoc.ImageData);

			var bo = docParent as EnterpriseBusinessObject;
			AssertNotNull("docParent should be EnterpriseBusinessObject.", bo);

			var dataImportEvents = bo.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractToDataAdapter_ValidationError()
		{
			const string importFilePath = @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\InvalidGJLJournal_ValidationError.xml";
			const string errorText = @"
Error: Journal Error 'EDI' - AL_OH: Organization can only be set for Elimination Journal.
Error: Journal Error 'EDI' - PostPeriod: This period is invalid. Please go to Period Management to setup periods
Error: Journal Error 'EDI' - AH_PostDate: Please enter an Invoice Post Date.
Error: Journal Error 'EDI' - AH_TransactionCategory: Enter a valid Transaction Category.
";
			XmlDocument xmlDoc;
			using (Stream reader = File.OpenRead(BaseSourcePath + importFilePath))
			{
				xmlDoc = fDirector.LoadXmlDoc(reader);
			}

			Xsd.GLJournalCollection xsd = fDirector.ExtractJournalNodes(xmlDoc);
			var notificationBuffer = new NotificationBuffer();
			fImporter.ExtractToDataAdapter(xsd, notificationBuffer);

			Assert(notificationBuffer.HasErrors);
			AssertContains(errorText, notificationBuffer.AsString);
		}

		public void TestExtractToDataAdapter_LocalAmountUnbalanceForForeignCurrency_ShouldNotCreateBalanceLineWhenValidationErrorExists()
		{
			var accountPK = GetBalancingAccountPK();

			using (AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateDifferenceAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK))
			{
				var journal = NewJournal("SYD",
					new[]
					{
						NewJournalLine("2010.00.00", 20m, "USD", 10m, "DR"),
						NewJournalLine("2020.00.00", 21m, "USD", 10m, "CR")
					});
				const string validationError = @"Error: Journal Error 'EDI' - PostPeriod: This period is invalid. Please go to Period Management to setup periods
Error: Journal Error 'EDI' - AH_PostDate: Please enter an Invoice Post Date.
";
				var notificationBuffer = AssertExtractToDataAdapter(journal);
				Assert("Should report errors.", notificationBuffer.HasErrors);
				AssertEquals(validationError, notificationBuffer.AsString);
				AssertNoBalanceLine(accountPK, totalLines: 2);
			}
		}

		public void TestExtractToDataAdapter_LocalAmountUnbalanceForLocalCurrency_DifferenceAccountNotSet_LocalAmountUnbalanceError()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2005, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var journal = NewJournal("SYD",
				new[]
				{
					NewJournalLine("2010.00.00", 20m, "USD", 10m, "DR"),
					NewJournalLine("2020.00.00", 21m, "USD", 10m, "CR")
				});

			var notificationBuffer = AssertExtractToDataAdapter(journal);
			Assert("Should report errors.", notificationBuffer.HasErrors);
			AssertEquals(GetJournalCreatedAndErrorMessageWithJournalDetail("EDI", "Journal does not balance. Discrepancy: -1.00"), notificationBuffer.AsString);
		}

		public void TestExtractToDataAdapter_LocalAmountUnbalanceForLocalCurrency_DifferenceAccountSet_AutoBalanced()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2005, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			var accountPK = GetBalancingAccountPK();

			using (AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateDifferenceAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK))
			{
				var journal = NewJournal("SYD",
					new[]
					{
						NewJournalLine("2010.00.00", 20m, "USD", 10m, "DR"),
						NewJournalLine("2020.00.00", 21m, "USD", 10m, "CR")
					});

				var notificationBuffer = AssertExtractToDataAdapter(journal);
				Assert("Should not report error.", !notification.HasErrors);

				AssertBalanceLine(accountPK, balancingAmount: 1, totalLines: 3);
			}
		}

		public void TestExtractToDataAdapter_NoValidJournalBranch_ShouldNotProcess()
		{
			var journal = NewJournal("AAA",
					new[]
					{
						NewJournalLine("2010.00.00", 20m, "USD", 10m, "DR"),
						NewJournalLine("2020.00.00", 20m, "USD", 10m, "CR")
					});

			var notificationBuffer = AssertExtractToDataAdapter(journal);
			Assert("Should not report error.", !notification.HasErrors);
			AssertEquals("Should not process any GL journal.", 0, fImporter.ImportedJournals_ForTestOnly.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractToDataAdapter_ApprovalRequestNotPostJounalWillNotBeSaved()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				var testCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "TST");
				periodTestHelper.PostPeriodsForEntireYear(2022, testCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				CreateGLJournalWithApprovalRequest(out var additionalTransactionAction);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("GeneralLedgerJournal_FirstApprovald is not allowed.", false, Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed);

				BusinessObjectFactory.SaveTogether(additionalTransactionAction);

				AssertEquals("Journal will not be saved because approval request status is not PST", false, fImporter.LastImportedJournal.IsInDatabase);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractToDataAdapter_ApprovalRequestPostJounalWillBeSaved()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				var testCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "TST");
				periodTestHelper.PostPeriodsForEntireYear(2022, testCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				CreateGLJournalWithApprovalRequest(out var additionalTransactionAction);

				AssertEquals("GeneralLedgerJournal_FirstApprovald is allowed.", true, Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed);
				var approvalRequest = fImporter.FactoryProvider_ForTestOnly.Current.LoadTop1<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, fImporter.LastImportedJournal.PK));

				AssertNotNull("Journal has 1 approval request.", approvalRequest);
				AssertEquals("approval request status is PST when GeneralLedgerJournal_FirstApprovald is allowed.", GenApprovalRequestApprovalStatus.Posted, approvalRequest.XP_ApprovalStatus);

				BusinessObjectFactory.SaveTogether(additionalTransactionAction);
				AssertEquals("Journal will be saved because approval request status is PST", true, fImporter.LastImportedJournal.IsInDatabase);
			});
		}

		Guid GetBalancingAccountPK()
		{
			var account = Factory.NewWithValidTestData<AccGLHeader>();
			account.AG_AccountNum = "1010.10.10";

			return account.PK.ToGuid();
		}

		NotificationBuffer AssertExtractToDataAdapter(string xml)
		{
			XmlDocument xmlDoc = fDirector.LoadXmlFromString(xml);
			Xsd.GLJournalCollection xsd = fDirector.ExtractJournalNodes(xmlDoc);
			var notificationBuffer = new NotificationBuffer();
			fImporter.ExtractToDataAdapter(xsd, notificationBuffer);

			return notificationBuffer;
		}

		void AssertNoBalanceLine(Guid accountPK, int totalLines)
		{
			var lines = fImporter.LastImportedJournal.GLJournalLines;
			var balanceLine = (GLJournalLine)lines.FirstOrDefault(jl => ((GLJournalLine)jl).AL_AG == accountPK);

			AssertEquals(totalLines, lines.Count);
			AssertNull("Balance line should not be created.", balanceLine);
		}

		void AssertBalanceLine(Guid accountPK, ZDecimal balancingAmount, int totalLines)
		{
			var lines = fImporter.LastImportedJournal.GLJournalLines;
			var balanceLine = (GLJournalLine)lines.FirstOrDefault(jl => ((GLJournalLine)jl).AL_AG == accountPK);

			AssertEquals(totalLines, lines.Count);
			AssertNotNull("The balance line should be created for the exchange rate difference account.", balanceLine);
			AssertEquals("The balance line should be in correct amount.", balancingAmount, balanceLine.AL_LineAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGLJournalApprovalRequest_WithoutSecurity()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				var testCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "TST");
				periodTestHelper.PostPeriodsForEntireYear(2022, testCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				CreateGLJournalWithApprovalRequest(out var additionalTransactionAction);

				var approvalRequests = fImporter.FactoryProvider_ForTestOnly.Current.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, null));

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("GeneralLedgerJournal_FirstApprovald is not allowed.", false, Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed);
				AssertEquals("2 approval request has been created.", 2, approvalRequests.Length);
				AssertEquals("First approval request status is REQ when GeneralLedgerJournal_FirstApprovald is not allowed.", GenApprovalRequestApprovalStatus.Requested, approvalRequests[0].XP_ApprovalStatus);
				AssertEquals("Second approval request status is REQ when GeneralLedgerJournal_FirstApprovald is not allowed.", GenApprovalRequestApprovalStatus.Requested, approvalRequests[1].XP_ApprovalStatus);
			});
		}

		string GetJournalCreatedAndErrorMessageWithJournalDetail(string company, string message)
		{
			return $"Error: Journal Error '{company}' - {message}\r\n";
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGLJournalApprovalRequest_WithSecurity()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				var testCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "TST");
				periodTestHelper.PostPeriodsForEntireYear(2022, testCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				CreateGLJournalWithApprovalRequest(out var additionalTransactionAction);

				var approvalRequests = fImporter.FactoryProvider_ForTestOnly.Current.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, fImporter.ImportedJournals_ForTestOnly[0].PK));

				AssertEquals("GeneralLedgerJournal_FirstApprovald is allowed.", true, Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed);
				AssertEquals("Journal has 1 approval request.", 1, approvalRequests.Length);
				AssertEquals("approval request status is APP when GeneralLedgerJournal_FirstApprovald is allowed.", GenApprovalRequestApprovalStatus.Posted, approvalRequests[0].XP_ApprovalStatus);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGLJournalApprovalRequest_AllowUsersToApproveOwnGLJournalsIsFalse()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				var testCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "TST");
				periodTestHelper.PostPeriodsForEntireYear(2022, testCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
				AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				CreateGLJournalWithApprovalRequest(out var additionalTransactionAction);

				var approvalRequests = fImporter.FactoryProvider_ForTestOnly.Current.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, null));

				AssertEquals("GeneralLedgerJournal_FirstApprovald is allowed.", true, Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed);
				AssertEquals("AllowUsersToApproveOwnGLJournals is false.", false, AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.Value);
				AssertEquals("2 approval request has been created.", 2, approvalRequests.Length);
				AssertEquals("First approval request status is REQ when AllowUsersToApproveOwnGLJournals is false.", GenApprovalRequestApprovalStatus.Requested, approvalRequests[0].XP_ApprovalStatus);
				AssertEquals("Second approval request status is REQ when AllowUsersToApproveOwnGLJournals is false.", GenApprovalRequestApprovalStatus.Requested, approvalRequests[1].XP_ApprovalStatus);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGLJournalApprovalRequest_HasError()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				var company1 = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "TST");
				var company2 = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DAN");

				TestObjectCreator.CreateTestPeriodsForEntireYear(company1, 2022);
				TestObjectCreator.CreateTestPeriodsForEntireYear(company2, 2022);

				Env.Security.GeneralLedgerPostToPreviousOpenPeriod.IsAllowed = false;

				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;

				CreateGLJournalWithApprovalRequest(out var additionalTransactionAction);

				AssertEquals(true, notification.HasErrors);
				AssertEquals("Journal has 0 approval request because there has error.", 0, fImporter.LastImportedJournal.Approvals.Count);
			});
		}

		void TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(Action action)
		{
			var company = TestObjectCreator.CreateNewCompany("DAN");
			company.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var sY1Branch = TestObjectCreator.CreateNewBranch(company, "SY1");

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				action.Invoke();
			}
		}

		void CreateGLJournalWithApprovalRequest(out ITransactionParticipant[] additionalTransactionAction)
		{
			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.All;
			var settings = threshold.AuthorisationSettings.AddNew();
			settings.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings.Amount = 1000000.00m;
			settings.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			var aboveSettings = threshold.AuthorisationSettings.AddNew();
			aboveSettings.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
			aboveSettings.Amount = 1000000.00m;
			aboveSettings.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			try
			{
				var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournal.csv";
				using (TextReader reader = new StreamReader(fileName))
				{
					fImporter.ImportDataToFactoryCore(reader, fileName, notification, out additionalTransactionAction);
				}
			}
			finally
			{
				if (fImporter.ImportLockWithHash_ForTestOnly != null)
				{
					fImporter.ImportLockWithHash_ForTestOnly.Dispose();
				}
			}
		}

		public void TestShouldSuspendValidation()
		{
			AssertEquals(false, fImporter.ShouldSuspendValidation_ForTestOnly);
		}

		public void TestSupportsHistoryForDuplicatesPrevention()
		{
			AssertEquals(true, fImporter.SupportsHistoryForDuplicatesPrevention_ForTestOnly);
		}

		public void TestDaysToKeepHistoryFor()
		{
			AssertEquals(7, fImporter.DaysToKeepHistoryFor_ForTestOnly);
		}

		public void TestImportTypeForDuplicatesPrevention()
		{
			AssertEquals("GLJournal", fImporter.ImportTypeForDuplicatesPrevention_ForTestOnly);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractToDataAdapter_WithoutNewSecurity()
		{
			XmlDocument xmlDoc;
			using (Stream reader = File.OpenRead(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\InvalidGJLJournal_SecurityError.xml"))
			{
				xmlDoc = fDirector.LoadXmlDoc(reader);
			}

			var company1 = TestObjectCreator.CreateNewCompany("TAT");
			company1.GC_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			var branch1 = TestObjectCreator.CreateNewBranch(company1, "TAT");

			var company2 = TestObjectCreator.CreateNewCompany("TET");
			company2.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var branch2 = TestObjectCreator.CreateNewBranch(company2, "TMT");

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, branch2.PK.ToGuid(), Env.CurrentDepartment.PK, company2.PK.ToGuid());
				testSecurity.NewGeneralLedgerJournal.IsAllowed = false;
				Env.SetTemporarySecurityInstanceForTest(testSecurity);
				var xsd = fDirector.ExtractJournalNodes(xmlDoc);
				var notificationBuffer = new NotificationBuffer();
				fImporter.ExtractToDataAdapter(xsd, notificationBuffer);

				Assert(notificationBuffer.HasErrors);
				AssertContains(Env.Security.NewGeneralLedgerJournal.ErrorMessageForNotAllowed, notificationBuffer.AsString);

				testSecurity.NewGeneralLedgerJournal.IsAllowed = true;
				Env.SetTemporarySecurityInstanceForTest(testSecurity);
				var xsd1 = fDirector.ExtractJournalNodes(xmlDoc);
				var notificationBuffer1 = new NotificationBuffer();
				fImporter.ExtractToDataAdapter(xsd1, notificationBuffer1);

				AssertNotContains(Env.Security.NewGeneralLedgerJournal.ErrorMessageForNotAllowed, notificationBuffer1.AsString);
			}
		}

		public void TestCheckImportHistoryInCurrentCompany()
		{
			AssertEquals(false, fImporter.ShouldCheckImportHistoryInCurrentCompany_ForTestOnly);
		}

		public void TestSetExchangeRateDifferenceBalanceJournal_HasGLJournalExchangeRateDifferenceAccount()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2005, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			var accountPK = GetBalancingAccountPK();

			using (AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateDifferenceAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, accountPK))
			{
				var journal = NewJournal("SYD",
					new[]
					{
						NewJournalLine("2010.00.00", 20m, "USD", 10m, "DR"),
						NewJournalLine("2020.00.00", 21m, "USD", 10m, "CR"),
						NewJournalLine("2010.00.00", 40m, "CNY", 10m, "DR"),
						NewJournalLine("2020.00.00", 42m, "CNY", 10m, "CR"),
						NewJournalLine("2010.00.00", 60m, "EUR", 10m, "DR"),
						NewJournalLine("2020.00.00", 61m, "EUR", 11m, "CR"),
						NewJournalLine("2020.00.00", 4m, "AUD", 4m, "DR"),
					});

				var notificationBuffer = AssertExtractToDataAdapter(journal);

				AssertEquals(9, fImporter.LastImportedJournal.Lines.Count);

				var exchangeDifferenceForUSD = fImporter.LastImportedJournal.Lines.FirstOrDefault(x => ((GLJournalLine)x).AL_Desc == "Foreign Currency Exchange Difference for USD") as GLJournalLine;
				AssertNotNull("Should have exchange difference for USD.", exchangeDifferenceForUSD);
				AssertEquals("Amount of exchange difference for USD is 1", 1m, exchangeDifferenceForUSD.AL_LineAmount);

				var exchangeDifferenceForCNY = fImporter.LastImportedJournal.Lines.FirstOrDefault(x => ((GLJournalLine)x).AL_Desc == "Foreign Currency Exchange Difference for CNY") as GLJournalLine;
				AssertNotNull("Should have exchange difference for CNY.", exchangeDifferenceForCNY);
				AssertEquals("Amount of exchange difference for CNY is 2", 2m, exchangeDifferenceForCNY.AL_LineAmount);

				var exchangeDifferenceForEUR = fImporter.LastImportedJournal.Lines.FirstOrDefault(x => ((GLJournalLine)x).AL_Desc == "Foreign Currency Exchange Difference for EUR");
				AssertNull("Should not have exchange difference for EUR because OS amount is not balance.", exchangeDifferenceForEUR);

				Assert("Should have report error.", notificationBuffer.HasErrors);
				AssertEquals("Local amount will not balance after exchange difference lins have been created.", GetJournalCreatedAndErrorMessageWithJournalDetail("EDI", "Journal does not balance. Discrepancy: 3.00"), notificationBuffer.AsString);
			}
		}

		public void TestSetExchangeRateDifferenceBalanceJournal_NoGLJournalExchangeRateDifferenceAccount()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2005, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			var accountPK = GetBalancingAccountPK();

			var journal = NewJournal("SYD",
				new[]
				{
					NewJournalLine("2010.00.00", 20m, "USD", 10m, "DR"),
					NewJournalLine("2020.00.00", 21m, "USD", 10m, "CR"),
					NewJournalLine("2010.00.00", 40m, "CNY", 10m, "DR"),
					NewJournalLine("2020.00.00", 42m, "CNY", 10m, "CR"),
					NewJournalLine("2010.00.00", 60m, "EUR", 10m, "DR"),
					NewJournalLine("2020.00.00", 61m, "EUR", 11m, "CR"),
					NewJournalLine("2020.00.00", 4m, "AUD", 4m, "DR"),
				});

			var notificationBuffer = AssertExtractToDataAdapter(journal);

			AssertEquals(7, fImporter.LastImportedJournal.Lines.Count);

			var exchangeDifferenceForUSD = fImporter.LastImportedJournal.Lines.FirstOrDefault(x => ((GLJournalLine)x).AL_Desc == "Foreign Currency Exchange Difference for USD") as GLJournalLine;
			AssertNull("Should have exchange difference for USD because GLJournalExchangeRateDifferenceAccount is not set.", exchangeDifferenceForUSD);

			var exchangeDifferenceForCNY = fImporter.LastImportedJournal.Lines.FirstOrDefault(x => ((GLJournalLine)x).AL_Desc == "Foreign Currency Exchange Difference for CNY") as GLJournalLine;
			AssertNull("Should have exchange difference for CNY because GLJournalExchangeRateDifferenceAccount is not set.", exchangeDifferenceForCNY);

			var exchangeDifferenceForEUR = fImporter.LastImportedJournal.Lines.FirstOrDefault(x => ((GLJournalLine)x).AL_Desc == "Foreign Currency Exchange Difference for EUR");
			AssertNull("Should not have exchange difference for EUR because GLJournalExchangeRateDifferenceAccount is not set.", exchangeDifferenceForEUR);

			Assert("Should not have report error.", !notificationBuffer.HasErrors);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageOfDataDetailsWhenImportSucceed()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				var testCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "TST");
				periodTestHelper.PostPeriodsForEntireYear(2022, testCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
				Factory.Save();

				var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournal_MultipleJournals.csv";

				var notify = new NotificationBuffer();
				DataImporter importer = new MultiCompaniesGLJournalFlatFileDataImporter();
				importer.ImportData(fileName, notify, SourceInfo.EmptySourceInfo);

				var expectMessage = @"
Saving the data to the database...

Imported data details below:
1. DAN: 00001000, 00001001
2. TST: 00001000";
				Assert(notify.AsString.Contains(expectMessage));
			});
		}

		#region Upload GL Journal count

		protected virtual string UploadGLJournalCountFeatureCode => UsageFeatures.Codes.UploadGLJournalCount;
		protected virtual string UploadGLJournalDetailsFeatureCode => UsageFeatures.Codes.UploadGLJournalDetails;
		protected virtual string UploadGLJournalCountDescription => "Upload GL Journal Count";
		protected virtual string UploadGLJournalDetailsDescription => "Upload GL Journal Details";

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCountForUploadGLJournalFailed()
		{
			Env.Security.NewGeneralLedgerJournal.IsAllowed = false;

			AssertCountForUploadGLJournal(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\InvalidGJLJournal.csv");

			var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			var json = JObject.Parse(ediMessage.EM_MessageTextDetail);

			AssertEquals(UploadGLJournalCountFeatureCode, json["FeatureCode"].ToString());
			AssertEquals(UsageFeatures.Modules.Accounting, json["Module"].ToString());
			AssertEquals(UploadGLJournalCountDescription, json["FeatureDescription"].ToString());

			AssertNull("Count shouldn't be included in the json.", json["Count"]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCountForUploadGLJournalSucceed()
		{
			var tstCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "TST");
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(2022, tstCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			AssertCountForUploadGLJournal(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournal_Aggregation.csv");

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("One UploadGLJournalCount and two UploadGLJournalDetails should be included.", 3, ediMessages.Length);

			var jObjects = ediMessages.Select(msg => JObject.Parse(msg.EM_MessageTextDetail));
			Assert("All the journal identifier should be the same.", jObjects.Select(json => json["JournalIdentifier"].ToString()).ToList().AllSame());

			AssertCountForUploadGLJournalSucceed(jObjects);
		}

		protected virtual void AssertCountForUploadGLJournalSucceed(IEnumerable<JObject> jObjects)
		{
			var uploadGLJournalCount = jObjects.First(json => json["FeatureCode"].ToString() == UploadGLJournalCountFeatureCode);
			AssertEquals(UsageFeatures.Modules.Accounting, uploadGLJournalCount["Module"].ToString());
			AssertEquals(UploadGLJournalCountDescription, uploadGLJournalCount["FeatureDescription"].ToString());
			AssertEquals(2, int.Parse(uploadGLJournalCount["CountOfCompanies"].ToString()));
			AssertEquals(1, int.Parse(uploadGLJournalCount["CountOfJournalTypes"].ToString()));

			var uploadGLJournalDetails = jObjects.Where(json => json["FeatureCode"].ToString() == UploadGLJournalDetailsFeatureCode);
			Assert(uploadGLJournalDetails.All(x => x["Module"].ToString() == UsageFeatures.Modules.Accounting));
			Assert(uploadGLJournalDetails.All(x => x["FeatureDescription"].ToString() == UploadGLJournalDetailsDescription));
			Assert(uploadGLJournalDetails.Any(x => x["JournalType"].ToString() == "GJL" && x["JournalCompanyCode"].ToString() == "TST"));
			Assert(uploadGLJournalDetails.Any(x => x["JournalType"].ToString() == "GJL" && x["JournalCompanyCode"].ToString() == "EDI"));
		}

		protected virtual void AssertCountForUploadGLJournal(string fileName)
		{
			var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertNull("Pre-condition: no USG EDI message in the database", ediMessage);

			try
			{
				using (TextReader reader = new StreamReader(fileName))
				{
					fImporter.ImportDataToFactoryCore(reader, fileName, notification, out var additionalTransactionAction);
					fImporter.LastImportedJournal?.Factory.Save();
				}
			}
			finally
			{
				if (fImporter.ImportLockWithHash_ForTestOnly != null)
				{
					fImporter.ImportLockWithHash_ForTestOnly.Dispose();
				}
			}
		}

		#endregion

		#region XML String

		string NewJournal(string branchCode, string[] journalLines)
		{
			return string.Format(gljh, branchCode, string.Join("", journalLines));
		}

		string NewJournalLine(string accountName, ZDecimal localAmount, string currency, ZDecimal osAmount, string drcr)
		{
			var currencyLine = string.IsNullOrEmpty(currency) ? string.Empty : $"<Currency>{currency}</Currency>";
			var osAmountLine = osAmount == 0m ? string.Empty : $"<Amount>{osAmount}</Amount>";

			return string.Format(gljl, accountName, localAmount, currencyLine, osAmountLine, drcr);
		}

		const string gljh = @"
<GLJournals xmlns=""http://www.edi.com.au/EnterpriseService/"">
	<GLJournal>
		<GLDetail>
			<Branch>{0}</Branch>
			<JournalType>GJL</JournalType>
			<JournalNumber>1234</JournalNumber>
			<Description></Description>
			<InPeriod>200501</InPeriod>
			<OutPeriod>0</OutPeriod>
		</GLDetail>
		<JournalLines>
			{1}
		</JournalLines>
	</GLJournal>
</GLJournals>
";

		const string gljl = @"
			<JournalLine>
				<Account>{0}</Account>
				<Branch>SYD</Branch>
				<Department>BRN</Department>
				<Description></Description>
				<LocalAmount>{1}</LocalAmount>
				{2}
				{3}
				<DRCR>{4}</DRCR>
			</JournalLine>
";
		#endregion

		#region Implementation

		MultiCompaniesGLJournalFlatFileDataImporterTestClass fImporter;
		MultiCompaniesGLJournalXmlDataTransferDirectorTestClass fDirector;
		protected NotificationBuffer notification;

		protected override void SetUp()
		{
			notification = new NotificationBuffer();
			fImporter = new MultiCompaniesGLJournalFlatFileDataImporterTestClass();
			fImporter.CreateConverter(notification);
			fDirector = new MultiCompaniesGLJournalXmlDataTransferDirectorTestClass(new GLJournalDataAdapter(), false);

			var org1 = TestObjectCreator.CreateOrgHeader("2010.00.01", true, true, true, true, true, true);
			org1.OH_Code = "2010.00.01";
			var org2 = TestObjectCreator.CreateOrgHeader("2020.00.01", true, true, true, true, true, true);
			org2.OH_Code = "2020.00.01";
			var org3 = TestObjectCreator.CreateOrgHeader("2010.00.05", true, true, true, true, true, true);
			org3.OH_Code = "2010.00.05";
			var org4 = TestObjectCreator.CreateOrgHeader("2020.00.05", true, true, true, true, true, true);
			org4.OH_Code = "2020.00.05";
			TestObjectCreator.CreateSalesGroup("2010.00.02");
			TestObjectCreator.CreateSalesGroup("2020.00.02");
			TestObjectCreator.CreateSalesGroup("2010.00.06");
			TestObjectCreator.CreateSalesGroup("2020.00.06");
			TestObjectCreator.CreateStaff("AAA");
			TestObjectCreator.CreateStaff("BBB");
			TestObjectCreator.CreateStaff("CCC");
			TestObjectCreator.CreateStaff("DDD");
			TestObjectCreator.CreateStaffGroup("2010.00.04");
			TestObjectCreator.CreateStaffGroup("2020.00.04");
			TestObjectCreator.CreateStaffGroup("2010.00.08");
			TestObjectCreator.CreateStaffGroup("2020.00.08");

			var company = TestObjectCreator.CreateNewCompany("TST");
			company.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateNewBranch(company, "TST");

			Factory.Save();
			base.SetUp();
		}

		class MultiCompaniesGLJournalFlatFileDataImporterTestClass : MultiCompaniesGLJournalFlatFileDataImporter
		{
			public new IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
			{
				return base.CreateConverter(notificationSubscriber);
			}

			public new bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
			{
				return base.ExtractToDataAdapter(xsd, notifications);
			}

			public new bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				return base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
			}

			public bool ShouldSuspendValidation_ForTestOnly => base.ShouldSuspendValidation;
			public bool SupportsHistoryForDuplicatesPrevention_ForTestOnly => SupportsHistoryForDuplicatesPrevention;
			public int DaysToKeepHistoryFor_ForTestOnly => DaysToKeepHistoryFor;
			public string ImportTypeForDuplicatesPrevention_ForTestOnly => ImportTypeForDuplicatesPrevention;
			public BusinessObjectFactoryProvider FactoryProvider_ForTestOnly => FactoryProvider;
			public bool ShouldCheckImportHistoryInCurrentCompany_ForTestOnly => ShouldCheckImportHistoryInCurrentCompany;
			public SqlApplicationLock ImportLockWithHash_ForTestOnly => ImportLockWithHash;
		}

		class MultiCompaniesGLJournalXmlDataTransferDirectorTestClass : GLJournalXmlDataTransferDirectorTestClass
		{
			public MultiCompaniesGLJournalXmlDataTransferDirectorTestClass(GLJournalDataAdapter adapter, bool hasLicence)
				: base(adapter, hasLicence)
			{
			}

			public XmlDocument LoadXmlFromString(string xml)
			{
				bool exceptionOccuredViaXmlManipulation = false;
				XmlDocument journalXmlDocument = new XmlDocument();

				try
				{
					journalXmlDocument.LoadXml(xml);
				}
				catch (XmlException)
				{
					exceptionOccuredViaXmlManipulation = true;
					Globals.Message.ShowError(GLJournalDataAdapter.InvalidXmlFileErrorMessage, "Invalid XML Document");
				}

				return exceptionOccuredViaXmlManipulation ? null : journalXmlDocument;
			}
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
