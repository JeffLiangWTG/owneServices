using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NonPersistentJobTypeOptionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJobType()
		{
			var jobTypeOption = new NonPersistentJobTypeOption(Factory);
			ValidationTestHelper.AssertErrorIfNotEntered(jobTypeOption.JobTypeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(jobTypeOption.JobTypeInfo, "XX", CusInBondApplicationCodeList.Codes.NCTS4);
		}
	}
}
