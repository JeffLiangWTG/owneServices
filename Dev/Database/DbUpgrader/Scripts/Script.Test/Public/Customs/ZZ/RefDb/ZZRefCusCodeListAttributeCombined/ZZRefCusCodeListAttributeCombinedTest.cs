using System;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCusCodeListAttributeCombined
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCusCodeListAttributeCombined.ZZRefCusCodeListAttributeCombined))]
	class ZZRefCusCodeListAttributeCombined_Test : DbCreateScriptTest
	{
		public void TestTransportModes()
		{
			var codeList1PK = Guid.NewGuid();
			var codeList2PK = Guid.NewGuid();
			var codeListAttr1PK = Guid.NewGuid();
			var codeListAttr2PK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping)
												VALUES (NEWID(), '_X', 'TEST', NULL)
				INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping)
												VALUES (NEWID(), '_XX_', 'JUST FOR TEST', 1, '_X')
				INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
												VALUES ('{codeList1PK}', '_XX_', '_X_1', '_X_1', '1900-01-01', '2079-06-06', '_X')
				INSERT INTO dbo.ZZRefCusCodeList (ZZD_PK, ZZD_CodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_CountryOrGrouping)
									VALUES ('{codeList2PK}', '_XX_', '_X_2', '_X_2', '1900-01-01', '2079-06-06', '_X')
				INSERT dbo.RefDatabase_RefCusCodeListAttributeName (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping)
														VALUES (newid(), 'ROLE', 'Desc.', '_XX_', '_X')
				INSERT INTO RefDatabase_RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value)
														VALUES('{codeListAttr1PK}', '{codeList1PK}', 'ROLE', 'ENT')");

			AssertTransportModes(codeListAttr1PK, false, false, false, false, false, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZE_Attribute) VALUES (NEWID(), 'AIR', '{codeListAttr1PK}')");
			AssertTransportModes(codeListAttr1PK, true, false, false, false, false, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZE_Attribute) VALUES (NEWID(), 'SEA', '{codeListAttr1PK}')");
			AssertTransportModes(codeListAttr1PK, true, true, false, false, false, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZE_Attribute) VALUES (NEWID(), 'FIX', '{codeListAttr1PK}')");
			AssertTransportModes(codeListAttr1PK, true, true, true, false, false, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZE_Attribute) VALUES (NEWID(), 'RAI', '{codeListAttr1PK}')");
			AssertTransportModes(codeListAttr1PK, true, true, true, true, false, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZE_Attribute) VALUES (NEWID(), 'ROA', '{codeListAttr1PK}')");
			AssertTransportModes(codeListAttr1PK, true, true, true, true, true, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZE_Attribute) VALUES (NEWID(), 'MAI', '{codeListAttr1PK}')");
			AssertTransportModes(codeListAttr1PK, true, true, true, true, true, true, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZE_Attribute) VALUES (NEWID(), 'INW', '{codeListAttr1PK}')");
			AssertTransportModes(codeListAttr1PK, true, true, true, true, true, true, true);

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.ZZRefCusCodeListAttributeCombined (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_IsAir, ZZE_IsSea, ZZE_IsFix, ZZE_IsRai, ZZE_IsRoa, ZZE_IsMai, ZZE_IsInw)
				VALUES ('{codeListAttr2PK}', '{codeList2PK}', 'ROLE', 'TRA', 1, 1, 0, 0, 1, 1, 0)");
			AssertTransportModes(codeListAttr2PK, true, true, false, false, true, true, false);

			TestConnection.ExecuteNonQuery($"UPDATE dbo.ZZRefCusCodeListAttributeCombined SET ZZE_IsSea = 0, ZZE_IsRai = 1, ZZE_IsMai = 1, ZZE_IsInw = 0 WHERE ZZE_PK = '{codeListAttr2PK}'");
			AssertTransportModes(codeListAttr2PK, true, false, false, true, true, true, false);

			TestConnection.ExecuteNonQuery($"UPDATE dbo.ZZRefCusCodeListAttributeCombined SET ZZE_IsSea = 1, ZZE_IsFix = 1, ZZE_IsInw = 1 WHERE ZZE_PK = '{codeListAttr2PK}'");
			AssertTransportModes(codeListAttr2PK, true, true, true, true, true, true, true);

			TestConnection.ExecuteNonQuery($"DELETE FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_PK = '{codeListAttr2PK}'");
			AssertEquals(0, (int)TestConnection.ExecuteScalar($"SELECT COUNT(1) FROM dbo.CusCodeData WHERE CY_PK = '{codeListAttr2PK}'"));
			AssertEquals(0, (int)TestConnection.ExecuteScalar($"SELECT COUNT(1) FROM dbo.GenAddOnColumn WHERE XA_ParentID = '{codeListAttr2PK}'"));
		}

		void AssertTransportModes(Guid codeListAttrPK, bool isAir, bool isSea, bool isFix, bool isRai, bool isRoa, bool isMai, bool isInw)
		{
			using (var reader = TestConnection.Command($@"SELECT ZZE_IsAir, ZZE_IsSea, ZZE_IsFix, ZZE_IsRai, ZZE_IsRoa, ZZE_IsMai, ZZE_IsInw FROM dbo.ZZRefCusCodeListAttributeCombined WHERE ZZE_PK='{codeListAttrPK}'").ExecuteReader())
			{
				reader.Read();

				AssertEquals("ZZE_IsAir", isAir, reader.GetBoolean(0));
				AssertEquals("ZZE_IsSea", isSea, reader.GetBoolean(1));
				AssertEquals("ZZE_IsFix", isFix, reader.GetBoolean(2));
				AssertEquals("ZZE_IsRai", isRai, reader.GetBoolean(3));
				AssertEquals("ZZE_IsRoa", isRoa, reader.GetBoolean(4));
				AssertEquals("ZZE_IsMai", isMai, reader.GetBoolean(5));
				AssertEquals("ZZE_IsInw", isInw, reader.GetBoolean(6));
			}
		}
	}
}

