using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CusCNEntryInstructionLookups : AutoCusCNEntryInstructionLookups
	{
		public CusCNEntryInstructionLookups(AutoCusCNEntryInstruction parent) : base(parent) { }

		public new CusCNEntryInstruction Parent => (CusCNEntryInstruction)base.Parent;

		#region TransitionSiteList

		public CodeDescriptionPairList TransitionSiteList
		{
			get
			{
				CodeDescriptionPairList result;

				var declaration = Parent.EntryInstruction?.JobDeclaration;
				if (declaration != null && !declaration.JE_OfficeOfEntryExit.IsEmpty)
				{
					var customsOfficeToSearch = declaration.JE_OfficeOfEntryExit.SubstringSafe(0, 2);
					result = Factory.GetCachedValue(
						$"CN_CusEntryInstruction_TransitionSiteList_{customsOfficeToSearch}",
						() => GetTransitionSiteList(customsOfficeToSearch, Parent.EntryInstruction.CEI_DateForDuty.FallbackIfEmpty(ZDateTime.Today))
					);
				}
				else
				{
					result = new CodeDescriptionPairList();
				}
				return result;
			}
		}

		CodeDescriptionPairList GetTransitionSiteList(ZString customsOffice, ZDateTime dateOfValuation)
		{
			var result = new CodeDescriptionPairList();

			var refCodes = CNRefCusCodeListTypes.GetTransitionSiteList(Factory, customsOffice, dateOfValuation);
			refCodes.Load();

			var groupedRefCodes = refCodes.Cast<ZZRefCusCodeListCombined>()
				.GroupBy(refCode => refCode.ZZD_Code + refCode.GetAttribute(RefCusCodeListAttributeTypes.Codes.CodeSuffix));

			foreach (var group in groupedRefCodes)
			{
				result.AddPair(group.Key, string.Join("/", group.Select(refCode => refCode.ZZD_Description).Distinct()));
			}
			result.SortByDescription();

			return result;
		}

		#endregion
	}
}
