using CargoWise.EntityFramework;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1080
	{
		public void Method()
		{
			//CW1080:Do Not Use BusinessObjectCollection IsAssignableFrom
			_ = typeof(BusinessObjectCollection).IsAssignableFrom(typeof(object));
		}
	}
}
