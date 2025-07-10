using System.Data.Spatial;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1170
	{
		public void BadCode(DbGeography dbg)
		{
			// CW1170 Do not use DbGeography. Consider using SqlGeography instead.
			_ = dbg.PointAt(5);
		}
	}
}
