using System;
using Enterprise.Customs.EU.Business.Declaration;
using EUCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	public abstract class DocSADHPageCollectionTest<T> : DocumentWrappers.Testing.DocBaseWrapperCollectionTest<T> where T : DocSADHPageCollection
	{
		protected override object GetNewObjectToWrap()
		{
			return null;
		}
		protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
		{
			DocSADHPage result = DocSADHPage.New(Factory, EntryLineCollection.AddNew());
			collection.Add(result);
			return result;
		}
		protected EUCusEntryLineCollection EntryLineCollection
		{
			get
			{
				if (entryLineCollection == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var entryHeader = declaration.CustomsEntryHeaders.AddNew();
					entryLineCollection = entryHeader.MergedLines;
				}
				return entryLineCollection;
			}
		}
		EUCusEntryLineCollection entryLineCollection;

		IDisposable temporarySetupCountry;
		protected override void SetUp()
		{
			base.SetUp();
			temporarySetupCountry = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(CountryToUseForTesting);
		}

		protected override void TearDown()
		{
			temporarySetupCountry?.Dispose();
			base.TearDown();
		}

		protected abstract string CountryToUseForTesting { get; }
	}
}
