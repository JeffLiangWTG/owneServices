using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.H7.Business
{
	public class SupportingDocument : EU.H7.Business.SupportingDocument
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string CSI_Actions = "CSI_Actions";
			public const string CSI_Availability = "CSI_Availability";

			public const int CSI_ActionsMaxLength = 1;
			public const int CSI_AvailabilityMaxLength = 1;
		}

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new SupportingDocumentLookups(this);

		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new SupportingDocumentValidation(this);
		}

		[MaxLength(4)]
		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.CodeList))]
		[ResourceStringData("GBH7.SupportingDocuments.CSI_Code", Caption = "Type", MediumCaption = "Type", ShortCaption = "Type", FullDescription = "Supporting document type.")]
		public override ZString CSI_Code
		{
			get { return base.CSI_Code; }
			set
			{
				var originalCodeValue = base.CSI_Code;
				if (originalCodeValue != value)
				{
					base.CSI_Code = value;
					if (value == SupportingDocumentTypeBRDAuthorization && Parent is AsycudaBill bill)
					{
						var birdsPermitHeader = bill.Header.BIRDSPermitHeader;
						if (birdsPermitHeader != null)
						{
							CSI_ReferenceNumber = birdsPermitHeader.CPH_Number;
						}
					}
					else if (originalCodeValue == SupportingDocumentTypeBRDAuthorization && Parent is AsycudaBill)
					{
						CSI_ReferenceNumber = ZString.Empty;
					}
				}
			}
		}

		const string SupportingDocumentTypeBRDAuthorization = "1BRD";

		[ResourceStringData("GBH7.SupportingDocuments.CSI_SubType", Caption = "Part")]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}

		[ResourceStringData("GBH7.SupportingDocuments.CSI_ReferenceNumber2", Caption = "Issuing Authority", MediumCaption = "Issuing Auth.", ShortCaption = "Issuing Auth.")]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber2_ReadOnly))]
		public override ZString CSI_ReferenceNumber2
		{
			get { return base.CSI_ReferenceNumber2; }
			set
			{
				base.CSI_ReferenceNumber2 = value;
				CSI_ReferenceNumber2Info.RefreshBinding();
			}
		}

		protected virtual bool CSI_ReferenceNumber2_ReadOnly => !HasValidCSI_Code;

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.ActionList))]
		[MaxLength(Schema.CSI_ActionsMaxLength)]
		[ResourceStringData("GBH7.SupportingDocuments.CSI_Actions", Caption = "Actions")]
		public ZString CSI_Actions
		{
			get { return csi_Actions; }
			set
			{
				if (SetNonPersistentPropertyValue(CSI_ActionsInfo, ref csi_Actions, value))
				{
					PersistToCSI_Status(csi_Availability, value);
				}
			}
		}

		ZString csi_Actions;

		public ZPropertyInfo CSI_ActionsInfo => GetZPropertyInfo(Schema.CSI_Actions);

		[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.AvailabilityList))]
		[MaxLength(Schema.CSI_AvailabilityMaxLength)]
		[ResourceStringData("GBH7.SupportingDocuments.CSI_Availability", ShortCaption = "Avail.", Caption = "Availability")]
		public ZString CSI_Availability
		{
			get { return csi_Availability; }
			set
			{
				if (SetNonPersistentPropertyValue(CSI_AvailabilityInfo, ref csi_Availability, value))
				{
					PersistToCSI_Status(value, csi_Actions);
				}
			}
		}
		ZString csi_Availability;

		public ZPropertyInfo CSI_AvailabilityInfo => GetZPropertyInfo(Schema.CSI_Availability);

		[ResourceStringData("GBH7.SupportingDocuments.CSI_Description", Caption = "Reason")]
		public override ZString CSI_Description
		{
			get { return base.CSI_Description; }
			set { base.CSI_Description = value; }
		}

		[ResourceStringData("GBH7.SupportingDocuments.CSI_DateOfIssue", Caption = "Date of Validity")]
		[ReadOnlyMember(nameof(CSI_DateOfIssue_ReadOnly))]
		public override ZDateTime CSI_DateOfIssue
		{
			get { return base.CSI_DateOfIssue; }
			set { base.CSI_DateOfIssue = value; }
		}

		protected virtual bool CSI_DateOfIssue_ReadOnly => !HasValidCSI_Code;

		[ResourceStringData("GBH7.SupportingDocuments.CSI_DateOfExpiry", Caption = "Date of Expiry")]
		public override ZDateTime CSI_DateOfExpiry
		{
			get { return base.CSI_DateOfExpiry; }
			set { base.CSI_DateOfExpiry = value; }
		}

		public ZString ReferenceNumberFieldType => nameof(FieldType.Text);

		public ICanBeImportOrExport ImportExportParent => base.Parent as ICanBeImportOrExport;

		void PersistToCSI_Status(ZString availability, ZString actions)
		{
			CSI_Status = availability.Left(1).PadRight(1) + actions.Left(1).PadRight(1);
		}

		bool HasValidCSI_Code => !CSI_Code.IsEmpty && !CSI_CodeInfo.HasMessageErrors();

		public bool ReferenceNumberRequired
		{
			get
			{
				var referenceNumberValues = RefCusCode?.GetAttributesValues(GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReferenceNumber);
				return referenceNumberValues != null && referenceNumberValues.Contains(GBCommonConstants.RefCusCodeListAttributeValue.ReferenceNumber.ReferenceNumberRequired);
			}
		}

		public bool IsAvailabilityAndActionCombinationValid
		{
			get
			{
				var actavAttributes = RefCusCode?.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.ACTAV);
				return actavAttributes != null && actavAttributes.Contains(CSI_Availability + CSI_Actions);
			}
		}

		public ZZRefCusCodeListCombined RefCusCode => Factory.GetCachedValue(ImportExportParent.DataGroupingCode + ImportExportParent.Direction() + CSI_Code, () =>
		{
			if (!string.IsNullOrEmpty(ImportExportParent.Direction()) && !string.IsNullOrEmpty(ImportExportParent.DataGroupingCode))
			{
				return base.Factory.GetSupportingDocumentCode(ImportExportParent.DataGroupingCode, ImportExportParent.Direction(), CSI_Code);
			}
			return null;
		});
	}
}
