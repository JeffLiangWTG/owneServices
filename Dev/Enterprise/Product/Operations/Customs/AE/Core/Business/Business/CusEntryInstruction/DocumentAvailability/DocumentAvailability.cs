using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AE.Business;

public class DocumentAvailability : CusSupportingInfo
{
	public DocumentAvailability(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[List(nameof(Lookups) + "." + nameof(DocumentAvailabilityLookups.DocumentTypeList))]
	[ResourceStringData("Enterprise.Customs.AE.Business.DocumentAvailability|CSI_Code", Caption = "Document Type")]
	public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

	[List(nameof(Lookups) + "." + nameof(DocumentAvailabilityLookups.AvailabilityStatusList))]
	[ResourceStringData("Enterprise.Customs.AE.Business.DocumentAvailability|CSI_Status", Caption = "Availability Status")]
	public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

	[List(nameof(Lookups) + "." + nameof(DocumentAvailabilityLookups.ReasonCodeList))]
	[ResourceStringData("Enterprise.Customs.AE.Business.DocumentAvailability|CSI_SubType", Caption = "Reason Code")]
	public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

	protected override CusSupportingInfoValidation GetNewValidation() => new DocumentAvailabilityValidation(this);

	public new DocumentAvailabilityValidation Validation => new DocumentAvailabilityValidation(this);

	protected override CusSupportingInfoLookups GetNewLookups() => new DocumentAvailabilityLookups(this);

	public new DocumentAvailabilityLookups Lookups => new DocumentAvailabilityLookups(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = AEConstants.CusSupportingInfoTypes.Codes.DocumentAvailability;
		CSI_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
	}
}
