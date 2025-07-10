using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.CH.Business;

public class SupportingDocument : Customs.Business.CusSupportingInfo
{
	public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

	public new class Schema : Customs.Business.CusSupportingInfo.Schema
	{
		public const int ReferenceNumberMaxLength = 35;
		public const int ReferenceNumber2MaxLength = 70;
	}

	public new ICusSupportingInfoParent Parent => (ICusSupportingInfoParent)base.Parent;

	public JobDeclaration Declaration
	{
		get
		{
			JobDeclaration result = null;
			switch (Parent)
			{
				case JobDeclaration declaration:
					result = declaration;
					break;
				case JobComInvoiceHeader invoiceHeader:
					result = invoiceHeader.JobDeclaration;
					break;
				case JobComInvoiceLine invoiceLine:
					result = invoiceLine.JobDeclaration;
					break;
			}
			return result;
		}
	}

	public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

	public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

	protected override ZString HumanReadableNameCore => Res.GetString("1B05ABB1-5EE9-47D4-B991-1AF5DBAE7173", "Supporting Document");

	protected override Customs.Business.CusSupportingInfoValidation GetNewValidation()
	{
		return new SupportingDocumentValidation(this);
	}

	protected override Customs.Business.CusSupportingInfoLookups GetNewLookups()
	{
		return new SupportingDocumentLookups(this);
	}

	public override bool SupportsNotes => false;

	#region Properties

	[ResourceStringData("CHSupportingDocument|CSI_Code", Caption = "Type")]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			var oldValue = CSI_Code;
			base.CSI_Code = value;
			if (!IsCopying && oldValue != value)
			{
				Parent.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("CHSupportingDocument|CSI_ReferenceNumber", Caption = "Reference", FullDescription = "Reference Number", ShortCaption = "Ref.")]
	[MaxLength(Schema.ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber
	{
		get => base.CSI_ReferenceNumber;
		set => base.CSI_ReferenceNumber = value;
	}

	[ResourceStringData("C8F7D84B-233B-42C6-9FE6-8C60BFC048AA", Caption = "Additional Information", FullDescription = "Additional Information of the Supporting Document", MediumCaption = "Add. Information", ShortCaption = "Add. Inf.")]
	[MaxLength(Schema.ReferenceNumber2MaxLength)]
	public override ZString CSI_ReferenceNumber2
	{
		get => base.CSI_ReferenceNumber2;
		set => base.CSI_ReferenceNumber2 = value;
	}

	#endregion

	public bool IsGSPCertificate => (isGSPCertificate ?? (isGSPCertificate = new CachedProperty<bool>(Factory, () => RefCusCodeListLoader.IsGSPCertificate(Factory, CSI_Code, Parent?.DateOfValuation ?? ZDateTime.Today)))).Value;
	CachedProperty<bool> isGSPCertificate;

	public bool IsValidOriginDocument => (isValidOriginDocument ?? (isValidOriginDocument = new CachedProperty<bool>(Factory, () => RefCusCodeListLoader.IsValidOriginDocument(Factory, CSI_Code, Parent?.DateOfValuation ?? ZDateTime.Today)))).Value;
	CachedProperty<bool> isValidOriginDocument;
}
