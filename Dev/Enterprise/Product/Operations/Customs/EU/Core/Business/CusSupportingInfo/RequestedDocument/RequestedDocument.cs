using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.EU.Business
{
	public class RequestedDocument : CusSupportingInfo
	{
		public RequestedDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new RequestedDocumentLookups Lookups => (RequestedDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new RequestedDocumentLookups(this);

		public new RequestedDocumentValidation Validation => (RequestedDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new RequestedDocumentValidation(this);

		[ResourceStringData("Enterprise.Customs.EU.Business.RequiredDocument|CSI_Code", Caption = "Type", MediumCaption = "Type", ShortCaption = "Type", FullDescription = "Requested document type.")]
		[MaxLength(4)]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.RequiredDocument|RequestInformation", Caption = "Requested Information", MediumCaption = "Requested Info.", ShortCaption = "Req. Info.", FullDescription = "Requested information in relation to the requested document type.")]
		[MaxLength(512)]
		public ZString RequestInformation
		{
			get
			{
				return CSI_Description + CSI_AdditionalDescription;
			}
			set
			{
				if (value.Length > Schema.CSI_DescriptionMaxLength)
				{
					CSI_Description = value.Substring(0, Schema.CSI_DescriptionMaxLength);
					CSI_AdditionalDescription = value.Substring(Schema.CSI_DescriptionMaxLength);
				}
				else
				{
					CSI_Description = value;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.Business.RequiredDocument|CSI_DateOfIssue", Caption = "Date of Request", MediumCaption = "Date of Request", ShortCaption = "DOR", FullDescription = "Date of document request.")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.RequiredDocument|CSI_DateOfExpiry", Caption = "Provide By Date", MediumCaption = "Prov. By Date", ShortCaption = "PBD", FullDescription = "Date by which the requested document must be provided.")]
		public override ZDateTime CSI_DateOfExpiry { get => base.CSI_DateOfExpiry; set => base.CSI_DateOfExpiry = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.RequiredDocument|CSI_Status", Caption = "Status", MediumCaption = "Status", ShortCaption = "Status", FullDescription = "Requested document status.")]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.RequiredDocument|StatusDescription", Caption = "Status Description", MediumCaption = "Status Desc.", ShortCaption = "Desc.", FullDescription = "Requested document status description.")]
		public ZString StatusDescription => Lookups.StatusList.GetDescriptionFromCode(CSI_Status);

		[ResourceStringData("Enterprise.Customs.EU.Business.RequiredDocument|CSI_ReferenceNumber", Caption = "Reference Number", MediumCaption = "Reference No.", ShortCaption = "Ref. No.", FullDescription = "Requested document reference number.")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		public bool IsOpen => CSI_Status == RequestedDocumentStatusList.Codes.RequestOpened;
	}
}
