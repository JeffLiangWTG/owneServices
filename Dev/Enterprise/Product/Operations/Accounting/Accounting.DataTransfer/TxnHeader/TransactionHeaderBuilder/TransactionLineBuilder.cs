using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public partial class TransactionLineBuilder
	{
		public TransactionLineBuilder(INotificationManager notifier, TransactionBuilderConfig config)
		{
			this.Notifier = notifier;
			this.Config = config;
		}

		readonly TransactionBuilderConfig Config;
		bool isChargeCodeOrGLAccountSet;
		bool isChargeCodeSetUsingIntercompanyChargeCodeMapping;

		public void AddTransactionLineToDirectReceiptPaymentBusinessObject(DirectTransactionHeaderBase header, Xsd.TxnLine xmlTransactionLine, IValueObjectImportContext context, ZString errorContext)
		{
			DependentTransactionLine transactionLine = header.Lines.AddNew();

			SetGLAccount(transactionLine, xmlTransactionLine, errorContext);
			SetBranch(transactionLine, xmlTransactionLine, errorContext);
			SetDepartment(transactionLine, xmlTransactionLine, errorContext);
			SetDescription(transactionLine, xmlTransactionLine, context, errorContext);
			SetOSExTaxAmount(transactionLine, xmlTransactionLine, context, errorContext);
			SetTaxRate(transactionLine, xmlTransactionLine, errorContext);
			SetTaxAmount(transactionLine, xmlTransactionLine);
			SetTaxMessage(transactionLine, xmlTransactionLine, errorContext);
			SetRecoverableGSTVATPercentage(transactionLine, xmlTransactionLine);

			transactionLine.Validation.ValidateAll();
		}

		public void AddTransactionLineToInvoiceBusinessObject(InvoicingBase invoice, Xsd.TxnLine xmlInvoiceLine, IValueObjectImportContext context, ZString errorContext)
		{
			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			try
			{
				SetJobAndChargeCodeOrGlAccount(xmlInvoiceLine, errorContext, invoiceLine);
				if (ShouldResetJobAndChargeCode(invoiceLine))
				{
					PrepareToResetJobAndChargeCode(xmlInvoiceLine);
					ResetJobAndChargeCode(invoiceLine, isChargeCodeSetUsingIntercompanyChargeCodeMapping);
					SetJobAndChargeCodeOrGlAccount(xmlInvoiceLine, errorContext, invoiceLine);
				}

				if (Config.SetBranch)
				{
					SetBranch(invoiceLine, xmlInvoiceLine, errorContext);
				}
				if (Config.SetDepartment)
				{
					SetDepartment(invoiceLine, xmlInvoiceLine, errorContext);
				}
			}
			finally
			{
				if (invoiceLine.IsValidationSuspended)
				{
					invoiceLine.ResumeValidation();
				}
				isChargeCodeOrGLAccountSet = false;
				isChargeCodeSetUsingIntercompanyChargeCodeMapping = false;
			}

			SetOSExTaxAmount(invoiceLine, xmlInvoiceLine, context, errorContext);
			SetTaxRate(invoiceLine, xmlInvoiceLine, errorContext);
			SetTaxAmount(invoiceLine, xmlInvoiceLine);
			SetTaxMessage(invoiceLine, xmlInvoiceLine, errorContext);
			SetRecoverableGSTVATPercentage(invoiceLine, xmlInvoiceLine);
			SetWhtRate(invoiceLine, xmlInvoiceLine, errorContext);
			//SetWHTAmount(InvoiceLine, XmlInvoiceLine); //HM: We do not allow to set WHT amount in GUI, so should not allow the value to be imported either. If we allow WHT amount to be edited then we should uncomment this too
			context.SetPropertyInfoValue(invoiceLine.AL_SequenceInfo, xmlInvoiceLine.Sequence, xmlInvoiceLine.SequenceSpecified, errorContext + Res.GetString("a39cacb7-d1a9-4836-9076-771f5f19b6a4", "Sequence"));
			SetDescription(invoiceLine, xmlInvoiceLine, context, errorContext);
			SetIsFinal(invoiceLine, xmlInvoiceLine);
			SetDescription(invoiceLine, xmlInvoiceLine, context, errorContext);

			if (!TransactionHeaderBuilder.IsTaxApplicable(invoiceLine.TransactionHeader) && (xmlInvoiceLine.OsTaxAmount.Value != 0m || xmlInvoiceLine.OSChargeTaxAmount.Value != 0m))
			{
				invoiceLine.MarkForWarningAsCreditorOrLoginCompanyIsNotTaxRegisteredForTaxedTransaction = true;
			}

			if (xmlInvoiceLine.SubAccounts.Count > 0)
			{
				SetMultipleSubAccounts(invoiceLine, xmlInvoiceLine);
			}
			else if (!xmlInvoiceLine.SubAccount.Code.IsEmpty)
			{
				ImportSubAccountForBackwardCompatibility(invoiceLine, xmlInvoiceLine.SubAccount.Type.Code, xmlInvoiceLine.SubAccount.Code);
			}

			SetRelatedJobAndTargetJobIds(invoiceLine, xmlInvoiceLine);

			invoiceLine.Validation.ValidateAll();
		}

		protected virtual void ResetJobAndChargeCode(InvoicingLineBase invoiceLine, bool shouldResetChargeCode)
		{
			invoiceLine.AL_JH = ZGuid.Empty;
			if (shouldResetChargeCode)
			{
				invoiceLine.AL_AC = ZGuid.Empty;
				invoiceLine.GenericCharge = ZGuid.Empty;
			}
		}

		protected virtual void PrepareToResetJobAndChargeCode(Xsd.TxnLine xmlInvoiceLine)
		{
		}

		protected virtual bool ShouldResetJobAndChargeCode(InvoicingLineBase invoiceLine) => false;

		void ImportSubAccountForBackwardCompatibility(DependentTransactionLine line, ZString subAccountType, ZString subAccountCode)
		{
			if (line.GLHeader != null)
			{
				var targetSubAccount = line.SubAccounts.Cast<TransactionLineSubAccount>().SingleOrDefault(x => x.AL1_SubClassParentTableCode == SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(subAccountType));
				if (targetSubAccount != null)
				{
					targetSubAccount.AL1_SubClassParentId = SubAccountHelper.GetSubAccountPKFromCode(line.Factory, subAccountType, subAccountCode);
				}
			}
		}

		void SetJobAndChargeCodeOrGlAccount(Xsd.TxnLine xmlInvoiceLine, ZString errorContext, InvoicingLineBase invoiceLine)
		{
			bool invoiceLinesHasJobSet = SetJobInformation(invoiceLine, xmlInvoiceLine, errorContext); // set job first because job's local client is required to calculate charge code
			if (!isChargeCodeOrGLAccountSet || isChargeCodeSetUsingIntercompanyChargeCodeMapping)
			{
				SetChargeCodeOrGlAccount(invoiceLine, xmlInvoiceLine, errorContext);
			}
			if (invoiceLinesHasJobSet && !invoiceLine.AL_JH.IsValid) // if charge code is invalid, job will be cleared in SetChargeCodeOrGlAccount, in this case, we set job again.
			{
				SetJobInformation(invoiceLine, xmlInvoiceLine, errorContext);
			}
		}

		void SetRelatedJobAndTargetJobIds(InvoicingLineBase invoiceLine, Xsd.TxnLine xmlInvoiceLine)
		{
			if (ZGuid.TryParse(xmlInvoiceLine.RelatedJobID, out ZGuid relatedJobID))
			{
				invoiceLine.RelatedJobFromIntercompanyInvoiceImport = (relatedJobID, xmlInvoiceLine.RelatedJobNumber);
			}

			if (ZGuid.TryParse(xmlInvoiceLine.TargetJobID, out ZGuid targetJobID))
			{
				invoiceLine.TargetJobIDFromIntercompanyInvoiceImport = targetJobID;
			}
		}

		void SetOSExTaxAmount(DependentTransactionLine transactionLine, Xsd.TxnLine xmlTransactionLine, IValueObjectImportContext context, ZString errorContext)
		{
			if (Config.UseForeignChargeAmountWhenPostingLocalCurrencyInvoices &&
				xmlTransactionLine.OSChargeAmount.IsSpecified &&
				transactionLine.TransactionHeader != null &&
				transactionLine.TransactionHeader.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
				)
			{
				context.SetPropertyInfoValue(transactionLine.AL_RX_NKTransactionCurrencyInfo, xmlTransactionLine.OSChargeAmount.CurrencyCode, ForeignKeyType.CurrencyNK, errorContext + Res.GetString("e5730b8f-94dc-44ec-a807-df97ba6b71c2", "Currency Code"));
				ZDecimal osExTaxChargeAmount = -TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionLine.Factory, xmlTransactionLine.OSChargeAmount, transactionLine.GetType());
				ZDecimal localExTaxAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionLine.Factory, xmlTransactionLine.LocalInvoiceAmtExclTax, transactionLine.GetType());
				transactionLine.AL_OSExTaxAmount = osExTaxChargeAmount;
				transactionLine.AL_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(localExTaxAmount, osExTaxChargeAmount);
			}
			else
			{
				var taxRateOverrideTransactionContextIsAll = transactionLine.TaxRateOverrideCalculator.IsUseTransactionContextAll;
				var osInvoiceAmtExclTax = taxRateOverrideTransactionContextIsAll && xmlTransactionLine.UseOriginalAmount ? xmlTransactionLine.OriginalOsInvoiceAmtExclTax : xmlTransactionLine.OsInvoiceAmtExclTax;
				var localInvoiceAmtExclTax = taxRateOverrideTransactionContextIsAll && xmlTransactionLine.UseOriginalAmount ? xmlTransactionLine.OriginalLocalInvoiceAmtExclTax : xmlTransactionLine.LocalInvoiceAmtExclTax;

				transactionLine.AL_OSExTaxAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionLine.Factory, osInvoiceAmtExclTax, transactionLine.GetType());
				if (xmlTransactionLine.OverrideSystemExchangeRate &&
					osInvoiceAmtExclTax.IsSpecified &&
					osInvoiceAmtExclTax.CurrencyCode != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					using (var osAmountRecalculationSuspender = new TransactionLine.OSAmountRecalculationSuspender(transactionLine))
					{
						transactionLine.AL_LocalExTaxAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(transactionLine.Factory, localInvoiceAmtExclTax, transactionLine.GetType());
					}
					transactionLine.CalculateHighPrecisionExchangeRate();
				}
			}

			if (transactionLine.AL_OSExTaxAmount != 0m && transactionLine.ChargeCode != null && transactionLine.ChargeCode.IsComment)
			{
				Notifier.AddErrorToNotifications(AccountingConstants.AmountCannotBeSetErrorMessage);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1135: DoNotUseCountrySpecificBusinessRule", Justification = "Testing")]
		void SetTaxRate(DependentTransactionLine transactionLine, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			if (!TransactionHeaderBuilder.IsTaxApplicable(transactionLine.TransactionHeader))
			{
				return;
			}

			var taxRateOverrideCalculator = transactionLine.TaxRateOverrideCalculator;

			if (taxRateOverrideCalculator.IsUseCopyARAmount)
			{
				ConvertToSameTaxID(transactionLine, xmlInvoiceLine, errorContext);
				return;
			}
			else if (taxRateOverrideCalculator.IsUseTaxRateOverride)
			{
				ConvertToTaxRateOverrideTaxID(transactionLine, taxRateOverrideCalculator.CachedTaxRateOverride.TaxRate);
				return;
			}

#if DEBUG
			transactionLine.IsUseDefaultTaxOverrideLogic_ForTestOnly = true;
#endif
			if (Config.CrossLedgerImport && transactionLine.TransactionHeader.Header != null && transactionLine.TransactionHeader.Header.CountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				ConvertToNotReportableTaxID(transactionLine);
			}
			else
			{
				ConvertToSameTaxID(transactionLine, xmlInvoiceLine, errorContext);
			}
		}

		void ConvertToTaxRateOverrideTaxID(DependentTransactionLine transactionLine, AccTaxRate taxRate)
		{
			if (taxRate != null)
			{
				transactionLine.AL_AT = taxRate.PK;
			}
		}

		void ConvertToSameTaxID(DependentTransactionLine transactionLine, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			if (!xmlInvoiceLine.TaxCode.IsEmpty)
			{
				var taxRateFilter = new ZQuery(AccTaxRateSchema.AT_Code, xmlInvoiceLine.TaxCode);
				taxRateFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var taxRate = transactionLine.Factory.LoadTop1<AccTaxRate>(taxRateFilter);

				var numberOfTaxRates = transactionLine.Factory.GetDatabaseCount(typeof(AccTaxRate), taxRateFilter);
				Notifier.ReportNoBizObjsFoundError(Res.GetString("815b30f6-9331-4795-8e30-438313ac8915", "Tax Rate"), xmlInvoiceLine.TaxCode, numberOfTaxRates, errorContext);

				if (taxRate != null)
				{
					transactionLine.AL_AT = taxRate.PK;
				}
			}
		}

		public static void ConvertToNotReportableTaxID(DependentTransactionLine transactionLine)
		{
			if (transactionLine.TaxRate != null && transactionLine.AL_TaxRateCalc != 0)
			{
				AccTaxRate notReportRate = AccTaxRate.GetNOTREPORTTaxID(transactionLine.Factory, GlbCompany.CurrentCompany);

				if (notReportRate != null)
				{
					transactionLine.AL_AT = notReportRate.PK;
				}
			}
		}

		void SetTaxMessage(DependentTransactionLine transactionLine, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			if (!TransactionHeaderBuilder.IsTaxApplicable(transactionLine.TransactionHeader))
			{
				return;
			}

			var taxRateOverrideCalculator = transactionLine.TaxRateOverrideCalculator;

			if (taxRateOverrideCalculator.IsUseTaxRateOverride && taxRateOverrideCalculator.CachedTaxRateOverride != null && transactionLine.AL_AT == taxRateOverrideCalculator.CachedTaxRateOverride.AO_AT)
			{
				transactionLine.AL_A9_VATClass = taxRateOverrideCalculator.CachedTaxRateOverride.AO_A9_DefaultVATClass;
				return;
			}

			if (!xmlInvoiceLine.TaxCode.IsEmpty && !xmlInvoiceLine.TaxMsgCode.IsEmpty)
			{
				var taxMessageFilter = new ZQuery(AccInvMsgSchema.A9_Code, xmlInvoiceLine.TaxMsgCode);
				taxMessageFilter.AddToFilter(AccInvMsgSchema.A9_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var taxMessage = transactionLine.Factory.LoadTop1<AccInvMsg>(taxMessageFilter);

				if (taxMessage != null)
				{
					transactionLine.AL_A9_VATClass = taxMessage.PK;
				}
				else
				{
					Notifier.ReportNoBizObjsFoundError(Res.GetString("55a65942-2ad0-4c99-a8c5-b8639f33866c", "Tax Message Code"), xmlInvoiceLine.TaxMsgCode, 0, errorContext);
				}
			}
		}

		void SetTaxAmount(DependentTransactionLine invoiceLine, Xsd.TxnLine xmlInvoiceLine)
		{
			ZDecimal taxAmount;

			if (invoiceLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Cost || invoiceLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.UnapprovedCost || invoiceLine.AL_LineType == ZArchitecture.Core.TransactionTypes.DirectPayment || invoiceLine.AL_LineType == ZArchitecture.Core.TransactionTypes.DirectReceipt)
			{
				var taxRateOverrideDefaultingRuleIsSum = invoiceLine.TaxRateOverrideCalculator.IsUseSumARAmount;
				var taxRateOverrideTransactionContextIsAll = invoiceLine.TaxRateOverrideCalculator.IsUseTransactionContextAll;

				if (Config.UseForeignChargeAmountWhenPostingLocalCurrencyInvoices &&
					invoiceLine.TransactionHeader != null &&
					invoiceLine.TransactionHeader.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency &&
					invoiceLine.AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					taxAmount = -TxnHeaderMapper.GetDecimalFromXmlFinancialValue(invoiceLine.Factory, xmlInvoiceLine.OSChargeTaxAmount, invoiceLine.GetType());
				}
				else
				{
					var osTaxAmount = taxRateOverrideTransactionContextIsAll && xmlInvoiceLine.UseOriginalAmount ? xmlInvoiceLine.OriginalOsTaxAmount : xmlInvoiceLine.OsTaxAmount;
					taxAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(invoiceLine.Factory, osTaxAmount, invoiceLine.GetType());
				}

				if (TransactionHeaderBuilder.IsTaxApplicable(invoiceLine.TransactionHeader) && !taxRateOverrideDefaultingRuleIsSum)
				{
					invoiceLine.AL_OSTaxAmount = taxAmount;
				}
				else
				{
					invoiceLine.AL_OSExTaxAmount += taxAmount;
				}
			}

			if (invoiceLine.AL_OSTaxAmount != 0)
			{
				if (Config.RunExtraValidation)
				{
					if (invoiceLine.TaxRate == null)
					{
						Notifier.AddErrorToNotifications(Res.GetString("373b605e-bb1e-4ecf-96d8-9fb01ba3d45b", "Line Tax Amount cannot be set if there is no Tax Code"));
					}
					else if (invoiceLine.AL_TaxRateCalc == 0)
					{
						Notifier.AddErrorToNotifications(Res.GetString("ce98b9fb-2c57-4c6a-9804-43c66bdc755d", "Line Tax Amount cannot be set if Tax Rate is zero"));
					}
				}
			}
		}

		void SetRecoverableGSTVATPercentage(DependentTransactionLine line, Xsd.TxnLine xmlLine)
		{
			if (xmlLine.RecoverableGSTVATPercentageSpecified && line.SupportsInputTaxRecoverable)
			{
				line.AL_Calc_InputGSTVATRecoverablePercentage = xmlLine.RecoverableGSTVATPercentage;
			}
		}

		void SetDescription(DependentTransactionLine transactionLine, Xsd.TxnLine xmlTransactionLine, IValueObjectImportContext context, ZString errorContext)
		{
			if (!xmlTransactionLine.Description.IsEmpty && (Config.SetLineDescription || (transactionLine.AL_AC.IsEmpty && transactionLine.AL_AG.IsEmpty) || xmlTransactionLine.IsSplitLine))
			{
				context.SetPropertyInfoValue(transactionLine.AL_DescInfo, xmlTransactionLine.Description, xmlTransactionLine.DescriptionSpecified, errorContext + Res.GetString("5a00ec2e-124d-406b-ad71-74da437ad89c", "Description"));
			}
			var electronicProcessingChargeCode = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value;
			if (Config.UseChargeDescAsLineDesc && electronicProcessingChargeCode != null && transactionLine.AL_AC == transactionLine.Factory.Load<AccChargeCode>(electronicProcessingChargeCode)?.ChildChargeCodes.FirstOrDefault(x => x.AC_GC == Env.CurrentCompanyPK)?.PK)
			{
				var query = new ZQuery(JobChargeSchema.JR_AL_ARLine, new Guid(xmlTransactionLine.TxnLineGUID));
				var lineCharge = transactionLine.Factory.LoadTop1<Charge>(query);
				transactionLine.AL_Desc = lineCharge.JR_Desc;
			}
		}

		void SetMultipleSubAccounts(DependentTransactionLine invoiceLine, Xsd.TxnLine xmlInvoiceLine)
		{
			SubAccountHelper.CreateSubAccountFromXml(invoiceLine, xmlInvoiceLine.SubAccounts);
		}

		void SetChargeCodeOrGlAccount(InvoicingLineBase invoiceLine, Xsd.TxnLine xmlInvoiceLine, ZString errorContext)
		{
			if (!xmlInvoiceLine.ChargeCode.IsEmpty)
			{
				SetChargeCode(invoiceLine, xmlInvoiceLine, Res.GetString("f887dab5-1428-49ae-ad0c-5509ff80da0d", "{0} Line", errorContext) + "  ");
			}
			else
			{
				SetGLAccount(invoiceLine, xmlInvoiceLine, errorContext);
			}
			isChargeCodeOrGLAccountSet = true;
		}

		bool SetJobInformation(InvoicingLineBase invoiceLine, Xsd.TxnLine xmlInvoiceLine, ZString errorContext)
		{
			bool result = false;
			if (invoiceLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Cost || invoiceLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.UnapprovedCost)
			{
				result = SetGenericJob(invoiceLine, xmlInvoiceLine, errorContext);
			}
			else if (!xmlInvoiceLine.ConsolOrJobNo.IsEmpty || !xmlInvoiceLine.HouseBIllNo.IsEmpty) // Can only be AR or AP
			{
				Notifier.AddErrorToNotifications(Res.GetString("f7ddac94-2fec-4dd5-baf2-0f88447b3fe5", "Job Related AR Transactions cannot be imported"));
			}
			return result;
		}

		void SetIsFinal(InvoicingLineBase invoiceLine, Xsd.TxnLine xmlInvoiceLine)
		{
			if (xmlInvoiceLine.IsFinalChargeSpecified)
			{
				InvoicingLineBase aPLine = invoiceLine;
				aPLine.AL_IsFinalCharge = xmlInvoiceLine.IsFinalCharge;
			}
		}

		GenericJob GetGenericJob(BusinessObjectFactory factory, Xsd.TxnLine xmlInvoiceLine)
		{
			GenericJob job = null;

			if (Config.UseConsolOrJobNumberToMatchJob)
			{
				if (!xmlInvoiceLine.ConsolOrJobNo.IsEmpty)
				{
					ZQuery findByJobNumberFilter = GetGenericJobBaseFilter();
					findByJobNumberFilter.AddToFilter(ViewGenericJobSchema.VJ_JobNumber, xmlInvoiceLine.ConsolOrJobNo);

					if (xmlInvoiceLine.ConsolOrJobTypeSpecified)
					{
						ZString jobTypeString = TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(xmlInvoiceLine.ConsolOrJobType, Notifier.NotificationSubscriber);
						findByJobNumberFilter.AddToFilter(JoinCondition.And, ViewGenericJobSchema.VJ_JobType, SQLComparisonOperator.Equal, jobTypeString);
					}

					var jobFound = factory.Load<GenericJob>(findByJobNumberFilter);
					if (jobFound.Length == 1)
					{
						job = jobFound[0];
					}
					else if (jobFound.Length > 1)
					{
						ZString errorMessage = Res.GetString("6a3e9672-b1bd-4df7-a67d-1754171378f9", "More than one Job was found with the following Job Number: {0}.", xmlInvoiceLine.ConsolOrJobNo) + "\r\n";
						Notifier.AddWarningToNotifications(errorMessage);
					}
				}
			}

			if (job == null && !xmlInvoiceLine.HouseBIllNo.IsEmpty)
			{
				ZQuery findByHouseBillNumberFilter = GetGenericJobBaseFilter();
				findByHouseBillNumberFilter.AddToFilter(ViewGenericJobSchema.VJ_HouseBillNumber, xmlInvoiceLine.HouseBIllNo);

				if (xmlInvoiceLine.ConsolOrJobTypeSpecified)
				{
					ZString jobTypeString = TxnHeaderMapper.GetCodeForJobInvoicingConsumerType(xmlInvoiceLine.ConsolOrJobType, Notifier.NotificationSubscriber);
					findByHouseBillNumberFilter.AddToFilter(JoinCondition.And, ViewGenericJobSchema.VJ_JobType, SQLComparisonOperator.Equal, jobTypeString);
				}

				var jobFound = factory.Load<GenericJob>(findByHouseBillNumberFilter);
				if (jobFound.Length == 1)
				{
					job = jobFound[0];
				}
				else if (jobFound.Length > 1)
				{
					ZString errorMessage = Res.GetString("b01a20f2-d7d6-44b7-9d6a-fbdf2e3bb49f", "More than one Job was found with the following House Bill Number: {0}.", xmlInvoiceLine.HouseBIllNo) + "\r\n";
					Notifier.AddWarningToNotifications(errorMessage);
				}
			}

			return job;
		}

		ZQuery GetGenericJobBaseFilter()
		{
			var query = new ZQuery(ViewGenericJobSchema.VJ_CompanyPK, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(JoinCondition.Or, ViewGenericJobSchema.VJ_CompanyPK, null);
			query.MaximumRows = 2;
			return query;
		}

		void SetBranch(DependentTransactionLine invoiceLine, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			// In Cross-Ledger import, Branch set from Job takes precedence over imported Branch Code
			if (Config.CrossLedgerImport)
			{
				return;
			}

			if (!xmlInvoiceLine.Branch.IsEmpty)
			{
				GlbBranch branch = BusinessObjectRetriever.GetBranchFromBranchCode(invoiceLine.Factory, xmlInvoiceLine.Branch);

				if (branch != null)
				{
					invoiceLine.AL_GB = branch.PK;
				}
				else
				{
					Notifier.ReportNoBizObjsFoundError(Res.GetString("2110efa3-d5c8-4294-a41e-69f894f387b3", "Branch"), xmlInvoiceLine.Branch, 0, errorContext);
				}
			}
		}

		void SetDepartment(DependentTransactionLine invoiceLine, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			// In Cross-Ledger import, Department set from Job takes precedence over imported Department Code
			if (Config.CrossLedgerImport && invoiceLine.AL_JH.IsValid && invoiceLine.AL_GE.IsValid)
			{
				return;
			}

			if (!xmlInvoiceLine.Department.IsEmpty)
			{
				GlbDepartment department = BusinessObjectRetriever.GetDepartmentFromDepartmentCode(invoiceLine.Factory, xmlInvoiceLine.Department);

				if (department != null)
				{
					invoiceLine.AL_GE = department.PK;
				}
				else
				{
					Notifier.ReportNoBizObjsFoundError(Res.GetString("5d5cc60c-3ec2-4b41-b4e8-569200f628f4", "Department"), xmlInvoiceLine.Department, 0, errorContext);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		protected virtual void SetChargeCode(InvoicingLineBase invoiceLine, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			var isLocalClientUsedToMapChargeCode = false;

			if (!xmlInvoiceLine.ChargeCode.IsEmpty)
			{
				var org = GetOrganisationForChargeCodeMapping(invoiceLine);
				if (org != null)
				{
					var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_GC);
					branchSubQuery.AddToFilter(GlbBranchSchema.GB_OH_OrgProxy, org.PK);

					var arCompanyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
					arCompanyQuery.AddToFilter(GlbCompanySchema.GC_OH_OrgProxy, org.PK);
					arCompanyQuery.AddSubQuery(GlbCompanySchema.PK, branchSubQuery, JoinCondition.Or);

					var arCompany = invoiceLine.Factory.LoadTop1<GlbCompany>(arCompanyQuery);
					var genericChargeCodeQuery = new ZDBOnlyQuery(typeof(GenericCharge));

					if (arCompany != null)
					{
						var localClientPK = ZGuid.Empty;
						if (invoiceLine.Job != null && invoiceLine.Job.LocalZAddressWithContact != null && invoiceLine.Job.LocalZAddressWithContact.OrgHeader != null)
						{
							localClientPK = invoiceLine.Job.LocalZAddressWithContact.OrgHeader.PK;
						}
						isLocalClientUsedToMapChargeCode = true;

						var query = string.Format(@"{0} IN (SELECT TOP 1 VC_PK
FROM dbo.ViewGenericCharge JOIN dbo.AccChargeCode DestinationChargeCode ON VC_PK = DestinationChargeCode.AC_PK
JOIN dbo.AccGlobalChargeCodeMapPivot APPivot ON APPivot.YP_TYPE = '{1}' AND APPivot.YP_AC = DestinationChargeCode.AC_PK
JOIN dbo.AccGlobalChargeCodeMap GlobalMap ON GlobalMap.YG_PK = APPivot.YP_YG
JOIN dbo.AccGlobalChargeCodeMapPivot ARPivot ON ARPivot.YP_YG = GlobalMap.YG_PK 
JOIN dbo.AccChargeCode SourceChargeCode ON ARPivot.YP_TYPE = '{2}' AND ARPivot.YP_AC = SourceChargeCode.AC_PK
WHERE 
	SourceChargeCode.AC_Code = @ARChargeCode
	AND SourceChargeCode.AC_GC = @ARCompany
	AND GlobalMap.YG_OH IS NULL
    AND GlobalMap.YG_IsActive = 1
	AND (APPivot.YP_OH_LocalClientOverride IS NULL OR APPivot.YP_OH_LocalClientOverride = @jobLocalClient)
	AND DestinationChargeCode.AC_GC = @currentCompanyPK
ORDER BY APPivot.YP_OH_LocalClientOverride DESC)
", ViewGenericChargeSchema.PK.Name, ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.LedgerTypes.AccountsReceivable);

						var parameters = new ZSqlParameterCollection();
						parameters.Add("@ARChargeCode", GetXMLChargeCodeValue(xmlInvoiceLine), AccChargeCodeSchema.AC_Code);
						parameters.Add("@ARCompany", arCompany.PK, GlbCompanySchema.PK);
						parameters.Add("@jobLocalClient", localClientPK, OrgHeaderSchema.PK);
						parameters.Add("@currentCompanyPK", GlbCompany.CurrentCompany.PK, GlbCompanySchema.PK);
						genericChargeCodeQuery.AddFilterAndZSQLParameterCollection(query, parameters);
					}
					else
					{
						var globalChargeCodeQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMap), AccGlobalChargeCodeMapSchema.PK);
						globalChargeCodeQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_OH, org.PK);
						globalChargeCodeQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_Code, GetXMLChargeCodeValue(xmlInvoiceLine));
						globalChargeCodeQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_IsActive, true);

						var pivotQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMapPivot), AccGlobalChargeCodeMapPivotSchema.YP_AC);
						pivotQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_TYPE, ZArchitecture.Core.LedgerTypes.AccountsPayable);
						pivotQuery.AddSubQuery(AccGlobalChargeCodeMapPivotSchema.YP_YG, globalChargeCodeQuery, JoinCondition.And);

						var chargeCodeQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), AccChargeCodeSchema.AC_Code);
						chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
						chargeCodeQuery.AddSubQuery(AccChargeCodeSchema.PK, pivotQuery, JoinCondition.And);

						genericChargeCodeQuery.AddSubQuery(ViewGenericChargeSchema.VC_Code, chargeCodeQuery, JoinCondition.And);
					}

					genericChargeCodeQuery.AddToFilter(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
					genericChargeCodeQuery.AddToFilter(ViewGenericChargeSchema.VC_IsGLAccount, false);
					var genericChargeCode = invoiceLine.Factory.LoadTop1<GenericCharge>(genericChargeCodeQuery);

					if (genericChargeCode != null)
					{
						invoiceLine.GenericCharge = genericChargeCode.PK;
						invoiceLine.AL_Desc = genericChargeCode.VC_Description;
						Config.UseChargeDescAsLineDesc = false;
						isChargeCodeSetUsingIntercompanyChargeCodeMapping = isLocalClientUsedToMapChargeCode;
					}
				}
				if (invoiceLine.GenericCharge.IsEmpty)
				{
					var patternMatchFilter = new ZQuery();
					if (org != null)
					{
						patternMatchFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, org.PK);
						patternMatchFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.ChargeCodes);
						patternMatchFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, GetXMLChargeCodeValue(xmlInvoiceLine));
					}
					else
					{
						patternMatchFilter.IsNoResultQuery = true;
					}

					var matches = invoiceLine.Factory.Load<OrgPatternMatchOverride>(patternMatchFilter);
					var localCodes = new List<ZString>();
					foreach (var match in matches)
					{
						if (!localCodes.Contains(match.OO_LocalCode))
						{
							localCodes.Add(match.OO_LocalCode);
						}
					}

					var chargecodeFilter = new ZQuery(AccChargeCodeSchema.AC_Code, localCodes.ToArray());
					chargecodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
					var patternMatchCode = invoiceLine.Factory.LoadTop1<AccChargeCode>(chargecodeFilter);

					if (patternMatchCode != null)
					{
						invoiceLine.GenericCharge = patternMatchCode.PK;
						invoiceLine.AL_Desc = patternMatchCode.AC_Desc;
					}
					else
					{
						var chargeCodeFilter = new ZQuery(ViewGenericChargeSchema.VC_Code, xmlInvoiceLine.ChargeCode);
						chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
						chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_IsGLAccount, ZBool.False);

						var chargeCode = invoiceLine.Factory.LoadTop1<GenericCharge>(chargeCodeFilter);

						if (chargeCode != null)
						{
							invoiceLine.GenericCharge = chargeCode.PK;
							invoiceLine.AL_Desc = chargeCode.VC_Description;
						}
						else
						{
							Notifier.ReportNoBizObjsFoundWarning(chargeCode, Res.GetString("3ef2208d-9c90-472c-b470-fdad079a2427", "Charge Code"), xmlInvoiceLine.ChargeCode, errorContext);
							ClearJobAndChargeCodeAndSetGLToDefaultClearingAccount(invoiceLine, xmlInvoiceLine, errorContext);
						}
					}
				}
			}
		}

		protected virtual ZString GetXMLChargeCodeValue(Xsd.TxnLine xmlInvoiceLine)
		{
			return xmlInvoiceLine.ChargeCode;
		}

		protected virtual OrgHeader GetOrganisationForChargeCodeMapping(InvoicingLineBase invoiceLine)
		{
			return invoiceLine.InvoiceBase.Header;
		}

		void SetGLAccount(DependentTransactionLine transactionLine, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			if (!xmlInvoiceLine.GLAccount.IsEmpty)
			{
				AccGLHeader gLHeader = transactionLine.Factory.LoadTop1<AccGLHeader>(BusinessObjectRetriever.GLHeaderFilter(xmlInvoiceLine.GLAccount, true));
				InvoicingLineBase invoiceLine = transactionLine as InvoicingLineBase;

				if (gLHeader == null)
				{
					AccGLHeader gLHeaderThatCantBeSelected = transactionLine.Factory.LoadTop1<AccGLHeader>(BusinessObjectRetriever.GLHeaderFilter(xmlInvoiceLine.GLAccount, false));

					if (gLHeaderThatCantBeSelected != null)
					{
						string errorString = Res.GetString("031ae73a-c1f9-431e-ac53-44d259fbe4b3", "The following GL Account cannot be used in this transaction: {0}", xmlInvoiceLine.GLAccount) + "\r\n";

						if (gLHeaderThatCantBeSelected.AG_DisallowDirectPosting)
						{
							errorString += Res.GetString("076763bb-8b43-4c31-bfc3-d72d8ee0d928", "The 'Disallow Direct Posting' flag is ticked on this GL Account. Please check the setup of the account.") + "\r\n";
						}

						if (gLHeaderThatCantBeSelected.AG_AccountType != Core.Constants.AccountType.ProfitAndLossAccount &&
							gLHeaderThatCantBeSelected.AG_AccountType != Core.Constants.AccountType.BalanceSheetAccount)
						{
							errorString += Res.GetString("c1718302-9197-4cd3-b7df-4d6ac52be5aa", "Only 'Profit & Loss' or 'Balance Sheet' GL Account types may be used") + "\r\n";
						}

						Notifier.AddWarningToNotifications(errorContext + errorString);
					}
					else
					{
						Notifier.ReportNoBizObjsFoundWarning(gLHeader, Res.GetString("1f5a8294-6855-47c6-a263-0a85088dab83", "GL Account"), xmlInvoiceLine.GLAccount, errorContext);
					}

					if (invoiceLine != null)
					{
						ClearJobAndChargeCodeAndSetGLToDefaultClearingAccount(invoiceLine, xmlInvoiceLine, errorContext);
					}
				}
				else
				{
					if (invoiceLine != null)
					{
						invoiceLine.GenericCharge = gLHeader.PK;
						transactionLine.AL_Desc = gLHeader.AG_DescriptionMultilingual;
					}
					else
					{
						transactionLine.AL_AG = gLHeader.PK;
					}
				}
			}
		}

		bool SetGenericJob(InvoicingLineBase invoiceLine, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			if (xmlInvoiceLine.ConsolOrJobNo.IsEmpty && xmlInvoiceLine.HouseBIllNo.IsEmpty)
			{
				return false;
			}

			var genericJob = GetGenericJob(invoiceLine.Factory, xmlInvoiceLine);
			if (genericJob == null)
			{
				var bizObjDescription = !xmlInvoiceLine.ConsolOrJobNo.IsEmpty
					? Res.GetString("3b78e24a-3d92-4a80-b84d-962839dd2a6b", "Job")
					: Res.GetString("433dfc06-8200-4fe0-ac5b-938ee2e5f60f", "Job (Searching by House Bill Number)");
				var valueForDisplay = !xmlInvoiceLine.ConsolOrJobNo.IsEmpty
					? xmlInvoiceLine.ConsolOrJobNo
					: xmlInvoiceLine.HouseBIllNo;

				Notifier.ReportNoBizObjsFoundWarning(null, bizObjDescription, valueForDisplay, errorContext);
				ClearJobAndChargeCodeAndSetGLToDefaultClearingAccount(invoiceLine, xmlInvoiceLine, errorContext);

				return false;
			}

			var consumer = genericJob.Consumer;
			var loader = new JobHeader.Loader(consumer);
			var job = loader.TryLoadOrCreateWithMutex();

			if (job == null)
			{
				string jobCreationError = loader.GetJobCreationError();
				Notifier.AddErrorToNotifications(jobCreationError);
				return false;
			}

			if (!invoiceLine.Factory.HasContext(BusinessContext.IntercompanyInvoiceAutoImport))
			{
				if (job.JH_GB.IsEmpty)
				{
					job.JH_GB = GlbBranch.CurrentBranch.PK;
				}

				if (job.JH_GE.IsEmpty)
				{
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				}
			}

			job.RunPreSaveValidation();
			invoiceLine.AL_JH = job.PK;
			return true;
		}

		void ClearJobAndChargeCodeAndSetGLToDefaultClearingAccount(InvoicingLineBase invoiceLine, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			if (Config.AllowResetChargeCodeAndJobOnError)
			{
				if (invoiceLine.GenericCharge != (Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
				{
					using (invoiceLine.GetValidationSuspender())
					{
						ResetJobAndChargeCode(invoiceLine, true);
						invoiceLine.AL_AG = ZGuid.Empty;
						invoiceLine.GenericCharge = (Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
					}
					if (Config.RunExtraValidation && !invoiceLine.GenericCharge.IsEmpty)
					{
						Notifier.AddWarningToNotifications(Res.GetString("c363b9d8-3601-44dc-bf22-f565e112c347", "This transaction line has been allocated to the GL Journal Clearing Account"));
					}
				}

				if (Config.RunExtraValidation && invoiceLine.GenericCharge.IsEmpty)
				{
					Notifier.AddErrorToNotifications(Res.GetString("b22cf036-7dd7-455f-a818-d9d009ba2774", "Cannot import transaction: Please set up the GL Journal Clearing Account in the Registry"));
				}
			}
		}

		void SetWhtRate(DependentTransactionLine invoiceLine, Xsd.TxnLine xmlInvoiceLine, string errorContext)
		{
			if (!xmlInvoiceLine.WHTCode.IsEmpty && GlbCompany.CurrentCompany.GC_IsWHTRegistered)
			{
				ZQuery whtRateFilter = new ZQuery(AccWithholdingSchema.AW_Code, xmlInvoiceLine.WHTCode);
				whtRateFilter.AddToFilter(AccWithholdingSchema.AW_GC, GlbCompany.CurrentCompany.PK);
				AccWithholding whtRate = invoiceLine.Factory.LoadTop1<AccWithholding>(whtRateFilter);

				int numberOfWhtRates = invoiceLine.Factory.GetDatabaseCount(typeof(AccWithholding), whtRateFilter);
				Notifier.ReportNoBizObjsFoundError(Res.GetString("c6449e7d-e544-478c-89ce-24632ae2c551", "WHT Rate"), xmlInvoiceLine.WHTCode, numberOfWhtRates, errorContext);

				if (whtRate != null)
				{
					invoiceLine.AL_AW = whtRate.PK;
				}
			}
		}

		readonly INotificationManager Notifier;
	}
}
