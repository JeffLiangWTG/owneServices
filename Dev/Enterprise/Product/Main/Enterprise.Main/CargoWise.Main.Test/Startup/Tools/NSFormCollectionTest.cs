using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(NSFormCollection))]
	sealed class NSFormCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NSFormCollection>
	{
		protected override NSFormCollection GetCollectionToTest()
		{
			return new NSFormCollection();
		}
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FormDetails(new Form());
		}
	}
}
