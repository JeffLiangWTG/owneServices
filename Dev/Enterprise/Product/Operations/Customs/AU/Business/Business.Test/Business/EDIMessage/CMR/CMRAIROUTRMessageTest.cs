using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRAIROUTRMessage))]
	sealed class CMRAIROUTRMessageTest : CMRCUSRESMessageTest
	{
		public void TestOutturnResponseMatchesWithStandAloneUnderbond()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "P026P";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;

			CMRUBMREQRMessage uBMREQRMessage = Factory.New<CMRUBMREQRMessage>();
			uBMREQRMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+402G DF4F 6CF6:1+32'
DTM+9:20060113152512156441:ZZZ'
DTM+132:20060113:102'
FTX+AAH+++FFK334E00000000696872'
TDT+20+001++6+OS::3'
TDT+1++AIR'
LOC+5+EF33J::95'
LOC+4+P026P::95'
NAD+MR+FGF697C::95'
NAD+UD+63050415668::95'
RFF+ABO:00000000696872::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++AIR:67:95'
PAC+0000002'
RFF+MWB:25771563030'
UNT+19+000001'".Replace("\r\n", "");
			uBMREQRMessage.SetEM_LinkedObject();
			Factory.Save();

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+4CBA CJC5 6CF6:1+8'
DTM+9:20060113152516490147:ZZZ'
DTM+132:20060113:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+001++6+OS::3'
LOC+12+AUSYD::6'
LOC+4+P026P::95'
NAD+MR+FGF697C::95'
NAD+UD+63050415668::95'
RFF+ABO:00000000696144::1'
RFF+MWB:25771563030'
DOC+1'
PAC+0000002'
UNT+16+000001'".Replace("\r\n", "");
			message.SetEM_LinkedObject();
			Factory.Save();

			var underbond1 = Factory.LoadTop1<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_MAWB, "25771563030"));
			CusOutturn outturn = underbond1.Outturns.AddNew();
			underbond1.C4_SendersMessageReference = "U00020203";
			outturn.C5_PackagesOutturned = 2;
			Factory.Save();

			CMRAIROUTRMessage airOutMessage = Factory.New<CMRAIROUTRMessage>();
			airOutMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::AIROUTR+1I9B 2DHC 97F6:001+11'
NAD+MR+FGF697C::95'
RFF+ACW:AIROUT'
RFF+AFM:9'
RFF+ABO:U00020203/BNE1::001'
DTM+310:20060115083841:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			airOutMessage.SetEM_LinkedObject();
			Factory.Save();

			var underbond = Factory.LoadTop1<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_MAWB, "25771563030"));
			AssertNotNull(underbond);
			AssertEquals(3, underbond.Messages.Count);
			AssertEquals(1, underbond.Outturns.Count);
		}
	}
}
