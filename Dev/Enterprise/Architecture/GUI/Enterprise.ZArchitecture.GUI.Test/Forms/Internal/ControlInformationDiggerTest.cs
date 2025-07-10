using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(ControlInformationDigger))]
	sealed class ControlInformationDiggerTest : NonPersistentBusinessObjectTestCase
	{
		readonly DisposableList disposablesCreatedForTest = new DisposableList(1);

		public void TestGetPropertiesValues()
		{
			using (var textBox = new KTextBox())
			{
				var controlInformationDigger = new ControlInformationDigger(textBox);
				var property = controlInformationDigger.PropertiesValues.Cast<ControlProperty>().First(p => p.Name == "ReadOnly");

				textBox.ReadOnly = true;
				AssertEquals("True", property.Value);

				textBox.ReadOnly = false;
				AssertEquals("False", property.Value);
			}
		}

		public void TestGetCollectionDoesntIncludeIndexers()
		{
			using (var control = new TestControlWithIndexer())
			{
				var controlInformationDigger = new ControlInformationDigger(control);
				var indexersIncluded = controlInformationDigger.PropertiesValues.Cast<ControlProperty>().Any(p => p.Name == "Item");
				Assert("Indexers should be ommitted from the list", !indexersIncluded);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var control = new KTextBox();
			disposablesCreatedForTest.Add(control);

			return new ControlInformationDigger(control);
		}

		protected override void TearDown()
		{
			disposablesCreatedForTest.Dispose();

			base.TearDown();
		}
	}
}
