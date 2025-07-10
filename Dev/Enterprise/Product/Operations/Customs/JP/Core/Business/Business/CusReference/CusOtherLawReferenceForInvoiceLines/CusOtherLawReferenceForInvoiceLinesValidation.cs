using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.JP.Business.JPExportDeclarationTypeList;

namespace Enterprise.Customs.JP.Business;

public class CusOtherLawReferenceForInvoiceLinesValidation(CusOtherLawReferenceForInvoiceLines parent) : CusOtherLawReferenceValidation(parent)
{
	public new CusOtherLawReferenceForInvoiceLines Parent => (CusOtherLawReferenceForInvoiceLines)base.Parent;

	JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent.Parent;

	protected override void CheckCFR_Reference_RoadTranportVehicleLaw()
	{
		var parent = Parent;
		var targetInfo = parent.CFR_ReferenceInfo;
		var invoiceLine = InvoiceLine;
		var entryInstruction = invoiceLine.EntryInstruction;
		var mutuallyExclusiveOtherLaws = invoiceLine.OtherLaws.Where(x => x.IsRoadTranportVehicleLaw && x.CFR_Reference != parent.CFR_Reference).ToArray();

		if (entryInstruction != null)
		{
			if (new ReturnedGoodsDeclarationTypeList().ContainsCode(entryInstruction.CEI_Style))
			{
				targetInfo.AddMessageError(Res.GetString("F731F5DE-8E03-49ED-923C-2C6175B4C675", "Value is incompatible with Declaration Type."));
			}

			var bondedAreaCode = JPRefCusCodeListTypes.GetJapanBondedAreaCode(parent.Factory, entryInstruction.CEI_BondedLocationCode);
			if (bondedAreaCode != null && bondedAreaCode.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_Value.EqualsIgnoringCase("洋上")))
			{
				targetInfo.AddMessageError(Res.GetString("9D41E6C6-EB5B-4DCD-A48E-3DB0F065AE8E", "Other Laws and Regulations Codes {0} should not be entered when direct landing (when a Bonded Location Code of Type 洋上 is entered).", parent.CFR_Reference));
			}

			if (parent.CFR_Reference == CusOtherLawReference.Constants.MS && entryInstruction.ApprovalCertificateInfos.ToArray().Cast<ApprovalCertificateInfo>().All(x => x.CSI_Code != ApprovalCertificateInfoCodes.MOTS))
			{
				targetInfo.AddMessageError(Res.GetString("76F3347F-77A4-45C6-AD6A-9756951E1151", "Approval Certificate MOTS is required when Other Laws and Regulations Code MS is entered."));
			}
		}

		if (mutuallyExclusiveOtherLaws.Length != 0)
		{
			mutuallyExclusiveOtherLaws.ForEach(l => l.Validation.ValidateCFR_Reference());
			targetInfo.AddMessageError(Res.GetString("A3E20785-751A-4CDE-9535-4D58B4720CE7", "The MS and MM codes in the Other Laws and Regulations are mutually exclusive. Please remove one of them."));
		}
	}
}
