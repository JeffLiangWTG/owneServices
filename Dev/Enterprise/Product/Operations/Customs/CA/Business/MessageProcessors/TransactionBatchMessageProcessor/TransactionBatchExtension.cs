using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public static class TransactionBatchExtension
	{
		public static List<TransactionInfo> GetTransactionCollectionByCategory(this TransactionBatch transactionBatch, string categoryCode)
		{
			Argument.NotNull(transactionBatch, "transactionBatch");
			return transactionBatch.TransactionCollection.Where(t => t.Category.GetValueOrDefault() == categoryCode).ToList();
		}

		public static List<TransactionInfo> GetTransactionCollectionByCategory(this TransactionBatch transactionBatch, string categoryCode, string bn)
		{
			return transactionBatch.TransactionCollection.Where(t => t.Category.GetValueOrDefault() == categoryCode && t.OrganizationAddress.GovRegNum.GetValueOrDefault() == bn).ToList();
		}

		public static ZDecimal GetChargeTotalAmountByChargeCode(this TransactionInfo transaction, string chargeCode)
		{
			var postingJournal = transaction.PostingJournalCollection.FirstOrDefault(p => p.ChargeCode != null && p.ChargeCode.Code.GetValueOrDefault() == chargeCode);
			if (postingJournal != null)
			{
				return postingJournal.ChargeTotalAmount.GetValueOrDefault();
			}
			return ZDecimal.Zero;
		}

		public static ZDecimal GetLocalAmountByDescription(this TransactionInfo transaction, string description)
		{
			var postingJournal = transaction.PostingJournalCollection.FirstOrDefault(p => p.Description.GetValueOrDefault() == description);
			if (postingJournal != null)
			{
				return postingJournal.LocalAmount.GetValueOrDefault();
			}
			return ZDecimal.Zero;
		}

		public static bool IsLocalAmountProvided(this TransactionInfo transaction, string description)
		{
			var postingJournal = transaction.PostingJournalCollection.FirstOrDefault(p => p.Description.GetValueOrDefault() == description);
			return postingJournal != null && postingJournal.LocalAmount.HasValue;
		}

		public static ZString GetAddInfo(this TransactionInfo transaction, string key)
		{
			var shipment = transaction.ShipmentCollection?.FirstOrDefault();
			if (shipment != null)
			{
				var addInfo = shipment.AddInfoCollection?.FirstOrDefault(a => a.Key.GetValueOrDefault() == key);
				if (addInfo != null)
				{
					return addInfo.Value.GetValueOrDefault();
				}
			}
			return ZString.Empty;
		}

		public static ZString GetRegistrationNumber(this TransactionInfo transaction, string type, string countryOfIssue)
		{
			var orgAddress = transaction.OrganizationAddress;
			if (orgAddress != null && orgAddress.RegistrationNumberCollection != null)
			{
				var registrationNumber = orgAddress.RegistrationNumberCollection.FirstOrDefault(r => r.Type.Code.GetValueOrDefault() == type && r.CountryOfIssue.Code.GetValueOrDefault() == countryOfIssue);
				if (registrationNumber != null)
				{
					return registrationNumber.Value.GetValueOrDefault();
				}
			}
			return ZString.Empty;
		}

		public static ZString GetAccountSecurityCode(this TransactionInfo transaction, string bn, string rmAccountNumber, BusinessObjectFactory factory, IXmlSessionTracker logger = null)
		{
			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}

			var accountSecurityCode = transaction.GetRegistrationNumber(OrgCusCode.CACodeTypes.AccountSecurityCode, Core.Constants.CountryCodes.Canada);
			if (accountSecurityCode.IsEmpty || accountSecurityCode == "00000")
			{
				var orgs = GetOrgsFromBN(bn, rmAccountNumber, factory);
				if (orgs.Length == 0 && logger != null)
				{
					logger.LogBoth(Integration.LogType.Warning, Res.GetString("95f545a0-6c06-4302-9842-1c361aaaaf5e", "Cannot find an organization with BN {0}", bn));
				}
				accountSecurityCode = GetAccountSecurityCode(orgs.FirstOrDefault(), logger);
			}

			return accountSecurityCode;
		}

		public static OrgHeader[] GetOrgsFromBN(ZString businessNumber, ZString rmAccountNumber, BusinessObjectFactory factory, bool isBroker = false)
		{
			return factory.GetCachedValue("BusinessNum:" + businessNumber + " rmAccNum:" + rmAccountNumber + "isBroker:" + isBroker, delegate
			{
				var orgs = Array.Empty<OrgHeader>();
				ZQuery codeFilter = null;

				if (businessNumber.Length == 9 && rmAccountNumber.Length == 4)
				{
					codeFilter = GetOrgFromBNQuery(ZString.Format("{0}RM{1}", businessNumber, rmAccountNumber), false, isBroker);
				}
				else if (businessNumber.Length == 15)
				{
					codeFilter = GetOrgFromBNQuery(businessNumber, false, isBroker);
				}
				if (codeFilter != null)
				{
					orgs = GetOrgByCodeFilter(codeFilter, factory);
				}

				if (orgs.Length == 0)
				{
					codeFilter = null;
					if (businessNumber.Length == 15)
					{
						codeFilter = GetOrgFromBNQuery(businessNumber.Left(9), true, isBroker);
					}
					else if (businessNumber.Length == 9)
					{
						codeFilter = GetOrgFromBNQuery(businessNumber, true, isBroker);
					}
					if (codeFilter != null)
					{
						orgs = GetOrgByCodeFilter(codeFilter, factory);
					}
				}
				return orgs;
			});
		}

		static OrgHeader[] GetOrgByCodeFilter(ZQuery codeFilter, BusinessObjectFactory factory)
		{
			var result = new List<OrgHeader>();
			if (codeFilter != null)
			{
				var cusCodes = factory.Load<OrgCusCode>(codeFilter);
				cusCodes.ToList().ForEach(x => factory.AddFetchHint(OrgHeaderSchema.PK, x.OK_OH));

				foreach (var groupData in cusCodes.GroupBy(x => x.OK_CustomsRegNo).OrderBy(x => x.Key))
				{
					foreach (var org in groupData.Select(x => x.Header).Where(x => x != null && x.OH_IsActive).OrderBy(x => x.OH_Code))
					{
						if (!result.Contains(org))
						{
							result.Add(org);
						}
					}
				}
			}

			return result.ToArray();
		}

		static ZQuery GetOrgFromBNQuery(ZString bn, bool startWith, bool isBroker)
		{
			var result = new ZDBOnlyQuery(typeof(OrgCusCode));
			result.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CACodeTypes.BusinessNumberForImportExport);
			result.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Canada);

			if (startWith)
			{
				result.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, SQLComparisonOperator.StartsWith, bn);
			}
			else
			{
				result.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, bn);
			}

			if (isBroker)
			{
				var companySubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.GC_OH_OrgProxy);
				companySubQuery.AddToFilter(GlbCompanySchema.GC_Code, GlbCompany.CurrentCompany.GC_Code);
				var isBrokerSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				isBrokerSubQuery.AddToFilter(OrgHeaderSchema.OH_IsBroker, true);
				result.AddSubQuery(OrgCusCodeSchema.OK_OH, companySubQuery, JoinCondition.And);
				result.AddSubQuery(OrgCusCodeSchema.OK_OH, isBrokerSubQuery, JoinCondition.And);
			}

			return result;
		}

		internal static ZString GetAccountSecurityCode(OrgHeader organization, IXmlSessionTracker logger)
		{
			var accountSecurityCode = ZString.Empty;
			if (organization != null)
			{
				accountSecurityCode = OrgImpAddInfo.Get(organization).ZO_AccountSecurityNumber;
				if (accountSecurityCode.IsEmpty)
				{
					accountSecurityCode = organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.AccountSecurityCode, Core.Constants.CountryCodes.Canada);
				}
			}

			if (accountSecurityCode.IsEmpty)
			{
				accountSecurityCode = CACustomsDataRegistry.Instance.AccountSecurityNo.Value;
				if (logger != null)
				{
					if (accountSecurityCode.IsEmpty)
					{
						logger.LogBoth(Integration.LogType.Warning, Res.GetString("26592e55-45b8-4448-bfe6-bb65d8085d49", "Cannot find the Account Security Code for importer or in Registry"));
					}
					else
					{
						logger.LogBoth(Integration.LogType.Information, Res.GetString("b7b93fea-1581-4625-b867-fef2c242cdf4", "Using the Account Security Code '{0}' in Registry", accountSecurityCode));
					}
				}
			}
			else if (logger != null)
			{
				logger.LogBoth(Integration.LogType.Information, Res.GetString("fe5e637f-3e16-40fc-a9cc-4b0989bc2c42", "Using the Account Security Code '{0}' for {1}", accountSecurityCode, organization.HumanReadableName));
			}

			return accountSecurityCode;
		}
	}
}
