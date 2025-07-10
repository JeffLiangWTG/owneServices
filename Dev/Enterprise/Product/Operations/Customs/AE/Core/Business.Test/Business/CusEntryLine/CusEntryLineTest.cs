using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CusEntryLine))]
class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
{
	public void TestDutyAmountAndVATAmount()
	{
		var entryLine = Factory.New<CusEntryLine>();
		entryLine.Fees.AddOrUpdate(FeeTypeList.Codes.A00, 100.5m);
		entryLine.Fees.AddOrUpdate(FeeTypeList.Codes.B00, 10.5m);
		AssertEquals(100.5m, entryLine.DutyAmount);
		AssertEquals(10.5m, entryLine.GSTVATAmount);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = Factory.New<JobDeclaration>();
		CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
		CusEntryLine line = entryHeader.MergedLines.AddNew();
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		line.Fees.AddNew();
		line.Header.PivotsToContainers.RemoveAndDeleteAll();
		line.Header.PivotsToContainers.GetOrCreatePivotFor(line.Header.Declaration.CusContainers.AddNew());
		return line;
	}

	protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);
}
