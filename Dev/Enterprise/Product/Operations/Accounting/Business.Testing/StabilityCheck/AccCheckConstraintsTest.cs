using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.StabilityCheck
{
	public class AccCheckConstraintsTest : TestCaseWithFactory
	{
		List<DbCheckConstraint> allConstraints;

		protected override void SetUp()
		{
			base.SetUp();

			DbCheckConstraintReader reader = new DbCheckConstraintReader();
			allConstraints = reader.ReadAllCheckConstraints();
		}

		void AssertConstraintExists(string schemaName, string tableName, string columnName, IEnumerable<string> allowedValues, bool allowedShouldMatchConstraintExactly = true)
		{
			if (tableName == "ZZRefExchangeRate" && columnName == RefExchangeRateSchema.Constants.RE_ExRateType)
			{
				Assert("Test is not applicable for this constraint due to the expression is more complicated than the test can handle.", true);
				return;
			}

			DbCheckConstraint checkConstraint = allConstraints.FirstOrDefault(c =>
				c.SchemaName == schemaName &&
				c.TableName == tableName &&
				c.IsInSetConstraint &&
				c.ColumnName == columnName
			);

			if (checkConstraint == null)
			{
				string tableConstraints = string.Join("\r\n", allConstraints.Where(c => c.SchemaName == schemaName && c.TableName == tableName).Select(c => " - " + c.ConstraintName + " CHECK (" + c.Expression + ")"));
				if (string.IsNullOrEmpty(tableConstraints))
				{
					tableConstraints = " - <none>";
				}
				string message = $"There is no 'in set' constraint on {tableName}.{columnName}. Existing check constraints on this table:\r\n{tableConstraints}";
				Fail(message);
			}

			if (allowedShouldMatchConstraintExactly)
			{
				AssertContainsExactElementsInAnyOrder($"Values in database constraint for '{tableName}.{columnName}' do not match values available in code.", allowedValues, checkConstraint.ColumnValues);
			}
			else
			{
				allowedValues.ForEach(val => AssertCollectionContains($"Values in database constraint for '{tableName}.{columnName}' do not contain '{val}'.", val, checkConstraint.ColumnValues));
			}
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccChargeBranchOverride_Fields()
		{
			var bizO = Factory.New<AccChargeBranchOverride>();

			// can be empty
			AssertConstraintExists(AccChargeBranchOverrideSchema.Constants.SqlSchemaName, AccChargeBranchOverrideSchema.Constants.TableName, AccChargeBranchOverrideSchema.Constants.YA_Direction, bizO.Lookups.DirectionList.GetAllCodes().Concat(new string[] { "" }));

			var transportModes = new HashSet<string>();
			foreach (var jobType in bizO.Lookups.JobTypeList.GetAllCodes())
			{
				bizO.YA_JobType = jobType;
				bizO.Lookups.TransportModeList.GetAllCodes().ForEach(x => transportModes.Add(x));
			}

			// can be empty
			AssertConstraintExists(AccChargeBranchOverrideSchema.Constants.SqlSchemaName, AccChargeBranchOverrideSchema.Constants.TableName, AccChargeBranchOverrideSchema.Constants.YA_TransportMode,
			transportModes.Concat(new[] { "" }));
			AssertConstraintExists(AccChargeBranchOverrideSchema.Constants.SqlSchemaName, AccChargeBranchOverrideSchema.Constants.TableName, AccChargeBranchOverrideSchema.Constants.YA_JobType, bizO.Lookups.JobTypeList.GetAllCodes());
		}

		public void TestInrternalCheckersAreSynchronisedWithLookupLists_AccChargeGLPostingOverride_Fields()
		{
			var bizO = Factory.NewWithValidTestData<AccChargeGLPostingOverride>();
			// Those values are not present in production, but are used in ediProd.
			// INC - Incident
			// PSQ - Professional Services Quote
			// TRS - Training Schedule
			// Also the special 'NJR' - Non Job Related
			var jobTypesList = bizO.Lookups.JobTypeList.GetAllCodes()
								.Concat(new[] { "INC", "PSQ", "TRS" })
								.Concat(new[] { "NJR" });

			AssertConstraintExists(AccChargeBranchOverrideSchema.Constants.SqlSchemaName, AccChargeGLPostingOverrideSchema.Constants.TableName, AccChargeGLPostingOverrideSchema.Constants.Y1_Direction, bizO.Lookups.DirectionList.GetAllCodes());

			var transportModes = new HashSet<string>();
			foreach (var jobType in jobTypesList)
			{
				bizO.Y1_JobType = jobType;
				bizO.Lookups.TransportModeList.GetAllCodes().ForEach(x => transportModes.Add(x));
			}

			AssertConstraintExists(AccChargeGLPostingOverrideSchema.Constants.SqlSchemaName, AccChargeGLPostingOverrideSchema.Constants.TableName, AccChargeGLPostingOverrideSchema.Constants.Y1_TransportMode,
				transportModes);
			AssertConstraintExists(AccChargeBranchOverrideSchema.Constants.SqlSchemaName, AccChargeGLPostingOverrideSchema.Constants.TableName, AccChargeGLPostingOverrideSchema.Constants.Y1_JobType, jobTypesList);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccChargeRevRecOverride_Fields()
		{
			var revRecOverride = Factory.New<AccChargeRevRecOverride>();

			// can be empty
			AssertConstraintExists(AccChargeRevRecOverrideSchema.Constants.SqlSchemaName, AccChargeRevRecOverrideSchema.Constants.TableName, AccChargeRevRecOverrideSchema.Constants.AE_BrokerType, revRecOverride.BrokerList.GetAllCodes().Concat(new string[] { "" }));
			// can be empty
			AssertConstraintExists(AccChargeRevRecOverrideSchema.Constants.SqlSchemaName, AccChargeRevRecOverrideSchema.Constants.TableName, AccChargeRevRecOverrideSchema.Constants.AE_Direction, revRecOverride.DirectionList.GetAllCodes().Concat(new string[] { "" }));
			// can be empty
			AssertConstraintExists(AccChargeRevRecOverrideSchema.Constants.SqlSchemaName, AccChargeRevRecOverrideSchema.Constants.TableName, AccChargeRevRecOverrideSchema.Constants.AE_Mode, revRecOverride.ModeList.GetAllCodes().Concat(new string[] { "" }));
			AssertConstraintExists(AccChargeRevRecOverrideSchema.Constants.SqlSchemaName, AccChargeRevRecOverrideSchema.Constants.TableName, AccChargeRevRecOverrideSchema.Constants.AE_RecognitionType, revRecOverride.RecognitionDateOptionList.GetAllCodes());

			// Those values are not present in production, but are used in ediProd.
			// INC - Incident
			// PSQ - Professional Services Quote
			// TRS - Training Schedule
			List<string> jobTypes = revRecOverride.JobTypeList.GetAllCodes().Concat(new string[] { "INC", "PSQ", "TRS" }).ToList();
			AssertConstraintExists(AccChargeRevRecOverrideSchema.Constants.SqlSchemaName, AccChargeRevRecOverrideSchema.Constants.TableName, AccChargeRevRecOverrideSchema.Constants.AE_JobType, jobTypes);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccChargeTaxOverride_Fields()
		{
			var taxOverride = Factory.New<AccChargeTaxOverride>();

			AssertConstraintExists(AccChargeTaxOverrideSchema.Constants.SqlSchemaName, AccChargeTaxOverrideSchema.Constants.TableName, AccChargeTaxOverrideSchema.Constants.AO_CostSellAll, taxOverride.Lookups.CostSellList.GetAllCodes());
			AssertConstraintExists(AccChargeTaxOverrideSchema.Constants.SqlSchemaName, AccChargeTaxOverrideSchema.Constants.TableName, AccChargeTaxOverrideSchema.Constants.AO_Direction, taxOverride.Lookups.DirectionList.GetAllCodes());
			AssertConstraintExists(AccChargeTaxOverrideSchema.Constants.SqlSchemaName, AccChargeTaxOverrideSchema.Constants.TableName, AccChargeTaxOverrideSchema.Constants.AO_IncoTerm, taxOverride.Lookups.Incoterms.GetAllCodes(), false);

			// Those values are not present in production, but are used in ediProd.
			// INC - Incident
			// PSQ - Professional Services Quote
			// TRS - Training Schedule
			List<string> jobTypes = taxOverride.Lookups.JobTypes.GetAllCodes().Concat(new string[] { "INC", "PSQ", "TRS" }).ToList();
			AssertConstraintExists(AccChargeTaxOverrideSchema.Constants.SqlSchemaName, AccChargeTaxOverrideSchema.Constants.TableName, AccChargeTaxOverrideSchema.Constants.AO_JobType, jobTypes);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccChargeTypeOverride_Fields()
		{
			var typeOverride = Factory.New<AccChargeTypeOverride>();
			var jobTypes = typeOverride.Lookups.JobTypes.GetAllCodes();
			var invoiceTypes = new HashSet<string>();

			foreach (var jobType in jobTypes)
			{
				typeOverride.AN_JobType = jobType;
				typeOverride.Lookups.InvoiceTypes.GetAllCodes().ForEach(x => invoiceTypes.Add(x));
			}

			AssertConstraintExists(AccChargeTypeOverrideSchema.Constants.SqlSchemaName, AccChargeTypeOverrideSchema.Constants.TableName, AccChargeTypeOverrideSchema.Constants.AN_InvoiceType, invoiceTypes);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccGLAccountDescriptor_Fields()
		{
			var accountDescriptor = Factory.New<AccGLAccountDescriptor>();
			AssertConstraintExists(AccGLAccountDescriptorSchema.Constants.SqlSchemaName, AccGLAccountDescriptorSchema.Constants.TableName, AccGLAccountDescriptorSchema.Constants.AJ_DebitCredit, accountDescriptor.AJ_DebitCredit_List.GetAllCodes());
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccGLBudget_Fields()
		{
			var budget = Factory.New<GLBudget>();
			AssertConstraintExists(AccGLBudgetSchema.Constants.SqlSchemaName, AccGLBudgetSchema.Constants.TableName, AccGLBudgetSchema.Constants.AU_AllocationType, budget.AllocationTypes.GetAllCodes());
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccGLHeader_Fields()
		{
			var glHeader = Factory.New<AccGLHeader>();
			List<string> allowedValues = new List<string>(glHeader.AG_AccountTypeList.GetAllCodes());

			// Value 'XXX' means invalid, and should not be created by code. However, such values can be present in database. See WI00134603.
			allowedValues.Add("XXX");

			AssertConstraintExists(AccGLHeaderSchema.Constants.SqlSchemaName, AccGLHeaderSchema.Constants.TableName, AccGLHeaderSchema.Constants.AG_AccountType, allowedValues);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccTaxRate_Fields()
		{
			var taxRate = Factory.New<AccTaxRate>();
			AssertConstraintExists(AccTaxRateSchema.Constants.SqlSchemaName, AccTaxRateSchema.Constants.TableName, AccTaxRateSchema.Constants.AT_Type, taxRate.Lookups.Types.GetAllCodes());
			// can be empty
			AssertConstraintExists(AccTaxRateSchema.Constants.SqlSchemaName, AccTaxRateSchema.Constants.TableName, AccTaxRateSchema.Constants.AT_ExtraTaxRateType, taxRate.Lookups.ExtraTypes.GetAllCodes().Concat(new string[] { "" }));
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccTransactionHeader_Fields()
		{
			var ledgerTypes = typeof(LedgerTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
			AssertConstraintExists(AccTransactionHeaderSchema.Constants.SqlSchemaName, AccTransactionHeaderSchema.Constants.TableName, AccTransactionHeaderSchema.Constants.AH_Ledger, ledgerTypes);

			var transactionTypes = typeof(TransactionTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
			AssertConstraintExists(AccTransactionHeaderSchema.Constants.SqlSchemaName, AccTransactionHeaderSchema.Constants.TableName, AccTransactionHeaderSchema.Constants.AH_TransactionType, transactionTypes);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccTransactionLines_Fields()
		{
			var lineTypes = typeof(TransactionLineTypes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToList();
			lineTypes.AddRange(new string[] { TransactionTypes.DirectPayment, TransactionTypes.DirectReceipt, TransactionTypes.GLStandardJournal, TransactionTypes.GLAutoJournal, TransactionTypes.GLReversingJournal, TransactionTypes.GLNoteJournal });
			AssertConstraintExists(AccTransactionLinesSchema.Constants.SqlSchemaName, AccTransactionLinesSchema.Constants.TableName, AccTransactionLinesSchema.Constants.AL_LineType, lineTypes);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_GenExportBatchSequence_Fields()
		{
			var types = typeof(Constants.DataExportBatchSubTypes.Codes).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
			AssertConstraintExists(GenExportBatchSequenceSchema.Constants.SqlSchemaName, GenExportBatchSequenceSchema.Constants.TableName, GenExportBatchSequenceSchema.Constants.XB_Type, types);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_JobChargeAttrib_Fields()
		{
			AssertConstraintExists(JobChargeAttribSchema.Constants.SqlSchemaName, JobChargeAttribSchema.Constants.TableName, JobChargeAttribSchema.Constants.EC_Name, new JobChargeAttribTypeList().GetAllCodes());
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_JobChargeRevRecognition_Fields()
		{
			var expectedCodes = RevenueRecognitionLookups.CompleteRecognitionDateOptionList.GetAllCodes().Union(new[] { "JOB" });
			AssertConstraintExists(JobChargeRevRecognitionSchema.Constants.SqlSchemaName, JobChargeRevRecognitionSchema.Constants.TableName, JobChargeRevRecognitionSchema.Constants.D3_RecognitionType, expectedCodes);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_JobHeader_Fields()
		{
			AssertConstraintExists(JobHeaderSchema.Constants.SqlSchemaName, JobHeaderSchema.Constants.TableName, JobHeaderSchema.Constants.JH_Status, new JobHeaderStatusList().GetAllCodes());
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_OrgARTerms_Fields()
		{
			var bizO = Factory.New<OrgARTerms>();

			var jobTypes = bizO.Lookups.JobTypeList.GetAllCodes();
			var invoiceTypes = new HashSet<string>();
			var transportModes = new HashSet<string>();

			foreach (var jobType in jobTypes)
			{
				bizO.PY_JobType = jobType;
				bizO.Lookups.InvoiceTypeList.GetAllCodes().ForEach(x => invoiceTypes.Add(x));
				bizO.Lookups.TransportModeList.GetAllCodes().ForEach(x => transportModes.Add(x));
			}

			AssertConstraintExists(OrgARTermsSchema.Constants.SqlSchemaName, OrgARTermsSchema.Constants.TableName, OrgARTermsSchema.Constants.PY_TransportMode,
				transportModes);

			// can be empty
			AssertConstraintExists(OrgARTermsSchema.Constants.SqlSchemaName, OrgARTermsSchema.Constants.TableName, OrgARTermsSchema.Constants.PY_InvoiceClass, invoiceTypes.Concat(new[] { "" }));

			AssertConstraintExists(OrgARTermsSchema.Constants.SqlSchemaName, OrgARTermsSchema.Constants.TableName, OrgARTermsSchema.Constants.PY_JobType, jobTypes);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_OrgCollectionNote_Fields()
		{
			var collectionNote = Factory.New<OrgCollectionNote>();
			// can be empty
			AssertConstraintExists(OrgCollectionNoteSchema.Constants.SqlSchemaName, OrgCollectionNoteSchema.Constants.TableName, OrgCollectionNoteSchema.Constants.PN_CallDisposition, collectionNote.Lookups.CallDispositionList.GetAllCodes().Concat(new string[] { "" }));
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_OrgInvTypeDeferredCharges_Fields()
		{
			var orgInvTypeDeferredCharges = Factory.New<OrgInvTypeDeferredCharges>();
			// can be empty
			AssertConstraintExists(
				OrgInvTypeDeferredChargesSchema.Constants.SqlSchemaName, OrgInvTypeDeferredChargesSchema.Constants.TableName, OrgInvTypeDeferredChargesSchema.Constants.PO_ChargeGroup,
				orgInvTypeDeferredCharges.Lookups.ChargeGroupList.GetAllCodes().Concat(new string[] { "" })
			);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_RefExchangeRate_Fields()
		{
			var exchangeRateTypes = typeof(Constants.ExchangeRateTypes.Code).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
			// RefExchangeRate is a view. Underlying table is ZZRefExchangeRate.
			AssertConstraintExists(RefExchangeRateSchema.Constants.SqlSchemaName, "ZZRefExchangeRate", RefExchangeRateSchema.Constants.RE_ExRateType, exchangeRateTypes);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_NettingPayableTransaction_Fields()
		{
			var approvalStatusList = typeof(NettingTransactionApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
			AssertConstraintExists(
				NettingPayableTransactionSchema.Constants.SqlSchemaName,
				NettingPayableTransactionSchema.Constants.TableName,
				NettingPayableTransactionSchema.Constants.NPT_ApprovalStatus,
				approvalStatusList);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_NettingReceivableTransaction_Fields()
		{
			var approvalStatusList = typeof(NettingTransactionApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).ToArray();
			AssertConstraintExists(
				NettingReceivableTransactionSchema.Constants.SqlSchemaName,
				NettingReceivableTransactionSchema.Constants.TableName,
				NettingReceivableTransactionSchema.Constants.NRT_ApprovalStatus,
				approvalStatusList);
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccChargeCreditorOverride_Fields()
		{
			var bizO = Factory.New<AccChargeCreditorOverride>();
			// Those values are not present in production, but are used in ediProd.
			// INC - Incident
			// PSQ - Professional Services Quote
			// TRS - Training Schedule
			// Also the special 'NJR' - Non Job Related
			var jobTypesList = bizO.Lookups.JobTypeList.GetAllCodes()
								.Concat(new[] { "INC", "PSQ", "TRS" })
								.Concat(new[] { "NJR" });
			AssertConstraintExists(
				AccChargeCreditorOverrideSchema.Constants.SqlSchemaName,
				AccChargeCreditorOverrideSchema.Constants.TableName,
				AccChargeCreditorOverrideSchema.Constants.ACC_JobType,
				jobTypesList);

			var transportModes = new HashSet<string>();
			foreach (var jobType in bizO.Lookups.JobTypeList.GetAllCodes())
			{
				bizO.ACC_JobType = jobType;
				bizO.Lookups.TransportModeList.GetAllCodes().ForEach(x => transportModes.Add(x));
			}

			AssertConstraintExists(AccChargeCreditorOverrideSchema.Constants.SqlSchemaName, AccChargeCreditorOverrideSchema.Constants.TableName, AccChargeCreditorOverrideSchema.Constants.ACC_TransportMode,
				transportModes.Concat(new[] { "" }));
		}

		public void TestInternalCheckersAreSynchronisedWithLookupLists_AccChargeSupplyTypeOverride_Fields()
		{
			var supplyTypeOverride = Factory.New<AccChargeSupplyTypeOverride>();

			AssertResult(AccChargeSupplyTypeOverrideSchema.Constants.ACS_SupplyType, supplyTypeOverride.SupplyTypeList.GetAllCodes());
			AssertResult(AccChargeSupplyTypeOverrideSchema.Constants.ACS_Direction, supplyTypeOverride.DirectionList.GetAllCodes());
			AssertResult(AccChargeSupplyTypeOverrideSchema.Constants.ACS_TransportMode, supplyTypeOverride.ModeList.GetAllCodes());
			AssertResult(AccChargeSupplyTypeOverrideSchema.Constants.ACS_ParentTableCode, new[] { "AC" });

			var incoTerms = supplyTypeOverride.IncotermList.GetAllCodes().Concat(new[] {
				Constants.IncoTerms.DeliveredDutyUnpaid,
				Constants.IncoTerms.DeliveredExQuay,
				Constants.IncoTerms.DeliveredExShip,
				Constants.IncoTerms.DeliveredAtFrontier
			}).ToList();
			AssertResult(AccChargeSupplyTypeOverrideSchema.Constants.ACS_IncoTerm, incoTerms);

			var jobTypes = supplyTypeOverride.JobTypeList.GetAllCodes().Concat(new[] { "INC", "PSQ", "TRS" }).ToList();
			AssertResult(AccChargeSupplyTypeOverrideSchema.Constants.ACS_JobType, jobTypes);

			void AssertResult(string columnName, IEnumerable<string> allowedValues)
			{
				AssertConstraintExists(
					AccChargeSupplyTypeOverrideSchema.Constants.SqlSchemaName,
					AccChargeSupplyTypeOverrideSchema.Constants.TableName,
					columnName, allowedValues);
			}
		}

		public void TestAccDraftInvoiceJob_ParentTableCode()
		{
			//WI00819249 will update this fixed list to be based on which BO's implement IJobInvoicingPlugIn and IJobCostingPlugIn.
			var allJobTypes = new[] { "BF", "BH", "BP", "C4", "CM", "CPH", "CS", "CSH", "D4", "EL", "ET", "EY", "JC", "JE", "JJ", "JK", "JM", "JS", "KB", "KG", "KM", "LTB", "LTC", "NA", "NC", "WD", "WDC", "WDH", "WDL", "WKI", "WKP", "WKR", "WRC", "WRH", "WS", "WSJ", "WVO", "YRA", "YRE", "YTU", "" };
			AssertConstraintExists(AccDraftInvoiceJobSchema.Constants.SqlSchemaName, AccDraftInvoiceJobSchema.Constants.TableName, AccDraftInvoiceJobSchema.Constants.AIJ_ParentTableCode, allJobTypes);
		}
	}
}
