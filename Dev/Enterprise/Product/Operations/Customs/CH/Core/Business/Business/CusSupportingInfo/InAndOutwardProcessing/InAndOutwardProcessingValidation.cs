using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public sealed class InAndOutwardProcessingValidation : CusSupportingInfoValidation
{
	public InAndOutwardProcessingValidation(InAndOutwardProcessing parent) : base(parent)
	{
	}

	new InAndOutwardProcessing Parent => (InAndOutwardProcessing)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New(Parent.Parent?.JobDeclaration));
	PlausiValidation plausiValidation;

	protected override void CheckCSI_SubType()
	{
		base.CheckCSI_SubType();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo);
		PlausiValidation.CheckR205(Parent.CSI_SubTypeInfo, Parent);
		PlausiValidation.CheckR206(Parent.CSI_SubTypeInfo, Parent);
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		PlausiValidation.CheckR205(Parent.CSI_CodeInfo, Parent);
		PlausiValidation.CheckNS30003_InwardOutward(Parent.CSI_CodeInfo, Parent);
		PlausiValidation.CheckNS30098_Procedures41And50(Parent.CSI_CodeInfo, Parent, () => DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.InAndOutwardProcessingRefinementType)).Caption);
		PlausiValidation.CheckNS30098_Procedure20(Parent.CSI_CodeInfo, Parent);
	}

	protected override void CheckCSI_Procedure()
	{
		base.CheckCSI_Procedure();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_ProcedureInfo);
		PlausiValidation.CheckR361(Parent.CSI_ProcedureInfo, Parent.Parent);
		PlausiValidation.CheckR190(Parent.CSI_ProcedureInfo, Parent);
		PlausiValidation.CheckR229(Parent.CSI_ProcedureInfo, Parent);
		PlausiValidation.CheckR205(Parent.CSI_ProcedureInfo, Parent);
		PlausiValidation.CheckR208(Parent.CSI_ProcedureInfo, Parent);
		PlausiValidation.CheckNP70162(Parent.CSI_ProcedureInfo, Parent.Parent);
		PlausiValidation.CheckNS30003_InwardOutward(Parent.CSI_ProcedureInfo, Parent);
		PlausiValidation.CheckNS30098_Procedures41And50(Parent.CSI_ProcedureInfo, Parent, () => DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.InAndOutwardProcessingProcessType)).Caption);
		PlausiValidation.CheckNS30098_Procedure20(Parent.CSI_ProcedureInfo, Parent);
		PlausiValidation.CheckNP70165(Parent.CSI_ProcedureInfo, Parent);
	}

	protected override void CheckCSI_IssuerType()
	{
		base.CheckCSI_IssuerType();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_IssuerTypeInfo);
		PlausiValidation.CheckR205(Parent.CSI_IssuerTypeInfo, Parent);
		PlausiValidation.CheckR261IssuerType(Parent.CSI_IssuerTypeInfo, Parent);
		PlausiValidation.CheckNS30003_InwardOutward(Parent.CSI_IssuerTypeInfo, Parent);
		plausiValidation.CheckNP70163(Parent.CSI_IssuerTypeInfo, Parent);
		PlausiValidation.CheckNS30098_Procedures41And50(Parent.CSI_IssuerTypeInfo, Parent, () => DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.InAndOutwardProcessingBillingType)).Caption);
		PlausiValidation.CheckNS30098_Procedure20(Parent.CSI_IssuerTypeInfo, Parent);
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		PlausiValidation.CheckR206(Parent.CSI_DescriptionInfo, Parent);
		PlausiValidation.CheckR358(Parent.CSI_DescriptionInfo, Parent);
		PlausiValidation.CheckNS30003_InwardOutward(Parent.CSI_DescriptionInfo, Parent);
		PlausiValidation.CheckNS30098_Description(Parent.CSI_DescriptionInfo, Parent);
	}

	protected override void CheckCSI_Status()
	{
		base.CheckCSI_Status();
		PlausiValidation.CheckNS30003_Ticked(Parent.RepairInfo, Parent.Parent?.EntryInstruction, () => DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.InAndOutwardProcessingRepair)).Caption);
	}

	protected override void CheckCSI_CustomsOffice()
	{
		base.CheckCSI_CustomsOffice();
		PlausiValidation.CheckNS30003_InwardOutward(Parent.CSI_CustomsOfficeInfo, Parent);
		PlausiValidation.CheckNS30098_CustomsOffice(Parent.CSI_CustomsOfficeInfo, Parent);

		if (Parent.Parent?.Declaration?.IsExport ?? false)
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CustomsOfficeInfo);
		}
	}
}
