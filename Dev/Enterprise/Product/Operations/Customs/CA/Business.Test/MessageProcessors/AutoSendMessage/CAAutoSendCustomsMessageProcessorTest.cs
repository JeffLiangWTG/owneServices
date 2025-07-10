using System;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	abstract class CAAutoSendCustomsMessageProcessorTest : Customs.Business.Testing.AutoSendCustomsMessageProcessorTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			var importTransactionNumber = new TransactionNumberSetting(Factory, "Test", "12345", GlbBranch.CurrentBranch, "IMP", true);
			importTransactionNumber.MinNumber = 10000000;
			importTransactionNumber.NextNumber = 11000000;
			importTransactionNumber.MaxNumber = 19999999;

			var exportTransactionNumber = new TransactionNumberSetting(Factory, "Test", "12345", GlbBranch.CurrentBranch, "EXP", true);
			exportTransactionNumber.MinNumber = 20000000;
			exportTransactionNumber.NextNumber = 21000000;
			exportTransactionNumber.MaxNumber = 29999999;
			Factory.Save();
		}
	}
}
