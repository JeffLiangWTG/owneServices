using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineColsDirectionCollection))]
	sealed class QuarantineColsDirectionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			return new QuarantineColsDirectionCollection(colsHeader);
		}
	}
}
