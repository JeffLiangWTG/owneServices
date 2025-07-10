using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class EntryInstructionDetailsUserControl : EU.GUI.EntryInstructionDetailsUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();
			FixTabPageOrder();
		}

		void FixTabPageOrder()
		{
			EntryInstructionTabControl.TabPages.Remove(SpecialProceduresTabPage);
			EntryInstructionTabControl.TabPages.Remove(AdditionalInfoTabPage);
			EntryInstructionTabControl.TabPages.Add(AdditionalInfoTabPage);
			EntryInstructionTabControl.TabPages.Add(SpecialProceduresTabPage);
		}

		protected new const string SupportingInfoColumnLayoutContext = "INS";

		protected override Type GetAdditionalInfosUserControlType() => IsImport ? typeof(ImportAdditionalInfosUserControlWithGrid) : typeof(AdditionalInfosUserControlWithGrid);

		protected override Type GetDetailsUserControlType() => typeof(LayoutEntryInstructionDetailBasicUserControl);

		protected override Type GetGridUserControl() => typeof(EntryInstructionGridUserControl);

		protected override ResourceStringData GetAdditionalInfosTabCaption() => Res.GetData("E1E673BC-9B79-40C4-8E24-1466C940C6A6", "Additional Documents");

		protected override void SetAuthorisationsTabPageCaption()
		{
			AuthorisationsTabPage.CaptionResourceString = IsUCC5AndIsImport
				? Res.GetData("81F00359-3491-4F76-A68B-1AC2D2A84F0C", "[3/39] Authorizations")
				: AuthorisationsTabPage.CaptionResourceString = Res.GetData("39F2F954-7A4B-4679-86A4-70205C9AFE6D", "Authorizations");
		}

		protected override void SetSupplyChainActorTabPageCaption()
		{
			if (JobDeclaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport)
			{
				SupplyChainActorTabPage.CaptionResourceString = Res.GetData("9EB848FB-0DCD-41D1-9878-CDC812504168", "[3/37] Add. Supply Chain Actors");
			}
			else
			{
				base.SetSupplyChainActorTabPageCaption();
			}
		}

		protected override Type GetPreviousDocumentsUserControlType() => IsUCC5AndIsImport ? typeof(UCC5ImportPreviousDocumentsUserControl) : typeof(InvoiceHeaderExportPreviousDocumentsUserControl);

		protected override ResourceStringData PreviousDocumentsTabPageCaption => IsUCC5AndIsImport
				? Res.GetData("5F610862-9887-4291-9CA3-A31BFBDC9674", "[2/1] Previous Documents")
				: Res.GetData("E42F47B7-52C3-4059-B1EA-83ECFB7A8476", "Previous Documents");

		protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLayoutSupportingDocumentsUserControl);

		protected override ResourceStringData SupportingDocumentsTabPageCaption => IsUCC5AndIsImport
				? Res.GetData("36086CB1-E6C6-4B04-8640-9042BF9354A7", "[2/3] Supporting Documents")
				: Res.GetData("9A67C0F8-3AC7-48B0-B7B0-D1199C6AC7D8", "Supporting Documents");

		protected override Type GetGuaranteesUserControlType() => typeof(IEEntryInstructionGuaranteesUserControl);

		protected override Type GetSpecialProceduresUserControlType() => typeof(SpecialProceduresUserControl);

		bool IsImport => JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsImport;

		bool IsUCC5AndIsImport => JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5AndIsImport;
	}
}
