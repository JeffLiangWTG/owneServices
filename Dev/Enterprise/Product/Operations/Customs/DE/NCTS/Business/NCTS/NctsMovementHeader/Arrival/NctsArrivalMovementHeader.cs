using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsArrivalMovementHeader : EU.NCTS.Business.NctsArrivalMovementHeader
		, Integration.Customs.DE.IArrivalMovementHeader
	{
		public NctsArrivalMovementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new NctsArrivalMovementHeaderLookups Lookups => (NctsArrivalMovementHeaderLookups)base.Lookups;

		public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

		public new NctsHeader Header => (NctsHeader)base.Header;

		public new EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> ArrivalTransportInfos => (EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>)base.ArrivalTransportInfos;

		public override ZGuid AuthorizationOwner
		{
			get => base.AuthorizationOwner;
			set
			{
				var oldValue = AuthorizationOwner;
				base.AuthorizationOwner = value;
				var newValue = AuthorizationOwner;
				if (oldValue != newValue)
				{
					GoodsLocation.AddressIdentificationHolderPK = newValue;

					if (GoodsLocation.AddressAuthorisationNumber != AuthorizationNumber)
					{
						GoodsLocation.AddressAuthorisationNumber = AuthorizationNumber;
					}

					Header.UpdateGoodsLocationAdditionalIdentifier(GoodsLocation, GoodsLocationDescriptionInfo);

					GoodsLocationDescriptionInfo.RefreshBinding();
				}
			}
		}

		public override ZString AuthorizationNumber
		{
			get => base.AuthorizationNumber;
			set
			{
				var oldValue = AuthorizationNumber;
				base.AuthorizationNumber = value;
				var newValue = AuthorizationNumber;
				if (oldValue != newValue)
				{
					if (GoodsLocation.AddressIdentificationHolderPK != AuthorizationOwner)
					{
						GoodsLocation.AddressIdentificationHolderPK = AuthorizationOwner;
					}

					GoodsLocation.AddressAuthorisationNumber = newValue;

					GoodsLocationDescriptionInfo.RefreshBinding();
				}
			}
		}

		protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments() => new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetAdditionalDocuments() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

		protected override CusInBondMoveHeaderLookups GetNewLookups() => new NctsArrivalMovementHeaderLookups(this);

		protected override EU.NCTS.Business.IArrivalCusTransportMeansCollection<EU.NCTS.Business.ArrivalCusTransportMeans> GetNewArrivalTransportInfos() => new EU.NCTS.Business.ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>(this);

		protected override void SetDefaultValuesAfterNctsHeaderIsSet(EU.NCTS.Business.NctsHeader header)
		{
			AuthorizationCode = EU.NCTS.Business.NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			AuthorizationOwner = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			if (AuthorizationNumber.IsEmpty)
			{
				header.CusAuthorizationUsages.RemoveAndDeleteAll();
			}
			IsSimplifiedNctsProcedure = true;
			BM_ArrivalDate = ZDateTime.Now;
		}

		protected override void SetReadOnlyForUnloadingDifferencesDataCore(bool readOnly)
		{
			base.SetReadOnlyForUnloadingDifferencesDataCore(readOnly);
			if (Header is NctsHeader header)
			{
				foreach (var bill in header.Bills)
				{
					bill.AdditionalDocuments.SetReadOnlyIncludingChildren(true);
					bill.PreviousDocuments.SetReadOnlyIncludingChildren(true);
					foreach (var goodsItems in bill.ArrivalGoodsItems)
					{
						goodsItems.AdditionalInfos.SetReadOnlyIncludingChildren(true);
						goodsItems.PreviousDocuments.SetReadOnlyIncludingChildren(true);
					}
				}
				header.Bills.RefreshBinding();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BM_NoChangesToReport = true;
			BM_StateOfSealsBoolean = true;
			BM_UnloadingCompleted = true;
		}

		[ReadOnly(true)]
		public override ZBool BM_UnloadingCompleted { get => base.BM_UnloadingCompleted; set => base.BM_UnloadingCompleted = value; }

		public override bool AreUnloadingRemarksFullyAccepted => BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease
			&& BM_Phase == EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;

		public override bool IsUnloadingRemarksReadOnly => AreUnloadingRemarksFullyAccepted;

		protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
		protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

		public override ZString DestinationCustomsOfficeCodeForArrival
		{
			get => base.DestinationCustomsOfficeCodeForArrival;
			set
			{
				var oldValue = DestinationCustomsOfficeCodeForArrival;
				base.DestinationCustomsOfficeCodeForArrival = value;
				DestinationCustomsOfficeCodeForArrivalInfo.RefreshBinding(oldValue);
				if (oldValue != DestinationCustomsOfficeCodeForArrival)
				{
					Header.UpdateGoodsLocationAdditionalIdentifier(GoodsLocation, GoodsLocationDescriptionInfo);
				}
			}
		}

		protected override EU.NCTS.Business.INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesForArrivalCore() => new EU.NCTS.Business.NctsGuaranteeCollection<Guarantee>(this);

		#region FetchHints

		public override void AddCusGoodsLocationAdditionalIdentifierDescriptionFetchHintsForViewCore()
		{
			Factory.AddFetchHint(CusAuthorizationUsageSchema.AGC_ParentID, Header.PK);

			var query = new ZQuery(CusCodeDataSchema.CY_ParentID, PK)
					.AddToFilter(CusCodeDataSchema.CY_Type, EU.Business.CusCodeDataTypeList.Codes.OfficeCode);

			Factory.AddFetchHint(CusCodeDataSchema.Instance, query);
		}

		#endregion
	}
}
