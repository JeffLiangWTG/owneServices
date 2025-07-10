using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(TallyOutturnHeaderOutturnCollection))]
	public class TallyOutturnHeaderOutturnCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTypeOfElements()
		{
			AssertEquals(typeof(TallyOutturn), GetCollectionToTest().TypeOfElements);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TallyOutturnHeaderOutturnCollection(Factory.New<TallyOutturnHeader>());
		}

		#endregion
	}
}
