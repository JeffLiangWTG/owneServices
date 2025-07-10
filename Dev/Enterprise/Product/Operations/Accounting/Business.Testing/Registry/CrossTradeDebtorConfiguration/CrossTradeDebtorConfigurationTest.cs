using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CrossTradeDebtorConfiguration))]
	public class CrossTradeDebtorConfigurationTest : ChargeGroupSettingTest
	{
		public void TestCrossTradeDebtorConfigurationDefaultValue()
		{
			var configuration = new CrossTradeDebtorConfiguration();

			AssertEquals("OTH", configuration.DirectionCode);
		}

		public void TestCrossTradeDebtorConfigurationDirection_ReadOnly()
		{
			var configuration = new CrossTradeDebtorConfiguration();
			Assert(configuration.DirectionCode_ReadOnly);
		}

		public void TestCrossTradeDebtorConfigurationMode_ReadOnly()
		{
			var configuration = new CrossTradeDebtorConfiguration();
			Assert(!configuration.Mode_ReadOnly);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new CrossTradeDebtorConfiguration();

			result.JobType = "ALL";
			result.Mode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			result.ChargePaymentType = PrepaidCollectFreightForwardingList.Codes.All;
			result.Debtor = DefaultDebtorList.Codes.PrepaidBillToParty;

			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBusinessObjectToClone();
		}
	}
}
