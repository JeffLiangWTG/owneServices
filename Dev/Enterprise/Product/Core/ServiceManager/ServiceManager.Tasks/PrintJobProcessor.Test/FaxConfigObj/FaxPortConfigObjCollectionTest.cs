using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(FaxPortConfigObjCollection))]
	sealed class FaxPortConfigObjCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FaxPortConfigObjCollection>
	{
		#region Implementation

		protected override FaxPortConfigObjCollection GetCollectionToTest()
		{
			return new FaxPortConfigObjCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FaxPortConfigObj(Factory);
		}

		#endregion
	}
}
