using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempStorageJobHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSJH_ContainerCount()
		{
			var job = Factory.New<CusTempStorageJobHeader>();
			var info = job.SJH_ContainerCountInfo;
			job.SJH_ContainerCount = -5;
			AssertHasErrorContaining(info, MandatoryValidation.ValueCannotBeNegative);
			job.SJH_ContainerCount = ZInt.Zero;
			AssertNoError(info, MandatoryValidation.ValueCannotBeNegative);
		}
	}
}
