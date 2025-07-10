namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1175A
	{
		public void BadCode()
		{
			// 	CW1175A Please review and correct the sql syntax to resolve this issue.
			_ = "UPDATE TOP(10) AccChargeBranchOverride SET A1_SystemCreateEditUser = ";
		}
	}
}
