using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	[TestedType(typeof(GridLayoutElementCollection))]
	sealed class GridLayoutElementCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GridLayoutElementCollection>
	{
		#region Overrides

		protected override GridLayoutElementCollection GetCollectionToTest()
		{
			return new GridLayoutElementCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GridLayoutElement();
		}

		#endregion
	}
}
