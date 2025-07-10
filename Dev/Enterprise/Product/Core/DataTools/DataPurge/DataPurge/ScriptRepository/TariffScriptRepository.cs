namespace Enterprise.DataPurge
{
	class TariffScriptRepository : ScriptRepository
	{
		protected override string[] PurgeScripts
		{
			get
			{
				return new string[] {
					CusClassificationScript
				};
			}
		}

		#region Purge Scripts

		#region CusClassification

		const string CusClassificationScript = @"
--CusClassification
DELETE dbo.CusSupImpClassOverride
DELETE dbo.CusCAClassification
DELETE dbo.CusCNClassification
DELETE dbo.CusClassPartPivotRef
DELETE dbo.CusKRClassification
DELETE dbo.CusClassPartPivot
DELETE dbo.CusUSClassification
DELETE dbo.CusClassification";

		#endregion

		#endregion
	}
}
