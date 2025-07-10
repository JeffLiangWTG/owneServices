using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaPackedItem : ASYCUDA.Business.AsycudaPackedItem
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AsycudaPackedItemLookups Lookups => (AsycudaPackedItemLookups)base.Lookups;
		protected override ManifestBase.AsycudaPackedItemLookups GetNewLookups() => new AsycudaPackedItemLookups(this);

		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;
		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);

		[ResourceStringData("8900fe96-78f0-4f3b-8f95-570cee7dbe24", Caption = "CUS Code")]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.ChemicalSubstanceCodeList))]
		public override ZString API_ChemicalSubstanceCode
		{
			get => base.API_ChemicalSubstanceCode;
			set
			{
				var oldValue = base.API_ChemicalSubstanceCode;
				base.API_ChemicalSubstanceCode = value;
				if (oldValue != value && !IsCopying)
				{
					Pack?.RefreshBinding();
				}
			}
		}

		public override ZString API_FormattedTariff
		{
			get => base.API_FormattedTariff;
			set
			{
				var oldValue = base.API_FormattedTariff;
				base.API_FormattedTariff = value;
				if (oldValue != value && !IsCopying)
				{
					Pack?.RefreshBinding();
				}
			}
		}

		[ResourceStringData("3BCEF749-85DA-41E8-A553-0C269791D4EE", Caption = "Type of Goods")]
		[List($"{nameof(Lookups)}.{nameof(Lookups.TypeOfGoodsList)}")]
		public override ZString API_TypeOfGoods { get => base.API_TypeOfGoods; set => base.API_TypeOfGoods = value; }

		[ResourceStringData("0E258725-40CA-410E-ADDB-6A7D5C7B2C47", Caption = "Postal Value")]
		[DecimalPlaces(2)]
		public override ZDecimal API_GoodsValue { get => base.API_GoodsValue; set => base.API_GoodsValue = value; }

		[ResourceStringData("50D31495-4814-4BFD-9F3F-04B6AEA8D468", Caption = "Postal Value Currency", MediumCaption = "Currency", ShortCaption = "Curr.")]
		public override ZString API_RX_NKGoodsValueCurrency { get => base.API_RX_NKGoodsValueCurrency; set => base.API_RX_NKGoodsValueCurrency = value; }
	}
}
