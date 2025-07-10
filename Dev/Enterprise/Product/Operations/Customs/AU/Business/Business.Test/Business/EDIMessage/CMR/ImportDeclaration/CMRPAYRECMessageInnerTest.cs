using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRPAYRECMessageInnerTest : TestCaseWithFactory
	{
		readonly string validPAYRECMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::PAYREC+2C4B 403F EIFF:1+11'
DTM+138:20050608:102'
NAD+MR+AAA447Y::95'
NAD+CM+66015286036::95'
NAD+IM+11001094097::95'
NAD+VT+AA33JL::95'
NAD+COQ+242200::215'
NAD+AO+123456::215++LEON BALL'
RFF+ABO:BAlklklk/1/SYD::1'
RFF+ABQ:SSA/3020033'
RFF+ADU:1001053'
RFF+ABT:AAAAXNTNW'
RFF+AII:AAAAXNTNW'
RFF+RA:AAAAXNTRY'
TAX+3'
MOA+7:0000000000000.00'
TAX+3'
MOA+23:0000000000029.25'
TAX+3'
MOA+9:0000000000000.00'
TAX+3'
MOA+58:0000000000000.00'
TAX+3'
MOA+149:0000000000000.00'
TAX+3'
MOA+369:0000000000550.00'
TAX+3'
MOA+371:0000000000000.00'
TAX+3'
MOA+26:0000000000002.50'
TAX+3'
MOA+206:0000000000000.00'
TAX+3'
MOA+304:0000000000000.00'
TAX+3'
MOA+128:0000000000581.75'
UNT+37+000001'".Replace("\r\n", "");

		public void TestGetWrappedObject()
		{
			CMRPAYRECMessage message = Factory.New<CMRPAYRECMessage>();

			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BAlklklk/1";
			message.EM_MessageText = validPAYRECMessageText;

			AssertEquals("Wrapped object is found", entryHeader, message.GetWrappedObject());
		}

		public void TestGetReport()
		{
			CMRPAYRECMessage message = Factory.New<CMRPAYRECMessage>();
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BAlklklk/1";
			message.EM_MessageText = validPAYRECMessageText;

			ZString report = message.GetReport();
			AssertEquals("EFT Run Number", true, report.Contains("EFT Run Number"));
			AssertEquals("ICS Receipt Number", true, report.Contains("ICS Receipt Number"));
			AssertEquals("Total Other Charges", true, report.Contains("Total Other Charges"));
			AssertEquals("Total Paid Amount", true, report.Contains("Total Paid Amount"));
			AssertEquals("Total Payable Admin", true, report.Contains("Total Payable Admin"));
			AssertEquals("Total Payable Duty", true, report.Contains("Total Payable Duty"));
			AssertEquals("Total Payable GST", true, report.Contains("Total Payable GST"));
			AssertEquals("Total Payable LCT", true, report.Contains("Total Payable LCT"));
			AssertEquals("Total Payable WET", true, report.Contains("Total Payable LCT"));
			AssertEquals("Total WoodLevy", true, report.Contains("Total WoodLevy"));
			AssertEquals("AQIS Processing Charge", true, report.Contains("AQIS Processing Charge"));
			AssertEquals("Declaration Processing Charge", true, report.Contains("Declaration Processing Charge"));
		}
	}
}
