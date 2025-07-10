using CargoWise.EntityFramework;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1044
	{
		public void Method()
		{
			var factory = new BusinessObjectFactory();

			//CW1044:Factory.GetDatabaseCount() Collection Count Rule
			_ = factory.GetDatabaseCount(typeof(object)) == 0;
		}
	}
}
