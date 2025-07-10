using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSDocument : CusSupportingInfo
	{
		public EMCSDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public EMCSJobDeclaration Declaration => (EMCSJobDeclaration)Parent;

		public new EMCSDocumentLookups Lookups => (EMCSDocumentLookups)base.Lookups;

		[ResourceStringData("{410DBBF9-5314-422B-8715-43BACE0893A5}", Caption = "Description", ShortCaption = "Desc.")]
		[ReadOnlyMember(nameof(IsLinkingToReadOnlyEMCSParent))]
		public override ZString CSI_Description
		{
			get { return base.CSI_Description; }
			set
			{
				if (base.CSI_Description != value)
				{
					base.CSI_Description = value;
				}
			}
		}

		[MaxLength(nameof(CSI_ReferenceNumberMaxLength))]
		[ResourceStringData("{E79022D2-EFBE-4B6F-8CF4-BE356F50C4A1}", Caption = "Reference", ShortCaption = "Ref.")]
		[ReadOnlyMember(nameof(IsLinkingToReadOnlyEMCSParent))]
		public override ZString CSI_ReferenceNumber
		{
			get { return base.CSI_ReferenceNumber; }
			set
			{
				if (base.CSI_ReferenceNumber != value)
				{
					base.CSI_ReferenceNumber = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(EMCSDocumentLookups.DocumentTypesList))]
		[ResourceStringData("C7762CF7-8AFF-4123-BBBE-6FB4092CD879", Caption = "Document Type", ShortCaption = "Doc. Type")]
		[ReadOnlyMember(nameof(IsLinkingToReadOnlyEMCSParent))]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}
		public ZBool IsLinkingToReadOnlyEMCSParent => Factory.GetValue(ref isMessageStatusSentOrAcknowledgedCached, () => CSI_ParentID.IsValid && Declaration.IsMessageStatusSentOrAcknowledged);
		CachedProperty<ZBool> isMessageStatusSentOrAcknowledgedCached;

		public override bool CanDelete => base.CanDelete && !IsLinkingToReadOnlyEMCSParent;

		internal ZString KeyToDetermineUniqueness => CSI_ReferenceNumber + CSI_Description;

		protected override ZString HumanReadableNameCore => Res.GetString("ecb87bc7-a531-4cf7-8cf4-999169cb32e9", "Document");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.Certificate;
			CSI_Code = CusSupportingInfoTypeList.Codes.Certificate;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new EMCSDocumentValidation(this);

		protected override CusSupportingInfoLookups GetNewLookups() => new EMCSDocumentLookups(this);

		int CSI_ReferenceNumberMaxLength => CSI_SubType.IsEmpty ? 100 : 35;
	}
}
