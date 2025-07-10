using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.UNDG;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.UNDG
{
	[TestedType(typeof(trgUNDGSubstance_Upd))]
	class trgUNDGSubstance_UpdTest : DbCreateScriptTest
	{
		public void TestUpdate()
		{
			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.UNDGSubstance (DG_PK, DG_UNNO, DG_Code, DG_Standard, DG_IsActive, DG_IsSystem, DG_Variation,DG_Class,DG_SubLabel1,DG_SubLabel2,DG_PSN,DG_PG,DG_EMS,DG_MP,DG_LQMaxAmt,DG_LQMaxAmtUQ,DG_TechName,DG_TreatAs,DG_DglPhrase,DG_PackIns,DG_PackProv,DG_IBCIns,DG_IBCProv,DG_IMOTankIns,DG_UNTankIns,DG_TankProv,DG_Markers,DG_Pointers,DG_EXVector,DG_StowCat,DG_CodedStow,DG_State,DG_ExpLim,DG_UlineEMS,DG_UsrUSDOTShippingName,DG_ExceptedQuantityCode,DG_Variant,DG_FlashPoint,DG_CargoMaxAmt,DG_CargoMaxAmtUQ,DG_CargoPackAmtType,DG_CargoPackIns,DG_EmergencyResponseGuide,DG_Hazards,DG_IsNotOtherwiseSpecified,DG_LQ2OrPaxMaxAmt,DG_LQ2OrPaxMaxAmtType,DG_LQ2OrPaxMaxAmtUQ,DG_LQMaxAmtType,DG_LQSpecProvIndex,DG_PaxPackIns,DG_SpecialHandlingCodes,DG_UniqueRecordId)
VALUES('7ECD7ECE-FE47-480B-A4E4-5B8C316CB8D8', 'XXXX', 'XXXX', 'IAT', 1, 1, '','','','','','','','',0,'','','','','','','','','','','','','','','','','','','','','','','',0,'','NLM','','','',0,0,'NLM','','NLM','','','','')");

			TestConnection.ExecuteNonQuery(@"UPDATE dbo.UNDGSubstance SET DG_Variant = 'A', DG_Code = 'XXXXA' WHERE DG_UNNO = 'XXXX'");

			using (var cmd = TestConnection.Command("SELECT DG_Code, DG_UNNO, DG_Variant FROM dbo.ZZUNDGSubstance WHERE DG_PK = '7ECD7ECE-FE47-480B-A4E4-5B8C316CB8D8'"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var code = reader.GetString(0);
					AssertEquals("XXXXA", code);
					var unno = reader.GetString(1);
					AssertEquals("XXXX", unno);
					var variant = reader.GetString(2);
					AssertEquals("A", variant);
				}
			}
		}
	}
}

