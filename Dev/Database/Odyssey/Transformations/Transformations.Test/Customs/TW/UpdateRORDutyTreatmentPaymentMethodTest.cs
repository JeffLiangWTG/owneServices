using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.TW;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Customs.TW.Testing
{
	[TestedType(typeof(UpdateRORDutyTreatmentPaymentMethod))]
	class UpdateRORDutyTreatmentPaymentMethodTest : DataTransformationTestCase
	{
		public override void TestNewIndex()
		{
			new TransformationTestDataCreator().CreateGlbCompany("~TW", "TW");
			base.TestNewIndex();
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update ROR Duty Treatment Payment Method_1] ON [dbo].[JobDeclaration] ([JE_DataModel]) WHERE ([JE_DataModel]='TW' AND [JE_MessageType] = 'IMP') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update ROR Duty Treatment Payment Method_2] ON [dbo].[JobComInvoiceLine] ([JI_DataModel], [JI_Procedure]) INCLUDE ([JI_AddInfo], [JI_CountryOfOrigin], [JI_PK], [JI_PrimaryPreference], [JI_SystemLastEditTimeUtc], [JI_SystemLastEditUser], [JI_Tariff]) WHERE ([JI_DataModel]='TW' AND [JI_Procedure] IN ('38', '3E')) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			AssertJobComInvoiceLineResults();
			AssertJobComInvoiceLineTaxResults();
		}

		void AssertJobComInvoiceLineResults()
		{
			var sql = @"
			SELECT
				JI_BrandName,
				DtyPymntMthd.Value AS DtyPymntMthd,
				VatPymntMthd.Value AS VatPymntMthd,
				TpfPymntMthd.Value AS TpfPymntMthd
			FROM
				[dbo].[JobComInvoiceLine]
				CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'DtyPymntMthd') as DtyPymntMthd
				CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'VatPymntMthd') as VatPymntMthd
				CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'TpfPymntMthd') as TpfPymntMthd";

			var resultList = new List<Tuple<string, string, string, string>>();

			TestConnection.ExecuteReader(sql, reader => resultList.Add(Tuple.Create((string)reader["JI_BrandName"], (string)reader["DtyPymntMthd"], (string)reader["VatPymntMthd"], (string)reader["TpfPymntMthd"])));

			AssertContainsExactElementsInAnyOrder(new[]
			{
				Tuple.Create("1", "CAS", "CAS", "CAS"),
				Tuple.Create("2", "ROR", "ROR", "ROR"),
				Tuple.Create("3", "CAS", "CAS", "CAS"),
				Tuple.Create("4", "DEF", "DEF", "DEF"),
				Tuple.Create("5", "ROR", "ROR", "ROR"),
				Tuple.Create("6", "CAS", "CAS", "CAS"),
				Tuple.Create("7", "DEF", "DEF", "DEF"),
				Tuple.Create("8", "CAS", "CAS", "CAS"),
			}, resultList);
		}

		void AssertJobComInvoiceLineTaxResults()
		{
			var sql = @"
			SELECT
				JLT_Type, JLT_Tariff, JLT_MethodOfPayment
			FROM
				[dbo].[JobComInvoiceLineTax]";

			var resultList = new List<Tuple<string, string, string>>();

			TestConnection.ExecuteReader(sql, reader => resultList.Add(Tuple.Create((string)reader["JLT_Type"], (string)reader["JLT_Tariff"], (string)reader["JLT_MethodOfPayment"])));
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Tuple.Create("SS", "qqq", "ROR"),
				Tuple.Create("CT", "CT123", "ROR"),
				Tuple.Create("AT", "qqq", "CAS"),
				Tuple.Create("TT", "qqq", "CAS"),
			}, resultList);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateRORDutyTreatmentPaymentMethod();

		protected override void PrepareTestData()
		{
			CreateTariffForJobJobComInvoiceLine();
			CreateTariffForJobJobComInvoiceLineTax();
			var creator = new TransformationTestDataCreator();
			var twCompanyPK = creator.CreateGlbCompany("~TW", "TW");
			var twBranchPK = creator.CreateGlbBranch("~TW", twCompanyPK);
			var org = creator.CreateOrg("OH1");
			var impDecl = creator.CreateJobDeclaration("TW", twBranchPK, twCompanyPK, org, "IMP", DateTime.Now, 1, "");
			var expDecl = creator.CreateJobDeclaration("TW", twBranchPK, twCompanyPK, org, "EXP", DateTime.Now, 2, "");
			creator.CreateCusEntryInstruction(impDecl, 1);
			creator.CreateCusEntryInstruction(expDecl, 2);
			var impInvoiceHeader = creator.CreateJobComInvoiceHeader(twBranchPK, impDecl, 1, "", "TW");
			var expInvoiceHeader = creator.CreateJobComInvoiceHeader(twBranchPK, expDecl, 2, "", "TW");
			var line1 = CreateJobComInvoiceLine(impInvoiceHeader, 1, "1", "37", "DtyPymntMthd=CAS*TpfPymntMthd=CAS*VatPymntMthd=CAS", "TC1", "CA", "PR1");
			var line2 = CreateJobComInvoiceLine(impInvoiceHeader, 1, "2", "38", "DtyPymntMthd=CAS*TpfPymntMthd=CAS*VatPymntMthd=CAS", "TC1", "CA", "PR1");
			var line3 = CreateJobComInvoiceLine(impInvoiceHeader, 1, "3", "38", "DtyPymntMthd=CAS*TpfPymntMthd=CAS*VatPymntMthd=CAS", "TC1", "CA", "PR2");
			var line4 = CreateJobComInvoiceLine(impInvoiceHeader, 1, "4", "38", "DtyPymntMthd=DEF*TpfPymntMthd=DEF*VatPymntMthd=DEF", "TC1", "CA", "PR1");
			var line5 = CreateJobComInvoiceLine(impInvoiceHeader, 1, "5", "3E", "DtyPymntMthd=CAS*TpfPymntMthd=CAS*VatPymntMthd=CAS", "TC1", "CA", "PR1");
			var line6 = CreateJobComInvoiceLine(impInvoiceHeader, 1, "6", "3E", "DtyPymntMthd=CAS*TpfPymntMthd=CAS*VatPymntMthd=CAS", "TC1", "CA", "PR2");
			var line7 = CreateJobComInvoiceLine(impInvoiceHeader, 1, "7", "3E", "DtyPymntMthd=DEF*TpfPymntMthd=DEF*VatPymntMthd=DEF", "TC1", "CA", "PR1");
			var line8 = CreateJobComInvoiceLine(expInvoiceHeader, 2, "8", "38", "DtyPymntMthd=CAS*TpfPymntMthd=CAS*VatPymntMthd=CAS", "TC1", "CA", "PR1");

			CreateJobComInvoiceLineTax(line3, 1, "SS", "qqq", "CAS");
			CreateJobComInvoiceLineTax(line3, 1, "CT", "CT123", "CAS");
			CreateJobComInvoiceLineTax(line3, 1, "AT", "qqq", "CAS");
			CreateJobComInvoiceLineTax(line3, 1, "TT", "qqq", "CAS");
		}

		void CreateTariffForJobJobComInvoiceLine()
		{
			var tariffPk1 = Guid.NewGuid();
			var tariffPk2 = Guid.NewGuid();
			var rateCodePk1 = Guid.NewGuid();
			var rateCodePk2 = Guid.NewGuid();
			var preferencePK1 = Guid.NewGuid();
			var preferencePK2 = Guid.NewGuid();
			var ratePk1 = new Guid("00000000-0000-0000-0000-000000000001");
			var ratePk2 = new Guid("00000000-0000-0000-0000-000000000002");
			var ratePk3 = new Guid("00000000-0000-0000-0000-000000000003");
			var ratePk4 = new Guid("00000000-0000-0000-0000-000000000004");
			var ratePk5 = new Guid("00000000-0000-0000-0000-000000000005");
			var ratePk6 = new Guid("00000000-0000-0000-0000-000000000006");

			var prepareTestDataSql = $@"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = newid();
				DECLARE @RateTypePK UNIQUEIDENTIFIER = newid();
				DECLARE @ParentDataGroupingPk UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupPK1 UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupCountryPK1 UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupCountryPK2 UNIQUEIDENTIFIER = newid();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (@ParentDataGroupingPk, 'TW', 'Taiwan');
				INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES (@TariffTypePK, 'TT1', 'TariffTypeOne', 'TW');

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES ('{tariffPk1}', @TariffTypePK, 'TC1', 'ZZ Tariff1', 'TW', '2021-01-01', '2079-06-06', '')

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES ('{tariffPk2}', @TariffTypePK, 'TC2', 'ZZ Tariff2', 'TW', '2021-01-01', '2079-06-06', '')

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePK, 'DTY', 'RateTypeOne', 'TW', '');

				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES('{rateCodePk1}', 'DTA', @RateTypePK, 'ZZ RateCode1')

				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES('{rateCodePk2}', 'DTS', @RateTypePK, 'ZZ RateCode2')

				INSERT INTO RefDatabase_RefCusPreference(ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
				VALUES('{preferencePK1}', 'PR1', 'ZZ Preference1', 'TW')

				INSERT INTO RefDatabase_RefCusPreference(ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
				VALUES('{preferencePK2}', 'PR2', 'ZZ Preference2', 'TW')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk1}', '{rateCodePk1}', '{tariffPk1}', '{preferencePK1}', 'TW', 'ZZ RateFormula1', '0.35', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk2}', '{rateCodePk1}', '{tariffPk1}', '{preferencePK2}', 'TW', 'ZZ RateFormula2', '68/KG', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk3}', '{rateCodePk2}', '{tariffPk1}', '{preferencePK1}', 'TW', 'ZZ RateFormula3', '79/KGM', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk4}', '{rateCodePk2}', '{tariffPk1}', '{preferencePK2}', 'TW', 'ZZ RateFormula4', '0.46', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk5}', '{rateCodePk1}', '{tariffPk2}', '{preferencePK1}', 'TW', 'ZZ RateFormula1', '79/KG', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk6}', '{rateCodePk2}', '{tariffPk2}', '{preferencePK2}', 'TW', 'ZZ RateFormula4', '90/KGM', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES (@tradeGroupPK1, 'TG', 'ZZ TG1', '2021-01-01', '2079-06-06', 'TW')

				INSERT INTO RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode)
				VALUES (@tradeGroupCountryPK1, @tradeGroupPK1, '2021-01-01', '2079-06-06', 'ZZ tradeGroupCountry1', 'CA')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk1}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk2}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk3}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk4}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk5}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (newid(), '{ratePk6}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '')
			";
			TestConnection.ExecuteNonQuery(prepareTestDataSql);
		}

		void CreateTariffForJobJobComInvoiceLineTax()
		{
			var prepareTestDataSql = @"
DECLARE @CTTariffTypePK UNIQUEIDENTIFIER = newid();
DECLARE @CTTariffPK UNIQUEIDENTIFIER = newid();
DECLARE @ATTariffPK UNIQUEIDENTIFIER = newid();
DECLARE @TTTariffPK UNIQUEIDENTIFIER = newid();
DECLARE @ComRateTypePK UNIQUEIDENTIFIER = newid();
DECLARE @DtaRateCodePk UNIQUEIDENTIFIER = newid();
DECLARE @CtaRateCodePk UNIQUEIDENTIFIER = newid();
DECLARE @PR1PreferencePk UNIQUEIDENTIFIER = (SELECT TOP 1 ZZS_PK FROM RefDatabase_RefCusPreference WHERE ZZS_Preference ='PR1' AND ZZS_ZZZ_NKDataGrouping = 'TW');

INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
				VALUES (@CTTariffTypePK, 'CT', 'Commodity Tax', 'TW');
INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES (@CTTariffPK, @CTTariffTypePK, 'CT123', 'CT Tariff', 'TW', '2021-01-01', '2079-06-06', '');
INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES (@ATTariffPK, @CTTariffTypePK, 'AT123', 'AT Tariff', 'TW', '2021-01-01', '2079-06-06', '');
INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES (@TTTariffPK, @CTTariffTypePK, 'TT123', 'TT Tariff', 'TW', '2021-01-01', '2079-06-06', '');

INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula)
	VALUES (@ComRateTypePK, 'COM', 'Commodity Taxes', 'TW', '');

INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES(@DtaRateCodePk, 'DTA', @ComRateTypePK, 'DTA RateCode1');

INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES(@CtaRateCodePk, 'CTA', @ComRateTypePK, 'CTA RateCode1');

INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES (newid(), @DtaRateCodePk, @CTTariffPK, @PR1PreferencePk, 'TW', 'ZZ RateFormula1', '0.35', '2021-01-01', '2029-12-31');

INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES (newid(), @DtaRateCodePk, @ATTariffPK, @PR1PreferencePk, 'TW', 'ZZ RateFormula1', '0.35', '2021-01-01', '2029-12-31');

INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_RateFormulaDerivedFrom, ZZ2_StartDate, ZZ2_EndDate)
				VALUES (newid(), @DtaRateCodePk, @TTTariffPK, @PR1PreferencePk, 'TW', 'ZZ RateFormula1', '0.35', '2021-01-01', '2029-12-31');
";
			TestConnection.ExecuteNonQuery(prepareTestDataSql);
		}

		const string CreateJobComInvoiceLineSql = @"INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_BrandName, JI_Procedure,JI_AddInfo, JI_Tariff, JI_CountryOfOrigin, JI_PrimaryPreference, JI_DataModel, JI_ClusterKey, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser)
				VALUES (@invoiceLinePK, @invoiceHeaderPK, @brandName, @procedure, @addinfo, @tariff, @countryOfOrigin, @primaryPreference, 'TW', @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobComInvoiceLine(Guid headerPk, int clusterKey, string brandName, string procedure, string addinfo, string tariff, string countryOfOrigin, string primaryPreference)
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateJobComInvoiceLineSql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, headerPk);
				command.AddParameter("@brandName", SqlDbType.VarChar, brandName);
				command.AddParameter("@procedure", SqlDbType.VarChar, procedure);
				command.AddParameter("@addinfo", SqlDbType.VarChar, addinfo);
				command.AddParameter("@tariff", SqlDbType.VarChar, tariff);
				command.AddParameter("@countryOfOrigin", SqlDbType.VarChar, countryOfOrigin);
				command.AddParameter("@primaryPreference", SqlDbType.VarChar, primaryPreference);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		const string CreateJobComInvoiceLineTaxSql = @"INSERT INTO dbo.JobComInvoiceLineTax(JLT_PK, JLT_JI, JLT_Type, JLT_Tariff, JLT_MethodOfPayment, JLT_ClusterKey, JLT_SystemCreateTimeUtc, JLT_SystemCreateUser, JLT_SystemLastEditTimeUtc, JLT_SystemLastEditUser)
				VALUES (@invoiceLineTaxPK, @invoiceLinePK, @type, @tariff, @paymentMethod, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
		public Guid CreateJobComInvoiceLineTax(Guid invoiceLinePK, int clusterKey, string type, string tariff, string paymentMethod)
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateJobComInvoiceLineTaxSql))
			{
				command.AddParameter("@invoiceLineTaxPK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@type", SqlDbType.VarChar, type);
				command.AddParameter("@tariff", SqlDbType.VarChar, tariff);
				command.AddParameter("@paymentMethod", SqlDbType.VarChar, paymentMethod);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return pk;
		}
	}
}
