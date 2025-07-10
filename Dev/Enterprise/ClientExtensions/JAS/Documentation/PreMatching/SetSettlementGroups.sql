USE OdysseyJAS

UPDATE OrgMiscServ
SET OM_OH_ARSettlementGroup = '28AEBD68-C558-445E-A690-1892D3012548'
WHERE OM_OJ_ARDebtorGroup IN (SELECT OJ_PK FROM dbo.OrgDebtorGroup WHERE OJ_Code IN ('INT','ASC','AGE'))

UPDATE OrgMiscServ
SET OM_OH_APSettlementGroup = '28AEBD68-C558-445E-A690-1892D3012548'
where OM_OG_APCreditorGroup IN (SELECT OG_PK FROM dbo.OrgCreditorGroup where OG_Code IN ('INT','ASC','AGE'))