using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR.Testing
{
	[TestedType(typeof(SetJLT_MethodOfCalculationAndClearJLT_RateOverrideReasonCode))]
	sealed class SetJLT_MethodOfCalculationAndClearJLT_RateOverrideReasonCodeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new SetJLT_MethodOfCalculationAndClearJLT_RateOverrideReasonCode();

		public override void TestNewIndex()
		{
			new TransformationTestDataCreator().CreateGlbCompany("~BR", "BR");
			base.TestNewIndex();
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Clear JLT_RateOverrideReasonCode on JobComInvoiceLineTax and set JLT_MethodOfCalculation for BR Cust_1] ON [dbo].[JobComInvoiceLine] ([JI_DataModel]) INCLUDE ([JI_ClusterKey], [JI_PK]) WHERE ([JI_DataModel] = 'BR') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Clear JLT_RateOverrideReasonCode on JobComInvoiceLineTax and set JLT_MethodOfCalculation for BR Cust_2] ON [dbo].[JobComInvoiceLineTax] ([JLT_MethodOfCalculation], [JLT_Type]) INCLUDE ([JLT_ClusterKey], [JLT_JI], [JLT_RateOverrideReasonCode], [JLT_SystemLastEditTimeUtc], [JLT_SystemLastEditUser]) WHERE ([JLT_Type] IN ('0086', '1038', '5602', '5629') AND [JLT_MethodOfCalculation] = '') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			CombineAssertions("When Country is BR, ClusterKey = 1", () =>
			{
				AssertEquals("ImportDuty JLT_MethodOfCalculation", SpecialCaseTaxType.AdValoremRate, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.ImportDuty, 1));
				AssertEquals("IPI JLT_MethodOfCalculation", SpecialCaseTaxType.TariffAgreement, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.IPI, 1));
				AssertEquals("Antidumping JLT_MethodOfCalculation", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.Antidumping, 1));
				AssertEquals("PIS JLT_MethodOfCalculation", SpecialCaseTaxType.Reduced, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.PIS, 1));
				AssertEquals("Cofins JLT_MethodOfCalculation", SpecialCaseTaxType.Reduced, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.Cofins, 1));

				AssertEquals("ImportDuty JLT_RateOverrideReasonCode", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.ImportDuty, 1));
				AssertEquals("IPI JLT_RateOverrideReasonCode", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.IPI, 1));
				AssertEquals("Antidumping JLT_RateOverrideReasonCode", RateOverrideReasonCodes.QuantityPerUnit, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.Antidumping, 1));
				AssertEquals("PIS JLT_RateOverrideReasonCode", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.PIS, 1));
				AssertEquals("Cofins JLT_RateOverrideReasonCode", RateOverrideReasonCodes.ReductionMargin, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.Cofins, 1));
			});

			CombineAssertions("When Country is AU, ClusterKey = 2", () =>
			{
				AssertEquals("ImportDuty JLT_MethodOfCalculation", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.ImportDuty, 2));
				AssertEquals("IPI JLT_MethodOfCalculation", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.IPI, 2));
				AssertEquals("Antidumping JLT_MethodOfCalculation", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.Antidumping, 2));
				AssertEquals("PIS JLT_MethodOfCalculation", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.PIS, 2));
				AssertEquals("Cofins JLT_MethodOfCalculation", SpecialCaseTaxType.Reduced, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.Cofins, 2));

				AssertEquals("ImportDuty JLT_RateOverrideReasonCode", RateOverrideReasonCodes.ExTariff, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.ImportDuty, 2));
				AssertEquals("IPI JLT_RateOverrideReasonCode", RateOverrideReasonCodes.FreeTradeAgreeentRate, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.IPI, 2));
				AssertEquals("Antidumping JLT_RateOverrideReasonCode", RateOverrideReasonCodes.QuantityPerUnit, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.Antidumping, 2));
				AssertEquals("PIS JLT_RateOverrideReasonCode", RateOverrideReasonCodes.ReducedRate, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.PIS, 2));
				AssertEquals("Cofins JLT_RateOverrideReasonCode", RateOverrideReasonCodes.ReductionMargin, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.Cofins, 2));
			});

			CombineAssertions("When Country is BR, ClusterKey = 3", () =>
			{
				AssertEquals("ImportDuty JLT_MethodOfCalculation", SpecialCaseTaxType.AdValoremRate, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.ImportDuty, 3));
				AssertEquals("IPI JLT_MethodOfCalculation", SpecialCaseTaxType.Reduced, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.IPI, 3));
				AssertEquals("Antidumping JLT_MethodOfCalculation", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.Antidumping, 3));
				AssertEquals("PIS JLT_MethodOfCalculation", SpecialCaseTaxType.AdValoremRate, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.PIS, 3));
				AssertEquals("Cofins JLT_MethodOfCalculation", SpecialCaseTaxType.Reduced, GetDataFromJobComInvoiceLineTax("JLT_MethodOfCalculation", RateCodes.Cofins, 3));

				AssertEquals("ImportDuty JLT_RateOverrideReasonCode", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.ImportDuty, 3));
				AssertEquals("IPI JLT_RateOverrideReasonCode", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.IPI, 3));
				AssertEquals("Antidumping JLT_RateOverrideReasonCode", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.Antidumping, 3));
				AssertEquals("PIS JLT_RateOverrideReasonCode", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.PIS, 3));
				AssertEquals("Cofins JLT_RateOverrideReasonCode", string.Empty, GetDataFromJobComInvoiceLineTax("JLT_RateOverrideReasonCode", RateCodes.Cofins, 3));
			});
		}

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);
			var creator = new TransformationTestDataCreator();

			var brCompanyPK = creator.CreateGlbCompany("~BR", "BR");
			var brBranchPK = creator.CreateGlbBranch("~BR", brCompanyPK);

			InsertDataForTesting(creator, helper, Guid.NewGuid(), brCompanyPK, brBranchPK, 1, "BR");
			InsertDataForTesting(creator, helper, Guid.NewGuid(), brCompanyPK, brBranchPK, 2, "AU");
			InsertDataForTesting(creator, helper, Guid.NewGuid(), brCompanyPK, brBranchPK, 3, "BR");
		}

		void InsertDataForTesting(TransformationTestDataCreator creator, TestDbHelper helper, Guid declarationPk, Guid companyPk, Guid branchPk, int clusterKey, string dataModelCountry)
		{
			creator.CreateDeclaration(declarationPk, clusterKey.ToString(), clusterKey, branchPk, companyPk, dataModel: dataModelCountry);
			var headerPk = creator.CreateJobComInvoiceHeader(branchPk, declarationPk, clusterKey, dataModel: dataModelCountry);
			var linePk = creator.CreateJobComInvoiceLine(headerPk, clusterKey, dataModel: dataModelCountry);

			InsertJobComInvoiceLineTax(helper, linePk, RateCodes.ImportDuty, clusterKey, clusterKey == 3 ? string.Empty : RateOverrideReasonCodes.ExTariff, string.Empty);
			InsertJobComInvoiceLineTax(helper, linePk, RateCodes.IPI, clusterKey, clusterKey == 3 ? string.Empty : RateOverrideReasonCodes.FreeTradeAgreeentRate, clusterKey == 3 ? SpecialCaseTaxType.Reduced : string.Empty);
			InsertJobComInvoiceLineTax(helper, linePk, RateCodes.Antidumping, clusterKey, clusterKey == 3 ? string.Empty : RateOverrideReasonCodes.QuantityPerUnit, string.Empty);
			InsertJobComInvoiceLineTax(helper, linePk, RateCodes.PIS, clusterKey, clusterKey == 3 ? string.Empty : RateOverrideReasonCodes.ReducedRate, string.Empty);
			InsertJobComInvoiceLineTax(helper, linePk, RateCodes.Cofins, clusterKey, clusterKey == 3 ? string.Empty : RateOverrideReasonCodes.ReductionMargin, SpecialCaseTaxType.Reduced);
		}

		void InsertJobComInvoiceLineTax(TestDbHelper helper, Guid linePk, string type, int clusterKey, string rateReasonCode, string methodOfCalculation)
		{
			helper.Insert(JobComInvoiceLineTaxSchema.Constants.TableName, new
			{
				JLT_PK = Guid.NewGuid(),
				JLT_JI = linePk,
				JLT_Type = type,
				JLT_ClusterKey = clusterKey,
				JLT_BaseQuantity = decimal.One,
				JLT_RateOverrideReasonCode = rateReasonCode,
				JLT_MethodOfCalculation = methodOfCalculation,
				JLT_SystemCreateTimeUtc = date,
				JLT_SystemLastEditTimeUtc = date,
				JLT_SystemCreateUser = "~E",
				JLT_SystemLastEditUser = "~E"
			});
		}

		DateTime date => new DateTime(2024, 10, 30, 12, 00, 00);

		static class RateCodes
		{
			public const string ImportDuty = "0086";
			public const string IPI = "1038";
			public const string Antidumping = "5529";
			public const string PIS = "5602";
			public const string Cofins = "5629";
		}

		static class RateOverrideReasonCodes
		{
			public const string ExTariff = "EXT";
			public const string FreeTradeAgreeentRate = "FTA";
			public const string ReductionMargin = "RMA";
			public const string ReducedRate = "RRA";
			public const string QuantityPerUnit = "QPU";
		}

		static class SpecialCaseTaxType
		{
			public const string QuantityPerUnit = "QPU";
			public const string Reduced = "RED";
			public const string Reduction = "MAR";
			public const string AdValoremRate = "ADV";
			public const string TariffAgreement = "FTA";
		}

		string GetDataFromJobComInvoiceLineTax(string column, string type, int clusterKey)
		{
			var sql = $@"
SELECT {column} FROM JobComInvoiceLineTax
WHERE JLT_Type = '{type}' AND JLT_ClusterKey = {clusterKey}";

			return TestConnection.ExecuteScalar<string>(sql);
		}
	}
}
