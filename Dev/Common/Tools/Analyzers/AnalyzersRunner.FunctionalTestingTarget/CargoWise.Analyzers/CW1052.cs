using CargoWise.EntityFramework;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1052
	{
		public void Method()
		{
			var factory = new BusinessObjectFactory();
			//CW1052:Do Not Cast Factory Method
			_ = (object)factory.Load(typeof(object), null);
		}
	}
}
