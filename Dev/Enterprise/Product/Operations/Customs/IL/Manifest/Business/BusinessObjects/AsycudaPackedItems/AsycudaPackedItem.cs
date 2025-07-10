using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IL.Manifest.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class AsycudaPackedItem : ASYCUDA.Business.AsycudaPackedItem, Integration.Customs.ASYCUDA.ILManifest.IAsycudaPackedItem, IHugeSequenceNumberLine, ITariffFormatProvider
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaPackedItem.Schema
		{
			public new const int API_GoodsDescriptionMaxLength = 256;
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		[ReadOnly(true)]
		[ResourceStringData("A3004071-74A4-4E07-9BFA-BBD604D4D2A9", Caption = "Sequence Number", ShortCaption = "Seq.")]
		public override ZInt API_LineNo
		{
			get { return base.API_LineNo; }
			set
			{
				var oldValue = API_LineNo;
				base.API_LineNo = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					Bill?.PackedItems.SequenceNumberCalculator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}

		ZGuid ISequenceNumberLine.FKToHeader => API_ABL_Bill;

		ZInt ISequenceNumberLine<ZInt>.SequenceNumber
		{
			get
			{
				return API_LineNo;
			}
			set
			{
				API_LineNo = value;
			}
		}

		[RelatedBusinessObject("Bill")]
		public override ZGuid API_ABL_Bill
		{
			get => base.API_ABL_Bill;
			set
			{
				var oldValue = base.API_ABL_Bill;
				base.API_ABL_Bill = value;
				if (!IsCopying && oldValue != API_ABL_Bill)
				{
					if (!API_ABL_Bill.IsValid)
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
				bill.PackedItems.SequenceNumberCalculator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		void AttachedToBill()
		{
			var bill = Bill;
			if (bill != null)
			{
				bill.PackedItems.SequenceNumberCalculator.RecalculateWhenAdded(this);
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.TariffList))]
		public override ZString API_Tariff
		{
			get
			{
				return base.API_Tariff;
			}
			set
			{
				base.API_Tariff = TariffFormatter.Format(value).Left(API_TariffInfo.MaxLength);
				Bill.PackedItems.MarkAsNeedingValidation();
			}
		}

		ITariffFormatter TariffFormatter => TariffFormatterDecider.GetByCountryCode(CountryCode);

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		[ResourceStringData("IL.Manifest.Business.AsycudaPackedItem.API_GoodsDescription", Caption = "Goods Description", ShortCaption = "Goods desc.")]
		[MaxLength(Schema.API_GoodsDescriptionMaxLength)]
		public override ZString API_GoodsDescription { get => base.API_GoodsDescription; set => base.API_GoodsDescription = value.Left(API_GoodsDescriptionInfo.MaxLength); }

		[ResourceStringData("IL.Manifest.Business.AsycudaPackedItem.API_GrossWeight", Caption = "Gross Weight", ShortCaption = "Weight")]
		public override ZDecimal API_GrossWeight { get => base.API_GrossWeight; set => base.API_GrossWeight = value; }

		[ResourceStringData("IL.Manifest.Business.AsycudaPackedItem.API_GrossWeightUQ", Caption = "Gross Weight UQ", ShortCaption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.GrossWeightUQList))]
		public override ZString API_GrossWeightUQ { get => base.API_GrossWeightUQ; set => base.API_GrossWeightUQ = value; }

		[ResourceStringData("IL.Manifest.Business.AsycudaPackedItem.API_PackStatus", Caption = "Cargo Status", MediumCaption = "Cargo Status")]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.PackStatusList))]
		public override ZString API_PackStatus { get => base.API_PackStatus; set => base.API_PackStatus = value; }

		protected override bool API_PackStatus_ReadOnly => false;

		#region Additional Infos

		[ChildEditable]
		public AsycudaAdditionalInfoCollection AdditionalInfos => fAdditionalInfos ??= GetAdditionalInfos();
		AsycudaAdditionalInfoCollection fAdditionalInfos;

		AsycudaAdditionalInfoCollection GetAdditionalInfos()
		{
			var additionalInfoCollection = new AsycudaAdditionalInfoCollection(this);
			additionalInfoCollection.Load();
			RegisterEditableChildObject(additionalInfoCollection);
			return additionalInfoCollection;
		}

		#endregion Additional Infos

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			API_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public IAsycudaLinkPackageCollection<AsycudaLinkPackage> AsycudaLinkPackages
		{
			get
			{
				if (asycudaLinkPackages == null)
				{
					asycudaLinkPackages = new AsycudaLinkPackageCollection<AsycudaLinkPackage>(this);
					asycudaLinkPackages.Load();
					RegisterEditableChildObject(asycudaLinkPackages);
				}

				return asycudaLinkPackages;
			}
		}
		IAsycudaLinkPackageCollection<AsycudaLinkPackage> asycudaLinkPackages;

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public AsycudaPackPackedItemPivotCollection PackagesPivot
		{
			get
			{
				if (fPackagesPivot == null)
				{
					fPackagesPivot = new AsycudaPackPackedItemPivotCollection<AsycudaPackedItem, AsycudaPack>(this);
					fPackagesPivot.Load();
					fPackagesPivot.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(fPackagesPivot);
				}
				return fPackagesPivot;
			}
		}
		AsycudaPackPackedItemPivotCollection fPackagesPivot;

		public AsycudaPackPackedItemPivot ToggleLinkageWithPackage(AsycudaPack package, bool value)
		{
			AsycudaPackPackedItemPivot result = null;
			var basePackagePivotCollection = (package != null) ? PackagesPivot : null;
			if (basePackagePivotCollection != null)
			{
				if (value)
				{
					result = basePackagePivotCollection.AddPivotFor(package);
				}
				else if (basePackagePivotCollection.Cast<AsycudaPackPackedItemPivot>().Any(p => p.APP_APA_Pack == package.PK))
				{
					basePackagePivotCollection.DeletePivotFor(package);
				}
			}

			return result;
		}

		public new AsycudaPackedItemLookups Lookups => (AsycudaPackedItemLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackedItemLookups GetNewLookups() => new AsycudaPackedItemLookups(this);

		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;

		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);
	}
}
