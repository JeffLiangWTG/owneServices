using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.XmlMapping;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class ExportFinancialInvoiceDataAdapter : FinancialInvoiceDataAdapter
	{
		public ExportFinancialInvoiceDataAdapter()
			: base(false)
		{ }
	}

	public class FinancialInvoiceDataAdapter : BaseAccountingDataAdapter<InvoicingBase, Xsd.TxnHeader>, IFinancialInvoiceDataAdapter
	{
		public FinancialInvoiceDataAdapter(bool isImportingMultipleInvoice)
		{
			IsImportingMultipleInvoice = isImportingMultipleInvoice;
			fRunExtraValidation = true;
			invoicingPreSaveHelper_constructorInitializedOnly = new InvoicingPreSaveHelper();
		}
		readonly bool IsImportingMultipleInvoice;

		IInvoicingPreSaveHelper InvoicingPreSaveHelper => invoicingPreSaveHelper_constructorInitializedOnly;
		IInvoicingPreSaveHelper invoicingPreSaveHelper_constructorInitializedOnly;

		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}

		public override string RootElementName
		{
			get { return "FinancialInvoice"; }
		}

		public override XmlSchema Schema
		{
			get { return null; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.FinancialTransactionsSchema; }
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(InvoicingBase bizObj, Xsd.TxnHeader constructedValueObject, IValueObjectExportContext context)
		{
			PopulateValuesForXmlInvoiceHeader(bizObj, constructedValueObject, context);
		}

		void PopulateValuesForXmlInvoiceHeader(InvoicingBase invoice, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectExportContext context)
		{
			xmlInvoiceHeader.Ledger = TxnHeaderMapper.GetTxnHeaderLedger(invoice.AH_Ledger);
			xmlInvoiceHeader.DebtorOrCreditor = GenerateDebtorOrCreditor(invoice, context);

			xmlInvoiceHeader.TxnType = TxnHeaderMapper.GetTxnHeaderTxnType(invoice.AH_TransactionType);
			xmlInvoiceHeader.TxnCount = invoice.AH_TransactionCount.ToString();
			xmlInvoiceHeader.TxnCategory = invoice.AH_TransactionCategory;
			xmlInvoiceHeader.TxnNumber = invoice.TransactionNumberPrefixed;
			xmlInvoiceHeader.JobInvoiceNo = invoice.AH_ConsolidatedInvoiceRef;
			xmlInvoiceHeader.TxnReference = invoice.AH_TransactionReference;

			if (invoice is ARInvoice && invoice.AH_IsDisbursementCalc)
			{
				xmlInvoiceHeader.DisbursementFlag = invoice.AH_IsDisbursementCalc;
				xmlInvoiceHeader.DisbursementFlagSpecified = true;
			}

			xmlInvoiceHeader.Description = invoice.AH_Desc;
			xmlInvoiceHeader.InvoiceDate = invoice.AH_InvoiceDate;
			xmlInvoiceHeader.InvTerm = invoice.AH_InvoiceTerm;
			xmlInvoiceHeader.InvTermDays = invoice.AH_InvoiceTermDays.ToString();
			xmlInvoiceHeader.DueDate = invoice.AH_DueDate;
			xmlInvoiceHeader.PostDate = invoice.AH_PostDate;
#if DEBUG
			xmlInvoiceHeader.TxnHeaderGUID = "headerGUID";
#else
			xmlInvoiceHeader.TxnHeaderGUID = invoice.PK.ToString();
#endif
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(invoice.Factory);
			xmlInvoiceHeader.GLPeriod = BusinessObjectRetriever.GetGLPeriodFromDate(invoice.Factory, invoice.AH_PostDate).ToString();

			xmlInvoiceHeader.Branch = invoice.Branch.GB_Code;
			xmlInvoiceHeader.Department = invoice.Department.GE_Code;

			xmlInvoiceHeader.LocalInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_LocalExTaxAmount, invoice.Branch.Company.LocalCurrency, invoice.GetType()); // Uses Multiplier
			xmlInvoiceHeader.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_LocalExTaxAmount + invoice.AH_LocalTaxAmount, invoice.Branch.Company.LocalCurrency, invoice.GetType());  // Uses Multiplier
			xmlInvoiceHeader.LocalTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_LocalTaxAmount, invoice.Branch.Company.LocalCurrency, invoice.GetType()); // Uses Multiplier
			xmlInvoiceHeader.LocalWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_LocalWHTAmount, invoice.Branch.Company.LocalCurrency, invoice.GetType()); // Uses Multiplier

			xmlInvoiceHeader.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_OSExTaxAmount, invoice.TransactionCurrency, invoice.GetType()); // Uses Multiplier
			xmlInvoiceHeader.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_OSExTaxAmount + invoice.AH_OSTaxAmount, invoice.TransactionCurrency, invoice.GetType()); // Uses Multiplier
			xmlInvoiceHeader.OsTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_OSTaxAmount, invoice.TransactionCurrency, invoice.GetType()); // Uses Multiplier
			xmlInvoiceHeader.OsWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_OSWHTAmount, invoice.TransactionCurrency, invoice.GetType()); // Uses Multiplier

			xmlInvoiceHeader.CashBasisTaxIndicator = TxnHeaderMapper.GetCashBasisTaxIndicator(invoice.Branch.Company.GC_IsGSTCashBasis);
			xmlInvoiceHeader.CashBasisTaxIndicatorSpecified = true;

			xmlInvoiceHeader.GlAccount = ZString.Empty;
			xmlInvoiceHeader.CreatedUserId = invoice.CreatingUserID;

			xmlInvoiceHeader.TxnLines = GetTxnLineCollection(invoice.Lines, context);
			xmlInvoiceHeader.ENettStoragePaymentDetails.IsSpecified = false;

			PopulatePaymentOrReceiptFieldValuesOnInvoice(xmlInvoiceHeader, invoice, context);
			PopulateOwnerOrderReferences(xmlInvoiceHeader, invoice);

			if (invoice.AH_OA_InvoiceAddressOverride.IsValid)
			{
				OrgAddress overrideAddress = invoice.Factory.Load<OrgAddress>(invoice.AH_OA_InvoiceAddressOverride);
				xmlInvoiceHeader.TxnOverrideAddress = new AddressValueObjectHelper("").ExportToValueObject(overrideAddress, context);
			}

			if (invoice.AH_OC_InvoiceContactOverride.IsValid)
			{
				OrgContact overrideContact = invoice.Factory.Load<OrgContact>(invoice.AH_OC_InvoiceContactOverride);
				xmlInvoiceHeader.TxnOverrideContact = new ContactValueObjectHelper("").ExportToValueObject(overrideContact, context);
			}

			if (ShouldPopulateAttachment(invoice))
			{
				PopulateAttachment(xmlInvoiceHeader, invoice, context);
			}

			xmlInvoiceHeader.ENettStoragePaymentDetails = new Xsd.ENettStoragePaymentDetails();
			xmlInvoiceHeader.ENettStoragePaymentDetails.IsSpecified = false;
		}

		protected virtual bool ShouldPopulateAttachment(InvoicingBase invoice)
		{
			return false;
		}

		void PopulateAttachment(Xsd.TxnHeader xmlInvoiceHeader, InvoicingBase invoice, INotifications notify)
		{
			ZString fileName;
			byte[] data = GetInvoicePdf(invoice, out fileName);

			if (data != null && data.Any() && !fileName.IsEmpty)
			{
				var attachment = xmlInvoiceHeader.Attachments.AddNew();
				attachment.Data = data;
				attachment.FileName = fileName;
			}
		}

		protected virtual InvoicePrintTask GetPrintTask(InvoicingBase invoice)
		{
			return new InvoicePrintTask(new InvoicePrintTask.Configuration(invoice) { ShouldCreateeDocs = false });
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filename construction")]
		protected virtual byte[] GetInvoicePdf(InvoicingBase invoice, out ZString fileName)
		{
			byte[] result = null;

			try
			{
				using (ExcelInterface xlInterface = new ExcelInterface())
				using (MemoryStream pdfStream = new MemoryStream())
				{
					xlInterface.LoadExcelFile(GetPrintTask(invoice).RunToStreamExcelOnly());
					xlInterface.ExportToPdfAndScale(pdfStream, 100, null);
					result = pdfStream.ToArray();

					fileName = string.Format("Invoice {0}.pdf", invoice.AH_TransactionNum);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				result = null;
				fileName = string.Empty;
#if DEBUG
				ErrorReporter.ReportOnce("Error when generating Invoice PDF attachment from FinancialInvoiceDataAadpter", e);
#endif
			}

			return result;
		}

		Xsd.Organisation GenerateDebtorOrCreditor(InvoicingBase invoice, IValueObjectExportContext context)
		{
			if (invoice.Header == null)
			{
				return null;
			}

			Xsd.Organisation xOrg = OrganisationAdapter.ExportToValueObject(invoice.Header, context);

			if (invoice.Header.APSettlementGroup != null)
			{
				xOrg.OrganisationDetails.AccountsPayables.AddNew().SettlementDetails.SettlementGroup = GetOrganisationDetails(invoice.Header.APSettlementGroup);
			}

			if (invoice.Header.ARSettlementGroup != null)
			{
				xOrg.OrganisationDetails.AccountsReceivables.AddNew().SettlementDetails.SettlementGroup = GetOrganisationDetails(invoice.Header.ARSettlementGroup);
			}

			if (invoice.Header.MiscServ.OM_OJ_ARDebtorGroup.IsValid)
			{
				Xsd.AccountsReceivable receivable = xOrg.OrganisationDetails.AccountsReceivables.Count > 0
																			? xOrg.OrganisationDetails.AccountsReceivables[0] :
																			xOrg.OrganisationDetails.AccountsReceivables.AddNew();

				receivable.AccountGroup = invoice.Factory.Load<OrgDebtorGroup>(invoice.Header.MiscServ.OM_OJ_ARDebtorGroup).OJ_Code;
			}

			if (invoice.Header.MiscServ.OM_OG_APCreditorGroup.IsValid)
			{
				Xsd.AccountsPayable payable = xOrg.OrganisationDetails.AccountsPayables.Count > 0
																			? xOrg.OrganisationDetails.AccountsPayables[0] :
																			xOrg.OrganisationDetails.AccountsPayables.AddNew();

				payable.AccountGroup = invoice.Factory.Load<OrgCreditorGroup>(invoice.Header.MiscServ.OM_OG_APCreditorGroup).OG_Code;
			}

			if (invoice.Header.CompanyData != null)
			{
				Xsd.AccountsReceivable accountsReceivable = xOrg.OrganisationDetails.AccountsReceivables.Count > 0
															? xOrg.OrganisationDetails.AccountsReceivables[0] :
															xOrg.OrganisationDetails.AccountsReceivables.AddNew();
				accountsReceivable.AllowMultiCurrencyPayment = invoice.Header.CompanyData.OB_ARAllowMultiCurrencyPayment;
				accountsReceivable.AllowMultiCurrencyPaymentSpecified = true;
			}

			if (!invoice.Header.CompanyData.OB_APExternalCreditorCode.IsEmpty)
			{
				Xsd.AccountsPayable payable = xOrg.OrganisationDetails.AccountsPayables.Count > 0
																			? xOrg.OrganisationDetails.AccountsPayables[0] :
																			xOrg.OrganisationDetails.AccountsPayables.AddNew();

				payable.ExternalCreditorCode = invoice.Header.CompanyData.OB_APExternalCreditorCode;
			}

			if (!invoice.Header.CompanyData.OB_ARExternalDebtorCode.IsEmpty)
			{
				Xsd.AccountsReceivable accountsReceivable = xOrg.OrganisationDetails.AccountsReceivables.Count > 0
															? xOrg.OrganisationDetails.AccountsReceivables[0] :
															xOrg.OrganisationDetails.AccountsReceivables.AddNew();

				accountsReceivable.ExternalDebtorCode = invoice.Header.CompanyData.OB_ARExternalDebtorCode;
			}

			return xOrg;
		}

		Xsd.OrganisationDetail GetOrganisationDetails(OrgHeader organisation)
		{
			var xsd = new Xsd.Organisation();
			OrganisationAdapter.ExportToValueObject(organisation, xsd, new ValueObjectExportContext(new NotificationBuffer()));

			xsd.OrganisationDetails.EDICode = organisation.OH_Code;
			xsd.OrganisationDetails.OwnerCode = organisation.OH_Code;

			return xsd.OrganisationDetails;
		}

		void PopulateOwnerOrderReferences(Xsd.TxnHeader xmlInvoiceHeader, InvoicingBase invoice)
		{
			if (invoice.IsJobRelated)
			{
				if (IsShipmentJob(invoice.Job))
				{
					var shipment = invoice.Factory.Load<ForwardingShipment>(invoice.Job.JH_ParentID);
					if (shipment != null)
					{
						xmlInvoiceHeader.OrderReference = shipment.DocsAndCartage.JP_OrderItemsAsString;
						if (shipment.Declarations.Length > 0)
						{
							xmlInvoiceHeader.OwnerReference = ((BaseJobDeclaration)shipment.Declarations[0]).JE_OwnerRef;
						}
					}
				}
				if (IsDeclarationJob(invoice.Job))
				{
					var declaration = invoice.Factory.Load<BaseJobDeclaration>(invoice.Job.JH_ParentID);
					if (declaration != null)
					{
						xmlInvoiceHeader.OwnerReference = declaration.JE_OwnerRef;
						if (declaration.IsStandAlone)
						{
							xmlInvoiceHeader.OrderReference = declaration.DocsAndCartage.JP_OrderItemsAsString;
						}
						else if (declaration.Shipment != null)
						{
							xmlInvoiceHeader.OrderReference = declaration.Shipment.DocsAndCartage.JP_OrderItemsAsString;
						}
					}
				}
			}
		}

		void PopulatePaymentOrReceiptFieldValuesOnInvoice(Xsd.TxnHeader xmlInvoiceHeader, InvoicingBase invoice, INotifications notify)
		{
			if (invoice.AH_OutstandingAmount == 0)
			{
				AccTransactionHeader paymentOrReceipt = null;

				if (invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
				{
					paymentOrReceipt = GetPaymentForFullyPaidAPInvoice(invoice);
				}
				else
				{
					paymentOrReceipt = GetPaymentOrReceiptForFullyPaidInvoice(invoice);
				}

				if (paymentOrReceipt != null)
				{
					xmlInvoiceHeader.BankCode = (paymentOrReceipt.BankAccount != null) ? paymentOrReceipt.BankAccount.AB_Code : ZString.Empty;
					xmlInvoiceHeader.ReceiptPaymentType = TxnHeaderMapper.GetTxnHeaderReceiptPaymentType(paymentOrReceipt.AH_ReceiptType, notify);
					xmlInvoiceHeader.ReceiptPaymentTypeSpecified = true;
					xmlInvoiceHeader.ChequeOrReference = paymentOrReceipt.AH_ChequeOrReference;

					if (paymentOrReceipt.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Receipt)
					{
						xmlInvoiceHeader.ChequeDrawer = paymentOrReceipt.AH_ChequeDrawer;
						xmlInvoiceHeader.DrawerBank = paymentOrReceipt.AH_DrawerBank;
						xmlInvoiceHeader.DrawerBankBranch = paymentOrReceipt.AH_DrawerBranch;
					}
				}
			}
		}

		#region Find Payment associated with Invoice

		AccTransactionHeader GetPaymentForFullyPaidAPInvoice(InvoicingBase invoice)
		{
			ZQuery paymentFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Payment);
			paymentFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			paymentFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Payment);
			paymentFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, invoice.AH_TransactionBelongsToGroup);
			paymentFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)(invoice.AH_TransactionCount + 1));
			paymentFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			return invoice.Factory.LoadTop1<AccTransactionHeader>(paymentFilter);
		}

		AccTransactionHeader GetPaymentOrReceiptForFullyPaidInvoice(InvoicingBase invoice)
		{
			TransactionMatchLinkCollection invoiceMatchLinks = GetAllMatchLinksForInvoice(invoice);
			TransactionMatchLinkCollection matchLinksInRelatedMatchGroups = GetMatchLinks(invoiceMatchLinks, invoice.Factory);

			AccTransactionHeader payment = null;
			ZString transactionType = (invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable) ? ZArchitecture.Core.TransactionTypes.Receipt : ZArchitecture.Core.TransactionTypes.Payment;

			foreach (AccTransactionMatchLink proposedPaymentLink in matchLinksInRelatedMatchGroups)
			{
				if (IsMatchlinkForSingleRelatedPaymentReceipt(proposedPaymentLink, invoice, transactionType))
				{
					payment = proposedPaymentLink.TransactionHeader;
					break;
				}
			}

			return payment;
		}

		TransactionMatchLinkCollection GetAllMatchLinksForInvoice(InvoicingBase invoice)
		{
			ZQuery findAllMatchLinksRelatingToInvoiceFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice.PK);
			TransactionMatchLinkCollection invoiceMatchLinks = new TransactionMatchLinkCollection(invoice.Factory, findAllMatchLinksRelatingToInvoiceFilter);
			invoiceMatchLinks.Load();

			return invoiceMatchLinks;
		}

