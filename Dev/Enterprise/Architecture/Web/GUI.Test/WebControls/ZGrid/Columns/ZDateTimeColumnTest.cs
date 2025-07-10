using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZDateTimeColumnTest : ZTemplateColumnTest
	{
		public void TestCanBeEnabledByClient()
		{
			var column = (ZDateTimeColumn)TestColumn;
			AssertEquals(false, column.CanBeEnabledByClient);

			column.CanBeEnabledByClient = true;
			AssertEquals(true, column.CanBeEnabledByClient);
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZDateTimeColumn); }
		}

		protected override bool ExpectedNoWrapDefaultValue
		{
			get { return true; }
		}
	}
}
