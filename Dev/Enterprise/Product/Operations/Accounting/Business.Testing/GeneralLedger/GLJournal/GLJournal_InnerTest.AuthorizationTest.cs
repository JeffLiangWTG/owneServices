using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournal;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	public partial class GLJournal_InnerTest
	{
		public void TestIsLevelAuthorizationRequired()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Now, ZDateTime.Now);
			AssertEquals("IsLevelAuthorizationRequired should be false for Note Journal", false, journal.IsLevelAuthorizationRequired);

			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;

			var anyThresholdSettings = new GLJournalApprovalThresholdCollection();
			var anyThresholdSetting = anyThresholdSettings.AddNew();
			anyThresholdSetting.Type = Enterprise.Accounting.Registry.Business.GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			using (AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, anyThresholdSettings))
			{
				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
				AssertEquals("IsLevelAuthorizationRequired should be true when security 'GeneralLedgerJournal_FirstApproval' is false.", true, journal.IsLevelAuthorizationRequired);

				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
				AssertEquals("IsLevelAuthorizationRequired should be false when security 'GeneralLedgerJournal_FirstApproval' is true.", false, journal.IsLevelAuthorizationRequired);

				using (AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("IsLevelAuthorizationRequired should be true when registry 'AllowUsersToApproveOwnGLJournals' is false.", true, journal.IsLevelAuthorizationRequired);
				}

				using (AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("IsLevelAuthorizationRequired should be false when registry 'AllowUsersToApproveOwnGLJournals' is true.", false, journal.IsLevelAuthorizationRequired);
				}
			}
		}

		public void TestGetLineAuthorisationRequiredType()
		{
			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Now, ZDateTime.Now);
			var line = journal.GLJournalLines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			AssertEquals("Note Journal line should have authorisation rights", AuthorisationRequiredType.HasAuthorisationRights, journal.GetLineAuthorisationRequiredType(line));

			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			AssertEquals(AuthorisationRequiredType.NoAuthorisationRequired, journal.GetLineAuthorisationRequiredType(line));

			var anyThresholdSettings = new GLJournalApprovalThresholdCollection();
			var anyThresholdSetting = anyThresholdSettings.AddNew();
			anyThresholdSetting.Type = Enterprise.Accounting.Registry.Business.GLJournalApprovalThreshold.TypeCodes.AnyChanges;

			using (AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, anyThresholdSettings))
			using (AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("GL Journal should have authorization required", true, journal.IsLevelAuthorizationRequired);

				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
				AssertEquals("GL Journal line should have authorisation required when registry 'AllowUsersToApproveOwnGLJournals' is false.", AuthorisationRequiredType.AuthorisationRequired, journal.GetLineAuthorisationRequiredType(line));

				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
				AssertEquals("GL Journal line should have authorisation required when registry 'AllowUsersToApproveOwnGLJournals' is false.", AuthorisationRequiredType.AuthorisationRequired, journal.GetLineAuthorisationRequiredType(line));
			}

			using (AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
				AssertEquals("GL Journal line should have authorisation required when security 'GeneralLedgerJournal_FirstApproval' is false.", AuthorisationRequiredType.AuthorisationRequired, journal.GetLineAuthorisationRequiredType(line));

				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
				AssertEquals("GL Journal line should have authorisation Rights when security 'GeneralLedgerJournal_FirstApproval' is true.", AuthorisationRequiredType.HasAuthorisationRights, journal.GetLineAuthorisationRequiredType(line));
			}
		}
	}
}
