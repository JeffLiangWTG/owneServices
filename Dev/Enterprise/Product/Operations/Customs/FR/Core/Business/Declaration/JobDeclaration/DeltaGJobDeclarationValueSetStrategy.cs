using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	internal class DeltaGJobDeclarationValueSetStrategy : JobDeclarationValueSetStrategy
	{
		public DeltaGJobDeclarationValueSetStrategy(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void DefaultZG_AgreedCodePlace()
		{
		}

		protected override void DefaultJE_DeltaMode()
		{
			var customsProfileAccounts = Declaration.DeltaAccounts.Where(x => x.CZ_Account == Declaration.JE_CustomsProfile);
			if (customsProfileAccounts.Count() == 1)
			{
				Declaration.JE_DeltaMode = customsProfileAccounts.First().CZ_Type;
			}
			else
			{
				Declaration.JE_DeltaMode = ZString.Empty;
			}
		}

		protected override void DefaultJE_DeclarationLanguage()
		{
			Declaration.JE_DeclarationLanguage = ZString.Empty;
		}

		protected override void DefaultJE_CustomsProfile()
		{
			var defaultDeltaGAccountOrg = Declaration.DefaultDeltaAccountOrgHeader;
			if (defaultDeltaGAccountOrg != null)
			{
				var deltaGAccount = OrgCusAccount.Loader.LoadTop1ByCodeAndCountry(Declaration.Factory, Declaration.DefaultDeltaAccountOrgHeader.PK, Declaration.DeltaGAccountCode, Core.Constants.CountryCodes.France);
				Declaration.JE_CustomsProfile = deltaGAccount?.CZ_Account ?? ZString.Empty;
			}
			if (Declaration.JE_CustomsProfile == ZString.Empty && Declaration.Declarant != null && defaultDeltaGAccountOrg != Declaration.Declarant)
			{
				var deltaGAccount = OrgCusAccount.Loader.LoadTop1ByCodeAndCountry(Declaration.Factory, Declaration.Declarant.Header.PK, Declaration.DeltaGAccountCode, Core.Constants.CountryCodes.France);
				Declaration.JE_CustomsProfile = deltaGAccount?.CZ_Account ?? ZString.Empty;
			}
		}

		protected override void SetDefaultOrSwapSubStyleIfNecessarily(CusEntryInstruction entryInstruction, ZDateTime dateInterestedIn)
		{
			var declaration = (JobDeclaration)entryInstruction.JobDeclaration;

			if (!dateInterestedIn.IsEmpty && dateInterestedIn < ZDateTime.Now)
			{
				if (entryInstruction.IsPrelodgedSubstyle || !entryInstruction.IsValidSubStyle)
				{
					entryInstruction.CEI_SubStyle = declaration.IsDeltaC ? EntrySubstyleCodePairList.Codes.A : EntrySubstyleCodePairList.Codes.C;
				}
			}
			else
			{
				if (entryInstruction.IsLodgedSubstyle || !entryInstruction.IsValidSubStyle)
				{
					entryInstruction.CEI_SubStyle = declaration.IsDeltaC ? EntrySubstyleCodePairList.Codes.D : EntrySubstyleCodePairList.Codes.F;
				}
			}
		}

		protected override List<ZGuid> GetGuaranteeSourcePriorityList()
		{
			var declaration = Declaration;
			var organisations = new List<ZGuid>();

			if ((declaration.IsImport ? declaration.JE_OH_Importer : declaration.JE_OH_Supplier) is { IsValid: true } clientPk)
			{
				organisations.Add(clientPk);
			}

			if (declaration.Declarant?.Header.PK is { IsValid: true } declarantHeaderPk)
			{
				organisations.Add(declarantHeaderPk);
			}

			if (declaration.Branch?.GB_OH_OrgProxy is { IsValid: true } branchProxyPk)
			{
				organisations.Add(branchProxyPk);
			}

			if (declaration.Company?.GC_OH_OrgProxy is { IsValid: true } companyProxyPk)
			{
				organisations.Add(companyProxyPk);
			}

			return organisations;
		}
	}
}
