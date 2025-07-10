using System;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class ARAPPaymentDataAdapter : BaseAccountingDataAdapter<Payment, Xsd.TxnHeader>
	{
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
			get { return AccountingXmlSchemaDefinitions.Instance.SingleFinancialInvoiceSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.FinancialInvoicesSchema; }
		}

		protected override Payment NewBusinessObject(Xsd.TxnHeader xmlTxnHeader, IValueObjectImportContext context)
		{
			Payment bizObj = null;

			Type newBizObjType = TxnHeaderMapper.GetBizObjTypeFromIValueObject(xmlTxnHeader);
			if (newBizObjType != null)
			{
				bizObj = context.Factory.New(newBizObjType) as APPayment;
			}

			return bizObj;
		}

		public override Payment CreateOrUpdateFromValueObject(Xsd.TxnHeader value, IValueObjectImportContext context)
		{
			Payment payment = null;
			try
			{
				payment = base.CreateOrUpdateFromValueObject(value, context);
			}
			catch (ArgumentNullException)
			{
				NotificationManager notificationManager = new NotificationManager(context);
				notificationManager.AddErrorToNotifications(Res.GetString("ad49c32a-ec95-45d0-af6f-51b45ac06278", "This transaction cannot be imported. Transaction type is not compatible."));
			}
			return payment;
		}

		#endregion

		protected override void ExportToValueObjectCore(Payment payment, Xsd.TxnHeader constructedValueObject, IValueObjectExportContext context)
		{
			constructedValueObject.Ledger = TxnHeaderMapper.GetTxnHeaderLedger(payment.AH_Ledger);
			constructedValueObject.DebtorOrCreditor = OrganisationAdapter.ExportToValueObject(payment.Header, context);
			constructedValueObject.TxnType = TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.Payment);

			constructedValueObject.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(payment.AH_OSTotalAmount, payment.TransactionCurrency, payment.GetType());
			constructedValueObject.OsInvoiceAmtExclTax = constructedValueObject.OsInvoiceAmtInclTax;
			constructedValueObject.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(payment.AH_LocalTotalAmount, payment.Branch.Company.LocalCurrency, payment.GetType());
			constructedValueObject.LocalInvoiceAmtExclTax = constructedValueObject.LocalInvoiceAmtInclTax;

			constructedValueObject.Description = payment.AH_Desc;
			constructedValueObject.InvoiceDate = payment.AH_InvoiceDate;
			constructedValueObject.DueDate = payment.AH_DueDate;
			constructedValueObject.PostDate = payment.AH_PostDate;

			constructedValueObject.Branch = payment.Branch.GB_Code;
			constructedValueObject.Department = payment.Department.GE_Code;

			constructedValueObject.BankCode = payment.BankAccount != null ? payment.BankAccount.AB_Code.ToString() : "";
			constructedValueObject.ReceiptPaymentType = TxnHeaderMapper.GetTxnHeaderReceiptPaymentType(payment.AH_ReceiptType, context);
			constructedValueObject.ReceiptPaymentTypeSpecified = true;

			constructedValueObject.ENettStoragePaymentDetails.IsSpecified = false;

			constructedValueObject.CreatedUserId = payment.CreatingUserID;

			if (payment.InvoiceAddressOverride != null)
			{
				constructedValueObject.TxnOverrideAddress = new AddressValueObjectHelper("").ExportToValueObject(payment.InvoiceAddressOverride, context);
			}

			if (payment.InvoiceContactOverride != null)
			{
				constructedValueObject.TxnOverrideContact = new ContactValueObjectHelper("").ExportToValueObject(payment.InvoiceContactOverride, context);
			}

			AccTransactionMatchLink paymentMatchLink = GetPaymentMatchLink(payment);
			AccTransactionMatchLink[] allMatchLinks = GetAllMatchlinksExcludingEXX(payment, paymentMatchLink);
			ZDecimal matchLinkTotal = 0m;
			foreach (AccTransactionMatchLink matchLink in allMatchLinks)
			{
				matchLinkTotal += matchLink.AP_Amount;
			}

			foreach (AccTransactionMatchLink matchLink in allMatchLinks)
			{
				TransactionHeader header = payment.Factory.Load<TransactionHeader>(matchLink.AP_AH);

				if (ShouldIncludeThisTransaction(header.AH_TransactionType))
				{
					Xsd.TxnHeader paidTransaction = constructedValueObject.PaidTransactions.AddNew();

					paidTransaction.DebtorOrCreditor = OrganisationAdapter.ExportToValueObject(header.Header, context);
					paidTransaction.TxnType = TxnHeaderMapper.GetTxnHeaderTxnType(header.AH_TransactionType);
					paidTransaction.Ledger = TxnHeaderMapper.GetTxnHeaderLedger(header.AH_Ledger);
					paidTransaction.TxnNumber = header.AH_TransactionNum;
					paidTransaction.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(header.AH_OSTotalAmount, header.TransactionCurrency, header.GetType());
					paidTransaction.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(header.AH_OSExTaxAmount, header.TransactionCurrency, header.GetType());
					paidTransaction.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(header.AH_LocalTotalAmount, GlbCompany.CurrentCompany.LocalCurrency, header.GetType());
					paidTransaction.LocalInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(header.AH_LocalExTaxAmount, GlbCompany.CurrentCompany.LocalCurrency, header.GetType());
					paidTransaction.Description = header.AH_Desc;
					paidTransaction.InvoiceDate = header.AH_InvoiceDate;
					paidTransaction.DueDate = header.AH_DueDate;
					paidTransaction.PostDate = header.AH_PostDate;

					paidTransaction.Branch = header.Branch.GB_Code;
					paidTransaction.Department = header.Department.GE_Code;

					APInvoice headerAsAPInvoice = header as APInvoice;
					if (headerAsAPInvoice != null && !headerAsAPInvoice.ContainerDetailsForCompayExport.ContainerNumber.IsEmpty)
					{
						constructedValueObject.ENettStoragePaymentDetails = new Xsd.ENettStoragePaymentDetails();
						constructedValueObject.ENettStoragePaymentDetails.IsSpecified = true;
						constructedValueObject.ENettStoragePaymentDetails.ContainerReference = headerAsAPInvoice.ContainerDetailsForCompayExport.ContainerNumber;
						constructedValueObject.ENettStoragePaymentDetails.TerminalCode = headerAsAPInvoice.ContainerDetailsForCompayExport.TerminalCode;
						constructedValueObject.ENettStoragePaymentDetails.PickupDate = headerAsAPInvoice.ContainerDetailsForCompayExport.PickupDate;
					}

					ZDecimal amountPaidInPaymentCurrency = 0m;

					if (!payment.AH_FullyPaidDate.IsEmpty) // this means that the payment amount doesn't need to be converted from local into the payment currency and the
					{
						if (payment.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							amountPaidInPaymentCurrency = matchLink.AP_Amount;
						}
						else
						{
							amountPaidInPaymentCurrency = TransactionMatchLinkOSAmountProvider.GetOSPaidAmountForPaymentDataAdapter(matchLink, matchLinkTotal, payment.AH_OSTotalAmount);
						}
					}
					else // this means that there is a part payment situation
					{
						var paymentMatchAmountInPaymentCurrency = Env.CurrentCompany.ExchangeRate.LocalToForeign(paymentMatchLink.AP_Amount, payment.AH_ExchangeRate, payment.AH_RX_NKTransactionCurrency);
						amountPaidInPaymentCurrency = TransactionMatchLinkOSAmountProvider.GetOSPaidAmountForPaymentDataAdapter(matchLink, matchLinkTotal, paymentMatchAmountInPaymentCurrency);
					}
					paidTransaction.AmountPaidThisPayment = TxnHeaderMapper.GetXmlFinancialValue(amountPaidInPaymentCurrency, payment.TransactionCurrency, header.GetType());
				}
			}
		}

		AccTransactionMatchLink GetPaymentMatchLink(Payment payment)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionMatchLink));
			if (payment != null)
			{
				ZString whereClause = AccTransactionMatchLinkSchema.Constants.PK + @" =
					(SELECT TOP 1 " + AccTransactionMatchLinkSchema.Constants.PK + @" from
						" + AccTransactionMatchLinkSchema.Constants.SqlSchemaName + "." + AccTransactionMatchLinkSchema.Constants.TableName + @" join
						" + StmALogSchema.Constants.SqlSchemaName + "." + StmALogSchema.Constants.TableName + @"
							ON " + AccTransactionMatchLinkSchema.Constants.PK + @" = " + StmALogSchema.Constants.SL_Parent +
						@" AND " + StmALogSchema.Constants.SL_SE_NKEvent + @" = 'ADD'
						WHERE " + AccTransactionMatchLinkSchema.Constants.AP_AH + @" = @PaymentPK
						ORDER BY " + StmALogSchema.Constants.SL_PostedTimeUtc + " DESC)";

				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add("@PaymentPK", payment.PK, AccTransactionMatchLinkSchema.AP_AH);
				query.AddFilterAndZSQLParameterCollection(whereClause, @params);
			}
			else
			{
				query.IsNoResultQuery = true;
			}
			AccTransactionMatchLink paymentMatchLink = payment.Factory.LoadTop1<AccTransactionMatchLink>(query);
			return paymentMatchLink;
		}

		AccTransactionMatchLink[] GetAllMatchlinksExcludingEXX(Payment payment, AccTransactionMatchLink paymentMatchLink)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionMatchLink));
			if (paymentMatchLink != null)
			{
				ZString whereClause = AccTransactionMatchLinkSchema.PK.Name + " IN (Select " + AccTransactionMatchLinkSchema.PK.Name +
	" from " + AccTransactionMatchLinkSchema.Constants.SqlSchemaName + "." + AccTransactionMatchLinkSchema.Constants.TableName + " join " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName +
	@" on ap_ah = ah_pk join
                                    dbo.glbbranch on ah_gb = gb_pk
                                    where ap_matchgroupnum = @MatchGroupNum AND
                                    GB_GC = @CurrentCompany AND
                                    AP_PK != @PaymentMatchLinkPK AND
                                    AH_TransactionType != 'EXX')";
				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add("@MatchGroupNum", paymentMatchLink.AP_MatchGroupNum, AccTransactionMatchLinkSchema.AP_MatchGroupNum);
				@params.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC);
				@params.Add("@PaymentMatchLinkPK", paymentMatchLink.PK, AccTransactionMatchLinkSchema.PK);

				query.AddFilterAndZSQLParameterCollection(whereClause, @params);
			}
			else
			{
				query.IsNoResultQuery = true;
			}
			AccTransactionMatchLink[] paymentMatchlinks = payment.Factory.Load<AccTransactionMatchLink>(query);
			return paymentMatchlinks;
		}

		protected override void ImportFromValueObjectCore(Payment bizObj, Xsd.TxnHeader value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		protected virtual bool ShouldIncludeThisTransaction(ZString transactionType)
		{
			return true;
		}
	}
}
