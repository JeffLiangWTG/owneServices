using System.ComponentModel;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.XmlMapping;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using InvConstants = Enterprise.Accounting.DataTransfer.Invoices.FlatFile.DataTransferInvoiceConstants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile
{
	public class TxnHeaderFlatFileConverter : FlatFileConverter
	{
		public TxnHeaderFlatFileConverter(INotifications subscribeNotification, BusinessObjectFactory factory) : base(subscribeNotification, factory)
		{
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			base.MapImport(valueObject, fileLines);
			CurrencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var txnHeaderCollection = (Xsd.TxnHeaderCollection)valueObject;
			var txnHeader = null as Xsd.TxnHeader;
			var lastValidatedTxnHeader = null as Xsd.TxnHeader;
			var lastProcessedTxnLine = null as Xsd.TxnLine;

			foreach (FlatFileDataRow row in fileLines)
			{
				string rowType = row[0];

				switch (rowType)
				{
					case InvConstants.RowTypes.JobInformation:
						ProcessJobInformationRow(row);
						break;

					case InvConstants.RowTypes.InvoiceHeader:
						txnHeader = txnHeaderCollection.AddNew();
						ProcessInvoiceHeaderRow(txnHeader, row);
						break;

					case InvConstants.RowTypes.InvoiceLine:
						ValidInvoiceHeaderRow();
						lastProcessedTxnLine = ProcessInvoiceLineRow(txnHeader, row);
						break;

					case InvConstants.RowTypes.InvoiceLineSubAccount:
						ProcessMultipleSubAccounts(lastProcessedTxnLine, row);
						break;

					case InvConstants.RowTypes.Attachment:
						ProcessAttachmentRow(txnHeader, row);
						break;

					default:
						ProcessUnRecognisedRow(row);
						break;
				}

				void ValidInvoiceHeaderRow()
				{
					if (lastValidatedTxnHeader != txnHeader && txnHeader != null)
					{
						if (txnHeader.TxnType == Xsd.TxnType.ADJ && CurrentJobInformation != null)
						{
							Notification.Notify(new ErrorNotification(
								ErrorType.DataTypeConversionError,
								Res.GetString("DAD14428-84F9-4D4E-AA5F-7C7F79A9E447", "Adjustments cannot be imported when JOBINFO is not NIL."))
							);
						}

						lastValidatedTxnHeader = txnHeader;
					}
				}
			}
		}

		void ProcessJobInformationRow(FlatFileDataRow jobInformationRow)
		{
			if (jobInformationRow[InvConstants.JobInfoPos.JobType] != "NIL")
			{
				CurrentJobInformation = jobInformationRow;
			}
			else
			{
				CurrentJobInformation = null;
			}
		}

		void ProcessUnRecognisedRow(FlatFileDataRow row)
		{
			string message = Res.GetString("79a8a5a6-37f6-4ca9-839e-72cb69dc651a", "Unrecognized row type detected. Only") + " ";
			message += InvConstants.RowTypes.JobInformation + ", ";
			message += InvConstants.RowTypes.InvoiceHeader + ", ";
			message += InvConstants.RowTypes.InvoiceLine + " & ";
			message += Res.GetString("6b36d3b6-2397-411b-aca2-f3cdd0d7a94f", "{0} row types are used for import.", InvConstants.RowTypes.Attachment);
			message += Res.GetString("10633b48-b191-4b3a-9348-c0f9686336b3", "The following row will be ignored:") + " " + row.GetField(0) + System.Environment.NewLine;

			if (Notification != null)
			{
				Notification.Notify(new WarningNotification(WarningType.Warning, message));
			}
		}

		#region Header Processing

		bool IsInclTaxTotalAmountSpecifiedInHeader;

		void ProcessInvoiceHeaderRow(Xsd.TxnHeader txnHeader, FlatFileDataRow invoiceHeaderRow)
		{
			ZString newCurrencyCode = invoiceHeaderRow[InvConstants.InvHeaderPos.Currency];

			if (!newCurrencyCode.IsEmpty)
			{
				CurrencyCode = newCurrencyCode;
			}
			TxnHeaderBuilder.SetLedger(txnHeader, invoiceHeaderRow[InvConstants.InvHeaderPos.Ledger], NotificationManager);
			TxnHeaderBuilder.SetDebtorOrCreditor(txnHeader, invoiceHeaderRow[InvConstants.InvHeaderPos.OrganisationCode], invoiceHeaderRow[InvConstants.InvHeaderPos.OrganisationName]);
			TxnHeaderBuilder.SetTransactionType(txnHeader, invoiceHeaderRow[InvConstants.InvHeaderPos.TxnType], NotificationManager);
			TxnHeaderBuilder.SetTransactionNum(txnHeader, invoiceHeaderRow[InvConstants.InvHeaderPos.TxnNumber]);
			TxnHeaderBuilder.SetDescription(txnHeader, invoiceHeaderRow[InvConstants.InvHeaderPos.TxnDescription]);
			TxnHeaderBuilder.SetPostDate(txnHeader, GetZDateTimeFromString(invoiceHeaderRow[InvConstants.InvHeaderPos.PostDate], nameof(InvConstants.InvHeaderPos.PostDate)));
			TxnHeaderBuilder.SetInvoiceDate(txnHeader, GetZDateTimeFromString(invoiceHeaderRow[InvConstants.InvHeaderPos.InvoiceDate], nameof(InvConstants.InvHeaderPos.InvoiceDate)), NotificationManager);
			TxnHeaderBuilder.SetDueDate(txnHeader, GetZDateTimeFromString(invoiceHeaderRow[InvConstants.InvHeaderPos.DueDate], nameof(InvConstants.InvHeaderPos.DueDate)));
			TxnHeaderBuilder.SetOverrideSystemExchangeRate(txnHeader, invoiceHeaderRow.GetFieldAsZBool(InvConstants.InvHeaderPos.OverrideSystemExchangeRate));

			decimal invoiceTotal = invoiceHeaderRow.GetFieldAsZDecimal(InvConstants.InvHeaderPos.InvoiceTotal);
			if (invoiceTotal == 0m)
			{
				IsInclTaxTotalAmountSpecifiedInHeader = false;
			}
			else
			{
				IsInclTaxTotalAmountSpecifiedInHeader = true;
			}
			TxnHeaderBuilder.SetOsInvoiceAmtInclTax(txnHeader, invoiceTotal, CurrencyCode, Factory, NotificationManager);

			TxnHeaderBuilder.SetOsInvoiceAmtExclTax(txnHeader, 0m, CurrencyCode, Factory, NotificationManager);
			TxnHeaderBuilder.SetOsTaxAmount(txnHeader, 0m, CurrencyCode, Factory, NotificationManager);

			TxnHeaderBuilder.SetLocalInvoiceAmtExclTax(txnHeader, 0m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Factory, NotificationManager);

			if (invoiceHeaderRow.FieldCount - 1 >= InvConstants.InvHeaderPos.DefaultBranch)
			{
				TxnHeaderBuilder.SetBranchCode(txnHeader, invoiceHeaderRow[InvConstants.InvHeaderPos.DefaultBranch]);
			}
			if (invoiceHeaderRow.FieldCount - 1 >= InvConstants.InvHeaderPos.DefaultDepartment)
			{
				TxnHeaderBuilder.SetDepartmentCode(txnHeader, invoiceHeaderRow[InvConstants.InvHeaderPos.DefaultDepartment]);
			}
			if (invoiceHeaderRow.FieldCount - 1 >= InvConstants.InvHeaderPos.PaymentReference)
			{
				TxnHeaderBuilder.SetPaymentReference(txnHeader, invoiceHeaderRow[InvConstants.InvHeaderPos.PaymentReference]);
			}
			if (invoiceHeaderRow.FieldCount - 1 >= InvConstants.InvHeaderPos.IsDisbursement)
			{
				TxnHeaderBuilder.SetIsDisbursement(txnHeader, invoiceHeaderRow[InvConstants.InvHeaderPos.IsDisbursement]);
			}
		}

		#endregion

		void ProcessAttachmentRow(Xsd.TxnHeader txnHeader, FlatFileDataRow attachmentRow)
		{
			if (txnHeader != null)
			{
				Xsd.TxnHeaderAttachment attachment = txnHeader.Attachments.AddNew();
				attachment.FileName = attachmentRow[InvConstants.AttachmentPos.FileName];
				attachment.FilePath = attachmentRow[InvConstants.AttachmentPos.FilePath];
				attachment.DocumentType = attachmentRow[InvConstants.AttachmentPos.DocumentType];

				if (!attachment.DocumentType.IsEmpty && DocTypes.Count > 0)
				{
					bool isDocumentTypeInList = false;
					foreach (RefDocType type in DocTypes.Find(new ZQuery(RefDocTypeSchema.RT_DocType, attachment.DocumentType)))
					{
						isDocumentTypeInList = true;
						break;
					}

					if (!isDocumentTypeInList)
					{
						StringBuilder builder = new StringBuilder();
						builder.AppendLine(Res.GetString("C41680BC-6F51-4d08-A320-968B5FA1221F", "The Document Type you supplied ({0}) is not recognized by this import process.", attachment.DocumentType) + " ");
						builder.Append(Res.GetString("21FBBB02-3B69-4b92-BDDB-A9AD8B3393FD", "You can only use Document Types with a category of 'ACC - Accounting' or 'ALL'.") + " ");
						builder.Append(Res.GetString("1BD0662E-1CF0-4afc-9488-9CD385FEDEB7", "Please use one of the following Document Types:") + " ");
						for (int i = 0; i < DocTypes.Count; i++)
						{
							builder.Append(DocTypes[i].RT_DocType + ", ");
						}
						Notification.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, builder.ToString().TrimEnd(' ', ',')));
					}
				}
			}
		}

		RefDocTypeCollection DocTypes
		{
			get
			{
				if (fDocTypes == null)
				{
					ZQuery docTypeQuery = new DocTypeCategoryQuery(Factory, Core.Constants.ReferenceTypes.Accounting);
					ZQuery visibleQuery = new ZQuery(RefDocTypeSchema.RT_IsActive, ZBool.True);
					docTypeQuery.AddToFilter(visibleQuery, JoinCondition.And);

					fDocTypes = new RefDocTypeCollection(Factory, docTypeQuery);
					fDocTypes.ApplySort(RefDocTypeSchema.RT_DocType.Name, ListSortDirection.Ascending);
				}
				return fDocTypes;
			}
		}

		RefDocTypeCollection fDocTypes;

		#region Line Processing

		Xsd.TxnLine ProcessInvoiceLineRow(Xsd.TxnHeader txnHeader, FlatFileDataRow invoiceLineRow)
		{
			Xsd.TxnLine txnLine = null;

			if (txnHeader != null)
			{
				txnLine = txnHeader.TxnLines.AddNew();

				SetLineJobDetails(txnLine, invoiceLineRow);
				SetLineDescription(txnLine, invoiceLineRow);
				SetLineChargeCode(txnLine, invoiceLineRow);
				SetLineGLAccount(txnLine, invoiceLineRow);
				SetLineBranch(txnLine, invoiceLineRow);
				SetLineDepartment(txnLine, invoiceLineRow);
				SetLineTaxCode(txnLine, invoiceLineRow);
				SetLineAmounts(txnLine, txnHeader, invoiceLineRow);
				SetLineIsFinal(txnLine, invoiceLineRow);
				SetSubAccountForBackwardCompatibility(txnLine, invoiceLineRow);
			}

			return txnLine;
		}

		void ProcessMultipleSubAccounts(Xsd.TxnLine txnLine, FlatFileDataRow invoiceLineRow)
		{
			if (txnLine == null)
			{
				Notification.Notify(new ErrorNotification(ErrorType.DataTypeConversionError,
					Res.GetString("636A1DC4-BB2A-4eba-83B8-A60AD0F79775", "There is a sub account which does not belong to any transaction line in this CSV file.")));
				return;
			}

			if (!invoiceLineRow[InvConstants.InvoiceLineSubAccountPos.SubAccountType].IsEmpty)
			{
				var subAccount = txnLine.SubAccounts.AddNew();
				subAccount.Type.Code = invoiceLineRow[InvConstants.InvoiceLineSubAccountPos.SubAccountType];
				subAccount.Code = invoiceLineRow[InvConstants.InvoiceLineSubAccountPos.SubAccountCode];
			}
		}

		void SetLineJobDetails(Xsd.TxnLine txnLine, FlatFileDataRow invoiceLineRow)
		{
			if (CurrentJobInformation != null)
			{
				txnLine.ConsolOrJobNo = CurrentJobInformation[InvConstants.JobInfoPos.JobNumber];
				string consolOrJobType = CurrentJobInformation[InvConstants.JobInfoPos.JobType];

				string enterpriseJobCode = TransactionLineConsolOrJobTypeXmlMapping.Instance.GetEnterpriseCode(consolOrJobType, "", Notification);

				if (TransactionLineConsolOrJobTypeXmlMapping.Instance.ContainsEnterpriseCode(enterpriseJobCode))
				{
					txnLine.ConsolOrJobType = TransactionLineConsolOrJobTypeXmlMapping.Instance.GetExternalCode(enterpriseJobCode, "", Notification);

					if (txnLine.ConsolOrJobType == Xsd.TxnLineConsolOrJobType.CSL || txnLine.ConsolOrJobType == Xsd.TxnLineConsolOrJobType.SHP)
					{
						txnLine.MasterBillNo = CurrentJobInformation[InvConstants.JobInfoPos.Reference1];
					}

					if (txnLine.ConsolOrJobType == Xsd.TxnLineConsolOrJobType.SHP)
					{
						txnLine.HouseBIllNo = CurrentJobInformation[InvConstants.JobInfoPos.Reference2];
					}

					if (txnLine.ConsolOrJobType == Xsd.TxnLineConsolOrJobType.CSL)
					{
						string apportionmentMethod = CurrentJobInformation[InvConstants.JobInfoPos.ApportionmentMethod];
						if (!string.IsNullOrEmpty(apportionmentMethod))
						{
							string enterpriseApportionmentCode = ApportionmentMethodXmlMapping.Instance.GetEnterpriseCode(apportionmentMethod, "", Notification);

							if (ApportionmentMethodXmlMapping.Instance.ContainsEnterpriseCode(enterpriseApportionmentCode))
							{
								txnLine.ConsolApportionmentMethod = ApportionmentMethodXmlMapping.Instance.GetExternalCode(enterpriseApportionmentCode, "", Notification);
							}
							else
							{
								Notification.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, Res.GetString("3e1a9259-6835-47ec-9a6d-94fb162f4c70", "The Apportionment Method you supplied ({0}) is not valid.", apportionmentMethod)));
							}
						}
					}
				}
				else
				{
					txnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.UNK;

					StringBuilder builder = new StringBuilder();
					builder.AppendLine(Res.GetString("d84a5fe7-8f94-45bb-ac62-70b7629799d5", "The Job Type you supplied ({0}) is not recognized by {1}.", consolOrJobType, Core.Constants.ProductName) + " ");
					builder.Append(Res.GetString("af58eac2-bedb-48b0-92a7-18a059aa21c2", "Please use one of the following Job Types:") + " ");
					foreach (var mapping in TransactionLineConsolOrJobTypeXmlMapping.Instance)
					{
						builder.Append(mapping.ExternalCode + ", ");
					}
					Notification.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, builder.ToString().TrimEnd(' ', ',')));
				}

				txnLine.ConsolOrJobTypeSpecified = true;
			}
		}

		void SetLineDescription(Xsd.TxnLine txnLine, FlatFileDataRow invoiceLineRow)
		{
			if (!invoiceLineRow[InvConstants.InvLinePos.Description].IsEmpty)
			{
				txnLine.Description = invoiceLineRow[InvConstants.InvLinePos.Description];
			}
		}

		void SetLineChargeCode(Xsd.TxnLine txnLine, FlatFileDataRow invoiceLineRow)
		{
			txnLine.ChargeCode = invoiceLineRow[InvConstants.InvLinePos.ChargeCode];
		}

		void SetLineGLAccount(Xsd.TxnLine txnLine, FlatFileDataRow invoiceLineRow)
		{
			txnLine.GLAccount = invoiceLineRow[InvConstants.InvLinePos.GLAccount];
		}

		void SetLineBranch(Xsd.TxnLine txnLine, FlatFileDataRow invoiceLineRow)
		{
			if (!invoiceLineRow[InvConstants.InvLinePos.Branch].IsEmpty)
			{
				txnLine.Branch = invoiceLineRow[InvConstants.InvLinePos.Branch];
			}
		}

		void SetLineDepartment(Xsd.TxnLine txnLine, FlatFileDataRow invoiceLineRow)
		{
			if (!invoiceLineRow[InvConstants.InvLinePos.Department].IsEmpty)
			{
				txnLine.Department = invoiceLineRow[InvConstants.InvLinePos.Department];
			}
		}

		void SetLineTaxCode(Xsd.TxnLine txnLine, FlatFileDataRow invoiceLineRow)
		{
			if (!invoiceLineRow[InvConstants.InvLinePos.TaxCode].IsEmpty)
			{
				txnLine.TaxCode = invoiceLineRow[InvConstants.InvLinePos.TaxCode];
			}
		}

		void SetLineAmounts(Xsd.TxnLine txnLine, Xsd.TxnHeader txnHeader, FlatFileDataRow invoiceLineRow)
		{
			txnLine.OsTaxAmount = GetZeroFinancialValue(CurrencyCode);
			txnLine.OsInvoiceAmtExclTax = GetZeroFinancialValue(CurrencyCode);
			txnLine.OsInvoiceAmtInclTax = GetZeroFinancialValue(CurrencyCode);

			txnLine.LocalTaxAmount = GetZeroFinancialValue(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			txnLine.LocalInvoiceAmtExclTax = GetZeroFinancialValue(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			txnLine.LocalInvoiceAmtInclTax = GetZeroFinancialValue(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			if (!invoiceLineRow[InvConstants.InvLinePos.TaxAmount].IsEmpty)
			{
				txnLine.OsTaxAmount = GetFinancialValue(invoiceLineRow[InvConstants.InvLinePos.TaxAmount], txnHeader, CurrencyCode);
			}

			if (!invoiceLineRow[InvConstants.InvLinePos.InvoiceAmtExclTax].IsEmpty)
			{
				txnLine.OsInvoiceAmtExclTax = GetFinancialValue(invoiceLineRow[InvConstants.InvLinePos.InvoiceAmtExclTax], txnHeader, CurrencyCode);
			}

			if (!invoiceLineRow[InvConstants.InvLinePos.InvoiceAmtIncTax].IsEmpty)
			{
				txnLine.OsInvoiceAmtInclTax = GetFinancialValue(invoiceLineRow[InvConstants.InvLinePos.InvoiceAmtIncTax], txnHeader, CurrencyCode);
			}

			if (!invoiceLineRow[InvConstants.InvLinePos.LocalInvoiceAmtExclTax].IsEmpty)
			{
				txnLine.LocalInvoiceAmtExclTax = GetFinancialValue(invoiceLineRow[InvConstants.InvLinePos.LocalInvoiceAmtExclTax], txnHeader, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			}

			ZDecimal amountExcludingTax = txnLine.OsInvoiceAmtInclTax.Value - txnLine.OsTaxAmount.Value;
			ZDecimal amountIncludingTax = txnLine.OsInvoiceAmtExclTax.Value + txnLine.OsTaxAmount.Value;

			if (txnLine.OsInvoiceAmtExclTax.Value == 0 && amountExcludingTax != 0)
			{
				txnLine.OsInvoiceAmtExclTax.Value = amountExcludingTax;
			}
			else if (txnLine.OsInvoiceAmtInclTax.Value == 0 && amountIncludingTax != 0)
			{
				txnLine.OsInvoiceAmtInclTax.Value = amountIncludingTax;
			}

			if (!IsInclTaxTotalAmountSpecifiedInHeader)
			{
				txnHeader.OsInvoiceAmtInclTax.Value += txnLine.OsInvoiceAmtInclTax.Value;
			}
			txnHeader.OsInvoiceAmtExclTax.Value += txnLine.OsInvoiceAmtExclTax.Value;
			txnHeader.OsTaxAmount.Value += txnLine.OsTaxAmount.Value;
			txnHeader.LocalInvoiceAmtExclTax.Value += txnLine.LocalInvoiceAmtExclTax.Value;
		}

		void SetLineIsFinal(Xsd.TxnLine txnLine, FlatFileDataRow invoiceLineRow)
		{
			if (invoiceLineRow[InvConstants.InvLinePos.IsFinalCharge] == ZBool.True.ToString())
			{
				txnLine.IsFinalCharge = true;
				txnLine.IsFinalChargeSpecified = true;
			}
			else if (invoiceLineRow[InvConstants.InvLinePos.IsFinalCharge] == ZBool.False.ToString())
			{
				txnLine.IsFinalCharge = false;
				txnLine.IsFinalChargeSpecified = true;
			}
		}

		void SetSubAccountForBackwardCompatibility(Xsd.TxnLine txnLine, FlatFileDataRow invoiceLineRow)
		{
			if (!invoiceLineRow[InvConstants.InvLinePos.SubAccountCode].IsEmpty)
			{
				var subAccount = txnLine.SubAccounts.AddNew();
				subAccount.Type.Code = invoiceLineRow[InvConstants.InvLinePos.SubAccountType];
				subAccount.Code = invoiceLineRow[InvConstants.InvLinePos.SubAccountCode];
			}
		}

		#endregion

		Xsd.FinancialValue GetFinancialValue(ZString amountAsString, Xsd.TxnHeader txnHeader, ZString currencyCode)
		{
			ZDecimal amount = ZDecimal.ParseSafe(amountAsString, 0);
			ZDecimal result = amount * TxnHeaderMapper.MultiplierForImportAndExport(TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));
			return Xsd.FinancialValue.FromAmountAndCurrencyCode(result, currencyCode);
		}

		Xsd.FinancialValue GetZeroFinancialValue(ZString currencyCode)
		{
			var result = Xsd.FinancialValue.FromAmountAndCurrencyCode(ZDecimal.Zero, currencyCode);
			result.IsSpecified = false;
			return result;
		}

		ZDateTime GetZDateTimeFromString(ZString dateTimeAsString, ZString propertyName)
		{
			var result = ZDateTime.Empty;
			if (!string.IsNullOrEmpty(dateTimeAsString))
			{
				var validFormat = "yyyyMMdd";
				var isValidFormat = ZDateTime.TryParseExact(dateTimeAsString, out result, validFormat);
				if (!isValidFormat)
				{
					Notification.Notify(new ErrorNotification(ErrorType.DataTypeConversionError, Res.GetString("1E3006E4-81B8-433B-87F9-0BF4D794BF3E", "{0} format should be {1}.", propertyName, validFormat)));
				}
			}

			return result;
		}

		NotificationManager NotificationManager
		{
			get { return notificationManager_internal ?? (notificationManager_internal = new NotificationManager(Notification)); }
		}

		NotificationManager notificationManager_internal;

		FlatFileDataRow CurrentJobInformation;
		ZString CurrencyCode;
	}
}
