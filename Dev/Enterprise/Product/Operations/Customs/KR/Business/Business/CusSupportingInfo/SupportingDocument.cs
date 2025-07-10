using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class SupportingDocument : CusSupportingInfo
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int SUP_CodeMaxLength = 2;
			public const int SUP_ReferenceNumberMaxLength = 15;
			public const int ValuationDocumentMaxLength = 50;
		}

		[MaxLength(Schema.SUP_CodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.LocalExportDocumentTypeList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[MaxLength(Schema.SUP_ReferenceNumberMaxLength)]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}
		protected override CusSupportingInfoValidation GetNewValidation() => new SupportingDocumentValidation(this);
		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;
		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new SupportingDocumentLookups(this);
		}

		public new ISupportingDocumentParent Parent => (ISupportingDocumentParent)base.Parent;
	}
}
