using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CustomsValidationTest : TestCaseWithFactory
	{
		public void TestErrorString()
		{
			AssertEquals("This is a key value for messaging. Please withdraw the message before changing this value or revert to the previous value (@)", CustomsValidation.ErrorString);
		}

		public void TestErrorOnKeyDataWithNoChildren()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			mawb.CM_MAWB = "79846546";
			var hawbNumber = "62390123";
			hawb.CS_HAWB = hawbNumber;
			hawb.CMRMessageStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Factory.Save();
			hawb.CS_HAWB = "123456";
			using (hawb.SuspendValidationTesting())
			{
				new CustomsValidation(hawb.CS_HAWBInfo).ErrorOnKeyDataWithNoChildren(hawb.CMRMessageStatus.Code);
				Assert("Should have this error", hawb.CS_HAWBInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", hawbNumber)));
			}
		}

		public void TestErrorOnKeyData()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var mawbNumber = "62390123";
			mawb.CM_MAWB = mawbNumber;
			hawb.CS_HAWB = "23242";
			hawb.CMRMessageStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Factory.Save();
			mawb.CM_MAWB = "123456";
			using (mawb.SuspendValidationTesting())
			{
				new CustomsValidation(mawb.CM_MAWBInfo).ErrorOnKeyData(mawb);
				Assert("Should have this error", mawb.CM_MAWBInfo.GetErrors().Contains(CustomsValidation.ErrorString.Replace("@", mawbNumber)));
			}
		}
	}
}
