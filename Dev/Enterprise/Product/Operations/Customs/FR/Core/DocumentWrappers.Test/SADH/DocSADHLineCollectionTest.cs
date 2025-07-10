using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using NUnit.Framework;
using FRCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.FR.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH.Testing;

[TestedType(typeof(DocSADHLineCollection))]
sealed class DocSADHLineCollectionTest : DocSADHLineCollectionTest<DocSADHLineCollection>
{
	protected override DocSADHLineCollection GetNewDocumentWrapperCollection()
	{
		return new DocSADHLineCollection(EntryLineCollection, Factory);
	}

	protected override object GetNewObjectToWrap()
	{
		return null;
	}

	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var result = DocSADHLine.New(EntryLineCollection.AddNew(), Factory);
		collection.Add(result);
		return result;
	}

	new FRCusEntryLineCollection EntryLineCollection
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
	FRCusEntryLineCollection entryHeaderCollection;

	protected override string CountryToUseForTesting => Core.Constants.CountryCodes.France;
}
