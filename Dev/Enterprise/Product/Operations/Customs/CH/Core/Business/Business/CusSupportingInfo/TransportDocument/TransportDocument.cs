using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.CH.Business;

public class TransportDocument : Customs.Business.CusSupportingInfo
{
	public TransportDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

	public new class Schema : Customs.Business.AutoCusSupportingInfo.Schema
	{
		public const int ReferenceNumberMaxLength = 70;
	}

	public new ITransportDocumentParent Parent => (ITransportDocumentParent)base.Parent;

	public JobDeclaration JobDeclaration => Parent.JobDeclaration;

	public new TransportDocumentValidation Validation => (TransportDocumentValidation)base.Validation;

	public new TransportDocumentLookups Lookups => (TransportDocumentLookups)base.Lookups;

	protected override ZString HumanReadableNameCore => Res.GetString("8F562239-22AE-49AA-AF34-461389EBCA88", "Transport Document");

	protected override Customs.Business.CusSupportingInfoValidation GetNewValidation()
	{
		return new TransportDocumentValidation(this);
	}

	protected override Customs.Business.CusSupportingInfoLookups GetNewLookups()
	{
		return new TransportDocumentLookups(this);
	}

	public override bool SupportsNotes => false;

	#region Properties

	[ResourceStringData("CHTransportDocument|CSI_Code", Caption = "Type")]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			var oldValue = CSI_Code;
			base.CSI_Code = value;
			if (!IsCopying && oldValue != value)
			{
				JobDeclaration.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("CHTransportDocument|CSI_ReferenceNumber", Caption = "Reference", FullDescription = "Reference Number", ShortCaption = "Ref.")]
	[MaxLength(Schema.ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber
	{
		get => base.CSI_ReferenceNumber;
		set => base.CSI_ReferenceNumber = value;
	}

	#endregion
}
