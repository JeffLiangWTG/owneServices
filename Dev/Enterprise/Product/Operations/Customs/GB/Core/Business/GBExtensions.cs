using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Universal.Constants;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.GB.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.GB.Business
{
	public static class GBExtensions
	{
		public static bool IsPhase1Active => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.CDS_ILE_PHASE1, CountryCodes.UnitedKingdom, ZDateTime.Today);

		public static bool IsPhase2Active => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.CDS_ILE_PHASE2, CountryCodes.UnitedKingdom, ZDateTime.Today);

		internal static ZString GetCDSChargeCode(this BaseJobComInvHeaderCharge charge)
		{
			return (charge?.Parent?.JobDeclaration as JobDeclaration)?.ApplicationExtender?.GetCDSChargeCode(charge) ?? ZString.Empty;
		}

		public static void UpdateStatusIfNotEmpty(this CusEntryHeader entry, ZString status)
		{
			if (!status.IsEmpty && entry != null)
			{
				entry.CH_Status = status;
			}
		}

		public static ZString ToAlphanumericOnlyString(this ZGuid guid)
		{
			return ((ZString)guid.ToString()).KeepAlphanumericCharacters();
		}

		public static ZString GetCredentialsKey(this JobDeclaration declaration)
		{
			var eori = declaration.GetEori();
			var badgeCode = declaration?.JE_CustomsProfile ?? ZString.Empty;
			return GetCredentialsKey(eori, badgeCode);
		}

		public static ZString GetCredentialsKeyForCurrentCompany()
		{
			var enterpriseCode = GetEnterpriseCode();

			var gbGlbExternalPasswordCollection = new GlbExternalPasswordCollection_GB(GlbCompany.CurrentCompany);
			gbGlbExternalPasswordCollection.Load();

			return gbGlbExternalPasswordCollection.OfType<GlbExternalPassword_GB>()
				.Where(x => x.Status == PasswordStatusList.Codes.Valid)
				.OrderByDescending(x => x.GP_IssueDate)
				.Select(x => FormattableString.Invariant($"{enterpriseCode}.{x.EORI}.{x.Badge}"))
				.FirstOrDefault() ?? ZString.Empty;
		}

		public static ZString GetEori(this JobDeclaration declaration) => declaration?.Branch?.OrgProxy?.GetEuIdentificationNumber() ?? ZString.Empty;

		public static ZString GetCredentialsKey(ZString eori, ZString badgeCode)
		{
			var enterpriseCode = GetEnterpriseCode();
			return FormattableString.Invariant($"{enterpriseCode}.{eori}.{badgeCode}");
		}

		public static ZString GetEnterpriseCode()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			return $"{registrationKey.EnterpriseCode}{registrationKey.ServerCode}";
		}

		public static CredentialsSetting GetCredentialsSettingByBadgeCode(this JobDeclaration declaration)
		{
			var credentials = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
			return credentials.FindByBadgeCode(declaration.JE_CustomsProfile);
		}

		public static ZString GetEoriFor(this JobDeclaration declaration, ZString paymentMethod)
		{
			switch (paymentMethod)
			{
				case EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority:
				case EU.Business.DefermentMethodList.Codes.ConsigneesAccountStandingAuthority:
				case EU.Business.DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration:
					return declaration.ImporterTraderId;

				case EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14:
					return declaration.DeclarantTraderId;
				default:
					return ZString.Empty;
			}
		}

		public static ZGuid GetOrgFor(this JobDeclaration declaration, ZString paymentMethod)
		{
			switch (paymentMethod)
			{
				case EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority:
				case EU.Business.DefermentMethodList.Codes.ConsigneesAccountStandingAuthority:
				case EU.Business.DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration:
					return declaration.Importer?.PK ?? ZGuid.Empty;

				case EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14:
					return declaration.Declarant?.Header?.PK ?? ZGuid.Empty;
				default:
					return ZGuid.Empty;
			}
		}

		public static void ClearAllHadLastErrorYellowFlagsOnEntrysInvoiceLines(this CusEntryHeader entry)
		{
			foreach (CusEntryLine entryLine in entry.MergedLines)
			{
				foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
				{
					invoiceLine.ZG_HadErrorInLastResponse = false;
				}
			}
		}

		public static void MarkInvoiceLinesAsErroneousYellow(this CusEntryHeader entry, IEnumerable<ZInt> numbers, Func<CusEntryHeader, ZInt, CusEntryLine> entryLineGetter = null)
		{
			try
			{
				var numberArray = numbers.ToArray();
				if (numberArray.Any())
				{
					entry.ClearAllHadLastErrorYellowFlagsOnEntrysInvoiceLines();
					foreach (var number in numberArray)
					{
						if (entryLineGetter == null)
						{
							entryLineGetter = DefaultEntryLineGetter;
						}
						var entryLine = entryLineGetter.Invoke(entry, number);
						entryLine?.MarkInvoiceLinesAsErroneousYellow();
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("MarkYellowInvoiceLines", "Error caught while trying to set erroneous lines as yellow.", ex);
			}
		}

		static CusEntryLine DefaultEntryLineGetter(CusEntryHeader entry, ZInt lineNumber)
		{
			return entry.AllEntryLines.FindByLineNumber(lineNumber);
		}

		public static void MarkInvoiceLinesAsErroneousYellow(this CusEntryLine entryLine)
		{
			if (entryLine != null)
			{
				foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
				{
					invoiceLine.ZG_HadErrorInLastResponse = true;
				}
			}
		}

		public static ZBool EqualsAny(this ZString str, IEnumerable<ZString> compareValues)
		{
			return compareValues.Any(x => x.Equals(str));
		}

		public static ZBool EqualsAnyIgnoringCase(this ZString str, IEnumerable<ZString> compareValues)
		{
			return compareValues.Any(x => x.EqualsIgnoringCase(str));
		}

		public static ZString GetEori(this ForwardingConsol consol)
		{
			return consol.SendingForwarder?.GetEuIdentificationNumber() ?? ZString.Empty;
		}

		public static ZString GetCredentialCompanyFromDeclarationsBadge(this JobDeclaration jobDeclaration)
		{
			var result = ZString.Empty;
			var credential = GetCredentialFromDeclarationsBadge(jobDeclaration);
			if (credential != null)
			{
				result = credential.Company.Right(3);
			}
			return result;
		}

		public static CredentialsSetting GetCredentialFromDeclarationsBadge(this JobDeclaration jobDeclaration)
		{
			return GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(jobDeclaration.CompanyPK.ToGuid(), System.Guid.Empty, System.Guid.Empty).FindByBadgeCode(jobDeclaration.JE_CustomsProfile);
		}

		public static List<string> GetFullBadgeProfile()
		{
			var license = GetEnterpriseCode();

			var gbGlbExternalPasswordCollection = new GlbExternalPasswordCollection_GB(GlbCompany.CurrentCompany);
			gbGlbExternalPasswordCollection.Load();

			return gbGlbExternalPasswordCollection.OfType<GlbExternalPassword_GB>()
					.Select(x => FormattableString.Invariant($"{x.EORI}.{x.Badge}"))
					.ToList();
		}

		public static ZString GetEuIdentificationNumberForCDS(this JobDocAddress jobDocAddress)
		{
			var result = jobDocAddress.GetEuIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom);
			if (result.IsEmpty)
			{
				result = jobDocAddress.GetEuIdentificationNumber(EconomicGroupList.Codes.EuropeanUnion);
			}
			if (result.IsEmpty)
			{
				result = jobDocAddress.GetEuIdentificationNumber();
			}
			return result;
		}
	}
}
