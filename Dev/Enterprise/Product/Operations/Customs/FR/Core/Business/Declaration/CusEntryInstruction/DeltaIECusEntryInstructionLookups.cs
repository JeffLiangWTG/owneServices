using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIECusEntryInstructionLookups : CusEntryInstructionLookups
	{
		public DeltaIECusEntryInstructionLookups(CusEntryInstruction parent) : base(parent)
		{
		}

		readonly string[] substyleCodeListForI1 = { EntrySubstyleCodePairList.Codes.C, EntrySubstyleCodePairList.Codes.F };

		protected override CodeDescriptionPairList GetEntrySubstyleList(ICanBeImportOrExport parent)
		{
			var result = base.GetEntrySubstyleList(parent);
			if (parent?.IsImport ?? false)
			{
				var substyleListForImport = RefCusCodeListTypes.GetCachedList(Factory, parent.DataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, ZDateTime.Now, includeParentDataGrouping: false);

				if (Parent.CEI_Style == DeltaIEImportDeclarationTypeList.Codes.I1)
				{
					var substyleListForI1 = new CodeDescriptionPairList();
					substyleCodeListForI1.ForEach(code => substyleListForI1.AddPair(code, substyleListForImport.GetDescriptionFromCode(code)));
					result = substyleListForI1;
				}
				else
				{
					result = substyleListForImport;
				}
			}
			return result;
		}

		protected override ZString GetDataGroupingCodeForCPCs() => Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE;
	}
}
