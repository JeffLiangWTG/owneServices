using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class LEXSEDDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var common = LEXSEDDetailsControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(108);
			var ruler2 = layout.CreateRuler(345);

			layout.Include(ruler1, common.CustomsOfficeCodeFindBox, ruler2, common.CustomsDivisionCodeFindBox);
			layout.Include(ruler1, common.BondedAreaCodeFindBox, ruler2, common.CrewCountCalcEdit);
			layout.Include(ruler1, common.SubLocationOfGoodsTextBox);
			layout.Include(ruler1, common.BlanketDeclarationDropEdit, ruler2, common.DeclarationDateEdit);
			layout.Include(common.GridUserControl);

			layout.SetCaption<JobDeclaration>(common.SubLocationOfGoodsTextBox, h => LocalExportTransactionNatureCodeList.Is5DP(h?.JE_MessageSubType) ? Res.GetData("17FA7CD4-9043-42BD-9C2E-836D05031C15", "Goods Loc. Details") : Res.GetData("AB82A32A-DADB-4A84-8A98-203EDD2AAA4A", "Goods Loc. Details"));

			layout.SetVisibility<JobDeclaration>(common.CrewCountCalcEdit, h => h.IsLocalExportToSeaVessel, h => h.JE_MessageSubTypeInfo);
			layout.SetVisibility<JobDeclaration>(common.BlanketDeclarationDropEdit, h => h.GetLocalExportMessageType() == ElectronicDocumentTypeList.Codes._5DP, h => h.JE_MessageSubTypeInfo);
			layout.SetVisibility<JobDeclaration>(common.DeclarationDateEdit, h => h.GetLocalExportMessageType() == ElectronicDocumentTypeList.Codes._5DP || h.JE_MessageSubType == LocalExportTransactionNatureCodeList.Codes._08, h => h.JE_MessageSubTypeInfo);
			layout.SetVisibility<JobDeclaration>(common.GridUserControl, h => h.IsLocalExportToSeaVessel, h => h.JE_MessageSubTypeInfo);

			return layout;
		}
	}
}
