using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using NUnit.Framework;
using ESCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.ES.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	[TestedType(typeof(ESDocSADHLineCollectionExport))]
	sealed class ESDocSADHLineCollectionExportTest : DocSADHLineCollectionTest<ESDocSADHLineCollectionExport>
	{
		protected override ESDocSADHLineCollectionExport GetNewDocumentWrapperCollection()
		{
			return new ESDocSADHLineCollectionExport(EntryLineCollection, Factory);
		}

		protected override object GetNewObjectToWrap()
		{
			return null;
		}

		protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
		{
			var result = ESDocSADHLineExport.New(EntryLineCollection.AddNew(), Factory);
			collection.Add(result);
			return result;
		}

		new ESCusEntryLineCollection EntryLineCollection
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
		ESCusEntryLineCollection entryHeaderCollection;

		protected override string CountryToUseForTesting => Core.Constants.CountryCodes.Spain;
	}
}
