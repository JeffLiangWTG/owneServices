using System.Linq;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
	class CusEntryHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetDocumentWrappersInternal_DataContextIEEAD()
		{
			var menuItemForTesting = Factory.New<IStmMenuItem>();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var documentSupporter = new CusEntryHeaderDocumentSupporter(entryHeader);
			var wrappers = documentSupporter.GetDocumentWrappers(DataContext.IEEAD, menuItemForTesting);
			AssertContainsExactElementsInAnyOrder("Wrapped Objects", new[] { entryHeader }, wrappers.Select(x => x.WrappedObject));
			AssertEquals("Enterprise.Customs.IE.DocumentWrappers.IEDocEAD", wrappers[0].GetType().FullName);

			wrappers = documentSupporter.GetDocumentWrappers(DataContext.SADH, menuItemForTesting);
			AssertContainsExactElementsInAnyOrder("Wrapped Objects", new[] { entryHeader }, wrappers.Select(x => x.WrappedObject));
			AssertEquals("Enterprise.Customs.IE.DocumentWrappers.IEDocEAD", wrappers[0].GetType().FullName);

			wrappers = documentSupporter.GetDocumentWrappers(DataContext.IEIADClearanceSlip, menuItemForTesting);
			AssertContainsExactElementsInAnyOrder("Wrapped Objects", new[] { entryHeader }, wrappers.Select(x => x.WrappedObject));
			AssertEquals("Enterprise.Customs.IE.DocumentWrappers.IADClearanceSlip", wrappers[0].GetType().FullName);

			wrappers = documentSupporter.GetDocumentWrappers(DataContext.IEImportAccompanyingDocument, menuItemForTesting);
			AssertContainsExactElementsInAnyOrder("Wrapped Objects", new[] { entryHeader }, wrappers.Select(x => x.WrappedObject));
			AssertEquals("Enterprise.Customs.IE.DocumentWrappers.ImportAccompanyingDocumentWrapper", wrappers[0].GetType().FullName);
		}

		public void TestGetFilterValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var supporter = (CusEntryHeaderDocumentSupporter)entryHeader.DocumentSupporter;

			CombineAssertions(() =>
			{
				AssertEquals("IE for DocumentFilters.CTYEGSADH", Core.Constants.CountryCodes.Ireland, supporter.GetFilterValue(DocumentFilters.CTYEGSADH));
				AssertEquals("EUN for DocumentFilters.CTYEG", EconomicGroupList.Codes.EuropeanUnion, supporter.GetFilterValue(DocumentFilters.CTYEG));
				AssertEquals("No exception for DocumentFilters.CTY", Core.Constants.CountryCodes.Ireland, supporter.GetFilterValue(DocumentFilters.CTY));
			});
		}

		public void TestGetSupportedDataContexts()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var documentSupporter = new CusEntryHeaderDocumentSupporter(entryHeader);
			var list = documentSupporter.ListOfSupportedDataContexts;

			CombineAssertions("ListOfSupportedDataContexts", () =>
			{
				Assert("IEEAD", list.ContainsCode(DataContext.IEEAD));
				Assert("IEEAD - Full", list.ContainsCode(DataContext.SADH));
				Assert("IEIADClearanceSlip", list.ContainsCode(DataContext.IEIADClearanceSlip));
				Assert("IEImportAccompanyingDocument", list.ContainsCode(DataContext.IEImportAccompanyingDocument));
			});
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			return entryHeader;
		}
	}
}
