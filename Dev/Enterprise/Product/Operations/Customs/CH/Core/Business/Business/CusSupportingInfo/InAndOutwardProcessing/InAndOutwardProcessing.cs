using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public sealed class InAndOutwardProcessing : SingleCusSupportingInfo
{
	public InAndOutwardProcessing(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : CusSupportingInfo.Schema
	{
		public new const int CSI_SubTypeMaxLength = 1;
		public new const int CSI_CodeMaxLength = 1;
		public new const int CSI_ProcedureMaxLength = 1;
		public new const int CSI_IssuerTypeMaxLength = 1;
		public new const int CSI_StatusMaxLength = 1;
		public new const int CSI_DescriptionMaxLength = 280;
	}

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	protected override CusSupportingInfoLookups GetNewLookups() => new InAndOutwardProcessingLookups(this);

	public new InAndOutwardProcessingLookups Lookups => (InAndOutwardProcessingLookups)base.Lookups;

	protected override CusSupportingInfoValidation GetNewValidation() => new InAndOutwardProcessingValidation(this);

	public new InAndOutwardProcessingValidation Validation => (InAndOutwardProcessingValidation)base.Validation;

	public override bool SupportsNotes => false;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.InAndOutwardProcessing;
		Repair = false;
	}

	protected override bool IsEmpty => base.IsEmpty && !Repair;

	public override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
	{
		yield return CSI_SubTypeInfo;
		yield return CSI_CodeInfo;
		yield return CSI_ProcedureInfo;
		yield return CSI_IssuerTypeInfo;
		yield return CSI_DescriptionInfo;
		yield return CSI_CustomsOfficeInfo;
	}

	#region Properties
	public ZDateTime EffectiveAssessmentDate => Parent?.EffectiveAssessmentDate ?? ZDate.Today;

	[LightValidationTestExempt]
	public override ZString CSI_ParentTableCode { get => base.CSI_ParentTableCode; set => base.CSI_ParentTableCode = value; }

	[LightValidationTestExempt]
	public override ZString CSI_Type { get => base.CSI_Type; set => base.CSI_Type = value; }

	[List(nameof(Lookups) + "." + nameof(InAndOutwardProcessingLookups.DirectionList))]
	[MaxLength(Schema.CSI_SubTypeMaxLength)]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingDirection", Caption = "Direction")]
	public override ZString CSI_SubType
	{
		get => base.CSI_SubType;
		set => base.CSI_SubType = value;
	}

	[List(nameof(Lookups) + "." + nameof(InAndOutwardProcessingLookups.RefinementTypeList))]
	[MaxLength(Schema.CSI_CodeMaxLength)]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingRefinementType", Caption = "Refinement Type")]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set => base.CSI_Code = value;
	}

	[List(nameof(Lookups) + "." + nameof(InAndOutwardProcessingLookups.ProcessTypeList))]
	[MaxLength(Schema.CSI_ProcedureMaxLength)]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingProcessType", Caption = "Process Type")]
	public override ZString CSI_Procedure
	{
		get => base.CSI_Procedure;
		set => base.CSI_Procedure = value;
	}

	[List(nameof(Lookups) + "." + nameof(InAndOutwardProcessingLookups.BillingTypeList))]
	[MaxLength(Schema.CSI_IssuerTypeMaxLength)]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingBillingType", Caption = "Billing Type")]
	public override ZString CSI_IssuerType
	{
		get => base.CSI_IssuerType;
		set => base.CSI_IssuerType = value;
	}

	[MaxLength(Schema.CSI_StatusMaxLength)]
	public override ZString CSI_Status
	{
		get => base.CSI_Status;
		set
		{
			base.CSI_Status = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateCSI_SubType();
				Validation.ValidateCSI_Description();
				Parent?.Validation.ValidateJI_NonTradingGoods();
				Parent?.MarkAsNeedingValidation();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(InAndOutwardProcessingLookups.NotifyCustomsOfficeList))]
	public override ZString CSI_CustomsOffice { get => base.CSI_CustomsOffice; set => base.CSI_CustomsOffice = value; }

	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingRepair", Caption = "Repair")]
	public ZBool Repair
	{
		get => CSI_Status == InAndOutwardProcessingStatusCodes.RepairTrue;
		set
		{
			CSI_Status = value ? InAndOutwardProcessingStatusCodes.RepairTrue : InAndOutwardProcessingStatusCodes.RepairFalse;
			RepairInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo RepairInfo => GetWrappedZPropertyInfo(nameof(Repair), x => CSI_StatusInfo);

	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingRepairReason", Caption = "Repair Reason")]
	[ResourceStringData("Enterprise.Customs.CH.Business.JobComInvoiceLine|InAndOutwardProcessingRepairReason", Caption = "Reason", IsApplicableMember = nameof(IsExportOrExportDeclarationActivation))]
	public override ZString CSI_Description
	{
		get => base.CSI_Description;
		set => base.CSI_Description = value;
	}

	#endregion

	protected override void OnFactorySaving()
	{
		base.OnFactorySaving();
		if (IsExportOrExportDeclarationActivation)
		{
			CSI_SubType = ZString.Empty;
		}
		else
		{
			CSI_CustomsOffice = ZString.Empty;
		}
	}

	public bool IsExportOrExportDeclarationActivation => Parent?.Declaration?.IsExportOrExportDeclarationActivation ?? false;
}
