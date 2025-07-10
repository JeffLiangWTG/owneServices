using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecIMPFeeDataProviderTest : TestCaseWithFactory
{
	public void TestFeeType()
	{
		feeType = EdecIMPFeeDataProvider.NewCollection(entryHeader.AllEntryLines.AddNew());
		AssertEquals("fee items", 0, feeType.Count());

		var additionalFee1 = invoiceLine.AdditionalFees.AddNew();
		additionalFee1.BZ_Tariff = "150";
		additionalFee1.BZ_Qty1 = 1;
		additionalFee1.BZ_ManualRate = 2;
		var additionalFee2 = invoiceLine.AdditionalFees.AddNew();
		additionalFee2.BZ_Tariff = "151";
		additionalFee2.BZ_Qty1 = 3;
		additionalFee2.BZ_ManualRate = 4;

		feeType = EdecIMPFeeDataProvider.NewCollection(entryLine);

		CombineAssertions(() =>
		{
			AssertEquals("fee items", 2, feeType.Count());

			AssertEquals("fee 1 type", "150", feeType.ElementAt(0).Type);
			AssertEquals("fee 1 quantity", 1m, feeType.ElementAt(0).Quantity);
			AssertEquals("fee 1 rate", 2m, feeType.ElementAt(0).Rate);
			AssertEquals("fee 2 type", "151", feeType.ElementAt(1).Type);
			AssertEquals("fee 2 quantity", 3m, feeType.ElementAt(1).Quantity);
			AssertEquals("fee 2 rate", 4m, feeType.ElementAt(1).Rate);
		});
	}

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		instruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
	}
	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryInstruction instruction;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
	IEnumerable<IEdecFee> feeType;
}
