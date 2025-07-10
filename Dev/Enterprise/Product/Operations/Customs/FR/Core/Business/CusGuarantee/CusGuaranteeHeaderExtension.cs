using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business
{
	public static class CusGuaranteeHeaderExtension
	{
		public static CusGuaranteeHeader[] GetCustomsGuarantees(this BusinessObjectFactory factory, JobDeclaration declaration, ZBool ignoreEndDate, string guaranteeType)
		{
			var query = GetCoreCustomsGuaranteeQuery(ignoreEndDate, guaranteeType);
			query.AddToFilter(GetHolderQuery(declaration));
			query.AddSubQuery(GetPermitRuleQueryForEnt(declaration), JoinCondition.And);
			var result = factory.Load<CusGuaranteeHeader>(query);

			var guaranteesWithAddRule = result.Where(g => g.CusGuaranteeRules.Any(r => r.CPR_RuleCode == PermitRuleCodeList.Codes.ADD)).ToList();
			if (guaranteesWithAddRule.Any())
			{
				query.AddSubQuery(GetPermitRuleQueryForAdd(declaration), JoinCondition.And);
				var reloadedResultsWithAddRule = factory.Load<CusGuaranteeHeader>(query);
				result = result.Except(guaranteesWithAddRule).Concat(reloadedResultsWithAddRule).ToArray();
			}

			return result.Where(g =>
			{
				var result = false;
				var additionalGuaranteeReferences = g.AdditionalGuaranteeReferences;
				if (declaration.IsUCC6)
				{
					result = additionalGuaranteeReferences.Count == 0;
				}
				else
				{
					var deltaMode = declaration.JE_DeltaMode;
					result = (additionalGuaranteeReferences.Count == 0 || additionalGuaranteeReferences.Cast<CusGuaranteeReferenceNumber>().Any(x => x.CY_Code == deltaMode)) && IsGuaranteeNumberSuitableForDeclaration(g, deltaMode);
				}
				return result;
			}).OrderByDescending(x => x.CPH_StartDate).ThenByDescending(x => x.CPH_SystemCreateTimeUtc).ToArray();
		}

		static ZDBOnlyQuery GetCoreCustomsGuaranteeQuery(ZBool ignoreEndDate, string guaranteeType)
		{
			var query = new ZDBOnlyQuery(typeof(CusPermitHeader));
			query.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Type, guaranteeType);
			query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDate.Today);
			query.AddToFilter(CusPermitHeaderSchema.CPH_IsActive, true);
			if (!ignoreEndDate)
			{
				query.AddToFilter(GetGuaranteeEndDateQuery());
			}
			return query;
		}

		static ZDBOnlySubQuery GetPermitRuleQueryForAdd(JobDeclaration declaration)
		{
			var addressCodeList = new List<ZString>();
			if (declaration.IsImport)
			{
				addressCodeList.Add(declaration.ImporterDocumentaryAddressCode);
			}
			else if (declaration.IsExport)
			{
				addressCodeList.Add(declaration.SupplierDocumentaryAddressCode);
			}

			addressCodeList.Add(declaration.DeclarantOrgAddress?.AddressCode ?? ZString.Empty);

			var branchAddressCode = declaration.Branch?.OrgProxy?.MainAddress?.AddressCode ?? ZString.Empty;
			if (branchAddressCode != ZString.Empty)
			{
				addressCodeList.Add(branchAddressCode);
			}

			var companyAddressCode = declaration.Company?.OrgProxy?.MainAddress?.AddressCode ?? ZString.Empty;
			if (companyAddressCode != ZString.Empty)
			{
				addressCodeList.Add(companyAddressCode);
			}

			var query = new ZDBOnlySubQuery(typeof(SharedCusPermitRule), CusPermitRuleSchema.CPR_CPH_PermitHeader);
			query.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, PermitRuleCodeList.Codes.ADD);
			query.AddToFilter(CusPermitRuleSchema.CPR_ValueFrom, addressCodeList);
			return query;
		}

		static ZDBOnlySubQuery GetPermitRuleQueryForEnt(JobDeclaration declaration)
		{
			var queryEnt = new ZDBOnlySubQuery(typeof(SharedCusPermitRule), CusPermitRuleSchema.CPR_CPH_PermitHeader);
			queryEnt.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, PermitRuleCodeList.Codes.ENT);
			queryEnt.AddToFilter(CusPermitRuleSchema.CPR_ValueFrom, GuaranteeEntryTypeList.Codes.BTH);
			queryEnt.AddToFilter(JoinCondition.Or, CusPermitRuleSchema.CPR_ValueFrom, SQLComparisonOperator.StartsWith, declaration.JE_MessageType);
			return queryEnt;
		}

		static ZQuery GetHolderQuery(JobDeclaration declaration)
		{
			var query = new ZQuery();

			var permitHolders = declaration.ActualClientAndDeclarant.Select(x => x.PK);

			var branchOrgProxy = declaration.Branch?.GB_OH_OrgProxy ?? ZGuid.Empty;
			if (branchOrgProxy != ZGuid.Empty)
			{
				permitHolders = permitHolders.Concat(new[] { declaration.Branch.GB_OH_OrgProxy });
			}

			var companyOrgProxy = declaration.Company?.GC_OH_OrgProxy ?? ZGuid.Empty;
			if (companyOrgProxy != ZGuid.Empty)
			{
				permitHolders = permitHolders.Concat(new[] { declaration.Company.GC_OH_OrgProxy });
			}

			query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, permitHolders);

			return query;
		}

		static ZQuery GetGuaranteeEndDateQuery()
		{
			var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);
			endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, ZDate.Empty);
			return endDateQuery;
		}

		static ZBool IsGuaranteeNumberSuitableForDeclaration(CusGuaranteeHeader guarantee, ZString deltaMode) => Regex.IsMatch(guarantee.GetApplicationSpecificReference(deltaMode), "^[A-Z]{4}$");
	}
}
