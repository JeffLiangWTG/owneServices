using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.UNDG
{
	class trgUNDGSubstance_InsIntegrationTest : TransactionedTestCase
	{
		public void TestInsert()
		{
			var zzUNDGSubstanceCountBefore = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.ZZUNDGSubstance");
			var uNDGSubstanceCountBefore = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.UNDGSubstance");

			AssertEquals(zzUNDGSubstanceCountBefore, uNDGSubstanceCountBefore);

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.UNDGSubstance (DG_PK, DG_UNNO, DG_Code, DG_Standard, DG_IsActive, DG_IsSystem, DG_Variation,DG_Class,DG_SubLabel1,DG_SubLabel2,DG_PSN,DG_PG,DG_EMS,DG_MP,DG_LQMaxAmt,DG_LQMaxAmtUQ,DG_TechName,DG_TreatAs,DG_DglPhrase,DG_PackIns,DG_PackProv,DG_IBCIns,DG_IBCProv,DG_IMOTankIns,DG_UNTankIns,DG_TankProv,DG_Markers,DG_Pointers,DG_EXVector,DG_StowCat,DG_CodedStow,DG_State,DG_ExpLim,DG_UlineEMS,DG_UsrUSDOTShippingName,DG_ExceptedQuantityCode,DG_Variant,DG_FlashPoint,DG_CargoMaxAmt,DG_CargoMaxAmtUQ,DG_CargoPackAmtType,DG_CargoPackIns,DG_EmergencyResponseGuide,DG_Hazards,DG_IsNotOtherwiseSpecified,DG_LQ2OrPaxMaxAmt,DG_LQ2OrPaxMaxAmtType,DG_LQ2OrPaxMaxAmtUQ,DG_LQMaxAmtType,DG_LQSpecProvIndex,DG_PaxPackIns,DG_SpecialHandlingCodes,DG_UniqueRecordId)
VALUES(newid(), 'XXXX', 'XXXX', 'IAT', 1, 1, '','','','','','','','',0,'','','','','','','','','','','','','','','','','','','','','','','',0,'','NLM','','','',0,0,'NLM','','NLM','','','','')");

			var zzUNDGSubstanceCountAfter = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.ZZUNDGSubstance");
			var uNDGSubstanceCountAfter = (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.UNDGSubstance");

			AssertEquals(zzUNDGSubstanceCountAfter, uNDGSubstanceCountAfter);

			AssertEquals(zzUNDGSubstanceCountAfter, zzUNDGSubstanceCountBefore + 1);
			AssertEquals(uNDGSubstanceCountAfter, uNDGSubstanceCountBefore + 1);
		}

		public void TestStandardBasedOnMode()
		{
			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.UNDGSubstance (DG_PK, DG_UNNO, DG_Code, DG_Mode, DG_IsActive, DG_IsSystem, DG_Variation,DG_Class,DG_SubLabel1,DG_SubLabel2,DG_PSN,DG_PG,DG_EMS,DG_MP,DG_LQMaxAmt,DG_LQMaxAmtUQ,DG_TechName,DG_TreatAs,DG_DglPhrase,DG_PackIns,DG_PackProv,DG_IBCIns,DG_IBCProv,DG_IMOTankIns,DG_UNTankIns,DG_TankProv,DG_Markers,DG_Pointers,DG_EXVector,DG_StowCat,DG_CodedStow,DG_State,DG_ExpLim,DG_UlineEMS,DG_UsrUSDOTShippingName,DG_ExceptedQuantityCode,DG_Variant,DG_FlashPoint,DG_CargoMaxAmt,DG_CargoMaxAmtUQ,DG_CargoPackAmtType,DG_CargoPackIns,DG_EmergencyResponseGuide,DG_Hazards,DG_IsNotOtherwiseSpecified,DG_LQ2OrPaxMaxAmt,DG_LQ2OrPaxMaxAmtType,DG_LQ2OrPaxMaxAmtUQ,DG_LQMaxAmtType,DG_LQSpecProvIndex,DG_PaxPackIns,DG_SpecialHandlingCodes,DG_UniqueRecordId)
VALUES(newid(), 'XXXX', 'XXXX', 'AIR', 1, 1, '','','','','','','','',0,'','','','','','','','','','','','','','','','','','','','','','','',0,'','NLM','','','',0,0,'NLM','','NLM','','','','')");

			var standardValue = TestConnection.ExecuteScalar("SELECT DG_Standard FROM dbo.UNDGSubstance WHERE DG_Code = 'XXXX'");
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, standardValue);

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.UNDGSubstance (DG_PK, DG_UNNO, DG_Code, DG_Mode, DG_IsActive, DG_IsSystem, DG_Variation,DG_Class,DG_SubLabel1,DG_SubLabel2,DG_PSN,DG_PG,DG_EMS,DG_MP,DG_LQMaxAmt,DG_LQMaxAmtUQ,DG_TechName,DG_TreatAs,DG_DglPhrase,DG_PackIns,DG_PackProv,DG_IBCIns,DG_IBCProv,DG_IMOTankIns,DG_UNTankIns,DG_TankProv,DG_Markers,DG_Pointers,DG_EXVector,DG_StowCat,DG_CodedStow,DG_State,DG_ExpLim,DG_UlineEMS,DG_UsrUSDOTShippingName,DG_ExceptedQuantityCode,DG_Variant,DG_FlashPoint,DG_CargoMaxAmt,DG_CargoMaxAmtUQ,DG_CargoPackAmtType,DG_CargoPackIns,DG_EmergencyResponseGuide,DG_Hazards,DG_IsNotOtherwiseSpecified,DG_LQ2OrPaxMaxAmt,DG_LQ2OrPaxMaxAmtType,DG_LQ2OrPaxMaxAmtUQ,DG_LQMaxAmtType,DG_LQSpecProvIndex,DG_PaxPackIns,DG_SpecialHandlingCodes,DG_UniqueRecordId)
VALUES(newid(), 'YYYY', 'YYYY', 'SEA', 1, 1, '','','','','','','','',0,'','','','','','','','','','','','','','','','','','','','','','','',0,'','NLM','','','',0,0,'NLM','','NLM','','','','')");

			standardValue = TestConnection.ExecuteScalar("SELECT DG_Standard FROM dbo.UNDGSubstance WHERE DG_Code = 'YYYY'");
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, standardValue);
		}
	}
}

