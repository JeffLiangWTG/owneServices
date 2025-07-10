using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Accounting.Testing
{
	[TestedType(typeof(RectifyLocalTaxAmountOnPivotsForReversalTaxTransactions))]
	class RectifyLocalTaxAmountOnPivotsForReversalTaxTransactionsTest : DataTransformationTestCase
	{
		public void TestOnlinePostUpgradeTransformation_LogsProgress_WhenCancellationRequestedOnToken()
		{
			PrepareTestData();

			var chunkerEnumerator = GetGuidChunkerForDataCreatedInPrepareTestData();
			chunkerEnumerator.MoveNext();

			string logMessage = "";
			var transformation = GetNewTestTransformationInstance(2);
			transformation.Initialise(manager: new DummyUpgradeManager());
			AssertExceptionThrown<OperationCanceledException>("Pre-condition: Exception when cancellation requested on token", () => ((IOnlineTransformation)transformation).Run(s => logMessage = s, new CancellationToken(canceled: true)));

			AssertContains("log message", $"Last processed PK: {chunkerEnumerator.Current.UpperBound}.", logMessage);
		}

		public void TestOnlinePostUpgradeTransformation_ThrowsException_WhenCancellationRequestedOnToken()
		{
			PrepareTestDataWithOneInvoiceAndItsReversal();

			var transformation = GetNewTestTransformationInstance(1);
			transformation.Initialise(manager: new DummyUpgradeManager());
			AssertExceptionThrown<OperationCanceledException>("Exception when cancellation requested on token", () => transformation.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(canceled: true)));
		}

		public void TestOnlinePostUpgradeTransformation_ThrowsNoException_WhenCancellationNotRequestedOnToken()
		{
			PrepareTestDataWithOneInvoiceAndItsReversal();

			var transformation = GetNewTestTransformationInstance(1);
			AssertNoExceptionThrown(() => transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None));
		}

		public void TestOnlinePostUpgradeTransformation_ProcessConfiguredBatchSize_PerIteration()
		{
			PrepareTestData();
			List<Guid> guidsExpectedToBeProcessed = new List<Guid>();

			var transformation = GetNewTestTransformationInstance(2);
			transformation.Initialise(manager: new DummyUpgradeManager());

			var chunkerEnumerator = GetGuidChunkerForDataCreatedInPrepareTestData();

			chunkerEnumerator.MoveNext();
			guidsExpectedToBeProcessed.Add(chunkerEnumerator.Current.LowerBound);
			guidsExpectedToBeProcessed.Add(chunkerEnumerator.Current.UpperBound);
			AssertExceptionThrown<OperationCanceledException>("Pre-condition: Exception when cancellation requested on token", () => transformation.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(canceled: true)));

			AssertPivotsUpdated();

			chunkerEnumerator.MoveNext();
			guidsExpectedToBeProcessed.Add(chunkerEnumerator.Current.LowerBound);
			guidsExpectedToBeProcessed.Add(chunkerEnumerator.Current.UpperBound);
			AssertExceptionThrown<OperationCanceledException>("Pre-condition: Exception when cancellation requested on token", () => transformation.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(canceled: true)));

			AssertPivotsUpdated();

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertEquals("Pre-condition: number of reverse tax record pivots not 0", true, ReverseTaxRecordPivotsDataForAssert.IsCountMoreThan(0));

			foreach (var reversePivotData in ReverseTaxRecordPivotsDataForAssert)
			{
				AssertTaxRecordPivotLocaAmount(reversePivotData.Key, reversePivotData.Value.ExpectedCorrectTaxAmount);
			}

			void AssertPivotsUpdated()
			{
				AssertEquals("Pre-condition: number of reverse tax record pivots not 0", true, ReverseTaxRecordPivotsDataForAssert.IsCountMoreThan(0));
				foreach (var reversePivotData in ReverseTaxRecordPivotsDataForAssert)
				{
					if (guidsExpectedToBeProcessed.Contains(reversePivotData.Value.TaxTransactionPK))
					{
						AssertTaxRecordPivotLocaAmount(reversePivotData.Key, reversePivotData.Value.ExpectedCorrectTaxAmount);
					}
					else
					{
						AssertTaxRecordPivotLocaAmount(reversePivotData.Key, reversePivotData.Value.ActualTaxAmount);
					}
				}
			}
		}

		public void TestOnlinePostUpgradeTransformation_DeletesLastProcessedGuidForRecitfyTaxAmountOnPivotExtProp_AfterTransformation()
		{
			var lastProcessedGuidString = Guid.NewGuid().ToString();
			ExtProperty.Table.Update(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, "LastProcessedGuidForRecitfyTaxAmountOnPivot", lastProcessedGuidString);

			var lastProcessedGuidBeforeUpgrade = ExtProperty.Table.Select(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, "LastProcessedGuidForRecitfyTaxAmountOnPivot");
			AssertEquals("Pre-condition: Last processed Guid", lastProcessedGuidString, lastProcessedGuidBeforeUpgrade);

			RunOnlinePostUpgrade(isCancellationRequestedOnToken: false);

			var lastProcessedGuidAfterUpgrade = ExtProperty.Table.Select(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, "LastProcessedGuidForRecitfyTaxAmountOnPivot");
			AssertEquals("Last processed Guid", expected: true, string.IsNullOrEmpty(lastProcessedGuidAfterUpgrade));
		}

		public void TestOnlinePostUpgradeTransformation_DeletesTransactionsRowCountForRecitfyTaxAmountExtProp_AfterTransformation()
		{
			ExtProperty.Table.Update(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, "TransactionHeaderRowCountForRecitfyTaxAmountOnPivot", "2");

			var rowCountBeforeUpgrade = ExtProperty.Table.Select(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, "TransactionHeaderRowCountForRecitfyTaxAmountOnPivot");
			AssertEquals("Pre-condition: AccTaxtransaction row count", "2", rowCountBeforeUpgrade);

			RunOnlinePostUpgrade(isCancellationRequestedOnToken: false);

			var rowCountAfterUpgrade = ExtProperty.Table.Select(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, "TransactionHeaderRowCountForRecitfyTaxAmountOnPivot");
			AssertEquals("AccTaxtransaction row count", expected: true, string.IsNullOrEmpty(rowCountAfterUpgrade));
		}

		public void TestOnlinePostUpgradeTransformation_AmendingTransactionPivotAmountIsUnchanged()
		{
			// This test fails if the use of AccMatchLink table, for handling amending transactions correctly, is removed form the transformation query. 
			var (sql, company, branch, department, taxConfig, taxID) = GetSQLFor_GC_GC_GE_ETC_AT();

			var invoiceHeader = new AccTransactionHeader("AP", "INV", branch.PK, department.PK, company.PK) { AH_TransactionNum = "INV3001", AH_IsCancelled = true }.AppendInsertAndReturnObject(sql);
			var invoiceLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = invoiceHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
			var invoiceLIne2 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = invoiceHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
			var invoiceTaxtransaction1 = new AccTaxTransaction(invoiceHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = 10m, ATT_LocalTaxAmount = 10m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" }.AppendInsertAndReturnObject(sql);
			var invoiceTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(invoiceTaxtransaction1.PK, invoiceLine1.PK) { ATP_LocalTaxAmount = 4.55m }.AppendInsertAndReturnObject(sql);
			var invoiceTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(invoiceTaxtransaction1.PK, invoiceLIne2.PK) { ATP_LocalTaxAmount = 5.45m }.AppendInsertAndReturnObject(sql);

			var amendingHeader = new AccTransactionHeader("AP", "CRD", branch.PK, department.PK, company.PK) { AH_TransactionNum = "CRD3002", AH_IsCancelled = true, AH_TransactionBelongsToGroup = invoiceHeader.PK }.AppendInsertAndReturnObject(sql);
			var amendingLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = amendingHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
			var amendingLine2 = new AccTransactionLines("CST", 50m, department.PK, branch.PK, company.PK) { AL_AH = amendingHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
			var amendingTaxtransaction1 = new AccTaxTransaction(amendingHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = -5m, ATT_LocalTaxAmount = -5m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" }.AppendInsertAndReturnObject(sql);
			var amendingTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(amendingTaxtransaction1.PK, amendingLine1.PK) { ATP_LocalTaxAmount = -2.5m }.AppendInsertAndReturnObject(sql);
			var amendingTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(amendingTaxtransaction1.PK, amendingLine2.PK) { ATP_LocalTaxAmount = -2.5m }.AppendInsertAndReturnObject(sql);

			var reverseAmendingHeader = new AccTransactionHeader("AP", "INV", branch.PK, department.PK, company.PK) { AH_TransactionNum = "REV3002", AH_IsCancelled = true, AH_TransactionBelongsToGroup = amendingHeader.PK }.AppendInsertAndReturnObject(sql);
			var reverseAmendingLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseAmendingHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
			var reverseAmendingLine2 = new AccTransactionLines("CST", 50m, department.PK, branch.PK, company.PK) { AL_AH = reverseAmendingHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
			var reverseAmendingTaxtransaction1 = new AccTaxTransaction(reverseAmendingHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = 5m, ATT_LocalTaxAmount = 5m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" }.AppendInsertAndReturnObject(sql);
			var reverseAmendingTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(reverseAmendingTaxtransaction1.PK, reverseAmendingLine1.PK) { ATP_LocalTaxAmount = 0m }.AppendInsertAndReturnObject(sql);
			var reverseAmendingTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(reverseAmendingTaxtransaction1.PK, reverseAmendingLine2.PK) { ATP_LocalTaxAmount = 5m }.AppendInsertAndReturnObject(sql);

			var reverseInvoiceHeader = new AccTransactionHeader("AP", "CRD", branch.PK, department.PK, company.PK) { AH_TransactionNum = "REV3001", AH_IsCancelled = true, AH_TransactionBelongsToGroup = invoiceHeader.PK }.AppendInsertAndReturnObject(sql);
			var reverseInvoiceLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseInvoiceHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
			var reverseInvoiceLine2 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseInvoiceHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
			var reverseInvoiceTaxtransaction1 = new AccTaxTransaction(reverseInvoiceHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = -10m, ATT_LocalTaxAmount = -10m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" }.AppendInsertAndReturnObject(sql);
			var reverseInvoiceTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(reverseInvoiceTaxtransaction1.PK, reverseInvoiceLine1.PK) { ATP_LocalTaxAmount = 0m }.AppendInsertAndReturnObject(sql);
			var reverseInvoiceTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(reverseInvoiceTaxtransaction1.PK, reverseInvoiceLine2.PK) { ATP_LocalTaxAmount = -10m }.AppendInsertAndReturnObject(sql);

			new AccTransactionMatchLink(invoiceHeader.PK) { AP_MatchGroupNum = "M0003001" }.AppendInsertAndReturnObject(sql);
			new AccTransactionMatchLink(reverseInvoiceHeader.PK) { AP_MatchGroupNum = "M0003001" }.AppendInsertAndReturnObject(sql);
			new AccTransactionMatchLink(amendingHeader.PK) { AP_MatchGroupNum = "M0003002" }.AppendInsertAndReturnObject(sql);
			new AccTransactionMatchLink(reverseAmendingHeader.PK) { AP_MatchGroupNum = "M0003002" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var transformation = GetNewTestTransformationInstance(2);
			transformation.Initialise(manager: new DummyUpgradeManager());

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertTaxRecordPivotLocaAmount(reverseAmendingTaxRecordPivot11.PK, 2.5m);
			AssertTaxRecordPivotLocaAmount(reverseAmendingTaxRecordPivot12.PK, 2.5m);
			AssertTaxRecordPivotLocaAmount(reverseInvoiceTaxRecordPivot11.PK, -4.55m);
			AssertTaxRecordPivotLocaAmount(reverseInvoiceTaxRecordPivot12.PK, -5.45m);

			AssertTaxRecordPivotLocaAmount(amendingTaxRecordPivot11.PK, -2.5m);
			AssertTaxRecordPivotLocaAmount(amendingTaxRecordPivot12.PK, -2.5m);
		}

		public void TestOnlinePostUpgradeTransformation_FirstMatchingOriginalInvPivotIsUsedForUpdate_WhenMoreThanOneMatchFound()
		{
			var (sql, company, branch, department, taxConfig, taxID) = GetSQLFor_GC_GC_GE_ETC_AT();

			var invoiceHeader = new AccTransactionHeader("AP", "INV", branch.PK, department.PK, company.PK) { AH_TransactionNum = "INV4001", AH_IsCancelled = true }.AppendInsertAndReturnObject(sql);
			var invoiceLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = invoiceHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
			var invoiceLIne2 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = invoiceHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
			var invoiceTaxtransaction1 = new AccTaxTransaction(invoiceHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = 11m, ATT_LocalTaxAmount = 11m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" }.AppendInsertAndReturnObject(sql);
			// Ideally, the ATP_LocalTaxAmount should be same for both lines as both lines have exact same values for LineAmount & TaxID. However, a slight variation in the value, like below, might be possible in the case of Japanese Yen because of rounding issues.
			var invoiceTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(invoiceTaxtransaction1.PK, invoiceLine1.PK) { ATP_LocalTaxAmount = 5m }.AppendInsertAndReturnObject(sql);
			var invoiceTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(invoiceTaxtransaction1.PK, invoiceLIne2.PK) { ATP_LocalTaxAmount = 6m }.AppendInsertAndReturnObject(sql);

			var reverseInvoiceHeader = new AccTransactionHeader("AP", "CRD", branch.PK, department.PK, company.PK) { AH_TransactionNum = "CRD4001", AH_IsCancelled = true, AH_TransactionBelongsToGroup = invoiceHeader.PK }.AppendInsertAndReturnObject(sql);
			var reverseInvoiceLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseInvoiceHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
			var reverseInvoiceLine2 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseInvoiceHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
			var reverseInvoiceTaxtransaction1 = new AccTaxTransaction(reverseInvoiceHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = -11m, ATT_LocalTaxAmount = -11m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" }.AppendInsertAndReturnObject(sql);
			var reverseInvoiceTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(reverseInvoiceTaxtransaction1.PK, reverseInvoiceLine1.PK) { ATP_LocalTaxAmount = 0m }.AppendInsertAndReturnObject(sql);
			var reverseInvoiceTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(reverseInvoiceTaxtransaction1.PK, reverseInvoiceLine2.PK) { ATP_LocalTaxAmount = -11m }.AppendInsertAndReturnObject(sql);

			new AccTransactionMatchLink(invoiceHeader.PK) { AP_MatchGroupNum = "M0004001" }.AppendInsertAndReturnObject(sql);
			new AccTransactionMatchLink(reverseInvoiceHeader.PK) { AP_MatchGroupNum = "M0004001" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var transformation = GetNewTestTransformationInstance(2);
			transformation.Initialise(manager: new DummyUpgradeManager());

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertTaxRecordPivotLocaAmountAmongValues(reverseInvoiceTaxRecordPivot11.PK, new[] { -5m, -6m });
			// The correct local tax amount value on reverseInvoiceTaxRecordPivot12 should be -6m, but because of duplicate match resulting from duplicate sequence number on original invoice lines, always the first matched pivot is used for updating the local tax amount on the reverse tax record pivot.
			// This is a highly unlikely but technically possible scenario where 2 lines are created from 2 different charges having same sequence number. There's no way to distinguish between them in the transformation query. Hence, it was agreed with product that the first matched pivot will be used
			// for updating the local tax amount on the reverse tax record pivot in such a scenario.
			AssertTaxRecordPivotLocaAmountAmongValues(reverseInvoiceTaxRecordPivot12.PK, new[] { -5m, -6m });

			void AssertTaxRecordPivotLocaAmountAmongValues(Guid pivotPK, decimal[] localTaxAmountValues)
			{
				AccTaxRecordTransactionLinePivot.AssertFromDB(TestConnection, pivotPK).
					ExpectEquals("LocalTaxAmount", p => (p.ATP_LocalTaxAmount == localTaxAmountValues[0] || p.ATP_LocalTaxAmount == localTaxAmountValues[1]) , true)
					.VerifyAll();
			}
		}

		void RunOnlinePostUpgrade(bool isCancellationRequestedOnToken)
		{
			var transformation = GetNewTestTransformationInstance();
			transformation.Initialise(manager: new DummyUpgradeManager());
			transformation.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(canceled: isCancellationRequestedOnToken));
		}

		void AssertTaxRecordPivotLocaAmount(Guid pivotPK, decimal localTaxAmount)
		{
			AccTaxRecordTransactionLinePivot.AssertFromDB(TestConnection, pivotPK).
				ExpectEquals("LocalTaxAmount", p => p.ATP_LocalTaxAmount, localTaxAmount)
				.VerifyAll();
		}

		(StringBuilder sql, GlbCompany company, GlbBranch branch, GlbDepartment dept, AccTaxConfiguration taxConfig, AccTaxRate taxID) GetSQLFor_GC_GC_GE_ETC_AT()
		{
			var sql = new StringBuilder();

			var company = new GlbCompany("CMP", "AU").AppendInsertAndReturnObject(sql);
			var branch = new GlbBranch("BRN", company.PK).AppendInsertAndReturnObject(sql);
			var department = new GlbDepartment("DPT").AppendInsertAndReturnObject(sql);
			var taxConfig = new AccTaxConfiguration().AppendInsertAndReturnObject(sql);
			var taxID = new AccTaxRate().AppendInsertAndReturnObject(sql);

			return (sql, company, branch, department, taxConfig, taxID);
		}

		void PrepareTestDataWithOneInvoiceAndItsReversal()
		{
			var (sql, company, branch, department, taxConfig, taxID) = GetSQLFor_GC_GC_GE_ETC_AT();

			var originalHeader = new AccTransactionHeader("AP", "INV", branch.PK, department.PK, company.PK) { AH_TransactionNum = "INV1001", AH_IsCancelled = true }.AppendInsertAndReturnObject(sql);
			var originalLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = originalHeader.PK }.AppendInsertAndReturnObject(sql);
			var originalLine2 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = originalHeader.PK }.AppendInsertAndReturnObject(sql);
			var originalTaxtransaction1 = new AccTaxTransaction(originalHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_OSTaxAmount = 10m, ATT_LocalTaxAmount = 10m, ATT_IsCancelled = true }.AppendInsertAndReturnObject(sql);
			var originalTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(originalTaxtransaction1.PK, originalLine1.PK) { ATP_LocalTaxAmount = 4.55m }.AppendInsertAndReturnObject(sql);
			var originalTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(originalTaxtransaction1.PK, originalLine2.PK) { ATP_LocalTaxAmount = 5.45m }.AppendInsertAndReturnObject(sql);

			var reverseHeader = new AccTransactionHeader("AP", "CRD", branch.PK, department.PK, company.PK) { AH_TransactionNum = "CRD1001", AH_IsCancelled = true, AH_TransactionBelongsToGroup = originalHeader.PK }.AppendInsertAndReturnObject(sql);
			var reverseLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseHeader.PK }.AppendInsertAndReturnObject(sql);
			var reverseLine2 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseHeader.PK }.AppendInsertAndReturnObject(sql);
			var reverseTaxtransaction1 = new AccTaxTransaction(reverseHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_OSTaxAmount = -10m, ATT_LocalTaxAmount = -10m, ATT_IsCancelled = true }.AppendInsertAndReturnObject(sql);
			var reverseTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(reverseTaxtransaction1.PK, reverseLine1.PK) { ATP_LocalTaxAmount = 0m }.AppendInsertAndReturnObject(sql);
			var reverseTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(reverseTaxtransaction1.PK, reverseLine2.PK) { ATP_LocalTaxAmount = -10m }.AppendInsertAndReturnObject(sql);

			var originalTxnMatchLink = new AccTransactionMatchLink(originalHeader.PK) { AP_MatchGroupNum = "M0001001" }.AppendInsertAndReturnObject(sql);
			var reverseTxnMatchLink = new AccTransactionMatchLink(reverseHeader.PK) { AP_MatchGroupNum = "M0001001" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void PrepareTestData()
		{
			GetGuidChunkerForDataCreatedInPrepareTestData = () => GuidChunker.GenerateChunks(2, 44, Guid.Empty).GetEnumerator(); // 44 is the number of tax transactions created in this method.

			var chunkerEnumerator = GetGuidChunkerForDataCreatedInPrepareTestData();

			ReverseTaxRecordPivotsDataForAssert = new Dictionary<Guid, PivotDataForAssert>();

			var (sql, company, branch, department, taxConfig, taxID) = GetSQLFor_GC_GC_GE_ETC_AT();

			for (int i = 0; i < 5; i++) // original and reverse transaction Headers with 3 lines, 2 tax transactions with pivots linking lines 1 and 2 to taxtransaction 1 and a pivot linking line 3 to tax transaction 2.
			{
				var invoiceNum = $"INV{1000 + i}";
				var originalHeader = new AccTransactionHeader("AP", "INV", branch.PK, department.PK, company.PK) { AH_TransactionNum = invoiceNum, AH_IsCancelled = true }.AppendInsertAndReturnObject(sql);
				var originalLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = originalHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
				var originalLine2 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = originalHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
				var originalLine3 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = originalHeader.PK, AL_Sequence = 3 }.AppendInsertAndReturnObject(sql);
				var originalTaxtransaction1 = new AccTaxTransaction(originalHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = 10m, ATT_LocalTaxAmount = 10m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" }.AppendInsertAndReturnObject(sql);
				var originalTaxtransaction2 = new AccTaxTransaction(originalHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = 7.11m, ATT_LocalTaxAmount = 7.11m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD2" }.AppendInsertAndReturnObject(sql);
				var originalTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(originalTaxtransaction1.PK, originalLine1.PK) { ATP_LocalTaxAmount = 4.55m }.AppendInsertAndReturnObject(sql);
				var originalTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(originalTaxtransaction1.PK, originalLine2.PK) { ATP_LocalTaxAmount = 5.45m }.AppendInsertAndReturnObject(sql);
				var originalTaxRecordPivot23 = new AccTaxRecordTransactionLinePivot(originalTaxtransaction2.PK, originalLine3.PK) { ATP_LocalTaxAmount = 7.11m }.AppendInsertAndReturnObject(sql);

				chunkerEnumerator.MoveNext();
				var creditNoteNum = $"CRD{1000 + i}";
				var reverseHeader = new AccTransactionHeader("AP", "CRD", branch.PK, department.PK, company.PK) { AH_TransactionNum = creditNoteNum, AH_IsCancelled = true, AH_TransactionBelongsToGroup = originalHeader.PK }.AppendInsertAndReturnObject(sql);
				var reverseLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
				var reverseLine2 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
				var reverseLine3 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseHeader.PK, AL_Sequence = 3 }.AppendInsertAndReturnObject(sql);
				var reverseTaxtransaction1 = new AccTaxTransaction(reverseHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = -10m, ATT_LocalTaxAmount = -10m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" };
				reverseTaxtransaction1.SetPK(chunkerEnumerator.Current.LowerBound);
				reverseTaxtransaction1.AppendInsertAndReturnObject(sql);
				var reverseTaxtransaction2 = new AccTaxTransaction(reverseHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = -7.11m, ATT_LocalTaxAmount = -7.11m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD2" };
				reverseTaxtransaction2.SetPK(chunkerEnumerator.Current.UpperBound);
				reverseTaxtransaction2.AppendInsertAndReturnObject(sql);
				var reverseTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(reverseTaxtransaction1.PK, reverseLine1.PK) { ATP_LocalTaxAmount = 0m }.AppendInsertAndReturnObject(sql);
				var reverseTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(reverseTaxtransaction1.PK, reverseLine2.PK) { ATP_LocalTaxAmount = -10m }.AppendInsertAndReturnObject(sql);
				var reverseTaxRecordPivot23 = new AccTaxRecordTransactionLinePivot(reverseTaxtransaction2.PK, reverseLine3.PK) { ATP_LocalTaxAmount = -7.11m }.AppendInsertAndReturnObject(sql);
				ReverseTaxRecordPivotsDataForAssert.Add(reverseTaxRecordPivot11.PK, new PivotDataForAssert(reverseTaxtransaction1.PK, -4.55m, 0m));
				ReverseTaxRecordPivotsDataForAssert.Add(reverseTaxRecordPivot12.PK, new PivotDataForAssert(reverseTaxtransaction1.PK, -5.45m, -10m));
				ReverseTaxRecordPivotsDataForAssert.Add(reverseTaxRecordPivot23.PK, new PivotDataForAssert(reverseTaxtransaction2.PK, -7.11m, -7.11m));

				var matchGroupNum = $"M000{1000 + i}";
				var originalTxnMatchLink = new AccTransactionMatchLink(originalHeader.PK) { AP_MatchGroupNum = matchGroupNum }.AppendInsertAndReturnObject(sql);
				var reverseTxnMatchLink = new AccTransactionMatchLink(reverseHeader.PK) { AP_MatchGroupNum = matchGroupNum }.AppendInsertAndReturnObject(sql);
			}

			for (int i = 0; i < 5; i++) // original and reverse transaction Headers with 2 lines, 2 tax transaction and pivots linking both tax transactions to both lines.
			{
				var invoiceNum = $"INV{2000 + i}";
				var originalHeader = new AccTransactionHeader("AR", "INV", branch.PK, department.PK, company.PK) { AH_TransactionNum = invoiceNum, AH_IsCancelled = true };
				originalHeader.AppendInsertAndReturnObject(sql);
				var originalLine1 = new AccTransactionLines("REV", 100m, department.PK, branch.PK, company.PK) { AL_AH = originalHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
				var originalLine2 = new AccTransactionLines("REV", 100m, department.PK, branch.PK, company.PK) { AL_AH = originalHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
				var originalTaxtransaction1 = new AccTaxTransaction(originalHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AR", ATT_OSTaxAmount = 10m, ATT_LocalTaxAmount = 10m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" }.AppendInsertAndReturnObject(sql);
				var originalTaxtransaction2 = new AccTaxTransaction(originalHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AR", ATT_OSTaxAmount = 12.08m, ATT_LocalTaxAmount = 12.08m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD2" }.AppendInsertAndReturnObject(sql);
				var originalTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(originalTaxtransaction1.PK, originalLine1.PK) { ATP_LocalTaxAmount = 4.55m }.AppendInsertAndReturnObject(sql);
				var originalTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(originalTaxtransaction1.PK, originalLine2.PK) { ATP_LocalTaxAmount = 5.45m }.AppendInsertAndReturnObject(sql);
				var originalTaxRecordPivot21 = new AccTaxRecordTransactionLinePivot(originalTaxtransaction2.PK, originalLine1.PK) { ATP_LocalTaxAmount = 6.09m }.AppendInsertAndReturnObject(sql);
				var originalTaxRecordPivot22 = new AccTaxRecordTransactionLinePivot(originalTaxtransaction2.PK, originalLine2.PK) { ATP_LocalTaxAmount = 5.99m }.AppendInsertAndReturnObject(sql);

				chunkerEnumerator.MoveNext();
				var creditNoteNum = $"CRD{2000 + i}";
				var reverseHeader = new AccTransactionHeader("AR", "CRD", branch.PK, department.PK, company.PK) { AH_TransactionNum = creditNoteNum, AH_IsCancelled = true, AH_TransactionBelongsToGroup = originalHeader.PK };
				reverseHeader.AppendInsertAndReturnObject(sql);
				var reverseLine1 = new AccTransactionLines("REV", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
				var reverseLine2 = new AccTransactionLines("REV", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
				var reverseTaxtransaction1 = new AccTaxTransaction(reverseHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AR", ATT_OSTaxAmount = -10m, ATT_LocalTaxAmount = -10m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" };
				reverseTaxtransaction1.SetPK(chunkerEnumerator.Current.LowerBound);
				reverseTaxtransaction1.AppendInsertAndReturnObject(sql);
				var reverseTaxtransaction2 = new AccTaxTransaction(reverseHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AR", ATT_OSTaxAmount = -12.08m, ATT_LocalTaxAmount = -12.08m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD2" };
				reverseTaxtransaction2.SetPK(chunkerEnumerator.Current.UpperBound);
				reverseTaxtransaction2.AppendInsertAndReturnObject(sql);
				var reverseTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(reverseTaxtransaction1.PK, reverseLine1.PK) { ATP_LocalTaxAmount = 0m }.AppendInsertAndReturnObject(sql);
				var reverseTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(reverseTaxtransaction1.PK, reverseLine2.PK) { ATP_LocalTaxAmount = -10m }.AppendInsertAndReturnObject(sql);
				var reverseTaxRecordPivot21 = new AccTaxRecordTransactionLinePivot(reverseTaxtransaction2.PK, reverseLine1.PK) { ATP_LocalTaxAmount = 0m }.AppendInsertAndReturnObject(sql);
				var reverseTaxRecordPivot22 = new AccTaxRecordTransactionLinePivot(reverseTaxtransaction2.PK, reverseLine2.PK) { ATP_LocalTaxAmount = -12.08m }.AppendInsertAndReturnObject(sql);
				ReverseTaxRecordPivotsDataForAssert.Add(reverseTaxRecordPivot11.PK, new PivotDataForAssert(reverseTaxtransaction1.PK, -4.55m, 0m));
				ReverseTaxRecordPivotsDataForAssert.Add(reverseTaxRecordPivot12.PK, new PivotDataForAssert(reverseTaxtransaction1.PK, -5.45m, -10m));
				ReverseTaxRecordPivotsDataForAssert.Add(reverseTaxRecordPivot21.PK, new PivotDataForAssert(reverseTaxtransaction2.PK, -6.09m, 0m));
				ReverseTaxRecordPivotsDataForAssert.Add(reverseTaxRecordPivot22.PK, new PivotDataForAssert(reverseTaxtransaction2.PK, -5.99m, -12.08m));

				var matchGroupNum = $"M000{2000 + i}";
				var originalTxnMatchLink = new AccTransactionMatchLink(originalHeader.PK) { AP_MatchGroupNum = matchGroupNum }.AppendInsertAndReturnObject(sql);
				var reverseTxnMatchLink = new AccTransactionMatchLink(reverseHeader.PK) { AP_MatchGroupNum = matchGroupNum }.AppendInsertAndReturnObject(sql);
			}

			// Amending transaction case with both original and amending transactions reversed.

			{
				var invoiceHeader = new AccTransactionHeader("AP", "INV", branch.PK, department.PK, company.PK) { AH_TransactionNum = "INV3001", AH_IsCancelled = true }.AppendInsertAndReturnObject(sql);
				var invoiceLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = invoiceHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
				var invoiceLIne2 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = invoiceHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
				var invoiceTaxtransaction1 = new AccTaxTransaction(invoiceHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = 10m, ATT_LocalTaxAmount = 10m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" }.AppendInsertAndReturnObject(sql);
				var invoiceTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(invoiceTaxtransaction1.PK, invoiceLine1.PK) { ATP_LocalTaxAmount = 4.55m }.AppendInsertAndReturnObject(sql);
				var invoiceTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(invoiceTaxtransaction1.PK, invoiceLIne2.PK) { ATP_LocalTaxAmount = 5.45m }.AppendInsertAndReturnObject(sql);

				var amendingHeader = new AccTransactionHeader("AP", "CRD", branch.PK, department.PK, company.PK) { AH_TransactionNum = "CRD3002", AH_IsCancelled = true, AH_TransactionBelongsToGroup = invoiceHeader.PK }.AppendInsertAndReturnObject(sql);
				var amendingLine1 = new AccTransactionLines("CST", 50m, department.PK, branch.PK, company.PK) { AL_AH = amendingHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
				var amendingLine2 = new AccTransactionLines("CST", 50m, department.PK, branch.PK, company.PK) { AL_AH = amendingHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
				var amendingTaxtransaction1 = new AccTaxTransaction(amendingHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = -5m, ATT_LocalTaxAmount = -5m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" }.AppendInsertAndReturnObject(sql);
				var amendingTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(amendingTaxtransaction1.PK, amendingLine1.PK) { ATP_LocalTaxAmount = -2.55m }.AppendInsertAndReturnObject(sql);
				var amendingTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(amendingTaxtransaction1.PK, amendingLine2.PK) { ATP_LocalTaxAmount = -2.45m }.AppendInsertAndReturnObject(sql);

				chunkerEnumerator.MoveNext();
				var reverseAmendingHeader = new AccTransactionHeader("AP", "INV", branch.PK, department.PK, company.PK) { AH_TransactionNum = "REV3002", AH_IsCancelled = true, AH_TransactionBelongsToGroup = amendingHeader.PK }.AppendInsertAndReturnObject(sql);
				var reverseAmendingLine1 = new AccTransactionLines("CST", 50m, department.PK, branch.PK, company.PK) { AL_AH = reverseAmendingHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
				var reverseAmendingLine2 = new AccTransactionLines("CST", 50m, department.PK, branch.PK, company.PK) { AL_AH = reverseAmendingHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
				var reverseAmendingTaxtransaction1 = new AccTaxTransaction(reverseAmendingHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = 5m, ATT_LocalTaxAmount = 5m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" };
				reverseAmendingTaxtransaction1.SetPK(chunkerEnumerator.Current.LowerBound);
				reverseAmendingTaxtransaction1.AppendInsertAndReturnObject(sql);
				var reverseAmendingTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(reverseAmendingTaxtransaction1.PK, reverseAmendingLine1.PK) { ATP_LocalTaxAmount = 0m }.AppendInsertAndReturnObject(sql);
				var reverseAmendingTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(reverseAmendingTaxtransaction1.PK, reverseAmendingLine2.PK) { ATP_LocalTaxAmount = 5m }.AppendInsertAndReturnObject(sql);
				ReverseTaxRecordPivotsDataForAssert.Add(reverseAmendingTaxRecordPivot11.PK, new PivotDataForAssert(reverseAmendingTaxtransaction1.PK, 2.55m, 0m));
				ReverseTaxRecordPivotsDataForAssert.Add(reverseAmendingTaxRecordPivot12.PK, new PivotDataForAssert(reverseAmendingTaxtransaction1.PK, 2.45m, 5m));

				var reverseInvoiceHeader = new AccTransactionHeader("AP", "CRD", branch.PK, department.PK, company.PK) { AH_TransactionNum = "REV3001", AH_IsCancelled = true, AH_TransactionBelongsToGroup = invoiceHeader.PK }.AppendInsertAndReturnObject(sql);
				var reverseInvoiceLine1 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseInvoiceHeader.PK, AL_Sequence = 1 }.AppendInsertAndReturnObject(sql);
				var reverseInvoiceLine2 = new AccTransactionLines("CST", 100m, department.PK, branch.PK, company.PK) { AL_AH = reverseInvoiceHeader.PK, AL_Sequence = 2 }.AppendInsertAndReturnObject(sql);
				var reverseInvoiceTaxtransaction1 = new AccTaxTransaction(reverseInvoiceHeader.PK, company.PK, taxConfig.PK, taxID.PK, branch.PK, department.PK) { ATT_Ledger = "AP", ATT_OSTaxAmount = -10m, ATT_LocalTaxAmount = -10m, ATT_IsCancelled = true, ATT_TaxAuthorityServiceCode = "TAXSERCOD1" };
				reverseInvoiceTaxtransaction1.SetPK(chunkerEnumerator.Current.UpperBound);
				reverseInvoiceTaxtransaction1.AppendInsertAndReturnObject(sql);
				var reverseInvoiceTaxRecordPivot11 = new AccTaxRecordTransactionLinePivot(reverseInvoiceTaxtransaction1.PK, reverseInvoiceLine1.PK) { ATP_LocalTaxAmount = 0m }.AppendInsertAndReturnObject(sql);
				var reverseInvoiceTaxRecordPivot12 = new AccTaxRecordTransactionLinePivot(reverseInvoiceTaxtransaction1.PK, reverseInvoiceLine2.PK) { ATP_LocalTaxAmount = -10m }.AppendInsertAndReturnObject(sql);
				ReverseTaxRecordPivotsDataForAssert.Add(reverseInvoiceTaxRecordPivot11.PK, new PivotDataForAssert(reverseInvoiceTaxtransaction1.PK, -4.55m, 0m));
				ReverseTaxRecordPivotsDataForAssert.Add(reverseInvoiceTaxRecordPivot12.PK, new PivotDataForAssert(reverseInvoiceTaxtransaction1.PK, -5.45m, -10m));

				new AccTransactionMatchLink(invoiceHeader.PK) { AP_MatchGroupNum = "M0003001" }.AppendInsertAndReturnObject(sql);
				new AccTransactionMatchLink(reverseInvoiceHeader.PK) { AP_MatchGroupNum = "M0003001" }.AppendInsertAndReturnObject(sql);
				new AccTransactionMatchLink(amendingHeader.PK) { AP_MatchGroupNum = "M0003002" }.AppendInsertAndReturnObject(sql);
				new AccTransactionMatchLink(reverseAmendingHeader.PK) { AP_MatchGroupNum = "M0003002" }.AppendInsertAndReturnObject(sql);
			}

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		delegate IEnumerator<GuidChunk> GetGuidChunker();
		GetGuidChunker GetGuidChunkerForDataCreatedInPrepareTestData { get; set; }

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RectifyLocalTaxAmountOnPivotsForReversalTaxTransactions();
		}

		DataTransformation GetNewTestTransformationInstance(int batchSize)
		{
			return new RectifyLocalTaxAmountOnPivotsForReversalTaxTransactions(batchSize);
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("Pre-condtion: Number of reverse tax record pivots not 0", true, ReverseTaxRecordPivotsDataForAssert.IsCountMoreThan(0));
			foreach (var reversePivotData in ReverseTaxRecordPivotsDataForAssert)
			{
				AssertTaxRecordPivotLocaAmount(reversePivotData.Key, reversePivotData.Value.ExpectedCorrectTaxAmount);
			}
		}

		Dictionary<Guid, PivotDataForAssert> ReverseTaxRecordPivotsDataForAssert { get; set; }

		class PivotDataForAssert
		{
			public PivotDataForAssert(Guid taxTransactionPK, decimal expectedCorrectTaxAmount, decimal actualTaxAmounbt)
			{
				TaxTransactionPK = taxTransactionPK;
				ExpectedCorrectTaxAmount = expectedCorrectTaxAmount;
				ActualTaxAmount = actualTaxAmounbt;
			}

			public Guid TaxTransactionPK { get; set; }
			public decimal ExpectedCorrectTaxAmount { get; set; }

			public decimal ActualTaxAmount { get; set; }
		}
	}
}
