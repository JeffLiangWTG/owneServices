using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalLineHelper))]
	class GLJournalLineHelperTest : TestCaseWithFactory
	{
		public void TestIsGlAccountConfiguredAsControlOrLinkAccount()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "9999.99.00";

			GLJournalLineHelper.ControlOrLinkAccountsForTest = null;

			Assert("GL Account is not set as Control or Link account", !GLJournalLineHelper.IsGlAccountConfiguredAsControlOrLinkAccount(glHeader.PK.ToGuid()));

			AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());

			Assert("GL Account is set as a link account", GLJournalLineHelper.IsGlAccountConfiguredAsControlOrLinkAccount(glHeader.PK.ToGuid()));

			AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			Assert("GL Account is not set as Control or Link account", !GLJournalLineHelper.IsGlAccountConfiguredAsControlOrLinkAccount(glHeader.PK.ToGuid()));

			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());

			Assert("GL Account is set as a link account", GLJournalLineHelper.IsGlAccountConfiguredAsControlOrLinkAccount(glHeader.PK.ToGuid()));
		}

		public void TestIsGlAccountConfiguredAsPlAppropriationAccount()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "9999.99.00";

			AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());

			Assert(GLJournalLineHelper.IsGlAccountConfiguredAsPlAppropriationAccount(glHeader.PK.ToGuid()));

			AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.DefaultValue);

			Assert(!GLJournalLineHelper.IsGlAccountConfiguredAsPlAppropriationAccount(glHeader.PK.ToGuid()));
		}
	}
}
