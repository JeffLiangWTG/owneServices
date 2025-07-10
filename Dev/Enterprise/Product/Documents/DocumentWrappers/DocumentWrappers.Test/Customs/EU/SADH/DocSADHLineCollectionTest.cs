using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[TestedType(typeof(DocSADHLineCollection))]
	sealed class DocSADHLineCollectionTest : DocSADHLineCollectionTest<DocSADHLineCollection>
	{
		protected override DocSADHLineCollection GetNewDocumentWrapperCollection()
		{
			return new DocSADHLineCollection(EntryLineCollection, Factory);
		}

		protected override string CountryToUseForTesting => Core.Constants.CountryCodes.UnitedKingdom;
	}
}
