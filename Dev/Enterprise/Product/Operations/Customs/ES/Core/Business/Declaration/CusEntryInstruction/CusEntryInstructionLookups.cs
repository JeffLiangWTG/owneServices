using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusEntryInstructionLookups : EU.Business.Declaration.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		protected override CodeDescriptionPairList DeclarationTypeListCore => Factory.GetCachedValue("ES.DeclarationTypeList_" + Parent.IsSubStyleAOrBOrXOrZ + "_" + Parent.CEI_SubStyle.IsEmpty, () => GetEntryStyleList());

		CodeDescriptionPairList GetEntryStyleList()
		{
			CodeDescriptionPairList result = new IMPDeclarationTypeList();
			if (!Parent.IsSubStyleAOrBOrXOrZ && !Parent.CEI_SubStyle.IsEmpty)
			{
				result.RemoveCode(IMPDeclarationTypeList.Codes.H2);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		protected override CodeDescriptionPairList GetEntrySubstyleList(ICanBeImportOrExport parent)
		{
			const string T2lDescriptionForImport = "Alta Indirecta T2L POUS";

			var declaration = JobDeclaration as JobDeclaration;
			var isDeclarationNotNull = declaration is not null;
			var isExport = isDeclarationNotNull && declaration.IsExport;
			var isImport = isDeclarationNotNull && declaration.IsImport;
			var isExportNormal = isExport && declaration.JE_EntryStyle == EntryStyleListExport.Codes.ExportNormal;
			var isH2 = isImport && Parent.IsH2;

			return Factory.GetCachedValue(GetCacheKey(isExportNormal, isH2, isExport, isImport), () =>
			{
				CodeDescriptionPairList result;

				if (isH2)
				{
					result = GetEntrySubStyleListForH2();
				}
				else
				{
					result = isExportNormal
						? new ExsEntrySubStyleList()
						: new EntrySubStyleList();

					if (isExport)
					{
						ModifyT2LForExport(result);
					}
					else if (isImport)
					{
						ModifyT2LForImport(result, T2lDescriptionForImport);
					}
				}

				return result;
			});
		}

		void ModifyT2LForExport(CodeDescriptionPairList list)
		{
			list.RemoveCode(Declaration.EntrySubStyleList.Codes.T2C);
			list.Sort();
		}

		void ModifyT2LForImport(CodeDescriptionPairList list, string t2lDescriptionForImport)
		{
			list.RemoveCode(Declaration.EntrySubStyleList.Codes.T2L);
			list.AddPair(Declaration.EntrySubStyleList.Codes.T2L, t2lDescriptionForImport);
			list.Sort();
		}

		ZString GetCacheKey(bool isExs, bool isH2, bool isExport, bool isImport)
		{
			var cacheSubKey = string.Empty;
			if (isExs)
			{
				cacheSubKey = "EXS";
			}
			else if (isH2)
			{
				cacheSubKey = "H2";
			}
			else if (isExport)
			{
				cacheSubKey = "EXP";
			}
			else if (isImport)
			{
				cacheSubKey = "IMP";
			}
			return string.Format("ES.EntrySubStyleList_{0}", cacheSubKey);
		}

		CodeDescriptionPairList GetEntrySubStyleListForH2()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Declaration.EntrySubStyleList.Codes.A, Declaration.EntrySubStyleList.Descriptions.A);
			result.AddPair(Declaration.EntrySubStyleList.Codes.B, Declaration.EntrySubStyleList.Descriptions.B);
			result.AddPair(Declaration.EntrySubStyleList.Codes.X, Declaration.EntrySubStyleList.Descriptions.X);
			result.AddPair(Declaration.EntrySubStyleList.Codes.Z, Declaration.EntrySubStyleList.Descriptions.Z);
			return result;
		}
	}
}
