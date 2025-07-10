using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class SupportingDocument : CusSupportingInfo
	{
		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int CodeMaxLength = 3;
		}

		public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);

		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

		bool CSI_TypeReadonly => true;

		[ReadOnlyMember(nameof(CSI_TypeReadonly))]
		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|CSI_Type", Caption = "Type")]
		public override ZString CSI_Type
		{
			get => base.CSI_Type;
			set => base.CSI_Type = value;
		}

		[MaxLength(Schema.CodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.SupportingDocumentCodeList))]
		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|CSI_Code", Caption = "Code")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|CSI_ReferenceNumber", Caption = "Reference")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[ResourceStringData("Enterprise.Customs.AsycudaCustoms.Business|CSI_AdditionalDescription", Caption = "Comments")]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}
	}
}
