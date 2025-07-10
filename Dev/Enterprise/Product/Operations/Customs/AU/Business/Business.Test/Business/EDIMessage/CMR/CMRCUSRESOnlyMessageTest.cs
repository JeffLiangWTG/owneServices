using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRCUSRESOnlyMessageTest : TestCaseWithFactory
	{
		public void TestGetWrappedObjectUsesEntryNumberToMatch()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00148283";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00148283/1";
			entryHeader.EntryNumber = "XYZ";

			string validCusresMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+28HJ E7EH 7DB5:1+11'
DTM+138:20050829:102'
NAD+MR+AAA374M::95'
NAD+CM+66015286036::95'
NAD+IM+51006765546::95'
NAD+VT+AA33HF::95'
NAD+COQ+242200::215'
NAD+AO+323232::215++DEBORAH SPAGARINO TEST ACCOUNT'
RFF+ABO:B00148283/1/SYD2::2'
RFF+ABQ:OWNER REF'
RFF+ADU:B00148283'
RFF+ABT:AAAA3YMJT'
RFF+RA:AAAA3YML6'
TAX+3'
MOA+7:0000000000000.00'
TAX+3'
MOA+23:0000000000030.10'
TAX+3'
MOA+9:0000000000050.00'
TAX+3'
MOA+58:0000000000000.00'
TAX+3'
MOA+149:0000000000000.00'
TAX+3'
MOA+369:0000000000155.00'
TAX+3'
MOA+371:0000000000000.00'
TAX+3'
MOA+26:0000000000006.50'
TAX+3'
MOA+206:0000000000000.00'
TAX+3'
MOA+304:0000000000000.00'
TAX+3'
MOA+128:0000000000241.60'
UNT+37+000001'".Replace("\r\n", "");

			CMRCUSRESMessage msg = Factory.New<CMRCUSRESMessage>();
			msg.EM_MessageText = validCusresMessage;

			msg.SetEM_LinkedObject();
			AssertNull(msg.EM_LinkedObject);
			entryHeader.EntryNumber = "";
			msg.SetEM_LinkedObject();
			AssertEquals(entryHeader, msg.EM_LinkedObject);
			ErrorReporter.Clear();
		}
	}
}
