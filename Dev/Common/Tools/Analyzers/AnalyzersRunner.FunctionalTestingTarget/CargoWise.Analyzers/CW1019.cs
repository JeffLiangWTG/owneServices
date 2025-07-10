namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1019
	{
		public void Method()
		{
			var conn = new SqlConnection("");
			var tran = conn.BeginTransaction();

			//CW1019:No SqlTransaction.Rollback Rule
			tran.Rollback();
		}
	}
}
