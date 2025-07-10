using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseLoadingObjectNcmDetails : NonPersistentBusinessObject
	{
		public ImportLicenseLoadingObjectNcmDetails(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static class Schema
		{
			public const string SequencialProductNumber = "SequencialProductNumber";
			public const string ComercialMeasureUnitName = "ComercialMeasureUnitName";
			public const string NetWeight = "NetWeight";
			public const string ComercialMerchandiseQuantity = "ComercialMerchandiseQuantity";
			public const string StatisticMerchandiseQuantity = "StatisticMerchandiseQuantity";
			public const string ShipmentTotalValue = "ShipmentTotalValue";
			public const string ProductDescription = "ProductDescription";
		}

		#region SequencialProductNumber

		public ZShort SequencialProductNumber
		{
			get { return fSequencialProductNumber; }
			set { SetNonPersistentPropertyValue(SequencialProductNumberInfo, ref fSequencialProductNumber, value); }
		}

		ZShort fSequencialProductNumber;

		public ZPropertyInfo SequencialProductNumberInfo => GetZPropertyInfo(Schema.SequencialProductNumber);

		#endregion

		#region ComercialMeasureUnitName

		public ZString ComercialMeasureUnitName
		{
			get { return fComercialMeasureUnitName; }
			set { SetNonPersistentPropertyValue(ComercialMeasureUnitNameInfo, ref fComercialMeasureUnitName, value); }
		}

		ZString fComercialMeasureUnitName;

		public ZPropertyInfo ComercialMeasureUnitNameInfo => GetZPropertyInfo(Schema.ComercialMeasureUnitName);

		#endregion

		#region NetWeight

		public ZDecimal NetWeight
		{
			get { return fNetWeight; }
			set { SetNonPersistentPropertyValue(NetWeightInfo, ref fNetWeight, value); }
		}

		ZDecimal fNetWeight;

		public ZPropertyInfo NetWeightInfo => GetZPropertyInfo(Schema.NetWeight);

		#endregion

		#region ComercialMerchandiseQuantity

		public ZDecimal ComercialMerchandiseQuantity
		{
			get { return fComercialMerchandiseQuantity; }
			set { SetNonPersistentPropertyValue(ComercialMerchandiseQuantityInfo, ref fComercialMerchandiseQuantity, value); }
		}

		ZDecimal fComercialMerchandiseQuantity;

		public ZPropertyInfo ComercialMerchandiseQuantityInfo => GetZPropertyInfo(Schema.ComercialMerchandiseQuantity);

		#endregion

		#region StatisticMerchandiseQuantity

		public ZDecimal StatisticMerchandiseQuantity
		{
			get { return fStatisticMerchandiseQuantity; }
			set { SetNonPersistentPropertyValue(StatisticMerchandiseQuantityInfo, ref fStatisticMerchandiseQuantity, value); }
		}

		ZDecimal fStatisticMerchandiseQuantity;

		public ZPropertyInfo StatisticMerchandiseQuantityInfo => GetZPropertyInfo(Schema.StatisticMerchandiseQuantity);

		#endregion

		#region ShipmentTotalValue

		public ZDecimal ShipmentTotalValue
		{
			get { return fShipmentTotalValue; }
			set { SetNonPersistentPropertyValue(ShipmentTotalValueInfo, ref fShipmentTotalValue, value); }
		}

		ZDecimal fShipmentTotalValue;

		public ZPropertyInfo ShipmentTotalValueInfo => GetZPropertyInfo(Schema.ShipmentTotalValue);

		#endregion

		#region ProductDescription

		public ZString ProductDescription
		{
			get { return fProductDescription; }
			set { SetNonPersistentPropertyValue(ProductDescriptionInfo, ref fProductDescription, value); }
		}

		ZString fProductDescription;

		public ZPropertyInfo ProductDescriptionInfo => GetZPropertyInfo(Schema.ProductDescription);

		#endregion
	}
}
