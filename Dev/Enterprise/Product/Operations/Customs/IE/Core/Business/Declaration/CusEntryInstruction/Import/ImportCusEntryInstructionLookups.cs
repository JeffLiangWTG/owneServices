using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportCusEntryInstructionLookups : CusEntryInstructionLookups
	{
		public ImportCusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction)
		{
		}

		protected override CodeDescriptionPairList DeclarationTypeListCore => Factory.GetCachedValue("IEUCC6ImportDeclarationTypeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ImportDeclarationTypeList.Codes.H1, ImportDeclarationTypeList.Descriptions.H1);
			result.AddPair(ImportDeclarationTypeList.Codes.H2, ImportDeclarationTypeList.Descriptions.H2);
			result.AddPair(ImportDeclarationTypeList.Codes.H3, ImportDeclarationTypeList.Descriptions.H3);
			result.AddPair(ImportDeclarationTypeList.Codes.H4, ImportDeclarationTypeList.Descriptions.H4);
			result.AddPair(ImportDeclarationTypeList.Codes.H5, ImportDeclarationTypeList.Descriptions.H5);
			result.AddPair(ImportDeclarationTypeList.Codes.H6, ImportDeclarationTypeList.Descriptions.H6);
			result.AddPair(ImportDeclarationTypeList.Codes.I1, ImportDeclarationTypeList.Descriptions.I1);
			return result;
		});

		protected override CodeDescriptionPairList GetEntrySubstyleList(ICanBeImportOrExport parent)
		{
			var declarationType = Parent.CEI_Style;
			return Factory.GetCachedValue("40156727-be82-4c21-aa33-27ea2bcc51f3|IE|EntrySubStyleList|" + declarationType, () =>
			{
				var result = new CodeDescriptionPairList();
				if (declarationType == ImportDeclarationTypeList.Codes.H1
					|| declarationType == ImportDeclarationTypeList.Codes.H2
					|| declarationType == ImportDeclarationTypeList.Codes.H3
					|| declarationType == ImportDeclarationTypeList.Codes.H4)
				{
					result.AddPair(EU.Business.EntrySubStyleList.Codes.NormalDeclaration, EU.Business.EntrySubStyleList.Descriptions.NormalDeclaration);
					result.AddPair(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EU.Business.EntrySubStyleList.Descriptions.PreliminaryDeclarationUnderCodeA);
					result.AddPair(EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EU.Business.EntrySubStyleList.Descriptions.SupplementaryDeclarationForCodeCOrCodeF);
					result.AddPair(EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic, EU.Business.EntrySubStyleList.Descriptions.SupplementaryDeclarationPeriodic);
				}
				else if (declarationType == ImportDeclarationTypeList.Codes.I1)
				{
					result.AddPair(EU.Business.EntrySubStyleList.Codes.SimplifiedDeclaration, EU.Business.EntrySubStyleList.Descriptions.SimplifiedDeclaration);
					result.AddPair(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EU.Business.EntrySubStyleList.Descriptions.PreliminaryDeclarationUnderCodeC);
					result.AddPair(EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic, EU.Business.EntrySubStyleList.Descriptions.SupplementaryDeclarationPeriodic);
				}
				return result;
			});
		}
	}
}
