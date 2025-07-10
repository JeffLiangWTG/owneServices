using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Accounting.ElectronicMessaging.Common.Universal.AccountingInvoiceDataContextManager;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal
{
	public class AccountingInvoiceEventParentFinder : EventParentFinder
	{
		public AccountingInvoiceEventParentFinder(BusinessObjectFactory factory, AccountingInvoiceDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			InvoicingBase[] result = null;

			if (xmlEvent.DataContext != null && xmlEvent.DataContext.DataTargetCollection != null && xmlEvent.DataContext.DataTargetCollection.Any())
			{
				var uniqueKey = xmlEvent.DataContext.DataTargetCollection.FirstOrDefault(x => x.Type.HasValue && x.Type.Value == nameof(DataContextType.AccountingInvoice));
				if (uniqueKey != null && uniqueKey.Key.HasValue)
				{
					GlbCompany company = null;
					var companyCode = xmlEvent.DataContext.CompanyCodeToImportInto;
					if (!companyCode.IsEmpty)
					{
						company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
					}
					var keys = GetAllKeys(uniqueKey.Key.Value);

					if (xmlEvent != null && xmlEvent.EventParameters != null)
					{
						var countryCode = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, xmlEvent.EventParameters) ?? string.Empty;
						var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, xmlEvent.EventParameters) ?? string.Empty;
						var countryFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(countryCode);

						if (countryFactory != null && countryFactory.GetGlobalXUEFunctionalityProvider().IsInvoiceEventMessageProcessSupported(messageSubType))
						{
							var targetQuery = countryFactory.GetInvoiceEventMessageTargetQuery(xmlEvent);
							result = factory.Load<InvoicingBase>(targetQuery);
						}
					}

					if (result == null && company != null && keys.AllKeys)
					{
						try
						{
							ZQuery query = new ZQuery(AccTransactionHeaderSchema.AH_GC, company.PK);
							query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, keys.Ledger);
							query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, keys.TransactionType);
							query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, keys.TransactionNum);
							var orgPKFromCusCode = RetrieveOrganizationFromContext(xmlEvent);
							var invoiceDate = RetrieveInvoiceDateFromContext(xmlEvent);
							var orgPKFromOrgCode = RetrieveOrgPKFromContext(xmlEvent);
							if (orgPKFromCusCode.IsValid)
							{
								query.AddToFilter(AccTransactionHeaderSchema.AH_OH, orgPKFromCusCode);
							}
							if (invoiceDate.IsValid)
							{
								query.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.EqualToDatePartOnly, invoiceDate);
							}
							if (orgPKFromOrgCode.IsValid)
							{
								query.AddToFilter(AccTransactionHeaderSchema.AH_OH, orgPKFromOrgCode);
							}
							result = factory.Load<InvoicingBase>(query);
							if (result == null || result.Length != 1)
							{
								var creditorCode = ZString.Empty;
								var orgPK = ZGuid.Empty;

								if (orgPKFromCusCode.IsValid)
								{
									orgPK = orgPKFromCusCode;
								}
								else if (orgPKFromOrgCode.IsValid)
								{
									orgPK = orgPKFromOrgCode;
								}

								if (!orgPK.IsEmpty)
								{
									var orgHeader = factory.Load<OrgHeader>(orgPK);
									if (orgHeader != null)
									{
										creditorCode = orgHeader.OH_Code;
									}
								}

								if (result == null || result.Length == 0)
								{
									throw new InvalidOperationException(AccountingConstants.AccountingInvoiceEventParentFinderErrorMessages.NoMatchedInvoiceFound(keys.Ledger, keys.TransactionType, keys.TransactionNum, creditorCode, invoiceDate.ToShortDateString()));
								}
								else if (result.Length > 1)
								{
									throw new InvalidOperationException(AccountingConstants.AccountingInvoiceEventParentFinderErrorMessages.TooManyMatchedInvoiceFound(keys.Ledger, keys.TransactionType, keys.TransactionNum, creditorCode, invoiceDate.ToShortDateString()));
								}
							}
						}
						catch (InvalidOperationException ex)
						{
							logger.LogBoth(LogType.Error, ex.Message);
						}
					}
				}
			}
			return result;
		}

		ZGuid RetrieveOrganizationFromContext(UniversalEvent xmlEvent)
		{
			Argument.NotNull(xmlEvent, "xmlEvent");

			ZGuid orgPK = ZGuid.Empty;
			if (xmlEvent.ContextCollection != null)
			{
				var taxRegNumberContext = xmlEvent.ContextCollection.FirstOrDefault(x => x.Type == EventDataConstants.Context_TaxRegNumber);
				if (taxRegNumberContext != null)
				{
					var taxRegNumber = taxRegNumberContext.Value.Value;
					OrgCusCode[] orgCusCodes;
					if (taxRegNumber.Length <= 5)
					{
						throw new InvalidOperationException(AccountingConstants.AccountingInvoiceEventParentFinderErrorMessages.TaxRegNumberTooShort(taxRegNumber));
					}
					var taxRegCountry = taxRegNumber.Left(2);       // position 1-2 ==> country code
					var taxRegCode = taxRegNumber.Substring(2, 3);  // position 3-5 ==> registration code type
					taxRegNumber = taxRegNumber.Substring(5);   // position starting from 6 ==> registration code number

					if (taxRegCountry == Core.Constants.CountryCodes.Spain)     //For ES, 'NIF' can mean NIF/DNI/IGC, therfore we need fallback logic
					{
						orgCusCodes = GetOrgCusCodes(OrgCusCode.SpainCodeTypes.NIF, taxRegNumber, taxRegCountry);
						if (orgCusCodes.Length == 0)
						{
							orgCusCodes = GetOrgCusCodes(OrgCusCode.SpainCodeTypes.DNI, taxRegNumber, taxRegCountry);
						}
						if (orgCusCodes.Length == 0)
						{
							orgCusCodes = GetOrgCusCodes(OrgCusCode.SpainCodeTypes.IGC, taxRegNumber, taxRegCountry);
						}
					}
					else
					{
						orgCusCodes = GetOrgCusCodes(taxRegCode, taxRegNumber, taxRegCountry);
					}

					if (orgCusCodes.Length == 1)
					{
						orgPK = orgCusCodes[0].OK_OH;
					}
					else if (orgCusCodes.Length == 0)
					{
						throw new InvalidOperationException(AccountingConstants.AccountingInvoiceEventParentFinderErrorMessages.NoMatchedOrgCusCodeFound(taxRegCountry, taxRegCode, taxRegNumber));
					}
					else
					{
						throw new InvalidOperationException(AccountingConstants.AccountingInvoiceEventParentFinderErrorMessages.TooManyMatchedOrgCusCodeFound(taxRegCountry, taxRegCode, taxRegNumber));
					}
				}
			}
			return orgPK;
		}

		OrgCusCode[] GetOrgCusCodes(ZString? taxRegCode, ZString? taxRegNumber, ZString? taxRegCountry)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_RN_NKCodeCountry, taxRegCountry);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, taxRegNumber);
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, taxRegCode);
			var orgCusCodes = factory.Load<OrgCusCode>(query);
			return orgCusCodes;
		}

		ZDate RetrieveInvoiceDateFromContext(UniversalEvent xmlEvent)
		{
			Argument.NotNull(xmlEvent, "xmlEvent");

			ZDate invoiceDate = ZDate.Empty;
			if (xmlEvent.ContextCollection != null)
			{
				var invoiceDateContext = xmlEvent.ContextCollection.FirstOrDefault(x => x.Type == EventDataConstants.Context_InvoiceDate);
				if (invoiceDateContext != null)
				{
					invoiceDate = new ZDate(invoiceDateContext.Value);
				}
			}
			return invoiceDate;
		}

		ZGuid RetrieveOrgPKFromContext(UniversalEvent xmlEvent)
		{
			Argument.NotNull(xmlEvent, "xmlEvent");

			var orgPK = ZGuid.Empty;

			if (xmlEvent.ContextCollection != null)
			{
				var orgCodeContext = xmlEvent.ContextCollection.FirstOrDefault(x => x.Type == EventDataConstants.Context_OrganizationCode);
				if (orgCodeContext != null)
				{
					var orgCode = orgCodeContext.Value;
					if (orgCode.HasValue)
					{
						var query = new ZQuery(OrgHeaderSchema.OH_Code, orgCode);
						var organization = factory.Load<OrgHeader>(query);

						if (organization.Length == 1)
						{
							orgPK = organization[0].PK;
						}
						else if (organization.Length == 0)
						{
							throw new InvalidOperationException(AccountingConstants.AccountingInvoiceEventParentFinderErrorMessages.NoMatchedOrganizationFound(orgCode.ToString()));
						}
					}
				}
			}
			return orgPK;
		}

		internal static (bool AllKeys, ZString Ledger, ZString TransactionType, ZString TransactionNum) GetAllKeys(ZString combinedKeys)
		{
			var result  = (false, ZString.Empty, ZString.Empty, ZString.Empty);
			var parts = combinedKeys.Split(' ');
			if (parts.Length >= 3)
			{
				var ledger = parts[0];
				var transactionType = parts[1];
				if ((ledger == LedgerTypes.AccountsPayable ||
					ledger == LedgerTypes.AccountsReceivable ||
					ledger == LedgerTypes.TransactionsPendingAllocation) &&
					(transactionType == TransactionTypes.Invoice ||
					transactionType == TransactionTypes.CreditNote ||
					transactionType == TransactionTypes.AdjustmentNote ||
					transactionType == TransactionTypes.CreditNotePendingAllocation ||
					transactionType == TransactionTypes.InvoicePendingAllocation))
				{
					var transactionNumBuilder = new ZStringBuilder(parts[2]);
					parts.Skip(3).ForEach(x => transactionNumBuilder.Append(x));
					result = (true, parts[0], parts[1], transactionNumBuilder.ToStringWithDelimiterBetweenAppends(" "));
				}
			}

			return result;
		}
	}
}
