using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public class TaxCoreEInvoiceCreator : ITaxCoreEInvoiceCreator
	{
		public ITaxCoreEInvoice Create(TaxCoreEInvoiceCreatorParameter eInvoiceCreatorParameter)
		{
			Argument.NotNull(eInvoiceCreatorParameter, nameof(eInvoiceCreatorParameter));
			Argument.NotNull(eInvoiceCreatorParameter.Branch, nameof(eInvoiceCreatorParameter), $"{nameof(eInvoiceCreatorParameter)}.{nameof(eInvoiceCreatorParameter.Branch)} is mandatory.");

			var transaction = eInvoiceCreatorParameter.UniversalTransaction;
			var notifications = eInvoiceCreatorParameter.Notifications;
			var certificateCredential = eInvoiceCreatorParameter.CertificateCredential;
			var transactionCreatedBy = eInvoiceCreatorParameter.CreateUser;
			var additionalTransactionInfo = eInvoiceCreatorParameter.AdditionalTransactionInfo;
			var taxFileCode = eInvoiceCreatorParameter.TaxFileCode;
			var tfnCode = eInvoiceCreatorParameter.TFNCode;
			var countryCode = eInvoiceCreatorParameter.CountryCode;

			if (Validate(transaction, additionalTransactionInfo, notifications))
			{
				var eInvoice = CreateTaxCoreInvoice(eInvoiceCreatorParameter.Branch);

				eInvoice.UTCCreateTime = transaction.CreateTime?.ToISO8601StringWithZeroOffsetSymbol();
				eInvoice.CashierTFN = GetCashierTFN(transactionCreatedBy, tfnCode);
				eInvoice.TaxIdentificationNumber = GetRegistrationNumber(transaction, taxFileCode, countryCode);
				eInvoice.TransactionType = transaction.TransactionType == TransactionType.CRD ? TaxCoreTransactionTypes.Refund : TaxCoreTransactionTypes.Sale;
				eInvoice.SetPayment(transaction);
				eInvoice.InvoiceNumber = transaction.Number;
				eInvoice.ReferentDocumentNumber = additionalTransactionInfo?.OriginalTransactionGovtReferenceNumber;
				eInvoice.ReferentDocumentDateAndTime = additionalTransactionInfo?.OriginalTransactionCreationDate?.ToISO8601StringWithZeroOffsetSymbol();
				eInvoice.PAC = certificateCredential.GetPAC();

				transaction.PostingJournalCollection
					.Where(pj => pj.LocalTotalAmount != 0)
					.ForEach(eInvoice.AddLine);

				//Populate Hash
				using (var md5 = MD5.Create())
				{
					var dataHash = md5.ComputeHash(Encoding.UTF8.GetBytes(eInvoice.GetJSONPayload()));
					eInvoice.Hash = Convert.ToBase64String(dataHash);
				}

				eInvoice.Validate(notifications);

				return eInvoice;
			}
			return null;
		}

		ITaxCoreEInvoice CreateTaxCoreInvoice(GlbBranch branch)
		{
			var newSchemaComplianceDate = AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.GetFallBackValueAtAllLevels(branch.Company.PK.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
			if (newSchemaComplianceDate == DateTime.MinValue || newSchemaComplianceDate > ZDateTime.Today.ToDateTime())
			{
				return new TaxCoreEInvoiceLegacy();
			}
			else
			{
				return new TaxCoreEInvoiceV3();
			}
		}

		bool Validate(UniversalTransactionInfo transaction, IAdditionalTransactionInfoForTaxCoreEInvoice additionalTransactionInfo, INotifications notifications)
		{
			var result = true;
			if (transaction.TransactionType == TransactionType.CRD &&
				(string.IsNullOrEmpty(additionalTransactionInfo?.OriginalTransactionGovtReferenceNumber) || (!additionalTransactionInfo.OriginalTransactionCreationDate?.IsValid ?? true)))
			{
				result = false;
				notifications.AddError(Res.GetString("3de7e4a7-b358-438c-b32f-c85de59a98ca", "'Referent Document Number' or 'Referent Document Date' is missing for refund transaction {0}", transaction.Number));
			}
			if (!transaction.PostingJournalCollection.All(pj => pj.OSGSTVATAmount == 0 || (pj.OSGSTVATAmount != 0 && !string.IsNullOrEmpty(pj.TaxMessageID?.TaxGroupCode?.Code ?? string.Empty))))
			{
				result = false;
				notifications.AddError(Res.GetString("e32e90ae-af51-4d23-8e2d-27c1960a21b1", "{0} has one or more line(s) with non zero tax amount but empty tax message", transaction.Number));
			}
			return result;
		}

		string GetCashierTFN(GlbStaff transactionCreatedBy, ZString tfnCode) => transactionCreatedBy?.Certificates
											.OfType<GenRegCertAccredMaintList>()
											.FirstOrDefault(c => c.XZ_Type == tfnCode)?.XZ_RefNumber;

		string GetRegistrationNumber(UniversalTransactionInfo transaction, ZString taxFileCode, ZString countryCode) => transaction.OrganizationAddress?.RegistrationNumberCollection?
											.FirstOrDefault(x => x.Type.Code.HasValue &&
															x.Type.Code.Value == taxFileCode &&
															x.CountryOfIssue.Code.HasValue &&
															x.CountryOfIssue.Code.Value == countryCode)?.Value;
	}
}
