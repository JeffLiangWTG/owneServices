using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
class CusEntryHeaderDocumentSupporterTest : DocumentSupporterTest
{
	public void TestGetBODocDataProviders_UTB()
	{
		AssertGetBODocDataProviders(CusEntryHeaderDocumentSupporter.UTBDocument);
	}

	public void TestGetBODocDataProviders_ReleaseDocument()
	{
		AssertGetBODocDataProviders(CusEntryHeaderDocumentSupporter.ReleaseDocument);
	}

	public void TestCusEntryHeaderGetsTheRightDocumentSupporter()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertType<CusEntryHeader>(entryHeader);
		AssertType<CusEntryHeaderDocumentSupporter>(entryHeader.DocumentSupporter);

		var docSupporter = (CusEntryHeaderDocumentSupporter)entryHeader.DocumentSupporter;
		AssertEquals(true, docSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CusEntryHeader)));
		AssertEquals(1, docSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null).Length);
	}

	public void AssertGetBODocDataProviders(string fullDataContext)
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.ActiveEntryHeaders.AddNew();
		var providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(fullDataContext), null);
		AssertEquals($"Provider for {fullDataContext}", 1, providers.Length);
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
	{
		var dec = Factory.New<JobDeclaration>();
		var inv = dec.Invoices.AddNew();
		var invLine = inv.InvoiceLines.AddNew();
		var ceh = dec.CustomsEntryHeaders.AddNew();
		var cl = ceh.MergedLines.AddNew();
		invLine.JI_CL = cl.PK;
		return ceh;
	}
}
