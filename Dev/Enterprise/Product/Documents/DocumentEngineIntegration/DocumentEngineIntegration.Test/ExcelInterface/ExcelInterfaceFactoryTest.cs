using NUnit.Framework;

namespace Enterprise.DocumentEngineIntegration.Testing
{
	sealed class ExcelInterfaceFactoryTest : TestCase
	{
		public void TestNew()
		{
			using (IExcelInterface excel = ExcelInterfaceFactory.New())
			{
				AssertNotNull(excel);
			}
		}
	}
}
