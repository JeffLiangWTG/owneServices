using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using NUnit.Framework;
using ITCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.IT.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(ITDocSADHLineCollection))]
sealed class ITDocSADHLineCollectionTest : DocSADHLineCollectionTest<ITDocSADHLineCollection>
{
	protected override ITDocSADHLineCollection GetNewDocumentWrapperCollection()
	{
		return new ITDocSADHLineCollection(EntryLineCollection, Factory);
	}

	protected override object GetNewObjectToWrap()
	{
		return null;
	}

		protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
		{
			var result = ITDocSADHLine.New(EntryLineCollection.AddNew(), Factory);
			collection.Add(result);
			return result;
		}

	new ITCusEntryLineCollection EntryLineCollection
	{
		get
		{
			if (entryHeaderCollection == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader.JobComInvoiceLines.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.MergedLines.AddNew();

				entryHeaderCollection = entryHeader.MergedLines;
			}
			return entryHeaderCollection;
		}
	}
	ITCusEntryLineCollection entryHeaderCollection;

	protected override string CountryToUseForTesting => Core.Constants.CountryCodes.Italy;
}
