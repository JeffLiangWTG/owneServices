using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class TestGetWrappedObject : TestCaseWithFactory
	{
		public void TestGetWrappedObjectCustomisedShipmentNumbers()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "43102-60011";

			CMREXDRMessage message = Factory.New<CMREXDRMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::EXDR+4D61 2HHD DBG5:001+11'
FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'
NAD+MR+94073687135::95'
RFF+ABO:43102-60011/SYD1::001'
RFF+ACW:EXD'
RFF+AFM:9'
RFF+ED:AAAJ3KTP9'
CNT+5:0001'
UNT+10+000001'".Replace("\r\n", "");

			AssertEquals("Failed to process message", dec, message.GetWrappedObject());
		}
	}
}
