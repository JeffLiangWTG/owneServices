namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1175
	{
		public void BadCode()
		{
			// 	CW1175 The update statement in table(s) {0} do/does not include SystemCreate and SystemLastEdit columns, modify the sql statement to ensure these columns are appropriately updated.
			_ = "UPDATE AccAccountFee SET NotRelatedCol=99, A1_SystemCreateEditUser='Sysadmin' WHERE Id = 1;";
		}
	}
}
