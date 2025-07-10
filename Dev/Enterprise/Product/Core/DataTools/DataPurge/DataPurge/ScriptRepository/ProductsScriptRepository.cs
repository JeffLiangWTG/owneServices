namespace Enterprise.DataPurge
{
	class ProductsScriptRepository : ScriptRepository
	{
		protected override string[] PurgeScripts
		{
			get
			{
				return new string[] {
					OrgSupplierPartScript
				};
			}
		}

		#region Purge Scripts

		#region OrgSupplierPart

		const string OrgSupplierPartScript = @"
--OrgSupplierPart
DELETE dbo.WhsABCCategory
DELETE dbo.RateLineItems WHERE TM_TL in (SELECT TL_PK FROM dbo.RateLines WHERE TL_ParentID is not null AND TL_ParentTableCode = 'OP')
DELETE dbo.RateLines WHERE TL_ParentID is not null AND TL_ParentTableCode = 'OP'
DELETE dbo.WhsPickFace WHERE WF_OP is not null
DELETE dbo.OrgTradeValue WHERE PAV_PAS in (SELECT PAS_PK FROM dbo.OrgTradeDetail JOIN dbo.OrgTradePeriod ON PAS_PA = PA_PK WHERE PA_OP is not null)
DELETE dbo.OrgTradePeriod WHERE PAS_PA in (SELECT PA_PK FROM dbo.OrgTradeDetail WHERE PA_OP is not null)
DELETE dbo.OrgTradeProspect WHERE PAP_PA in (SELECT PA_PK FROM dbo.OrgTradeDetail WHERE PA_OP is not null)
DELETE dbo.OrgTradeDetail WHERE PA_OP is not null
DELETE dbo.OrgSecondaryPartBOMPivot
DELETE dbo.OrgSecondaryPartBOM
DELETE dbo.OrgPartBOM
DELETE dbo.OrgPartLocation
DELETE dbo.CusCAClassification
DELETE dbo.CusCNClassification
DELETE dbo.CusSupImpClassOverride
DELETE dbo.CusClassPartPivotRef
DELETE dbo.CusKRClassification
DELETE dbo.CusClassPartPivot
DELETE dbo.OrgPartRelation
DELETE dbo.OrgPartUnit
DELETE dbo.OrgSupplierPartBarcode
DELETE dbo.OrgSupplierPart";

		#endregion

		#endregion
	}
}
