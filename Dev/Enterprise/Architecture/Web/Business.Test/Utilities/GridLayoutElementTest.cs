using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	[TestedType(typeof(GridLayoutElement))]
	sealed class GridLayoutElementTest : NonPersistentBusinessObjectTestCase
	{
		#region set up

		protected override void SetUp()
		{
			base.SetUp();
			TestLayoutElement = (GridLayoutElement)GetNewBusinessObject();
		}

		GridLayoutElement TestLayoutElement;

		#endregion

		public void TestColumnNumber()
		{
			AssertEquals("Should be ZInt.Zero", ZInt.Zero, TestLayoutElement.ColumnNumber);

			TestLayoutElement.ColumnNumber = 5;
			AssertEquals("Should be 5", 5, TestLayoutElement.ColumnNumber);
			AssertEquals("Should not have errors", 0, TestLayoutElement.ColumnNumberInfo.GetErrors().Count());

			TestLayoutElement.ColumnNumber = -1;
			AssertEquals("Should have an error", 1, TestLayoutElement.ColumnNumberInfo.GetErrors().Count());
			AssertEquals("Error Message", "Column Number should be greater or equal to zero", TestLayoutElement.ColumnNumberInfo.GetErrors().GetFirstMessage());

			TestLayoutElement.ColumnNumber = 0;
			AssertEquals("Should not have errors", 0, TestLayoutElement.ColumnNumberInfo.GetErrors().Count());
		}

		public void TestHeaderText()
		{
			AssertEquals("Should be ZString.Empty", ZString.Empty, TestLayoutElement.HeaderText);

			TestLayoutElement.HeaderText = "Test";
			AssertEquals("Should be \"Test\"", "Test", TestLayoutElement.HeaderText);
			AssertEquals("Should not have errors", 0, TestLayoutElement.HeaderTextInfo.GetErrors().Count());

			AssertEquals("MaxLength for HeaderText should be 50", 50, TestLayoutElement.HeaderTextInfo.MaxLength);
		}
	}
}
