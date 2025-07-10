using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace ZClientEDI.Business.Testing
{
	class EdiProdDbHelperTest : TestCaseWithFactory
	{
		public void TestIsRunningRunsOnEdiProdDatabase_NoCurrentCompanyLicence()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "A.G";
			staff.GS_LoginName = "Anton";
			Factory.Save();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				AssertNull(Env.CurrentCompany?.GetLicenceCode());
				AssertEquals(false, EdiProdDbHelper.IsRunningOnEdiProdDatabase);
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				var code = registrationKey.EnterpriseCode + "XXX" + registrationKey.ServerCode;
				SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, code);
				AssertEquals(true, EdiProdDbHelper.IsRunningOnEdiProdDatabase);
			}
		}
	}
}
