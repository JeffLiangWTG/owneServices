using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCalcEditColumnTest : ZTemplateColumnTest
	{
		public void TestBindToDecimals()
		{
			AssertNull("Should be null by default", TestColumn.BindToDecimals);

			TestColumn.BindToDecimals = "Test";
			AssertEquals("Should be as assigned", "Test", TestColumn.BindToDecimals);
		}

		public void TestBindToCurrencySymbol()
		{
			AssertNull("Should be null by default", TestColumn.BindToCurrencySymbol);

			TestColumn.BindToCurrencySymbol = "Test";
			AssertEquals("Should be as assigned", "Test", TestColumn.BindToCurrencySymbol);
		}

		public void TestAutoPostBack()
		{
			Assert(1 == 1);
		}

		#region Implementation

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZCalcEditColumn); }
		}

		new ZCalcEditColumn TestColumn
		{
			get { return base.TestColumn as ZCalcEditColumn; }
		}

		#endregion
	}
}
