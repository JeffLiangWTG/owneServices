using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class CusEntryInstructionValidation : AutoCHCusEntryInstructionValidation
{
	public CusEntryInstructionValidation(CusEntryInstruction parent)
		: base(parent)
	{
	}

	protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

	protected PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New(Parent.JobDeclaration));
	PlausiValidation plausiValidation;

	protected override void CheckCEI_Style()
	{
		base.CheckCEI_Style();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_StyleInfo);
	}

	protected override void CheckCEI_SubStyle()
	{
		base.CheckCEI_SubStyle();
		var jobDeclaration = Parent.JobDeclaration;
		if (jobDeclaration.IsImport || jobDeclaration.IsExportActivationEdec)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_SubStyleInfo);
		}
	}

	protected override void CheckCEI_Description()
	{
		base.CheckCEI_Description();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_DescriptionInfo);
	}

	protected override void CheckCEI_OA_Warehouse()
	{
		base.CheckCEI_OA_Warehouse();
		if (!Parent.CEI_WarehouseTypeIsNotBondedWarehouse)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_OA_WarehouseInfo);
		}
	}

	protected override void CheckCEI_OH_Owner()
	{
		base.CheckCEI_OH_Owner();
		if (!Parent.CEI_WarehouseTypeIsNotBondedWarehouse)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_OH_OwnerInfo);
		}
	}

	protected override void CheckCEI_Procedure()
	{
		base.CheckCEI_Procedure();

		if (Parent.JobDeclaration.JE_MessageType == MessageTypeCodeList.Codes.Export)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_ProcedureInfo);
		}
	}

	protected override void CheckCEI_WarehouseType()
	{
		base.CheckCEI_WarehouseType();
		ListValidation.MessageErrorIfInvalidCode(Parent.CEI_WarehouseTypeInfo);
	}

	protected override void CheckCEI_DeclarationReason()
	{
		if (!Parent.CEI_DeclarationReason_ReadOnly)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_DeclarationReasonInfo);
			PlausiValidation.CheckR288(Parent.CEI_DeclarationReasonInfo, Parent);
			PlausiValidation.CheckR166c(Parent.CEI_DeclarationReasonInfo, Parent);
		}
	}

	protected override void CheckCEI_NextProcedure()
	{
		base.CheckCEI_NextProcedure();
		ListValidation.MessageErrorIfInvalidCode(Parent.CEI_NextProcedureInfo);
	}

	protected override void CheckCEI_TransportChargesMethodOfPayment()
	{
		base.CheckCEI_TransportChargesMethodOfPayment();
		PlausiValidation.CheckNS30108(Parent.CEI_TransportChargesMethodOfPaymentInfo, Parent);
		PlausiValidation.CheckNS30003_NotEmpty(Parent.CEI_TransportChargesMethodOfPaymentInfo, Parent);
	}

	protected override void CheckCEI_PartialDelivery()
	{
		base.CheckCEI_PartialDelivery();
		PlausiValidation.CheckNS30003_Ticked(Parent.CEI_PartialDeliveryInfo, Parent);
		PlausiValidation.CheckNP70212(Parent.CEI_PartialDeliveryInfo, Parent);
	}
}
