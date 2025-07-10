using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class NotifyCustomsOfficeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCY_Data()
	{
		RefCusCodeTestHelper.CreateNotifyCustomsOfficeList(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(NotifyCustomsOffice.CY_DataInfo, RefCusCodeTestHelper.InvalidNotifyCustomsOfficeCode, RefCusCodeTestHelper.ValidNotifyCustomsOfficeCode);
	}

	NotifyCustomsOffice NotifyCustomsOffice => notifyCustomsOffice ?? (notifyCustomsOffice = Factory.New<NotifyCustomsOffice>());
	NotifyCustomsOffice notifyCustomsOffice;
}
