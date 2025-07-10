using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentScanning.Business.Test
{
	internal class StorageMainValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateSM_Type()
		{
			StorageMain parentMain = (new DocumentFactoryProvider().GetFactory(Factory)).New<StorageMain>();

			parentMain.SM_Type = "AAA";
			Assert("Type is not from the list, error expected", parentMain.SM_TypeInfo.HasErrors());

			parentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Assert("Type is valid, no error expected", !parentMain.SM_TypeInfo.HasErrors());
		}

		public void TestValidateSM_PhysicalLocation_SupportNonWesternLanguage()
		{
			var parentMain = (new DocumentFactoryProvider().GetFactory(Factory)).New<StorageMain>();
			parentMain.SM_PhysicalLocation = "中文测试";

			Assert("Chinese is supported, no error expected", !parentMain.SM_TypeInfo.HasErrors());
		}
	}
}
