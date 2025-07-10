using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class DocPAYRECInfoProviderTest : DocD99BCUSRESInfoProviderTest
	{
		public void TestBankDetails()
		{
			var pAYRECMessage = Factory.New<CMRPAYRECMessage>();
			pAYRECMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::PAYREC+26H1 FG9F 2F6F:1+11'DTM+138:20051108:102'NAD+MR+AAA374M::95'NAD+CM+66015286036::95'NAD+IM+12090995252::95'NAD+VT+AA33HF::95'NAD+COQ+242200::215'NAD+AO+323232::215++TEST ACCOUNT NAME:MORE ACCOUNT NAME'RFF+ABO:B00148665/1/SYD1::1'RFF+ABQ:CATE TEST PARTS TRAN'RFF+ADU:B00148665/1 CATE TES'RFF+ABT:AAAA9XNCG'RFF+RA:AAAA9XNFH'TAX+3'MOA+7:0000000000000.00'TAX+3'MOA+23:0000000000030.10'TAX+3'MOA+9:0000000000825.00'TAX+3'MOA+58:0000000000000.00'TAX+3'MOA+149:0000000000000.00'TAX+3'MOA+369:0000000001765.50'TAX+3'MOA+371:0000000000000.00'TAX+3'MOA+26:0000000000006.50'TAX+3'MOA+206:0000000000000.00'TAX+3'MOA+304:0000000000000.00'TAX+3'MOA+128:0000000002627.10'UNT+37+000001'";
			AssertNotNull("PreCondition:Edifact message", pAYRECMessage.CUSRESEdifactMessageForTestOnly);

			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYRECMessage.CUSRESEdifactMessageForTestOnly);
			AssertEquals("PreCondition:BSB number", "242200", infoProvider.BSBNumber);
			AssertEquals("PreCondition:Account Number", "323232", infoProvider.BankAccountNumber);
			AssertEquals("PreCondition:Account Name", "TEST ACCOUNT NAME MORE ACCOUNT NAME", infoProvider.BankAccountName);

			DocPAYRECInfoProvider wrapper = DocPAYRECInfoProvider.New(infoProvider, Factory);
			AssertEquals("BSB number from Wrapper", "242200", wrapper.BSBNumber);
			AssertEquals("Account Number from Wrapper", "323232", wrapper.BankAccountNumber);
			AssertEquals("Account Name from Wrapper", "TEST ACCOUNT NAME MORE ACCOUNT NAME", wrapper.BankAccountName);
		}

		public void TestCharges()
		{
			CUSRESMessage pAYREC = new CUSRESMessage();
			PAYRECInfoProvider infoProvider = new PAYRECInfoProvider(pAYREC);
			DocPAYRECInfoProvider wrapper = DocPAYRECInfoProvider.New(infoProvider, Factory);
			AssertEquals("Charges count", 11, wrapper.Charges.Count);
			AssertEquals("Charges contains Total Payable Admin", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.TotalPayableAdmin));
			AssertEquals("Charges contains AQIS Container Charge", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.AQISContainerCharges));
			AssertEquals("Charges contains AQISProcessingCharge", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.AQISProcessingCharge));
			AssertEquals("Charges contains DeclarationProcessingCharge", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.DeclarationProcessingCharge));
			AssertEquals("Charges contains Woodlevy", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.Woodlevy));
			AssertEquals("Charges contains AQISServicePaymentAmount", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.AQISServicePaymentAmount));
			AssertEquals("Charges contains DutyAmount", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.DutyAmount));
			AssertEquals("Charges contains WetAmount", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.WetAmount));
			AssertEquals("Charges contains LCTAmount", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.LCTAmount));
			AssertEquals("Charges contains GSTAmount", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.GSTAmount));
			AssertEquals("Charges contains OtherCharges", ZBool.True, ChargeItemFound(wrapper.Charges, CusEntryChargeTypeList.Descriptions.OtherCharges));
		}

		ZBool ChargeItemFound(DocCMRMessageChargeItemCollection coll, ZString chargeType)
		{
			foreach (DocCMRMessageChargeItem chargeItem in coll)
			{
				if (chargeItem.ChargeType == chargeType)
				{
					return ZBool.True;
				}
			}

			return ZBool.False;
		}

		protected override D99BCUSRESInfoProvider GetInfoProvider(CUSRESMessage cUSRES)
		{
			return new PAYRECInfoProvider(cUSRES);
		}

		protected override DocD99BCUSRESInfoProvider GetWrapper(D99BCUSRESInfoProvider infoProvider, BusinessObjectFactory factory)
		{
			return DocPAYRECInfoProvider.New((PAYRECInfoProvider)infoProvider, factory);
		}
	}
}
