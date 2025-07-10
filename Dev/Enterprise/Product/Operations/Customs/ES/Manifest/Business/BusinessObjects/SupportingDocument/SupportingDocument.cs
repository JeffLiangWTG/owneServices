using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class SupportingDocument : CusSupportingInfo
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int ReferenceNumberMaxLength = 35;
		}

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.TypeCodeList))]
		[ResourceStringData("A0535339-48EF-436E-BE05-2B95E75C97D8", Caption = "Type", MediumCaption = "Type", ShortCaption = "Type")]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[MaxLength(Schema.ReferenceNumberMaxLength)]
		[ResourceStringData("9F2C77EB-8546-41DC-9236-4577E4BCAC62", Caption = "Reference", MediumCaption = "Reference", ShortCaption = "Ref.")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);
		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;
		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);
	}
}
