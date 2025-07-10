using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Extensions;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	[DependentBusinessObject(typeof(AsycudaBill), nameof(AsycudaBill.Packs))]
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack, IDataGroupingProvider
	{
		public new class Schema : ManifestBase.AsycudaPack.Schema
		{
			public new const int APA_MarksAndNumbersMaxLength = 512;
			public const int APA_PackQtyMaxLength = 8;
		}

		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaPackLookups Lookups => (AsycudaPackLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackLookups(this);

		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;

		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		public ZString DataGrouping
		{
			get
			{
				if (!dataGroupingCached.HasValue)
				{
					dataGroupingCached = Bill?.DataGrouping ?? ZString.Empty;
				}
				return dataGroupingCached.Value;
			}
		}
		ZString? dataGroupingCached;

		public override ZGuid APA_ABL_Bill
		{
			get => base.APA_ABL_Bill;
			set
			{
				var oldValue = APA_ABL_Bill;
				base.APA_ABL_Bill = value;
				if (!IsCopying && oldValue != APA_ABL_Bill)
				{
					dataGroupingCached = null;
				}
			}
		}

		[MaxLength(Schema.APA_PackQtyMaxLength)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.Quantity", Caption = "Quantity (on Pack)", MediumCaption = "Quantity", ShortCaption = "Qty.", FullDescription = "Number of packs.")]
		public override ZInt APA_PackQty
		{
			get => base.APA_PackQty;
			set
			{
				CheckMaximumLength(APA_PackQtyInfo, value.ToString());
				SetPropertyValue(APA_PackQtyInfo, value);
				if (!base.IsValidationSuspended)
				{
					Validation.ValidateAPA_PackQty();
				}
				APA_PackQtyInfo.RefreshBinding();
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.PackUnit", Caption = "Pack Unit", MediumCaption = "Pack UQ", ShortCaption = "UQ", FullDescription = "Measurement unit of the pack quantity.")]
		public override ZString APA_PackUQ { get => base.APA_PackUQ; set => base.APA_PackUQ = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.CommodityCode", Caption = "Commodity Code", MediumCaption = "Commodity", ShortCaption = "Comm.")]
		public override ZString APA_CommodityCode { get => base.APA_CommodityCode; set => base.APA_CommodityCode = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.GoodsDescription", Caption = "Goods Description (on Pack)", MediumCaption = "Description", ShortCaption = "Desc.", FullDescription = "Description of goods, as stipulated on the pack.")]
		public override ZString APA_GoodsDescription { get => base.APA_GoodsDescription; set => base.APA_GoodsDescription = value; }

		[MaxLength(Schema.APA_MarksAndNumbersMaxLength)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.MarksAndNumbers", Caption = "Marks and Numbers (on Pack)", MediumCaption = "Marks & Nums.", ShortCaption = "Marks", FullDescription = "Free form description of the marks and numbers stipulated on the pack.")]
		public override ZString APA_MarksAndNumbers { get => base.APA_MarksAndNumbers; set => base.APA_MarksAndNumbers = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.Weight", Caption = "Weight (on Pack)", MediumCaption = "Weight", ShortCaption = "Wt.", FullDescription = "Weight, as stipulated on the pack.")]
		public override ZDecimal APA_Weight { get => base.APA_Weight; set => base.APA_Weight = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.WeightUnit", Caption = "Weight Unit", MediumCaption = "Wt. UQ", ShortCaption = "UQ", FullDescription = "Measurement unit of the pack weight.")]
		public override ZString APA_WeightUQ { get => base.APA_WeightUQ; set => base.APA_WeightUQ = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.Volume", Caption = "Volume (on Pack)", MediumCaption = "Volume", ShortCaption = "Vol.", FullDescription = "Pack volume.")]
		public override ZDecimal APA_Volume { get => base.APA_Volume; set => base.APA_Volume = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.VolumeUnit", Caption = "Volume Unit", MediumCaption = "Volume UQ", ShortCaption = "Vol. UQ", FullDescription = "Measurement unit of the pack volume.")]
		public override ZString APA_VolumeUQ { get => base.APA_VolumeUQ; set => base.APA_VolumeUQ = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.VINNumber", Caption = "VIN Number", MediumCaption = "VIN No.", ShortCaption = "VIN No.", FullDescription = "Vehicle Identification Number.")]
		public override ZString APA_VINNumber { get => base.APA_VINNumber; set => base.APA_VINNumber = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.LinePrice", Caption = "Line Price", MediumCaption = "Line Price", ShortCaption = "Price", FullDescription = "Total customs value of the pack.")]
		public override ZDecimal LinePrice { get => base.LinePrice; set => base.LinePrice = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.LinePriceCurrency", Caption = "Line Price Currency", MediumCaption = "Currency", ShortCaption = "Curr.", FullDescription = "Currency code associated with the Line Price.")]
		public override ZString LinePriceCurrency { get => base.LinePriceCurrency; set => base.LinePriceCurrency = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaPack.SequenceNumber", Caption = "Sequence Number", MediumCaption = "Seq No.", ShortCaption = "Seq No.", FullDescription = "Pack sequence number.")]
		public override ZShort APA_LineNo { get => base.APA_LineNo; set => base.APA_LineNo = value; }

		public ZDecimal PackWeightInKG => Core.Constants.Weight.ConvertSafe(APA_Weight, APA_WeightUQ, Core.Constants.Weight.Kilograms);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		public new AsycudaPackedItem PackedItem => (AsycudaPackedItem)base.PackedItem;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaPackFetchStrategy(this);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#region Clone

		protected override bool SupportsCloneCore() => true;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var templateCopy = (AsycudaPack)base.CloneInternal(args);

			templateCopy.LinePrice = LinePrice;
			templateCopy.LinePriceCurrency = LinePriceCurrency;
			return templateCopy;
		}

		#endregion
	}
}
