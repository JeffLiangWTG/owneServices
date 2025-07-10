using System;
using System.Collections.Immutable;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.ES.NCTS.Business
{
	[SystemDefinedValues]
	public class NctsArrivalAndUnloadingCargoDesc : EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc
		, Integration.Customs.ES.IArrivalAndUnloadingCargoDesc
	{
		public NctsArrivalAndUnloadingCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ReadOnlyMember(nameof(LineNoIsReadOnly))]
		public override ZShort BY_LineNo
		{
			get => base.BY_LineNo;
			set => base.BY_LineNo = value;
		}
		ZBool LineNoIsReadOnly => MoveHeader.IsUnloadingMovementHeader && !IsNew;

		[ResourceStringData("114AD946-650A-4A93-BCB0-05AA7BE19A20", Caption = "[46] Statistical Value")]
		[DecimalPlaces(2)]
		public override ZDecimal BY_MonetaryValue
		{
			get => base.BY_MonetaryValue;
			set => base.BY_MonetaryValue = value;
		}

		[ResourceStringData("4EE56220-5958-402E-B45A-B9A8EC9556EB", Caption = "Bill of Lading/Item")]
		[MaxLength(21)]
		public ZString BillOfLadingItem
		{
			get => this.GetSystemDefinedValue<ZString>(GenAddOnHelper.BillOfLadingItem);
			set
			{
				value = value.ToUpper();
				var oldValue = BillOfLadingItem;
				CheckMaximumLength(BillOfLadingItemInfo, value);
				this.SetSystemDefinedValue(GenAddOnHelper.BillOfLadingItem, value);
				BillOfLadingItemInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BillOfLadingItemInfo => GetZPropertyInfo(nameof(BillOfLadingItem));

		public new EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;
		protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetNewNctsSupportingDocumentCollection() => new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
		protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

		public bool ShouldAddUnloadingGoodsToWrapper => HasDifferences || IsNew || IsMissing;

		public new NctsArrivalAndUnloadingCargoDescValidation Validation => (NctsArrivalAndUnloadingCargoDescValidation)base.Validation;

		protected override CusInBondCargoDescValidation GetNewValidation() => new NctsArrivalAndUnloadingCargoDescValidation(this);

		public override bool CanDelete => IsDeletableUnloadingGoodsItem() || base.CanDelete;

		ZBool IsDeletableUnloadingGoodsItem()
		{
			var arrivalStatusNotAllowingDeleteList = new ZString[] { EU.NCTS.Business.NctsTransitStatusList.Codes.GoodsUnderCustomsControl, EU.NCTS.Business.NctsTransitStatusList.Codes.GoodsWrittenOff }.ToImmutableArray();
			var result = false;
			var moveHeader = MoveHeader;
			var nctsHeader = MoveHeader?.Header;
			if (moveHeader != null && nctsHeader != null)
			{
				if (moveHeader.IsUnloadingMovementHeader && IsNew && !nctsHeader.EffectiveMessageStatus.Equals(EU.NCTS.Business.NctsMessageStatusList.Codes.ArrivalNotificationSent) && !arrivalStatusNotAllowingDeleteList.Contains(moveHeader.Header.ArrivalMovementHeader.BM_CustomsStatus))
				{
					result = true;
				}
			}
			return result;
		}
	}
}
