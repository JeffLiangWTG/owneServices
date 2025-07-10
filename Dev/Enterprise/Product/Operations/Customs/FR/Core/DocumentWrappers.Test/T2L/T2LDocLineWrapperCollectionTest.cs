using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;
using FRCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.FR.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.FR.DocumentWrappers.Transit.Testing;

[TestedType(typeof(T2LDocLineWrapperCollection))]
sealed class T2LDocLineWrapperCollectionTest : DocBaseWrapperCollectionTest<T2LDocLineWrapperCollection>
{
	protected override T2LDocLineWrapperCollection GetNewDocumentWrapperCollection()
	{
		return new T2LDocLineWrapperCollection(EntryLines, Factory, false);
	}

	protected override object GetNewObjectToWrap()
	{
		return null;
	}

	protected override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
	{
		var result = T2LDocLineWrapper.New(EntryLines.AddNew(), Factory, false);
		collection.Add(result);
		return result;
	}

	FRCusEntryLineCollection EntryLines
	{
		get
		{
			if (entryLineCollection == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader.JobComInvoiceLines.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.MergedLines.AddNew();

				entryLineCollection = entryHeader.MergedLines;
			}
			return entryLineCollection;
		}
	}
	FRCusEntryLineCollection entryLineCollection;
}
