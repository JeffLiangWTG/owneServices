using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmLoginFailureLog))]
	sealed class StmLoginFailureLogTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return loginFailureLog;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return loginFailureLog;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return loginFailureLog;
		}

		protected override void SetUp()
		{
			base.SetUp();
			loginFailureLog = Factory.New<StmLoginFailureLog>();
			loginFailureLog.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			loginFailureLog.SFL_LoginName = "test@wisetech.com";
		}

		StmLoginFailureLog loginFailureLog;
	}
}