#if DEBUG
		protected
#endif
 TransactionMatchLinkCollection GetMatchLinks(TransactionMatchLinkCollection matchLinks, BusinessObjectFactory factory)
		{
			ZQuery findOtherMatchLinksFilter = new ZQuery();
			findOtherMatchLinksFilter.DefaultJoinCondition = JoinCondition.Or;

			foreach (AccTransactionMatchLink matchLink in matchLinks)
			{
				findOtherMatchLinksFilter.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchLink.AP_MatchGroupNum);
			}

			TransactionMatchLinkCollection otherMatchLinks = new TransactionMatchLinkCollection(factory);

			if (matchLinks.Count != 0)
			{
				otherMatchLinks.Load(findOtherMatchLinksFilter);
			}

			return otherMatchLinks;
		}

		bool IsMatchlinkForSingleRelatedPaymentReceipt(AccTransactionMatchLink proposedPaymentLink, InvoicingBase invoice, ZString transactionType)
		{
			return proposedPaymentLink != null && proposedPaymentLink.TransactionHeader != null &&
				proposedPaymentLink.TransactionHeader.AH_TransactionType == transactionType &&
				proposedPaymentLink.TransactionHeader.AH_GC == GlbCompany.CurrentCompany.PK &&
				invoice.AH_InvoiceAmount + invoice.AH_GSTAmount + proposedPaymentLink.AP_Amount == 0;
		}

		#endregion

		#region Transaction Lines

		protected Xsd.TxnLineCollection GetTxnLineCollection(InvoicingLineBaseCollection invoiceLines, INotifications notify)
		{
			Xsd.TxnLineCollection xmlInvoiceLines = null;

			if (invoiceLines != null && invoiceLines.Count > 0)
			{
				xmlInvoiceLines = new Xsd.TxnLineCollection();

				foreach (InvoicingLineBase invoiceLine in invoiceLines)
				{
					Xsd.TxnLine newInvoiceLine = xmlInvoiceLines.AddNew();
					PopulateValuesForXmlInvoiceLine(newInvoiceLine, invoiceLine, notify);
				}
			}

			return xmlInvoiceLines;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Xml construction")]
		protected void PopulateValuesForXmlInvoiceLine(Xsd.TxnLine xmlInvoiceLine, InvoicingLineBase invoiceLine, INotifications notify)
		{
			if (invoiceLine.ChargeCode != null)
			{
				xmlInvoiceLine.ChargeCode = invoiceLine.ChargeCode.AC_Code;
				xmlInvoiceLine.ChargeGroup = invoiceLine.ChargeCode.AC_ChargeGroup;
				xmlInvoiceLine.ChargeSubGroup = invoiceLine.ChargeCode.AC_ChargeSubGroup;
				xmlInvoiceLine.eNettChargeCodeMapping = invoiceLine.ChargeCode.GetMappingForOrganisation(AccountingConfigurationRegistry.Instance.ENettRegistration.Value.OrganisationPK);

				if (invoiceLine.ChargeCode.SalesGroup != null)
				{
					xmlInvoiceLine.ChargeCodeSalesGroup = invoiceLine.ChargeCode.SalesGroup.AR_Code;
				}

				if (invoiceLine.ChargeCode.ExpenseGroup != null)
				{
					xmlInvoiceLine.ChargeCodeExpenseGroup = invoiceLine.ChargeCode.ExpenseGroup.AR_Code;
				}
			}

			if (invoiceLine.GLHeader != null)
			{
				xmlInvoiceLine.GLAccount = invoiceLine.GLHeader.AG_AccountNum;
			}

			if (invoiceLine.Branch != null)
			{
				xmlInvoiceLine.Branch = invoiceLine.Branch.GB_Code;
			}

			if (invoiceLine.Department != null)
			{
				xmlInvoiceLine.Department = invoiceLine.Department.GE_Code;
			}

			GenericJob jobDetails = null;

			if (invoiceLine.AL_JH.IsValid && invoiceLine.Job != null && !invoiceLine.Job.JH_ParentID.IsEmpty && !invoiceLine.Job.JH_ParentTableCode.IsEmpty)
			{
				jobDetails = invoiceLine.Job.LoadGenericJob<GenericJob>();
			}

			if (jobDetails != null)
			{
				InvoicingBase invoice = invoiceLine.InvoiceBase;
				if (invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
				{
					ZQuery query = new ZQuery(JobChargeSchema.JR_AL_ARLine, invoiceLine.PK);
					Charge lineCharge = invoiceLine.Factory.LoadTop1<Charge>(query);

					if (lineCharge != null && lineCharge.SellCurrency != null)
					{
						xmlInvoiceLine.OSChargeAmount = TxnHeaderMapper.GetXmlFinancialValue(lineCharge.JR_OSSellAmt, lineCharge.SellCurrency, lineCharge.GetType()); // Uses Multiplier
						xmlInvoiceLine.OSChargeTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(lineCharge.JR_OSSellGSTAmt_Calc, lineCharge.SellCurrency, lineCharge.GetType()); // Uses Multiplier
					}
				}

				if (jobDetails.InvoicingSupporter.Origin != null)
				{
					xmlInvoiceLine.OriginPortCode = Xsd.UNLOCO.FromPortCode(invoiceLine.Factory, jobDetails.InvoicingSupporter.Origin.RL_Code);
				}

				if (jobDetails.InvoicingSupporter.Destination != null)
				{
					xmlInvoiceLine.DestinationPortCode = Xsd.UNLOCO.FromPortCode(invoiceLine.Factory, jobDetails.InvoicingSupporter.Destination.RL_Code);
				}

				xmlInvoiceLine.ConsolOrJobNo = jobDetails.JobNumber;
				xmlInvoiceLine.ConsolOrJobType = TxnHeaderMapper.GetTxnLineConsolOrJobType(jobDetails.VJ_JobType, notify);
				xmlInvoiceLine.ConsolOrJobTypeSpecified = true;
				xmlInvoiceLine.MasterBillNo = jobDetails.InvoicingSupporter.MasterBillNumber;
				xmlInvoiceLine.HouseBIllNo = jobDetails.InvoicingSupporter.HouseBillNumber;

				var paymentTermInfo = jobDetails.InvoicingSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue);
				xmlInvoiceLine.Incoterm = paymentTermInfo != null && paymentTermInfo.InfoType == PaymentTermType.Incoterm ? paymentTermInfo.Value : string.Empty;

				xmlInvoiceLine.ETA = jobDetails.VJ_ETA.IsEmpty ? jobDetails.InvoicingSupporter.ETA : jobDetails.VJ_ETA;
				xmlInvoiceLine.ETD = jobDetails.VJ_ETD.IsEmpty ? jobDetails.InvoicingSupporter.ETD : jobDetails.VJ_ETD;

				if (!jobDetails.InvoicingSupporter.TransportMode.IsEmpty)
				{
					xmlInvoiceLine.ModeOfTransport = TransportModeToXmlCodeMappings.Instance.GetExternalCode(jobDetails.InvoicingSupporter.TransportMode, "", null);
				}

				xmlInvoiceLine.Weight = Xsd.DimensionValue.FromAmountAndUnit(jobDetails.InvoicingSupporter.ActualWeight, jobDetails.InvoicingSupporter.ActualWeightUnit);
				xmlInvoiceLine.Volume = Xsd.DimensionValue.FromAmountAndUnit(jobDetails.InvoicingSupporter.ActualVolume, jobDetails.InvoicingSupporter.ActualVolumeUnit);

				xmlInvoiceLine.Chargeable = jobDetails.InvoicingSupporter.ActualChargeable;
				xmlInvoiceLine.ChargeableSpecified = true;
			}
			else
			{
				xmlInvoiceLine.ConsolOrJobNo = "";
				xmlInvoiceLine.MasterBillNo = "";
				xmlInvoiceLine.HouseBIllNo = "";
				xmlInvoiceLine.Incoterm = "";
				xmlInvoiceLine.ConsolOrJobTypeSpecified = false;

				xmlInvoiceLine.Weight.IsSpecified = true;
				xmlInvoiceLine.Volume.IsSpecified = true;
			}

			if (invoiceLine.TaxRate != null)
			{
				xmlInvoiceLine.TaxCode = invoiceLine.TaxRate.AT_Code;
			}

			if (invoiceLine.Withholding != null)
			{
				xmlInvoiceLine.WHTCode = invoiceLine.Withholding.AW_Code;
			}

			if (invoiceLine.VATClass != null)
			{
				xmlInvoiceLine.TaxMsgCode = invoiceLine.VATClass.A9_Code;
			}

			xmlInvoiceLine.LineType = TxnHeaderMapper.GetTxnLineLineType(invoiceLine.AL_LineType);
			xmlInvoiceLine.LineTypeSpecified = true;

			xmlInvoiceLine.Sequence = invoiceLine.AL_Sequence.ToString();
			xmlInvoiceLine.Description = invoiceLine.AL_Desc;
			SetTxnLineGuid(xmlInvoiceLine, invoiceLine);

			xmlInvoiceLine.LocalInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(invoiceLine.AL_LocalExTaxAmount, invoiceLine.Branch.Company.LocalCurrency, invoiceLine.GetType()); // Uses Multiplier
			xmlInvoiceLine.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(invoiceLine.AL_LocalTotalAmount, invoiceLine.Branch.Company.LocalCurrency, invoiceLine.GetType()); // Uses Multiplier
			xmlInvoiceLine.LocalTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(invoiceLine.AL_LocalTaxAmount, invoiceLine.Branch.Company.LocalCurrency, invoiceLine.GetType()); // Uses Multiplier
			xmlInvoiceLine.LocalWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(invoiceLine.AL_LocalWHTAmount, invoiceLine.Branch.Company.LocalCurrency, invoiceLine.GetType()); // Uses Multiplier

			xmlInvoiceLine.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(invoiceLine.AL_OSExTaxAmount, invoiceLine.TransactionCurrency, invoiceLine.GetType()); // Uses Multiplier
			xmlInvoiceLine.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(invoiceLine.AL_OverseasTotal, invoiceLine.TransactionCurrency, invoiceLine.GetType()); // Uses Multiplier
			xmlInvoiceLine.OsTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(invoiceLine.AL_OSTaxAmount, invoiceLine.TransactionCurrency, invoiceLine.GetType()); // Uses Multiplier
			xmlInvoiceLine.OsWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(invoiceLine.AL_OSWHTAmount, invoiceLine.TransactionCurrency, invoiceLine.GetType()); // Uses Multiplier

			if (invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage != 100)
			{
				xmlInvoiceLine.RecoverableGSTVATPercentage = invoiceLine.AL_Calc_InputGSTVATRecoverablePercentage;
			}

			xmlInvoiceLine.IsFinalCharge = false; // used only used for importing transactions
			xmlInvoiceLine.IsFinalChargeSpecified = false; // used only used for importing transactions

			xmlInvoiceLine.DepartmentActivitySpecified = true;
			xmlInvoiceLine.DepartmentActivity = DepartmentActivityXmlMapping.Instance.GetExternalCode(invoiceLine.Department.GE_Activity, "Setting Department Activity", notify);

			CommonShipment shipment = invoiceLine.Job != null ? invoiceLine.Factory.Load<CommonShipment>(invoiceLine.Job.JH_ParentID) : null;
			if (shipment != null)
			{
				xmlInvoiceLine.BookingReference = shipment.JS_BookingReference;
			}
			xmlInvoiceLine.RevenueRecognitionDate = invoiceLine.AL_ReverseDate;

			if (!invoiceLine.Factory.HasContext(BusinessContext.InterCompanyInvoiceExport))
			{
				xmlInvoiceLine.SubAccounts = SubAccountHelper.GetSubAccountsXmlFromSubAccounts(invoiceLine.Factory, invoiceLine.SubAccounts);
			}

			PopulateAgentsOwnerOrderReference(xmlInvoiceLine, invoiceLine, notify);
		}

		protected virtual void SetTxnLineGuid(Xsd.TxnLine xmlInvoiceLine, InvoicingLineBase invoiceLine)
		{
			xmlInvoiceLine.TxnLineGUID = invoiceLine.PK.ToString();
		}

		protected void PopulateAgentsOwnerOrderReference(Xsd.TxnLine xmlInvoiceLine, InvoicingLineBase invoiceLine, INotifications notify)
		{
			BusinessObjectFactory factory = invoiceLine.Factory;

			if (invoiceLine.Job != null)
			{
				BaseJobDeclaration jobDec = null;
				if (IsDeclarationJob(invoiceLine.Job))
				{
					jobDec = factory.Load<BaseJobDeclaration>(invoiceLine.Job.JH_ParentID);
					if (jobDec != null)
					{
						xmlInvoiceLine.OwnerReference = jobDec.JE_OwnerRef;
						if (jobDec.IsStandAlone)
						{
							xmlInvoiceLine.OrderReference = jobDec.DocsAndCartage.JP_OrderItemsAsString;
						}
						else if (jobDec.Shipment != null)
						{
							xmlInvoiceLine.OrderReference = jobDec.Shipment.DocsAndCartage.JP_OrderItemsAsString;
						}
					}
				}
				else if (IsShipmentJob(invoiceLine.Job))
				{
					ZQuery filter = new ZQuery(JobDeclarationSchema.JE_JS, invoiceLine.Job.JH_ParentID);
					ZQuery branchFilter = new ZQuery(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
					filter.AddToFilter(branchFilter);
					jobDec = factory.LoadTop1<BaseJobDeclaration>(filter);
					if (jobDec != null)
					{
						xmlInvoiceLine.OrderReference = jobDec.Shipment.DocsAndCartage.JP_OrderItemsAsString;
						xmlInvoiceLine.OwnerReference = jobDec.JE_OwnerRef;
					}
					else
					{
						var shipment = factory.Load<ForwardingShipment>(invoiceLine.Job.JH_ParentID);
						if (shipment != null)
						{
							xmlInvoiceLine.OrderReference = shipment.DocsAndCartage.JP_OrderItemsAsString;
						}
					}
				}

				if (jobDec != null)
				{
					xmlInvoiceLine.AgentsReference = jobDec.JE_AgentsReference;
				}
			}
		}

		#endregion

		#endregion

		#region Import

		public new void ImportFromValueObject(InvoicingBase invoiceHeader, Xsd.TxnHeader value, IValueObjectImportContext context)
		{
			if (invoiceHeader != null)
			{
				base.ImportFromValueObject(invoiceHeader, value, context);
			}
			else
			{
				context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("c80649a7-7a15-44d4-a115-cbe272272b4e", "This transaction cannot be imported. Ledger: {0}, Transaction Type: {1}.", value.Ledger.ToString(), value.TxnType.ToString())));
			}
		}

		protected override void ImportFromValueObjectCore(InvoicingBase invoiceHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
			INotifications notify = context;

			Notifier = new NotificationManager(notify);

			if (invoiceHeader != null)
			{
				string payRef = string.IsNullOrEmpty(xmlInvoiceHeader.PaymentReference) ? string.Empty : Res.GetString("961aab51-ad46-420d-8854-5a8e707211e9", "(Pay Ref: {0})", xmlInvoiceHeader.PaymentReference);
				string errorContext = Res.GetString("80e3d307-4152-406d-abd6-0e96f4df9c96", "Transaction {0} {1} {2} {3}:", xmlInvoiceHeader.Ledger.ToString(), xmlInvoiceHeader.TxnType.ToString(), xmlInvoiceHeader.DebtorOrCreditor.EDICode, xmlInvoiceHeader.TxnNumber) + " " + payRef;
				notify.Notify(new InfoNotification(Res.GetString("1abda08c-60e0-466f-aded-7238ca6235b8", "Processing Transaction: {0}", errorContext)));

				ImportInvoice(invoiceHeader, xmlInvoiceHeader, context, "");
			}
			else if (xmlInvoiceHeader.GetType() == typeof(Xsd.TxnHeader))
			{
				Notifier.AddErrorToNotifications(Res.GetString("733690ad-1e41-4154-b981-e75dd40145b0", "This transaction cannot be imported. Ledger: {0}, Transaction Type: {1}.", xmlInvoiceHeader.Ledger.ToString(), xmlInvoiceHeader.TxnType.ToString()));
			}
			else
			{
				Notifier.AddErrorToNotifications(Res.GetString("41be2f0d-ccd6-4f40-a910-dcd67f4b9e47", "This transaction cannot be imported."));
			}
		}

		Xsd.TxnHeader ImportInvoice(InvoicingBase invoiceHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, string errorContext)
		{
			invoiceHeader.OnComplianceSequenceFailedToAssign += new EventHandler(ComplianceSequenceFailedToAssign);
			invoiceHeader.OnDigitalSignatureFailedToSign += new EventHandler<UserMessageEventArgs>(DigitalSignatureFailedToSign);
			invoiceHeader.ConsolCosting.ConsolCosts.OnJobCreationError += new EventHandler<JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs>(JobCreationFailed);
			bool isCrossLedgerImport = ShouldImportFromARtoAP(xmlInvoiceHeader, context);

			if (isCrossLedgerImport)
			{
				xmlInvoiceHeader = TransformXsdForCrossLedgerImport(invoiceHeader, xmlInvoiceHeader, context);
			}

			TransactionHeaderBuilder builder = GetTransactionHeaderBuilder(isCrossLedgerImport);

			SetValuesOnBusinessObject(invoiceHeader, builder, xmlInvoiceHeader, Notifier, context, errorContext);

			if (!TransactionHeaderBuilder.IsTaxApplicable(invoiceHeader))
			{
				TransformXsdIntoNonTaxableInvoice(xmlInvoiceHeader);
			}

			if (IsImportingMultipleInvoice && invoiceHeader != null && (invoiceHeader is APInvoice || invoiceHeader is APCreditNote))
			{
				invoiceHeader.SetContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer);
			}

			if (RunExtraValidation)
			{
				ExecuteExtraValidation(builder, invoiceHeader, xmlInvoiceHeader, context, errorContext, Notifier);
			}

			if (IsImportingMultipleInvoice && invoiceHeader != null && !invoiceHeader.IsDeleted)
			{
				InvoicingPreSaveHelper.PreSaveActionsForNonJobBillingPosting(invoiceHeader);
			}

			return xmlInvoiceHeader;
		}

		void JobCreationFailed(object sender, JobConsolCost.APInvoiceCostingJobCreationErrorEventArgs e)
		{
			Notifier.AddErrorToNotifications(e.ErrorMessage);
		}

		void ComplianceSequenceFailedToAssign(object sender, EventArgs e)
		{
			InvoicingBase invoice = (InvoicingBase)sender;
			if (!ComplianceEmails.ContainsKey(invoice.AH_ComplianceSubType))
			{
				var msg = InvoicingBase.GetMessageForComplianceSequenceErrors(e);
				Notifier.AddWarningToNotifications(msg);
				var email = new ComplianceNumberAllocationFailureEmail(((InvoicingBase)sender));
				ComplianceEmails.Add(invoice.AH_ComplianceSubType, email);
				email.Send();
			}
		}

		void DigitalSignatureFailedToSign(object sender, UserMessageEventArgs e)
		{
			InvoicingBase invoice = (InvoicingBase)sender;
			if (invoice.IsDigitialSignatureFailedDueToPreviousInvoiceNotFound && !DigitalSignatureEmails.ContainsKey(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToPreviousInvoiceNotFound))
			{
				Notifier.AddWarningToNotifications(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToPreviousInvoiceNotFound);
				var email = new DigitalSignatureSigningFailureEmail(e.Message);
				DigitalSignatureEmails.Add(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToPreviousInvoiceNotFound, email);
				email.Send();
			}
			else if (invoice.IsDigitialSignatureFailedDueToEmptySignatureInPreviousInvoice && !DigitalSignatureEmails.ContainsKey(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToEmptySignatureInPreviousInvoice))
			{
				Notifier.AddWarningToNotifications(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToEmptySignatureInPreviousInvoice);
				var email = new DigitalSignatureSigningFailureEmail(e.Message);
				DigitalSignatureEmails.Add(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToEmptySignatureInPreviousInvoice, email);
				email.Send();
			}
			else if (invoice.IsDigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime && !DigitalSignatureEmails.ContainsKey(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToNotBeingAbleToGetInvoiceCreatedLogTime))
			{
				Notifier.AddWarningToNotifications(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToNotBeingAbleToGetInvoiceCreatedLogTime);
				var email = new DigitalSignatureSigningFailureEmail(e.Message);
				DigitalSignatureEmails.Add(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToNotBeingAbleToGetInvoiceCreatedLogTime, email);
				email.Send();
			}
		}

