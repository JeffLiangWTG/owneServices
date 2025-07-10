using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosTempTableCreatorTest : TestCaseWithFactory
	{
		public void TestCognosTempTableCreatorWhenTempTablesAlreadyExist()
		{
			ExecuteNonQuery("CREATE TABLE #CounterCompanies ( PK uniqueidentifier )");
			ExecuteNonQuery("CREATE TABLE #CognosModes ( PK uniqueidentifier )");
			ExecuteNonQuery("CREATE TABLE #CognosRawAggregate ( PK uniqueidentifier )");
			ExecuteNonQuery("CREATE TABLE #CognosExport( PK uniqueidentifier )");
			AssertTableExist("Dummy table. ", CognosTempTableCreator.TempTableNames.CounterCompanies, true);
			AssertTableExist("Dummy table. ", CognosTempTableCreator.TempTableNames.CognosModes, true);
			AssertTableExist("Dummy table. ", CognosTempTableCreator.TempTableNames.CognosRawAggregate, true);
			AssertTableExist("Dummy table. ", CognosTempTableCreator.TempTableNames.CognosExport, true);
			using (CognosTempTableCreator creator = new CognosTempTableCreator())
			{
				AssertTableExist("", CognosTempTableCreator.TempTableNames.CounterCompanies, true);
				AssertTableExist("", CognosTempTableCreator.TempTableNames.CognosModes, true);
				AssertTableExist("", CognosTempTableCreator.TempTableNames.CognosRawAggregate, true);
				AssertTableExist("", CognosTempTableCreator.TempTableNames.CognosExport, true);
			}
		}

		public void TestCognosTempTablesDroppedOnDisposed()
		{
			AssertTableExist("Pre-condition. ", CognosTempTableCreator.TempTableNames.CounterCompanies, false);
			AssertTableExist("Pre-condition. ", CognosTempTableCreator.TempTableNames.CognosModes, false);
			AssertTableExist("Pre-condition. ", CognosTempTableCreator.TempTableNames.CognosRawAggregate, false);
			AssertTableExist("Pre-condition. ", CognosTempTableCreator.TempTableNames.CognosExport, false);
			using (CognosTempTableCreator creator = new CognosTempTableCreator())
			{
				AssertTableExist("", CognosTempTableCreator.TempTableNames.CounterCompanies, true);
				AssertTableExist("", CognosTempTableCreator.TempTableNames.CognosModes, true);
				AssertTableExist("", CognosTempTableCreator.TempTableNames.CognosRawAggregate, true);
				AssertTableExist("", CognosTempTableCreator.TempTableNames.CognosExport, true);
			}

			AssertTableExist("Should be dropped OnDisposed. ", CognosTempTableCreator.TempTableNames.CounterCompanies, false);
			AssertTableExist("Should be dropped OnDisposed. ", CognosTempTableCreator.TempTableNames.CognosModes, false);
			AssertTableExist("Should be dropped OnDisposed. ", CognosTempTableCreator.TempTableNames.CognosRawAggregate, false);
			AssertTableExist("Should be dropped OnDisposed. ", CognosTempTableCreator.TempTableNames.CognosExport, false);
		}

		#region TestCounterCompanyTable
		public void TestCounterCompanyTable()
		{
			SetupForTestCounterCompanyTable();
			using (new CognosTempTableCreator())
			{
				DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
				collection.Load("SELECT * FROM #CounterCompanies ORDER BY T2_CompanyCode");
				AssertEquals("Should only include organsations associated with Transaction from current company", 14, collection.Count);
				AssertCounterCompanyDynamicBizO(collection[0], "4", "", "", "NAM", "", "INT");
				AssertCounterCompanyDynamicBizO(collection[1], "7", "AEFZE", "", "MEA");
				AssertCounterCompanyDynamicBizO(collection[2], "1", "AUCOR", "", "ANZ", "INT", "ASC");
				AssertCounterCompanyDynamicBizO(collection[3], "2", "BRSAO", "", "SAM", "TPY", "");
				AssertCounterCompanyDynamicBizO(collection[4], "8", "DEFRA", "", "EUR");
				AssertCounterCompanyDynamicBizO(collection[5], "5", "IDJKT", "", "SEA", "", "TPY");
				AssertCounterCompanyDynamicBizO(collection[6], "12", "INBOM", "", "OTH");
				AssertCounterCompanyDynamicBizO(collection[7], "10", "KENBO", "", "AFR");
				AssertCounterCompanyDynamicBizO(collection[8], "6", "NZAKL", "", "ANZ");
				AssertCounterCompanyDynamicBizO(collection[9], "13", "RUDME", "", "OTH");
				AssertCounterCompanyDynamicBizO(collection[10], "14", "SBHIR", "", "OTH");
				AssertCounterCompanyDynamicBizO(collection[11], "11", "TTPOS", "", "OTH");
				AssertCounterCompanyDynamicBizO(collection[12], "3", "USCOR", "", "NAM", "ASC", "");
				AssertCounterCompanyDynamicBizO(collection[13], "9", "ZACOR", "", "AFR");
			}
		}

		void SetupForTestCounterCompanyTable()
		{
			CreateOrgHeader("1", true, true, "AUCOR", "AUSYD", "INT", "ASC", true);
			CreateOrgHeader("2", true, true, "BRSAO", "BRSAO", "TPY", "", true);
			CreateOrgHeader("3", true, true, "USCOR", "USATL", "ASC", "", true);
			CreateOrgHeader("4", true, true, "", "CAYTO", "", "INT", true);
			CreateOrgHeader("5", true, true, "IDJKT", "IDJKT", "", "TPY", true);
			CreateOrgHeader("6", true, true, "NZAKL", "NZAKL", "INT", "TPY", false);
			CreateOrgHeader("7", true, true, "AEFZE", "AEDXB", "ASC", "TPY", false);
			CreateOrgHeader("8", true, true, "DEFRA", "DEFRA");
			CreateOrgHeader("9", true, true, "ZACOR", "ZACPT");
			CreateOrgHeader("10", true, true, "KENBO", "KENBO");
			CreateOrgHeader("11", true, true, "TTPOS", "TTPOS");
			CreateOrgHeader("12", true, true, "INBOM", "INBOM");
			CreateOrgHeader("13", true, true, "RUDME", "RUDME");
			CreateOrgHeader("14", true, true, "SBHIR", "SBHIR");
			CreateOrgHeader("15", false, true, "AUCOR", "AUMEL");
			CreateOrgHeader("16", false, false, "HKHKG", "HKHKG");
			CreateOrgHeader("17", true, false, "HKHKG", "HKHKG");
			Factory.Save();
		}

		JASOrgHeader CreateOrgHeader(ZString orgCode, ZBool isAssociatedWithTransaction, ZBool isTransactionFromCurrentCompany, ZString nettingCode, ZString portCode)
		{
			return CreateOrgHeader(orgCode, isAssociatedWithTransaction, isTransactionFromCurrentCompany, nettingCode, portCode, "", "", true);
		}

		JASOrgHeader CreateOrgHeader(ZString orgCode, ZBool isAssociatedWithTransaction, ZBool isTransactionFromCurrentCompany, ZString nettingCode, ZString portCode, ZString creditorGroup, ZString debtorGroup, ZBool companyDataBelongsToCurrentCompany)
		{
			JASOrgHeader result = Factory.New<JASOrgHeader>();
			result.OH_Code = orgCode;
			if (isAssociatedWithTransaction)
			{
				JASARInvoice transaction = Factory.NewWithValidTestData<JASARInvoice>();
				transaction.AH_OH = result.PK;
				transaction.AH_GB = (isTransactionFromCurrentCompany) ? OtherBranchFromCurrentCompany.PK : BranchFromOtherCompany.PK;
				transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;
				transaction.AH_InvoiceDate = ZDateTime.Now;
			}

			if (!nettingCode.IsEmpty)
			{
				result.NettingCode = nettingCode;
			}

			result.OH_RL_NKClosestPort = portCode;
			OrgCompanyData companyData;
			if (!companyDataBelongsToCurrentCompany)
			{
				companyData = Factory.New<OrgCompanyData>();
				companyData.OB_GC = OtherCompany.PK;
				companyData.OB_OH = result.PK;
			}
			else
			{
				companyData = result.CompanyData;
			}

			if (!creditorGroup.IsEmpty)
			{
				companyData.OB_OG_APCreditorGroup = Factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, creditorGroup).PK;
			}

			if (!debtorGroup.IsEmpty)
			{
				companyData.OB_OJ_ARDebtorGroup = Factory.LoadFromNaturalKey<OrgDebtorGroup>(OrgDebtorGroupSchema.OJ_Code, debtorGroup).PK;
			}

			return result;
		}

		void AssertCounterCompanyDynamicBizO(DynamicBusinessObject bizO, string expectedOrgCode, string expectedT2_CompanyCode, string expectedT2_BusinessType, string expectedT2_Geographical)
		{
			AssertCounterCompanyDynamicBizO(bizO, expectedOrgCode, expectedT2_CompanyCode, expectedT2_BusinessType, expectedT2_Geographical, "", "");
		}

		void AssertCounterCompanyDynamicBizO(DynamicBusinessObject bizO, string expectedOrgCode, string expectedT2_CompanyCode, string expectedT2_BusinessType, string expectedT2_Geographical, string expectedCreditorGroupCode, string expectedDebtorGroupCode)
		{
			AssertEquals(expectedOrgCode, Factory.Load<OrgHeader>(new ZGuid(bizO["T2_OH"])).OH_Code);
			AssertEquals(expectedT2_CompanyCode, bizO["T2_CompanyCode"]);
			AssertEquals(expectedT2_BusinessType, bizO["T2_BusinessType"]);
			AssertEquals(expectedT2_Geographical, bizO["T2_Geographical"]);
			var expectedT2_OG_CreditorGroup = !string.IsNullOrEmpty(expectedCreditorGroupCode) ? Factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, expectedCreditorGroupCode).PK : ZGuid.Empty;
			AssertEquals(expectedT2_OG_CreditorGroup, bizO["T2_OG_CreditorGroup"]);
			var expectedT2_OJ_DebtorGroup = !string.IsNullOrEmpty(expectedDebtorGroupCode) ? Factory.LoadFromNaturalKey<OrgDebtorGroup>(OrgDebtorGroupSchema.OJ_Code, expectedDebtorGroupCode).PK : ZGuid.Empty;
			AssertEquals(expectedT2_OJ_DebtorGroup, bizO["T2_OJ_DebtorGroup"]);
		}

		#endregion
		#region TestCognosModeTable
		public void TestCognosModeTable()
		{
			SetupForTestCognosModeTable();
			using (new CognosTempTableCreator())
			{
				DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
				collection.Load("SELECT #CognosModes.*, GE_Code FROM #CognosModes INNER JOIN dbo.GlbDepartment ON GE_PK =  T3_GE ORDER BY T3_Mode, GE_Code");
				AssertEquals(25, collection.Count);
				AssertMappedCognosMode(collection[0], "AE", "DEL");
				AssertMappedCognosMode(collection[1], "AE", "DES");
				AssertMappedCognosMode(collection[2], "AE", "LEA");
				AssertMappedCognosMode(collection[3], "AI", "CEA");
				AssertMappedCognosMode(collection[4], "AI", "FER");
				AssertMappedCognosMode(collection[5], "AI", "TLL");
				AssertMappedCognosMode(collection[6], "CHB", "CPP");
				AssertMappedCognosMode(collection[7], "CHB", "CXB");
				AssertMappedCognosMode(collection[8], "CHB", "WBS");
				AssertMappedCognosMode(collection[9], "ME", "DEA");
				AssertMappedCognosMode(collection[10], "ME", "DIA");
				AssertMappedCognosMode(collection[11], "ME", "DIS");
				AssertMappedCognosMode(collection[12], "MI", "CIR");
				AssertMappedCognosMode(collection[13], "MI", "TIA");
				AssertMappedCognosMode(collection[14], "MI", "TIS");
				AssertMappedCognosMode(collection[15], "NV", "CER");
				AssertMappedCognosMode(collection[16], "NV", "FIA");
				AssertMappedCognosMode(collection[17], "NV", "WFS");
				AssertMappedCognosMode(collection[18], "OTH", "FDR");
				AssertMappedCognosMode(collection[19], "OTH", "FIS");
				AssertMappedCognosMode(collection[20], "OTH", "TES");
				AssertMappedCognosMode(collection[21], "PR", "LES");
				AssertMappedCognosMode(collection[22], "PR", "TEA");
				AssertMappedCognosMode(collection[23], "PR", "TOT");
				AssertMappedCognosMode(collection[24], "WPT", "FEA");
			}
		}

		void SetupForTestCognosModeTable()
		{
			CognosModeMapping modeMapping = JASDataRegistry.Instance.CognosModeMapping;
			modeMapping.SelectedMode = nameof(CognosModes.AE);
			modeMapping.MapDepartments(GetDeptsByCodes("DEL", "LEA", "DES"));
			modeMapping.SelectedMode = nameof(CognosModes.PR);
			modeMapping.MapDepartments(GetDeptsByCodes("TEA", "TOT", "LES"));
			modeMapping.SelectedMode = nameof(CognosModes.OTH);
			modeMapping.MapDepartments(GetDeptsByCodes("TES", "FIS", "FDR"));
			modeMapping.SelectedMode = nameof(CognosModes.MI);
			modeMapping.MapDepartments(GetDeptsByCodes("CIR", "TIA", "TIS"));
			modeMapping.SelectedMode = nameof(CognosModes.NV);
			modeMapping.MapDepartments(GetDeptsByCodes("CER", "FIA", "WFS"));
			modeMapping.SelectedMode = nameof(CognosModes.AI);
			modeMapping.MapDepartments(GetDeptsByCodes("FER", "CEA", "TLL"));
			modeMapping.SelectedMode = nameof(CognosModes.WPT);
			modeMapping.MapDepartments(GetDeptsByCodes("FEA"));
			modeMapping.SelectedMode = nameof(CognosModes.CHB);
			modeMapping.MapDepartments(GetDeptsByCodes("CXB", "WBS", "CPP"));
			modeMapping.SelectedMode = nameof(CognosModes.ME);
			modeMapping.MapDepartments(GetDeptsByCodes("DEA", "DIA", "DIS"));
			JASDataRegistry.Instance.CognosModeMappingItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, modeMapping);
		}

		void AssertMappedCognosMode(DynamicBusinessObject bizO, ZString expectedT3_Mode, ZString expectedDeptCode)
		{
			AssertEquals(expectedDeptCode, bizO[GlbDepartmentSchema.GE_Code]);
			AssertEquals(expectedT3_Mode, bizO["T3_Mode"]);
		}

		GlbDepartment[] GetDeptsByCodes(params ZString[] codes)
		{
			ZQuery filter = new ZQuery();
			filter.DefaultJoinCondition = JoinCondition.Or;
			foreach (ZString code in codes)
			{
				filter.AddToFilter(GlbDepartmentSchema.GE_Code, codes);
			}

			return Factory.Load<GlbDepartment>(filter);
		}

		#endregion
		public void TestCognosRawAggregate()
		{
			using (new CognosTempTableCreator())
			{
				Guid testGuid = Guid.NewGuid();
				Guid creditorGroupPK = Factory.LoadTop1<OrgCreditorGroup>(new ZQuery()).PK.ToGuid();
				Guid debtorGroupPK = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery()).PK.ToGuid();
				string commandText = string.Format("INSERT INTO #CognosRawAggregate VALUES ('{0}', '30', 'AUCOR','{1}', '{2}', 'AI', 'SYD', 'TEL', 23, 'AUD', 44, 'NAM')", testGuid, creditorGroupPK, debtorGroupPK);
				ExecuteNonQuery(commandText);
				using (var reader = ExecuteReader("SELECT * FROM #CognosRawAggregate"))
				{
					reader.Read();
					AssertEquals("There should be 12 columns", 12, reader.FieldCount);
					AssertEquals(testGuid, reader["T5_AJ"]);
					AssertEquals("30", reader["T5_Age"]);
					AssertEquals("AUCOR", reader["T5_CompanyCode"]);
					AssertEquals(creditorGroupPK, reader["T5_OG_CreditorGroup"]);
					AssertEquals(debtorGroupPK, reader["T5_OJ_DebtorGroup"]);
					AssertEquals("AI", reader["T5_Mode"]);
					AssertEquals("SYD", reader["T5_Branch"]);
					AssertEquals("TEL", reader["T5_BusinessType"]);
					AssertEquals(23m, reader["T5_Amount"]);
					AssertEquals("AUD", reader["T5_TransactionCurrency"]);
					AssertEquals(44m, reader["T5_TransactionAmount"]);
					AssertEquals("NAM", reader["T5_Geographical"]);
					Assert("Should contain no other records than the test record", !reader.Read());
				}
			}
		}

		public void TestCognosExport()
		{
			using (new CognosTempTableCreator())
			{
				Guid testGuid = Guid.NewGuid();
				ExecuteNonQuery("INSERT INTO #CognosExport VALUES ('" + testGuid.ToString() + "', 'AUCOR', 'AI', 'SYD', 'TEL', 23, 'AUD', 44, 'NAM')");
				using (var reader = ExecuteReader("SELECT * FROM #CognosExport"))
				{
					reader.Read();
					AssertEquals("There should be 9 columns", 9, reader.FieldCount);
					AssertEquals(testGuid, reader["T6_AJ"]);
					AssertEquals("AUCOR", reader["T6_CompanyCode"]);
					AssertEquals("AI", reader["T6_Mode"]);
					AssertEquals("SYD", reader["T6_Branch"]);
					AssertEquals("TEL", reader["T6_BusinessType"]);
					AssertEquals(23m, reader["T6_Amount"]);
					AssertEquals("AUD", reader["T6_TransactionCurrency"]);
					AssertEquals(44m, reader["T6_TransactionAmount"]);
					AssertEquals("NAM", reader["T6_Geographical"]);
					Assert("Should contain no other records than the test record", !reader.Read());
				}
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			BranchFromOtherCompany = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "DEM");
			OtherCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
			OtherBranchFromCurrentCompany = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "TES");
			Assert("If this fails, change the code above to fetch other branch", OtherBranchFromCurrentCompany.PK != GlbBranch.CurrentBranch.PK);
		}

		void ExecuteNonQuery(string commandText)
		{
			DbCommand dbCommand = GetDbCommand(commandText);
			dbCommand.ExecuteNonQuery();
		}

		object ExecuteScalar(string commandText)
		{
			DbCommand dbCommand = GetDbCommand(commandText);
			return dbCommand.ExecuteScalar();
		}

		IDataReader ExecuteReader(string commandText)
		{
			DbCommand dbCommand = GetDbCommand(commandText);
			return dbCommand.ExecuteReader();
		}

		DbCommand GetDbCommand(string commandText)
		{
			return Db.Connection.Command(commandText);
		}

		void AssertTableExist(string additionalFailureMessage, string tableName, bool shouldExist)
		{
			object result = ExecuteScalar(string.Format("SELECT OBJECT_ID('tempdb..{0}')", tableName));
			AssertEquals(additionalFailureMessage + "Table should " + ((shouldExist) ? "" : "not ") + "exist", shouldExist, result != DBNull.Value);
		}

		GlbCompany OtherCompany;
		GlbBranch OtherBranchFromCurrentCompany;
		GlbBranch BranchFromOtherCompany;
		#endregion
	}
}
