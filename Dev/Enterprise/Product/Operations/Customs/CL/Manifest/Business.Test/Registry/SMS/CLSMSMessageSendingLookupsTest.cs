using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class CLSMSMessageSendingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestXtCredentialStatusList()
		{
			AssertType<CLSMSMessageXtCredentialStatusList>(bizObj.Lookups.XtCredentialStatusList);

			AssertSame("Using factory cache.", bizObj.Lookups.XtCredentialStatusList, new CLSMSMessageSending(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory).Lookups.XtCredentialStatusList);
		}

		protected override void SetUp()
		{
			base.SetUp();

			bizObj = new CLSMSMessageSending(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
		}
		CLSMSMessageSending bizObj;
	}
}
