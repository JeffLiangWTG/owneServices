using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using InvoicingLineBase = Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	public class IncompleteTransactionDataAdapter<T> : BaseAccountingDataAdapter<T, IncompleteTransactionHeader> where T : InvoicingBase
	{
		public IncompleteTransactionDataAdapter(T incompleteTransaction)
		{
			IncompleteTransaction = incompleteTransaction;
		}

		#region Export

		protected override void ExportToValueObjectCore(T transaction, IncompleteTransactionHeader xmlTransaction, IValueObjectExportContext context)
		{
			PopulateValuesForXmlInvoiceHeader(transaction, xmlTransaction, context);
		}

		void PopulateValuesForXmlInvoiceHeader(T transaction, IncompleteTransactionHeader xmlTransaction, IValueObjectExportContext context)
		{
			xmlTransaction.InvoiceNumber = transaction.AH_TransactionNum;
			xmlTransaction.ExpectedTotal = Xsd.FinancialValue.FromAmountAndCurrency(transaction.ExpectedInvoiceTotal, transaction.TransactionCurrency);
			xmlTransaction.ExpectedExclTaxTotal = Xsd.FinancialValue.FromAmountAndCurrency(transaction.ExpectedInvoiceExclTaxTotal, transaction.TransactionCurrency);
			xmlTransaction.ExpectedTaxTotal = Xsd.FinancialValue.FromAmountAndCurrency(transaction.ExpectedInvoiceTaxTotal, transaction.TransactionCurrency);
			xmlTransaction.ValidateExpectedTotal = transaction.ValidateExpectedInvoiceTotal;
			var lineOrApportionmentChargeImportedFromPKToIDMapping = CreateLinesOrAppotionSplitChargesPKToIDMapping(transaction.Lines);
			xmlTransaction.ConsolCosts = GetConsolCostCollection(transaction.ConsolCosting.ConsolCosts, lineOrApportionmentChargeImportedFromPKToIDMapping);
			xmlTransaction.JobRelatedLines = GetJobRelatedLinesCollection(transaction.Lines, lineOrApportionmentChargeImportedFromPKToIDMapping);
			xmlTransaction.TaxTransactionsInfo = GetXmlTaxTransactionsInfo(transaction, lineOrApportionmentChargeImportedFromPKToIDMapping, context);
		}

		ConsolCostCollection GetConsolCostCollection(APInvoiceConsolCostCollection consolCosts, IReadOnlyDictionary<ZGuid, int> lineOrApportionmentChargeImportedFromPKToIDMapping)
		{
			ConsolCostCollection result = null;
			if (consolCosts != null && consolCosts.Count > 0)
			{
				result = new ConsolCostCollection();
				foreach (JobConsolCost consolCost in consolCosts)
				{
					ConsolCost xmlConsolCost = result.AddNew();
					PopulateValuesForXmlConsolCost(consolCost, xmlConsolCost, lineOrApportionmentChargeImportedFromPKToIDMapping);
				}
			}

			return result;
		}

		void PopulateValuesForXmlConsolCost(JobConsolCost consolCost, ConsolCost xmlConsolCost, IReadOnlyDictionary<ZGuid, int> lineOrApportionmentChargeImportedFromPKToIDMapping)
		{
			xmlConsolCost.ChargeCode = consolCost.ChargeCode != null ? consolCost.ChargeCode.AC_Code : ZString.Empty;
			xmlConsolCost.ConsolId = consolCost.Consol != null ? consolCost.Consol.JK_UniqueConsignRef : ZString.Empty;
			xmlConsolCost.ExchangeRate = consolCost.E6_ExchangeRate;
			xmlConsolCost.OSCostAmount = FinancialValue.FromAmountAndCurrency(consolCost.E6_OSCostAmount, consolCost.Currency);
			xmlConsolCost.TaxId = consolCost.TaxRate != null ? consolCost.TaxRate.AT_Code : ZString.Empty;
			xmlConsolCost.TaxMessage = consolCost.VATClass != null ? consolCost.VATClass.A9_Code : ZString.Empty;
			xmlConsolCost.TaxDate = consolCost.E6_TaxDate;
			xmlConsolCost.OSCostGstAmount = FinancialValue.FromAmountAndCurrency(consolCost.E6_OSGSTAmount_Calc, consolCost.Currency);
			xmlConsolCost.OSCostQstAmount = FinancialValue.FromAmountAndCurrency(consolCost.E6_OSExtraTaxAmount, consolCost.Currency);
			xmlConsolCost.GstInclusiveAmount = FinancialValue.FromAmountAndCurrency((ZDecimal)(consolCost.E6_OSCostAmount + consolCost.E6_OSGSTRealAmount), consolCost.Currency);
			xmlConsolCost.PrepaidCollect = consolCost.E6_PPDCLT;
			xmlConsolCost.ApportionmentMethod = consolCost.E6_ApportionmentMethod;
			xmlConsolCost.OSTotalTaxAmount = FinancialValue.FromAmountAndCurrency(consolCost.E6_OSGSTAmount_Calc, consolCost.Currency);
			xmlConsolCost.IsFinal = consolCost.IsFinal;
			xmlConsolCost.GovernmentReportingChargeCode = consolCost.E6_CostGovtChargeCode;
			xmlConsolCost.SellGovernmentChargeCode = consolCost.E6_SellGovtChargeCode;
			xmlConsolCost.DisplayRelatedShipments = consolCost.E6_ApportionToRelatedShipments;
			xmlConsolCost.WHTTaxId = consolCost.WithholdingTax != null ? consolCost.WithholdingTax.AW_Code : ZString.Empty;
			xmlConsolCost.CostTaxBranch = consolCost.CostTaxBranch != null ? consolCost.CostTaxBranch.GB_Code : ZString.Empty;
			xmlConsolCost.FixedPlaceOfSupply = consolCost.E6_PlaceOfSupply;
			xmlConsolCost.FixedPlaceOfSupplyType = consolCost.E6_PlaceOfSupplyType;
			xmlConsolCost.SupplyType = consolCost.E6_SupplyType;

			if (consolCost.RelatedConsolCostPK.IsValid)
			{
				xmlConsolCost.RelatedCostFromDatabase = consolCost.RelatedConsolCostPK.ToString();
			}

			xmlConsolCost.ConsolCostCharges = GetConsolCostChargesCollection(consolCost, lineOrApportionmentChargeImportedFromPKToIDMapping);
		}

		ConsolCostChargeCollection GetConsolCostChargesCollection(JobConsolCost consolCost, IReadOnlyDictionary<ZGuid, int> lineOrApportionmentChargeImportedFromPKToIDMapping)
		{
			ConsolCostChargeCollection result = null;
			if (consolCost.ApportionmentCharges != null && consolCost.ApportionmentCharges.Count > 0)
			{
				result = new ConsolCostChargeCollection();
				foreach (ApportionSplitCharge charge in consolCost.ApportionmentCharges)
				{
					ConsolCostCharge xmlCharge = result.AddNew();
					PopulateValuesForXmlConsolCostCharge(consolCost, charge, xmlCharge);

					if (lineOrApportionmentChargeImportedFromPKToIDMapping.TryGetValue(charge.PK, out int consolCostChargeId))
					{
						xmlCharge.ConsolCostChargeID = consolCostChargeId;
					}
				}
			}

			return result;
		}

		void PopulateValuesForXmlConsolCostCharge(JobConsolCost consolCost, ApportionSplitCharge charge, ConsolCostCharge xmlCharge)
		{
			xmlCharge.IsUse = charge.JR_IsUsedForApportionment;
			xmlCharge.JobNumber = charge.JobReference;
			xmlCharge.Branch = charge.Branch.GB_Code;
			xmlCharge.Department = charge.Department.GE_Code;
			xmlCharge.CostAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode(charge.JR_OSCostAmt, charge.JR_OSCostCurrencyCode);
			xmlCharge.GstAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode(charge.JR_OSCostGSTAmt_Calc, charge.JR_OSCostCurrencyCode);
			xmlCharge.TotalAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode(charge.JR_Calc_OSCostAmtWithGST, charge.JR_OSCostCurrencyCode);
			xmlCharge.GovernmentReportingChargeCode = charge.JR_CostGovtChargeCode;
			xmlCharge.SellGovernmentChargeCode = charge.JR_SellGovtChargeCode;

			xmlCharge.LocalCostAmount = Xsd.FinancialValue.FromAmountAndCurrency(charge.JR_LocalCostAmt, charge.Branch.Company.LocalCurrency);

			xmlCharge.IsFinal = charge.IsFinal;
			xmlCharge.SellAccount = charge.JR_OH_SellAccount.ToString();

			xmlCharge.InternalJobNumber = charge.InternalJob != null ? charge.InternalJob.JH_JobNum : ZString.Empty;
			xmlCharge.InternalBranch = charge.InternalBranch != null ? charge.InternalBranch.GB_Code : ZString.Empty;
			xmlCharge.InternalDepartment = charge.InternalDept != null ? charge.InternalDept.GE_Code : ZString.Empty;

			xmlCharge.CostTaxBranch = charge.CostTaxBranch != null ? charge.CostTaxBranch.GB_Code : ZString.Empty;
			xmlCharge.SellTaxBranch = charge.SellTaxBranch != null ? charge.SellTaxBranch.GB_Code : ZString.Empty;

			xmlCharge.CostPlaceOfSupply = charge.JR_CostPlaceOfSupply;
			xmlCharge.CostPlaceOfSupplyType = charge.JR_CostPlaceOfSupplyType;

			xmlCharge.CostSupplyType = charge.JR_CostSupplyType;

			if (charge.RelatedApportionChargeFromDB != null)
			{
				xmlCharge.RelatedApportionChargeFromDB = charge.RelatedApportionChargeFromDB.PK.ToString();
			}
			if (charge.CostWHTRate != null)
			{
				xmlCharge.WHTTaxId = charge.CostWHTRate.AW_Code;
				xmlCharge.OSCostWHTAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode(charge.JR_OSCostWHTAmt, charge.JR_OSCostCurrencyCode);
			}

			var line = consolCost.ParentAPInvoice?.Lines?.Cast<InvoicingLineBase>()?.FirstOrDefault(x => x.ApportionmentChargeImportedFrom?.PK == charge.PK);
			xmlCharge.ComplianceDocumentInfo = line != null ? GetComplianceInfo(line) : null;
		}

		IncompleteTransactionLineCollection GetJobRelatedLinesCollection(InvoicingLineBaseCollection lines, IReadOnlyDictionary<ZGuid, int> lineOrApportionmentChargeImportedFromPKToIDMapping)
		{
			IncompleteTransactionLineCollection result = null;
			if (lines != null && lines.Count > 0)
			{
				foreach (InvoicingLineBase line in lines)
				{
					if (!line.IsPopulatedFromImportedApportionment)
					{
						IncompleteTransactionLine xmlLine = (result ?? (result = new IncompleteTransactionLineCollection())).AddNew();
						PopulateValuesForXmlInvoiceLine(line, xmlLine);

						xmlLine.TransactionLineID = lineOrApportionmentChargeImportedFromPKToIDMapping[line.PK];
					}
				}
			}

			return result;
		}

		PeriodApportionmentLineCollection GetPeriodApportionmentLines(InvoicingLineBase invoiceLine)
		{
			PeriodApportionmentLineCollection result = null;
			var periodApportionmentLines = invoiceLine.PeriodApportionmentLines;
			if (periodApportionmentLines != null && periodApportionmentLines.Count > 0)
			{
				result = new PeriodApportionmentLineCollection();
				foreach (Business.ARAP.Invoicing.PeriodApportionmentLine apportionmentLine in periodApportionmentLines)
				{
					Xsd.PeriodApportionmentLine xmlLine = result.AddNew();
					xmlLine.Period = apportionmentLine.Period;
					xmlLine.OSAmount = apportionmentLine.OSAmount;
					xmlLine.LocalAmount = apportionmentLine.LocalAmount;
					xmlLine.ExchangeRate = apportionmentLine.ExchangeRate;
				}
			}

			return result;
		}

		void PopulateValuesForXmlInvoiceLine(InvoicingLineBase line, IncompleteTransactionLine xmlLine)
		{
			xmlLine.JobNumber = line.JobNumber;
			xmlLine.GenericChargeCode = line.GenericChargeBizO == null ? ZString.Empty : line.GenericChargeBizO.VC_Code;
			xmlLine.Description = line.AL_Desc;
			xmlLine.Branch = line.Branch == null ? ZString.Empty : line.Branch.GB_Code;
			xmlLine.Department = line.Department == null ? ZString.Empty : line.Department.GE_Code;
			xmlLine.Amount = Xsd.FinancialValue.FromAmountAndCurrency(line.AL_OSExTaxAmount, line.TransactionCurrency);
			xmlLine.TaxId = line.TaxRate == null ? ZString.Empty : line.TaxRate.AT_Code;
			xmlLine.TaxDate = line.AL_TaxDate;
			xmlLine.TaxAmount = Xsd.FinancialValue.FromAmountAndCurrency(line.AL_OSTaxAmount, line.TransactionCurrency);
			xmlLine.GstAmount = Xsd.FinancialValue.FromAmountAndCurrency(line.AL_OSGSTAmount, line.TransactionCurrency);
			xmlLine.GstInclusiveAmount = Xsd.FinancialValue.FromAmountAndCurrency(line.AL_OSAmount, line.TransactionCurrency);
			xmlLine.FixedPlaceOfSupply = line.AL_PlaceOfSupply;
			xmlLine.FixedPlaceOfSupplyType = line.AL_PlaceOfSupplyType;
			xmlLine.SupplyType = line.AL_SupplyType;
			xmlLine.GovernmentReportingChargeCode = line.AL_GovtChargeCode;
			xmlLine.TaxBranch = line.TaxBranch == null ? ZString.Empty : line.TaxBranch.GB_Code;

			xmlLine.LocalAmount = Xsd.FinancialValue.FromAmountAndCurrency(line.AL_LocalExTaxAmount, line.Branch == null ? null : line.Branch.Company.LocalCurrency);
			xmlLine.LocalTaxAmount = Xsd.FinancialValue.FromAmountAndCurrency(line.AL_LocalTaxAmount, line.Branch == null ? null : line.Branch.Company.LocalCurrency);
			xmlLine.LocalGstAmount = Xsd.FinancialValue.FromAmountAndCurrency(line.AL_LocalGSTAmount, line.Branch == null ? null : line.Branch.Company.LocalCurrency);

			if (line.AL_ExchangeRate != 1m)
			{
				xmlLine.ExchangeRate = line.AL_ExchangeRate;
			}

			if (line.AL_Calc_InputGSTVATRecoverablePercentage != 100)
			{
				xmlLine.RecoverableGSTVATPercentage = line.AL_Calc_InputGSTVATRecoverablePercentage;
			}

			if (line.OriginalJobCharge != null)
			{
				xmlLine.OriginalJobCharge = line.OriginalJobCharge.PK.ToString();
			}
			xmlLine.IsFinal = line.AL_IsFinalCharge;

			xmlLine.SubAccounts = Invoices.SubAccountHelper.GetSubAccountsXmlFromSubAccounts(line.Factory, line.SubAccounts);

			xmlLine.TaxMessage = line.VATClass != null ? line.VATClass.A9_Code : ZString.Empty;
			if (line.IndexOfImportedUniversalTransactionLine != -1)
			{
				xmlLine.UXMLLineIndex = line.IndexOfImportedUniversalTransactionLine;
			}
			xmlLine.WHTTaxId = line.Withholding != null ? line.Withholding.AW_Code : ZString.Empty;

			xmlLine.PeriodApportionmentMethod = line.PeriodApportionmentMethod;
			xmlLine.PeriodStartDate = line.PeriodStartDate;
			xmlLine.PeriodEndDate = line.PeriodEndDate;
			xmlLine.PeriodClearingGLAccountPK = line.PeriodClearingGLAccountPK.ToString();
			xmlLine.PeriodApportionmentLines = GetPeriodApportionmentLines(line);
			xmlLine.ComplianceDocumentInfo = GetComplianceInfo(line);
		}

		ComplianceDocumentInfo GetComplianceInfo(IComplianceInfoToImport complianceToImport)
		{
			ComplianceDocumentInfo complianceInfo = null;

			if (complianceToImport.CreateComplianceDocumentRecordOnPosting)
			{
				complianceInfo = new ComplianceDocumentInfo();
				complianceInfo.CreateComplianceDocumentRecordOnPosting = complianceToImport.CreateComplianceDocumentRecordOnPosting;
				complianceInfo.ComplianceDocumentNumber = complianceToImport.ComplianceDocumentNumber;
				complianceInfo.ComplianceSubType = complianceToImport.ComplianceSubType;
				complianceInfo.ComplianceDocumentOrganization = complianceToImport.ComplianceDocumentOrganization.ToString();
				complianceInfo.ComplianceDocumentVATRegistrationNum = complianceToImport.ComplianceDocumentVATRegistrationNum;
				complianceInfo.ComplianceDocumentDate = complianceToImport.ComplianceDocumentDate;
				complianceInfo.ComplianceDocumentReportingPeriod = complianceToImport.ComplianceDocumentReportingPeriod;
				complianceInfo.ComplianceDocumentSupportingReason = complianceToImport.ComplianceDocumentSupportingReason;
				complianceInfo.ComplianceSupportingDocumentType = complianceToImport.ComplianceSupportingDocumentType;
				complianceInfo.ComplianceSupportingDocumentNumber = complianceToImport.ComplianceSupportingDocumentNumber;
			}

			return complianceInfo;
		}

		#region Export - TaxTransacitons

		IReadOnlyDictionary<ZGuid, int> CreateLinesOrAppotionSplitChargesPKToIDMapping(InvoicingLineBaseCollection lines)
		{
			var pkToIDMapping = new Dictionary<ZGuid, int>();

			int id = 0;
			foreach (InvoicingLineBase line in lines)
			{
				// If line is imported from apportionment split charge, then we add the same id for the charge PK.
				// This is because while saving apportionment split charge, we save the id against the split charge as we do not save its line in xml. While saving tax transaction we have only line PKs for its pivots. 			
				// On restore, we use this id to trace the new lines imported from the restored apportionment split charges and inform the TaxFramework (for restoring tax transactions) of these lines via the TransactionLineIDs properties in TaxRecordData list.
				if (line.IsPopulatedFromImportedApportionment)
				{
					pkToIDMapping.Add(line.ApportionmentChargeImportedFrom.PK, id);
				}
				pkToIDMapping.Add(line.PK, id);

				id++;
			}

			return pkToIDMapping;
		}

		TaxTransactionsInfo GetXmlTaxTransactionsInfo(T transaction, IReadOnlyDictionary<ZGuid, int> lineOrApportionmentChargeImportedFromPKToIDMapping, IValueObjectExportContext context)
		{
			TaxTransactionsInfo result = new TaxTransactionsInfo();
			result.IsSpecified = false;

			TaxTransactionCollection taxTransactionCollection = null;

			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(transaction);
			var taxRecordsData = TaxProcessor.GetTaxRecordDataForDataTransfer(taxRecordParent);

			if (taxRecordsData != null)
			{
				result.IsSpecified = true;
				result.IsTaxTransactionsCalculated = true;

				taxTransactionCollection = new TaxTransactionCollection();

				foreach (var taxRecordData in taxRecordsData)
				{
					var xmlTaxTransaction = taxTransactionCollection.AddNew();
					PopulateValuesForXmlTaxTransaction(taxRecordData, xmlTaxTransaction, transaction.Factory, lineOrApportionmentChargeImportedFromPKToIDMapping, context);
				}

				result.TaxTransactions = taxTransactionCollection;
			}

			return result;
		}

		void PopulateValuesForXmlTaxTransaction(IReadOnlyTaxRecordData taxRecordData, TaxTransaction xmlTaxTransaction, BusinessObjectFactory factory, IReadOnlyDictionary<ZGuid, int> lineOrApportionmentChargeImportedFromPKToIDMapping, IValueObjectExportContext context)
		{
			xmlTaxTransaction.TaxMessage = GetBizoByPK<AccInvMsg>(factory, taxRecordData.TaxMessagePK, context)?.A9_Code ?? ZString.Empty;
			xmlTaxTransaction.AffectsSourceTransactionTotal = taxRecordData.AffectsSourceTransactionTotal;
			xmlTaxTransaction.LedgerControlAccountPK = taxRecordData.LedgerControlGLAccountPK.ToString();
			xmlTaxTransaction.TaxControlAccountPK = taxRecordData.TaxControlGLAccountPK.ToString();
			xmlTaxTransaction.TaxExpenseAccountPK = taxRecordData.TaxExpenseGLAccountPK.ToString();
			xmlTaxTransaction.TaxPendingControlAccountPK = taxRecordData.TaxPendingControlGLAccountPK.ToString();
			xmlTaxTransaction.TaxId = GetBizoByPK<AccTaxRate>(factory, taxRecordData.TaxIDPK, context)?.AT_Code ?? ZString.Empty;
			xmlTaxTransaction.Basis = taxRecordData.TaxBasis;
			xmlTaxTransaction.TaxConfigurationPK = taxRecordData.TaxConfigurationPK.ToString();
			xmlTaxTransaction.LocalTaxAmount.Value = taxRecordData.LocalTaxAmount;
			xmlTaxTransaction.LocalTaxBaseAmount.Value = taxRecordData.LocalTaxBaseAmount;
			xmlTaxTransaction.OSTaxAmount.Value = taxRecordData.OSTaxAmount;
			xmlTaxTransaction.OSTaxBaseAmount.Value = taxRecordData.OSTaxBaseAmount;
			xmlTaxTransaction.BranchCode = GetBizoByPK<GlbBranch>(factory, taxRecordData.BranchPK, context)?.GB_Code ?? ZString.Empty;
			xmlTaxTransaction.DepartmentCode = GetBizoByPK<GlbDepartment>(factory, taxRecordData.DepartmentPK, context)?.GE_Code ?? ZString.Empty;
			xmlTaxTransaction.Ledger = taxRecordData.Ledger;
			xmlTaxTransaction.PostDate = taxRecordData.PostDate;
			xmlTaxTransaction.RateDenominator = taxRecordData.RateDenominator;
			xmlTaxTransaction.RateNumerator = taxRecordData.RateNumerator;
			xmlTaxTransaction.RealisationDate = taxRecordData.RealisationDate;
			xmlTaxTransaction.OSTaxCurrency = taxRecordData.OSTaxCurrency;
			xmlTaxTransaction.TaxAuthorityServiceCode = taxRecordData.TaxAuthorityServiceCode;
			xmlTaxTransaction.TaxAuthorityServiceCodeDescription = taxRecordData.TaxAuthorityServiceCodeDescription;
			xmlTaxTransaction.TaxDate = taxRecordData.TaxDate;
			xmlTaxTransaction.TaxSuperType = taxRecordData.TaxSuperType;
			xmlTaxTransaction.TaxSystemCode = taxRecordData.TaxSystemCode;
			xmlTaxTransaction.TransactionLineOrConsolCostChargeIDs = PopulateXmlTransactionLineIDs(taxRecordData, lineOrApportionmentChargeImportedFromPKToIDMapping);
			PopulateXmlTaxTransactionSystemCalculatedValues(taxRecordData, xmlTaxTransaction);
		}

		int[] PopulateXmlTransactionLineIDs(IReadOnlyTaxRecordData taxRecordData, IReadOnlyDictionary<ZGuid, int> lineOrApportionmentChargeImportedFromPKToIDMapping)
		{
			var xmlTransactionLineIDs = new List<int>();

			foreach (var linePK in taxRecordData.TransactionLinePKs)
			{
				xmlTransactionLineIDs.Add(lineOrApportionmentChargeImportedFromPKToIDMapping[linePK]);
			}

			return xmlTransactionLineIDs.ToArray();
		}

		void PopulateXmlTaxTransactionSystemCalculatedValues(IReadOnlyTaxRecordData taxRecordData, TaxTransaction xmlTaxTransaction)
		{
			xmlTaxTransaction.SystemCalculatedValuesInfo.IsSystemCalculatedValuesDefined = false;

			if (taxRecordData.SystemCalculatedValues != null)
			{
				xmlTaxTransaction.SystemCalculatedValuesInfo.IsSystemCalculatedValuesDefined = true;
				xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues = new SystemCalculatedValues();

				xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.OSTaxBaseAmount.Value = taxRecordData.SystemCalculatedValues.OSTaxBaseAmount;
				xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.OSTaxAmount.Value = taxRecordData.SystemCalculatedValues.OSTaxAmount;
				xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.RateNumerator = taxRecordData.SystemCalculatedValues.RateNumerator;
				xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.RateDenominator = taxRecordData.SystemCalculatedValues.RateDenominator;
				xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.TaxDate = taxRecordData.SystemCalculatedValues.TaxDate;
				xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.TaxAuthorityServiceCode = taxRecordData.SystemCalculatedValues.TaxAuthorityServiceCode;
				xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.TaxAuthorityServiceCodeDescription = taxRecordData.SystemCalculatedValues.TaxAuthorityServiceCodeDescription;
			}
		}

		#endregion

		BizoTypeName GetBizoByPK<BizoTypeName>(BusinessObjectFactory factory, ZGuid pk, IValueObjectExportContext context) where BizoTypeName : BusinessObject
		{
			var bizo = factory.Load<BizoTypeName>(pk);
			return bizo;
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(T transaction, IncompleteTransactionHeader xmlTransaction, IValueObjectImportContext context)
		{
			transaction.Factory.SetContext(BusinessContext.IncompleteInvoiceDataAdapter);
			transaction.Factory.SuspendValidation();
			try
			{
				transaction.ExpectedInvoiceTotal = xmlTransaction.ExpectedTotal.Value;
				transaction.ExpectedInvoiceExclTaxTotal = xmlTransaction.ExpectedExclTaxTotal.Value;
				transaction.ExpectedInvoiceTaxTotal = xmlTransaction.ExpectedTaxTotal.Value;
				transaction.ValidateExpectedInvoiceTotal = xmlTransaction.ValidateExpectedTotal;

				using (transaction.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender().GetSuspender())
				{
					var lineOrApportionmentChargeImportedFromIDToPKMapping = new Dictionary<int, ZGuid>();
					BuildJobNumberToPKMapping(xmlTransaction, transaction.Factory);
					ImportConsolCosts(transaction, xmlTransaction, lineOrApportionmentChargeImportedFromIDToPKMapping, context);
					ImportTransactionLines(transaction, xmlTransaction, lineOrApportionmentChargeImportedFromIDToPKMapping, context);
					ImportTaxTransactions(transaction, xmlTransaction, lineOrApportionmentChargeImportedFromIDToPKMapping, context);
				}
			}
			finally
			{
				transaction.Factory.ResumeValidation();
				transaction.Factory.RemoveContext(BusinessContext.IncompleteInvoiceDataAdapter);
			}
		}

		void BuildJobNumberToPKMapping(IncompleteTransactionHeader xmlTransaction, BusinessObjectFactory factory)
		{
			var jobNumberToPKMappingProvider = new JobNumberToPKMappingProvider(factory, LoadJobs);
			jobNumberToPKMappingProvider.BatchLoadJobs(xmlTransaction);
			factory.ServiceContainer.AddService(jobNumberToPKMappingProvider);
		}

		void ImportTransactionLines(T transaction, IncompleteTransactionHeader xmlTransaction, Dictionary<int, ZGuid> lineOrApportionmentChargeImportedFromIDToPKMapping, IValueObjectImportContext context)
		{
			if (xmlTransaction.JobRelatedLines != null)
			{
				var jobNumberToPKMapping = transaction.Factory.ServiceContainer.GetService<JobNumberToPKMappingProvider>().JobNumberToPKMapping;

				using (transaction.Lines.SuspendListChanged())
				{
					foreach (IncompleteTransactionLine xmlLine in xmlTransaction.JobRelatedLines)
					{
						InvoicingLineBase line = (InvoicingLineBase)transaction.Lines.AddNew();
						ImportTransactionLine(line, xmlLine, context, jobNumberToPKMapping);
						if (xmlLine.TransactionLineIDSpecified)
						{
							lineOrApportionmentChargeImportedFromIDToPKMapping.Add(xmlLine.TransactionLineID, line.PK);
						}
					}
				}
			}
		}

		void ImportTransactionLine(InvoicingLineBase line, IncompleteTransactionLine xmlLine, IValueObjectImportContext context, IReadOnlyDictionary<ZString, ZGuid> jobNumberToPKMapping)
		{
			line.GenericCharge = GetGenericChargePK(line.Factory, xmlLine.GenericChargeCode, context);
			line.AL_JH = GetJobPK(xmlLine.JobNumber, context, jobNumberToPKMapping);
			line.AL_Desc = xmlLine.Description;
			line.AL_GB = GetBranchPK(line.Factory, xmlLine.Branch, context);
			line.AL_GE = GetDepartmentPK(line.Factory, xmlLine.Department, context);
			line.AL_RX_NKTransactionCurrency = ExistingOrEmptyCurrencyCode(line.Factory, xmlLine.Amount.CurrencyCode, context);
			line.AL_GB_TaxBranch = GetBranchPK(line.Factory, xmlLine.TaxBranch, context);

			context.SetPropertyInfoValue(line.AL_GovtChargeCodeInfo, xmlLine.GovernmentReportingChargeCode, xmlLine.GovernmentReportingChargeCodeSpecified, Res.GetString("67D80496-4700-4F7E-85CE-81E9DD47C98C", "{0} Govt Charge Code", context));
			context.SetPropertyInfoValue(line.AL_PlaceOfSupplyInfo, xmlLine.FixedPlaceOfSupply, xmlLine.FixedPlaceOfSupplySpecified, Res.GetString("489D8B2C-FA11-4CF0-81F8-F7D290B81252", "{0} Fixed Place Of Supply", context));
			context.SetPropertyInfoValue(line.AL_PlaceOfSupplyTypeInfo, xmlLine.FixedPlaceOfSupplyType, xmlLine.FixedPlaceOfSupplyTypeSpecified, Res.GetString("7441FF8F-8A04-479E-A62F-32F169B29D03", "{0} Fixed Place Of Supply Type", context));
			context.SetPropertyInfoValue(line.AL_SupplyTypeInfo, xmlLine.SupplyType, xmlLine.SupplyTypeSpecified, Res.GetString("A2460A0D-92F4-4585-9541-2A6FA6B08ABE", "{0} Supply Type", context));

			if (xmlLine.ExchangeRateSpecified)
			{
				line.AL_ExchangeRate = xmlLine.ExchangeRate;
			}

			line.AL_OSExTaxAmount = xmlLine.Amount.Value;
			line.AL_AT = GetTaxRatePK(line.Factory, xmlLine.TaxId, context);

			if (!xmlLine.TaxDate.IsEmpty)
			{
				line.AL_TaxDate = xmlLine.TaxDate;
			}

			line.AL_OSTaxAmount = xmlLine.TaxAmount.Value;
			line.AL_OSGSTAmount = xmlLine.GstAmount.Value;
			line.AL_OSAmount = xmlLine.GstInclusiveAmount.Value;
			line.AL_A9_VATClass = GetTaxMessagePK(line.Factory, xmlLine.TaxMessage, context);
			line.AL_AW = GetWHTTaxPK(line.Factory, xmlLine.WHTTaxId, context);

			using (line.GetOSAmountCalculationSuspender())
			{
				if (xmlLine.LocalAmountSpecified)
				{
					line.AL_LocalExTaxAmount = xmlLine.LocalAmount.Value;
				}
				if (xmlLine.LocalTaxAmountSpecified)
				{
					line.AL_LocalTaxAmount = xmlLine.LocalTaxAmount.Value;
				}
				if (xmlLine.LocalGstAmountSpecified)
				{
					line.AL_LocalGSTAmount = xmlLine.LocalGstAmount.Value;
				}
			}

			if (xmlLine.RecoverableGSTVATPercentageSpecified && line.SupportsInputTaxRecoverable)
			{
				line.AL_Calc_InputGSTVATRecoverablePercentage = xmlLine.RecoverableGSTVATPercentage;
			}

			line.AL_IsFinalCharge = xmlLine.IsFinal;

			var originalJobCharge = GetCharge(line.Factory, xmlLine.OriginalJobCharge);
			if (line.ValidateIfLineCanBeMarkedAsImported(originalJobCharge))
			{
				line.OriginalJobCharge = originalJobCharge;
			}

			if (xmlLine.SubAccounts != null && xmlLine.SubAccounts.Count > 0)
			{
				foreach (SubAccount subAccount in xmlLine.SubAccounts)
				{
					ImportSubAccount(line, subAccount);
				}
			}
			else
			{
				//For INI backward compatibility.
				ImportSubAccount(line, xmlLine.SubAccount);
			}

			if (xmlLine.UXMLLineIndexSpecified)
			{
				line.IndexOfImportedUniversalTransactionLine = xmlLine.UXMLLineIndex;
			}

			ImportComplianceInfo(line, xmlLine.ComplianceDocumentInfo, context, line.Factory);

			ImportPeriodApportionmentLines(line, xmlLine, context);
		}

		void ImportSubAccount(InvoicingLineBase line, SubAccount subAccount)
		{
			var lineSubAccount = line.SubAccounts.OfType<TransactionLineSubAccount>().FirstOrDefault(x => x.SubAccountTypeDisplayCode.Equals(subAccount.Type.Code));
			if (lineSubAccount != null)
			{
				lineSubAccount.AL1_SubClassParentId = Invoices.SubAccountHelper.GetSubAccountPKFromCode(line.Factory, subAccount.Type.Code, subAccount.Code);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error message")]
		void ImportPeriodApportionmentLines(InvoicingLineBase line, IncompleteTransactionLine xmlLine, IValueObjectImportContext context)
		{
			if (xmlLine.PeriodApportionmentMethod.IsEmpty)
			{
				line.PeriodApportionmentMethod = "DEF";
			}
			else
			{
				line.PeriodApportionmentMethod = xmlLine.PeriodApportionmentMethod;
				line.PeriodStartDate = xmlLine.PeriodStartDate;
				line.PeriodEndDate = xmlLine.PeriodEndDate;

				line.PeriodClearingGLAccountPK = GetRequestedGLAccountPK(line.Factory, xmlLine.PeriodClearingGLAccountPK, context, "Account Type: Period Clearing Account");
			}

			line.PeriodApportionmentLines.RemoveAndDeleteAll();
			if (xmlLine.PeriodApportionmentLines != null && xmlLine.PeriodApportionmentLines.Count > 0)
			{
				foreach (Xsd.PeriodApportionmentLine xmlApportionmentLine in xmlLine.PeriodApportionmentLines)
				{
					var newApportinmentLine = new Business.ARAP.Invoicing.PeriodApportionmentLine(line, xmlApportionmentLine.Period, xmlApportionmentLine.OSAmount);
					line.PeriodApportionmentLines.Add(newApportinmentLine);
				}
				line.PeriodApportionment.UpdateLastLine();
			}
		}

		void ImportConsolCosts(T transaction, IncompleteTransactionHeader xmlTransaction, Dictionary<int, ZGuid> lineOrApportionmentChargeImportedFromIDToPKMapping, IValueObjectImportContext context)
		{
			if (xmlTransaction.ConsolCosts != null)
			{
				using (transaction.GetConsolCostImportPopupSuspender())
				using (transaction.ConsolCosting.ConsolSummary.UpdateSuspender.GetSuspender())
				{
#if DEBUG
					IsConsolCostImportPopupSuspended_ForTestOnly = transaction.IsConsolCostImportPopupSuspended;
					isConsolSummaryUpdateSuspended_ForTestOnly = transaction.ConsolCosting.ConsolSummary.UpdateSuspender.IsSuspended;
#endif

					foreach (ConsolCost xmlConsolCost in xmlTransaction.ConsolCosts)
					{
						JobConsolCost consolCost = transaction.ConsolCosting.ConsolCosts.AddNew();
						ImportConsolCost(consolCost, xmlConsolCost, lineOrApportionmentChargeImportedFromIDToPKMapping, context, transaction.IsAllowedToSetExchangeRate);
					}
				}
			}
		}

		#region Import - Tax Transactions

		void ImportTaxTransactions(T transaction, IncompleteTransactionHeader xmlTransaction, IReadOnlyDictionary<int, ZGuid> lineOrApportionmentChargeImportedFromIDToPKMapping, IValueObjectImportContext context)
		{
			List<TaxRecordData> taxRecordsData = null;

			if (xmlTransaction.TaxTransactionsInfo.IsTaxTransactionsCalculated)
			{
				taxRecordsData = new List<TaxRecordData>();

				foreach (TaxTransaction taxTransaction in xmlTransaction.TaxTransactionsInfo.TaxTransactions)
				{
					var taxRecord = new TaxRecordData();
					PopulateTaxRecordData(taxTransaction, taxRecord, transaction.Factory, lineOrApportionmentChargeImportedFromIDToPKMapping, context);
					taxRecordsData.Add(taxRecord);
				}
			}

			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(transaction);
			RestoreTaxTransactionInIncompleteInvoiceHelper.SaveTaxRecordsDataInInvoiceFactoryCache(transaction, taxRecordsData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error message")]
		void PopulateTaxRecordData(TaxTransaction xmlTaxTransaction, TaxRecordData taxRecordData, BusinessObjectFactory factory, IReadOnlyDictionary<int, ZGuid> lineOrApportionmentChargeImportedFromIDToPKMapping, IValueObjectImportContext context)
		{
			var errorMsgBuilder = new StringBuilder();
			errorMsgBuilder.Append($"Tax Transaction info: Branch = {xmlTaxTransaction.BranchCode}, Ledger = {xmlTaxTransaction.Ledger}, Tax system = {xmlTaxTransaction.TaxSystemCode}");

			var taxConfig = GetTaxConfiguration(factory, xmlTaxTransaction.TaxConfigurationPK, context, errorMsgBuilder.ToString());
			errorMsgBuilder.Append($", Tax configuration Code = {taxConfig?.ETC_Code ?? ZString.Empty}");

			var errorMsg = errorMsgBuilder.ToString();

			taxRecordData.TaxMessagePK = GetTaxMessagePK(factory, xmlTaxTransaction.TaxMessage, context);
			taxRecordData.AffectsSourceTransactionTotal = xmlTaxTransaction.AffectsSourceTransactionTotal;
			taxRecordData.LedgerControlGLAccountPK = GetRequestedGLAccountPK(factory, xmlTaxTransaction.LedgerControlAccountPK, context, "Account Type: Ledger Control Account, " + errorMsg);
			taxRecordData.TaxControlGLAccountPK = GetRequestedGLAccountPK(factory, xmlTaxTransaction.TaxControlAccountPK, context, "Account Type: Tax Control Account, " +  errorMsg);
			taxRecordData.TaxExpenseGLAccountPK = GetRequestedGLAccountPK(factory, xmlTaxTransaction.TaxExpenseAccountPK, context, "Account Type: Tax Expense Account, " + errorMsg);
			taxRecordData.TaxPendingControlGLAccountPK = GetRequestedGLAccountPK(factory, xmlTaxTransaction.TaxPendingControlAccountPK, context, "Account Type: Tax Pending Control Account, " + errorMsg);
			taxRecordData.TaxIDPK = GetTaxRatePK(factory, xmlTaxTransaction.TaxId, context);
			taxRecordData.TaxBasis = xmlTaxTransaction.Basis;
			taxRecordData.TaxConfigurationPK = taxConfig?.PK ?? ZGuid.Empty;
			taxRecordData.LocalTaxAmount = xmlTaxTransaction.LocalTaxAmount.Value;
			taxRecordData.LocalTaxBaseAmount = xmlTaxTransaction.LocalTaxBaseAmount.Value;
			taxRecordData.OSTaxAmount = xmlTaxTransaction.OSTaxAmount.Value;
			taxRecordData.OSTaxBaseAmount = xmlTaxTransaction.OSTaxBaseAmount.Value;
			taxRecordData.BranchPK = GetBranchPK(factory, xmlTaxTransaction.BranchCode, context);
			taxRecordData.DepartmentPK = GetDepartmentPK(factory, xmlTaxTransaction.DepartmentCode, context);
			taxRecordData.Ledger = xmlTaxTransaction.Ledger;
			taxRecordData.PostDate = xmlTaxTransaction.PostDate;
			taxRecordData.RateDenominator = xmlTaxTransaction.RateDenominator;
			taxRecordData.RateNumerator = xmlTaxTransaction.RateNumerator;
			taxRecordData.RealisationDate = xmlTaxTransaction.RealisationDate;
			taxRecordData.OSTaxCurrency = xmlTaxTransaction.OSTaxCurrency;
			taxRecordData.TaxAuthorityServiceCode = xmlTaxTransaction.TaxAuthorityServiceCode;
			taxRecordData.TaxAuthorityServiceCodeDescription = xmlTaxTransaction.TaxAuthorityServiceCodeDescription;
			taxRecordData.TaxDate = xmlTaxTransaction.TaxDate;
			taxRecordData.TaxSuperType = xmlTaxTransaction.TaxSuperType;
			taxRecordData.TaxSystemCode = xmlTaxTransaction.TaxSystemCode;
			PopulateTaxRecordDataObjectLineOrChargePKs(xmlTaxTransaction.TransactionLineOrConsolCostChargeIDs, taxRecordData, lineOrApportionmentChargeImportedFromIDToPKMapping);
			PopulateTaxRecordDataSystemCalculatedValues(xmlTaxTransaction, taxRecordData);
		}

		void PopulateTaxRecordDataObjectLineOrChargePKs(int[] xmlTranscationLineOrConsolCostChargeIDs, TaxRecordData taxRecordData, IReadOnlyDictionary<int, ZGuid> lineOrApportionmentChargeImportedFromIDToPKMapping)
		{
			var lineOrChargePKs = new List<ZGuid>();

			foreach (var id in xmlTranscationLineOrConsolCostChargeIDs)
			{
				lineOrChargePKs.Add(lineOrApportionmentChargeImportedFromIDToPKMapping[id]);
			}

			//Apportionment Charge PKs, we add here, will be transformed to line PKs later in InvocingBase.RestoreSavedData after lines will be imported for those apportionment charges.
			taxRecordData.TransactionLinePKs = lineOrChargePKs;
		}

		void PopulateTaxRecordDataSystemCalculatedValues(TaxTransaction xmlTaxTransaction, TaxRecordData taxRecordData)
		{
			if (xmlTaxTransaction.SystemCalculatedValuesInfo.IsSystemCalculatedValuesDefined)
			{
				taxRecordData.SystemCalculatedValues = new TaxRecordDataSystemCalculatedValues(
					xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.OSTaxBaseAmount.Value,
					xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.OSTaxAmount.Value,
					xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.RateNumerator,
					xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.RateDenominator,
					xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.TaxDate,
					xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.TaxAuthorityServiceCode,
					xmlTaxTransaction.SystemCalculatedValuesInfo.SystemCalculatedValues.TaxAuthorityServiceCodeDescription
				);
			}
			else
			{
				taxRecordData.SystemCalculatedValues = null;
			}
		}

		#endregion

#if DEBUG

		internal bool IsConsolCostImportPopupSuspended_ForTestOnly
		{
			get
			{
				var isPopupSuspended = isConsolCostImportPopupSuspended_ForTestOnly;
				ResetConsolCostImportPopupSuspended_ForTestOnly();
				return isPopupSuspended;
			}
			set
			{
				isConsolCostImportPopupSuspended_ForTestOnly = value;
			}
		}

		void ResetConsolCostImportPopupSuspended_ForTestOnly()
		{
			IsConsolCostImportPopupSuspended_ForTestOnly = false;
		}

		internal bool isConsolSummaryUpdateSuspended_ForTestOnly;
		bool isConsolCostImportPopupSuspended_ForTestOnly;
#endif

		void ImportConsolCost(JobConsolCost consolCost, ConsolCost xmlConsolCost, Dictionary<int, ZGuid> lineOrApportionmentChargeImportedFromIDToPKMapping, IValueObjectImportContext context, bool isAllowedToSetExchangeRate)
		{
			IJobCostingPlugIn consol = GetConsol(consolCost.Factory, xmlConsolCost.ConsolId, context);
			consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol == null ? ZGuid.Empty : consol.CostSupporter.PK, consol == null ? ZString.Empty : consol.CostSupporter.Type);
			consolCost.E6_AC_ChargeCode = GetChargeCodePK(consolCost.Factory, xmlConsolCost.ChargeCode, context);
			context.SetPropertyInfoValue(consolCost.E6_RX_NKCurrencyInfo, xmlConsolCost.OSCostAmount.CurrencyCode, xmlConsolCost.OSCostAmount.CurrencyCodeSpecified, Res.GetString("82bf23c1-4d38-4827-9ed4-f2bdfff9e3c5", "{0} Consol Cost Currency Code", context));
			if (isAllowedToSetExchangeRate && xmlConsolCost.ExchangeRate != ZDecimal.Zero)
			{
				consolCost.E6_ExchangeRate = xmlConsolCost.ExchangeRate;
			}
			consolCost.E6_OSCostAmount = xmlConsolCost.OSCostAmount.Value;
			consolCost.E6_AT_TaxRate = GetTaxRatePK(consolCost.Factory, xmlConsolCost.TaxId, context);
			consolCost.E6_A9_VATClass = GetTaxMessagePK(consolCost.Factory, xmlConsolCost.TaxMessage, context);

			if (!xmlConsolCost.TaxDate.IsEmpty)
			{
				consolCost.E6_TaxDate = xmlConsolCost.TaxDate;
			}

			consolCost.E6_OSGSTAmount_Calc = xmlConsolCost.OSCostGstAmount.Value;
			context.SetPropertyInfoValue(consolCost.E6_PPDCLTInfo, xmlConsolCost.PrepaidCollect, xmlConsolCost.PrepaidCollectSpecified, Res.GetString("c0b4c313-8d95-402f-9601-34174a0c3f86", "{0} Consol Cost Prepaid Collect", context));
			context.SetPropertyInfoValue(consolCost.E6_ApportionmentMethodInfo, xmlConsolCost.ApportionmentMethod, xmlConsolCost.ApportionmentMethodSpecified, Res.GetString("cffb7b5c-5570-4e93-a5b9-b31668c99910", "{0} Consol Cost Apportionment Method", context));
			consolCost.IsFinal = xmlConsolCost.IsFinal;
			consolCost.E6_ApportionToRelatedShipments = xmlConsolCost.DisplayRelatedShipments;
			consolCost.E6_AW = GetWHTTaxPK(consolCost.Factory, xmlConsolCost.WHTTaxId, context);

			var relatedCost = GetConsolCost(consolCost.Factory, xmlConsolCost.RelatedCostFromDatabase);
			if (consolCost.ValidateIfRelatedConsolCostCanBeMarkedAsImported(relatedCost))
			{
				consolCost.RelatedConsolCostPK = relatedCost.PK;
			}

			context.SetPropertyInfoValue(consolCost.E6_CostGovtChargeCodeInfo, xmlConsolCost.GovernmentReportingChargeCode, xmlConsolCost.GovernmentReportingChargeCodeSpecified, Res.GetString("6C549C86-8D82-4C24-96C4-48471874E0BD", "{0} Govt Charge Code", context));
			context.SetPropertyInfoValue(consolCost.E6_SellGovtChargeCodeInfo, xmlConsolCost.SellGovernmentChargeCode, xmlConsolCost.SellGovernmentChargeCodeSpecified, Res.GetString("D1899AD2-8961-427d-972A-5EE85E3FCCD3", "{0} Sell Govt Charge Code", context));

			consolCost.E6_GB_CostTaxBranch = GetBranchPK(consolCost.Factory, xmlConsolCost.CostTaxBranch, context);

			context.SetPropertyInfoValue(consolCost.E6_PlaceOfSupplyInfo, xmlConsolCost.FixedPlaceOfSupply, xmlConsolCost.FixedPlaceOfSupplySpecified, Res.GetString("D32187E7-8E21-4AE8-8B15-02AEABD2D9DE", "{0} Fixed Place of Supply", context));
			context.SetPropertyInfoValue(consolCost.E6_PlaceOfSupplyTypeInfo, xmlConsolCost.FixedPlaceOfSupplyType, xmlConsolCost.FixedPlaceOfSupplyTypeSpecified, Res.GetString("C1987737-BD61-4DF9-8784-77C4C21BF45C", "{0} Fixed Place of Supply Type", context));

			context.SetPropertyInfoValue(consolCost.E6_SupplyTypeInfo, xmlConsolCost.SupplyType, xmlConsolCost.SupplyTypeSpecified, Res.GetString("0A7113AE-03C3-46B5-B149-1997371658AC", "{0} Supply Type", context));

			consolCost.ApportionmentCharges.RemoveAndDeleteAll();
			ImportConsolCostCharges(consolCost.ApportionmentCharges, xmlConsolCost.ConsolCostCharges, consolCost, lineOrApportionmentChargeImportedFromIDToPKMapping, context);

			consolCost.UpdateShipmentInfosOnCharges();
		}

		void ImportConsolCostCharges(ApportionmentSplitChargeCollection apportionmentSplitCharges, ConsolCostChargeCollection xmlConsolCostCharges, JobConsolCost consolCost, Dictionary<int, ZGuid> lineOrApportionmentChargeImportedFromIDToPKMapping, IValueObjectImportContext context)
		{
			var jobNumberToPKMapping = consolCost.Factory.ServiceContainer.GetService<JobNumberToPKMappingProvider>().JobNumberToPKMapping;

			foreach (ConsolCostCharge xmlCharge in xmlConsolCostCharges)
			{
				ApportionSplitCharge charge = consolCost.Factory.New<ApportionSplitCharge>();
				using (charge.GetValidationSuspender())
				{
					ImportConsolCostCharge(charge, xmlCharge, consolCost, context, jobNumberToPKMapping);
					if (xmlCharge.ConsolCostChargeIDSpecified)
					{
						lineOrApportionmentChargeImportedFromIDToPKMapping.Add(xmlCharge.ConsolCostChargeID, charge.PK);
					}
					apportionmentSplitCharges.Add(charge);
				}
			}
		}

		IEnumerable<Job> LoadJobs(BusinessObjectFactory factory, IEnumerable<ZString> jobNumbers, bool fetchFromCacheOnly)
		{
			if (jobNumbers != null && jobNumbers.Any())
			{
				var listJobNumber = jobNumbers.Distinct().Where(s => !s.IsEmpty);
				var jobQuery = new ZQuery()
				{
					FetchOnlyFromLocalCache = fetchFromCacheOnly
				};
				jobQuery.AddToFilter(JobHeaderSchema.JH_JobNum, listJobNumber);
				jobQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				jobQuery.AddToFilter(JobHeaderSchema.JH_IsActive, true);
				var jobs = factory.Load<Job>(jobQuery);

				var duplicatedJobs = jobs.GroupBy(x => x.JH_JobNum).Where(x => x.Count() > 1);
				if (duplicatedJobs.Any())
				{
					ErrorReporter.ReportOnce(GetJobsInfo(factory, duplicatedJobs));
				}

				return jobs;
			}

			return Enumerable.Empty<Job>();
		}

		ZString GetJobsInfo(BusinessObjectFactory factory, IEnumerable<IGrouping<ZString, Job>> duplicatedJobs)
		{
			var jobsInfoBuilder = new ZStringBuilder();
			jobsInfoBuilder.Append($"Duplicated Jobs with job number");
			foreach (var duplicatedJobsForSameJobNum in duplicatedJobs)
			{
				jobsInfoBuilder.Append($" {duplicatedJobsForSameJobNum.Key} count: {duplicatedJobsForSameJobNum.Count()}");
				jobsInfoBuilder.AppendLine();

				foreach (var job in duplicatedJobsForSameJobNum)
				{
					AddSingleJobInfo(jobsInfoBuilder, job);
				}
			}
			return jobsInfoBuilder.ToString();

			void AddSingleJobInfo(ZStringBuilder zStringBuilder, Job job)
			{
				var branch = factory.Load<GlbBranch>(job.JH_GB)?.GB_Code ?? ZString.Empty;
				var department = factory.Load<GlbDepartment>(job.JH_GE)?.GE_Code ?? ZString.Empty;
				var company = factory.Load<GlbCompany>(job.JH_GC)?.GC_Code ?? ZString.Empty;
				zStringBuilder.AppendLine($"Job| JH_JobNum:{job.JH_JobNum}, Job type:{job.JobType}, IsInDb:{job.IsInDatabase}, HasChange:{job.HasChanges}, JH_Status:{job.JH_Status}, JH_A_JOP:{job.JH_A_JOP}, JH_A_JCL:{job.JH_A_JCL}, Branch:{branch}, Department:{department}, Company:{company}, JH_SystemCreateTimeUtc:{job.JH_SystemCreateTimeUtc}, JH_SystemCreateUser:{job.JH_SystemCreateUser}, JH_SystemLastEditTimeUtc:{job.JH_SystemLastEditTimeUtc}, JH_SystemLastEditUser:{job.JH_SystemLastEditUser}, JH_JobLocalReference:{job.JH_JobLocalReference}, JH_ParentID:{job.JH_ParentID}, JH_ParentTableCode:{job.JH_ParentTableCode}");
			}
		}

		void ImportConsolCostCharge(ApportionSplitCharge charge, ConsolCostCharge xmlCharge, JobConsolCost consolCost, IValueObjectImportContext context, IReadOnlyDictionary<ZString, ZGuid> jobNumberToPKMapping)
		{
			charge.JR_OH_CostAccount = consolCost.E6_OH_Creditor;
			charge.JR_AC = consolCost.E6_AC_ChargeCode;
			charge.JR_JH = GetJobPK(xmlCharge.JobNumber, context, jobNumberToPKMapping);
			charge.JR_GB = GetBranchPK(charge.Factory, xmlCharge.Branch, context);
			charge.JR_GE = GetDepartmentPK(charge.Factory, xmlCharge.Department, context);
			RefCurrency costCurrency = charge.Factory.Load<RefCurrency>(GetCurrencyPK(charge.Factory, xmlCharge.CostAmount.CurrencyCode, context));

			if (costCurrency != null)
			{
				charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
			}
			charge.JR_OSCostExRate = consolCost.E6_ExchangeRate;
			charge.JR_OSCostAmt = xmlCharge.CostAmount.Value;
			charge.JR_CostTaxDate = consolCost.E6_TaxDate;
			charge.JR_AT_CostGSTRate = consolCost.E6_AT_TaxRate;
			charge.JR_A9_CostVATClass = consolCost.E6_A9_VATClass;
			using (charge.StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender())
			{
				charge.JR_IsCostTaxAmountOverridden = true;
				charge.JR_OSCostGSTAmt_Calc = xmlCharge.GstAmount.Value;
			}
			context.SetPropertyInfoValue(charge.JR_CostGovtChargeCodeInfo, xmlCharge.GovernmentReportingChargeCode, xmlCharge.GovernmentReportingChargeCodeSpecified, Res.GetString("C1670D53-E86E-4841-A6A3-34DB0E7EC3B8", "{0} Govt Charge Code", context));
			context.SetPropertyInfoValue(charge.JR_SellGovtChargeCodeInfo, xmlCharge.SellGovernmentChargeCode, xmlCharge.SellGovernmentChargeCodeSpecified, Res.GetString("5FF2B2D2-3305-49e7-B4D4-B75DE789D892", "{0} Sell Govt Charge Code", context));

			if (xmlCharge.LocalCostAmountSpecified)
			{
				charge.JR_LocalCostAmt = xmlCharge.LocalCostAmount.Value;
			}

			charge.IsFinal = xmlCharge.IsFinal;
			charge.JR_IsUsedForApportionment = xmlCharge.IsUse;

			charge.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			charge.JR_APDocumentReceivedDate = consolCost.E6_DocumentReceivedDate;
			charge.JR_PaymentDate = consolCost.E6_PaymentDate;
			charge.JR_CostReference = consolCost.E6_CostReference;

			charge.JR_JH_InternalJob = GetJobPK(xmlCharge.InternalJobNumber, context, jobNumberToPKMapping);
			charge.JR_GB_InternalBranch = GetBranchPK(charge.Factory, xmlCharge.InternalBranch, context);
			charge.JR_GE_InternalDept = GetDepartmentPK(charge.Factory, xmlCharge.InternalDepartment, context);

			using (charge.Factory.HasContext(BusinessContext.CASS) ? charge.UpdateCostTaxInfoSuspender.GetSuspender() : null)
			{
				charge.JR_GB_CostTaxBranch = GetBranchPK(charge.Factory, xmlCharge.CostTaxBranch, context);

				charge.JR_CostPlaceOfSupply = consolCost.E6_PlaceOfSupply;
				charge.JR_CostPlaceOfSupplyType = consolCost.E6_PlaceOfSupplyType;

				charge.JR_CostSupplyType = consolCost.E6_SupplyType;
			}

			ZGuid sellAccount;
			if (!string.IsNullOrEmpty(xmlCharge.SellAccount) && ZGuid.TryParse(xmlCharge.SellAccount, out sellAccount))
			{
				charge.JR_OH_SellAccount = sellAccount;
			}
			charge.JR_GB_SellTaxBranch = GetBranchPK(charge.Factory, xmlCharge.SellTaxBranch, context);

			var relatedApportionChargeFromDB = GetCharge(charge.Factory, xmlCharge.RelatedApportionChargeFromDB);
			if (charge.ValidateIfRelatedApportionChargeCanBeMarkedAsImported(relatedApportionChargeFromDB))
			{
				charge.RelatedApportionChargeFromDB = relatedApportionChargeFromDB;
			}

			charge.JR_AW_CostWHTRate = GetWHTTaxPK(charge.Factory, xmlCharge.WHTTaxId, context);
			charge.JR_OSCostWHTAmt = xmlCharge.OSCostWHTAmount.Value;

			ImportComplianceInfo(charge, xmlCharge.ComplianceDocumentInfo, context, charge.Factory);
		}

		void ImportComplianceInfo(IComplianceInfoToImport complianceToImport, ComplianceDocumentInfo complianceInfo, IValueObjectImportContext context, BusinessObjectFactory factory)
		{
			if (complianceInfo != null)
			{
				if (complianceInfo.CreateComplianceDocumentRecordOnPosting)
				{
					complianceToImport.CreateComplianceDocumentRecordOnPosting = complianceInfo.CreateComplianceDocumentRecordOnPosting;
					complianceToImport.ComplianceDocumentNumber = complianceInfo.ComplianceDocumentNumber;
					complianceToImport.ComplianceSubType = complianceInfo.ComplianceSubType;
					complianceToImport.ComplianceDocumentOrganization = GetComplianceDocumentOrganizationPK(factory, complianceInfo.ComplianceDocumentOrganization, context);
					complianceToImport.ComplianceDocumentVATRegistrationNum = complianceInfo.ComplianceDocumentVATRegistrationNum;
					complianceToImport.ComplianceDocumentDate = complianceInfo.ComplianceDocumentDate;
					complianceToImport.ComplianceDocumentReportingPeriod = complianceInfo.ComplianceDocumentReportingPeriod;
					complianceToImport.ComplianceDocumentSupportingReason = complianceInfo.ComplianceDocumentSupportingReason;
					complianceToImport.ComplianceSupportingDocumentType = complianceInfo.ComplianceSupportingDocumentType;
					complianceToImport.ComplianceSupportingDocumentNumber = complianceInfo.ComplianceSupportingDocumentNumber;
				}
				else
				{
					complianceToImport.CreateComplianceDocumentRecordOnPosting = false;
				}
			}
		}

#if DEBUG
		public IEnumerable<Job> LoadJobs_ForTestOnly(BusinessObjectFactory factory, IEnumerable<ZString> jobNumbers, bool fetchFromCacheOnly) => LoadJobs(factory, jobNumbers, fetchFromCacheOnly);
#endif

		#region Helper methods

		ZGuid GetCurrencyPK(BusinessObjectFactory factory, ZString currencyCode, IValueObjectImportContext context)
		{
			if (currencyCode.IsEmpty)
			{
				return ZGuid.Empty;
			}

			RefCurrency currency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
			if (currency == null)
			{
				context.AddError(Res.GetString("9ecb858a-1d8a-4084-b2ae-feafa77c61af", "Could not find currency {0}", currencyCode));
				return ZGuid.Empty;
			}

			return currency.PK;
		}

		ZString ExistingOrEmptyCurrencyCode(BusinessObjectFactory factory, ZString currencyCode, IValueObjectImportContext context)
		{
			if (currencyCode.IsEmpty)
			{
				return ZString.Empty;
			}

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			if (currency == null)
			{
				context.AddError(Res.GetString("9ecb858a-1d8a-4084-b2ae-feafa77c61af", "Could not find currency {0}", currencyCode));
				return ZString.Empty;
			}

			return currencyCode;
		}

		ZGuid GetTaxMessagePK(BusinessObjectFactory factory, ZString taxMessageCode, IValueObjectImportContext context)
		{
			if (taxMessageCode.IsEmpty)
			{
				return ZGuid.Empty;
			}
			var taxMessageQuery = new ZQuery(AccInvMsgSchema.A9_Code, taxMessageCode);
			taxMessageQuery.AddToFilter(AccInvMsgSchema.A9_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var taxMessage = factory.LoadTop1<AccInvMsg>(taxMessageQuery);
			if (taxMessage == null)
			{
				context.AddError(Res.GetString("bf6f59d3-fa4b-4e75-80f2-ecdf05d6b5e2", "Could not find Tax Message {0}", taxMessageCode));
				return ZGuid.Empty;
			}

			return taxMessage.PK;
		}

		ZGuid GetBranchPK(BusinessObjectFactory factory, ZString branchCode, IValueObjectImportContext context)
		{
			if (branchCode.IsEmpty)
			{
				return ZGuid.Empty;
			}

			GlbBranch branch = factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
			if (branch == null)
			{
				context.AddError(Res.GetString("e9a9a440-e3e5-49b9-9810-cea0cea9962d", "Could not find branch {0}", branchCode));
				return ZGuid.Empty;
			}

			return branch.PK;
		}

		ZGuid GetDepartmentPK(BusinessObjectFactory factory, ZString departmentCode, IValueObjectImportContext context)
		{
			if (departmentCode.IsEmpty)
			{
				return ZGuid.Empty;
			}

			GlbDepartment department = factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode);
			if (department == null)
			{
				context.AddError(Res.GetString("61903de5-c3aa-4872-a432-235a7761c8d6", "Could not find department {0}", departmentCode));
				return ZGuid.Empty;
			}

			return department.PK;
		}

		IJobCostingPlugIn GetConsol(BusinessObjectFactory factory, ZString uniqueConsignRef, IValueObjectImportContext context)
		{
			if (uniqueConsignRef.IsEmpty)
			{
				return null;
			}
			IJobCostingPlugIn consol = GenericConsol.GetIJobCostingPlugInByPrimaryCode(factory, uniqueConsignRef);
			if (consol == null)
			{
				context.AddError(Res.GetString("52bef2d4-85f4-4f88-88b6-f97b78ad7c4e", "Could not find consol {0}", uniqueConsignRef));
			}
			return consol;
		}

		ZGuid GetChargeCodePK(BusinessObjectFactory factory, ZString code, IValueObjectImportContext context)
		{
			if (code.IsEmpty)
			{
				return ZGuid.Empty;
			}

			ZQuery chargeCodeQuery = new ZQuery();
			chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, code);
			chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			AccChargeCode chargeCode = factory.LoadTop1<AccChargeCode>(chargeCodeQuery);
			if (chargeCode == null)
			{
				context.AddError(Res.GetString("0349bee3-bb91-4324-a5d2-322553f40ff5", "Could not find charge code {0}", code));
				return ZGuid.Empty;
			}

			return chargeCode.PK;
		}

		ZGuid GetGenericChargePK(BusinessObjectFactory factory, ZString code, IValueObjectImportContext context)
		{
			if (code.IsEmpty)
			{
				return ZGuid.Empty;
			}

			ZQuery genericChargeQuery = new ZQuery();
			genericChargeQuery.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
			genericChargeQuery.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_GC, null);
			genericChargeQuery.AddToFilter(ViewGenericChargeSchema.VC_Code, code);
			GenericCharge genericCharge = factory.LoadTop1<GenericCharge>(genericChargeQuery);
			if (genericCharge == null)
			{
				context.AddError(Res.GetString("14e34c87-fcbe-4ac2-b566-2e3e23aa2590", "Could not find generic charge {0}", code));
				return ZGuid.Empty;
			}

			return genericCharge.PK;
		}

		ZGuid GetJobPK(ZString jobNumber, IValueObjectImportContext context, IReadOnlyDictionary<ZString, ZGuid> jobNumberToPKMapping)
		{
			if (jobNumber.IsEmpty)
			{
				return ZGuid.Empty;
			}

			ZGuid result;
			if (jobNumberToPKMapping.TryGetValue(jobNumber, out result))
			{
				return result;
			}
			else
			{
				context.AddError(Res.GetString("a12c72d8-ae60-410b-a54c-8b47a124f20e", "Could not find job {0}", jobNumber));
				return ZGuid.Empty;
			}
		}

		ZGuid GetTaxRatePK(BusinessObjectFactory factory, ZString taxRateId, IValueObjectImportContext context)
		{
			if (taxRateId.IsEmpty)
			{
				return ZGuid.Empty;
			}

			ZQuery taxRateQuery = new ZQuery();
			taxRateQuery.AddToFilter(AccTaxRateSchema.AT_Code, taxRateId);
			taxRateQuery.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AccTaxRate taxRate = factory.LoadTop1<AccTaxRate>(taxRateQuery);
			if (taxRate == null)
			{
				context.AddError(Res.GetString("fa929885-ba4c-401b-8bc5-1245390a277b", "Could not find tax rate {0}", taxRateId));
				return ZGuid.Empty;
			}

			return taxRate.PK;
		}

		ZGuid GetWHTTaxPK(BusinessObjectFactory factory, ZString wHTTaxId, IValueObjectImportContext context)
		{
			if (wHTTaxId.IsEmpty)
			{
				return ZGuid.Empty;
			}

			var whtTaxQuery = new ZQuery();
			whtTaxQuery.AddToFilter(AccWithholdingSchema.AW_Code, wHTTaxId);
			whtTaxQuery.AddToFilter(AccWithholdingSchema.AW_GC, GlbCompany.CurrentCompany.PK.ToGuid());

			var whtTax = factory.LoadTop1<AccWithholding>(whtTaxQuery);
			if (whtTax == null)
			{
				context.AddError(Res.GetString("927EA6D5-4CFF-44DE-B166-31F6D51764B1", "Could not find Withholding tax {0}", wHTTaxId));
				return ZGuid.Empty;
			}

			return whtTax.PK;
		}

		ZGuid GetComplianceDocumentOrganizationPK(BusinessObjectFactory factory, ZString complianceDocumentOrganization, IValueObjectImportContext context)
		{
			ZGuid organizationPK;
			if (ZGuid.TryParse(complianceDocumentOrganization, out organizationPK) && organizationPK.IsValid)
			{
				var organization = factory.Load<OrgHeader>(organizationPK);
				if (organization == null)
				{
					context.AddError(Res.GetString("8450E9C9-D65D-4EE8-884D-245EC346CB2D", "Could not find compliance document organization {0}", organization));
					return ZGuid.Empty;
				}
				else
				{
					return organization.PK;
				}
			}
			return ZGuid.Empty;
		}

		ZGuid GetRequestedGLAccountPK(BusinessObjectFactory factory, ZString requestedGLAccountPK, IValueObjectImportContext context, string errorMsg)
		{
			ZGuid glAccountPK;
			if (ZGuid.TryParse(requestedGLAccountPK, out glAccountPK) && glAccountPK.IsValid)
			{
				var glAccount = factory.Load<AccGLHeader>(glAccountPK);
				if (glAccount == null)
				{
					context.AddError(Res.GetString("F8BA9EF9-622C-4658-86B7-DDE95B7C36BC", "Could not find GL Account. {0}", errorMsg));
				}
				else
				{
					return glAccount.PK;
				}
			}

			return ZGuid.Empty;
		}

		JobCharge GetCharge(BusinessObjectFactory factory, ZString chargePKString)
		{
			ZGuid chargePK;
			if (string.IsNullOrEmpty(chargePKString) || !ZGuid.TryParse(chargePKString, out chargePK))
			{
				return null;
			}

			JobCharge charge = factory.Load<JobCharge>(chargePK);
			return charge;
		}

		JobConsolCost GetConsolCost(BusinessObjectFactory factory, ZString consolCostString)
		{
			ZGuid consolCostPK;
			if (string.IsNullOrEmpty(consolCostString) || !ZGuid.TryParse(consolCostString, out consolCostPK))
			{
				return null;
			}

			return factory.Load<JobConsolCost>(consolCostPK);
		}

		AccTaxConfiguration GetTaxConfiguration(BusinessObjectFactory factory, ZString taxConfigurationPk, IValueObjectImportContext context, ZString errorMessage)
		{
			ZGuid resultPK;
			if (ZGuid.TryParse(taxConfigurationPk, out resultPK) && resultPK.IsValid)
			{
				var taxConfig = factory.Load<AccTaxConfiguration>(resultPK);
				if (taxConfig == null)
				{
					context.AddError(Res.GetString("51925F53-FFD8-4100-ADA7-B066E39D43FE", "Could not find tax configuration. {0}", errorMessage));
					return null;
				}
				else if (!taxConfig.ETC_IsActive)
				{
					context.AddError(Res.GetString("E6136162-C3B4-4128-BF9F-672ECEBDE09E", "Could not find active tax configuration. {0}, Tax configuration code = {1}", errorMessage, taxConfig.ETC_Code));
					return null;
				}
				else
				{
					return taxConfig;
				}
			}
			return null;
		}

		#endregion

		#endregion

		public override string RootCollectionElementName
		{
			get { return "IncompleteTransactions"; }
		}

		public override string RootElementName
		{
			get { return "IncompleteTransaction"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.IncompleteTransaction; }
		}

		protected override T FindBusinessObject(IncompleteTransactionHeader value, IValueObjectImportContext context)
		{
			return IncompleteTransaction;
		}

		T IncompleteTransaction { get; }

		ITaxProcessor TaxProcessor => ObjectFactory.Get<ITaxProcessor>();
	}
}
