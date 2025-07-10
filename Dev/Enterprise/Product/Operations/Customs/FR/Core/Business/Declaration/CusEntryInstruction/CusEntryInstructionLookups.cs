using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryInstructionLookups : EU.Business.Declaration.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction)
		{
		}

		protected override IEnumerable<ZString> GetDeclarationTypeCodes(EU.Business.Declaration.JobDeclaration declaration)
		{
			var dateForDuty = Parent.CEI_DateForDuty;
			var date = dateForDuty.IsValid ? dateForDuty : ZDateTime.Today;
			return new RefCusProcedure.Loader(Factory).LoadDistinctGroupCodes(declaration.JE_MessageType, GetDataGroupingCodeForCPCs(), date);
		}

		protected override string GetDeclarationTypeListCacheKey(EU.Business.Declaration.JobDeclaration declaration) => string.Join("_", base.GetDeclarationTypeListCacheKey(declaration), !Parent.CEI_DateForDuty.IsEmpty ? Parent.CEI_DateForDuty : ZDateTime.Today);

		protected override CodeDescriptionPairList GetDefinedDeclarationTypeList(EU.Business.Declaration.JobDeclaration declaration)
		{
			CodeDescriptionPairList descriptions;
			if (declaration is JobDeclaration frDeclaration && (frDeclaration.IsImport || frDeclaration.IsExport))
			{
				descriptions = frDeclaration.ApplicationExtender.GetDefinedDeclarationTypeList(frDeclaration);
			}
			else
			{
				descriptions = base.GetDefinedDeclarationTypeList(declaration);
			}

			return descriptions;
		}

		public CodeDescriptionPairList CPCList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var jobDeclaration = Parent.JobDeclaration;
				if (jobDeclaration != null)
				{
					var dataGroupingCode = GetDataGroupingCodeForCPCs();
					var dateOfValuation = jobDeclaration.DateOfValuation;
					var style = Parent.CEI_Style;
					var messageType = jobDeclaration.JE_MessageType;
					result = Factory.GetCachedValue(string.Join("|", $"{dataGroupingCode}.CusEntryInstructionLookups.CPCList", string.Join("_", dataGroupingCode, dateOfValuation, style, messageType)), () =>
					{
						var list = new CodeDescriptionPairList();
						new RefCusProcedureCollection(Factory, dataGroupingCode, dateOfValuation, style, messageType).ForEach(x => list.AddPairIfNotExist(x.ZZ6_ProcedureCode, x.ZZ6_Description));
						list.Sort();
						return list;
					});
				}

				return result;
			}
		}

		protected virtual ZString GetDataGroupingCodeForCPCs() => Parent.JobDeclaration.GetDefaultDataGroupingCode();

		protected override CodeDescriptionPairList GetEntrySubstyleList(ICanBeImportOrExport parent) => Factory.GetCachedValue<EntrySubstyleCodePairList>();
	}
}
