using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class StatusRequestInterchangeSenderProviderTest : TestCaseWithFactory
	{
		public void TestEoriNumber_Empty() => AssertEquals(string.Empty, provider.EoriNumber);

		public void TestEoriBranchSuffix_Empty() => AssertEquals(string.Empty, provider.EoriBranchSuffix);

		public void TestEoriNumber()
		{
			using (DECustomsDataRegistry.Instance.ATLASEORINumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "GR1234"))
			{
				AssertEquals("GR1234", provider.EoriNumber);
			}
		}

		public void TestEoriBranchSuffix_Branch()
		{
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			{
				AssertEquals("0001", provider.EoriBranchSuffix);
			}
		}

		public void TestEoriBranchSuffix_Company()
		{
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "0002"))
			{
				AssertEquals("0002", provider.EoriBranchSuffix);
			}
		}

		public void TestTCUNumber()
		{
			AssertNull(provider.TCUNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new StatusRequestInterchangeSenderProvider();
		}
		IPartyID provider;
	}
}
