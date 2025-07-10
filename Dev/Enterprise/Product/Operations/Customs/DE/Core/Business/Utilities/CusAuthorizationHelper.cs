using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes;
using static Enterprise.Customs.DE.Business.CusAuthorisationRuleTypeList.Codes;
using static Enterprise.Customs.DE.Business.CusAuthorisationUsageRuleList.Codes;
using static Enterprise.Customs.DE.Business.ImportDeclarationTypeList.Codes;
using static Enterprise.Customs.EU.Business.RepresentationTypeList.Codes;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business
{
	public static class CusAuthorizationHelper
	{
		public static CodeDescriptionPairList GetCachedAuthorizationNumbers(BusinessObjectFactory factory, ZString[] types, ZGuid[] authorizationHolders, ZDate transactionDate)
		{
			var effectiveAuthorizationHolders = authorizationHolders.Where(x => !x.IsEmpty).OrderBy(x => x).ToArray();
			if (effectiveAuthorizationHolders.Length == 0)
			{
				return new CodeDescriptionPairList();
			}
			else
			{
				return factory.GetCachedValue(string.Join("|", "GetCachedAuthorizationNumbers|DE", string.Join("_", types.OrderBy(x => x)), string.Join("_", effectiveAuthorizationHolders), transactionDate), () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPairsIfNotExist(CusAuthorisationHeader.Loader.GetAuthorisations(factory, Core.Constants.CountryCodes.Germany, types, transactionDate, effectiveAuthorizationHolders));
					result.Sort();
					return result;
				});
			}
		}

		public static CodeDescriptionPairList GetCachedAuthorizationNumbersForAddresses(BusinessObjectFactory factory, ZString[] types, ZGuid[] authorizationAddresses, ZDate transactionDate)
		{
			var effectiveAuthorizationAddresses = authorizationAddresses.Where(x => !x.IsEmpty).OrderBy(x => x).ToArray();
			if (effectiveAuthorizationAddresses.Length == 0)
			{
				return new CodeDescriptionPairList();
			}
			else
			{
				return factory.GetCachedValue(string.Join("|", "GetCachedAuthorizationNumbersForAddresses|DE", string.Join("_", types.OrderBy(x => x)), string.Join("_", effectiveAuthorizationAddresses), transactionDate), () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPairsIfNotExist(CusAuthorisationHeader.Loader.GetAuthorisationsForAddresses(factory, Core.Constants.CountryCodes.Germany, types, transactionDate, effectiveAuthorizationAddresses));
					result.Sort();
					return result;
				});
			}
		}

		public static ZString ValidateRequiredAuthorisationForStyle(JobDeclaration declaration, CusEntryInstruction instruction, bool seekForDeclarant, bool seekForRepresentative, bool seekForBuyingAgent)
		{
			var errorMessage = ZString.Empty;

			var organisationAuthorisationValidators = GetOrganisationAuthorisationValidators(declaration, instruction);
			foreach (var (isForDeclarant, isForRepresentative, isForBuyingAgent, isApplicableAndNoRequiredAuthorisation, message) in organisationAuthorisationValidators)
			{
				if ((seekForDeclarant && isForDeclarant) || (seekForRepresentative && isForRepresentative) || (seekForBuyingAgent && isForBuyingAgent))
				{
					if (isApplicableAndNoRequiredAuthorisation())
					{
						errorMessage = message;
						break;
					}
				}
			}
			return errorMessage;
		}

		public static IEnumerable<ZString> ValidateRequiredCusAuthorisationUsageForStyleImport(CusEntryInstruction instruction)
		{
			foreach (var (isApplicableAndNoRequiredAuthorisation, message) in GetCusAuthorisationUsageRequirementValidatorsImport(instruction))
			{
				if (isApplicableAndNoRequiredAuthorisation())
				{
					yield return message;
				}
			}
		}

		public static IEnumerable<ZString> ValidateRequiredCusAuthorisationUsageForStyleExport(CusEntryInstruction instruction)
		{
			foreach (var (isApplicableAndNoRequiredAuthorisation, message) in GetCusAuthorisationUsageRequirementValidatorsExport(instruction))
			{
				if (isApplicableAndNoRequiredAuthorisation())
				{
					yield return message;
				}
			}
		}

		public static ZBool HasAuthorisationForBondedWarehouse(this OrgHeader header, ZString authorisationType)
		{
			return header.HasAuthorizationWithRule(authorisationType, Usage, ZDate.Today, new ZString[] { CustomsWarehousingCWP, CustomsWarehousingCW1 });
		}

		public static CodeDescriptionPairList GetCachedAuthorizationRules(BusinessObjectFactory factory, ZString authorizationType, ZGuid authorizationHolderPk, ZString ruleCode, ZString linkedRuleCode, ZString linkedRuleValue, ZDate transactionDate, string authorizationNumber = "")
		{
			return factory.GetCachedValue(string.Join("|", "GetCachedAuthorizationRules|DE", authorizationType, authorizationHolderPk, ruleCode, linkedRuleCode, linkedRuleValue, transactionDate, authorizationNumber), () =>
			{
				var result = new CodeDescriptionPairList();
				if (!authorizationHolderPk.IsEmpty && !linkedRuleValue.IsEmpty)
				{
					var authorizationList = CusAuthorisationHeader.Loader.GetAuthorisations(factory, Core.Constants.CountryCodes.Germany, new[] { authorizationType }, transactionDate, authorizationHolderPk);
					if (!string.IsNullOrEmpty(authorizationNumber))
					{
						authorizationList = authorizationList.Where(x => x.CPH_Number == authorizationNumber).ToArray();
					}
					var authorizationRuleList = authorizationList.SelectMany(x => x.CusAuthorisationRules).Where(rule =>
						rule.CPR_RuleCode == ruleCode && rule.LinkedCusAuthorisationRules.Any(linkedRule => linkedRule.CPR_RuleCode == linkedRuleCode && linkedRule.CPR_ValueFrom == linkedRuleValue)).ToArray();
					result.AddRange(authorizationRuleList);
					result.Sort();
				}
				return result;
			});
		}

		public static CodeDescriptionPairList GetLocationOfGoodsListForTemporaryStorage(this OrgHeader header, ZString customsOffice)
		{
			var result = new CodeDescriptionPairList();
			if (header != null)
			{
				result = GetCachedAuthorizationRules(header.Factory
					, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage
					, header.PK
					, Customs.Business.CusAuthorisationRuleTypeList.Codes.Location
					, Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice
					, customsOffice
					, ZDate.Today);
			}
			return result;
		}

		public static CodeDescriptionPairList GetCachedRuleValues(BusinessObjectFactory factory, ZString authorisationType, ZGuid authorisationHolderPk, ZString ruleCode, ZString ruleValueType, ZString attributeFilterName, ZString attributeFilterValue)
		{
			var transactionDate = ZDate.Today;
			return factory.GetCachedValue(string.Join("|", "AuthorisationRuleValues|DE", authorisationType, authorisationHolderPk, ruleCode, ruleValueType, attributeFilterName, attributeFilterValue, transactionDate), () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(GetAuthorisationRuleValues(factory, authorisationType, authorisationHolderPk, ruleCode, ruleValueType, attributeFilterName, attributeFilterValue, transactionDate));
				result.Sort();
				return result;
			});
		}

		static ZZRefCusCodeListCombined[] GetAuthorisationRuleValues(BusinessObjectFactory factory, ZString authorisationType, ZGuid authorisationHolderPk, ZString ruleCode, ZString ruleValueType, ZString attributeFilterName, ZString attributeFilterValue, ZDate transactionDate)
		{
			var filteredRuleValues = Array.Empty<ZZRefCusCodeListCombined>();
			if (!authorisationHolderPk.IsEmpty && !attributeFilterValue.IsEmpty)
			{
				var authorisations = CusAuthorisationHeader.Loader.GetAuthorisations(factory
				, Core.Constants.CountryCodes.Germany
				, new ZString[] { authorisationType }
				, transactionDate
				, authorisationHolderPk);

				var authorisationRuleValues = authorisations.SelectMany(x => x.CusAuthorisationRules).Where(x => x.CPR_RuleCode == ruleCode).Select(x => x.CPR_ValueFrom).Distinct().ToArray();
				if (authorisationRuleValues.Any())
				{
					var attributeFilterList = new List<RefCusCodeListAttributeFilter>() { new RefCusCodeListAttributeFilter(attributeFilterName, SQLComparisonOperator.Equal, attributeFilterValue) };
					filteredRuleValues = ZZRefCusCodeListCombined.Loader.LoadForCodes(factory,
						Core.Constants.CountryCodes.Germany,
						ruleValueType,
						authorisationRuleValues,
						transactionDate,
						attributeFilterList);
				}
			}
			return filteredRuleValues;
		}

		public static CodeDescriptionPairList GetCachedRuleValuesFilteredByLinkedRules(BusinessObjectFactory factory, ZString authorizationType, ZGuid[] authorizationHolderPks, ZString targetRuleCode, ZString filterRuleCode, ZString filterRuleValue, ZString linkedRuleCode, ZString linkedRuleValue)
		{
			var transactionDate = ZDate.Today;
			var effectiveAuthorizationHolders = authorizationHolderPks.Where(x => !x.IsEmpty).OrderBy(x => x).ToArray();
			return factory.GetCachedValue(string.Join("|", "AuthorizationRuleValuesFilteredByLinkedRules|DE", authorizationType, string.Join("_", effectiveAuthorizationHolders), targetRuleCode, filterRuleCode, filterRuleValue, linkedRuleCode, linkedRuleValue, transactionDate), () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPairsIfNotExist(GetAuthorizationRuleValuesFilteredByLinkedRules(factory, authorizationType, effectiveAuthorizationHolders, targetRuleCode, filterRuleCode, filterRuleValue, linkedRuleCode, linkedRuleValue, transactionDate));
				result.Sort();
				return result;
			});
		}

		static CusAuthorisationRule[] GetAuthorizationRuleValuesFilteredByLinkedRules(BusinessObjectFactory factory, ZString authorizationType, ZGuid[] authorizationHolderPks, ZString targetRuleCode, ZString filterRuleCode, ZString filterRuleValue, ZString linkedRuleCode, ZString linkedRuleValue, ZDate transactionDate)
		{
			var authorizationRules = Array.Empty<CusAuthorisationRule>();
			if (!authorizationType.IsEmpty && authorizationHolderPks.Any())
			{
				var authorizations = CusAuthorisationHeader.Loader.GetAuthorisations(factory
					, Core.Constants.CountryCodes.Germany
					, new[] { authorizationType }
					, transactionDate
					, authorizationHolderPks);

				authorizationRules = authorizations.Where(x => x.CusAuthorisationRules.Any(rule => rule.CPR_RuleCode == filterRuleCode && rule.CPR_ValueFrom == filterRuleValue))
					.SelectMany(x => x.CusAuthorisationRules)
					.Where(rule => rule.CPR_RuleCode == targetRuleCode && rule.LinkedCusAuthorisationRules.Any(linkedRule => linkedRule.CPR_RuleCode == linkedRuleCode && linkedRule.CPR_ValueFrom == linkedRuleValue))
					.ToArray();
			}
			return authorizationRules;
		}

		public static ZString GetAuthorizationNumber(this OrgAddress orgAddress, ZString type) => orgAddress?.Header?.GetAuthorizationNumber(type) ?? ZString.Empty;

		public static ZString GetAuthorizationNumber(this OrgHeader orgHeader, ZString type) => orgHeader != null ? CusAuthorisationHeader.Loader.GetAuthorisationNumber(orgHeader.Factory, Core.Constants.CountryCodes.Germany, type, ZDate.Today, orgHeader.PK) : ZString.Empty;

		public static ZString GetAuthorizationNumber(this OrgHeader orgHeader, ZString type, ZDate transactionDate) => orgHeader != null ? CusAuthorisationHeader.Loader.GetAuthorisationNumber(orgHeader.Factory, Core.Constants.CountryCodes.Germany, type, transactionDate, orgHeader.PK) : ZString.Empty;

		public static CusAuthorisationHeader GetAuthorization(this OrgHeader orgHeader, ZString type) => orgHeader != null ? CusAuthorisationHeader.Loader.GetAuthorisation(orgHeader.Factory, Core.Constants.CountryCodes.Germany, type, ZDate.Today, orgHeader.PK) : null;

		public static ZBool HasInwardProcessingAuthorization(this OrgHeader orgHeader) => HasAuthorization(orgHeader, new ZString[] { InwardProcessing });

		public static ZBool HasConsignorTransitAuthorization(this OrgHeader orgHeader) => HasAuthorization(orgHeader, new ZString[] { AuthorizedConsignorTransit });

		public static ZBool HasOutwardProcessingAuthorization(this OrgHeader orgHeader) => HasAuthorization(orgHeader, new ZString[] { OutwardProcessing });

		public static ZBool HasAuthorization(this OrgHeader orgHeader, ZString[] authorizationTypes) => orgHeader != null && CusAuthorisationHeader.Loader.GetAuthorisations(orgHeader.Factory, Core.Constants.CountryCodes.Germany, authorizationTypes, ZDateTime.Today, orgHeader.PK).Any();

		public static ZBool HasAuthorization(this OrgHeader[] orgHeaders, ZString[] authorizationTypes)
		{
			if (orgHeaders != null && orgHeaders.Length > 0)
			{
				foreach (var orgHeader in orgHeaders)
				{
					if (orgHeader.HasAuthorization(authorizationTypes))
					{
						return ZBool.True;
					}
				}
			}
			return ZBool.False;
		}

		public static ZBool HasAuthorization(this OrgAddress orgAddress, ZString[] authorizationTypes) => orgAddress?.Header?.HasAuthorization(authorizationTypes) ?? ZBool.False;

		public static CusAuthorisationHeader GetAuthorisationWithRule(this OrgHeader orgHeader, ZString authorizationType, ZString ruleCode, ZString[] ruleValues = null, bool matchAllValues = false)
			=> orgHeader.GetAuthorisationWithRule(authorizationType, ruleCode, ZDate.Today, ruleValues, matchAllValues);

		public static CusAuthorisationHeader GetAuthorisationWithRule(this OrgHeader orgHeader, ZString authorizationType, ZString ruleCode, ZDate transactionDate, ZString[] ruleValues = null, bool matchAllValues = false)
		{
			CusAuthorisationHeader result = null;
			if (orgHeader != null)
			{
				var authorisations = CusAuthorisationHeader.Loader.GetAuthorisations(orgHeader.Factory, Core.Constants.CountryCodes.Germany, new[] { authorizationType }, transactionDate, orgHeader.PK);
				if (ruleValues?.Any() ?? false)
				{
					result = matchAllValues ? authorisations.FirstOrDefault(x => ruleValues.All(ruleValue => x.CusAuthorisationRules.Any(y => y.CPR_RuleCode == ruleCode && y.CPR_ValueFrom == ruleValue)))
					: authorisations.FirstOrDefault(x => x.CusAuthorisationRules.Any(rule => rule.CPR_RuleCode == ruleCode && ruleValues.Contains(rule.CPR_ValueFrom)));
				}
				else
				{
					result = authorisations.FirstOrDefault(x => x.CusAuthorisationRules.Any(rule => rule.CPR_RuleCode == ruleCode));
				}
			}
			return result;
		}

		public static ZBool HasAuthorizationWithRule(this OrgHeader orgHeader, ZString authorizationType, ZString ruleCode, ZDate transactionDate, ZString[] ruleValues = null, bool matchAllValues = false)
		{
			return orgHeader.GetAuthorisationWithRule(authorizationType, ruleCode, transactionDate, ruleValues, matchAllValues) != null;
		}

		public static ZBool HasAuthorizationWithRule(this OrgHeader[] orgHeaders, ZString authorizationType, ZString ruleCode, ZDate transactionDate, ZString[] ruleValues = null, bool matchAllValues = false)
		{
			var result = ZBool.False;
			if (orgHeaders != null && orgHeaders.Length > 0)
			{
				foreach (var orgHeader in orgHeaders)
				{
					if (orgHeader.HasAuthorizationWithRule(authorizationType, ruleCode, transactionDate, ruleValues, matchAllValues))
					{
						result = ZBool.True;
						break;
					}
				}
			}
			return result;
		}

		public static ZBool HasCusAuthorizationUsageWithRule(this CusEntryInstruction entryInstruction, ZString authorizationType, ZString ruleCode, ZString valueFrom)
		{
			var result = ZBool.False;
			var eirAuthorization = entryInstruction.CusAuthorizationUsages.Cast<EU.Business.CusAuthorizationUsage>().FirstOrDefault(x => x.AGC_Code == authorizationType);
			if (eirAuthorization != null)
			{
				result = CusAuthorisationHeader.Loader.HolderHasSpecificAuthorisationWithRule(entryInstruction.Factory,
					eirAuthorization.AGC_OH_Owner,
					authorizationType,
					eirAuthorization.AGC_Number,
					Core.Constants.CountryCodes.Germany,
					ZDateTime.Today,
					ruleCode,
					valueFrom);
			}
			return result;
		}

		public static ZBool HasCusAuthorizationUsage(this CusEntryInstruction entryInstruction, string authorizationType)
		{
			return entryInstruction.CusAuthorizationUsages.Cast<EU.Business.CusAuthorizationUsage>().Any(x => x.AGC_Code == authorizationType);
		}

		public static ZBool RequiresAuthorizationRuleMandateReference(this CusAuthorisationHeader authorizationHeader)
		{
			return authorizationHeader.CusAuthorisationRules.Any(rc => rc.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Usage && rc.CPR_ValueFrom == CusAuthorisationUsageRuleList.Codes.FreeCirculation);
		}

		public static ZBool RequiresAuthorizationRuleRelease(this CusAuthorisationHeader authorizationHeader)
		{
			return authorizationHeader.CusAuthorisationRules.Any(rc => rc.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Usage && UsageValuesRequiringRuleCodeREL.Contains(rc.CPR_ValueFrom));
		}

		public static ZString DeclarantRequiresEndUserAuthorization => Res.GetString("149E60AF-13B0-4A05-8AC4-9182FAB8F357", "The Declarant must have an End Use Authorization (EUS) for this declaration.");

		public static ZString DeclarantRequiresCustomsWarehousingAuthorizationMessage => Res.GetString("3A8B8920-391F-4666-92DB-11381FAFD94C", "Declarant must have an Authorization of Type 'CWP' or 'CW1' for this Declaration Type.");

		public static ZString DeclarantRequiresAuthorizationTypeEIR => Res.GetString("08A34DF4-E238-4B8D-AD6C-9A7270A564D8", "The selected Declaration Type requires the Declarant to have an EIR Authorization for Free Circulation in Local Clearance Date.");

		public static ZString DeclarantOrRepresentativeRequiresAuthorizationTypeEIR => Res.GetString("EE6397CA-C444-43F2-91DB-0C5BE9F18DBD", "The selected Declaration Type requires the Declarant or Representative to have an EIR Authorization for Free Circulation in Local Clearance Date.");

		public static ZString DeclarantOrRepresentedPartyRequiresAuthorizationTypeEIR => Res.GetString("EB1EB306-42DB-4330-9CF6-D37615E76C59", "The selected Declaration Type requires the Declarant or Represented Party to have an EIR Authorization for Free Circulation in Local Clearance Date.");

		public static ZString DeclarantRequiresAuthorizationTypeSDE => Res.GetString("B5CFAE3E-0925-4E60-AF98-6754A8FEAD75", "The selected Declaration Type requires the Declarant to have an SDE Authorization for Free Circulation.");

		public static ZString DeclarantOrRepresentativeRequiresAuthorizationTypeSDE => Res.GetString("980652AB-7B96-4DE7-86F7-DDC270173BFC", "The selected Declaration Type requires the Declarant or Representative to have an SDE Authorization for Free Circulation.");

		public static ZString DeclarantOrRepresentedPartyRequiresAuthorizationTypeSDE => Res.GetString("AA5DDBA6-5DA3-4329-89A9-04F9F65617D9", "The selected Declaration Type requires the Declarant or Represented Party to have an SDE Authorization for Free Circulation.");

		public static ZString DeclarantRequiresEIRAuthorisationForBondedWarehouseMessage => Res.GetString("D57F8AF9-0944-4DA8-A044-B31417809C5A", "For this Declaration Type the Declarant must have an EIR Authorization for Bonded Warehouse.");

		public static ZString DeclarantRequiresSDEAuthorisationForBondedWarehouseMessage => Res.GetString("83B45559-A536-468E-A176-C2B72E40765A", "For this Declaration Type the Declarant must have an SDE Authorization for Bonded Warehouse.");

		public static ZString DeclarantRequiresEIRAuthorizationForInwardProcessingMessage => Res.GetString("BD5BE414-10D7-4A8C-9078-05F67680DA3B", "For this Declaration Type the Declarant must have an EIR Authorization for Inward Processing.");

		public static ZString DeclarantRequiresSDEAuthorizationForInwardProcessingMessage => Res.GetString("E8DF80BF-5E7C-471B-AE19-E5DCE802ECE4", "For this Declaration Type the Declarant must have an SDE Authorization for Inward Processing.");

		public static ZString DeclarantAZRequiresEIRAuthorization => Res.GetString("0087C5C9-3C7A-458B-8A95-D796ACA64FB4", "For Declaration Type 'AZ' an Authorization record of Type ‘EIR’ is mandatory.");

		public static ZString DeclarantVZARequiresSDEAuthorization => Res.GetString("83F2B9AB-C974-431E-8EBC-2463E39306C7", "For Declaration Type 'VZA' an Authorization record of Type ‘SDE’ is mandatory.");

		public static ZString DeclarantAAVRequiresEIRAuthorization => Res.GetString("CB9E6DD6-70B0-463C-A2C8-627710B4EDE9", "For Declaration Type 'AAV' an Authorization record of Type ‘EIR’ is mandatory.");

		public static ZString DeclarantAAVRequiresIPOAuthorization => Res.GetString("C22CE357-A810-46C3-B53D-8CB2FA1E02B8", "For Declaration Type 'AAV' an Authorization record of Type ‘IPO’ is mandatory.");

		public static ZString DeclarantVAVRequiresSDEAuthorization => Res.GetString("FA289F71-AA34-42EB-BD91-B0268113EB2B", "For Declaration Type 'VAV' an Authorization record of Type ‘SDE’ is mandatory.");

		public static ZString DeclarantVAVRequiresIPOAuthorization => Res.GetString("B49B1EF9-E774-47EB-A6F8-0E9F7385C1B2", "For Declaration Type 'VAV' an Authorization record of Type ‘IPO’ is mandatory.");

		public static ZString DeclarantAZLRequiresEIRAuthorization => Res.GetString("1B03AD26-ECFA-4309-BDBC-7B5556F49C2A", "For Declaration Type 'AZL' an Authorization record of Type ‘EIR’ is mandatory.");

		public static ZString DeclarantAZLRequiresCWPOrCW1Authorization => Res.GetString("3FB67714-00D3-42C7-B524-2534E717747D", "For Declaration Type 'AZL' an Authorization record of Type ‘CWP’ or ‘CW1’ is mandatory.");

		public static ZString DeclarantAZLRequiresCW1Authorization => Res.GetString("CE32430C-91BE-4444-A234-67615EE37E61", "For Declaration Type 'AZL' an Authorization record of Type ‘CW1’ is mandatory.");

		public static ZString DeclarantINDMustNotHaveCWPAuthorization => Res.GetString("84E90C0C-5D45-4883-86E5-43863C25FED3", "For Declaration with Representative Type ‘IND’ an Authorization from Type ‘CWP’ is not allowed.");

		public static ZString DeclarantVZLRequiresSDEAuthorization => Res.GetString("820D2DA2-657F-41E2-B703-E9094F165616", "For Declaration Type 'VZL' an Authorization record of Type ‘SDE’ is mandatory.");

		public static ZString DeclarantVZLRequiresCWPOrCW1Authorization => Res.GetString("3B7DE591-9F6A-4E1F-846A-AC61AF37F966", "For Declaration Type 'VZL' an Authorization record of Type ‘CWP’ or ‘CW1’ is mandatory.");

		public static ZString DeclarantVZLRequiresCW1Authorization => Res.GetString("016BEF21-A8E5-464D-B495-0672A0328BD9", "For Declaration Type 'VZL' an Authorization record of Type ‘CW1’ is mandatory.");

		public static ZString AuthHolderAndDeclarantToBeSame => Res.GetString("5E7B0129-CFFE-449E-B5A6-0050B5040044", "Authorization Holder and Declarant must be equal.");

		public static ZString AuthHolderAndDeclarantOrRepresentativeToBeSame => Res.GetString("99E4AF17-2EE6-43CD-98E2-A8FDF62726A0", "Authorization Holder and Declarant or Representative must be equal.");

		public static ZString AuthHolderAndDeclarantOrRepresentedPartyToBeSame => Res.GetString("0061BB63-0D62-4150-A736-C67C034DA95A", "Authorization Holder and Declarant or Represented Party must be equal.");

		public static ZString AuthHolderSdeAndCclToBeSame => Res.GetString("71950F44-5426-418A-A688-8466B210D593", "Authorization Holder of Type ‘SDE‘ and ‘CCL‘ must be equal.");

		public static ZString DeclarantEZLRequiresCWPOrCW1Authorization => Res.GetString("91863B82-3114-4E1B-88B1-F0C4C3125A30", "For Declaration Type 'EZL' an Authorization record of Type ‘CWP’ or ‘CW1’ is mandatory.");

		public static readonly ImmutableArray<ZString> SupportingDocumentTypesRequiringEndOfUseAuthorisation = new ZString[]
		{
			UniversalReferenceConstants.SupportingDocumentTypes.C990,
			UniversalReferenceConstants.SupportingDocumentTypes.D019,
			UniversalReferenceConstants.SupportingDocumentTypes.N990
		}.ToImmutableArray();

		static readonly ImmutableHashSet<string> UsageValuesRequiringRuleCodeREL = new[]
		{
			CusAuthorisationUsageRuleList.Codes.FreeCirculation,
			CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure,
			CusAuthorisationUsageRuleList.Codes.CustomsWarehousing,
			CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1
		}.ToImmutableHashSet();

		public static ZString ValidateCusAuthorisationUsageOwner(CusAuthorizationUsage cusAuthorizationUsage)
		{
			var instruction = cusAuthorizationUsage.Instruction;
			var declaration = instruction?.JobDeclaration;
			var errorMessage = ZString.Empty;

			if (declaration != null)
			{
				var style = instruction.CEI_Style;
				var code = cusAuthorizationUsage.AGC_Code;
				var authHolder = cusAuthorizationUsage.AGC_OH_Owner;
				var declarantType = declaration.JE_DeclarantType;
				var declarant = declaration.DeclarantOrgAddress?.OA_OH ?? ZGuid.Empty;
				var representative = declaration.RepresentativeOrgAddress?.OA_OH ?? ZGuid.Empty;
				var representedParty = declaration.BuyingAgentAddress?.OA_OH ?? ZGuid.Empty;

				if (declaration.IsImport)
				{
					errorMessage = GetCusAuthorisationUsageOwnerValidatorsImport().FirstOrDefault(x => x.validationConditionMet).message;

					IEnumerable<(bool validationConditionMet, string message)> GetCusAuthorisationUsageOwnerValidatorsImport()
					{
						yield return (style == AZ && declarantType == _1Self && code == EntryOfDataInTheDeclarantsRecords && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return (style == VZA && declarantType == _1Self && code == SimplifiedDeclaration && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return ((style == AZ || style == VZA) && code == EndUse && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return (style == AZ && declarantType == _2Direct && code == EntryOfDataInTheDeclarantsRecords && declarant != authHolder && representative != authHolder, AuthHolderAndDeclarantOrRepresentativeToBeSame);
						yield return (style == VZA && declarantType == _2Direct && code == SimplifiedDeclaration && declarant != authHolder && representative != authHolder, AuthHolderAndDeclarantOrRepresentativeToBeSame);
						yield return (style == AZ && declarantType == _3Indirect && code == EntryOfDataInTheDeclarantsRecords && declarant != authHolder && representedParty != authHolder, AuthHolderAndDeclarantOrRepresentedPartyToBeSame);
						yield return (style == VZA && declarantType == _3Indirect && code == SimplifiedDeclaration && declarant != authHolder && representedParty != authHolder, AuthHolderAndDeclarantOrRepresentedPartyToBeSame);

						yield return (style == AAV && declarantType == _1Self && code == EntryOfDataInTheDeclarantsRecords && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return (style == AAV && declarantType == _2Direct && code == EntryOfDataInTheDeclarantsRecords && declarant != authHolder && representative != authHolder, AuthHolderAndDeclarantOrRepresentativeToBeSame);
						yield return (style == AAV && declarantType == _3Indirect && code == EntryOfDataInTheDeclarantsRecords && declarant != authHolder && representedParty != authHolder, AuthHolderAndDeclarantOrRepresentedPartyToBeSame);
						yield return (style == VAV && declarantType == _1Self && code == SimplifiedDeclaration && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return (style == VAV && declarantType == _2Direct && code == SimplifiedDeclaration && declarant != authHolder && representative != authHolder, AuthHolderAndDeclarantOrRepresentativeToBeSame);
						yield return (style == VAV && declarantType == _3Indirect && code == SimplifiedDeclaration && declarant != authHolder && representedParty != authHolder, AuthHolderAndDeclarantOrRepresentedPartyToBeSame);
						yield return ((style == AAV || style == VAV) && declarantType.In(new ZString[] { _1Self, _2Direct }) && code == InwardProcessing && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return ((style == AAV || style == VAV) && declarantType == _3Indirect && code == InwardProcessing && declarant != authHolder && representedParty != authHolder, AuthHolderAndDeclarantOrRepresentedPartyToBeSame);

						yield return ((style == AZL || style == VZL) && code == CustomsWarehousingCWP && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return (style == AZL && declarantType == _1Self && code == EntryOfDataInTheDeclarantsRecords && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return (style == AZL && declarantType == _2Direct && code == EntryOfDataInTheDeclarantsRecords && declarant != authHolder && representative != authHolder, AuthHolderAndDeclarantOrRepresentativeToBeSame);
						yield return ((style == AZL || style == VZL) && declarantType == _2Direct && code == CustomsWarehousingCW1 && declarant != authHolder && representative != authHolder, AuthHolderAndDeclarantOrRepresentativeToBeSame);
						yield return (style == VZL && declarantType == _1Self && code == SimplifiedDeclaration && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return (style == VZL && declarantType == _2Direct && code == SimplifiedDeclaration && declarant != authHolder && representative != authHolder, AuthHolderAndDeclarantOrRepresentativeToBeSame);
						yield return (style == EZL && code == CustomsWarehousingCWP && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
					}
				}
				else if (declaration.IsExport)
				{
					errorMessage = GetCusAuthorisationUsageOwnerValidatorsExport().FirstOrDefault(x => x.validationConditionMet).message;

					IEnumerable<(bool validationConditionMet, string message)> GetCusAuthorisationUsageOwnerValidatorsExport()
					{
						yield return (code == EntryOfDataInTheDeclarantsRecords && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return (code == SimplifiedDeclaration && instruction.IsSDEExport() && declarant != authHolder && (instruction.Constellation2ndDigitIs0And3rdIs0() || instruction.Constellation2ndDigitIs1And3rdIs0()), AuthHolderAndDeclarantToBeSame);
						yield return (code == SimplifiedDeclaration && instruction.IsSDEExport() && declarant != authHolder && representative != authHolder && (instruction.Constellation2ndDigitIs0And3rdIs1() || instruction.Constellation2ndDigitIs1And3rdIs1()), AuthHolderAndDeclarantOrRepresentativeToBeSame);
						yield return (code == SimplifiedDeclaration && instruction.IsSDEOutwardProcessing() && declarant != authHolder && instruction.Constellation3rdDigitIs0And4thIs0(), AuthHolderAndDeclarantToBeSame);
						yield return (code == SimplifiedDeclaration && instruction.IsSDEOutwardProcessing() && declarant != authHolder && representative != authHolder && instruction.Constellation3rdDigitIs1And4thIs0(), AuthHolderAndDeclarantOrRepresentativeToBeSame);
						yield return (code == OutwardProcessing && instruction.IsOPOOutwardProcessing() && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return (code == CentralizedClearance && instruction.Constellation3rdDigitIs0() && declarant != authHolder, AuthHolderAndDeclarantToBeSame);
						yield return (code == CentralizedClearance && instruction.Constellation3rdDigitIs1() && declarant != authHolder && representative != authHolder, AuthHolderAndDeclarantOrRepresentativeToBeSame);
						yield return (code == SimplifiedDeclaration && instruction.Style4thDigitIs4() && instruction.IsSDEExportOrSDEOutwardProcessing() && HasCusAuthorizationUsageWithDifferentOwner(instruction, CentralizedClearance, authHolder), AuthHolderSdeAndCclToBeSame);
						yield return (code == CentralizedClearance && instruction.Style4thDigitIs4() && instruction.IsSDEExportOrSDEOutwardProcessing() && HasCusAuthorizationUsageWithDifferentOwner(instruction, SimplifiedDeclaration, authHolder), AuthHolderSdeAndCclToBeSame);
					}
				}
			}

			return errorMessage;
		}

		static bool HasCusAuthorizationUsageWithDifferentOwner(CusEntryInstruction entryInstruction, string authorizationType, ZGuid authorizationOwner)
		{
			var result = false;
			var auth = entryInstruction.CusAuthorizationUsages.Cast<EU.Business.CusAuthorizationUsage>().FirstOrDefault(x => x.AGC_Code == authorizationType);
			if (auth != null)
			{
				result = auth.AGC_OH_Owner != authorizationOwner;
			}
			return result;
		}

		static List<(bool isForDeclarant, bool isForRepresentative, bool isForBuyingAgent, Func<ZBool>, ZString)> GetOrganisationAuthorisationValidators(JobDeclaration declaration, CusEntryInstruction instruction)
		{
			var declarantType = declaration.JE_DeclarantType;
			var declarantOrgHeader = declaration.DeclarantOrgAddress?.Header;
			var representativeOrgHeader = declaration.Representative?.Header;
			var buyingAgentOrgHeader = declaration.BuyingAgentAddress?.Header;
			var style = instruction.CEI_Style;
			var clearanceDate = instruction.CEI_LocalClearanceDate.Date;

			return new List<(bool, bool, bool, Func<ZBool>, ZString)>
			{
				(true, false, false, () => style == EZL && !declarantOrgHeader.HasAuthorization(new ZString[] { CustomsWarehousingCWP, CustomsWarehousingCW1 }), DeclarantRequiresCustomsWarehousingAuthorizationMessage),
				(true, false, false, () => style == AZL && !declarantOrgHeader.HasAuthorisationForBondedWarehouse(EntryOfDataInTheDeclarantsRecords), DeclarantRequiresEIRAuthorisationForBondedWarehouseMessage),
				(true, false, false, () => style == VZL && !declarantOrgHeader.HasAuthorisationForBondedWarehouse(SimplifiedDeclaration), DeclarantRequiresSDEAuthorisationForBondedWarehouseMessage),
				(true, false, false, () => style == AAV && !declarantOrgHeader.HasAuthorizationWithRule(EntryOfDataInTheDeclarantsRecords, Usage, ZDate.Today, new ZString[] { InwardProcessingProcedure }), DeclarantRequiresEIRAuthorizationForInwardProcessingMessage),
				(true, false, false, () => style == VAV && !declarantOrgHeader.HasAuthorizationWithRule(SimplifiedDeclaration, Usage, ZDate.Today, new ZString[] { InwardProcessingProcedure }), DeclarantRequiresSDEAuthorizationForInwardProcessingMessage),
				(true, false, false, () => style == AZ && declarantType == _1Self && !declarantOrgHeader.HasAuthorizationWithRule(EntryOfDataInTheDeclarantsRecords, Usage, clearanceDate, new ZString[] { FreeCirculation }), DeclarantRequiresAuthorizationTypeEIR),
				(true, false, false, () => style == VZA && declarantType == _1Self && !declarantOrgHeader.HasAuthorizationWithRule(SimplifiedDeclaration, Usage, ZDate.Today, new ZString[] { FreeCirculation }), DeclarantRequiresAuthorizationTypeSDE),
				(true, true, false, () => style == AZ && declarantType == _2Direct && !new OrgHeader[] { declarantOrgHeader, representativeOrgHeader }.HasAuthorizationWithRule(EntryOfDataInTheDeclarantsRecords, Usage, clearanceDate, new ZString[] { FreeCirculation }), DeclarantOrRepresentativeRequiresAuthorizationTypeEIR),
				(true, true, false, () => style == VZA && declarantType == _2Direct && !new OrgHeader[] { declarantOrgHeader, representativeOrgHeader }.HasAuthorizationWithRule(SimplifiedDeclaration, Usage, ZDate.Today, new ZString[] { FreeCirculation }), DeclarantOrRepresentativeRequiresAuthorizationTypeSDE),
				(true, false, true, () => style == AZ && declarantType == _3Indirect && !new OrgHeader[] { declarantOrgHeader, buyingAgentOrgHeader }.HasAuthorizationWithRule(EntryOfDataInTheDeclarantsRecords, Usage, clearanceDate, new ZString[] { FreeCirculation }), DeclarantOrRepresentedPartyRequiresAuthorizationTypeEIR),
				(true, false, true, () => style == VZA && declarantType == _3Indirect && !new OrgHeader[] { declarantOrgHeader, buyingAgentOrgHeader }.HasAuthorizationWithRule(SimplifiedDeclaration, Usage, ZDate.Today, new ZString[] { FreeCirculation }), DeclarantOrRepresentedPartyRequiresAuthorizationTypeSDE),
			};
		}

		static IEnumerable<(Func<ZBool>, ZString)> GetCusAuthorisationUsageRequirementValidatorsImport(CusEntryInstruction instruction)
		{
			var style = instruction.CEI_Style;
			var declarantType = instruction.JobDeclaration.JE_DeclarantType;

			yield return (() => style == AZ && !HasCusAuthorizationUsage(instruction, EntryOfDataInTheDeclarantsRecords), DeclarantAZRequiresEIRAuthorization);
			yield return (() => style == VZA && !HasCusAuthorizationUsage(instruction, SimplifiedDeclaration), DeclarantVZARequiresSDEAuthorization);
			yield return (() => style == AAV && !HasCusAuthorizationUsage(instruction, EntryOfDataInTheDeclarantsRecords), DeclarantAAVRequiresEIRAuthorization);
			yield return (() => style == AAV && !HasCusAuthorizationUsage(instruction, InwardProcessing), DeclarantAAVRequiresIPOAuthorization);
			yield return (() => style == VAV && !HasCusAuthorizationUsage(instruction, SimplifiedDeclaration), DeclarantVAVRequiresSDEAuthorization);
			yield return (() => style == VAV && !HasCusAuthorizationUsage(instruction, InwardProcessing), DeclarantVAVRequiresIPOAuthorization);
			yield return (() => style == AZL && !HasCusAuthorizationUsage(instruction, EntryOfDataInTheDeclarantsRecords), DeclarantAZLRequiresEIRAuthorization);
			yield return (() => style == AZL && declarantType.In(new ZString[] { _1Self, _2Direct }) && !HasCusAuthorizationUsage(instruction, CustomsWarehousingCWP) && !HasCusAuthorizationUsage(instruction, CustomsWarehousingCW1), DeclarantAZLRequiresCWPOrCW1Authorization);
			yield return (() => style == AZL && declarantType == _3Indirect && !HasCusAuthorizationUsage(instruction, CustomsWarehousingCW1), DeclarantAZLRequiresCW1Authorization);
			yield return (() => style == AZL && declarantType == _3Indirect && HasCusAuthorizationUsage(instruction, CustomsWarehousingCWP), DeclarantINDMustNotHaveCWPAuthorization);
			yield return (() => style == VZL && !HasCusAuthorizationUsage(instruction, SimplifiedDeclaration), DeclarantVZLRequiresSDEAuthorization);
			yield return (() => style == VZL && declarantType.In(new ZString[] { _1Self, _2Direct }) && !HasCusAuthorizationUsage(instruction, CustomsWarehousingCWP) && !HasCusAuthorizationUsage(instruction, CustomsWarehousingCW1), DeclarantVZLRequiresCWPOrCW1Authorization);
			yield return (() => style == VZL && declarantType == _3Indirect && !HasCusAuthorizationUsage(instruction, CustomsWarehousingCW1), DeclarantVZLRequiresCW1Authorization);
			yield return (() => style == VZL && declarantType == _3Indirect && HasCusAuthorizationUsage(instruction, CustomsWarehousingCWP), DeclarantINDMustNotHaveCWPAuthorization);
			yield return (() => style == EZL && !HasCusAuthorizationUsage(instruction, CustomsWarehousingCWP) && !HasCusAuthorizationUsage(instruction, CustomsWarehousingCW1), DeclarantEZLRequiresCWPOrCW1Authorization);
		}

		static IEnumerable<(Func<ZBool>, ZString)> GetCusAuthorisationUsageRequirementValidatorsExport(CusEntryInstruction instruction)
		{
			var style = instruction.CEI_Style;
			var subStyle = instruction.CEI_SubStyle;

			yield return (() => subStyle == ExportDeclarationTypeTimeList.Codes._20 && !HasCusAuthorizationUsage(instruction, EntryOfDataInTheDeclarantsRecords), DeclarantSubStyle20RequiresEIRAuthorization(subStyle, style));
			yield return (() => instruction.IsSDEExport() && !HasCusAuthorizationUsage(instruction, SimplifiedDeclaration), DeclarantRequiresSDEAuthorization(subStyle, style));
			yield return (() => instruction.IsSDEOutwardProcessing() && !HasCusAuthorizationUsage(instruction, SimplifiedDeclaration), DeclarantRequiresSDEOutwardProcessingAuthorization(subStyle, style));
			yield return (() => instruction.IsCCLExport() && !HasCusAuthorizationUsage(instruction, CentralizedClearance), DeclarantRequiresCCLAuthorization(subStyle, style));
			yield return (() => instruction.IsOPOOutwardProcessing() && !HasCusAuthorizationUsage(instruction, OutwardProcessing), DeclarantRequiresOPOAuthorization(subStyle, style));
		}

		public static ZString DeclarantSubStyle20RequiresEIRAuthorization(string ceiSubStyle, string ceiStyle)
		{
			return Res.GetString("9F89139E-3EE7-4347-87DA-8E011BA30A66", "For Declaration Type '{0}|{1}' an Authorization record of Type 'EIR' is mandatory.", ceiSubStyle, ceiStyle);
		}

		public static ZString DeclarantRequiresSDEAuthorization(string ceiSubStyle, string ceiStyle)
		{
			return Res.GetString("92AD9C8A-864D-4EF3-9114-5AD982DD1FD5", "For Declaration Type '{0}|{1}' an Authorization record of Type 'SDE' is mandatory.", ceiSubStyle, ceiStyle);
		}

		public static ZString DeclarantRequiresSDEOutwardProcessingAuthorization(string ceiSubStyle, string ceiStyle)
		{
			return Res.GetString("3E0F5E5D-D64C-4737-8138-9570A2D4E6CD", "For Declaration Type '{0}|{1}' an Authorization record of Type 'SDE'(Outward Processing) is mandatory.", ceiSubStyle, ceiStyle);
		}

		public static ZString DeclarantRequiresCCLAuthorization(string ceiSubStyle, string ceiStyle)
		{
			return Res.GetString("83635943-A973-40A0-A9EE-5A35B4AA208A", "For Declaration Type '{0}|{1}' an Authorization record of Type 'CCL' is mandatory.", ceiSubStyle, ceiStyle);
		}

		public static ZString DeclarantRequiresOPOAuthorization(string ceiSubStyle, string ceiStyle)
		{
			return Res.GetString("18A8EEE1-4F07-4B66-B4F9-C33C98B04C25", "For Declaration Type '{0}|{1}' an Authorization record of Type 'OPO' is mandatory.", ceiSubStyle, ceiStyle);
		}

		public static CodeDescriptionPairList GetDeclarationAuthorizationNumberList(this JobDeclaration declaration, CusEntryInstruction entryInstructionToObtainAuthorizations, ZDate transactionDate, ZString procedure)
		{
			var authorizationOrgAddresses = declaration.GetAuthorizationOrgAddress().ToList();
			if (entryInstructionToObtainAuthorizations != null)
			{
				authorizationOrgAddresses.Add(entryInstructionToObtainAuthorizations.Warehouse);
			}
			var authorizationOrgAddressesPKs = authorizationOrgAddresses.Select(x => x?.PK ?? ZGuid.Empty).ToArray();
			var factory = declaration.Factory;
			var declarationType = declaration.IsImport || declaration.IsExport ? declaration.JE_MessageType : ZString.Empty;
			return factory.GetCachedValue(string.Join("|", "DeclarationAuthorizationNumberList|DE", string.Join("_", declarationType, procedure, transactionDate, string.Join("_", authorizationOrgAddressesPKs))), () =>
			{
				var list = new CodeDescriptionPairList();
				if (procedure == PreviousProcedureList.Codes._ATAV)
				{
					var authorizationHoldersPKs = authorizationOrgAddresses.Select(x => x?.Header?.PK ?? ZGuid.Empty).ToArray();
					list.AddRange(GetCachedAuthorizationNumbers(factory, new ZString[] { InwardProcessing }, authorizationHoldersPKs, transactionDate));
				}
				else if (procedure == PreviousProcedureList.Codes._ATZL)
				{
					list.AddRange(GetCachedAuthorizationNumbersForAddresses(factory, new ZString[] { CustomsWarehousingCW1, CustomsWarehousingCW2, CustomsWarehousingCWP }, authorizationOrgAddressesPKs, transactionDate));
				}

				list.Sort();
				return list;
			});
		}

		public static CodeDescriptionPairList GetAuthorizationNumbersForPrimaryAndSecondaryHolders(BusinessObjectFactory factory, ZString authorizationType, ZDate transactionDate, ZGuid primaryAuthorizationHolder, ZGuid secondaryAuthorizationHolder)
		{
			var list = GetCachedAuthorizationNumbers(factory, new[] { authorizationType }, new[] { primaryAuthorizationHolder }, transactionDate);
			if (list.Count == 0 && !secondaryAuthorizationHolder.IsEmpty)
			{
				list = GetCachedAuthorizationNumbers(factory, new[] { authorizationType }, new[] { secondaryAuthorizationHolder }, transactionDate);
			}
			return list;
		}

		public static void UpdateAuthorizationNumberOnEntryInstructionsIfNecessary(this JobDeclaration declaration) => declaration.CustomsEntryInstructions.ForEach(x => x.UpdateAuthorizationNumberOnPreviousDocumentMasterIfNecessary());

		public static void UpdateAuthorizationNumberOnPreviousDocumentMasterIfNecessary(this CusEntryInstruction entryInstruction) => entryInstruction.PreviousDocumentMaster.UpdateAuthorizationNumberIfOnlyOneExists();

		public static ZString GetAuthorizationNumberIfOnlyOneExists(this PreviousDocumentMaster previousDocumentMaster)
		{
			var authorizationNumber = previousDocumentMaster.Lookups.AuthorizationNumberList;
			return authorizationNumber.Count == 1 ? (ZString)authorizationNumber[0].Code : ZString.Empty;
		}
	}
}
