using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryFactoryTest : TestCaseWithFactory
	{
		public void TestInstance()
		{
			AssertEquals("Instance should be cached.", RegistryFactory.Instance, RegistryFactory.Instance);
			AssertNotNull("Instance should not be null.", RegistryFactory.Instance);
		}

		class TestRegistryFactory : RegistryFactory
		{
			public TestRegistryFactory()
			{
			}
		}

		public void TestRefreshEnabled()
		{
			Globals.IsUserInteractive = true;
			AssertEquals("Support refresh bus because maybe the current user updates the registrya.", true, RegistryFactory.Instance.RefreshEnabled);
			Globals.IsUserInteractive = false;
			AssertEquals("Service task probably shouldn't be updating registry items.", false, new TestRegistryFactory().RefreshEnabled);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "This factory cannot be saved.")]
		public void TestSave()
		{
			RegistryFactory.Instance.Save();
		}

		public void TestCanSave()
		{
			AssertEquals("CanSave", false, ((IBusinessObjectFactoryInternals)RegistryFactory.Instance).CanSave);
		}

		public void TestGetGroupPK()
		{
			AssertEquals("GetGroupPK(\"XXX\")", Guid.Empty, RegistryFactory.Instance.GetGroupPK("XXX"));
			BusinessObject group = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbGroup)));
			group[GlbGroupSchema.Constants.GG_Code] = "XXX";
			Factory.Save();
			AssertEquals("GetGroupPK(\"XXX\")", group.PK, RegistryFactory.Instance.GetGroupPK("XXX"));
		}

		public void TestLoad()
		{
			BusinessObject chargeCode = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IAccChargeCode)));
			Factory.Save();
			BusinessObject loadedChargeCode = (BusinessObject)RegistryFactory.Instance.Load<Enterprise.MasterFiles.Integration.IAccChargeCode>(chargeCode.PK);
			AssertEquals("Load() should have loaded the correct BusinessObject.", chargeCode.PK, loadedChargeCode.PK);
		}
	}
}
