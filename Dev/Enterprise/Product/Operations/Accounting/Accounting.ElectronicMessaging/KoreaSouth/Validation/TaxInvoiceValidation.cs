using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class TaxInvoiceValidation
	{
		public virtual void ValidateTaxInvoice(INotifications validationNotifier, AdditionalInfo additionalInfo, TransactionInfo transactionInfo)
		{
			ValidateInvoicerError(validationNotifier, additionalInfo);
			ValidateInvoiceeError(validationNotifier, additionalInfo);
			ValidateTransactionDateError(validationNotifier, additionalInfo, transactionInfo);
			ValidateAmendment(validationNotifier, additionalInfo);
		}

		void ValidateInvoicerError(INotifications validationNotifier, IInvoicerPartyAdditionalInfo additionalInfo)
		{
			if (additionalInfo.ID.Length != 10)
			{
				validationNotifier.AddError(Res.GetString("A6E08E57-EFD9-4B73-B348-7DF70D65C979", "Invalid KR VAT registration number recorded against the Company Organization Proxy. It should be in format 'NNNNNNNNNN'."));
			}

			if (string.IsNullOrEmpty(additionalInfo.NameText))
			{
				validationNotifier.AddError(Res.GetString("c06ad500-8e81-4aaa-bb5e-fb09fde43194", "Company Name of the Current Login Company's organization proxy cannot left as empty."));
			}
			else if (additionalInfo.NameText.Length > 200)
			{
				validationNotifier.AddError(Res.GetString("d4acd3fc-5863-4d0f-9b8e-bd53c2fc5378", "Company Name of the Current Login Company's organization proxy exceeded the maximum length of 200 characters."));
			}

			if (string.IsNullOrEmpty(additionalInfo.SpecifiedPersonNameText))
			{
				validationNotifier.AddError(Res.GetString("1C7E7B5E-406B-4C08-B75F-4A1B2CFD057C", "Please add a contact against the Company Organization Proxy with 'KRC - Korea Company Representative' allocated contact type."));
			}

			if (additionalInfo.SpecifiedAddressLineOneText.Length > 300)
			{
				validationNotifier.AddError(Res.GetString("eb6803e1-e2f5-416d-8c34-3789ec757793", "Company Address of the Current Login Company's organization proxy exceeded the maximum length of 300 characters."));
			}

			if (additionalInfo.TypeCode.Length > 100)
			{
				validationNotifier.AddError(Res.GetString("cf730926-c361-4ed4-b77f-ea1a0f21fa4d", "Korea Business Principal Activity Type (KBT) recorded against the Current Login Company's organization proxy exceeded the maximum length of 100 characters."));
			}

			if (additionalInfo.ClassificationCode.Length > 100)
			{
				validationNotifier.AddError(Res.GetString("9b260a2f-b6e9-470a-b864-ba15966a25aa", "Korea Business Principal Industry Category (KBC) recorded against the Current Login Company's organization proxy exceeded the maximum length of 100 characters."));
			}

			if (additionalInfo.DefinedContactPersonName.Length > 100)
			{
				validationNotifier.AddError(Res.GetString("5190d74c-7dad-4a12-a5b6-d8c9b186eb0b", "Contact Staff Name of the Current Login Company exceeded the maximum length of 100 characters."));
			}

			if (additionalInfo.DefinedContactURICommunication.Length > 100)
			{
				validationNotifier.AddError(Res.GetString("42f5af3b-91bb-48bb-b8af-8f15ff6194e7", "Contact Staff Email Address of the Current Login Company exceeded the maximum length of 100 characters."));
			}

			if (!string.IsNullOrEmpty(additionalInfo.TaxRegistrationID) && additionalInfo.TaxRegistrationID.Length != 4)
			{
				validationNotifier.AddError(Res.GetString("19EDA1AE-770F-42D9-BC25-A248B2465DB4", "Invalid KR Office ID (Code:08) recorded against the Company Organization Proxy. It should be in format 'NNNN'."));
			}
		}

		void ValidateInvoiceeError(INotifications validationNotifier, IInvoiceePartyAdditionalInfo additionalInfo)
		{
			if (additionalInfo.BusinessTypeCode == ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfBusinessOperator
				&& additionalInfo.ID.Length != 10)
			{
				validationNotifier.AddError(Res.GetString("78ED57B9-3E97-429D-B245-8EA7A99E1017", "Invalid KR VAT registration number recorded against the Debtor Organization. It should be in format 'NNNNNNNNNN'."));
			}
			else if (additionalInfo.BusinessTypeCode == ReadyKoreaConstants.InvoiceePartyBusinessTypeCode.RegistrationNumberOfResident
				&& additionalInfo.ID.Length != 13)
			{
				validationNotifier.AddError(Res.GetString("F513A439-8639-4623-9F35-048E66EE009C", "Invalid KR Citizen Registration Number (Code:01) of the Debtor Organization. It should be in format 'NNNNNNNNNNNNN'."));
			}
			else if (string.IsNullOrEmpty(additionalInfo.BusinessTypeCode) && string.IsNullOrEmpty(additionalInfo.ID))
			{
				validationNotifier.AddError(Res.GetString("564C510A-D5A8-4146-BDC4-10E0F3B0EAAB", "Invoice Debtor does not have a valid Korea Government VAT Code ('VAT'), Citizen Registration Number ('01') or Foreigner Registration Number ('03'). Please specify a value then re-queue the invoice."));
			}

			if (string.IsNullOrEmpty(additionalInfo.NameText))
			{
				validationNotifier.AddError(Res.GetString("f77459f8-a7e5-4ed6-8bac-7584be047376", "Company Name of the Debtor Organization cannot be empty."));
			}
			else if (additionalInfo.NameText.Length > 200)
			{
				validationNotifier.AddError(Res.GetString("92ffed87-4f40-4a1d-b55b-47f7830202a9", "Company Name of the Debtor Organization exceeded the maximum length of 200 characters."));
			}

			if (string.IsNullOrEmpty(additionalInfo.SpecifiedPersonNameText))
			{
				validationNotifier.AddError(Res.GetString("1DEA55E6-5981-44DB-9C6F-F39FA1B742B3", "Please add a contact against the Debtor Organization with 'KRC - Korea Company Representative' allocated contact type."));
			}

			if (additionalInfo.SpecifiedAddressLineOneText.Length > 300)
			{
				validationNotifier.AddError(Res.GetString("4a329445-0cb0-4619-a816-8da3cc716652", "Company Address of the Debtor Organization exceeded the maximum length of 300 characters."));
			}

			if (additionalInfo.TypeCode.Length > 100)
			{
				validationNotifier.AddError(Res.GetString("688a7b9e-8703-4a08-bbb9-2aa0f704ed50", "Korea Business Principal Activity Type (KBT) recorded against the Debtor Organization exceeded the maximum length of 100 characters."));
			}

			if (additionalInfo.ClassificationCode.Length > 100)
			{
				validationNotifier.AddError(Res.GetString("ad50eaf8-ea30-4350-aabb-c22aa0d07782", "Korea Business Principal Industry Category (KBC) recorded against the Debtor Organization exceeded the maximum length of 100 characters."));
			}

			if (additionalInfo.PrimaryDefinedContactPersonName.Length > 100 || additionalInfo.SecondaryDefinedContactPersonName.Length > 100)
			{
				validationNotifier.AddError(Res.GetString("e36ed0fa-6354-4bcd-82a4-9afc70cb25ba", "Contact Person Name of the Debtor Organization exceeded the maximum length of 100 characters."));
			}

			if (additionalInfo.PrimaryDefinedContactURICommunication.Length > 100 || additionalInfo.SecondaryDefinedContactURICommunication.Length > 100)
			{
				validationNotifier.AddError(Res.GetString("342acd4d-39d9-4f30-8850-854bb21b8c8c", "Contact Person Email Address of the Debtor Organization exceeded the maximum length of 100 characters."));
			}

			if (!string.IsNullOrEmpty(additionalInfo.TaxRegistrationID) && additionalInfo.TaxRegistrationID.Length != 4)
			{
				validationNotifier.AddError(Res.GetString("1D5653CC-5B52-4D35-87AE-42F938519C61", "Invalid KR Office ID (Code:08) recorded against the Debtor Organization. It should be in format 'NNNN'."));
			}
		}

		void ValidateTransactionDateError(INotifications validationNotifier, AdditionalInfo additionalInfo, TransactionInfo transactionInfo)
		{
			var invoiceDate = transactionInfo.TransactionDate.Value;
			if (invoiceDate.Date > ZDate.Today)
			{
				validationNotifier.AddError(Res.GetString("3CC6A8CE-24CD-4412-A373-91834AC34809", "The invoice date cannot be a future date."));
			}

			var dates = additionalInfo.Lines?.Select(x => x.ReverseDate).Append(invoiceDate).ToArray() ?? Array.Empty<ZDateTime>();

			if (string.IsNullOrWhiteSpace(additionalInfo.OriginalIssueID) && !AccountingUtils.AreDatesInTheSameCalendarMonth(dates))
			{
				validationNotifier.AddError(Res.GetString("E3151C23-1087-4FF4-AB3A-7EF0AF30E36D", "The month of the invoice date and the revenue recognition date must be the same."));
			}
		}

		void ValidateAmendment(INotifications validationNotifier, AdditionalInfo additionalInfo)
		{
			if (string.IsNullOrEmpty(additionalInfo.FullTypeCode))
			{
				validationNotifier.AddError(Res.GetString("0FA2AE81-0D66-49B3-82DB-57729B0B1DA4", "Invalid Tax Invoice Document's Type Code. Please raise eRequest for assistance."));
			}
			else if (additionalInfo.FullTypeCode.StartsWith("02") || additionalInfo.FullTypeCode.StartsWith("04"))
			{
				if (additionalInfo.OriginalIssueID.IsEmpty)
				{
					validationNotifier.AddError(Res.GetString("A46DAB17-A316-49B5-BBE6-3CB3953342E1", "{0} is mandatory for amendment transaction.", new string[] { nameof(additionalInfo.OriginalIssueID) }));
				}

				if (additionalInfo.AmendStatusCode.IsEmpty)
				{
					validationNotifier.AddError(Res.GetString("A46DAB17-A316-49B5-BBE6-3CB3953342E1", "{0} is mandatory for amendment transaction.", new string[] { nameof(additionalInfo.AmendStatusCode) }));
				}
				else if (!EInvoicingKoreaSouthConstants.AmendStatusList.ContainsCode(additionalInfo.AmendStatusCode))
				{
					validationNotifier.AddError(Res.GetString("9ECE5607-E0A2-4913-A06E-81684EF74972", "The '{0}' element is invalid. Your value is '{1}'. The expected value is '01', '02', '03', '04', '05' or '06'.", new string[] { nameof(additionalInfo.AmendStatusCode), additionalInfo.AmendStatusCode }));
				}
			}
		}
	}
}
