using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class ZWebModuleFactoryTest : ZModuleFactoryTest
	{
		#region setup

		protected override ModuleIdentifier NAModuleID
		{
			get { return WebModuleIDs.NotAssigned; }
		}

		protected override IEnumerable<ModuleIdentifier> GetModuleIDs()
		{
			return WebModuleIDs.All;
		}

		#endregion

		public void TestCreateZWebModule()
		{
			using (ZWebModule module = ZWebModuleFactory.Create(WebModuleIDs.Dummy, new BusinessObjectFactory()))
			{
				AssertNotNull(module);
				AssertEquals(WebModuleIDs.Dummy, module.ID);
				AssertNotNull("Should be registered with empty Country Code", ZWebModuleFactory.InstanceInternal.RegistrationListInternal[WebModuleIDs.Dummy, "", false]);
				AssertNull("Should not be registered with the Current Company's Country Code", ZWebModuleFactory.InstanceInternal.RegistrationListInternal[WebModuleIDs.Dummy, Enterprise.Environment.Env.CurrentCompany.Country.Code, false]);
			}
		}

		public void TestCreateZFilterGridModule()
		{
			using (ZFilterGridModule filterGridModule = ZWebModuleFactory.Create(WebModuleIDs.Dummy, new BusinessObjectFactory(), null))
			{
				AssertNotNull(filterGridModule);
				AssertEquals(WebModuleIDs.Dummy, filterGridModule.ID);
				AssertNotNull("Should be registered with empty Country Code", ZWebModuleFactory.InstanceInternal.RegistrationListInternal[WebModuleIDs.Dummy, "", false]);
				AssertNull("Should not be registered with the Current Company's Country Code", ZWebModuleFactory.InstanceInternal.RegistrationListInternal[WebModuleIDs.Dummy, Enterprise.Environment.Env.CurrentCompany.Country.Code, false]);
			}
		}
	}
}
