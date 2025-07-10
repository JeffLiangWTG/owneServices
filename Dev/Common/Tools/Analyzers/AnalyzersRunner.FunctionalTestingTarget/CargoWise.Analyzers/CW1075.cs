using CargoWise.EntityFramework;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1075
	{
		public void Method()
		{
			var query = new ZQuery();

			for (var i = 0; i < 10; i++)
			{
				//CW1075:Do Not Use Loop To Add Or Conditions To Filter
				query.AddToFilter(JoinCondition.Or, null, i);
			}
		}
	}
}
