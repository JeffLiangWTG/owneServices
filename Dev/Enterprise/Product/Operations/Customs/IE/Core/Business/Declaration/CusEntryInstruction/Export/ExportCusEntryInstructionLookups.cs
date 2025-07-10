using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportCusEntryInstructionLookups : CusEntryInstructionLookups
	{
		public ExportCusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction)
		{
		}

		protected override CodeDescriptionPairList DeclarationTypeListCore => Factory.GetCachedValue<ExportDeclarationTypeList>();

		public CodeDescriptionPairList DeclarationTypeListG2018 =>
			JobSubStyle.IsEmpty
			? new CodeDescriptionPairList()
			: Factory.GetCachedValue(DeclarationTypeListKey + JobSubStyle, () => GetNewDeclarationTypeList(JobSubStyle));

		ZString JobSubStyle => Parent.JobDeclaration?.JE_EntryStyle ?? ZString.Empty;

		CodeDescriptionPairList GetNewDeclarationTypeList(string jobSubStyle)
		{
			var result = new CodeDescriptionPairList();
			switch (jobSubStyle)
			{
				case MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion:
					result.AddPair(ExportDeclarationTypeList.Codes.B1, ExportDeclarationTypeList.Descriptions.B1);
					result.AddPair(ExportDeclarationTypeList.Codes.B2, ExportDeclarationTypeList.Descriptions.B2);
					result.AddPair(ExportDeclarationTypeList.Codes.C1, ExportDeclarationTypeList.Descriptions.C1);
					break;
				case MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec:
					result.AddPair(ExportDeclarationTypeList.Codes.B3, ExportDeclarationTypeList.Descriptions.B3);
					result.AddPair(ExportDeclarationTypeList.Codes.B4, ExportDeclarationTypeList.Descriptions.B4);
					break;
			}

			return result;
		}

		protected override CodeDescriptionPairList GetEntrySubstyleList(ICanBeImportOrExport parent)
		{
			var declarationType = Parent.CEI_Style;
			return Factory.GetCachedValue("IE.ExportCusEntryInstructionLookups.EntrySubStyleList|" + declarationType, () =>
			{
				var result = new CodeDescriptionPairList();

				switch (declarationType)
				{
					case ExportDeclarationTypeList.Codes.B1:
					case ExportDeclarationTypeList.Codes.B2:
					case ExportDeclarationTypeList.Codes.B3:
						result.AddPair(EU.Business.EntrySubStyleList.Codes.NormalDeclaration, EU.Business.EntrySubStyleList.Descriptions.NormalDeclaration);
						result.AddPair(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EU.Business.EntrySubStyleList.Descriptions.PreliminaryDeclarationUnderCodeA);
						result.AddPair(EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EU.Business.EntrySubStyleList.Descriptions.SupplementaryDeclarationForCodeCOrCodeF);
						break;
					case ExportDeclarationTypeList.Codes.B4:
						result.AddPair(EU.Business.EntrySubStyleList.Codes.NormalDeclaration, EU.Business.EntrySubStyleList.Descriptions.NormalDeclaration);
						result.AddPair(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EU.Business.EntrySubStyleList.Descriptions.PreliminaryDeclarationUnderCodeA);
						result.AddPair(EU.Business.EntrySubStyleList.Codes.SimplifiedDeclaration, EU.Business.EntrySubStyleList.Descriptions.SimplifiedDeclaration);
						result.AddPair(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EU.Business.EntrySubStyleList.Descriptions.PreliminaryDeclarationUnderCodeC);
						result.AddPair(EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EU.Business.EntrySubStyleList.Descriptions.SupplementaryDeclarationForCodeCOrCodeF);
						break;
					case ExportDeclarationTypeList.Codes.C1:
						result.AddPair(EU.Business.EntrySubStyleList.Codes.SimplifiedDeclaration, EU.Business.EntrySubStyleList.Descriptions.SimplifiedDeclaration);
						result.AddPair(EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EU.Business.EntrySubStyleList.Descriptions.PreliminaryDeclarationUnderCodeC);
						break;
				}

				return result;
			});
		}

		const string DeclarationTypeListKey = "IE.ExportCusEntryInstructionLookups.DeclarationTypeList";
	}
}
