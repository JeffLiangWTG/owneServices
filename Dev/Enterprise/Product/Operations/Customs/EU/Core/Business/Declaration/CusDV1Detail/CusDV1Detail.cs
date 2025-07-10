using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(JobDeclaration), nameof(JobDeclaration.DV1Details))]
	public class CusDV1Detail : AutoCusDV1Detail
		, IShortSequenceNumberLine
		, IClusterKeyWorker
	{
		public CusDV1Detail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		[ReadOnly(true)]
		public ZShort Sequence { get; set; }

		[List(nameof(Lookups) + "." + nameof(CusDV1DetailLookups.YesNoList))]
		[ResourceStringData("AFACA9E2-B442-4961-9591-E3FB08DFDA5D", Caption = "Relationship")]
		public override ZString DV1_Relationship
		{
			get => base.DV1_Relationship;
			set
			{
				var oldValue = DV1_Relationship;
				base.DV1_Relationship = value;
				if (!IsCopying && oldValue != DV1_Relationship)
				{
					if (NotBuyerSellerRelationship)
					{
						DV1_PriceInfluence = ZString.Empty;
						DV1_RelationDetails = ZString.Empty;
						DV1_CloseApproximation = ZString.Empty;
					}
					else
					{
						DV1_PriceInfluence = YesNoList.Codes.No;
						DV1_CloseApproximation = YesNoList.Codes.No;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(NotBuyerSellerRelationship))]
		[List(nameof(Lookups) + "." + nameof(CusDV1DetailLookups.YesNoList))]
		[ResourceStringData("2347A5F3-C8FE-4160-A1B0-D326A1CEABDF", Caption = "Price influenced?")]
		public override ZString DV1_PriceInfluence { get => base.DV1_PriceInfluence; set => base.DV1_PriceInfluence = value; }

		public ZBool NotBuyerSellerRelationship => DV1_Relationship != YesNoList.Codes.Yes;

		[ReadOnlyMember(nameof(NotBuyerSellerRelationship))]
		[ResourceStringData("90CC9DD9-4375-46AC-9C60-FFD843E71F51", Caption = "Details")]
		public override ZString DV1_RelationDetails { get => base.DV1_RelationDetails; set => base.DV1_RelationDetails = value; }

		[List(nameof(Lookups) + "." + nameof(CusDV1DetailLookups.YesNoList))]
		[ResourceStringData("559B2D2A-097A-4492-B3F7-5E36464DE740", Caption = "Restrictions")]
		public override ZString DV1_Restrictions
		{
			get => base.DV1_Restrictions;
			set
			{
				var oldValue = DV1_Restrictions;
				base.DV1_Restrictions = value;
				if (!IsCopying && oldValue != DV1_Restrictions)
				{
					if (DV1_RestrictionConsiderationDetailsReadOnly)
					{
						DV1_RestrictionConsiderationDetails = ZString.Empty;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusDV1DetailLookups.YesNoList))]
		[ResourceStringData("6CE72FC5-C31D-4A74-B360-581B453184FA", Caption = "Conditions")]
		public override ZString DV1_Consideration
		{
			get => base.DV1_Consideration;
			set
			{
				var oldValue = DV1_Consideration;
				base.DV1_Consideration = value;
				if (!IsCopying && oldValue != DV1_Consideration)
				{
					if (DV1_RestrictionConsiderationDetailsReadOnly)
					{
						DV1_RestrictionConsiderationDetails = ZString.Empty;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(DV1_RestrictionConsiderationDetailsReadOnly))]
		[ResourceStringData("28A7EBD4-CD57-42E8-9337-60E14BE96A4D", Caption = "Details")]
		public override ZString DV1_RestrictionConsiderationDetails { get => base.DV1_RestrictionConsiderationDetails; set => base.DV1_RestrictionConsiderationDetails = value; }

		public ZBool DV1_RestrictionConsiderationDetailsReadOnly => DV1_Restrictions != YesNoList.Codes.Yes && DV1_Consideration != YesNoList.Codes.Yes;

		[List(nameof(Lookups) + "." + nameof(CusDV1DetailLookups.YesNoList))]
		[ResourceStringData("9B241558-BAF9-4D64-8866-D91AAEF0824E", Caption = "License Fees")]
		public override ZString DV1_RoyaltiesLicence
		{
			get => base.DV1_RoyaltiesLicence;
			set
			{
				var oldValue = DV1_RoyaltiesLicence;
				base.DV1_RoyaltiesLicence = value;
				if (!IsCopying && oldValue != DV1_RoyaltiesLicence)
				{
					if (DV1_RoyaltiesLicenceDetailsReadOnly)
					{
						DV1_RoyaltiesLicenceDetails = ZString.Empty;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(DV1_RoyaltiesLicenceDetailsReadOnly))]
		[ResourceStringData("875A8E9D-A079-4E01-B198-BB068E0205C7", Caption = "Details")]
		public override ZString DV1_RoyaltiesLicenceDetails { get => base.DV1_RoyaltiesLicenceDetails; set => base.DV1_RoyaltiesLicenceDetails = value; }

		public ZBool DV1_RoyaltiesLicenceDetailsReadOnly => DV1_RoyaltiesLicence != YesNoList.Codes.Yes;

		[List(nameof(Lookups) + "." + nameof(CusDV1DetailLookups.YesNoList))]
		[ResourceStringData("5D061CCD-C8B8-4209-8266-290842074435", Caption = "Resale")]
		public override ZString DV1_Resale
		{
			get => base.DV1_Resale;
			set
			{
				var oldValue = DV1_Resale;
				base.DV1_Resale = value;
				if (!IsCopying && oldValue != DV1_Resale)
				{
					if (DV1_ResaleDetailsReadOnly)
					{
						DV1_ResaleDetails = ZString.Empty;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(DV1_ResaleDetailsReadOnly))]
		[ResourceStringData("90BB41A3-9D3D-4242-A8EB-D2F16094028D", Caption = "Details")]
		public override ZString DV1_ResaleDetails { get => base.DV1_ResaleDetails; set => base.DV1_ResaleDetails = value; }

		public ZBool DV1_ResaleDetailsReadOnly => DV1_Resale != YesNoList.Codes.Yes;

		[MaxLength(100)]
		[ResourceStringData("7759D877-404C-4625-84C8-D5F1E573D463", Caption = "Former Decisions")]
		public override ZString DV1_CustomsDecisionNumber { get => base.DV1_CustomsDecisionNumber; set => base.DV1_CustomsDecisionNumber = value; }

		[ReadOnlyMember(nameof(NotBuyerSellerRelationship))]
		[List(nameof(Lookups) + "." + nameof(CusDV1DetailLookups.YesNoList))]
		[ResourceStringData("425D8631-F96C-4E92-AD8B-CCA8952077BE", Caption = "Consistent Price?")]
		public override ZString DV1_CloseApproximation { get => base.DV1_CloseApproximation; set => base.DV1_CloseApproximation = value; }

		[ResourceStringData("EDE21D5B-7889-4DAF-B760-3FAEBFA3C964", Caption = "Contract Number")]
		public override ZString DV1_ContractNumber { get => base.DV1_ContractNumber; set => base.DV1_ContractNumber = value; }

		[ResourceStringData("7563E107-B620-4F8C-80BE-8E40E9F7874E", Caption = "Contract Date")]
		public override ZDate DV1_ContractDate { get => base.DV1_ContractDate; set => base.DV1_ContractDate = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DV1_Relationship = YesNoList.Codes.No;
			DV1_Restrictions = YesNoList.Codes.No;
			DV1_Consideration = YesNoList.Codes.No;
			DV1_RoyaltiesLicence = YesNoList.Codes.No;
			DV1_Resale = YesNoList.Codes.No;
		}

		protected override CusDV1DetailValidation GetNewValidation() => new CusDV1DetailValidation(this);

		protected override bool SupportsCloneCore() => true;

		#region IShortSequenceNumberLine

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => Sequence; set => Sequence = value; }
		ZGuid ISequenceNumberLine.FKToHeader => DV1_JE;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)DV1_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(JobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)DV1_JEInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
