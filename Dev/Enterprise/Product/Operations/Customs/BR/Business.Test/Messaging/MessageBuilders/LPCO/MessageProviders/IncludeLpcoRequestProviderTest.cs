using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.LPCO.Testing
{
	class IncludeLpcoRequestProviderTest : TestCaseWithFactory
	{
		public void TestIncludeLpcoRequestProvider()
		{
			var date = ZDateTimeOffset.Now;
			var lpco = Factory.NewWithValidTestData<CusLPCOHeader>();
			lpco.CPH_Number = "E2100000000";
			lpco.CPH_Type = PermitTypeList.Codes.LPC;
			lpco.CPH_RetroactiveDate = date;

			var parent = new LPCOMessageSendingObjectParent(lpco);
			var messageObject = new LPCOMessageSendingObject(parent);
			messageObject.MessageType = LPCOEntryActionCodeList.Codes.ORI;

			var provider = new IncludeLpcoRequestProvider(messageObject);
			AssertEquals(date.ToDateTime(), provider.ReferenceDate);
		}
	}
}
