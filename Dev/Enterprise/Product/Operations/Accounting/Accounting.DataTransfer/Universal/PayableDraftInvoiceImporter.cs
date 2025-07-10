using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.Universal;

public class PayableDraftInvoiceImporter : IPayableDraftInvoiceImporter
{
	public bool ImportPayableDraftInvoice(IEDIMessage message, ITopLevelDataObject dataObject, IXmlImportLogger logger, IUniversalObjectFactory factory)
	{
		var result = false;
		var factoryCasted = (UniversalObjectFactory)factory;

		var universalTransaction = dataObject as UniversalTransaction;
		if (universalTransaction == null)
		{
			logger.LogBoth(LogType.Error, Res.GetString("0dc65d97-48b7-49cc-80db-0ab41c8a6b72", "Data object type is not Universal Transaction type."));
			return result;
		}

		if (universalTransaction.Ledger.HasValue && universalTransaction.Ledger.Value == LedgerTypes.AccountsPayable)
		{
			result = ImportIntoPayableDraftInvoices(universalTransaction, message, logger, factoryCasted);
			return result;
		}

		logger.LogBoth(LogType.Error, Res.GetString("af1d72c9-074c-4396-8d93-f5278919221d", "Transaction Ledger is not supported"));
		return result;
	}

	public IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject,
		IXmlImportLogger logger, IUniversalObjectFactory factory)
	{
		return new UniversalKeysResult(Enumerable.Empty<(string KeyValue, string KeySource)>());
	}

	bool ImportIntoPayableDraftInvoices(UniversalTransaction universalTransaction, IEDIMessage message, IXmlImportLogger logger, UniversalObjectFactory factory)
	{
		var draftInvoiceHeader = factory.BOFactory.New<AccDraftInvoiceHeader>();
		var result = false;

		using (factory.BOFactory.AddDisposableService())
		{
			using (factory.BOFactory.SetTempContext(OrganisationMatcherContexts.Payables))
			{
				SetTransactionValues(factory, draftInvoiceHeader, universalTransaction, logger);
			}

			if (!(logger?.HasErrors()).GetValueOrDefault())
			{
				using (((IBusinessObjectInternals)draftInvoiceHeader).ResumeValidationForAllDescendantsTemporarily())
				{
					draftInvoiceHeader.RunPreSaveValidation();
				}

				var draftInvoiceHeaderErrors = GetDraftInvoiceHeaderErrors(draftInvoiceHeader);
				if (!draftInvoiceHeaderErrors.IsEmpty)
				{
					throw new MessageProcessingBusinessFailureException(draftInvoiceHeaderErrors, false, string.Empty);
				}
				else if (!(logger?.HasErrors()).GetValueOrDefault())
				{
					factory.AssertAllowedSaveTypes(GetAllowableTypes());
					factory.RecordEndOfEveryRead(draftInvoiceHeader);
					result = SaveTransaction(logger, factory, message.EM_MessageNum);
				}
			}
		}

		return result;
	}

	ZString GetDraftInvoiceHeaderErrors(AutoAccDraftInvoiceHeader draftInvoiceHeader)
	{
		if (draftInvoiceHeader.HasErrors)
		{
			var draftInvoiceHeaderErrors = new StringBuilder();
			draftInvoiceHeaderErrors.AppendLine(Res.GetString("39F5F56B-83A4-4D51-A7D7-349DDA442D2B", "Import failed because draft invoice header has validation errors:"));
			draftInvoiceHeaderErrors.AppendLine(string.Join("\r\n", new ZNotificationCollector(draftInvoiceHeader, true, true, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).Where(x => x.Type == NotificationType.Error).Select(y => y.Message)));

			return draftInvoiceHeaderErrors.ToString();
		}

		return ZString.Empty;
	}

	static IEnumerable<Type> GetAllowableTypes()
	{
		var transactionTypes = new HashSet<Type>()
		{
			typeof(AccDraftInvoiceHeader)
		};

		foreach (Type type in transactionTypes)
		{
			yield return type;
		}
	}

	bool SaveTransaction(IXmlImportLogger logger, UniversalObjectFactory factory, ZString messageNum)
	{
		try
		{
			factory.SaveAtEndOfImport(logger);
			return true;
		}
		catch (ZSaveException e) when (!e.IndexNameIfUniqueIndexViolation.IsEmpty)
		{
			logger.LogBoth(LogType.Error, Res.GetString("6AA9A0D4-B9E2-45D3-8981-4AD35A7B21B4", "EDI Message {0} could not be imported due to duplicate Draft Invoice", messageNum));
			return false;
		}
	}

	void SetTransactionValues(UniversalObjectFactory factory, AutoAccDraftInvoiceHeader draftInvoiceHeader, UniversalTransaction universalTransaction, IXmlImportLogger logger)
	{
		SetOrganization(factory, draftInvoiceHeader, universalTransaction, logger);

		SetValue(draftInvoiceHeader, DraftInvoiceHeaderElement.TransactionDate, universalTransaction.TransactionDate);

		SetValue(draftInvoiceHeader, DraftInvoiceHeaderElement.PostDate, universalTransaction.PostDate);
		SetValue(draftInvoiceHeader, DraftInvoiceHeaderElement.Number, universalTransaction.Number);
		SetValue<ZString>(draftInvoiceHeader, DraftInvoiceHeaderElement.TransactionType, new ZString(universalTransaction.TransactionType.ToString()));
		SetValue(draftInvoiceHeader, DraftInvoiceHeaderElement.DueDate, universalTransaction.DueDate);

		ZString osCurrencyCodeXmlValue;
		if (universalTransaction.OSCurrency.GetValueSafe(x => x.Code).TryGetValue(out osCurrencyCodeXmlValue) && !osCurrencyCodeXmlValue.IsEmpty)
		{
			draftInvoiceHeader.AIH_RX_NKTransactionCurrency = osCurrencyCodeXmlValue;
			ZDecimal exchangeRateXmlValue;
			if (universalTransaction.ExchangeRate.TryGetValue(out exchangeRateXmlValue) && exchangeRateXmlValue > 0)
			{
				draftInvoiceHeader.AIH_ExchangeRate = exchangeRateXmlValue;
			}
		}

		SetTransactionAmounts(draftInvoiceHeader, universalTransaction, logger);

		SetValue(draftInvoiceHeader, DraftInvoiceHeaderElement.Description, universalTransaction.Description);
		SetValue(draftInvoiceHeader, DraftInvoiceHeaderElement.Branch, universalTransaction.Branch.GetValueSafe(x => x.Code));
		SetValue(draftInvoiceHeader, DraftInvoiceHeaderElement.Department, universalTransaction.Department.GetValueSafe(x => x.Code));
	}

	static void SetTransactionAmounts(AutoAccDraftInvoiceHeader draftInvoiceHeader, UniversalTransaction universalTransaction, IXmlImportLogger logger)
	{
		int multiplier = GetMultiplier(universalTransaction.TransactionType);
		var osExTaxAmount = multiplier * universalTransaction.OSExGSTVATAmount.GetValueOrDefault();
		var osTaxAmount = multiplier * universalTransaction.OSGSTVATAmount.GetValueOrDefault();
		var osTotal = multiplier * universalTransaction.OSTotal.GetValueOrDefault();

		draftInvoiceHeader.AIH_ExpectedOSExTaxAmount = osExTaxAmount;
		draftInvoiceHeader.AIH_ExpectedOSTaxAmount = osTaxAmount;
		draftInvoiceHeader.AIH_ExpectedOSTotalAmount = osTotal;
	}

	static void SetOrganization(UniversalObjectFactory universalFactory, AutoAccDraftInvoiceHeader draftInvoiceHeader, UniversalTransaction universalTransaction, IXmlImportLogger logger)
	{
		var addressBO = GetOrgAddress(universalFactory, universalTransaction, logger);
		if (addressBO != null)
		{
			draftInvoiceHeader.AIH_OH_Creditor = addressBO.OA_OH;
			draftInvoiceHeader.AIH_OA_CreditorAddress = addressBO.PK;
		}

		ZString contactXmlValue;
		if (universalTransaction.OrganizationAddress.Contact.TryGetValue(out contactXmlValue) && !contactXmlValue.IsEmpty)
		{
			var query = new ZQuery(OrgContactSchema.OC_ContactName, contactXmlValue);
			ZString phoneXmlValue;
			if (universalTransaction.OrganizationAddress.Phone.TryGetValue(out phoneXmlValue) && !phoneXmlValue.IsEmpty)
			{
				query.AddToFilter(OrgContactSchema.OC_Phone, phoneXmlValue);
			}
			ZString emailXmlValue;
			if (universalTransaction.OrganizationAddress.Email.TryGetValue(out emailXmlValue) && !emailXmlValue.IsEmpty)
			{
				query.AddToFilter(OrgContactSchema.OC_Email, emailXmlValue);
			}

			var contact = universalFactory.BOFactory.LoadTop1<OrgContact>(query);
			if (contact != null)
			{
				draftInvoiceHeader.AIH_OC_CreditorContact = contact.PK;
			}
		}
	}

	static int GetMultiplier(TransactionType? transactionType)
	{
		if (!transactionType.HasValue)
		{
			throw new MessageProcessingBusinessFailureException("Imported Universal Transaction has no TransactionType set");
		}

		if (!(transactionType == TransactionType.CRD || transactionType == TransactionType.INV))
		{
			throw new MessageProcessingBusinessFailureException($"Provided transaction type is [{transactionType}].Only transaction types [{TransactionType.CRD}] and [{TransactionType.INV}] can be imported to Payables Invoice Processing Portal");
		}

		return transactionType == TransactionType.CRD ? 1 : -1;
	}

	static OrgAddress GetOrgAddress(UniversalObjectFactory factoryCasted, UniversalTransaction universalTransaction, IXmlImportLogger logger) => TxnHeaderBuilder.GetOrgAddress(factoryCasted, universalTransaction.OrganizationAddress, logger);

	bool SetValue<T>(BusinessObject businessObj, Enum xmlFieldName, T? value)
		where T : struct, IZType
		=> Helpers.SetValue(businessObj, xmlFieldName, value, mapping, setWhenReadOnly: true, setWhenInvalid: false);

	readonly Dictionary<Enum, SchemaColumn> mapping = new()
	{
		{ DraftInvoiceHeaderElement.TransactionDate, AccDraftInvoiceHeaderSchema.AIH_TransactionDate },
		{ DraftInvoiceHeaderElement.PostDate, AccDraftInvoiceHeaderSchema.AIH_PostDate },
		{ DraftInvoiceHeaderElement.Number, AccDraftInvoiceHeaderSchema.AIH_TransactionNumber },
		{ DraftInvoiceHeaderElement.DueDate, AccDraftInvoiceHeaderSchema.AIH_DueDate },
		{ DraftInvoiceHeaderElement.Description, AccDraftInvoiceHeaderSchema.AIH_Description },
		{ DraftInvoiceHeaderElement.Branch, AccDraftInvoiceHeaderSchema.AIH_GB_Branch },
		{ DraftInvoiceHeaderElement.Department, AccDraftInvoiceHeaderSchema.AIH_GE_Department },
		{ DraftInvoiceHeaderElement.TransactionType, AccDraftInvoiceHeaderSchema.AIH_TransactionType },
	};

	enum DraftInvoiceHeaderElement
	{
		TransactionDate,
		PostDate,
		Number,
		DueDate,
		Description,
		Branch,
		Department,
		TransactionType,
	}
}
