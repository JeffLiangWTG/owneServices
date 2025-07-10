using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[TestedType(typeof(DocSADHPageCollection))]
	sealed class DocSADHPageCollectionTest : DocSADHPageCollectionTest<DocSADHPageCollection>
	{
		protected override DocSADHPageCollection GetNewDocumentWrapperCollection()
		{
			return new DocSADHPageCollection(EntryLineCollection, Factory);
		}

		protected override string CountryToUseForTesting => Core.Constants.CountryCodes.UnitedKingdom;
	}
}
