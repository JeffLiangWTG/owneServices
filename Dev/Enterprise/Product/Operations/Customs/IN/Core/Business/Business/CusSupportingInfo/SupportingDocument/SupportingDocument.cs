using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.Customs.IN.Business;

public class SupportingDocument : CusSupportingInfoWithSerialNo, IDocAddresses
{
	public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusSupportingInfo.Schema
	{
		public new const int CSI_ReferenceNumber2MaxLength = 16;
		public new const int CSI_IssuerTypeMaxLength = 3;
		public const string OrganizationPK = nameof(SupportingDocument.OrganizationPK);
	}

	[MaxLength(Schema.CSI_ReferenceNumber2MaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.SupportingDocument|CSI_ReferenceNumber2", Caption = "Image Reference Number", MediumCaption = "Image Ref. No.", ShortCaption = "IRN")]
	public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

	[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.CodeList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.SupportingDocument|CSI_Code", Caption = "Document Type Code", MediumCaption = "Doc. Type", ShortCaption = "Type")]
	public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

	[MaxLength(Schema.CSI_IssuerTypeMaxLength)]
	[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.RegistrationCodeList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.SupportingDocument|CSI_IssuerType", Caption = "Code", MediumCaption = "Code", ShortCaption = "Code")]
	public override ZString CSI_IssuerType { get => base.CSI_IssuerType; set => base.CSI_IssuerType = value; }

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
	}

	protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

	public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

	public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

	protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);

	IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new DocAddressType[] { DocAddressType.SupportingDocumentOrganizationAddress };

	ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => new SupportingDocumentJobDocAddressValidation(addressToValidate);

	SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

	public JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType) => addressType switch
	{
		DocAddressType.SupportingDocumentOrganizationAddress => new JobDocAddressRequirement(DocAddressType.SupportingDocumentOrganizationAddress),
		_ => null
	};

	void IDocAddresses.DocAddressChanged(JobDocAddress changedAddress) { }

	void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress) { }

	void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress) { }

	void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress) { }

	void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress) => CSI_IssuerType = ZString.Empty;

	bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

	public OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType) => Lookups.OrganizationList;

	[ChildEditable(true)]
	public JobDocAddressDependentCollection DocAddresses
	{
		get
		{
			if (docAddresses == null)
			{
				docAddresses = new JobDocAddressDependentCollection(this);
				docAddresses.Load();
				RegisterEditableChildObject(docAddresses);
			}

			return docAddresses;
		}
	}
	JobDocAddressDependentCollection docAddresses;

	[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.OrganizationList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.SupportingDocument|Organization", Caption = "Organization", MediumCaption = "Org.", ShortCaption = "Org.")]
	public ZGuid OrganizationPK { get => OrganizationAddress.OrganisationPK; set => OrganizationAddress.OrganisationPK = value; }

	public ZPropertyInfo OrganizationPKInfo => GetWrappedZPropertyInfo(Schema.OrganizationPK, x => OrganizationAddress.OrganisationPKInfo);

	public JobDocAddress OrganizationAddress
	{
		get
		{
			if (docAddress == null || docAddress.IsDeleted)
			{
				docAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupportingDocumentOrganizationAddress);
				RegisterEditableChildObject(docAddress);
			}
			return docAddress;
		}
	}
	JobDocAddress docAddress;
}
