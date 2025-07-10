using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CAeMHDocAddressDependentCollection))]
	sealed class CAeMHDocAddressDependentCollectionTest : JobDocAddressDependentCollectionTest
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CAeMHDocAddressDependentCollection(Factory.New<CusCAeMHHouse>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CAeMHDocAddress>();
		}
		#endregion
	}
}
