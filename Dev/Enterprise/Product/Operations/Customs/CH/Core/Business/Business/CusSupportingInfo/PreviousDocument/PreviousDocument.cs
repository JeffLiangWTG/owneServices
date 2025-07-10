using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.CH.Business;

public class PreviousDocument : Customs.Business.CusSupportingInfo
{
	public new class Schema : Customs.Business.CusSupportingInfo.Schema
	{
		public const int CodeMaxLength = 6;
		public const int ReferenceNumberMaxLength = 35;
		public const int DescriptionMaxLength = 70;
	}

	public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

	public new ICusSupportingInfoParent Parent => (ICusSupportingInfoParent)base.Parent;

	protected override Customs.Business.CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

	protected override Customs.Business.CusSupportingInfoValidation GetNewValidation() => new PreviousDocumentValidation(this);

	public override bool SupportsNotes => false;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.PreviousDocument;
	}

	#region Properties
	[ResourceStringData("Enterprise.Customs.CH.Business.PreviousDocument|CSI_Code", Caption = "Type")]
	[MaxLength(Schema.CodeMaxLength)]
	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.CodeList))]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set => base.CSI_Code = value;
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.PreviousDocument|CSI_ReferenceNumber", Caption = "Reference")]
	[MaxLength(Schema.ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber
	{
		get => base.CSI_ReferenceNumber;
		set => base.CSI_ReferenceNumber = value;
	}

	[ResourceStringData("Enterprise.Customs.CH.Business.PreviousDocument|CSI_Description", Caption = "Additional Information")]
	[MaxLength(Schema.DescriptionMaxLength)]
	public override ZString CSI_Description
	{
		get => base.CSI_Description;
		set => base.CSI_Description = value;
	}
	#endregion
}