#if DEBUG
		internal
#endif
		Dictionary<string, ComplianceNumberAllocationFailureEmail> ComplianceEmails => fComplianceEmails ?? (fComplianceEmails = new Dictionary<string, ComplianceNumberAllocationFailureEmail>());
		Dictionary<string, ComplianceNumberAllocationFailureEmail> fComplianceEmails;

#if DEBUG
		internal
#endif
		Dictionary<string, DigitalSignatureSigningFailureEmail> DigitalSignatureEmails => fDigitalSignatureEmails ?? (fDigitalSignatureEmails = new Dictionary<string, DigitalSignatureSigningFailureEmail>());
		Dictionary<string, DigitalSignatureSigningFailureEmail> fDigitalSignatureEmails;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1135: DoNotUseCountrySpecificBusinessRule", Justification = "Testing")]
		static bool IsCrossCountryImport(InvoicingBase invoiceHeader, Xsd.TxnHeader xmlInvoiceHeader)
		{
			bool crossCountryImport = false;
			if (!xmlInvoiceHeader.Branch.IsEmpty && invoiceHeader.Branch != null)
			{
				GlbBranch originalBranch = invoiceHeader.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, xmlInvoiceHeader.Branch);
				crossCountryImport = originalBranch != null && originalBranch.Company.GC_RN_NKCountryCode != invoiceHeader.Branch.Company.GC_RN_NKCountryCode;
			}
			return crossCountryImport;
		}

		void TransformXsdIntoNonTaxableInvoice(Xsd.TxnHeader xmlInvoiceHeader, AccChargeCode chargeCode = null)
		{
			var originalLocalTaxAmount = xmlInvoiceHeader.LocalTaxAmount;
			var originalOsTaxAmount = xmlInvoiceHeader.OsTaxAmount;

			xmlInvoiceHeader.LocalInvoiceAmtExclTax = xmlInvoiceHeader.LocalInvoiceAmtInclTax;
			xmlInvoiceHeader.LocalTaxAmount = new Xsd.FinancialValue();
			xmlInvoiceHeader.OsInvoiceAmtExclTax = xmlInvoiceHeader.OsInvoiceAmtInclTax;
			xmlInvoiceHeader.OsTaxAmount = new Xsd.FinancialValue();

			var lindeDescription = new ZStringBuilder();
			foreach (Xsd.TxnLine line in xmlInvoiceHeader.TxnLines)
			{
				if (chargeCode != null)
				{
					if (line.OsTaxAmount.Value != 0)
					{
						lindeDescription.AppendLine(string.Format(CultureInfo.InvariantCulture, "-{0} {1}", line.Description, line.OsTaxAmount.Value * new ZDecimal(-1.0)));
					}
				}
				else
				{
					line.OriginalLocalInvoiceAmtExclTax = line.LocalInvoiceAmtExclTax;
					line.OriginalOsInvoiceAmtExclTax = line.OsInvoiceAmtExclTax;
					line.OriginalOsTaxAmount = line.OsTaxAmount;
					line.UseOriginalAmount = true;
					line.UseOriginalAmountSpecified = true;

					line.LocalInvoiceAmtExclTax = line.LocalInvoiceAmtInclTax;
					line.OsInvoiceAmtExclTax = line.OsInvoiceAmtInclTax;
				}

				line.LocalTaxAmount = new Xsd.FinancialValue();
				line.OsTaxAmount = new Xsd.FinancialValue();
				line.TaxCode = ZString.Empty;
			}

			if (!lindeDescription.IsEmpty)
			{
				var line = xmlInvoiceHeader.TxnLines.AddNew();
				line.IsSplitLine = true;
				line.LineType = Xsd.TxnLineLineType.CST;
				line.Branch = xmlInvoiceHeader.Branch;
				line.Department = xmlInvoiceHeader.Department;
				line.ChargeCode = chargeCode.AC_Code;
				line.Description = chargeCode.AC_DescMultilingual + System.Environment.NewLine + lindeDescription;
				line.LocalInvoiceAmtExclTax = originalLocalTaxAmount;
				line.LocalInvoiceAmtInclTax = originalLocalTaxAmount;
				line.OsInvoiceAmtExclTax = originalOsTaxAmount;
				line.OsInvoiceAmtInclTax = originalOsTaxAmount;
				line.LocalTaxAmount = new Xsd.FinancialValue();
				line.OsTaxAmount = new Xsd.FinancialValue();
				line.TaxCode = ZString.Empty;
			}
		}

		protected virtual TransactionHeaderBuilder GetTransactionHeaderBuilder(bool isCrossLedgerImport)
		{
			TransactionHeaderBuilder builder = new TransactionHeaderBuilder(Notifier, GetTransactionBuilderConfig(isCrossLedgerImport));
			return builder;
		}

		protected virtual TransactionBuilderConfig GetTransactionBuilderConfig(bool isCrossLedgerImport)
		{
			TransactionBuilderConfig config = new TransactionBuilderConfig();
			config.RunExtraValidation = RunExtraValidation;
			config.CrossLedgerImport = isCrossLedgerImport;
			return config;
		}

		protected virtual bool ShouldImportFromARtoAP(Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			return InvoiceXmlValueObjectSerializer.IsCrossLedger(context) && xmlInvoiceHeader.Ledger.ToString() == ZArchitecture.Core.LedgerTypes.AccountsReceivable && !DisableCrossLedgerImport;
		}

		protected virtual void SetValuesOnBusinessObject(InvoicingBase invoiceHeader, TransactionHeaderBuilder builder, Xsd.TxnHeader xmlInvoiceHeader, NotificationManager notifier, IValueObjectImportContext context, string errorContext)
		{
			using (invoiceHeader.SuspendCreditLimitCheck())
			{
				builder.SetValuesOnInvoiceBusinessObject(invoiceHeader, xmlInvoiceHeader, context, errorContext);
				invoiceHeader.SuspendCreditLimitCheckOnSaved();
			}
		}

		protected virtual void ExecuteExtraValidation(TransactionHeaderBuilder builder, InvoicingBase invoiceHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context, string errorContext, NotificationManager notify)
		{
			builder.CheckTotalsOnTransactionHeaderWithLines(invoiceHeader, xmlInvoiceHeader, errorContext);
			builder.RunValidationAndReportErrors(invoiceHeader);
			if (invoiceHeader.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable &&
				(invoiceHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice ||
					invoiceHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote ||
						invoiceHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote))
			{
				builder.CheckInCacheForDuplicateTransactionNumbers(invoiceHeader);
			}

			if (Notifier.ErrorsHaveBeenReported)
			{
				DeleteTransactionAndAnythingCreatedByIt(invoiceHeader);
				context.Notify(new InfoNotification("  " + Res.GetString("694ef52d-6fee-4fe6-b1b9-5ae04995e3ad", "This transaction has errors and was not imported")));
			}
			else
			{
				context.Notify(new InfoNotification("  " + Res.GetString("f7c1eb0a-0c45-40d4-bff0-c530714af3cd", "Completed Processing Transaction")));
			}

			context.Notify(new NewlineNotification());
		}

		#region New BusinessObject

		protected override InvoicingBase NewBusinessObject(Xsd.TxnHeader xmlTxnHeader, IValueObjectImportContext context)
		{
			InvoicingBase bizObj = null;

			bool isCrossLedger = InvoiceXmlValueObjectSerializer.IsCrossLedger(context) && !DisableCrossLedgerImport;
			Type newBizObjType = TxnHeaderMapper.GetBizObjTypeFromIValueObject(xmlTxnHeader, isCrossLedger);
			if (newBizObjType != null)
			{
				bizObj = (InvoicingBase)context.Factory.New(newBizObjType);
				SetTransactionPropertiesCachedInFactory(bizObj, context);
			}

			return bizObj;
		}

		void SetTransactionPropertiesCachedInFactory(InvoicingBase invoiceHeader, IValueObjectImportContext context)
		{
			invoiceHeader.USSalesTaxCalculator = context.Factory.GetCachedValue("FinancialInvoiceDataAdapter_IUSSalesTaxCalculator", () => ObjectFactory.Get<IUSSalesTaxCalculator>());
		}

		protected override bool MinimumRequirementsMetForNewImport(Xsd.TxnHeader xmlTxnHeader, IValueObjectImportContext context)
		{
			return IsValidLedgerTypeandTxnType(xmlTxnHeader, context);
		}

		internal bool IsValidLedgerTypeandTxnType(Xsd.TxnHeader xmlTxnHeader, IValueObjectImportContext context)
		{
			bool isValid = false;

			if ((xmlTxnHeader.Ledger == Xsd.TxnLedgerType.AP || xmlTxnHeader.Ledger == Xsd.TxnLedgerType.AR)
				&& (xmlTxnHeader.TxnType == Xsd.TxnType.INV || xmlTxnHeader.TxnType == Xsd.TxnType.ADJ || xmlTxnHeader.TxnType == Xsd.TxnType.CRD))
			{
				isValid = true;
			}
			else
			{
				context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("26417caf-1dc2-4a10-8993-f4a6f969f340", "This transaction cannot be imported as the Ledger or Transaction Type is invalid. Ledger: {0}, Transaction Type: {1}. Valid Ledgers are [AP, AR] and Valid Transaction Types are [ADJ, CRD, INV].", xmlTxnHeader.Ledger.ToString(), xmlTxnHeader.TxnType.ToString())));
			}

			return isValid;
		}

		#endregion

		#region Cross Ledger Import

		protected virtual Xsd.TxnHeader TransformXsdForCrossLedgerImport(InvoicingBase invoiceHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			xmlInvoiceHeader.Ledger = Xsd.TxnLedgerType.AP;
			xmlInvoiceHeader.DebtorOrCreditor = GetCreditorForCrossLedgerImport(invoiceHeader, context);

			xmlInvoiceHeader.LocalInvoiceAmtInclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlInvoiceHeader.LocalInvoiceAmtInclTax);
			xmlInvoiceHeader.LocalInvoiceAmtExclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlInvoiceHeader.LocalInvoiceAmtExclTax);
			xmlInvoiceHeader.LocalTaxAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlInvoiceHeader.LocalTaxAmount);
			xmlInvoiceHeader.LocalWHTAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlInvoiceHeader.LocalWHTAmount);

			xmlInvoiceHeader.OsInvoiceAmtInclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlInvoiceHeader.OsInvoiceAmtInclTax);
			xmlInvoiceHeader.OsInvoiceAmtExclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlInvoiceHeader.OsInvoiceAmtExclTax);
			xmlInvoiceHeader.OsTaxAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlInvoiceHeader.OsTaxAmount);
			xmlInvoiceHeader.OsWHTAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(xmlInvoiceHeader.OsWHTAmount);

			foreach (Xsd.TxnLine line in xmlInvoiceHeader.TxnLines)
			{
				line.LineType = Xsd.TxnLineLineType.CST;

				line.LocalInvoiceAmtInclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(line.LocalInvoiceAmtInclTax);
				line.LocalInvoiceAmtExclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(line.LocalInvoiceAmtExclTax);
				line.LocalTaxAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(line.LocalTaxAmount);
				line.LocalWHTAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(line.LocalWHTAmount);

				line.OsInvoiceAmtInclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(line.OsInvoiceAmtInclTax);
				line.OsInvoiceAmtExclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(line.OsInvoiceAmtExclTax);
				line.OsTaxAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(line.OsTaxAmount);
				line.OsWHTAmount = TxnHeaderMapper.SwapXmlFinancialValueSign(line.OsWHTAmount);
			}

			if (IsCrossCountryImport(invoiceHeader, xmlInvoiceHeader))
			{
				AccChargeCode chargeCode = null;
				var chargeCodePK = AccountingConfigurationRegistry.Instance.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.GetValueWithoutFallback(invoiceHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
				if (chargeCodePK != Guid.Empty)
				{
					chargeCode = invoiceHeader.Factory.Load<AccChargeCode>(chargeCodePK);
				}

				TransformXsdIntoNonTaxableInvoice(xmlInvoiceHeader, chargeCode);
			}

			return xmlInvoiceHeader;
		}

		protected virtual Xsd.Organisation GetCreditorForCrossLedgerImport(InvoicingBase invoiceHeader, IValueObjectImportContext context)
		{
			return ((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation;
		}

		#endregion

		#region Delete BusinessObject
		protected virtual void DeleteTransactionAndAnythingCreatedByIt(InvoicingBase invoiceHeader)
		{
			using (invoiceHeader.GetReportingDeletedApportionmentChargesSuspender())
			{
				// This is a special case, normally we dont want to delete transactions in accounting
				for (int i = invoiceHeader.Lines.Count - 1; i >= 0; i--)
				{
					InvoicingLineBase line = invoiceHeader.Lines[i];

					if (line.Job != null && !line.Job.IsInDatabase)
					{
						using (line.Job)
						{
							DeleteJobAndRelatedObjects(line);
						}
					}

					line.Delete();
				}

				invoiceHeader.Delete();
			}
		}

		void DeleteJobAndRelatedObjects(InvoicingLineBase line)
		{
			Job lineJob = line.Factory.Load<Job>(line.Job.PK);

			if (IsJobUsedInAnotherTransaction(line))
			{
				ZQuery findChargeForThisLineFilter = new ZQuery(JobChargeSchema.JR_AL_APLine, line.PK);
				Charge charge = line.Factory.LoadTop1<Charge>(findChargeForThisLineFilter);

				if (charge != null && !charge.IsInDatabase)
				{
					charge.Delete();
				}
			}
			else
			{
				lineJob.DeleteUnpostedLines();
				lineJob.Delete();
			}
		}

		bool IsJobUsedInAnotherTransaction(InvoicingLineBase line)
		{
			ZQuery otherTransactionLinesThatUseThisJobFilter = new ZQuery(AccTransactionLinesSchema.AL_AH, SQLComparisonOperator.NotEqual, line.AL_AH);
			otherTransactionLinesThatUseThisJobFilter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, line.AL_JH);
			otherTransactionLinesThatUseThisJobFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, line.AL_GC);

			// GetDatabaseCount doesnt include other transactions to be imported in the same batch.
			BusinessObject otherLine = line.Factory.LoadTop1(typeof(AccTransactionLines), otherTransactionLinesThatUseThisJobFilter);

			return otherLine != null;
		}

		#endregion

		/// <summary>
		/// Setting this to false will ignore the BusinessObject validation.
		/// Do NOT set this to false unless you are going to show the user 
		/// the applicable 'Create' form for the transaction (Form will force 
		/// validation to be run before saving). The extra validation is required 
		/// when importing multiple transactions or when not displaying a form before
		/// saving the transaction.
		/// </summary>
		public bool RunExtraValidation
		{
			get { return fRunExtraValidation; }
			set { fRunExtraValidation = value; }
		}

		bool fRunExtraValidation;

		public bool DisableCrossLedgerImport
		{
			get { return fDisableCrossLedgerImport; }
			set { fDisableCrossLedgerImport = value; }
		}

		bool fDisableCrossLedgerImport;

		protected NotificationManager Notifier;

		#endregion

		#if DEBUG

		public void SubstituteInvoicingPreSaveHelper_ForTestOnly(IInvoicingPreSaveHelper replacement) => invoicingPreSaveHelper_constructorInitializedOnly = replacement;
		public IInvoicingPreSaveHelper InvoicingPreSaveHelper_ExposedForTestOnly => InvoicingPreSaveHelper;

		#endif
	}
}
