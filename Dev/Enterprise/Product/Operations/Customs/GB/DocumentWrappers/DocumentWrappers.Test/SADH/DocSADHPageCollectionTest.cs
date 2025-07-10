using NUnit.Framework;

namespace Enterprise.Customs.GB.DocumentWrappers.Testing
{
	[TestedType(typeof(DocSADHPageCollection))]
	class DocSADHPageCollectionTest : Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHPageCollectionTest<DocSADHPageCollection>
	{
		protected override DocSADHPageCollection GetNewDocumentWrapperCollection() => new(EntryLineCollection, Factory);

		protected override string CountryToUseForTesting => Core.Constants.CountryCodes.UnitedKingdom;
	}
}
