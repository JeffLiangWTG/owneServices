using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRSACMessage))]
	public class CMRSACMessageTest : CMRCUSDECMessageTest
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CMRSACMessage result = (CMRSACMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CMRSACMessage>();
		}

		protected override ZString GetSampleWithdrawMessage()
		{
			return sACSampleWithdrawMessage;
		}

		readonly string sACSampleWithdrawMessage = @"UNH+1+CUSDEC:D:99B:UN'
BGM+929:::SAC+B00148623/9/SYD1:3+50'
FII+COQ+323232+:::242200::215'
FTX+CHG+++WITHDRAW - NO ANSWER TO Q15 - I DO NOT KNOW WHAT THE HELL IS WITH Q15!'
FTX+ACD+++0014:N'
RFF+AMG:0012'
RFF+ABT:AAAA9LR3H'
RFF+ANU:B'
UNS+D'
UNS+S'
UNT+11+1'".Replace("\r\n", "");
	}
}
