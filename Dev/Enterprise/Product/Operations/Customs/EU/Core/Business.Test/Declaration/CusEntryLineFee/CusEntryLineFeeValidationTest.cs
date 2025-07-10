using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusEntryLineFeeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckNationalFeeTypeCode()
		{
			(JobDeclaration declaration, CusEntryLine entryLine, CusEntryLineFee entryLineFee) = SetEntryLineFeeData();
			if (entryLineFee.Lookups.NationalFeeTypeCodeList.Count > 0)
			{
				entryLineFee.NationalFeeTypeCode = ZString.Empty;
				AssertNoMessageErrorContaining(entryLineFee.NationalFeeTypeCodeInfo, "The code you have selected is not in the list");

				entryLineFee.NationalFeeTypeCode = entryLineFee.Lookups.NationalFeeTypeCodeList[0].Code;
				AssertNoMessageErrorContaining(entryLineFee.NationalFeeTypeCodeInfo, "The code you have selected is not in the list");

				entryLineFee.NationalFeeTypeCode = "XXXX";
				AssertHasMessageErrorContaining(entryLineFee.NationalFeeTypeCodeInfo, "The code you have selected is not in the list");
			}
			else
			{
				entryLineFee.NationalFeeTypeCode = ZString.Empty;
				AssertEquals(false, entryLineFee.NationalFeeTypeCodeInfo.HasNotifications());
				entryLineFee.NationalFeeTypeCode = "XXXX";
				AssertEquals(false, entryLineFee.NationalFeeTypeCodeInfo.HasNotifications());
			}
		}

		(JobDeclaration, CusEntryLine, CusEntryLineFee) SetEntryLineFeeData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.Cast<CusEntryLine>().First();
			var entryLineFee = entryLine.Fees.AddNew();
			return (declaration, entryLine, entryLineFee);
		}
	}
}
