using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Export.Business.TaxFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Export.Business.Testing
{
	public class BatchExportDataAccessTest : TestCaseWithFactory
	{
		public void TestGetEnableGovernmentChargeCode_IsWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = true;
			TestGetEnableGovernmentChargeCodeCore(true);
		}

		public void TestGetEnableGovernmentChargeCode_IsNonWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = false;
			TestGetEnableGovernmentChargeCodeCore(false);
		}

		void TestGetEnableGovernmentChargeCodeCore(bool isWeb)
		{
			var batchExportDataAccess1 = new BatchExportDataAccess(Connection, Transaction);

			var companyForIN = TestObjectCreator.CreateCompanyAndBranch("IN");
			AssertEquals("IN registry default value", true, GetEnableGovernmentChargeCode(companyForIN.GC_Code, batchExportDataAccess1));
			var companyForAU = TestObjectCreator.CreateCompanyAndBranch("AU");
			AssertEquals("AU registry default value", false, GetEnableGovernmentChargeCode(companyForAU.GC_Code, batchExportDataAccess1));

			var batchExportDataAccess2 = new BatchExportDataAccess(Connection, Transaction);

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(companyForIN.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(companyForAU.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("IN registry value", false, GetEnableGovernmentChargeCode(companyForIN.GC_Code, batchExportDataAccess2));
				AssertEquals("AU registry value", true, GetEnableGovernmentChargeCode(companyForAU.GC_Code, batchExportDataAccess2));
			}

			TestConnection.ExecuteNonQuery($@"
DELETE FROM dbo.StmData WHERE SD_Name = 'EnableGovernmentChargeCode' AND SD_Owner = '{companyForAU.PK.ToString()}'
INSERT INTO dbo.StmData (SD_PK, SD_Name , SD_Owner , SD_Type, SD_BinaryValue ) VALUES (NEWID(), 'EnableGovernmentChargeCode' , '{companyForAU.PK.ToString()}' , 'BOL', convert(varbinary(max), N'False'));");

			AssertEquals("registry value in the cache", true, GetEnableGovernmentChargeCode(companyForAU.GC_Code, batchExportDataAccess2));

			var batchExportDataAccess3 = new BatchExportDataAccess(Connection, Transaction);
			AssertEquals("The latest registry value in the new BatchExportDataAccess Object", false, GetEnableGovernmentChargeCode(companyForAU.GC_Code, batchExportDataAccess3));

			var batchExportDataAccess4 = new BatchExportDataAccess(Connection, Transaction);
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(companyForAU.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("registry value", true, GetEnableGovernmentChargeCodeRegistryValue(companyForAU));

				TestConnection.ExecuteNonQuery($@"
DELETE FROM dbo.StmData WHERE SD_Name = 'EnableGovernmentChargeCode' AND SD_Owner = '{companyForAU.PK.ToString()}'
INSERT INTO dbo.StmData (SD_PK, SD_Name , SD_Owner , SD_Type, SD_BinaryValue ) VALUES (NEWID(), 'EnableGovernmentChargeCode' , '{companyForAU.PK.ToString()}' , 'BOL', convert(varbinary(max), N'False'));");

				if (isWeb)
				{
					AssertEquals("registry value should be up to date when in Web environment", false, GetEnableGovernmentChargeCode(companyForAU.GC_Code, batchExportDataAccess4));
				}
				else
				{
					AssertEquals("registry value should be cached when value from non-Web", true, GetEnableGovernmentChargeCode(companyForAU.GC_Code, batchExportDataAccess4));
				}
			}

			bool GetEnableGovernmentChargeCodeRegistryValue(GlbCompany company)
			{
				return AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			}

			bool GetEnableGovernmentChargeCode(string companyCode, BatchExportDataAccess batchExportDataAccess)
			{
				var methodInfo = batchExportDataAccess.GetType().GetMethod("GetEnableGovernmentChargeCode", BindingFlags.NonPublic | BindingFlags.Instance);
				return (bool)methodInfo.Invoke(batchExportDataAccess, new object[] { companyCode });
			}
		}

		public void TestGetTaxGroupCodes_IsWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = true;
			TestGetTaxGroupCodesCore(true);
		}

		public void TestGetTaxGroupCodes_IsNonWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = false;
			TestGetTaxGroupCodesCore(false);
		}

		void TestGetTaxGroupCodesCore(bool isWeb)
		{
			var batchExportDataAccess1 = new BatchExportDataAccess(Connection, Transaction);
			var companyForIN = TestObjectCreator.CreateCompanyAndBranch("IN");
			AssertContainsExactElementsInAnyOrder("IN registry default value", Array.Empty<string>(), GetTaxGroupCodes(companyForIN.GC_Code, batchExportDataAccess1));
			var companyForAU = TestObjectCreator.CreateCompanyAndBranch("AU");
			AssertContainsExactElementsInAnyOrder("AU registry default value", Array.Empty<string>(), GetTaxGroupCodes(companyForAU.GC_Code, batchExportDataAccess1));

			var batchExportDataAccess2 = new BatchExportDataAccess(Connection, Transaction);

			var taxMessageGroupsManagementForIN = new CodeDescriptionBoolRelatedItemCollection();
			taxMessageGroupsManagementForIN.Add("N1", (NoResString)"Description N1", true, "N1.0");
			taxMessageGroupsManagementForIN.Add("N2", (NoResString)"Description N2", false, "N2.0");

			var taxMessageGroupsManagementForAU = new CodeDescriptionBoolRelatedItemCollection();
			taxMessageGroupsManagementForAU.Add("N1", (NoResString)"Description N1", true, "N1.0");
			taxMessageGroupsManagementForAU.Add("N2", (NoResString)"Description N2", false, "N2.0");
			taxMessageGroupsManagementForAU.Add("N3", (NoResString)"Description N3", true, "N3.0");

			using (AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(companyForIN.PK.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagementForIN))
			using (AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(companyForAU.PK.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagementForAU))
			{
				AssertContainsExactElementsInAnyOrder("IN registry value", new string[] { "N1" }, GetTaxGroupCodes(companyForIN.GC_Code, batchExportDataAccess2));
				AssertContainsExactElementsInAnyOrder("AU registry value", new string[] { "N1", "N3" }, GetTaxGroupCodes(companyForAU.GC_Code, batchExportDataAccess2));
			}

			TestConnection.ExecuteNonQuery($"DELETE FROM dbo.StmData WHERE SD_Name = 'TaxMessageGroupsManagement' AND SD_Owner = '{companyForAU.PK.ToString()}'");

			AssertContainsExactElementsInAnyOrder("registry value in the cache", new string[] { "N1", "N3" }, GetTaxGroupCodes(companyForAU.GC_Code, batchExportDataAccess2));

			var batchExportDataAccess3 = new BatchExportDataAccess(Connection, Transaction);
			AssertContainsExactElementsInAnyOrder("The latest registry value in the new BatchExportDataAccess Object", Array.Empty<string>(), GetTaxGroupCodes(companyForAU.GC_Code, batchExportDataAccess3));

			var batchExportDataAccess4 = new BatchExportDataAccess(Connection, Transaction);
			using (AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(companyForAU.PK.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagementForAU))
			{
				AssertContainsExactElementsInAnyOrder("registry value", new string[] { "N1", "N3" }, GetTaxGroupCodesRegistryValue(companyForAU));

				TestConnection.ExecuteNonQuery($"DELETE FROM dbo.StmData WHERE SD_Name = 'TaxMessageGroupsManagement' AND SD_Owner = '{companyForAU.PK.ToString()}'");

				if (isWeb)
				{
					AssertContainsExactElementsInAnyOrder("registry value should be up to date when in Web environment", Array.Empty<string>(), GetTaxGroupCodes(companyForAU.GC_Code, batchExportDataAccess4));
				}
				else
				{
					AssertContainsExactElementsInAnyOrder("registry value should be cached when value from non-Web", new string[] { "N1", "N3" }, GetTaxGroupCodes(companyForAU.GC_Code, batchExportDataAccess4));
				}
			}

			IEnumerable<string> GetTaxGroupCodesRegistryValue(GlbCompany company)
			{
				return AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList().Cast<ICodeDescription>().Select(x => x.Code);
			}

			IEnumerable<string> GetTaxGroupCodes(string companyCode, BatchExportDataAccess batchExportDataAccess)
			{
				var methodInfo = batchExportDataAccess.GetType().GetMethod("GetTaxGroupCodes", BindingFlags.NonPublic | BindingFlags.Instance);
				var result = (ICodeDescriptionPairList)methodInfo.Invoke(batchExportDataAccess, new object[] { companyCode });
				return result.Cast<ICodeDescription>().Select(x => x.Code);
			}
		}

		#region TestGetNoteGLAccountsStatisticalUnitsofMeasurement

		public void TestGetNoteGLAccountsStatisticalUnitsofMeasurement()
		{
			ZArchitecture.Environment.Globals.IsWeb = true;

			TestCodeDescriptionCodeBehaviourCaching(
				isWeb: true,
				AccountingConfigurationRegistry.Instance.NoteGLAccountsStatisticalUnitsofMeasurement,
				(dataAccess, companyCode) => dataAccess.GetNoteGLAccountsStatisticalUnitsofMeasurement_ForTestOnly(companyCode),
				testDatasetForAU: CreateTestDatasetForGetNoteGLAccountsStatisticalUnitsofMeasurementAU(),
				testDatasetForIN: CreateTestDatasetForGetNoteGLAccountsStatisticalUnitsofMeasurementIN()
			);
		}

		public void TestGetNoteGLAccountsStatisticalUnitsofMeasurement_IsNonWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = false;

			TestCodeDescriptionCodeBehaviourCaching(
				isWeb: false,
				AccountingConfigurationRegistry.Instance.NoteGLAccountsStatisticalUnitsofMeasurement,
				(dataAccess, companyCode) => dataAccess.GetNoteGLAccountsStatisticalUnitsofMeasurement_ForTestOnly(companyCode),
				testDatasetForAU: CreateTestDatasetForGetNoteGLAccountsStatisticalUnitsofMeasurementAU(),
				testDatasetForIN: CreateTestDatasetForGetNoteGLAccountsStatisticalUnitsofMeasurementIN()
			);
		}

		static CodeDescriptionPairList CreateTestDatasetForGetNoteGLAccountsStatisticalUnitsofMeasurementIN()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("KWH", "Kilowatt Hours");
			result.AddPair("KG", "Kilograms");
			return result;
		}

		static CodeDescriptionPairList CreateTestDatasetForGetNoteGLAccountsStatisticalUnitsofMeasurementAU()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("TON", "Ton");
			return result;
		}

		#endregion

		#region TestGetReversalReasonCodesList

		public void TestGetReversalReasonCodesList_WhenWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = true;
			TestCodeDescriptionCodeBehaviourCaching(
				isWeb: true,
				AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList,
				(dataAccess, companyCode) => dataAccess.GetReversalReasonCodesList_ForTestOnly(companyCode)
			);
		}

		public void TestGetReversalReasonCodesList_WhenNotWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = false;
			TestCodeDescriptionCodeBehaviourCaching(
				isWeb: false,
				AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList,
				(dataAccess, companyCode) => dataAccess.GetReversalReasonCodesList_ForTestOnly(companyCode)
			);
		}

		#endregion

		#region TestGetCreditNoteReasonCodesList

		public void TestGetCreditNoteReasonCodesList_WhenWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = true;
			TestCodeDescriptionCodeBehaviourCaching(
				isWeb: true,
				AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList,
				(dataAccess, companyCode) => dataAccess.GetCreditNoteReasonCodesList_ForTestOnly(companyCode)
			);
		}

		public void TestGetCreditNoteReasonCodesList_WhenNotWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = false;
			TestCodeDescriptionCodeBehaviourCaching(
				isWeb: false,
				AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList,
				(dataAccess, companyCode) => dataAccess.GetCreditNoteReasonCodesList_ForTestOnly(companyCode)
			);
		}

		#endregion

		#region TestGetAmendmentReasonCodesList

		public void TestGetAmendmentReasonCodesList_WhenWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = true;
			TestCodeDescriptionCodeBehaviourCaching(
				isWeb: true,
				AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList,
				(dataAccess, companyCode) => dataAccess.GetAmendmentReasonCodesList_ForTestOnly(companyCode)
			);
		}

		public void TestGetAmendmentReasonCodesList_WhenNotWeb()
		{
			ZArchitecture.Environment.Globals.IsWeb = false;
			TestCodeDescriptionCodeBehaviourCaching(
				isWeb: false,
				AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList,
				(dataAccess, companyCode) => dataAccess.GetAmendmentReasonCodesList_ForTestOnly(companyCode)
			);
		}

		#endregion

		#region Test Optimised Stored Procs

		#region TransactionHeader

		public void TestOptimisedStoredProcs_TransactionHeader_WhenRegistryEnabled_AndTransactionPK()
			=> TestOptimisedStoredProcs(
					(dataAccess) => dataAccess.GetByPKTransactionHeaderPopulatingTransactionInfo(new AccountingTransactionDataObjectWriterStrategy(), GlbCompany.CurrentCompany.GC_Code, Guid.NewGuid()),
					expectedProcs: HeaderProcsForSingle()
				);

		public void TestOptimisedStoredProcs_TransactionHeader_WhenRegistryEnabled_AndBatch()
			=> TestOptimisedStoredProcs(
					(dataAccess) => dataAccess.GetTransactionHeaders(new AccountingTransactionDataObjectWriterStrategy(), GlbCompany.CurrentCompany.GC_Code, 1),
					expectedProcs: HeaderProcsForBatch()
				);

		public void TestGetRegistrationNumberWithDifferentOrganizationCodes()
		{
			var orgAddress1 = new List<OrganizationAddress>()
			{
				new OrganizationAddress()
				{
					OrganizationCode = "TEST"
				},
				new OrganizationAddress()
				{
					OrganizationCode = "TEST1"
				},
				new OrganizationAddress()
				{
					OrganizationCode = " TEST"
				},
				new OrganizationAddress()
				{
					OrganizationCode = " ŤEST "
				},
				new OrganizationAddress()
				{
					OrganizationCode = " tést "
				}
			};

			var batchExportDataAccess = new BatchExportDataAccess(Connection, Transaction);
			var lstUniqueOrgCodes = batchExportDataAccess.GetUniqueListOfOrgCodes_ForTestOnly(orgAddress1);

			AssertEquals("The unique Org Codes count should be 5", 5, lstUniqueOrgCodes.Length);
			AssertNoExceptionThrown(() => batchExportDataAccess.GetRegistrationNumbers_ForTestOnly(lstUniqueOrgCodes));
		}

		IEnumerable<string> HeaderProcsForBatch() => new[]
		{
			"AccountingTransactionExportGetHeaders",
			"AccountingTransactionExportGetAddresses",
			"AccountingTransactionExportGetSenderAddresses",
		};
		IEnumerable<string> HeaderProcsForSingle() => HeaderProcsForBatch().Select(x => x + "Single");

		#endregion

		#region TransactionLines

		public void TestOptimisedStoredProcs_TransactionLines_WhenRegistryEnabled_AndTransactionPK()
			=> TestOptimisedStoredProcs(
					(dataAccess) => dataAccess.GetTransactionLines(GlbCompany.CurrentCompany.GC_Code, -1, transactionHeaderPK: Guid.NewGuid()),
					expectedProcs: LineProcsForSingle()
				);

		public void TestOptimisedStoredProcs_TransactionLines_WhenRegistryEnabled_AndBatch()
			=> TestOptimisedStoredProcs(
					(dataAccess) => dataAccess.GetTransactionLines(GlbCompany.CurrentCompany.GC_Code, 1, transactionHeaderPK: null),
					expectedProcs: LineProcsForBatch()
				);

		IEnumerable<string> LineProcsForBatch() => new[]
		{
			"AccountingTransactionExportGetLines",
		};
		IEnumerable<string> LineProcsForSingle() => LineProcsForBatch().Select(x => x + "Single");

		public void TestOSExtraVATAmount_WhenTaxIDHasExtraTaxAndLineAmountIsAnSmallAmount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Mexico))
			{
				var taxRate = TestObjectCreator.CreateTaxRate("REDREB", "Gravado Fronteriza & Retencion", AccTaxRate.Types.Rated, 8, "RET", 3, 1, "MX");
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
				var cc10 = TestObjectCreator.CreateChargeCode("CC10", "Charge Code Test", Core.Constants.ChargeType.Revenue, 0, taxRate, null);
				var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, null, cc10, 0.12M, taxRate: taxRate);

				Factory.Save();

				var dataAccess = new BatchExportDataAccess(Connection, Transaction);
				var line = dataAccess.GetTransactionLines(GlbCompany.CurrentCompany.GC_Code, -1, transactionHeaderPK: invoice.PK.ToGuid());

				AssertEquals("OSExtraVATAmount", 0M, line[invoiceLine.PK.ToGuid()].OSExtraVATAmount);
			}
		}

		public void TestOSExtraVATAmount_WhenTaxIDHasExtraTaxAndLineAmountIsMoreThanOne()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Mexico))
			{
				var taxRate = TestObjectCreator.CreateTaxRate("IVAREF", "RateWithExtraRate", AccTaxRate.Types.Rated, 16, "REF", 2, 3, "MX");
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
				var cc10 = TestObjectCreator.CreateChargeCode("CC10", "Charge Code Test", Core.Constants.ChargeType.Revenue, 0, taxRate, null);
				var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, null, cc10, 1.38M, taxRate: taxRate);

				Factory.Save();

				var dataAccess = new BatchExportDataAccess(Connection, Transaction);
				var line = dataAccess.GetTransactionLines(GlbCompany.CurrentCompany.GC_Code, -1, transactionHeaderPK: invoice.PK.ToGuid());

				AssertEquals("OSExtraVATAmount", -0.15M, line[invoiceLine.PK.ToGuid()].OSExtraVATAmount);
			}
		}

		#endregion

		#region TransactionTaxGLMovements

		public void TestOptimisedStoredProcs_TransactionTaxGLMovements_WhenTransactionHeaderPKHasValue()
			=> TestOptimisedStoredProcs(
					(dataAccess) => dataAccess.GetTaxGLMovements(GlbCompany.CurrentCompany.GC_Code, -1, transactionHeaderPK: Guid.NewGuid()),
					expectedProcs: TaxGLMovementProcsForSingle()
				);

		public void TestOptimisedStoredProcs_TransactionTaxGLMovements_WhenBatchNumberHasValidValue()
			=> TestOptimisedStoredProcs(
					(dataAccess) => dataAccess.GetTaxGLMovements(GlbCompany.CurrentCompany.GC_Code, 1, transactionHeaderPK: null),
					expectedProcs: TaxGLMovementProcsForBatch()
				);

		IEnumerable<string> TaxGLMovementProcsForBatch() => new[]
		{
			"AccountingTransactionExportGetTaxGLMovements",
		};

		IEnumerable<string> TaxGLMovementProcsForSingle() => TaxGLMovementProcsForBatch().Select(x => x + "Single");

		#endregion

		#region TaxTransactions

		public void TestOptimisedStoredProcs_TaxTransactions_WhenTransactionHeaderPKHasValue()
			=> TestOptimisedStoredProcs(
					(dataAccess) => dataAccess.GetTaxTransactions(DefaultDataObjectWriterStrategy.TestInstance, GlbCompany.CurrentCompany.GC_Code, -1, transactionHeaderPK: Guid.NewGuid()),
					expectedProcs: TaxTransactionsProcsForSingle()
				);

		public void TestOptimisedStoredProcs_TaxTransactions_WhenBatchNumberHasValidValue()
			=> TestOptimisedStoredProcs(
					(dataAccess) => dataAccess.GetTaxTransactions(DefaultDataObjectWriterStrategy.TestInstance, GlbCompany.CurrentCompany.GC_Code, 1, transactionHeaderPK: null),
					expectedProcs: TaxTransactionsProcsForBatch()
				);

		IEnumerable<string> TaxTransactionsProcsForBatch() => new[]
		{
			"AccountingTransactionExportGetTaxTransactions",
		};

		IEnumerable<string> TaxTransactionsProcsForSingle() => TaxTransactionsProcsForBatch().Select(x => x + "Single");

		#endregion

		#region TaxTransactionLinks

		public void TestOptimisedStoredProcs_TaxTransactionLinks_WhenTransactionHeaderPKHasValue()
			=> TestOptimisedStoredProcs(
					(dataAccess) => dataAccess.GetTaxTransactionLinks(GlbCompany.CurrentCompany.GC_Code, -1, EmptyTaxTransactionRowDictionary(), transactionHeaderPK: Guid.NewGuid()),
					expectedProcs: TaxTransactionLinksProcsForSingle()
				);

		public void TestOptimisedStoredProcs_TaxTransactionLinks_WhenBatchNumberHasValidValue()
			=> TestOptimisedStoredProcs(
					(dataAccess) => dataAccess.GetTaxTransactionLinks(GlbCompany.CurrentCompany.GC_Code, 1, EmptyTaxTransactionRowDictionary(), transactionHeaderPK: null),
					expectedProcs: TaxTransactionLinksProcsForBatch()
				);

		IEnumerable<string> TaxTransactionLinksProcsForBatch() => new[]
		{
			"AccountingTransactionExportGetTaxTransactionLinks",
		};

		IEnumerable<string> TaxTransactionLinksProcsForSingle() => TaxTransactionLinksProcsForBatch().Select(x => x + "Single");

		Dictionary<ZGuid, TaxTransactionRow> EmptyTaxTransactionRowDictionary() => new Dictionary<ZGuid, TaxTransactionRow>();

		#endregion

		#region Helpers for TestOptimisedStoredProcs

		void TestOptimisedStoredProcs(Action<BatchExportDataAccess> accessData, IEnumerable<string> expectedProcs)
		{
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			accessData(dataAccess);

			foreach (var ep in expectedProcs)
			{
				var message = $"Expected stored proc '{ep}' in commands:\r\n {string.Join("\r\n---\r\n", dataAccess.CreatedCommandTexts_ForTestOnly)}";
				var storedProcCallText = "EXEC " + ep + " ";
				var isInCreatedCommandList = dataAccess.CreatedCommandTexts_ForTestOnly.Any(x => x.Contains(storedProcCallText));
				Assert(message, isInCreatedCommandList);
			}
		}

		#endregion

		public void TestCashAdvanceReceivedIsSetCorrectlyOnLine()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			var arInvoiceLine = TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100M);
			arInvoiceLine.AL_JH = TestObjectCreator.Job1.PK;

			var charge = TestObjectCreator.CreateJobCharge(arInvoiceLine, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, TestObjectCreator.Debtor, LedgerTypes.AccountsReceivable, 100M, 100M, charge.JR_RX_NKSellCurrency);

			var cal = TestObjectCreator.CreateCashAdvanceRequestLine(cah, charge.JR_LocalSellAmt, charge.JR_OSSellAmt);
			cal.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cal.CAL_LocalPaidAmount = 100M;
			cal.CAL_OSPaidAmount = 100M;

			Factory.Save();

			var batchExportDataAccess = new BatchExportDataAccess(Connection, Transaction);
			var lines = batchExportDataAccess.GetTransactionLines(GlbCompany.CurrentCompany.GC_Code, 1, arInvoice.PK.ToGuid());

			AssertEquals(1, lines.Count);
			AssertEquals(lines.FirstOrDefault().Value.CashAdvanceAmount, null);

			charge.JR_CAL_ARLine = cal.PK;
			Factory.Save();

			lines = batchExportDataAccess.GetTransactionLines(GlbCompany.CurrentCompany.GC_Code, 1, arInvoice.PK.ToGuid());

			AssertEquals(1, lines.Count);
			AssertEquals(lines.FirstOrDefault().Value.CashAdvanceAmount, 100M);
		}

		#endregion

		#region Exemption Document Export

		[TestDate(2022, 09, 02)]
		public void TestExportExemptionDocumentForSpecificCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var febReceivedDate = new ZDateTime(2022, 2, 1);
				var marReceivedDate = new ZDateTime(2022, 3, 1);
				var novExpiryDate = new ZDateTime(2022, 11, 30);
				var decExpiryDate = new ZDateTime(2022, 12, 31);

				createDocumentExemption(TestObjectCreator.Debtor, "12345", febReceivedDate, novExpiryDate, "12345678912345678-123456", GlbCompany.CurrentCompany, CountryCodes.Italy);
				createDocumentExemption(TestObjectCreator.Debtor, "12348", marReceivedDate, decExpiryDate, "12345678912345678-123488", GlbCompany.CurrentCompany, CountryCodes.Italy);
				createDocumentExemption(TestObjectCreator.Debtor, "12346", febReceivedDate, novExpiryDate, "12345678912345678-123466", TestObjectCreator.NonCurrentCompany, CountryCodes.Italy);
				createDocumentExemption(TestObjectCreator.Debtor, "12399", marReceivedDate, decExpiryDate, "12345678912345678-123499", TestObjectCreator.NonCurrentCompany, CountryCodes.Italy);

				var taxRate = TestObjectCreator.CreateTaxRate("IVA", "Rate", AccTaxRate.Types.Rated, 22, "REF", 2, 3, "IT");
				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m, TestObjectCreator.Debtor);
				var cc10 = TestObjectCreator.CreateChargeCode("CC10", "Charge Code Test", ChargeType.Revenue, 0, taxRate, null);
				var invoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, null, cc10, 1.38M, taxRate: taxRate);
				Factory.Save();

				var dataAccess = new BatchExportDataAccess(Connection, Transaction);
				var exporterExemptionDocumentTracking = dataAccess.GetExporterExemptionDocumentDetails(DefaultDataObjectWriterStrategy.TestInstance, GlbCompany.CurrentCompany.GC_Code, 1, arInvoice.PK.ToGuid());

				Dictionary<Guid, DocumentTracking> documentTrackingList;
				if (exporterExemptionDocumentTracking.TryGetValue(arInvoice.PK.ToGuid(), out documentTrackingList))
				{
					var documentTracking = documentTrackingList.Values.ToArray()[0];
					AssertEquals("Doc. Number is 12345", "12345", documentTracking.DocumentNumber);
					AssertEquals("Doc. Received Date is 01/02/2022", febReceivedDate, documentTracking.ReceivedDate);
					AssertEquals("Doc. Expiry Date is 30/11/2022", novExpiryDate, documentTracking.ValidToDate);
					var govAuthRefNumber = documentTracking.DocumentTrackingAttributeCollection.Find(x => x.Type.Equals(JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference)).Value;
					AssertEquals("Doc. Gov. Auth. Reference Number is 12345678912345678-123456", "12345678912345678-123456", govAuthRefNumber);

					var documentTracking2 = documentTrackingList.Values.ToArray()[1];
					AssertEquals("Doc. Number is 12348", "12348", documentTracking2.DocumentNumber);
					AssertEquals("Doc. Received Date is 01/03/2022", marReceivedDate, documentTracking2.ReceivedDate);
					AssertEquals("Doc. Expiry Date is 31/12/2022", decExpiryDate, documentTracking2.ValidToDate);
					var govAuthRefNumber2 = documentTracking2.DocumentTrackingAttributeCollection.Find(x => x.Type.Equals(JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference)).Value;
					AssertEquals("Doc. Gov. Auth. Reference Number is 12345678912345678-123488", "12345678912345678-123488", govAuthRefNumber2);
				}

				exporterExemptionDocumentTracking.Clear();
				exporterExemptionDocumentTracking = dataAccess.GetExporterExemptionDocumentDetails(DefaultDataObjectWriterStrategy.TestInstance, TestObjectCreator.NonCurrentCompany.GC_Code, 1, arInvoice.PK.ToGuid());

				if (exporterExemptionDocumentTracking.TryGetValue(arInvoice.PK.ToGuid(), out documentTrackingList))
				{
					var documentTracking = documentTrackingList.Values.ToArray()[0];
					AssertEquals("Doc. Number is 12399", "12399", documentTracking.DocumentNumber);
					AssertEquals("Doc. Received Date is 01/03/2022", marReceivedDate, documentTracking.ReceivedDate);
					AssertEquals("Doc. Expiry Date is 31/12/2022", decExpiryDate, documentTracking.ValidToDate);
					var govAuthRefNumber = documentTracking.DocumentTrackingAttributeCollection.Find(x => x.Type.Equals(JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference)).Value;
					AssertEquals("Doc. Gov. Auth. Reference Number is 12345678912345678-123499", "12345678912345678-123499", govAuthRefNumber);

					var documentTracking2 = documentTrackingList.Values.ToArray()[1];
					AssertEquals("Doc. Number is 12346", "12346", documentTracking2.DocumentNumber);
					AssertEquals("Doc. Received Date is 01/02/2022", febReceivedDate, documentTracking2.ReceivedDate);
					AssertEquals("Doc. Expiry Date is 30/11/2022", novExpiryDate, documentTracking2.ValidToDate);
					var govAuthRefNumber2 = documentTracking2.DocumentTrackingAttributeCollection.Find(x => x.Type.Equals(JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference)).Value;
					AssertEquals("Doc. Gov. Auth. Reference Number is 12345678912345678-123466", "12345678912345678-123466", govAuthRefNumber2);
				}
			}
		}

		[TestDate(2022, 09, 02)]
		public void TestExportExemptionDocumentForNotSpecificCompany()
		{
			var febReceivedDate = new ZDateTime(2022, 2, 1);
			var marReceivedDate = new ZDateTime(2022, 3, 1);
			var novExpiryDate = new ZDateTime(2022, 11, 30);
			var decExpiryDate = new ZDateTime(2022, 12, 31);

			createDocumentExemption(TestObjectCreator.Debtor, "12345", febReceivedDate, novExpiryDate, "12345678912345678-123456", null, CountryCodes.Australia);
			createDocumentExemption(TestObjectCreator.Debtor, "12399", marReceivedDate, decExpiryDate, "12345678912345678-123499", null, CountryCodes.Australia);

			var taxRate = TestObjectCreator.CreateTaxRate("IVA", "Rate", AccTaxRate.Types.Rated, 22, "REF", 2, 3, "IT");
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m, TestObjectCreator.Debtor);
			var cc10 = TestObjectCreator.CreateChargeCode("CC10", "Charge Code Test", ChargeType.Revenue, 0, taxRate, null);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, null, cc10, 1.38M, taxRate: taxRate);
			Factory.Save();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporterExemptionDocumentTracking = dataAccess.GetExporterExemptionDocumentDetails(DefaultDataObjectWriterStrategy.TestInstance, GlbCompany.CurrentCompany.GC_Code, 1, arInvoice.PK.ToGuid());

			Dictionary<Guid, DocumentTracking> documentTrackingList;
			if (exporterExemptionDocumentTracking.TryGetValue(arInvoice.PK.ToGuid(), out documentTrackingList))
			{
				var documentTracking = documentTrackingList.Values.ToArray()[0];
				Assert("Old existing functionality preserved for the following extracted data", true);
				AssertEquals("Doc. Number is 12345", "12345", documentTracking.DocumentNumber);
				AssertEquals("Doc. Received Date is 01/02/2022", febReceivedDate, documentTracking.ReceivedDate);
				AssertEquals("Doc. Expiry Date is 30/11/2022", novExpiryDate, documentTracking.ValidToDate);
				var govAuthRefNumber = documentTracking.DocumentTrackingAttributeCollection.Find(x => x.Type.Equals(JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference)).Value;
				AssertEquals("Doc. Gov. Auth. Reference Number is 12345678912345678-123456", "12345678912345678-123456", govAuthRefNumber);

				var documentTracking2 = documentTrackingList.Values.ToArray()[1];
				AssertEquals("Doc. Number is 12399", "12399", documentTracking2.DocumentNumber);
				AssertEquals("Doc. Received Date is 01/03/2022", marReceivedDate, documentTracking2.ReceivedDate);
				AssertEquals("Doc. Expiry Date is 31/12/2022", decExpiryDate, documentTracking2.ValidToDate);
				var govAuthRefNumber2 = documentTracking2.DocumentTrackingAttributeCollection.Find(x => x.Type.Equals(JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference)).Value;
				AssertEquals("Doc. Gov. Auth. Reference Number is 12345678912345678-123499", "12345678912345678-123499", govAuthRefNumber2);
			}
		}

		void createDocumentExemption(OrgHeader orgHeader, string documentNumber, ZDateTime receivedDate, ZDateTime validToDate, string govAuthReferenceNumber, GlbCompany company, ZString countryCode)
		{
			JobRequiredDocument jobDocument = orgHeader.RequiredDocuments.AddNew();
			jobDocument.EQ_DocNumber = documentNumber;
			jobDocument.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
			jobDocument.EQ_DocType = RefDocTypes.VATExporterExemption;
			jobDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			jobDocument.EQ_DateReceived = receivedDate.ToDateTimeOffset(null);
			jobDocument.EQ_ValidToDate = validToDate;
			jobDocument.EQ_RN_NKRelatedCountry = countryCode;

			JobRequiredDocAttrib attrib1 = jobDocument.Attributes.AddNew();
			attrib1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference;
			attrib1.D0_AttribValue = govAuthReferenceNumber;

			if (company != null)
			{
				JobRequiredDocAttrib attrib2 = jobDocument.Attributes.AddNew();
				attrib2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
				attrib2.D0_AttribValue = company.PK.ToString();
			}

			Factory.Save();
		}

		#endregion

		public void TestGetCorrectContact_WhenGetByPKTransactionHeader()
		{
			AssertGetBranchAddress(dataAccess =>
				dataAccess.GetByPKTransactionHeaderPopulatingTransactionInfo(new AccountingTransactionDataObjectWriterStrategy(), GlbCompany.CurrentCompany.GC_Code, arInvoiceForGetBranchAddress.PK.ToGuid(), new TransactionInfo(new DefaultDataObjectWriterStrategy())));
		}

		public void TestGetCorrectContact_WhenTransactionHeaderIsNull()
		{
			AssertGetBranchAddress(dataAccess =>
				dataAccess.GetTransactionHeaders(new AccountingTransactionDataObjectWriterStrategy(),
					GlbCompany.CurrentCompany.GC_Code, 1));
		}

		void AssertGetBranchAddress(Func<BatchExportDataAccess, Dictionary<Guid, TransactionHeaderRow>> accessData)
		{
			var orgHeader = TestObjectCreator.CreateOrgHeader("Header", true, true);
			var glbBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			glbBranch.GB_OH_OrgProxy = orgHeader.PK;

			arInvoiceForGetBranchAddress = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, orgHeader);
			var line = TestObjectCreator.CreateARInvoiceLine(arInvoiceForGetBranchAddress, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100M);

			TestObjectCreator.CreateGenExportBatchSequenceWebServicePostLine(1, line.PK, 1);

			var address = TestObjectCreator.CreateAddress(orgHeader, addressType: OrgAddressType.Office, isMain: true, streetAddress1: "Proxy Address Line 1",
				streetAddress2: "Proxy Address Line 2", city: "Org ProxyCity", stateCode: "HH", countryCode: Core.Constants.CountryCodes.Germany, postCode: "", phone: "000", email: "address@example.com");

			address.AdditionalInfos.AddNew().OAI_AdditionalInfo = "First, but not primary";
			address.AdditionalInfos.AddNew().OAI_AdditionalInfo = "Second and primary";
			address.AdditionalInfos[1].OAI_IsPrimary = true;

			var contact1 = testObjectCreator.CreateContact(orgHeader, "First Address", "contact1@example.com");
			contact1.OC_Phone = "111";

			var contact2 = testObjectCreator.CreateContact(orgHeader, "Second Address", "contact2@example.com");
			contact2.OC_Phone = "222";

			Factory.Save();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var transactionHeaderRows = accessData(dataAccess);

			var branchAddress = transactionHeaderRows[arInvoiceForGetBranchAddress.PK.ToGuid()].Info.BranchAddress;
			AssertEquals(contact1.OC_Phone, branchAddress.Phone);
			AssertEquals(contact1.Email, branchAddress.Email);
			AssertEquals(address.AdditionalInfos[1].OAI_AdditionalInfo, branchAddress.AdditionalAddressInformation);

			arInvoiceForGetBranchAddress.AH_OC_InvoiceContactOverride = contact2.PK;

			Factory.Save();

			transactionHeaderRows = accessData(dataAccess);

			branchAddress = transactionHeaderRows[arInvoiceForGetBranchAddress.PK.ToGuid()].Info.BranchAddress;
			AssertEquals("Phone should be Contact1", contact1.OC_Phone, branchAddress.Phone);
			AssertEquals("Email should be Contact1", contact1.OC_Email, branchAddress.Email);
		}

		public void TestSellersBankAccountInARWhenOnlyDebtorARPayToAccountIsSet()
		{
			AssertSellersBankAccount(SellersBankAccountConfiguration.OnlyDebtorARPayToAccountIsSet, currency: "EUR");
		}
		public void TestSellersBankAccountInARWhenOnlyDebtorGroupBankAccountIsSet()
		{
			AssertSellersBankAccount(SellersBankAccountConfiguration.OnlyDebtorGroupBankAccountIsSet, currency: "EUR");
		}
		public void TestSellersBankAccountInARWhenOnlyBankAccountsBasedOnCurrencyRegistryIsSet()
		{
			AssertSellersBankAccount(SellersBankAccountConfiguration.OnlyBankAccountsBasedOnCurrencyRegistryIsSet, currency: "EUR");
		}
		public void TestSellersBankAccountInARWhenOnlyDefaultReceiptBankAccountIsSet()
		{
			AssertSellersBankAccount(SellersBankAccountConfiguration.OnlyDefaultReceiptBankAccountIsSet, currency: "EUR");
		}
		public void TestSellersBankAccountInARWhenNoBankAccountIsSet()
		{
			AssertSellersBankAccount(SellersBankAccountConfiguration.NoBankAccountIsSet, currency: "EUR");
		}

		public void TestSellersBankAccountInARWhenOnlyDebtorARPayToAccountIsSet_USD()
		{
			AssertSellersBankAccount(SellersBankAccountConfiguration.OnlyDebtorARPayToAccountIsSet, currency: "USD");
		}

		void AssertSellersBankAccount(SellersBankAccountConfiguration bankAccountConfiguration, string currency)
		{
			var orgHeader = TestObjectCreator.CreateOrgHeader("Header", true, true);
			var glbBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			glbBranch.GB_OH_OrgProxy = orgHeader.PK;
			var companyData = orgHeader.GetCompanyDataForGlbCompany(glbBranch.Company);
			AccBankAccount expectedAccBankAccount = null;

			switch (bankAccountConfiguration)
			{
				case SellersBankAccountConfiguration.OnlyDebtorARPayToAccountIsSet:
					AccBankAccount bank1 = GenerateBankAccountFromData(currency, "Test Name 1", "Test Bank", "TRXBIZXXX",
						"BSB1", "6826399", "DE 00001 0001000213 123", "TEST1", "Test Bank Account 1", "Hamburg, Germany",
						"22334455", "DE");
					companyData.OB_AB_ARPayToAccount = bank1.PK;
					expectedAccBankAccount = bank1;
					break;

				case SellersBankAccountConfiguration.OnlyDebtorGroupBankAccountIsSet:
					AccBankAccount bank2 = GenerateBankAccountFromData(currency, "Test Name 2", "Test Bank", "TRXBIZXXX",
						"BSB2", "6826399", "DE 00001 0001000213 123", "TEST2", "Test Bank Account 2", "Hamburg, Germany",
						"22334455", "DE");
					var orgDebtorGroup = TestObjectCreator.CreateDebtorGroup();
					orgDebtorGroup.OJ_Code = "OG1";
					companyData.OB_OJ_ARDebtorGroup = orgDebtorGroup.PK;

					var orgDebtorGroupBankDefault = Factory.New<OrgDebtorGroupBankDefault>();
					orgDebtorGroupBankDefault.P6_OJ = orgDebtorGroup.PK;
					orgDebtorGroupBankDefault.P6_AB = bank2.PK;
					expectedAccBankAccount = bank2;
					break;

				case SellersBankAccountConfiguration.OnlyBankAccountsBasedOnCurrencyRegistryIsSet:
					AccBankAccount bank3 = GenerateBankAccountFromData(currency, "Test Name 3", "Test Bank", "TRXBIZXXX",
						"BSB3", "6826399", "DE 00001 0001000213 123", "TEST3", "Test Bank Account 3", "Hamburg, Germany",
						"22334455", "DE");
					BankAccountBasedOnCurrencyCollection collection = new BankAccountBasedOnCurrencyCollection();
					collection.AddNew();
					collection[0].Currency = bank3.AB_RX_NKAccountCurrency;
					collection[0].BankAccount = bank3.PK;
					Factory.Save();
					OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
					expectedAccBankAccount = bank3;
					break;

				case SellersBankAccountConfiguration.OnlyDefaultReceiptBankAccountIsSet:
					AccBankAccount bank4 = GenerateBankAccountFromData(currency, "Test Name 4", "Test Bank", "TRXBIZXXX",
						"BSB4", "6826399", "DE 00001 0001000213 123", "TEST4", "Test Bank Account 4", "Hamburg, Germany",
						"22334455", "DE");
					bank4.AB_Code = "Bank3";
					bank4.AB_IsDefaultReceiptBankAccount = true;
					expectedAccBankAccount = bank4;
					break;

				default:
					break;
			}

			var arInvoiceForGetSellersBankAccount = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.EUR, 1.0m, orgHeader);
			var line = TestObjectCreator.CreateARInvoiceLine(arInvoiceForGetSellersBankAccount, null, TestObjectCreator.CC1, TestObjectCreator.EUR, 1.0m, "Desc", 100M);
			TestObjectCreator.CreateGenExportBatchSequenceHeader(1, line.PK, 1);

			Factory.Save();

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var transactionHeaderRows = dataAccess.GetByPKTransactionHeaderPopulatingTransactionInfo(
				new AccountingTransactionDataObjectWriterStrategy(), GlbCompany.CurrentCompany.GC_Code,
				arInvoiceForGetSellersBankAccount.PK.ToGuid(), new TransactionInfo(new DefaultDataObjectWriterStrategy()));

			var sellersBankAccount = transactionHeaderRows[arInvoiceForGetSellersBankAccount.PK.ToGuid()].Info.SellersBankAccountCollection;
			if (expectedAccBankAccount == null)
			{
				AssertNull("Sellers Bank Account list should be null", sellersBankAccount);
			}
			else
			{
				AssertNotNull("Sellers Bank Account list should be not null", sellersBankAccount);
				AssertEquals("Sellers Bank Account list should have 1 element", 1, sellersBankAccount.Count);
				AssertEquals("Sellers Bank Account list should have the expected bank", expectedAccBankAccount.AB_Code, sellersBankAccount[0].BankAccountCode);
				AssertSellersBankAccountAttributes(expectedAccBankAccount, sellersBankAccount[0]);
			}
		}

		AccBankAccount GenerateBankAccountFromData(string currency, string bankAccName, string bankName, string swift, string bSB, string bankAccNumber, string iban, string bankAccCode, string bankAccDescription, string bankAddress, string uniqueBankAccNo, string countryCode)
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_RX_NKAccountCurrency = currency;
			bankAccount.AB_BankAccountName = bankAccName;
			bankAccount.AB_BankName = bankName;
			bankAccount.AB_SWIFT = swift;
			bankAccount.AB_BSB = bSB;
			bankAccount.AB_AccountNum = bankAccNumber;
			bankAccount.IBAN = iban;
			bankAccount.AB_Code = bankAccCode;
			bankAccount.AB_Desc = bankAccDescription;
			bankAccount.AB_BankAddress = bankAddress;
			bankAccount.AB_FullAccountNumber = uniqueBankAccNo;
			bankAccount.AB_RN_NKBankAccountCountry = countryCode;
			return bankAccount;
		}

		void AssertSellersBankAccountAttributes(AccBankAccount bankAccount, BankAccount sellersBankAccount)
		{
			AssertEquals("Currency should be mapped correctly.", bankAccount.AB_RX_NKAccountCurrency, sellersBankAccount.Currency);
			AssertEquals("AccountName should be mapped correctly.", bankAccount.AB_BankAccountName, sellersBankAccount.AccountName);
			AssertEquals("BankName should be mapped correctly.", bankAccount.AB_BankName, sellersBankAccount.BankName);
			AssertEquals("BankSwift should be mapped correctly.", bankAccount.AB_SWIFT, sellersBankAccount.BankSwift);
			AssertEquals("BankBranch should be mapped correctly with BSB.", bankAccount.AB_BSB, sellersBankAccount.BankBranch);
			AssertEquals("AccountNumber should be mapped correctly.", bankAccount.AB_AccountNum, sellersBankAccount.AccountNumber);
			AssertEquals("IBANNumber should be mapped correctly.", bankAccount.IBAN, sellersBankAccount.IBANNumber);
			AssertEquals("BankAccountCode should be mapped correctly.", bankAccount.AB_Code, sellersBankAccount.BankAccountCode);
			AssertEquals("BankAccountDescription should be mapped correctly.", bankAccount.AB_Desc, sellersBankAccount.BankAccountDescription);
			AssertEquals("BankAddress should be mapped correctly.", bankAccount.AB_BankAddress, sellersBankAccount.BankAddress);
			AssertEquals("BankUniqueAccNo should be mapped correctly with full account number.", bankAccount.AB_FullAccountNumber, sellersBankAccount.BankUniqueAccNo);
			AssertEquals("Country should be mapped correctly.", bankAccount.AB_RN_NKBankAccountCountry, sellersBankAccount.Country.Code);
		}

		ARInvoice arInvoiceForGetBranchAddress;

		#region Helpers

		void TestCodeDescriptionCodeBehaviourCaching(
			bool isWeb,
			ZArchitecture.Environment.CodeDescriptionPairListRegistryItem registryToTest,
			Func<BatchExportDataAccess, string, ICodeDescriptionPairList> dataAccessGetter,
			CodeDescriptionPairList testDatasetForIN = null,
			CodeDescriptionPairList testDatasetForAU = null
		)
		{
			var expectedDefaultRegistryValue = registryToTest.DefaultValue;

			var batchExportDataAccess1 = new BatchExportDataAccess(Connection, Transaction);
			var companyForIN = TestObjectCreator.CreateCompanyAndBranch("IN");
			AssertContainsExactElementsInAnyOrder("IN registry default value", expectedDefaultRegistryValue, dataAccessGetter(batchExportDataAccess1, companyForIN.GC_Code));
			var companyForAU = TestObjectCreator.CreateCompanyAndBranch("AU");
			AssertContainsExactElementsInAnyOrder("AU registry default value", expectedDefaultRegistryValue, dataAccessGetter(batchExportDataAccess1, companyForAU.GC_Code));

			var batchExportDataAccess2 = new BatchExportDataAccess(Connection, Transaction);

			if (testDatasetForIN == null)
			{
				testDatasetForIN = new CodeDescriptionPairList();
				testDatasetForIN.AddPair("ABC", "Description for ABC");
				testDatasetForIN.AddPair("DEF", "The DEF Description");
			}

			if (testDatasetForAU == null)
			{
				testDatasetForAU = new CodeDescriptionPairList();
				testDatasetForAU.AddPair("GHI", "GHI as a Description");
			}

			using (registryToTest.SetTemporaryValue(companyForIN.PK.ToGuid(), Guid.Empty, Guid.Empty, testDatasetForIN))
			using (registryToTest.SetTemporaryValue(companyForAU.PK.ToGuid(), Guid.Empty, Guid.Empty, testDatasetForAU))
			{
				AssertContainsExactElementsInAnyOrder("IN registry value", testDatasetForIN, dataAccessGetter(batchExportDataAccess2, companyForIN.GC_Code));
				AssertContainsExactElementsInAnyOrder("AU registry value", testDatasetForAU, dataAccessGetter(batchExportDataAccess2, companyForAU.GC_Code));
			}

			TestConnection.ExecuteNonQuery($"DELETE FROM dbo.StmData WHERE SD_Name = '{registryToTest.Name}' AND SD_Owner = '{companyForAU.PK}'");

			AssertContainsExactElementsInAnyOrder("registry value in the cache", testDatasetForAU, dataAccessGetter(batchExportDataAccess2, companyForAU.GC_Code));

			var batchExportDataAccess3 = new BatchExportDataAccess(Connection, Transaction);
			AssertContainsExactElementsInAnyOrder("The latest registry value in the new BatchExportDataAccess Object", expectedDefaultRegistryValue, dataAccessGetter(batchExportDataAccess3, companyForAU.GC_Code));

			var batchExportDataAccess4 = new BatchExportDataAccess(Connection, Transaction);
			using (registryToTest.SetTemporaryValue(companyForAU.PK.ToGuid(), Guid.Empty, Guid.Empty, testDatasetForAU))
			{
				AssertContainsExactElementsInAnyOrder("registry value", testDatasetForAU, registryToTest.GetFallBackValueAtAllLevels(companyForAU.PK.ToGuid(), Guid.Empty, Guid.Empty));

				TestConnection.ExecuteNonQuery($"DELETE FROM dbo.StmData WHERE SD_Name = '{registryToTest.Name}' AND SD_Owner = '{companyForAU.PK}'");

				if (isWeb)
				{
					AssertContainsExactElementsInAnyOrder("registry value should be up to date when in Web environment", expectedDefaultRegistryValue, dataAccessGetter(batchExportDataAccess4, companyForAU.GC_Code));
				}
				else
				{
					AssertContainsExactElementsInAnyOrder("registry value should be cached when value from non-Web", testDatasetForAU, dataAccessGetter(batchExportDataAccess4, companyForAU.GC_Code));
				}
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			Connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			Transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
		}

		enum SellersBankAccountConfiguration
		{
			OnlyDebtorARPayToAccountIsSet = 1,
			OnlyDebtorGroupBankAccountIsSet = 2,
			OnlyBankAccountsBasedOnCurrencyRegistryIsSet = 3,
			OnlyDefaultReceiptBankAccountIsSet = 4,
			NoBankAccountIsSet = 5
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		System.Data.Common.DbConnection Connection;
		System.Data.Common.DbTransaction Transaction;
	}
}
