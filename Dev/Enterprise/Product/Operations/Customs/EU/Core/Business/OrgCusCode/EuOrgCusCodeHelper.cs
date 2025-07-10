using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business
{
	public class EuOrgCusCodeHelper : IEuOrgCusCodeHelper
	{
		ZString IEuOrgCusCodeHelper.GetEuIdentificationNumber(JobDocAddress docAddress, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			var result = ZString.Empty;
			if (docAddress != null)
			{
				if (docAddress.E2_AddressOverride)
				{
					result = docAddress.E2_GovRegNum;
				}
				else if (docAddress.Address is OrgAddress address)
				{
					if (countryOfIssuance != null)
					{
						result = GetEuIdentificationNumber(address, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
					}
					else
					{
						var countryCode = address.Header == null ? ZString.Empty : address.Header.CountryCode;
						result = GetEuIdentificationNumber(address, countryCode.IsEmpty ? null : countryCode.ToString(), true);
					}
				}
			}
			return result;
		}

		ZString IEuOrgCusCodeHelper.GetEuIdentificationNumber(OrgAddress address, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			return GetEuIdentificationNumber(address, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
		}

		ZString IEuOrgCusCodeHelper.GetEuIdentificationNumber(OrgHeader organisation, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			return GetEuIdentificationNumber(organisation, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched);
		}

		ZString IEuOrgCusCodeHelper.GetEORI(OrgAddress address, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			return GetEORICodeCore(address, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, true);
		}

		ZString IEuOrgCusCodeHelper.GetEORI(OrgHeader organisation, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			return GetEORICodeCore(organisation, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, true);
		}

		ZString IEuOrgCusCodeHelper.GetUnprefixedEORI(OrgHeader organisation, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			return GetEORICodeCore(organisation, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, false);
		}

		ZString IEuOrgCusCodeHelper.GetUnprefixedEORI(OrgAddress address, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			return GetEORICodeCore(address, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, false);
		}

		string IEuOrgCusCodeHelper.PrivateEoriReg => "PR";
		string IEuOrgCusCodeHelper.UnregForEori => "UNREG";

		bool IEuOrgCusCodeHelper.ValidEORIorTCUIFormat(ZString eoriOrTCUI, BusinessObjectFactory factory)
		{
			var isValid = false;
			if (Regex.IsMatch(eoriOrTCUI, @"^[A-Z]{2}[\x21-\x7E]{1,15}$"))
			{
				var countryCode = eoriOrTCUI.Left(2);
				isValid = factory.LoadFromNaturalKey<Enterprise.MasterFiles.Integration.IRefCountry>(ZArchitecture.Schema.RefCountrySchema.RN_Code, countryCode) != null;
			}
			return isValid;
		}

		ZString IEuOrgCusCodeHelper.GetRegoCodeOfThisOrg(OrgHeader organisation, string typeOfCodeEoriTurnEtc, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			return GetOrgCusCode(organisation, premiseAddress: null, typeOfCodeEoriTurnEtc, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, appendCountryCodePrefix: true);
		}

		ZString IEuOrgCusCodeHelper.GetRegoCodeOfThisAddress(OrgAddress address, string typeOfCodeEoriTurnEtc, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			return GetOrgCusCode(address.Header, premiseAddress: address, typeOfCodeEoriTurnEtc, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, appendCountryCodePrefix: true);
		}

		(ZString CountryCode, ZString RegistrationNumber) IEuOrgCusCodeHelper.GetEuIdentificationNumberComponents(OrgHeader organisation)
		{
			var registeredCode = organisation?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).FirstOrDefault();
			return (registeredCode?.OK_RN_NKCodeCountry ?? ZString.Empty, registeredCode?.OK_CustomsRegNo ?? ZString.Empty);
		}

		ZString IEuOrgCusCodeHelper.GetEUVATCodeOfThisOrg(OrgHeader organisation)
		{
			var countryVatCodeType = Extensions.CountryVatCodeType.Value;
			var vatCodeQuery = new ZQuery(OrgCusCodeSchema.OK_OH, organisation.PK);
			vatCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryVatCodeType.Keys);
			vatCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, countryVatCodeType.Values);
			var registeredCodes = organisation.Factory.Load<OrgCusCode>(vatCodeQuery);
			var vatCode = registeredCodes.FirstOrDefault(x => x.OK_RN_NKCodeCountry.GetVATCodeType() == x.OK_CodeType);
			return vatCode == null ? ZString.Empty : AppendCountryCodePrefix(vatCode);
		}

		IEnumerable<ZString> IEuOrgCusCodeHelper.GetEUVATCodesOfThisOrg(OrgHeader organisation)
		{
			var countryVatCodeType = Extensions.CountryVatCodeType.Value;
			var vatCodeQuery = new ZQuery(OrgCusCodeSchema.OK_OH, organisation.PK);
			vatCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryVatCodeType.Keys);
			vatCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, countryVatCodeType.Values);
			var vatCodes = organisation.Factory.Load<OrgCusCode>(vatCodeQuery);
			var vatCodesWithPrefix = new List<ZString>();
			foreach (OrgCusCode c in vatCodes)
			{
				if (c.OK_RN_NKCodeCountry.GetVATCodeType() == c.OK_CodeType)
				{
					vatCodesWithPrefix.Add(AppendCountryCodePrefix(c));
				}
			}
			return vatCodesWithPrefix;
		}

		ZString AppendCountryCodePrefix(OrgCusCode cusCode)
		{
			if (cusCode.OK_CustomsRegNo.StartsWith(cusCode.OK_RN_NKCodeCountry, StringComparison.OrdinalIgnoreCase)
				|| (cusCode.OK_CustomsRegNo.StartsWith(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, StringComparison.OrdinalIgnoreCase) && cusCode.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedKingdom)
				|| (cusCode.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Spain && cusCode.OK_CodeType == OrgCusCode.SpainCodeTypes.NIF && cusCode.Organisation.OH_Category == OrgConstants.Category.NaturalPersonIndividual)
				|| (cusCode.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.France && cusCode.OK_CustomsRegNo == "OCCASIONNEL"))
			{
				return cusCode.OK_CustomsRegNo.ToUpper();
			}
			else
			{
				return cusCode.OK_RN_NKCodeCountry + cusCode.OK_CustomsRegNo.ToUpper();
			}
		}

		ZString IEuOrgCusCodeHelper.GetBranchSuffixesForBox44(CusEntryHeader entryHeader)
		{
			List<ZString> codesForBox44 = new List<ZString>();
			List<EoriSuffixAndType> suffixes = ((IEuOrgCusCodeHelper)this).GetBranchSuffixesListForBox44(entryHeader);
			if (suffixes.Count > 0)
			{
				foreach (EoriSuffixAndType suffixPair in suffixes)
				{
					codesForBox44.Add(suffixPair.Type + suffixPair.Number);
				}
				return ZString.Join(" ", codesForBox44.ToArray());
			}
			return ZString.Empty;
		}

		/// <summary>
		/// Gets a list looking like (123, AG),(456, BR),(789, BR)
		/// </summary>
		List<EoriSuffixAndType> IEuOrgCusCodeHelper.GetBranchSuffixesListForBox44(CusEntryHeader entryHeader)
		{
			List<EoriSuffixAndType> eoriSuffixes = new List<EoriSuffixAndType>();

			AddOrganisationEoriToListIfRelevant(2, entryHeader, eoriSuffixes);
			AddOrganisationEoriToListIfRelevant(8, entryHeader, eoriSuffixes);
			AddOrganisationEoriToListIfRelevant(14, entryHeader, eoriSuffixes);

			return eoriSuffixes;

			void AddOrganisationEoriToListIfRelevant(int boxNumberToIdentifyPartyType, CusEntryHeader entry, List<EoriSuffixAndType> suffixes)
			{
				switch (boxNumberToIdentifyPartyType)
				{
					case 2:
						AddOrgDetailsToListIfOrgNotNullForJobDocAddress(entry, suffixes, entry.Supplier, EoriSuffixAndType.EoriBranchStatementType);
						break;

					case 8:
						AddOrgDetailsToListIfOrgNotNullForJobDocAddress(entry, suffixes, entry.Importer, EoriSuffixAndType.EoriBranchStatementType);

						break;

					case 14:
						AddOrgDetailsToListIfOrgNotNullForOrgAddress(entry, suffixes, entry.Declaration.Declarant, EoriSuffixAndType.EoriDeclarantStatementType);
						break;

					default:
						throw new NotImplementedException("We can only add EORI prefixed for boxes 2, 8 or 14.  You passed in " + boxNumberToIdentifyPartyType.ToString());
				}
			}

			void AddOrgDetailsToListIfOrgNotNullForJobDocAddress(CusEntryHeader entry, List<EoriSuffixAndType> suffixes, JobDocAddress docAddress, string statementType)
			{
				if (docAddress != null && !docAddress.E2_AddressOverride)   // If overridden, we cannot have EBS branch suffixes.
				{
					AddOrgDetailsToListIfOrgNotNullForOrgAddress(entry, suffixes, docAddress.Address, statementType);
				}
			}

			void AddOrgDetailsToListIfOrgNotNullForOrgAddress(CusEntryHeader entry, List<EoriSuffixAndType> suffixes, OrgAddress orgAddress, ZString eoriPrefixType)
			{
				if (orgAddress != null)
				{
					ZString branchSuffix = orgAddress.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix, Core.Constants.CountryCodes.UnitedKingdom, orgAddress.PK);
					if (!branchSuffix.IsEmpty)
					{
						suffixes.Add(new EoriSuffixAndType(branchSuffix, eoriPrefixType));
					}
				}
			}
		}

		ZString GetEuIdentificationNumber(OrgAddress address, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			if (address == null)
			{
				return ZString.Empty;
			}

			address.Factory.AddFetchHint(OrgCusCodeSchema.OK_OA_PremisesAddress, address.PK);
			var eori = GetEORICodeCore(address, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, true);
			if (!eori.IsEmpty)
			{
				return eori;
			}

			return GetOtherValidNumberIfEoriNotFound(address.Header);
		}

		ZString GetEuIdentificationNumber(OrgHeader organisation, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched)
		{
			if (organisation == null)
			{
				return ZString.Empty;
			}

			organisation.Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, organisation.PK);
			var eori = GetEORICodeCore(organisation, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, true);
			if (!eori.IsEmpty)
			{
				return eori;
			}

			return GetOtherValidNumberIfEoriNotFound(organisation);
		}

		ZString GetOtherValidNumberIfEoriNotFound(OrgHeader organisation)
		{
			if (organisation == null)
			{
				return ZString.Empty;
			}

			ZString eoriFromTurnOrVat = GetTurnOrVatCodeOfThisOrg(organisation);
			if (eoriFromTurnOrVat.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
			{
				if (IsSimpleEoriEndingWith000(eoriFromTurnOrVat) || IsPrOrUngreg(eoriFromTurnOrVat))
				{
					return eoriFromTurnOrVat;
				}
				else if (eoriFromTurnOrVat.Length < 3)
				{
					// It's a VAT/TRN entry of fewer than 3 chars that is NOT 'PR'.  This is wrong. 
					return string.Empty;
				}
				else if (IsNonMainEoriBranch(eoriFromTurnOrVat))
				{
					ZString partialTurn = eoriFromTurnOrVat.Left(eoriFromTurnOrVat.Length - 3) + OrgCusCodeValidation.EU.BlankEoriRegSuffix;
					return partialTurn;
				}
			}
			if (eoriFromTurnOrVat.IsEmpty)
			{
				eoriFromTurnOrVat = GetOrgCusCode(organisation, premiseAddress: null, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, organisation.CountryCode, false, true);
				if (eoriFromTurnOrVat.IsEmpty)
				{
					eoriFromTurnOrVat = GetOrgCusCode(organisation, premiseAddress: null, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, countryOfIssuance: null, ignoreCountryOfIssuanceIfNotMatched: false, appendCountryCodePrefix: true);
				}
			}
			if (eoriFromTurnOrVat.IsEmpty)
			{
				// For US0500 etc
				eoriFromTurnOrVat = GetOrgCusCode(organisation, premiseAddress: null, OrgCusCode.CodeTypes.BrokerageRegistration, Core.Constants.CountryCodes.UnitedKingdom, ignoreCountryOfIssuanceIfNotMatched: false, appendCountryCodePrefix: true).SubstringSafe(2);
			}
			return eoriFromTurnOrVat;

			bool IsSimpleEoriEndingWith000(ZString turn)
			{
				return !turn.IsEmpty && turn.EndsWith(OrgCusCodeValidation.EU.BlankEoriRegSuffix);
			}

			bool IsPrOrUngreg(ZString turn)
			{
				return
					turn.EndsWith(((IEuOrgCusCodeHelper)this).PrivateEoriReg)
					||
					turn.EndsWith(((IEuOrgCusCodeHelper)this).UnregForEori);
			}

			bool IsNonMainEoriBranch(ZString eoriCode)
			{
				return !eoriCode.IsEmpty
					   &&
					   !eoriCode.EndsWith(OrgCusCodeValidation.EU.BlankEoriRegSuffix)
					   &&
					   !eoriCode.EndsWith(((IEuOrgCusCodeHelper)this).PrivateEoriReg)
					   &&
					   !eoriCode.EndsWith(((IEuOrgCusCodeHelper)this).UnregForEori);
			}

			string GetTurnOrVatCodeOfThisOrg(OrgHeader org)
			{
				var turnCode = GetOrgCusCode(org, premiseAddress: null, OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, countryOfIssuance: null, ignoreCountryOfIssuanceIfNotMatched: false, appendCountryCodePrefix: true);

				if (turnCode.IsEmpty)
				{
					var vatCode = org.GetEUVATCodeOfThisOrg();
					if (vatCode.IsEmpty)
					{
						return ZString.Empty;  // We have neither a VAT or a TRN record
					}
					if (vatCode.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
					{
						if (vatCode.Length == 9 + 2)  // 9 chars of VAT plus length of 'GB', e.g. GB123456789
						{
							vatCode += "000";  //The UK TURN is the VAT number plus a 3-char suffix.  What is stored in the VAT column will be either 9 or 12 chars.
						}
					}
					turnCode = vatCode;
				}

				return turnCode;
			}
		}

		protected virtual ZString GetEORICodeCore(OrgHeader organisation, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched, bool appendCountryCodePrefix)
		{
			return organisation == null ? ZString.Empty : GetOrgCusCode(organisation, premiseAddress: null, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, appendCountryCodePrefix);
		}

		protected virtual ZString GetEORICodeCore(OrgAddress address, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched, bool appendCountryCodePrefix)
		{
			return address == null ? ZString.Empty : GetOrgCusCode(address.Header, premiseAddress: address, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, appendCountryCodePrefix);
		}

		ZString GetOrgCusCode(OrgHeader organisation, OrgAddress premiseAddress, string typeOfCodeEoriTurnEtc, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched, bool appendCountryCodePrefix)
		{
			var result = ZString.Empty;

			if (organisation != null)
			{
				string lastCountryCodeUsed = null;
				if (countryOfIssuance == null && UseCurrentLoggedInCountryWhenNotSpecified)
				{
					lastCountryCodeUsed = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode).ToUpperInvariant();
					result = GetOrgCusCode(organisation, premiseAddress: premiseAddress, typeOfCodeEoriTurnEtc, countryOfIssuance: lastCountryCodeUsed, appendCountryCodePrefix: appendCountryCodePrefix);
					if (result.IsEmpty && premiseAddress != null)
					{
						result = GetOrgCusCode(organisation, premiseAddress: null, typeOfCodeEoriTurnEtc, countryOfIssuance: lastCountryCodeUsed, appendCountryCodePrefix: appendCountryCodePrefix);
					}
				}
				if (lastCountryCodeUsed == null || lastCountryCodeUsed != countryOfIssuance)
				{
					if (result.IsEmpty)
					{
						result = GetOrgCusCode(organisation, premiseAddress: premiseAddress, typeOfCodeEoriTurnEtc, countryOfIssuance: countryOfIssuance, appendCountryCodePrefix: appendCountryCodePrefix);
					}
					if (result.IsEmpty && premiseAddress != null)
					{
						result = GetOrgCusCode(organisation, premiseAddress: null, typeOfCodeEoriTurnEtc, countryOfIssuance: countryOfIssuance, appendCountryCodePrefix: appendCountryCodePrefix);
					}
				}
				if (ignoreCountryOfIssuanceIfNotMatched && result.IsEmpty && countryOfIssuance != null)
				{
					result = GetOrgCusCode(organisation, premiseAddress: premiseAddress, typeOfCodeEoriTurnEtc, countryOfIssuance: null, appendCountryCodePrefix: appendCountryCodePrefix);
					if (result.IsEmpty && premiseAddress != null)
					{
						result = GetOrgCusCode(organisation, premiseAddress: null, typeOfCodeEoriTurnEtc, countryOfIssuance: null, appendCountryCodePrefix: appendCountryCodePrefix);
					}
				}
			}

			return result;
		}

		protected ZString GetOrgCusCode(OrgHeader organisation, OrgAddress premiseAddress, string typeOfCodeEoriTurnEtc, string countryOfIssuance, bool appendCountryCodePrefix)
		{
			var result = ZString.Empty;

			if (organisation != null)
			{
				var query = new ZQuery(OrgCusCodeSchema.OK_OH, organisation.PK);
				query.AddToFilter(OrgCusCodeSchema.OK_CodeType, typeOfCodeEoriTurnEtc);
				if (premiseAddress != null)
				{
					query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, premiseAddress.PK);
				}
				else
				{
					query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, null);
				}

				if (countryOfIssuance != null && countryOfIssuance != EconomicGroupList.Codes.EuropeanUnion)
				{
					query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryOfIssuance);
				}
				query.OrderBy = OrgCusCodeSchema.OK_SystemCreateTimeUtc.Name;

				var registeredCode = (OrgCusCode)null;
				if (countryOfIssuance == EconomicGroupList.Codes.EuropeanUnion && typeOfCodeEoriTurnEtc == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori)
				{
					var registeredCodes = organisation.Factory.Load<OrgCusCode>(query);
					registeredCode = registeredCodes.FirstOrDefault(x => x.OK_RN_NKCodeCountry != CountryCodes.UnitedKingdom && x.CountryIsMemberOf(EconomicGroupList.Codes.EuropeanUnion));
					if (registeredCode == null)
					{
						registeredCode = registeredCodes.FirstOrDefault(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedKingdom && x.OK_CustomsRegNo.StartsWith(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, StringComparison.OrdinalIgnoreCase));
					}
				}
				else
				{
					registeredCode = organisation.Factory.LoadTop1<OrgCusCode>(query);
				}

				if (registeredCode != null)
				{
					if (appendCountryCodePrefix)
					{
						result = AppendCountryCodePrefix(registeredCode);
					}
					else
					{
						result = registeredCode.OK_CustomsRegNo.ToUpper();
					}
				}
			}

			return result;
		}

		protected virtual bool UseCurrentLoggedInCountryWhenNotSpecified => false;
	}
}
