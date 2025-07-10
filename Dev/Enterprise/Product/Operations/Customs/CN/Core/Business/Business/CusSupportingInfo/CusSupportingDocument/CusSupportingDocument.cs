using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CusSupportingDocument : CusSupportingInfo
	{
		public CusSupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string DocumentType = "DocumentType";
		}

		#region Basic Overrides/TypeSafe

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		public new CusSupportingDocumentLookups Lookups => (CusSupportingDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups()
		{
			return new CusSupportingDocumentLookups(this);
		}

		public new CusSupportingDocumentValidation Validation => (CusSupportingDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new CusSupportingDocumentValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.CSI_Type = Constants.CusSupportingInfoTypes.CusSupportingDocument;
		}

		public override bool SupportsNotes => false;

		#endregion

		#region Proxied Properties

		public ZDateTime EffectiveDate => Parent?.EffectiveAssessmentDate ?? ZDateTime.Today;

		#endregion

		#region RelatedBO: RefCusCode

		internal ZZRefCusCodeListCombined RefCusCode => CNRefCusCodeListLoader.GetRequiredDocuments(Factory, CSI_Code, EffectiveDate);

		#endregion

		[ResourceStringData("Enterprise.Customs.CN.Business.CusSupportingDocument.CSI_Code", Caption = "Document Type")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != CSI_Code)
				{
					if (CSI_LineNoReadonly)
					{
						CSI_LineNo = ZShort.Zero;
					}

					if (!value.IsEmpty && CSI_ReferenceNumber.IsEmpty)
					{
						var existingDocument = Parent?.EntryInstruction?.CusSupportingDocuments.FirstOrDefault(x => x.CSI_Code == value && !x.CSI_ReferenceNumber.IsEmpty);
						if (existingDocument != null)
						{
							CSI_ReferenceNumber = existingDocument.CSI_ReferenceNumber;
						}
					}
				}
			}
		}

		#region new Property: Document Type

		public ZString DisplayCode => RefCusCode?.GetAttribute(Constants.UniversalReferenceConstants.CusCodeListAttributeName.DisplayCode) ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.CN.Business.CusSupportingDocument.DocumentType", Caption = "Document Type")]
		[List(nameof(Lookups) + "." + nameof(CusSupportingDocumentLookups.SupportingDocumentsList))]
		public ZString DocumentType
		{
			get => Lookups.SupportingDocumentsList.GetDescriptionFromCode(CSI_Code);
			set
			{
				CSI_Code = Lookups.SupportingDocumentsList.GetCodeFromDescription(value);
				if (!IsCertificateOfOrigin)
				{
					CSI_SubType = ZString.Empty;
				}
				CSI_CodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DocumentTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DocumentType, x => CSI_CodeInfo); }
		}

		#endregion

		[MaxLength("CSI_ReferenceNumberMaxLength")]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusSupportingDocument.CSI_ReferenceNumber", Caption = "Document Number")]
		public override ZString CSI_ReferenceNumber
		{
			get
			{
				return IsCertificateOfOriginX ? (ZString)CertificateOfOriginForTypeX : base.CSI_ReferenceNumber;
			}
			set => base.CSI_ReferenceNumber = value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Validators used via reflection")]
		int CSI_ReferenceNumberMaxLength => IsLicense ? 20 : 32;

		[ReadOnlyMember(nameof(CSI_LineNoReadonly))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusSupportingDocument.CSI_LineNo", Caption = "Item No on Document")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		ZBool CSI_LineNoReadonly => !LineNumberRequiredOptions.Contains(RequiresLineNumber);

		internal ZString RequiresLineNumber => RefCusCode?.GetAttribute(Constants.UniversalReferenceConstants.CusCodeListAttributeName.RequiresLineNumber) ?? ZString.Empty;

		static List<string> LineNumberRequiredOptions =>
			new List<string>
			{
				Constants.UniversalReferenceConstants.RequiresLineNumberAttributeValue.Mandatory,
				Constants.UniversalReferenceConstants.RequiresLineNumberAttributeValue.Optional
			};

		public bool IsLicense => RefCusCode != null && RefCusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.IsLicense);

		public bool IsImport => RefCusCode != null && RefCusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.Import);

		public bool IsExport => RefCusCode != null && RefCusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.Export);

		public bool IsCertificateOfOrigin => CSI_Code == Constants.DocumentCodes.CertificateOfOrigin;

		internal bool IsCertificateOfOriginX => IsCertificateOfOrigin && CertificateOfOriginTypeList.IsSmallAmountGoods(CSI_SubType);

		internal const string CertificateOfOriginForTypeX = "XJE00000";

		public bool IsTheSameDocument(CusSupportingDocument itemToCompare)
		{
			return !CSI_ReferenceNumber.IsEmpty && itemToCompare.CSI_Code == CSI_Code && itemToCompare.CSI_ReferenceNumber == CSI_ReferenceNumber;
		}

		public bool IsDifferentDocumentWithTheSameType(CusSupportingDocument itemToCompare)
		{
			return !CSI_ReferenceNumber.IsEmpty && itemToCompare.CSI_Code == CSI_Code && (itemToCompare.CSI_ReferenceNumber != CSI_ReferenceNumber || itemToCompare.CSI_SubType != CSI_SubType);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusSupportingDocument cusSupportingDocument) : base(cusSupportingDocument)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();

				var cusSupportingDocument = BusinessObject as CusSupportingDocument;
				Factory.AddRefCusCodeListFetchHintIfNotEmpty(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, cusSupportingDocument.CSI_Code, cusSupportingDocument.EffectiveDate);
			}
		}
	}
}
