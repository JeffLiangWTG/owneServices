using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AE.Business;

public class CONLine : ManifestLine
{
	public CONLine() : base(numberOfFields) { }

	#region Override

	protected override void SetPreambleData()
	{
		SetField(Schema.RecordIdentifier, RecordIdentifier);
	}

	public override string RecordIdentifier
	{
		get { return "CON"; }
	}

	#endregion

	#region Properties

	public ZString Identifier
	{
		get { return this[Schema.RecordIdentifier.Name]; }
	}

	public ZString SerialNumber
	{
		get { return this[Schema.SerialNumber.Name]; }
		set { SetField(Schema.SerialNumber, value); }
	}

	public ZString MarksAndNumbers
	{
		get { return this[Schema.MarksAndNumbers.Name]; }
		set { SetField(Schema.MarksAndNumbers, value); }
	}

	public ZString CargoDescription
	{
		get { return this[Schema.CargoDescription.Name]; }
		set { SetField(Schema.CargoDescription, value); }
	}

	public ZString UsedOrNewIndicator
	{
		get { return this[Schema.UsedOrNewIndicator.Name]; }
		set { SetField(Schema.UsedOrNewIndicator, value); }
	}

	public ZString CommodityCode
	{
		get { return this[Schema.CommodityCode.Name]; }
		set { SetField(Schema.CommodityCode, value); }
	}

	public ZInt ConsignmentPackages
	{
		get { return GetFieldAsZInt(Schema.ConsignmentPackages.Name); }
		set { SetField(Schema.ConsignmentPackages, value); }
	}

	public ZString PackageType
	{
		get { return this[Schema.PackageType.Name]; }
		set { SetField(Schema.PackageType, value); }
	}

	public ZString PackageTypeCode
	{
		get { return this[Schema.PackageTypeCode.Name]; }
		set { SetField(Schema.PackageTypeCode, value); }
	}

	public ZInt NoOfPallets
	{
		get { return GetFieldAsZInt(Schema.NoOfPallets.Name); }
		set { SetField(Schema.NoOfPallets, value); }
	}

	public ZDecimal ConsignmentWeightInKG
	{
		get { return GetFieldAsZDecimal(Schema.ConsignmentWeightInKG.Name, Schema.ConsignmentWeightInKG.Length); }
		set { SetField(Schema.ConsignmentWeightInKG, value); }
	}

	public ZDecimal ConsignmentVolumeInM3
	{
		get { return GetFieldAsZDecimal(Schema.ConsignmentVolumeInM3.Name, Schema.ConsignmentVolumeInM3.Length); }
		set { SetField(Schema.ConsignmentVolumeInM3, value); }
	}

	public ZString DangerousGoodsIndicator
	{
		get { return this[Schema.DangerousGoodsIndicator.Name]; }
		set { SetField(Schema.DangerousGoodsIndicator, value); }
	}

	public ZString IMOClassNumber
	{
		get { return this[Schema.IMOClassNumber.Name]; }
		set { SetField(Schema.IMOClassNumber, value); }
	}

	public ZString UnNumberOfDangerousGoods
	{
		get { return this[Schema.UnNumberOfDangerousGoods.Name]; }
		set { SetField(Schema.UnNumberOfDangerousGoods, value); }
	}

	public ZString FlashPoint
	{
		get { return this[Schema.FlashPoint.Name]; }
		set { SetField(Schema.FlashPoint, value); }
	}

	public ZString UnitOfTemperature1
	{
		get { return this[Schema.UnitOfTemperature1.Name]; }
		set { SetField(Schema.UnitOfTemperature1, value); }
	}

	public ZString StorageRequestedForDangerousGoods
	{
		get { return this[Schema.StorageRequestedForDangerousGoods.Name]; }
		set { SetField(Schema.StorageRequestedForDangerousGoods, value); }
	}

	public ZString RefrigerationRequired
	{
		get { return this[Schema.RefrigerationRequired.Name]; }
		set { SetField(Schema.RefrigerationRequired, value); }
	}

	public ZString MinimumTemperatureOfRefregeration
	{
		get { return this[Schema.MinimumTemperatureOfRefregeration.Name]; }
		set { SetField(Schema.MinimumTemperatureOfRefregeration, value); }
	}

	public ZString MaximumTemperatureOfRefregeration
	{
		get { return this[Schema.MaximumTemperatureOfRefregeration.Name]; }
		set { SetField(Schema.MaximumTemperatureOfRefregeration, value); }
	}

	public ZString UnitOfTemperature2
	{
		get { return this[Schema.UnitOfTemperature2.Name]; }
		set { SetField(Schema.UnitOfTemperature2, value); }
	}

	#endregion

	public static class Schema
	{
		public static readonly FlatFileFieldProperty RecordIdentifier = new FlatFileFieldProperty(0, 3);
		public static readonly FlatFileFieldProperty SerialNumber = new FlatFileFieldProperty(1, 6);
		public static readonly FlatFileFieldProperty MarksAndNumbers = new FlatFileFieldProperty(2, 200);
		public static readonly FlatFileFieldProperty CargoDescription = new FlatFileFieldProperty(3, 100);
		public static readonly FlatFileFieldProperty UsedOrNewIndicator = new FlatFileFieldProperty(4, 1);
		public static readonly FlatFileFieldProperty CommodityCode = new FlatFileFieldProperty(5, 10);
		public static readonly FlatFileFieldProperty ConsignmentPackages = new FlatFileFieldProperty(6, 9);
		public static readonly FlatFileFieldProperty PackageType = new FlatFileFieldProperty(7, 30);
		public static readonly FlatFileFieldProperty PackageTypeCode = new FlatFileFieldProperty(8, 3);
		public static readonly FlatFileFieldProperty NoOfPallets = new FlatFileFieldProperty(9, 4);
		public static readonly FlatFileFieldProperty ConsignmentWeightInKG = new FlatFileFieldProperty(10, 3);
		public static readonly FlatFileFieldProperty ConsignmentVolumeInM3 = new FlatFileFieldProperty(11, 3);
		public static readonly FlatFileFieldProperty DangerousGoodsIndicator = new FlatFileFieldProperty(12, 1);
		public static readonly FlatFileFieldProperty IMOClassNumber = new FlatFileFieldProperty(13, 3);
		public static readonly FlatFileFieldProperty UnNumberOfDangerousGoods = new FlatFileFieldProperty(14, 5);
		public static readonly FlatFileFieldProperty FlashPoint = new FlatFileFieldProperty(15, 7);
		public static readonly FlatFileFieldProperty UnitOfTemperature1 = new FlatFileFieldProperty(16, 1);
		public static readonly FlatFileFieldProperty StorageRequestedForDangerousGoods = new FlatFileFieldProperty(17, 1);
		public static readonly FlatFileFieldProperty RefrigerationRequired = new FlatFileFieldProperty(18, 1);
		public static readonly FlatFileFieldProperty MinimumTemperatureOfRefregeration = new FlatFileFieldProperty(19, 7);
		public static readonly FlatFileFieldProperty MaximumTemperatureOfRefregeration = new FlatFileFieldProperty(20, 7);
		public static readonly FlatFileFieldProperty UnitOfTemperature2 = new FlatFileFieldProperty(21, 1);
	}

	const int numberOfFields = 22;
}
