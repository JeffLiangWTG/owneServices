using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.GUI
{
	public class CdcControlTest : TestCase
	{
		public void TestPopulateCdcSchemaErrorsWithoutDataSource()
		{
			using (var cdcControl = new CdcControlForTest())
			{
				AssertNoExceptionThrown(() => cdcControl.PopulateCdcSchemaErrors());
				AssertNoExceptionThrown(() => cdcControl.RefreshInfo());
			}
		}

		public class CdcControlForTest : CdcControl
		{
			protected override BiManagerForm GetParentForm()
			{
				return null;
			}
		}
	}
}
