using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(JobDocAddressesModule))]
	sealed class JobDocAddressesModuleTest : ZModuleBasherTest
	{
		public void TestCheckpoints()
		{
			using (JobDocAddressesModule module = new JobDocAddressesModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.CAJobDocAddresses, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestGetNewFilterControl()
		{
			using (JobDocAddressesModuleForTest module = new JobDocAddressesModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is JobDocAddressesFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (JobDocAddressesModuleForTest module = new JobDocAddressesModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is JobDocAddressCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (JobDocAddressesModuleForTest module = new JobDocAddressesModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is JobDocAddressesFilterBusinessObject);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CAJobDocAddresses;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;

		sealed class JobDocAddressesModuleForTest : JobDocAddressesModule
		{
			public JobDocAddressesModuleForTest()
			{
			}

			public IFilterControl NewFilterControl => GetNewFilterControl();

			public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

			public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();
		}
	}
}
