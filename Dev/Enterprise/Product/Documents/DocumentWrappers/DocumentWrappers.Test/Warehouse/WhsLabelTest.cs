using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(WhsLabel))]
	public class WhsLabelTest : NonPersistentBusinessObjectTestCase
	{
		#region TestConstructors

		public void TestConstructors()
		{
			AssertEquals("", new WhsLabel().String);
			AssertEquals("palletID", new WhsLabel("palletID").String);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WhsLabel();
		}

		#endregion
	}
}
