using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(ControlPropertyCollection))]
	sealed class ControlPropertyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ControlPropertyCollection>
	{
		readonly DisposableList disposablesCreatedForTest = new DisposableList(1);

		protected override ControlPropertyCollection GetCollectionToTest()
		{
			return new ControlPropertyCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var control = new KTextBox();
			disposablesCreatedForTest.Add(control);

			var properties = control.GetType().GetProperties().First();
			return new ControlProperty(properties, control);
		}

		protected override void TearDown()
		{
			disposablesCreatedForTest.Dispose();

			base.TearDown();
		}
	}
}
