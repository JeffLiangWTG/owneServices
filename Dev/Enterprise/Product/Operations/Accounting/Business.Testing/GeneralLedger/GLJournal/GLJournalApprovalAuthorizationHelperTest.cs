using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	public class GLJournalApprovalAuthorizationHelperTest : TestCaseWithFactory
	{
		public void TestRequiredSecurityCheckPointByCachedDataExpectsCorrectCacheType()
		{
			this.SetupTestData();

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);

			object cache = null;
			AssertNoExceptionThrown("Initialize new cache.", () => GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData(journal, ref cache));
			AssertNoExceptionThrown("Use cache for Journal", () => GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData(journal, ref cache));
			AssertNoExceptionThrown("Use cache for Journal line", () => GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData((GLJournalLine)journal.Lines[0], cache));

			cache = new List<string>();
			var expectedExceptionMessage = "Type of cache should not be exposed outside this helper as this is internal data. However correct type should be passed if it's not null. It have to be value returned from RequiredSecurityCheckPointByCachedData method for journal.\r\nParameter name: cachedData";

			AssertExceptionThrown("Invalid cache for Journal", typeof(ArgumentNullException), expectedExceptionMessage, () => GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData(journal, ref cache), true);
			AssertExceptionThrown("Invalid cache for Journal line", typeof(ArgumentNullException), expectedExceptionMessage, () => GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData((GLJournalLine)journal.Lines[0], cache), true);

			cache = null;
			AssertNoExceptionThrown("Without cache for Journal", () => GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData(journal, ref cache));
			AssertNoExceptionThrown("Without cache for Journal line", () => GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData((GLJournalLine)journal.Lines[0], cache));

			var anyThresholdSettings = new GLJournalApprovalThresholdCollection();
			var anyThresholdSetting = anyThresholdSettings.AddNew();
			anyThresholdSetting.Type = Enterprise.Accounting.Registry.Business.GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, anyThresholdSettings);

			AssertNotNull(cache);
			cache = null;

			AssertEquals("Get the default securit check point", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal), Env.Security.GeneralLedgerJournal_FirstApproval);
			AssertEquals("Without cache for Journal", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData(journal, ref cache), Env.Security.GeneralLedgerJournal_FirstApproval);
			AssertNotNull("Cache should be initialized", cache);
			AssertEquals("Without cache for Journal", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData((GLJournalLine)journal.Lines[0], cache), Env.Security.GeneralLedgerJournal_FirstApproval);

			cache = null;
			var noteJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Now, ZDateTime.Now);
			var noteJournalLine = noteJournal.GLJournalLines.AddNew();
			AssertEquals("Without cache for Note Journal", Env.Security.None, GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData(noteJournal, ref cache));
			AssertEquals("Without cache for Note Journal line", Env.Security.None, GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData(noteJournalLine, cache));
		}

		#region TestCheckLevelSecurityRights

		public void TestCheckLevelSecurityRightsWithNonInteractivePrivider()
		{
			var journal = CheckLevelSecurityRightSetup(false);

			SecurityOverrideProviderSource.Get(journal).Provider = new NonInteractiveSecurityOverrideProvider();
			Assert("CheckLevelSecurityRights", GLJournalApprovalAuthorizationHelper.CheckLevelSecurityRights(journal));

			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
			Assert("CheckLevelSecurityRights", !GLJournalApprovalAuthorizationHelper.CheckLevelSecurityRights(journal));

			var noteJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Now, ZDateTime.Now);
			Assert("CheckLevelSecurityRights for note journal", GLJournalApprovalAuthorizationHelper.CheckLevelSecurityRights(noteJournal));
		}

		public void TestCheckLevelSecurityRightsWithInteractiveProvider()
		{
			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertCheckLevelSecurityRightsWithInteractiveProvider();

			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertCheckLevelSecurityRightsWithInteractiveProvider();

			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertCheckLevelSecurityRightsWithInteractiveProvider(approvalNotAllowedOwnJournal: true);

			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertCheckLevelSecurityRightsWithInteractiveProvider();
		}

		public void TestCheckLevelSecurityRightsWithInteractiveProviderAndCreateRequestWithoutPopUpsConfiguration()
		{
			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertCheckLevelSecurityRightsWithInteractiveProvider(withCreateRequestWithoutPopUpsConfiguation: true);

			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertCheckLevelSecurityRightsWithInteractiveProvider(withCreateRequestWithoutPopUpsConfiguation: true);

			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertCheckLevelSecurityRightsWithInteractiveProvider(approvalNotAllowedOwnJournal: true, withCreateRequestWithoutPopUpsConfiguation: true);

			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertCheckLevelSecurityRightsWithInteractiveProvider(withCreateRequestWithoutPopUpsConfiguation: true);
		}

		void AssertCheckLevelSecurityRightsWithInteractiveProvider(bool approvalNotAllowedOwnJournal = false, bool withCreateRequestWithoutPopUpsConfiguation = false)
		{
			var journal = CheckLevelSecurityRightSetup(withCreateRequestWithoutPopUpsConfiguation);

			var interactiveSecurityProviderWithApprovalRequestMock = new Mock<IDummyInteractiveSecurityOverrideProviderWithApprovalRequest>();
			SecurityOverrideProviderSource.Get(journal).Provider = interactiveSecurityProviderWithApprovalRequestMock.Object;
			if (withCreateRequestWithoutPopUpsConfiguation)
			{
				interactiveSecurityProviderWithApprovalRequestMock.Verify(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>()), Times.Never);
			}
			else
			{
				if (approvalNotAllowedOwnJournal)
				{
					interactiveSecurityProviderWithApprovalRequestMock.Setup(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>()));
				}
				else
				{
					return; //can't test with mock SecurityCertificates[requiredCheckPoint]
				}
			}
			var expectedResult = withCreateRequestWithoutPopUpsConfiguation && !approvalNotAllowedOwnJournal && Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed;
			AssertEquals("CheckLevelSecurityRights", expectedResult, GLJournalApprovalAuthorizationHelper.CheckLevelSecurityRights(journal));
			interactiveSecurityProviderWithApprovalRequestMock.Verify();

			if (!withCreateRequestWithoutPopUpsConfiguation)
			{
				interactiveSecurityProviderWithApprovalRequestMock.Reset();
				interactiveSecurityProviderWithApprovalRequestMock.Setup(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>())).Returns(SecurityCertificate.Granted);
				Assert("CheckLevelSecurityRights", GLJournalApprovalAuthorizationHelper.CheckLevelSecurityRights(journal));
				interactiveSecurityProviderWithApprovalRequestMock.Verify();
			}

			var interactiveSecurityProviderMock = new Mock<IDummyInteractiveSecurityOverrideProvider>();
			SecurityOverrideProviderSource.Get(journal).Provider = interactiveSecurityProviderMock.Object;
			if (approvalNotAllowedOwnJournal)
			{
				interactiveSecurityProviderMock.Setup(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>()));
			}
			else
			{
				return; //can't test with mock SecurityCertificates[requiredCheckPoint]
			}
			Assert("CheckLevelSecurityRights", !GLJournalApprovalAuthorizationHelper.CheckLevelSecurityRights(journal));
			interactiveSecurityProviderMock.Verify();

			interactiveSecurityProviderMock.Setup(m => m.PromptForTemporaryAccess(It.IsAny<SecurityCheckpoint>())).Returns(SecurityCertificate.Granted);
			Assert("CheckLevelSecurityRights", GLJournalApprovalAuthorizationHelper.CheckLevelSecurityRights(journal));
			interactiveSecurityProviderMock.Verify();
		}

		GLJournal CheckLevelSecurityRightSetup(bool withCreateRequestWithoutPopUpsConfiguation)
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);

			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();

			if (withCreateRequestWithoutPopUpsConfiguation)
			{
				threshold.Type = Enterprise.Accounting.Registry.Business.GLJournalApprovalThreshold.TypeCodes.AnyChanges;

				AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
			else
			{
				threshold.Type = GLJournalApprovalThreshold.TypeCodes.All;
				var settings = threshold.AuthorisationSettings.AddNew();
				settings.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
				settings.Amount = 0;
				settings.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			}
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			AssertEquals("Precondition: ShouldCreateApprovalWithoutUsersConfirm", withCreateRequestWithoutPopUpsConfiguation, GLJournalApprovalAuthorizationHelper.ShouldCreateApprovalWithoutUsersConfirm(journal));
			return journal;
		}

		public interface IDummyInteractiveSecurityOverrideProviderWithApprovalRequest : Enterprise.Integration.Security.IInteractiveSecurityOverrideProvider, ISecurityOverrideProviderWithApprovalRequest
		{
		}

		public interface IDummyInteractiveSecurityOverrideProvider : Enterprise.Integration.Security.IInteractiveSecurityOverrideProvider, ISecurityOverrideProvider
		{
		}

		#endregion

		public void TestRequiredSecurityCheckPoint__DontGroupByReportSection_ForSavedJournal()
		{
			header1 = TestObjectCreator.CreateAccGLHeader("1111.11.11", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);
			header2 = TestObjectCreator.CreateAccGLHeader("1111.11.22", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);

			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.ReportSection;
			threshold.ReportSection = AccGLHeader.Constants.SectionTypes.Codes.TradingStatement;

			var settings1 = threshold.AuthorisationSettings.AddNew();
			settings1.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings1.Amount = 100.00m;
			settings1.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			var settings2 = threshold.AuthorisationSettings.AddNew();
			settings2.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings2.Amount = 200.00m;
			settings2.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			var settings3 = threshold.AuthorisationSettings.AddNew();
			settings3.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
			settings3.Amount = 200.00m;
			settings3.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.ExistingGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			var journal = Factory.NewWithValidTestData<GLJournal>();
			var line = journal.GLJournalLines.AddNew();
			line.AL_OSExTaxAmount = 210m;
			line.AL_AG = header1.PK;

			line = journal.GLJournalLines.AddNew();
			line.AL_OSExTaxAmount = -70m;
			line.AL_AG = header1.PK;

			line = journal.GLJournalLines.AddNew();
			line.AL_OSExTaxAmount = -140m;
			line.AL_AG = header2.PK;
			AssertEquals("The registry is not set for new journal, only for saved one.", "None", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);

			journal.Factory.Save();
			AssertEquals("We don't group by report section and still use single GL account grouping", "Second Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);
		}

		public void TestRequiredSecurityCheckPoint_DontGroupByReportSection()
		{
			header1 = TestObjectCreator.CreateAccGLHeader("1111.11.11", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);
			header2 = TestObjectCreator.CreateAccGLHeader("1111.11.22", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);

			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.ReportSection;
			threshold.ReportSection = AccGLHeader.Constants.SectionTypes.Codes.TradingStatement;

			var settings1 = threshold.AuthorisationSettings.AddNew();
			settings1.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings1.Amount = 100.00m;
			settings1.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			var settings2 = threshold.AuthorisationSettings.AddNew();
			settings2.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings2.Amount = 200.00m;
			settings2.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			var settings3 = threshold.AuthorisationSettings.AddNew();
			settings3.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
			settings3.Amount = 200.00m;
			settings3.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			var journal = Factory.NewWithValidTestData<GLJournal>();
			var line = journal.GLJournalLines.AddNew();
			line.AL_AG = header1.PK;
			line.AL_LineAmount = 60m;

			line = journal.GLJournalLines.AddNew();
			line.AL_AG = header1.PK;
			line.AL_LineAmount = 70m;

			line = journal.GLJournalLines.AddNew();
			line.AL_AG = header2.PK;
			line.AL_LineAmount = 80m;
			AssertEquals("We don't group by report section and still use single GL account grouping", "Second Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);
		}

		public void TestRequiredSecurityCheckPoint_GroupingForRegistryFallbackLevel()
		{
			header1 = TestObjectCreator.CreateAccGLHeader("1111.11.11", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);

			Guid branch1PK = Env.CurrentBranch.PK;
			Guid branch2PK = TestObjectCreator.NonCurrentBranch.PK.ToGuid();

			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.All;

			var settings1 = threshold.AuthorisationSettings.AddNew();
			settings1.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings1.Amount = 100.00m;
			settings1.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			var settings2 = threshold.AuthorisationSettings.AddNew();
			settings2.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings2.Amount = 200.00m;
			settings2.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			var settings3 = threshold.AuthorisationSettings.AddNew();
			settings3.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
			settings3.Amount = 200.00m;
			settings3.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;

			var journal = Factory.NewWithValidTestData<GLJournal>();
			var line = journal.GLJournalLines.AddNew();
			line.AL_AG = header1.PK;
			line.AL_LineAmount = 60m;
			line.AL_GB = branch1PK;

			line = journal.GLJournalLines.AddNew();
			line.AL_AG = header1.PK;
			line.AL_LineAmount = 70m;
			line.AL_GB = branch1PK;

			line = journal.GLJournalLines.AddNew();
			line.AL_AG = header1.PK;
			line.AL_LineAmount = 80m;
			line.AL_GB = branch2PK;

			var registryInteranls = (IRegistryItemInternals)AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup;
			AssertEquals("No registry setup", "None", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);

			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Should use amount total for whole journal as company approval level is higher", "Third Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);

			registryInteranls.DeleteValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals("No registry setup", "None", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);

			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Should use amount total for whole journal as approval level with fallback to system level is higher", "Third Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);

			newValue = new GLJournalApprovalThresholdCollection();
			threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.All;

			settings1 = threshold.AuthorisationSettings.AddNew();
			settings1.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings1.Amount = 200.00m;
			settings1.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			settings2 = threshold.AuthorisationSettings.AddNew();
			settings2.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
			settings2.Amount = 200.00m;
			settings2.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Should use amount total for whole journal as only system level is defined", "Third Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);

			settings1.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.NoApprovalRequired;
			settings2.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Should use amount total for whole journal as company level is defined and it has more priority than system level", "First Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);
		}

		public void TestRequiredSecurityCheckPointFallbackLogic()
		{
			SetupTestData();

			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			journal.GLJournalLines.AddNew();
			journal.GLJournalLines.AddNew();

			journal.GLJournalLines[0].AL_PostDate = ZDateTime.Today;
			journal.GLJournalLines[0].AL_OSExTaxAmount = 60m;
			journal.GLJournalLines[0].AL_AG = header1.PK;
			journal.GLJournalLines[0].AL_GB = Env.CurrentBranch.PK;

			journal.GLJournalLines[1].AL_PostDate = ZDateTime.Today;
			journal.GLJournalLines[1].AL_OSExTaxAmount = 70m;
			journal.GLJournalLines[1].AL_AG = header1.PK;
			journal.GLJournalLines[1].AL_GB = Env.CurrentBranch.PK;

			AssertEquals("Should use GLA configuration", "First Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);

			journal.GLJournalLines[0].AL_AG = header2.PK;
			journal.GLJournalLines[1].AL_AG = header2.PK;

			AssertEquals("Should use RSN configuration", "Third Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);

			journal.GLJournalLines[0].AL_AG = header3.PK;
			journal.GLJournalLines[1].AL_AG = header3.PK;

			AssertEquals("Should use ALL configuration", "Second Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);
		}

		public void TestRequiredSecurityCheckPointReturnsTheMaxSecurityLevelFound()
		{
			SetupTestData();

			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			journal.GLJournalLines.AddNew();
			journal.GLJournalLines.AddNew();
			journal.GLJournalLines.AddNew();

			journal.GLJournalLines[0].AL_PostDate = ZDateTime.Today;
			journal.GLJournalLines[0].AL_AG = header1.PK;
			journal.GLJournalLines[0].AL_LineAmount = 101m;
			journal.GLJournalLines[0].AL_GB = Env.CurrentBranch.PK;

			journal.GLJournalLines[1].AL_PostDate = ZDateTime.Today;
			journal.GLJournalLines[1].AL_AG = header2.PK;
			journal.GLJournalLines[1].AL_LineAmount = 101m;
			journal.GLJournalLines[1].AL_GB = Env.CurrentBranch.PK;

			journal.GLJournalLines[2].AL_PostDate = ZDateTime.Today;
			journal.GLJournalLines[2].AL_AG = header3.PK;
			journal.GLJournalLines[2].AL_LineAmount = 101m;
			journal.GLJournalLines[2].AL_GB = Env.CurrentBranch.PK;

			AssertEquals("Should use the maximum security level found across all GL lines", "Third Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);
		}

		public void TestRequiredSecurityCheckPointWithMixedLineBranch()
		{
			SetupTestData();

			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			journal.GLJournalLines.AddNew();
			journal.GLJournalLines.AddNew();

			journal.GLJournalLines[0].AL_PostDate = ZDateTime.Today;
			journal.GLJournalLines[0].AL_AG = header1.PK;
			journal.GLJournalLines[0].AL_LineAmount = 99m;
			journal.GLJournalLines[0].AL_GB = Env.CurrentBranch.PK;

			journal.GLJournalLines[1].AL_PostDate = ZDateTime.Today;
			journal.GLJournalLines[1].AL_AG = header2.PK;
			journal.GLJournalLines[1].AL_LineAmount = 99m;
			journal.GLJournalLines[1].AL_GB = Env.CurrentBranch.PK;

			AssertEquals("Second Level Approval", GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPoint(journal).HumanReadableName);
		}

		public void TestShouldCreateApprovalWithoutUsersConfirm()
		{
			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SetupTestData();
			var originNewThresholdValue = AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.Value;

			var journal = Factory.New<GLJournal>();
			Assert(!GLJournalApprovalAuthorizationHelper.ShouldCreateApprovalWithoutUsersConfirm(journal));

			var newValue = new GLJournalApprovalThresholdCollection();
			var newThreshold = newValue.AddNew();
			newThreshold.Type = Enterprise.Accounting.Registry.Business.GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(!GLJournalApprovalAuthorizationHelper.ShouldCreateApprovalWithoutUsersConfirm(journal));

			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(GLJournalApprovalAuthorizationHelper.ShouldCreateApprovalWithoutUsersConfirm(journal));

			Factory.Save();
			Assert(!GLJournalApprovalAuthorizationHelper.ShouldCreateApprovalWithoutUsersConfirm(journal));

			AccountingConfigurationRegistry.Instance.ExistingGLJournalApprovalThresholdSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, originNewThresholdValue);

			Assert(GLJournalApprovalAuthorizationHelper.ShouldCreateApprovalWithoutUsersConfirm(journal));

			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(!GLJournalApprovalAuthorizationHelper.ShouldCreateApprovalWithoutUsersConfirm(journal));

			var noteJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Now, ZDateTime.Now);
			Assert(!GLJournalApprovalAuthorizationHelper.ShouldCreateApprovalWithoutUsersConfirm(noteJournal));
		}

		void SetupTestData()
		{
			header1 = TestObjectCreator.CreateAccGLHeader("1111.11.11", "OV", "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);
			header2 = TestObjectCreator.CreateAccGLHeader("1111.11.22", "TS", "ACCOUNT 1", Constants.AccountType.ProfitAndLossAccount, Constants.DebitCredit.Credit);
			header3 = TestObjectCreator.CreateAccGLHeader("1111.11.33", "LI", "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);

			GLJournalApprovalThresholdCollection newValue = new GLJournalApprovalThresholdCollection();
			GLJournalApprovalThreshold threshold1 = newValue.AddNew();
			threshold1.Type = "RSN";
			threshold1.ReportSection = "OV";

			var settings1 = threshold1.AuthorisationSettings.AddNew();
			settings1.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings1.Amount = 100.00m;
			settings1.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.NoApprovalRequired;

			var settings2 = threshold1.AuthorisationSettings.AddNew();
			settings2.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
			settings2.Amount = 100.00m;
			settings2.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			GLJournalApprovalThreshold threshold2 = newValue.AddNew();
			threshold2.Type = "GLA";
			threshold2.GLAccount = header2.PK;

			var settings3 = threshold2.AuthorisationSettings.AddNew();
			settings3.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings3.Amount = 100.00m;
			settings3.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			var settings4 = threshold2.AuthorisationSettings.AddNew();
			settings4.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
			settings4.Amount = 100.00m;
			settings4.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;

			GLJournalApprovalThreshold threshold3 = newValue.AddNew();
			threshold3.Type = "ALL";

			var settings5 = threshold3.AuthorisationSettings.AddNew();
			settings5.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.UpTo;
			settings5.Amount = 100.00m;
			settings5.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			var settings6 = threshold3.AuthorisationSettings.AddNew();
			settings6.Range = PaymentThreeLevelAuthorisationSettings.RangeCodes.Above;
			settings6.Amount = 100.00m;
			settings6.AuthorisationRequirement = PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
			AccountingConfigurationRegistry.Instance.ExistingGLJournalApprovalThresholdSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		AccGLHeader header1;
		AccGLHeader header2;
		AccGLHeader header3;
	}
}
