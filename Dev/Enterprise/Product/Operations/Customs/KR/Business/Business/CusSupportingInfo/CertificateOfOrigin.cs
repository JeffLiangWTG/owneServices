using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class CertificateOfOrigin : CusSupportingInfo
	{
		public CertificateOfOrigin(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int COO_CodeMaxLength = 1;
			public const int COO_ReferenceNumberMaxLength = 60;
			public const int COO_ReferenceNumber2MaxLength = 60;
			public const int COO_ProcedureMaxLength = 2;
			public const int COO_DescriptionMaxLength = 100;
			public const int COO_AdditionalDescriptionMaxLength = 30;
			public const int COO_StatusMaxLength = 1;
			public const int COO_SubTypeMaxLength = 1;
			public const int COO_IssuerTypeMaxLength = 1;
		}

		[MaxLength(Schema.COO_CodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CertificateOfOriginLookups.CertificateOfOriginIssuedCodeList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[MaxLength(Schema.COO_ReferenceNumberMaxLength)]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[MaxLength(Schema.COO_ReferenceNumber2MaxLength)]
		public override ZString CSI_ReferenceNumber2
		{
			get => base.CSI_ReferenceNumber2;
			set => base.CSI_ReferenceNumber2 = value;
		}

		[MaxLength(Schema.COO_ProcedureMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CertificateOfOriginLookups.CountryOfOriginDeterminationRuleCodeList))]
		public override ZString CSI_Procedure
		{
			get => base.CSI_Procedure;
			set => base.CSI_Procedure = value;
		}

		[MaxLength(Schema.COO_DescriptionMaxLength)]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		[MaxLength(Schema.COO_AdditionalDescriptionMaxLength)]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}

		[MaxLength(Schema.COO_StatusMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CertificateOfOriginLookups.CertificateOfOriginSplitCodeList))]
		public override ZString CSI_Status
		{
			get => base.CSI_Status;
			set => base.CSI_Status = value;
		}

		[MaxLength(Schema.COO_SubTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CertificateOfOriginLookups.CountryOfOriginDeterminationRuleCodeList))]
		[ResourceStringData("{6B21D111-A253-4C40-85CD-C5DDDF21D55A}", Caption = "C/O Determination Rule")]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}

		[MaxLength(Schema.COO_IssuerTypeMaxLength)]
		public override ZString CSI_IssuerType
		{
			get => base.CSI_IssuerType;
			set => base.CSI_IssuerType = value;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new CertificateOfOriginValidation(this);
		public new CertificateOfOriginValidation Validation => (CertificateOfOriginValidation)base.Validation;

		protected override CusSupportingInfoLookups GetNewLookups() => new CertificateOfOriginLookups(this);
		public new CertificateOfOriginLookups Lookups => (CertificateOfOriginLookups)base.Lookups;

		public new ISupportingDocumentParent Parent => (ISupportingDocumentParent)base.Parent;
	}
}
