using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCTOCusHAWBCollection))]
	sealed class DocCTOCusHAWBCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCTOCusHAWBCollection>
	{
		#region Implementation

		protected override DocCTOCusHAWBCollection GetCollectionToTest()
		{
			return new DocCTOCusHAWBCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var hawb = Factory.New<CTOCusHAWB>();
			return DocCTOCusHAWB.New(hawb, Factory);
		}

		#endregion
	}
}
