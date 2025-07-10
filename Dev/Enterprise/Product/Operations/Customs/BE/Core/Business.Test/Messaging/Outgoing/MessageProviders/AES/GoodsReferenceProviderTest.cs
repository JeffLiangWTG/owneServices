using System;
using Enterprise.Customs.BE.Business.Declaration;
using CusEntryLine = Enterprise.Customs.BE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class GoodsReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsReferenceProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new GoodsReferenceProvider(null, 1));
	}

	public void TestSequenceNumber()
	{
		AssertEquals(1, Provider.SequenceNumber);
	}

	public void TestDeclarationGoodsItemNumber()
	{
		AssertEquals(2, Provider.DeclarationGoodsItemNumber);
	}

	protected override GoodsReferenceProvider GetProvider()
	{
		var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
		entryLine.CL_LineNumber = 2;
		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
		invoiceLine.JI_CL = entryLine.PK;
		Factory.Save();
		var containerInvLinePivot = invoiceLine.ContainersPivot.AddNew();
		return new GoodsReferenceProvider(containerInvLinePivot, 1);
	}
}
