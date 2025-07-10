using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryInstructionValidation : EU.Business.Declaration.CusEntryInstructionValidation
{
	public CusEntryInstructionValidation(CusEntryInstruction parent)
		: base(parent)
	{
	}

	protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

	protected override void CheckCEI_OA_Warehouse2()
	{
		base.CheckCEI_OA_Warehouse2();
		if (Parent.CEI_OA_Warehouse2.IsEmpty
			&& Parent.JobDeclaration is JobDeclaration jobDeclaration
			&& jobDeclaration.IsImport
			&& jobDeclaration.HasProcedureOutOfWarehouse)
		{
			Parent.CEI_OA_Warehouse2Info.AddMessageError(Res.GetString("7927CE42-9078-458B-B891-181B1A12A51A", "Warehouse details are required for this procedure"));
		}
	}

	protected override void CheckCEI_OA_Warehouse()
	{
		base.CheckCEI_OA_Warehouse();
		if (Parent.CEI_OA_Warehouse.IsEmpty
			&& Parent.JobDeclaration is JobDeclaration jobDeclaration
			&& jobDeclaration.IsImport
			&& jobDeclaration.HasProcedureIntoWarehouse)
		{
			Parent.CEI_OA_WarehouseInfo.AddMessageError(Res.GetString("7EE56CE9-10CD-416A-85DA-104378AE1AE9", "Warehouse details are required for this procedure"));
		}
	}

	protected override void CheckCEI_SubStyle()
	{
		base.CheckCEI_SubStyle();

		if (Parent.JobDeclaration is JobDeclaration jobDeclaration)
		{
			if ((Parent.CEI_SubStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE
			|| Parent.CEI_SubStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF)
			&& !jobDeclaration.PreviousDocuments.Cast<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument>().Any(x => x.CSI_Code == NLConstants.PreviousDocumentTypes.AcknowledgmentOfMRN)
			&& !jobDeclaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.PreviousDocuments.Cast<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument>().Any(y => y.CSI_Code == NLConstants.PreviousDocumentTypes.AcknowledgmentOfMRN))
			&& !jobDeclaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.PreviousDocuments.Cast<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument>().Any(y => y.CSI_Code == NLConstants.PreviousDocumentTypes.AcknowledgmentOfMRN)))
			{
				Parent.CEI_SubStyleInfo.AddMessageError(Res.GetString("BD2E07FF-7996-4EFE-A6DE-0FCFFA9B2C70", "For Sub Style X or Y, a Previous Document with a Type of NMRN is required to be present on the Invoice Header or Invoice Line or Misc. tab"));
			}

			if ((Parent.CEI_SubStyle == EntrySubStyleList.Codes.IncompleteDeclaration
				|| Parent.CEI_SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB
				|| Parent.CEI_SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC
				|| Parent.CEI_SubStyle == EntrySubStyleList.Codes.SimplifiedDeclaration)
				&& Parent.AdditionalInfos.Count == 0)
			{
				Parent.CEI_SubStyleInfo.AddMessageError(Res.GetString("7F953F63-BDA3-4FFD-9780-AB9DE830A2FB", "[C9003] If Sub style is B, C, E or F an additional document kind INF is required of type NRV"));
			}
		}
	}

	protected override void CheckGoodsLocationDescription()
	{
		base.CheckGoodsLocationDescription();
		var parent = Parent;
		if (parent.JobDeclaration is JobDeclaration jobDeclaration
			&& jobDeclaration.IsExport && parent.GoodsLocationDescription.IsEmpty && !parent.CEI_SubStyle.In(new ZString[] { DeclarationSubTypeList.Codes.D, DeclarationSubTypeList.Codes.E, DeclarationSubTypeList.Codes.F }))
		{
			parent.GoodsLocationDescriptionInfo.AddMessageError(Res.GetString("801231b3-f1af-4d8a-a771-dd5d0434c333", "Goods location is required for this Sub Style"));
		}
	}
}
