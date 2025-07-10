using CargoWise.Application;

namespace Enterprise.DocumentEngineIntegration
{
	public static class ExcelInterfaceFactory
	{
		public static IExcelInterface New()
		{
			return ObjectFactory.Get<IExcelInterface>();
		}
	}
}
