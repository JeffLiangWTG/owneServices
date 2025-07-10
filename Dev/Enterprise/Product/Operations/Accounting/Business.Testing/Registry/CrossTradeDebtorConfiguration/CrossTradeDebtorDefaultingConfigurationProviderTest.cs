using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class CrossTradeDebtorDefaultingConfigurationProviderTest : TestCaseWithFactory
	{
		public void TestProvider()
		{
			foreach (var isFunctionalityEnabled in new[] { true, false } )
			{
				using (AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isFunctionalityEnabled))
				{
					var provider = new CrossTradeDebtorDefaultingConfigurationProvider();
					var defaultConfigs = provider.GetConfiguration();
					if (isFunctionalityEnabled)
					{
						AssertEquals(2, defaultConfigs.Count);
						AssertResults(defaultConfigs[0], "ALL", "ALL", true, false, ChargedPartyForCrossTradeJob.Agent);
						AssertResults(defaultConfigs[1], "ALL", "ALL", false, true, ChargedPartyForCrossTradeJob.LocalClient);
					}
					else
					{
						AssertEquals(0, defaultConfigs.Count);
					}

					var configs = new CrossTradeDebtorConfigurationHeader();
					CreateConfig("SHP", "AIR", "PPD", DefaultDebtorList.Codes.CollectBillToParty);
					CreateConfig("QSH", "AIR", "CCX", DefaultDebtorList.Codes.PrepaidBillToParty);
					CreateConfig("ALL", "SEA", "ALL", DefaultDebtorList.Codes.PrepaidBillToParty);

					using (AccountingConfigurationRegistry.Instance.CrossTradeDebtorDefaultingConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configs))
					{
						provider = new CrossTradeDebtorDefaultingConfigurationProvider();
						var retConfigs = provider.GetConfiguration();

						if (isFunctionalityEnabled)
						{
							AssertEquals(3, retConfigs.Count);
							AssertResults(retConfigs[0], "SHP", "AIR", false, true, ChargedPartyForCrossTradeJob.Agent);
							AssertResults(retConfigs[1], "QSH", "AIR", true, false, ChargedPartyForCrossTradeJob.LocalClient);
							AssertResults(retConfigs[2], "ALL", "SEA", true, true, ChargedPartyForCrossTradeJob.LocalClient);
						}
						else
						{
							AssertEquals(0, retConfigs.Count);
						}
					}

					void CreateConfig(string jobType, string mode, string chargePaymentType, string debtor)
					{
						var config = configs.Configurations.AddNew();
						config.JobType = jobType;
						config.Mode = mode;
						config.ChargePaymentType = chargePaymentType;
						config.Debtor = debtor;
					}

					void AssertResults(ICrossTradeDebtorDefaultingConfigurationItem retConfig
											, string expectedJobType, string expectedMode, bool expectedIsCollect, bool expectedIsPrepaid, ChargedPartyForCrossTradeJob expectedBillToParty)
					{
						AssertEquals("JobTypeCode", expectedJobType, retConfig.JobTypeCode);
						AssertEquals("TransportModeCode", expectedMode, retConfig.TransportModeCode);
						AssertEquals("IsCollect", expectedIsCollect, retConfig.IsCollect);
						AssertEquals("IsPrepaid", expectedIsPrepaid, retConfig.IsPrepaid);
						AssertEquals("BillToParty", expectedBillToParty, retConfig.BillToParty);
					}
				}
			}
		}
	}
}
