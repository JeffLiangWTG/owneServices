using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class AddInfoCusEntryInstructionLookups : EU.Business.Declaration.AddInfoCusEntryInstructionLookups
	{
		public AddInfoCusEntryInstructionLookups(EU.Business.Declaration.AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		public new AddInfoCusEntryInstruction Parent => (AddInfoCusEntryInstruction)base.Parent;

		public CodeDescriptionPairList PartyConstellationCodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var entry = Parent.Parent;
				var isExportDeclaration = entry.JobDeclaration?.IsExport ?? false;
				if (isExportDeclaration)
				{
					var subStyle = entry.CEI_SubStyle;
					var style = entry.CEI_Style;
					result = Factory.GetCachedValue(string.Join("|", "DE.AddInfoCusEntryInstructionLookups.PartyConstellationCodeList", style, subStyle), () =>
					{
						var list = new CodeDescriptionPairList();
						if (subStyle == ExportDeclarationTypeTimeList.Codes._20 && style == ExportDeclarationTypeProcedureList.Codes._000000)
						{
							list.AddPair(Business.PartyConstellationCodeList.Codes._0000, Business.PartyConstellationCodeList.Descriptions._0000);
							list.AddPair(Business.PartyConstellationCodeList.Codes._0100, Business.PartyConstellationCodeList.Descriptions._0100);
							list.AddPair(Business.PartyConstellationCodeList.Codes._1000, Business.PartyConstellationCodeList.Descriptions._1000);
							list.AddPair(Business.PartyConstellationCodeList.Codes._1100, Business.PartyConstellationCodeList.Descriptions._1100);
						}
						else if (style.SubstringSafe(3, 1) == "9" || style.StartsWith("111"))
						{
							list.AddPair(Business.PartyConstellationCodeList.Codes._0000, Business.PartyConstellationCodeList.Descriptions._0000);
							list.AddPair(Business.PartyConstellationCodeList.Codes._0010, Business.PartyConstellationCodeList.Descriptions._0010);
							list.AddPair(Business.PartyConstellationCodeList.Codes._0100, Business.PartyConstellationCodeList.Descriptions._0100);
							list.AddPair(Business.PartyConstellationCodeList.Codes._0110, Business.PartyConstellationCodeList.Descriptions._0110);
							list.AddPair(Business.PartyConstellationCodeList.Codes._1000, Business.PartyConstellationCodeList.Descriptions._1000);
							list.AddPair(Business.PartyConstellationCodeList.Codes._1010, Business.PartyConstellationCodeList.Descriptions._1010);
							list.AddPair(Business.PartyConstellationCodeList.Codes._1100, Business.PartyConstellationCodeList.Descriptions._1100);
							list.AddPair(Business.PartyConstellationCodeList.Codes._1110, Business.PartyConstellationCodeList.Descriptions._1110);
						}
						else
						{
							list = new PartyConstellationCodeList();
						}
						return list;
					});
				}
				return result;
			}
		}
	}
}
