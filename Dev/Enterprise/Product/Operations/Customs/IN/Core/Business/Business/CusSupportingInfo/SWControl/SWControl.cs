using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public class SWControl : CusSupportingInfoWithSerialNo
{
	public SWControl(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoINCusSupportingInfo.Schema
	{
		public new const int CSI_DescriptionMaxLength = 4000;
		public new const int CSI_ReferenceNumber2MaxLength = 8;
	}

	[ResourceStringData("Enterprise.Customs.IN.Business.SWControl|ControlResultDescription", Caption = "Control Result Description", MediumCaption = "Result Desc.", ShortCaption = "Desc.")]
	public ZString ControlResultDescription => UniversalReferenceDataHelper.GetSWControl(Factory, CSI_ReferenceNumber2, DateOfValuation)?.ZZD_Description ?? ZString.Empty;

	public ZPropertyInfo ControlResultDescriptionInfo => GetZPropertyInfo(nameof(ControlResultDescription));

	[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
	[List(nameof(Lookups) + "." + nameof(SWControlLookups.ControlResultCodeList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.SWControl|CSI_ReferenceNumber2", Caption = "Control Result Code", MediumCaption = "Ctrl. Result", ShortCaption = "Result")]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	[MaxLength(Schema.CSI_DescriptionMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.SWControl|CSI_Description", Caption = "Control Result Remark", MediumCaption = "Ctrl. Remark", ShortCaption = "Remark")]
	public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.SWControl|CSI_ControlLocation ", Caption = "Control Location", MediumCaption = "Ctrl. Location", ShortCaption = "Location")]
	public override ZString CSI_ControlLocation { get => base.CSI_ControlLocation; set => base.CSI_ControlLocation = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.SWControl|CSI_DateOfIssue", Caption = "Control Start Date", MediumCaption = "Ctrl. Start Dt.", ShortCaption = "Start Dt.")]
	public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.SWControl|CSI_DateOfExpiry", Caption = "Control End Date", MediumCaption = "Ctrl. End Dt.", ShortCaption = "End Dt.")]
	public override ZDateTime CSI_DateOfExpiry { get => base.CSI_DateOfExpiry; set => base.CSI_DateOfExpiry = value; }

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = CusSupportingInfoTypeList.Codes.SingleWindowControl;
	}

	public new SWControlValidation Validation => (SWControlValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new SWControlValidation(this);

	public new SWControlLookups Lookups => (SWControlLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new SWControlLookups(this);

	public ZDateTime DateOfValuation => (Parent as IDateOfValuationProvider)?.DateOfValuation ?? ZDateTime.Today;
}
