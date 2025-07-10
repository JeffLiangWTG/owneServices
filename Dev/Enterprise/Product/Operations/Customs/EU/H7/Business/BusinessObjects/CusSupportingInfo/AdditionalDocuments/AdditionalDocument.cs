using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AdditionalDocument : BaseAdditionalInfo, IDataGroupingProvider
	{
		public AdditionalDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[MaxLength(3)]
		[ResourceStringData("EU.H7.AdditionalDocument.CSI_SubType", Caption = "Kind", MediumCaption = "Kind", ShortCaption = "Kind", FullDescription = "Additional document list type.")]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}

		[ReadOnlyMember(nameof(CSI_CodeReadOnly))]
		[MaxLength(5)]
		[ResourceStringData("EU.H7.AdditionalDocument.CSI_Code", Caption = "Full Type", MediumCaption = "Full Type", ShortCaption = "Type", FullDescription = "Code within relevant additional document list.")]
		[List(nameof(Lookups) + "." + nameof(AdditionalDocumentLookups.CodeList))]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		bool CSI_CodeReadOnly => !Lookups.SubTypeList.ContainsCode(CSI_SubType);

		[ReadOnlyMember(nameof(CSI_ReferenceNumberReadOnly))]
		[ResourceStringData("EU.H7.AdditionalDocument.CSI_ReferenceNumber", Caption = "Reference", MediumCaption = "Reference", ShortCaption = "Ref.", FullDescription = "Additional document reference number.")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		bool CSI_ReferenceNumberReadOnly => IsAnAdditionalInformation;

		[ReadOnlyMember(nameof(CSI_DescriptionReadOnly))]
		[ResourceStringData("EU.H7.AdditionalDocument.CSI_Description", Caption = "Description", MediumCaption = "Description", ShortCaption = "Desc.", FullDescription = "Additional document description.")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		bool CSI_DescriptionReadOnly => IsAnAdditionalReference || IsATransportDocument;

		public ZString DataGrouping
		{
			get
			{
				if (!dataGroupingCached.HasValue)
				{
					dataGroupingCached = Parent is IDataGroupingProvider provider ? provider.DataGrouping : ZString.Empty;
				}

				return dataGroupingCached.Value;
			}
		}
		ZString? dataGroupingCached;

		public override ZGuid CSI_ParentID
		{
			get => base.CSI_ParentID;
			set
			{
				var oldValue = CSI_ParentID;
				base.CSI_ParentID = value;
				if (!IsCopying && oldValue != CSI_ParentID)
				{
					dataGroupingCached = null;
				}
			}
		}

		public ValidationConfiguration ValidationConfiguration => validationConfiguration ??= GetNewValidationConfiguration();
		ValidationConfiguration validationConfiguration;

		protected virtual ValidationConfiguration GetNewValidationConfiguration() => new ValidationConfiguration();

		public new AdditionalDocumentLookups Lookups => (AdditionalDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new AdditionalDocumentLookups(this);

		public new AdditionalDocumentValidation Validation => (AdditionalDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new AdditionalDocumentValidation(this);
	}
}
