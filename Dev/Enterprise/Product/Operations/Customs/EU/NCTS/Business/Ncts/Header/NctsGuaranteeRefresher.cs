using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsGuaranteeRefresher
	{
		public NctsGuaranteeRefresher(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			factory = nctsHeader.Factory;
			totalGuarantees = nctsHeader.GetEffectiveGuarantees().Select(x => x.PW_BondNumber).ToList();
		}
		readonly NctsHeader nctsHeader;
		readonly BusinessObjectFactory factory;
		internal readonly List<ZString> totalGuarantees;

		public void PopulateGuaranteeWithFallbacks()
		{
			var configuration = nctsHeader.Configuration;
			ClearExistingGuarantees();

			_ = nctsHeader.Principal?.Address is OrgAddress principalAddress && PopulateGuaranteeFromAddress(principalAddress)
			|| (totalGuarantees.Count == 0 && configuration.UseDeclarantFallBack && nctsHeader.Declarant?.Address is OrgAddress declarantAddress && PopulateGuaranteeFromAddress(declarantAddress))
			|| (totalGuarantees.Count == 0 && configuration.UseBranchOrgProxyFallBack && !nctsHeader.Branch.GB_OH_OrgProxy.IsEmpty && PopulateGuaranteeFromAddress(nctsHeader.Branch.OrgProxy.MainAddress))
			|| (totalGuarantees.Count == 0 && configuration.UseCompanyOrgProxyFallBack && !nctsHeader.Company.GC_OH_OrgProxy.IsEmpty && PopulateGuaranteeFromAddress(nctsHeader.Company.OrgProxy.MainAddress));
		}

		void ClearExistingGuarantees()
		{
			totalGuarantees.Clear();
			if (nctsHeader.Configuration.ClearExistingGuaranteesConfiguration)
			{
				nctsHeader.GetEffectiveGuarantees().RemoveAndDeleteAll();
			}
		}

		bool PopulateGuaranteeFromAddress(OrgAddress address)
		{
			var result = false;
			var guarantee = GetTargetGuarantee(address);
			if (guarantee != null)
			{
				CreateNctsGuaranteeFromPrincipalGuarantee(guarantee);
				result = true;
			}
			return result;
		}

		CusGuaranteeHeader GetTargetGuarantee(OrgAddress address)
		{
			var orgHeader = address?.OA_OH ?? ZGuid.Empty;
			if (!orgHeader.IsEmpty)
			{
				var possibleCusGuarantees = CusGuaranteeHeader.Loader.LoadCusGuaranteeHeaderListFromPermitHolder(factory, orgHeader, nctsHeader.CountryCode);
				possibleCusGuarantees = FilterGuaranteesIfNecessary(possibleCusGuarantees);

				var numberofGuarantees = possibleCusGuarantees?.Length ?? 0;
				if (numberofGuarantees >= 1)
				{
					if (numberofGuarantees > 1)
					{
						if (ManageMultipleGuarantees)
						{
							var guaranteeToCreateList = possibleCusGuarantees.Where(x => x.CusGuaranteeRules.Any(y => y.CPR_RuleCode == EU.Business.PermitRuleCodeList.Codes.ADD && y.CPR_ValueFrom.EqualsIgnoringCase(address.OA_Code)));
							totalGuarantees.AddRange(guaranteeToCreateList.Select(x => x.CPH_Number).ToList());
							if (guaranteeToCreateList.Count() == 1)
							{
								return guaranteeToCreateList.First();
							}
						}
					}
					else
					{
						var targetGuarantee = possibleCusGuarantees[0];
						totalGuarantees.Add(targetGuarantee.CPH_Number);
						return targetGuarantee;
					}
				}
			}
			return null;
		}

		protected virtual Func<CusGuaranteeHeader, bool> GuaranteeFilter => null;
		protected virtual ZBool ManageMultipleGuarantees => ZBool.True;

		CusGuaranteeHeader[] FilterGuaranteesIfNecessary(CusGuaranteeHeader[] possibleCusGuarantees) => GuaranteeFilter != null ? possibleCusGuarantees.Where(x => GuaranteeFilter(x)).ToArray() : possibleCusGuarantees;

		protected virtual void CreateNctsGuaranteeFromPrincipalGuarantee(CusGuaranteeHeader guarantee)
		{
			var guarantees = nctsHeader.GetEffectiveGuarantees();
			var nctsGuarantee = guarantees.FirstOrDefault(x => x.PW_BondNumber == guarantee.CPH_Number) ?? guarantees.AddNew();
			nctsGuarantee.PW_BondType = guarantee.CPH_SubType;
			nctsGuarantee.PW_BondNumber = guarantee.CPH_Number;
			nctsGuarantee.PW_Password = guarantee.MainAccessCode.Left(CusBondDetail.Schema.PW_PasswordMaxLength);
		}
	}
}
