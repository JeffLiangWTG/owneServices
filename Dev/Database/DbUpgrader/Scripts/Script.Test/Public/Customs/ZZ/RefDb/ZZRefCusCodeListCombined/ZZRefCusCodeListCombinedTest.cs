using System;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCusCodeListCombined
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCusCodeListCombined.ZZRefCusCodeListCombined))]
	class ZZRefCusCodeListCombined_Test : DbCreateScriptTest
	{
		public void TestTransportModes()
		{
			var codeList1PK = Guid.NewGuid();
			var codeList2PK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), '_X', 'TEST', NULL)
				INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping) VALUES (NEWID(), '_XX_', 'JUST FOR TEST', 1, '_X')
				INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
					VALUES ('{codeList1PK}', '_XX_', '_X_1', '_X_1', '1900-01-01', '2079-06-06', '_X')");

			AssertTransportModes(codeList1PK, false, false, false, false, false, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList) VALUES (NEWID(), 'AIR', '{codeList1PK}')");
			AssertTransportModes(codeList1PK, true, false, false, false, false, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList) VALUES (NEWID(), 'SEA', '{codeList1PK}')");
			AssertTransportModes(codeList1PK, true, true, false, false, false, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList) VALUES (NEWID(), 'FIX', '{codeList1PK}')");
			AssertTransportModes(codeList1PK, true, true, true, false, false, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList) VALUES (NEWID(), 'RAI', '{codeList1PK}')");
			AssertTransportModes(codeList1PK, true, true, true, true, false, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList) VALUES (NEWID(), 'ROA', '{codeList1PK}')");
			AssertTransportModes(codeList1PK, true, true, true, true, true, false, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList) VALUES (NEWID(), 'MAI', '{codeList1PK}')");
			AssertTransportModes(codeList1PK, true, true, true, true, true, true, false);
			TestConnection.ExecuteNonQuery($"INSERT INTO RefDatabase_RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList) VALUES (NEWID(), 'INW', '{codeList1PK}')");
			AssertTransportModes(codeList1PK, true, true, true, true, true, true, true);

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.ZZRefCusCodeListCombined (ZZD_PK, ZZD_CountryOrGrouping, ZZD_CodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_IsAir, ZZD_IsSea, ZZD_IsFix, ZZD_IsRai, ZZD_IsRoa, ZZD_IsMai, ZZD_IsInw)
				VALUES ('{codeList2PK}', '_X', '_XX_', '_X_2', '_X_2', '1900-01-01', '2079-06-06', 1, 1, 0, 0, 1, 1, 0)");
			AssertTransportModes(codeList2PK, true, true, false, false, true, true, false);

			TestConnection.ExecuteNonQuery($"UPDATE dbo.ZZRefCusCodeListCombined SET ZZD_IsSea = 0, ZZD_IsRai = 1, ZZD_IsMai = 1, ZZD_IsInw = 0 WHERE ZZD_PK = '{codeList2PK}'");
			AssertTransportModes(codeList2PK, true, false, false, true, true, true, false);

			TestConnection.ExecuteNonQuery($"UPDATE dbo.ZZRefCusCodeListCombined SET ZZD_IsSea = 1, ZZD_IsFix = 1, ZZD_IsInw = 1 WHERE ZZD_PK = '{codeList2PK}'");
			AssertTransportModes(codeList2PK, true, true, true, true, true, true, true);

			TestConnection.ExecuteNonQuery($"DELETE FROM dbo.ZZRefCusCodeListCombined WHERE ZZD_PK = '{codeList2PK}'");
			AssertEquals(0, (int)TestConnection.ExecuteScalar($"SELECT COUNT(1) FROM dbo.ZZRefCusCodeList WHERE ZZD_PK = '{codeList2PK}'"));
			AssertEquals(0, (int)TestConnection.ExecuteScalar($"SELECT COUNT(1) FROM dbo.GenAddOnColumn WHERE XA_ParentID = '{codeList2PK}'"));
		}

		void AssertTransportModes(Guid codeListPK, bool isAir, bool isSea, bool isFix, bool isRai, bool isRoa, bool isMai, bool isInw)
		{
			using (var reader = TestConnection.Command($@"SELECT ZZD_IsAir, ZZD_IsSea, ZZD_IsFix, ZZD_IsRai, ZZD_IsRoa, ZZD_IsMai, ZZD_IsInw FROM dbo.ZZRefCusCodeListCombined WHERE ZZD_PK='{codeListPK}'").ExecuteReader())
			{
				reader.Read();

				AssertEquals("ZZD_IsAir", isAir, reader.GetBoolean(0));
				AssertEquals("ZZD_IsSea", isSea, reader.GetBoolean(1));
				AssertEquals("ZZD_IsFix", isFix, reader.GetBoolean(2));
				AssertEquals("ZZD_IsRai", isRai, reader.GetBoolean(3));
				AssertEquals("ZZD_IsRoa", isRoa, reader.GetBoolean(4));
				AssertEquals("ZZD_IsMai", isMai, reader.GetBoolean(5));
				AssertEquals("ZZD_IsInw", isInw, reader.GetBoolean(6));
			}
		}

		public void TestPriorityToUserEntered()
		{
			var codeList1PK = Guid.NewGuid();
			var codeList2PK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), '_X', 'TEST', NULL)
				INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping) VALUES (NEWID(), '_XX_', 'JUST FOR TEST', 1, '_X')
				INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
					VALUES ('{codeList1PK}', '_XX_', '_X_1', '_X_1', '1900-01-01', '2079-06-06', '_X')");

			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.ZZRefCusCodeList (ZZD_PK, ZZD_CodeType, ZZD_Code, ZZD_Description, ZZD_CountryOrGrouping, ZZD_StartDate, ZZD_EndDate) VALUES ('{codeList2PK}', '_XX_', '_X_1', '_X_1', '_X', '2019-10-29', '2079-06-06')");
			AssertEquals(1, (int)TestConnection.ExecuteScalar($"SELECT COUNT(1) FROM dbo.ZZRefCusCodeListCombined WHERE ZZD_CodeType = '_XX_' AND ZZD_Code = '_X_1' AND ZZD_CountryOrGrouping = '_X'"));
			AssertEquals(codeList2PK, (Guid)TestConnection.ExecuteScalar($"SELECT ZZD_PK FROM dbo.ZZRefCusCodeListCombined WHERE ZZD_CodeType = '_XX_' AND ZZD_Code = '_X_1' AND ZZD_CountryOrGrouping = '_X'"));
		}
	}
}

