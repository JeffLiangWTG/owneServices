using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using NUnit.Framework;
using IECusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.IE.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.IE.DocumentWrappers.Testing
{
	[TestedType(typeof(IEDocSADHLineCollection))]
	class IEDocSADHLineCollectionTest : DocSADHLineCollectionTest<IEDocSADHLineCollection>
	{
		protected override IEDocSADHLineCollection GetNewDocumentWrapperCollection()
		{
			return new IEDocSADHLineCollection(EntryLineCollection, Factory);
		}

		protected override object GetNewObjectToWrap()
		{
			return null;
		}

		protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
		{
			var result = IEDocSADHLine.New(EntryLineCollection.AddNew(), Factory);
			collection.Add(result);
			return result;
		}

		protected new IECusEntryLineCollection EntryLineCollection
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
		IECusEntryLineCollection entryHeaderCollection;

		protected override string CountryToUseForTesting => Core.Constants.CountryCodes.Ireland;
	}
}
