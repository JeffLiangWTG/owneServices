using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsDefaultConsignorConsigneeRegistryManagerTest : TestCaseWithFactory
	{
		public void TestIsRegistryEnabled()
		{
			var nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			nctsDefaultConsignorConsignee.LeaveBlank = false;
			nctsDefaultConsignorConsignee.ValueFrom = false;
			EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
			AssertEquals("When LeaveBlank is not ticked and Value From is not ticked, IsRegistryEnabled should be false.", false, defaultConsignorConsigneeRegistryManager.IsRegistryEnabled());

			nctsDefaultConsignorConsignee.LeaveBlank = true;
			nctsDefaultConsignorConsignee.ValueFrom = false;
			EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
			AssertEquals("When LeaveBlank is ticked and Value From is not ticked, IsRegistryEnabled should be true.", true, defaultConsignorConsigneeRegistryManager.IsRegistryEnabled());

			nctsDefaultConsignorConsignee.LeaveBlank = false;
			nctsDefaultConsignorConsignee.ValueFrom = true;
			EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
			AssertEquals("When LeaveBlank is not ticked and Value From is ticked, IsRegistryEnabled should be true.", true, defaultConsignorConsigneeRegistryManager.IsRegistryEnabled());
		}

		public void TestIsBlank()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultConsignorConsignee.LeaveBlank = false;
				EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
				AssertEquals("When LeaveBlank is not ticked, IsBlank should be false.", false, defaultConsignorConsigneeRegistryManager.IsLeaveBlank());

				nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultConsignorConsignee.LeaveBlank = true;
				EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
				AssertEquals("When LeaveBlank is ticked, IsBlank should be true.", true, defaultConsignorConsigneeRegistryManager.IsLeaveBlank());
			});
		}

		public void TestIsConsignee()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultConsignorConsignee.ValueFrom = false;
				EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
				AssertEquals("When Value From is not ticked, IsConsignee should be false.", false, defaultConsignorConsigneeRegistryManager.IsConsignee());

				nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultConsignorConsignee.ValueFrom = true;
				nctsDefaultConsignorConsignee.Consignee = false;
				EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
				AssertEquals("When Value From is not ticked and consignee is not ticked, IsConsignee should be false.", false, defaultConsignorConsigneeRegistryManager.IsConsignee());

				nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultConsignorConsignee.ValueFrom = true;
				nctsDefaultConsignorConsignee.Consignee = true;
				EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
				AssertEquals("When Value From is not ticked and consignee is ticked, IsConsignee should be true.", true, defaultConsignorConsigneeRegistryManager.IsConsignee());
			});
		}

		public void TestIsConsignor()
		{
			CombineAssertions(() =>
			{
				var nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultConsignorConsignee.ValueFrom = false;
				EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
				AssertEquals("When Value From is not ticked, IsConsignee should be false.", false, defaultConsignorConsigneeRegistryManager.IsConsignor());

				nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultConsignorConsignee.ValueFrom = true;
				nctsDefaultConsignorConsignee.Consignor = false;
				EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
				AssertEquals("When Value From is not ticked and consignee is not ticked, IsConsignee should be false.", false, defaultConsignorConsigneeRegistryManager.IsConsignor());

				nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultConsignorConsignee.ValueFrom = true;
				nctsDefaultConsignorConsignee.Consignor = true;
				EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultConsignorConsignee);
				AssertEquals("When Value From is not ticked and consignee is ticked, IsConsignee should be true.", true, defaultConsignorConsigneeRegistryManager.IsConsignor());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			defaultConsignorConsigneeRegistryManager = new NctsDefaultConsignorConsigneeRegistryManager();
		}

		NctsDefaultConsignorConsigneeRegistryManager defaultConsignorConsigneeRegistryManager;
	}
}
