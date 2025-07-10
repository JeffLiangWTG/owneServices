using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.EU;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(Report_EuTemporaryStorageRegister))]
	sealed class Report_EuTemporaryStorageRegisterTest : CustomsReportDbCreateScriptTest
	{
		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.SmallDateTime, Report_EuTemporaryStorageRegisterParameters.PresentationDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_EuTemporaryStorageRegisterParameters.PresentationDateTo);
			yield return (SqlDbType.VarChar, Report_EuTemporaryStorageRegisterParameters.Status);
			yield return (SqlDbType.VarChar, Report_EuTemporaryStorageRegisterParameters.AppCode);
		}

		public void TestColumns() => CombineAssertions(() =>
		{
			AssertFunctionReturnExpectedValue("ApplicationCode", "SUM");
			AssertFunctionReturnExpectedValue("RegistrationNumber", "REF1");
			AssertFunctionReturnExpectedValue("CustomerReference", "IREF1");
			AssertFunctionReturnExpectedValue("PresentationDate", new DateTime(2023, 12, 25));
			AssertFunctionReturnExpectedValue("PreviousReferenceNumber", "PREF1");
			AssertFunctionReturnExpectedValue("PreviousReferenceType", "PRE");
			AssertFunctionReturnExpectedValue("StatusHeader", "OPN");
			AssertFunctionReturnExpectedValue("CustodianEoriBranch", "CI1");
			AssertFunctionReturnExpectedValue("CustodianEoriBranchNo", "0000");
			AssertFunctionReturnExpectedValue("LineStatus", "CST");
			AssertFunctionReturnExpectedValue("GoodsDescription", "DESC1");
			AssertFunctionReturnExpectedValue("DisposalEntTraderEoriBranch", "OWN");
			AssertFunctionReturnExpectedValue("DisposalEntTraderEoriBranchNo", "0000");
			AssertFunctionReturnExpectedValue("LimitDate", new DateTime(2023, 12, 31));
			AssertFunctionReturnExpectedValue("LineNumber", 1);
			AssertFunctionReturnExpectedValue("LocationOfGoods", "1");
			AssertFunctionReturnExpectedValue("OwnerReferenceNumber", "OWNREF");
			AssertFunctionReturnExpectedValue("OwnerReferenceType", "OWT");
			AssertFunctionReturnExpectedValue("PackageQty", 15);
			AssertFunctionReturnExpectedValue("ReferenceNumber", "SRTREF");
			AssertFunctionReturnExpectedValue("ReferenceType", "SRT");
			AssertFunctionReturnExpectedValue("TransactionDateTimeUTC", new DateTime(2023, 12, 6, 15, 16, 00));
			AssertFunctionReturnExpectedValue("TransactionType", "TRN");
		});

		public void TestFilterPresentationDate()
		{
			CreateSecondHeaderForFilter();
			var filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "REF1", "REF2" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber", (Report_EuTemporaryStorageRegisterParameters.PresentationDateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new[] { "REF1", "REF2" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber", (Report_EuTemporaryStorageRegisterParameters.PresentationDateFrom, new DateTime(2020, 01, 01)), (Report_EuTemporaryStorageRegisterParameters.PresentationDateTo, new DateTime(2024, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new[] { "REF1" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber", (Report_EuTemporaryStorageRegisterParameters.PresentationDateFrom, new DateTime(2020, 01, 05)), (Report_EuTemporaryStorageRegisterParameters.PresentationDateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber", (Report_EuTemporaryStorageRegisterParameters.PresentationDateFrom, new DateTime(2024, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new[] { "REF2" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber", (Report_EuTemporaryStorageRegisterParameters.PresentationDateTo, new DateTime(2024, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new[] { "REF1" }, filteredRows);
		}

		public void TestFilterStatus_IST()
		{
			CreateSecondHeaderForFilter();

			var filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "REF1", "REF2" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
				(Report_EuTemporaryStorageRegisterParameters.Status, "OPN"));
			AssertContainsExactElementsInAnyOrder(new[] { "REF1" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
				(Report_EuTemporaryStorageRegisterParameters.Status, "CLS"));
			AssertContainsExactElementsInAnyOrder(new[] { "REF2" }, filteredRows);
		}

		public void TestFilterStatus_SUM()
		{
			CreateThirdHeaderForFilter();

			CombineAssertions(() =>
			{
				var filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber");
				AssertContainsExactElementsInAnyOrder(new[] { "REF1", "REF_DEL", "REF_FIN", "REF_LCK", "REF_PAC", "REF_PRE", "REF_TST" }, filteredRows);

				filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
					(Report_EuTemporaryStorageRegisterParameters.Status, "DEL"));
				AssertContainsExactElementsInAnyOrder(new[] { "REF_DEL" }, filteredRows);

				filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
					(Report_EuTemporaryStorageRegisterParameters.Status, "FIN"));
				AssertContainsExactElementsInAnyOrder(new[] { "REF_FIN" }, filteredRows);

				filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
					(Report_EuTemporaryStorageRegisterParameters.Status, "LCK"));
				AssertContainsExactElementsInAnyOrder(new[] { "REF_LCK" }, filteredRows);

				filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
					(Report_EuTemporaryStorageRegisterParameters.Status, "PAC"));
				AssertContainsExactElementsInAnyOrder(new[] { "REF_PAC" }, filteredRows);

				filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
					(Report_EuTemporaryStorageRegisterParameters.Status, "PRE"));
				AssertContainsExactElementsInAnyOrder(new[] { "REF_PRE" }, filteredRows);

				filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
					(Report_EuTemporaryStorageRegisterParameters.Status, "TST"));
				AssertContainsExactElementsInAnyOrder(new[] { "REF_TST" }, filteredRows);

				filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
					(Report_EuTemporaryStorageRegisterParameters.Status, "NCM"));
				AssertContainsExactElementsInAnyOrder(new[] { "REF_LCK", "REF_PAC", "REF_PRE", "REF_TST" }, filteredRows);
			});
		}

		public void TestFilterAppCode()
		{
			CreateSecondHeaderForFilter();

			var filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "REF1", "REF2" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
				(Report_EuTemporaryStorageRegisterParameters.AppCode, "SUM"));
			AssertContainsExactElementsInAnyOrder(new[] { "REF1" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "RegistrationNumber",
				(Report_EuTemporaryStorageRegisterParameters.AppCode, "IST"));
			AssertContainsExactElementsInAnyOrder(new[] { "REF2" }, filteredRows);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new TestDbHelper(TestConnection);
			var header = CreateHeader(helper, "SUM", "REF1", "IREF1", new DateTime(2023, 12, 25), "PREF1", "PRE", "OPN");
			var line = CreateLine(helper, header, "CI1", "0000", "CST", "DESC1", "OWN", "0000", new DateTime(2023, 12, 31), 1, "1", "OWNREF", "OWT");
			CreateTransaction(helper, line, 15, "SRTREF", "SRT", new DateTime(2023, 12, 6, 15, 16, 00), "TRN");
		}

		void CreateSecondHeaderForFilter()
		{
			var helper = new TestDbHelper(TestConnection);
			var header2 = CreateHeader(helper, "IST", "REF2", "IREF2", new DateTime(2024, 12, 25), "PREF2", "PRE", "CLS");
			var line2 = CreateLine(helper, header2, "CI1", "0000", "OST", "DESC1", "OWN", "0000", new DateTime(2024, 12, 31), 1, "1", "OWNREF", "OWN");
			CreateTransaction(helper, line2, 15, "SRTREF", "SRT", new DateTime(2023, 12, 6, 15, 16, 00), "TRN");
		}

		void CreateThirdHeaderForFilter()
		{
			var helper = new TestDbHelper(TestConnection);

			var header1 = CreateHeader(helper, "SUM", "REF_DEL", "IREF1", new DateTime(2023, 10, 1), "PREF1", "PREF", "DEL");
			var header2 = CreateHeader(helper, "SUM", "REF_FIN", "IREF2", new DateTime(2023, 10, 2), "PREF2", "PREF", "FIN");
			var header3 = CreateHeader(helper, "SUM", "REF_LCK", "IREF3", new DateTime(2023, 10, 3), "PREF3", "PREF", "LCK");
			var header4 = CreateHeader(helper, "SUM", "REF_PAC", "IREF4", new DateTime(2023, 10, 4), "PREF4", "PREF", "PAC");
			var header5 = CreateHeader(helper, "SUM", "REF_PRE", "IREF5", new DateTime(2023, 10, 5), "PREF5", "PREF", "PRE");
			var header6 = CreateHeader(helper, "SUM", "REF_TST", "IREF6", new DateTime(2023, 10, 6), "PREF6", "PREF", "TST");

			var line1 = CreateLine(helper, header1, "CI1", "0001", "CST", "Goods A", "OWN", "0001", new DateTime(2023, 12, 31), 1, "1", "OWNREE", "OWT");
			var line2 = CreateLine(helper, header2, "CT2", "0002", "CST", "Goods B", "OWN", "0002", new DateTime(2023, 12, 31), 1, "1", "OWNREF", "OWT");
			var line3 = CreateLine(helper, header3, "CT3", "0003", "CST", "Goods C", "OWN", "0003", new DateTime(2023, 12, 31), 1, "1", "OWNREF", "OWT");
			var line4 = CreateLine(helper, header4, "CT4", "0004", "CST", "Goods D", "OWN", "0004", new DateTime(2023, 12, 31), 1, "1", "OWNREF", "OWT");
			var line5 = CreateLine(helper, header5, "CT5", "0005", "CST", "Goods E", "OWN", "0005", new DateTime(2023, 12, 31), 1, "1", "OWNREF", "OWT");
			var line6 = CreateLine(helper, header6, "CT6", "0006", "CST", "Goods F", "OWN", "0006", new DateTime(2023, 12, 31), 1, "1", "OWNREF", "OWT");

			CreateTransaction(helper, line1, 10, "SRTREF", "SRT", new DateTime(2023, 12, 6, 15, 16, 00), "TRN");
			CreateTransaction(helper, line2, 20, "SRTREF", "SRT", new DateTime(2023, 12, 6, 15, 16, 00), "TRN");
			CreateTransaction(helper, line3, 15, "SRTREF", "SRT", new DateTime(2023, 12, 6, 15, 16, 00), "TRN");
			CreateTransaction(helper, line4, 10, "SRTREF", "SRT", new DateTime(2023, 12, 6, 15, 16, 00), "TRN");
			CreateTransaction(helper, line5, 20, "SRTREF", "SRT", new DateTime(2023, 12, 6, 15, 16, 00), "TRN");
			CreateTransaction(helper, line6, 15, "SRTREF", "SRT", new DateTime(2023, 12, 6, 15, 16, 00), "TRN");
		}

		Guid CreateHeader(TestDbHelper helper, string appCode, string reference, string internalReference, DateTime presentationDate, string previousReference, string previousReferenceType, string status)
		{
			var headerId = Guid.NewGuid();
			helper.Insert(CusTempStorageRegHeaderSchema.Constants.TableName, new
			{
				SRH_PK = headerId,
				SRH_AppCode = appCode,
				SRH_Reference = reference,
				SRH_InternalReference = internalReference,
				SRH_PresentationDate = presentationDate,
				SRH_PreviousReference =  previousReference,
				SRH_PreviousReferenceType = previousReferenceType,
				SRH_Status = status,
			});
			return headerId;
		}

		Guid CreateLine(TestDbHelper helper, Guid headerId, string custodianIdentifier, string custodianBranchNo, string customsStatus, string goodsDescription, string goodsOwnerIdentifier, string ownerBranchNo, DateTime limitDate, int lineNumber, string locationOfGoods, string ownerReference, string ownerReferenceType)
		{
			var lineId = Guid.NewGuid();
			helper.Insert(CusTempStorageRegLineSchema.Constants.TableName, new
			{
				SRL_PK = lineId,
				SRL_SRH = headerId,
				SRL_CustodianIdentifier = custodianIdentifier,
				SRL_CustodianIdentifierBranchNo = custodianBranchNo,
				SRL_CustomsStatus = customsStatus,
				SRL_GoodsDescription = goodsDescription,
				SRL_GoodsOwnerIdentifier = goodsOwnerIdentifier,
				SRL_GoodsOwnerIdentifierBranchNo = ownerBranchNo,
				SRL_LimitDate = limitDate,
				SRL_LineNumber = lineNumber,
				SRL_LocationOfGoods = locationOfGoods,
				SRL_OwnerReference = ownerReference,
				SRL_OwnerReferenceType = ownerReferenceType,
			});
			return lineId;
		}

		void CreateTransaction(TestDbHelper helper, Guid lineId, int packageQty, string reference, string referenceType, DateTime createTimeUtc, string transactionType)
		{
			var transactionId = Guid.NewGuid();
			helper.Insert(CusTempStorageRegLineTransactionSchema.Constants.TableName, new
			{
				SRT_PK = transactionId,
				SRT_SRL = lineId,
				SRT_PackageQty = packageQty,
				SRT_Reference = reference,
				SRT_ReferenceType = referenceType,
				SRT_SystemCreateTimeUtc = createTimeUtc,
				SRT_TransactionType = transactionType,
			});
		}

		class Report_EuTemporaryStorageRegisterParameters
		{
			public const string PresentationDateFrom = "@PresentationDateFrom";
			public const string PresentationDateTo = "@PresentationDateTo";
			public const string Status = "@Status";
			public const string AppCode = "@AppCode";
		}
	}
}
