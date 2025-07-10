using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GroupInvoiceChargeLookupsTest : TestCaseWithFactory
	{
		public void TestGroupInvoiceChargeListByValuationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var groupHeader = declaration.TopGroupInvoice;
			var groupHeaderCharge = groupHeader.Charges.AddNew();

			AssertChargeTypeList(new ImportChargeMethodOneCodeList().CodesAsString, [ValuationCodeList.Codes.MethodOne, ValuationCodeList.Codes.MethodOne]);
			AssertChargeTypeList(new ImportChargeMethodTwoAndThreeCodeList().CodesAsString, [ValuationCodeList.Codes.MethodTwo, ValuationCodeList.Codes.MethodTwo, ValuationCodeList.Codes.MethodThree]);
			AssertChargeTypeList(new ImportChargeMethodFourCodeList().CodesAsString, [ValuationCodeList.Codes.MethodFourA, ValuationCodeList.Codes.MethodFourB, ValuationCodeList.Codes.MethodFourB]);
			AssertChargeTypeList(new ImportChargeMethodFiveAndSixCodeList().CodesAsString, [ValuationCodeList.Codes.MethodFive, ValuationCodeList.Codes.MethodSix]);

			AssertChargeTypeList(string.Empty, [ValuationCodeList.Codes.MethodOne, ValuationCodeList.Codes.MethodTwo]);
			AssertChargeTypeList(string.Empty, [ValuationCodeList.Codes.MethodOne, ValuationCodeList.Codes.MethodFourA]);
			AssertChargeTypeList(string.Empty, [ValuationCodeList.Codes.MethodOne, ValuationCodeList.Codes.MethodFive]);
			AssertChargeTypeList(string.Empty, [ValuationCodeList.Codes.MethodTwo, ValuationCodeList.Codes.MethodFourB]);
			AssertChargeTypeList(string.Empty, [ValuationCodeList.Codes.MethodTwo, ValuationCodeList.Codes.MethodThree, ValuationCodeList.Codes.MethodFourA]);
			AssertChargeTypeList(string.Empty, [ValuationCodeList.Codes.MethodThree, ValuationCodeList.Codes.MethodSix]);
			AssertChargeTypeList(string.Empty, [ValuationCodeList.Codes.MethodFourB, ValuationCodeList.Codes.MethodFive]);

			void AssertChargeTypeList(string codeList, string[] invoiceValuationCodes)
			{
				declaration.Invoices.RemoveAll();
				for (int i = 0; i < invoiceValuationCodes.Length; i++)
				{
					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_ValuationCode = invoiceValuationCodes[i];
				}
				AssertEquals(codeList, groupHeaderCharge.Lookups.ChargeTypeList.CodesAsString);
			}
		}
	}
}
