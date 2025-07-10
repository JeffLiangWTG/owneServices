using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[CodeProperty("CodeProperty"), DescriptionProperty("CodeProperty")]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[SystemDefinedValues]
	public class AsycudaPack : ManifestBase.AsycudaPack
		, ISynchroniserReadOnlyMembersProvider
		, IUNDGDataItemProvider
		, Integration.Customs.ASYCUDA.IAsycudaPack
		, ISelectionItem
		, IShortSequenceNumberLine
		, ISailingSynchronisationTarget<BillOfLadingPackLine>
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ManifestBase.AsycudaPack.Schema
		{
			public const string ConsignmentReference = "ConsignmentReference";
			public const string MatchingReference = "MatchingReference";
			public const string LinePrice = "LinePrice";
			public const string LinePriceCurrency = "LinePriceCurrency";

			public const int LinePriceCurrencyMaxLength = 3;
			public const int MatchingReferenceMaxLength = GenAddOnColumn.Schema.XA_DataMaxLength;
		}

		public new static readonly AsycudaPackTypeDecider TypeDecider = new AsycudaPackTypeDecider();

		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;
		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		public new AsycudaPackLookups Lookups => (AsycudaPackLookups)base.Lookups;
		protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackLookups(this);

		public new AsycudaPackedItem GetPackedItemFromCollection() => (AsycudaPackedItem)base.GetPackedItemFromCollection();
		public new AsycudaPackedItem PackedItem => (AsycudaPackedItem)base.PackedItem;

		public new AsycudaPackPackedItemPivotCollection PackedItems => (AsycudaPackPackedItemPivotCollection)base.PackedItems;
		protected override ManifestBase.AsycudaPackPackedItemPivotCollection CreateNewAsycudaPackCollection() => new AsycudaPackPackedItemPivotCollection(this);

		[ChildEditable]
		public IAsycudaPackedItemCollection<AsycudaPackedItem, AsycudaPack> PackedItemsForBinding
		{
			get
			{
				if (packedItemsForBinding == null)
				{
					packedItemsForBinding = CreateNewAsycudaPackedItemCollection();
					packedItemsForBinding.Load();
					RegisterEditableChildObject(packedItemsForBinding);
				}
				return packedItemsForBinding;
			}
		}
		IAsycudaPackedItemCollection<AsycudaPackedItem, AsycudaPack> packedItemsForBinding;

		protected virtual IAsycudaPackedItemCollection<AsycudaPackedItem, AsycudaPack> CreateNewAsycudaPackedItemCollection() => new AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaPack>(this);

		public ZString CodeProperty => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1}", APA_PackQty, APA_GoodsDescription);

		public bool HasManifestBeenSubmittedToCustomsIncludingChildren =>
			Factory.GetValue(ref hasManifestBeenSubmittedToCustomsIncludingChildrenCached, () => IsOnePackedItemRelationship && (GetPackedItemFromCollection()?.HasManifestBeenSubmittedToCustoms ?? false));

		CachedProperty<bool> hasManifestBeenSubmittedToCustomsIncludingChildrenCached;

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public bool IsPackUQSynchroniserReadonly => IsPackUQSynchroniserReadonlyCore;

		protected virtual bool IsPackUQSynchroniserReadonlyCore => true;

		public bool IsPackUQNeedToConvert => IsPackUQNeedToConvertCore;

		protected virtual bool IsPackUQNeedToConvertCore => true;

		[RelatedBusinessObject("Bill")]
		public override ZGuid APA_ABL_Bill
		{
			get => base.APA_ABL_Bill;
			set
			{
				var oldValue = base.APA_ABL_Bill;
				base.APA_ABL_Bill = value;
				if (!IsCopying && oldValue != APA_ABL_Bill)
				{
					if (!APA_ABL_Bill.IsValid)
					{
						DetachedFromBill(oldValue);
					}
					else
					{
						AttachedToBill();
					}
				}
			}
		}

		void DetachedFromBill(ZGuid oldValue)
		{
			var bill = Factory.Load<AsycudaBill>(oldValue);
			if (bill != null)
			{
				bill.Packs.SequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				DetachFromCustomsNumbers(bill);
			}
		}

		void AttachedToBill()
		{
			var bill = Bill;
			if (bill != null)
			{
				bill.Packs.SequenceGenerator.RecalculateWhenAdded(this);
				AttachToCustomsNumbers(bill);
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			AssignConsignmentReferenceNumber();
		}

		#region LinePrice

		[ResourceStringData("AsycudaPack.LinePrice", Caption = "Line Price", ShortCaption = "Price")]
		public virtual ZDecimal LinePrice
		{
			get { return this.GetSystemDefinedValue<ZDecimal>(Schema.LinePrice); }
			set
			{
				var oldValue = LinePrice;
				this.SetSystemDefinedValue(Schema.LinePrice, value);
				if (!IsCopying && oldValue != value)
				{
					Bill?.MarkApportionmentDirty();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateLinePrice();
				}

				LinePriceInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo LinePriceInfo
		{
			get { return GetZPropertyInfo(Schema.LinePrice); }
		}

		[ResourceStringData("AsycudaPack.LinePriceCurrency", Caption = "Line Price Currency")]
		[ReadOnlyMember(nameof(LinePriceCurrency_ReadOnly))]
		[MaxLength(Schema.LinePriceCurrencyMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackLookups.LinePriceCurrencies))]
		public virtual ZString LinePriceCurrency
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.LinePriceCurrency); }
			set
			{
				var oldValue = LinePriceCurrency;
				CheckMaximumLength(LinePriceCurrencyInfo, value);
				this.SetSystemDefinedValue(Schema.LinePriceCurrency, value);
				if (!IsCopying && oldValue != value)
				{
					Bill?.MarkApportionmentDirty();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateLinePriceCurrency();
				}

				LinePriceCurrencyInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo LinePriceCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.LinePriceCurrency); }
		}

		protected virtual bool LinePriceCurrency_ReadOnly => false;

		public RefCurrency RefLinePriceCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, LinePriceCurrency); }
		}

		#endregion

		#region APA_LineNo
		public override ZShort APA_LineNo
		{
			get { return base.APA_LineNo; }
			set
			{
				var oldValue = APA_LineNo;
				base.APA_LineNo = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					Bill?.Packs.SequenceGenerator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}
		#endregion

		#region IShortSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => APA_ABL_Bill;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get { return APA_LineNo; }
			set { APA_LineNo = value; }
		}

		#endregion

		[ReadOnly(true)]
		[ResourceStringData("AsycudaPack.ConsignmentReference", Caption = "Consignment Reference")]
		public ZInt ConsignmentReference
		{
			get { return this.GetSystemDefinedValue<ZInt>(Schema.ConsignmentReference); }
			set
			{
				var oldValue = ConsignmentReference;
				this.SetSystemDefinedValue(Schema.ConsignmentReference, value);
				Bill.Header.ConsignmentReferenceTracker = value;
				ConsignmentReferenceInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ConsignmentReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.ConsignmentReference); }
		}

		[MaxLength(Schema.MatchingReferenceMaxLength)]
		public ZString MatchingReference
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.MatchingReference); }
			set
			{
				var oldValue = MatchingReference;
				CheckMaximumLength(MatchingReferenceInfo, value);
				this.SetSystemDefinedValue(Schema.MatchingReference, value);
				MatchingReferenceInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo MatchingReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.MatchingReference); }
		}

		public new AsycudaContainer Container => (AsycudaContainer)base.Container;

		public new AsycudaContainerBillOrPackageLink Pivot => (AsycudaContainerBillOrPackageLink)base.Pivot;

		[ChildEditable]
		public virtual UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchStrategy.FetchForDelete();
				UNDGs.DeleteAll();
			}
			base.Delete();
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = "Manifest Pack";
				var consignmentReference = ConsignmentReference;
				if (consignmentReference != 0)
				{
					result += " Consignment Reference " + consignmentReference.ToString();
				}

				return result;
			}
		}

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>());

		List<string> synchroniserReadOnlyMembers;

		protected virtual bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaPackFetchStrategy(this);

		#region ISailingSynchronisationTarget

		BillOfLadingPackLine ISailingSynchronisationTarget<BillOfLadingPackLine>.Source => SailingSynchronisationSource;

		bool ISailingSynchronisationTarget<BillOfLadingPackLine>.IsMatched(BillOfLadingPackLine sailingTarget)
		{
			return IsMatched(sailingTarget);
		}

		void ISailingSynchronisationTarget<BillOfLadingPackLine>.Set(BillOfLadingPackLine source)
		{
			sailingSynchronisationSource = source;
		}

		void ISailingSynchronisationTarget<BillOfLadingPackLine>.Synchronise()
		{
			var source = SailingSynchronisationSource;
			if (source != null)
			{
				using (GetValidationDataSuspender())
				{
					APA_LineNo = source.JL_ItemNo;
					APA_PackQty = source.JL_PackageCount;
					APA_PackUQ = source.JL_F3_NKPackType;
					APA_Weight = source.JL_ActualWeight;
					APA_WeightUQ = source.JL_ActualWeightUQ;
					APA_Volume = source.JL_ActualVolume;
					APA_VolumeUQ = source.JL_ActualVolumeUQ;
					APA_GoodsDescription = source.JL_DetailedDescription.Left(Schema.APA_GoodsDescriptionMaxLength);
					APA_MarksAndNumbers = source.JL_MarksAndNumbers.Left(Schema.APA_MarksAndNumbersMaxLength);

					SetContainer(source);
				}
			}
		}

		void SetContainer(BillOfLadingPackLine source)
		{
			var sourceContainerNumber = source.Container?.JC_ContainerNum ?? ZString.Empty;
			SetContainer(sourceContainerNumber, Bill?.Header);
		}

		public void SetContainer(ZString sourceContainerNumber, AsycudaManifestHeader header)
		{
			AsycudaContainer container = null;
			if (!sourceContainerNumber.IsEmpty)
			{
				container = header?.Containers.Cast<AsycudaContainer>().FirstOrDefault(x => x.ACN_ContainerNumber == sourceContainerNumber);
			}

			ContainerPK = container?.PK ?? ZGuid.Empty;
		}

		BillOfLadingPackLine SailingSynchronisationSource
		{
			get
			{
				if (sailingSynchronisationSource == null && Container is AsycudaContainer container)
				{
					// source is supplied by the Set method.
					// if it is missing then we need to find the pack line that is being synchronised.

					var containerNumber = container.ACN_ContainerNumber;
					var realContainer = Bill?.Header?.BillOfLadingForSync?.RealContainers.Cast<BillOfLadingContainer>().FirstOrDefault(x => x.JC_ContainerNum == containerNumber);
					sailingSynchronisationSource = realContainer?.PackLines.Cast<BillOfLadingPackLine>().FirstOrDefault(x => IsMatched(x, needToMatchContainer: false));
				}

				return sailingSynchronisationSource;
			}
		}
		BillOfLadingPackLine sailingSynchronisationSource;

		bool IsMatched(BillOfLadingPackLine source, bool needToMatchContainer = true)
		{
			return source != null
				&& source.JL_ItemNo == APA_LineNo
				&& source.JL_JS_HouseBill == (Bill?.ABL_BillNumber ?? ZString.Empty)
				&& (!needToMatchContainer || (source.Container?.JC_ContainerNum ?? ZString.Empty) == (Container?.ACN_ContainerNumber ?? ZString.Empty));
		}

		#endregion

		#region Implementation

		void AttachToCustomsNumbers(AsycudaBill bill)
		{
			if (SupportAssociatedPacks(bill))
			{
				var customsEntryNumbers = bill.CustomsEntryNumbers;
				if (customsEntryNumbers.Count == 1)
				{
					customsEntryNumbers[0].PackPivots.AddPivotFor(this);
				}
			}
		}

		void DetachFromCustomsNumbers(AsycudaBill bill)
		{
			if (SupportAssociatedPacks(bill))
			{
				var customsEntryNumbers = bill.CustomsEntryNumbers;
				foreach (ABLEntryNum entryNumber in customsEntryNumbers)
				{
					entryNumber.PackPivots.DeletePivotFor(this);
				}
			}
		}

		static ZBool SupportAssociatedPacks(AsycudaBill bill)
		{
			return bill != null && (bill.Header?.SupportAssociatedPacks ?? ZBool.False);
		}

		void AssignConsignmentReferenceNumber()
		{
			if (ConsignmentReference == 0)
			{
				var header = Bill?.Header;
				if (header != null)
				{
					ConsignmentReference = header.ConsignmentReferenceTracker + 1;
				}
			}
		}

		string ISelectionItem.SelectionDescription(bool showStatus)
		{
			var function = "";

			if (showStatus)
			{
				var packedItem = IsOnePackedItemRelationship ? GetPackedItemFromCollection() : null;
				function = packedItem?.MessageStatusProvider?.AllowModificationMessage(packedItem) ?? false ? " - Amendment" : " - Original";
			}

			return Invariant($"Pack - {Bill?.ABL_BillNumber ?? ZString.Empty} - {CodeProperty}{function}");
		}

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);
		#endregion
	}
}
