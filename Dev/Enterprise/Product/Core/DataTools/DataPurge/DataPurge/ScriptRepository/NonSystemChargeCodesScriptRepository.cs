namespace Enterprise.DataPurge
{
	class NonSystemChargeCodesScriptRepository : ScriptRepository
	{
		protected override string[] PurgeScripts
		{
			get
			{
				return new string[] {
					NonSystemChargeCodesScript
				};
			}
		}

		#region Purge Scripts

		#region Non System Charge Codes

		const string NonSystemChargeCodesScript = @"
--AccChargeCode
DECLARE @DemoCompanyPk uniqueidentifier
SET @DemoCompanyPk = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM')

UPDATE dbo.OrgCompanyData SET OB_AC_APDefaultChargeCode = null 
  WHERE OB_AC_APDefaultChargeCode in (SELECT AC_PK FROM dbo.AccChargeCode 
                                      WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

UPDATE dbo.GlbCompanyCampaignBudgetItem SET G9_AC = null
  WHERE G9_AC in (SELECT AC_PK FROM dbo.AccChargeCode 
                  WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccClientInvoiceOrder
  WHERE AI_AC in (SELECT AC_PK FROM dbo.AccChargeCode 
                            WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))
DELETE dbo.RateLineItems
  WHERE TM_AC in (SELECT AC_PK FROM dbo.AccChargeCode 
                            WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))
DELETE dbo.RateLineItems
  WHERE TM_TL in (SELECT TL_PK FROM dbo.RateLines
                  WHERE TL_AC in (SELECT AC_PK FROM dbo.AccChargeCode 
                                  WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk)))

DELETE dbo.RateLines
  WHERE TL_AC in (SELECT AC_PK FROM dbo.AccChargeCode 
                  WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccChargeTaxOverride
 WHERE  AO_ParentID in (SELECT AC_PK FROM dbo.AccChargeCode 
                  WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccChargeTypeOverride
  WHERE AN_AC_ChargeCode in (SELECT AC_PK FROM dbo.AccChargeCode 
                             WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))
DELETE dbo.GlbDeptCharges 
  WHERE GD_AC in (SELECT AC_PK FROM dbo.AccChargeCode 
                  WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccTaxOverrideGroupChargeCodePivot
	WHERE ACP_AC_ChargeCode in (SELECT AC_PK FROM dbo.AccChargeCode 
                  WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccChargeCodeCarrierIataMapping
	WHERE ACI_AC_ChargeCode in (SELECT AC_PK FROM dbo.AccChargeCode 
                  WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccChargeGovtChargeCodeOverride
	WHERE ACG_AC in(SELECT AC_PK FROM dbo.AccChargeCode 
                  WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccChargeApportionmentMethodOverride
	WHERE AAM_AC in(SELECT AC_PK FROM dbo.AccChargeCode 
                  WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccChargeComplianceDescription
	WHERE ADE_AC in (SELECT AC_PK FROM dbo.AccChargeCode
					WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccSurchargeBasis
	WHERE ASB_ASC_SurchargeConfiguration in (SELECT ASC_PK FROM dbo.AccSurchargeConfiguration
											WHERE ASC_AC_ChargeCode in (SELECT AC_PK FROM dbo.AccChargeCode 
																		WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk)))

DELETE dbo.AccSurchargeConfiguration
	WHERE ASC_AC_ChargeCode in (SELECT AC_PK FROM dbo.AccChargeCode 
								WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccChargeSupplyTypeOverride
	WHERE ACS_ParentID in (SELECT AC_PK FROM dbo.AccChargeCode
					WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.RatingDocumentsChargeOrder
  WHERE RCO_AC_ChargeCode in (SELECT AC_PK FROM dbo.AccChargeCode 
					WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk))

DELETE dbo.AccChargeCode 
  WHERE AC_Code not in (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_GC = @DemoCompanyPk)";

		#endregion

		#endregion
	}
}
