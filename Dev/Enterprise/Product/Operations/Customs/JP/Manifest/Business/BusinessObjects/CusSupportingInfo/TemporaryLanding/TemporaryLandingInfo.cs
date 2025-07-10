using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class TemporaryLandingInfo : CusSupportingInfo
	{
		public TemporaryLandingInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override ZDateTime CSI_DateOfExpiry
		{
			get => base.CSI_DateOfExpiry;
			set
			{
				base.CSI_DateOfExpiry = value;
				Parent?.MarkAsNeedingValidation();
			}
		}

		public override ZDateTime CSI_DateOfIssue
		{
			get => base.CSI_DateOfIssue;
			set
			{
				base.CSI_DateOfIssue = value;
				Parent?.MarkAsNeedingValidation();
			}
		}

		public override ZString CSI_ParentTableCode
		{
			get => base.CSI_ParentTableCode;
			set
			{
				base.CSI_ParentTableCode = value;
				Parent?.MarkAsNeedingValidation();
			}
		}

		[List(nameof(Lookups) + "." + nameof(TemporaryLandingInfoLookups.BondedTransportList))]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set
			{
				base.CSI_ReferenceNumber = value;
				Parent?.MarkAsNeedingValidation();
			}
		}

		public override ZString CSI_Type
		{
			get => base.CSI_Type;
			set
			{
				base.CSI_Type = value;
				Parent?.MarkAsNeedingValidation();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.ApprovalCertificate;
			CSI_ParentTableCode = AsycudaBillSchema.Constants.Prefix;
		}

		internal static CodeDescriptionPairList GetBondedTransportList(BusinessObjectFactory factory) => factory.GetCachedValue<BondedTransportCodeList>();

		internal static CodeDescriptionPairList GetReasonList(BusinessObjectFactory factory) => factory.GetCachedValue<TemporaryLandingReasonList>();

		public new TemporaryLandingInfoValidation Validation => (TemporaryLandingInfoValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new TemporaryLandingInfoValidation(this);

		public new TemporaryLandingInfoLookups Lookups => (TemporaryLandingInfoLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new TemporaryLandingInfoLookups(this);

		#region Delete empty row
		IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_CodeInfo;
			yield return CSI_ItemNumberInfo;
			yield return CSI_DateOfIssueInfo;
			yield return CSI_DateOfExpiryInfo;
			yield return CSI_ReferenceNumberInfo;
		}

		bool IsEmpty => !GetUsedFieldsInfos().Any(x => !x.Value.IsEmpty);

		public override bool IsSavedByFactory => IsInDatabase ? base.IsSavedByFactory : !(IsDeleted || IsEmpty);

		public override void OnSaving()
		{
			if (!IsDeleted && IsInDatabase && IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}
		#endregion
	}
}
