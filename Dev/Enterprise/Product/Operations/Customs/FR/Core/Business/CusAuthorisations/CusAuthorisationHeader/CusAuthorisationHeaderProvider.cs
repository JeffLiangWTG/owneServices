using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.FR.Business
{
	public class CusAuthorisationHeaderProvider : EU.Business.CusAuthorisationHeaderProvider
	{
		protected CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
		{
		}

		public Customs.Business.CusAuthorisationRuleTypeList CusAuthorisationRuleTypeListForCW1CW2CWP
		{
			get
			{
				var cusAuthorisationRuleTypeList = new Customs.Business.CusAuthorisationRuleTypeList();
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.AUT, CusAuthorisationRuleTypeList.Descriptions.AUT);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.CLE, CusAuthorisationRuleTypeList.Descriptions.CLE);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.CNT, CusAuthorisationRuleTypeList.Descriptions.CNT);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.PCD, CusAuthorisationRuleTypeList.Descriptions.PCD);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.PCP, CusAuthorisationRuleTypeList.Descriptions.PCP);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.PCV, CusAuthorisationRuleTypeList.Descriptions.PCV);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.STO, CusAuthorisationRuleTypeList.Descriptions.STO);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.USE, CusAuthorisationRuleTypeList.Descriptions.USE);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.WAR, CusAuthorisationRuleTypeList.Descriptions.WAR);
				return cusAuthorisationRuleTypeList;
			}
		}

		public Customs.Business.CusAuthorisationRuleTypeList CusAuthorisationRuleTypeListForTST
		{
			get
			{
				var cusAuthorisationRuleTypeList = new Customs.Business.CusAuthorisationRuleTypeList();
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.STO, CusAuthorisationRuleTypeList.Descriptions.STO);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.USE, CusAuthorisationRuleTypeList.Descriptions.USE);
				return cusAuthorisationRuleTypeList;
			}
		}

		public Customs.Business.CusAuthorisationRuleTypeList CusAuthorisationRuleTypeListForAUL
		{
			get
			{
				var cusAuthorisationRuleTypeList = new Customs.Business.CusAuthorisationRuleTypeList();
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.OFC, CusAuthorisationRuleTypeList.Descriptions.OFC);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.SUB, CusAuthorisationRuleTypeList.Descriptions.SUB);
				return cusAuthorisationRuleTypeList;
			}
		}

		public CodeDescriptionPairList CusAuthorisationUSEValueFromListForCW1CW2
		{
			get
			{
				var cusAuthorisationRuleTypeList = new CodeDescriptionPairList();
				cusAuthorisationRuleTypeList.AddPair(AuthorizationRuleUseValueFromList.Codes.ENE, AuthorizationRuleUseValueFromList.Descriptions.ENE);
				return cusAuthorisationRuleTypeList;
			}
		}

		public CodeDescriptionPairList CusAuthorisationUSEValueFromListForCWP
		{
			get
			{
				var cusAuthorisationRuleTypeList = new CodeDescriptionPairList();
				cusAuthorisationRuleTypeList.AddPair(AuthorizationRuleUseValueFromList.Codes.PAA, AuthorizationRuleUseValueFromList.Descriptions.PAA);
				cusAuthorisationRuleTypeList.AddPair(AuthorizationRuleUseValueFromList.Codes.PSA, AuthorizationRuleUseValueFromList.Descriptions.PSA);
				cusAuthorisationRuleTypeList.AddPair(AuthorizationRuleUseValueFromList.Codes.U, AuthorizationRuleUseValueFromList.Descriptions.U);
				return cusAuthorisationRuleTypeList;
			}
		}

		public CodeDescriptionPairList CusAuthorisationUSEValueFromListForTST
		{
			get
			{
				var cusAuthorisationRuleTypeList = new CodeDescriptionPairList();
				cusAuthorisationRuleTypeList.AddPair(AuthorizationRuleUseValueFromList.Codes.IST, AuthorizationRuleUseValueFromList.Descriptions.IST);
				cusAuthorisationRuleTypeList.AddPair(AuthorizationRuleUseValueFromList.Codes.LAD, AuthorizationRuleUseValueFromList.Descriptions.LAD);
				return cusAuthorisationRuleTypeList;
			}
		}

		public CodeDescriptionPairList RuleCodeCONValueFromListForIPO
		{
			get
			{
				var ruleCodeCONValueFromList = new CodeDescriptionPairList();
				ruleCodeCONValueFromList.AddPair(RuleCodeCONValueFromList.Codes.RMA, RuleCodeCONValueFromList.Descriptions.RMA);
				ruleCodeCONValueFromList.AddPair(RuleCodeCONValueFromList.Codes.PRT, RuleCodeCONValueFromList.Descriptions.PRT);
				ruleCodeCONValueFromList.AddPair(RuleCodeCONValueFromList.Codes.RLA, RuleCodeCONValueFromList.Descriptions.RLA);
				ruleCodeCONValueFromList.AddPair(RuleCodeCONValueFromList.Codes.SUC, RuleCodeCONValueFromList.Descriptions.SUC);
				ruleCodeCONValueFromList.AddPair(RuleCodeCONValueFromList.Codes.CNA, RuleCodeCONValueFromList.Descriptions.CNA);
				ruleCodeCONValueFromList.AddPair(RuleCodeCONValueFromList.Codes._324, RuleCodeCONValueFromList.Descriptions._324);
				return ruleCodeCONValueFromList;
			}
		}

		public CodeDescriptionPairList RuleCodeCONValueFromListForEndUse
		{
			get
			{
				var ruleCodeCONValueFromList = new CodeDescriptionPairList();
				ruleCodeCONValueFromList.AddPair(RuleCodeCONValueFromList.Codes.N990, RuleCodeCONValueFromList.Descriptions.N990);
				ruleCodeCONValueFromList.AddPair(RuleCodeCONValueFromList.Codes.C990, RuleCodeCONValueFromList.Descriptions.C990);
				return ruleCodeCONValueFromList;
			}
		}

		public CodeDescriptionPairList CusAuthorisationRuleTypeListForIPOTEA
		{
			get
			{
				var cusAuthorisationRuleTypeList = new Customs.Business.CusAuthorisationRuleTypeList();
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.AUT, CusAuthorisationRuleTypeList.Descriptions.AUT);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.CLE, CusAuthorisationRuleTypeList.Descriptions.CLE);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.CON, CusAuthorisationRuleTypeList.Descriptions.CON);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.INF, CusAuthorisationRuleTypeList.Descriptions.INF);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.NAT, CusAuthorisationRuleTypeList.Descriptions.NAT);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.OFC, CusAuthorisationRuleTypeList.Descriptions.OFC);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.PCD, CusAuthorisationRuleTypeList.Descriptions.PCD);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.PCP, CusAuthorisationRuleTypeList.Descriptions.PCP);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.PCV, CusAuthorisationRuleTypeList.Descriptions.PCV);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.STO, CusAuthorisationRuleTypeList.Descriptions.STO);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.TRA, CusAuthorisationRuleTypeList.Descriptions.TRA);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.WAR, CusAuthorisationRuleTypeList.Descriptions.WAR);
				return cusAuthorisationRuleTypeList;
			}
		}

		public CodeDescriptionPairList CusAuthorisationRuleTypeListForOPO
		{
			get
			{
				var cusAuthorisationRuleTypeList = new Customs.Business.CusAuthorisationRuleTypeList();
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.AUT, CusAuthorisationRuleTypeList.Descriptions.AUT);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.CLE, CusAuthorisationRuleTypeList.Descriptions.CLE);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.CON, CusAuthorisationRuleTypeList.Descriptions.CON);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.INF, CusAuthorisationRuleTypeList.Descriptions.INF);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.NAT, CusAuthorisationRuleTypeList.Descriptions.NAT);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.OFC, CusAuthorisationRuleTypeList.Descriptions.OFC);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.STO, CusAuthorisationRuleTypeList.Descriptions.STO);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.TRA, CusAuthorisationRuleTypeList.Descriptions.TRA);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.WAR, CusAuthorisationRuleTypeList.Descriptions.WAR);
				return cusAuthorisationRuleTypeList;
			}
		}

		public CodeDescriptionPairList CusAuthorisationRuleTypeListForOTOAndTEE
		{
			get
			{
				var cusAuthorisationRuleTypeList = new Customs.Business.CusAuthorisationRuleTypeList();
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.CLE, CusAuthorisationRuleTypeList.Descriptions.CLE);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.CON, CusAuthorisationRuleTypeList.Descriptions.CON);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.INF, CusAuthorisationRuleTypeList.Descriptions.INF);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.NAT, CusAuthorisationRuleTypeList.Descriptions.NAT);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.OFC, CusAuthorisationRuleTypeList.Descriptions.OFC);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.STO, CusAuthorisationRuleTypeList.Descriptions.STO);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.TRA, CusAuthorisationRuleTypeList.Descriptions.TRA);
				return cusAuthorisationRuleTypeList;
			}
		}

		public Customs.Business.CusAuthorisationRuleTypeList CusAuthorisationRuleTypeListForEUS
		{
			get
			{
				var cusAuthorisationRuleTypeList = new Customs.Business.CusAuthorisationRuleTypeList();
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.AUT, CusAuthorisationRuleTypeList.Descriptions.AUT);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.CON, CusAuthorisationRuleTypeList.Descriptions.CON);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.OFC, CusAuthorisationRuleTypeList.Descriptions.OFC);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.PCD, CusAuthorisationRuleTypeList.Descriptions.PCD);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.PCP, CusAuthorisationRuleTypeList.Descriptions.PCP);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.PCV, CusAuthorisationRuleTypeList.Descriptions.PCV);
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.STO, CusAuthorisationRuleTypeList.Descriptions.STO);
				return cusAuthorisationRuleTypeList;
			}
		}

		public CodeDescriptionPairList CusAuthorisationRuleTypeListForTransit
		{
			get
			{
				var cusAuthorisationRuleTypeList = new Customs.Business.CusAuthorisationRuleTypeList();

				if (IsUsingPhase4)
				{
					cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.OFC, CusAuthorisationRuleTypeList.Descriptions.OFC);
				}

				return cusAuthorisationRuleTypeList;
			}
		}

		bool IsUsingPhase4 => (isUsingPhase4 ?? (isUsingPhase4 = IsPhase4FunctionalityValid(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))).Value;
		bool? isUsingPhase4;

		bool IsPhase4FunctionalityValid(string countryCode)
		{
			var today = ZDateTime.Today;
			return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.NCTSPhase4, countryCode, today)
				&& !ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.NCTSPhase5Override, countryCode, today);
		}

		public CodeDescriptionPairList CusAuthorisationRuleTypeNotSuitableForAdHoc
		{
			get
			{
				var cusAuthorisationRuleTypeList = new CodeDescriptionPairList();
				cusAuthorisationRuleTypeList.AddPair(CusAuthorisationRuleTypeList.Codes.AUT, CusAuthorisationRuleTypeList.Descriptions.AUT);
				return cusAuthorisationRuleTypeList;
			}
		}

		protected override CodeDescriptionPairList GetRuleCodeListForModuleCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.FR.Business.CusAuthorisationHeaderProvider.GetRuleCodeListForModule", () =>
			{
				var result = new CodeDescriptionPairList(base.GetRuleCodeListForModuleCore(factory));
				result.AddRangeOverwriteIfExists(new CusAuthorisationRuleTypeList());
				result.Sort();
				return result;
			});
		}

		protected override bool EnableAdHocCore => true;

		protected override Customs.Business.CusAuthorisationHeaderLookups GetNewLookupsCore(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationHeaderLookups(cusAuthorisationHeader);

		protected override Customs.Business.CusAuthorisationRuleLookups GetNewLookupsCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleLookups(cusAuthorisationRule);

		protected override Customs.Business.CusAuthorisationRuleValidation GetNewValidationCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleValidation(cusAuthorisationRule);

		protected override Customs.Business.CusAuthorisationHeaderValidation GetNewValidationCore(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationHeaderValidation(cusAuthorisationHeader);

		protected override List<ZString> GetAuthorizationTypesNeedAddress()
		{
			var result = base.GetAuthorizationTypesNeedAddress();
			result.Add(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation);
			return result;
		}

		protected override Dictionary<ZString, FieldType> GetRuleValueFieldTypesCore()
		{
			var fieldTypes = base.GetRuleValueFieldTypesCore();
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.CLE, FieldType.Boolean);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.CNT, FieldType.Integer);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.CON, FieldType.TextMultiLine);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.INF, FieldType.TextMultiLine);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.NAT, FieldType.TextMultiLine);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.OFC, FieldType.TextCodeFindBox);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.PCD, FieldType.Decimal);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.PCP, FieldType.Decimal);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.PCV, FieldType.Decimal);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.USE, FieldType.TextDropEdit);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.STO, FieldType.Integer);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.TRA, FieldType.TextDropEdit);
			fieldTypes.Add(CusAuthorisationRuleTypeList.Codes.WAR, FieldType.Integer);
			return fieldTypes;
		}

		protected override string GetRuleValueFromFieldTypeCore(CusAuthorisationRule cusAuthorisationRule)
		{
			var result = base.GetRuleValueFromFieldTypeCore(cusAuthorisationRule);

			if (cusAuthorisationRule != null)
			{
				var header = cusAuthorisationRule.AuthorisationHeader;

				if (header != null)
				{
					if (cusAuthorisationRule.CPR_RuleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location && CusAuthorisationHeaderExtensions.TypeThatHaveAfreeTextLOCRuleValueFrom.Contains(header.CPH_Type))
					{
						result = nameof(FieldType.Text);
					}
					else if (cusAuthorisationRule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.CON && (header.CPH_Type == Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing || header.CPH_Type == Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse))
					{
						result = nameof(FieldType.TextDropEdit);
					}
				}
			}
			return result;
		}

		protected override Dictionary<ZString, List<CusAuthorisationRuleRequirement>> GetValidAuthorisationRuleRequirementsCore(CusAuthorisationHeader cusAuthorisationHeader)
		{
			var rules = base.GetValidAuthorisationRuleRequirementsCore(cusAuthorisationHeader);

			foreach (var authorisationType in new CusAuthorizationHeaderTypeList().GetAllCodes())
			{
				AddOrUpdateValidAuthorisationRuleRepititions(rules, authorisationType, new List<CusAuthorisationRuleRequirement>
				{
					CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, authorisationType, CusAuthorisationRuleTypeList.Codes.USE)
				});
				AddOrUpdateValidAuthorisationRuleRepititions(rules, authorisationType, new List<CusAuthorisationRuleRequirement>
				{
					CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, authorisationType, CusAuthorisationRuleTypeList.Codes.OFC, NotificationType.MessageError)
				});
				AddOrUpdateValidAuthorisationRuleRepititions(rules, authorisationType, new List<CusAuthorisationRuleRequirement>
				{
					CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, authorisationType, CusAuthorisationRuleTypeList.Codes.CON, NotificationType.MessageError)
				});
				AddOrUpdateValidAuthorisationRuleRepititions(rules, authorisationType, new List<CusAuthorisationRuleRequirement>
				{
					CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, authorisationType, CusAuthorisationRuleTypeList.Codes.NAT, NotificationType.MessageError)
				});
				AddOrUpdateValidAuthorisationRuleRepititions(rules, authorisationType, new List<CusAuthorisationRuleRequirement>
				{
					CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, authorisationType, CusAuthorisationRuleTypeList.Codes.STO, NotificationType.MessageError)
				});
				AddOrUpdateValidAuthorisationRuleRepititions(rules, authorisationType, new List<CusAuthorisationRuleRequirement>
				{
					CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, authorisationType, Customs.Business.CusAuthorisationRuleTypeList.Codes.Location, NotificationType.MessageError)
				});
				AddOrUpdateValidAuthorisationRuleRepititions(rules, authorisationType, new List<CusAuthorisationRuleRequirement>
				{
					CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, authorisationType, CusAuthorisationRuleTypeList.Codes.PCP, NotificationType.MessageError)
				});
				AddOrUpdateValidAuthorisationRuleRepititions(rules, authorisationType, new List<CusAuthorisationRuleRequirement>
				{
					CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, authorisationType, CusAuthorisationRuleTypeList.Codes.PCV, NotificationType.MessageError)
				});
				AddOrUpdateValidAuthorisationRuleRepititions(rules, authorisationType, new List<CusAuthorisationRuleRequirement>
				{
					CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, authorisationType, CusAuthorisationRuleTypeList.Codes.PCD, NotificationType.MessageError)
				});
				AddOrUpdateValidAuthorisationRuleRepititions(rules, authorisationType, new List<CusAuthorisationRuleRequirement>
				{
					CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, authorisationType, CusAuthorisationRuleTypeList.Codes.AUT, CargoWise.ComponentModel.NotificationType.Error)
				});
			}

			var generalRules = new CusAuthorisationRuleTypeList().GetAllCodes().Except(KeyForNotGeneralHeaderType).Select(x => CusAuthorisationRuleRequirementHelper.GetRequirement(cusAuthorisationHeader, KeyForGeneralHeaderType, x)).ToList();
			AddOrUpdateValidAuthorisationRuleRepititions(rules, KeyForGeneralHeaderType, generalRules);

			return rules;
		}

		public IEnumerable<string> KeyForNotGeneralHeaderType => new string[] { CusAuthorisationRuleTypeList.Codes.USE, CusAuthorisationRuleTypeList.Codes.CON, CusAuthorisationRuleTypeList.Codes.NAT, CusAuthorisationRuleTypeList.Codes.OFC, CusAuthorisationRuleTypeList.Codes.STO, CusAuthorisationRuleTypeList.Codes.PCD, CusAuthorisationRuleTypeList.Codes.PCP, CusAuthorisationRuleTypeList.Codes.PCV, CusAuthorisationRuleTypeList.Codes.AUT, CusAuthorisationRuleTypeList.Codes.Location };

		protected override void SetupRuleDescriptionFunctions()
		{
			base.SetupRuleDescriptionFunctions();
			AddRuleDescriptionFunction(CusAuthorisationRuleTypeList.Codes.OFC, rule => GetCusCodeDescription(rule, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice));
			AddRuleDescriptionFunction(CusAuthorisationRuleTypeList.Codes.TRA, rule => new RuleCodeTRAValueFromList().GetDescriptionFromCode(rule.CPR_ValueFrom));
			AddRuleDescriptionFunction(CusAuthorisationRuleTypeList.Codes.CON, rule => new RuleCodeCONValueFromList().GetDescriptionFromCode(rule.CPR_ValueFrom));
		}

		protected override CodeDescriptionPairList GetAuthorisationTypeListCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue($"Enterprise.Customs.FR.Business.CusAuthorisationHeaderProvider.GetAuthorisationTypeListCore_{ZDateTime.Today}", () =>
			{
				var authorizationTypes = new CusAuthorizationHeaderTypeList();
				var dieCodeList = RefCusCodeListTypes.GetCachedList(
							factory,
							Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE,
							EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AUTH,
							ZDateTime.Today
							);
				authorizationTypes.AddRangeOverwriteIfExists(dieCodeList);
				authorizationTypes.Sort();
				return authorizationTypes;
			});
		}

		protected override string GetCustomsNumberProviderKeyCore(ZString type)
		{
			return type == Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryStorage ? CusAuthorisationHeaderCustomsNumberProviderKeyList.Codes.TemporaryStorage : base.GetCustomsNumberProviderKeyCore(type);
		}

		protected override ZBool IsAuthorisationNumberValidCore(CusAuthorisationHeader authorisationHeader) => !authorisationHeader.IsSpecificRegime() || authorisationHeader.IsSpecificRegime() && IsMax30DigitsAlphabeticNumericCharacter(authorisationHeader.CPH_Number);

		protected override ZString GetAuthorisationNumberInvalidFormatMessageCore(CusAuthorisationHeader authorisationHeader) => Res.GetString("11776AA8-7F77-4A6E-98CF-446903C9B159", "Please note for specific regime, French Customs has set the maximum length to 30 characters. No special char allowed.");

		public static ZBool IsMax30DigitsAlphabeticNumericCharacter(string valueToCheck) => valueToCheck.Length <= 30 && valueToCheck.All(char.IsLetterOrDigit);

		public override ZString DefaultTemporaryAuthorizationNumber => "S/DECLARATION";

		protected override ZString AuthorizedLocationCodeCore => CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation;
	}
}
