using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AE.Business;

public class BOLLine : ManifestLine
{
	public BOLLine() : base(numberOfFields) { }

	#region Override

	protected override void SetPreambleData()
	{
		SetField(Schema.RecordIdentifier, RecordIdentifier);
	}

	public override string RecordIdentifier
	{
		get { return "BOL"; }
	}

	#endregion

	#region Properties

	public ZString Identifier
	{
		get { return this[Schema.RecordIdentifier.Name]; }
	}

	public ZString BillOfLadingNo
	{
		get { return this[Schema.BillOfLadingNo.Name]; }
		set { SetField(Schema.BillOfLadingNo, value); }
	}

	public ZString PartneringLineCode
	{
		get { return this[Schema.PartneringLineCode.Name]; }
		set { SetField(Schema.PartneringLineCode, value); }
	}

	public ZString PartneringAgentCode
	{
		get { return this[Schema.PartneringAgentCode.Name]; }
		set { SetField(Schema.PartneringAgentCode, value); }
	}

	public ZString PortCodeOfOrigin
	{
		get { return this[Schema.PortCodeOfOrigin.Name]; }
		set { SetField(Schema.PortCodeOfOrigin, value); }
	}

	public ZString PortCodeOfLoading
	{
		get { return this[Schema.PortCodeOfLoading.Name]; }
		set { SetField(Schema.PortCodeOfLoading, value); }
	}

	public ZString PortCodeOfDischarge
	{
		get { return this[Schema.PortCodeOfDischarge.Name]; }
		set { SetField(Schema.PortCodeOfDischarge, value); }
	}

	public ZString PortCodeOfDestination
	{
		get { return this[Schema.PortCodeOfDestination.Name]; }
		set { SetField(Schema.PortCodeOfDestination, value); }
	}

	public ZDateTime DateOfLoading
	{
		get { return GetFieldAsZDateTime(Schema.DateOfLoading); }
		set { SetField(Schema.DateOfLoading, value); }
	}

	public ZString ManifestRegistrationNumber
	{
		get { return this[Schema.ManifestRegistrationNumber.Name]; }
		set { SetField(Schema.ManifestRegistrationNumber, value); }
	}

	public ZString TradeCode
	{
		get { return this[Schema.TradeCode.Name]; }
		set { SetField(Schema.TradeCode, value); }
	}

	public ZString TransShipmentMode
	{
		get { return this[Schema.TransShipmentMode.Name]; }
		set { SetField(Schema.TransShipmentMode, value); }
	}

	public ZString BillOfLadingOwnerName
	{
		get { return this[Schema.BillOfLadingOwnerName.Name]; }
		set { SetField(Schema.BillOfLadingOwnerName, value); }
	}

	public ZString BillOfLadingOwnerAddress
	{
		get { return this[Schema.BillOfLadingOwnerAddress.Name]; }
		set { SetField(Schema.BillOfLadingOwnerAddress, value); }
	}

	public ZString CargoCode
	{
		get { return this[Schema.CargoCode.Name]; }
		set { SetField(Schema.CargoCode, value); }
	}

	public ZString ConsolidatedCargoIndicator
	{
		get { return this[Schema.ConsolidatedCargoIndicator.Name]; }
		set { SetField(Schema.ConsolidatedCargoIndicator, value); }
	}

	public ZString StorageRequestCode
	{
		get { return this[Schema.StorageRequestCode.Name]; }
		set { SetField(Schema.StorageRequestCode, value); }
	}

	public ZString ContainerServiceType
	{
		get { return this[Schema.ContainerServiceType.Name]; }
		set { SetField(Schema.ContainerServiceType, value); }
	}

	public ZString CountryOfOrigin
	{
		get { return this[Schema.CountryOfOrigin.Name]; }
		set { SetField(Schema.CountryOfOrigin, value); }
	}

	public ZString OriginalConsigneeName
	{
		get { return this[Schema.OriginalConsigneeName.Name]; }
		set { SetField(Schema.OriginalConsigneeName, value); }
	}

	public ZString OriginalConsigneeAddress
	{
		get { return this[Schema.OriginalConsigneeAddress.Name]; }
		set { SetField(Schema.OriginalConsigneeAddress, value); }
	}

	public ZString OriginalVesselName
	{
		get { return this[Schema.OriginalVesselName.Name]; }
		set { SetField(Schema.OriginalVesselName, value); }
	}

	public ZString OriginalVoyageNumber
	{
		get { return this[Schema.OriginalVoyageNumber.Name]; }
		set { SetField(Schema.OriginalVoyageNumber, value); }
	}

	public ZString OriginalBOLNumber
	{
		get { return this[Schema.OriginalBOLNumber.Name]; }
		set { SetField(Schema.OriginalBOLNumber, value); }
	}

	public ZString OriginalShipperName
	{
		get { return this[Schema.OriginalShipperName.Name]; }
		set { SetField(Schema.OriginalShipperName, value); }
	}

	public ZString OriginalShipperAddress
	{
		get { return this[Schema.OriginalShipperAddress.Name]; }
		set { SetField(Schema.OriginalShipperAddress, value); }
	}

	public ZString ShipperName
	{
		get { return this[Schema.ShipperName.Name]; }
		set { SetField(Schema.ShipperName, value); }
	}

	public ZString ShipperAddress
	{
		get { return this[Schema.ShipperAddress.Name]; }
		set { SetField(Schema.ShipperAddress, value); }
	}

	public ZString ShipperCountryCode
	{
		get { return this[Schema.ShipperCountryCode.Name]; }
		set { SetField(Schema.ShipperCountryCode, value); }
	}

	public ZString ConsigneeCode
	{
		get { return this[Schema.ConsigneeCode.Name]; }
		set { SetField(Schema.ConsigneeCode, value); }
	}

	public ZString ConsigneeName
	{
		get { return this[Schema.ConsigneeName.Name]; }
		set { SetField(Schema.ConsigneeName, value); }
	}

	public ZString ConsigneeAddress
	{
		get { return this[Schema.ConsigneeAddress.Name]; }
		set { SetField(Schema.ConsigneeAddress, value); }
	}

	#region Notify

	#region Notify 1

	public ZString NotifyCode1
	{
		get { return this[Schema.NotifyCode1.Name]; }
		set { SetField(Schema.NotifyCode1, value); }
	}

	public ZString NotifyName1
	{
		get { return this[Schema.NotifyName1.Name]; }
		set { SetField(Schema.NotifyName1, value); }
	}

	public ZString NotifyAddress1
	{
		get { return this[Schema.NotifyAddress1.Name]; }
		set { SetField(Schema.NotifyAddress1, value); }
	}

	#endregion

	#region Notify 2

	public ZString NotifyCode2
	{
		get { return this[Schema.NotifyCode2.Name]; }
		set { SetField(Schema.NotifyCode2, value); }
	}

	public ZString NotifyName2
	{
		get { return this[Schema.NotifyName2.Name]; }
		set { SetField(Schema.NotifyName2, value); }
	}

	public ZString NotifyAddress2
	{
		get { return this[Schema.NotifyAddress2.Name]; }
		set { SetField(Schema.NotifyAddress2, value); }
	}

	#endregion

	#region Notify 3

	public ZString NotifyCode3
	{
		get { return this[Schema.NotifyCode3.Name]; }
		set { SetField(Schema.NotifyCode3, value); }
	}

	public ZString NotifyName3
	{
		get { return this[Schema.NotifyName3.Name]; }
		set { SetField(Schema.NotifyName3, value); }
	}

	public ZString NotifyAddress3
	{
		get { return this[Schema.NotifyAddress3.Name]; }
		set { SetField(Schema.NotifyAddress3, value); }
	}

	#endregion

	#endregion

	public ZString MarksAndNumbers
	{
		get { return this[Schema.MarksAndNumbers.Name]; }
		set { SetField(Schema.MarksAndNumbers, value); }
	}

	public ZString CommodityCode
	{
		get { return this[Schema.CommodityCode.Name]; }
		set { SetField(Schema.CommodityCode, value); }
	}

	public ZString CommodityDescription
	{
		get { return this[Schema.CommodityDescription.Name]; }
		set { SetField(Schema.CommodityDescription, value); }
	}

	public ZInt Packages
	{
		get { return GetFieldAsZInt(Schema.Packages.Name); }
		set { SetField(Schema.Packages, value); }
	}

	public ZString PackagesType
	{
		get { return this[Schema.PackagesType.Name]; }
		set { SetField(Schema.PackagesType, value); }
	}

	public ZString PackagesTypeCode
	{
		get { return this[Schema.PackagesTypeCode.Name]; }
		set { SetField(Schema.PackagesTypeCode, value); }
	}

	public ZString ContainerNumber
	{
		get { return this[Schema.ContainerNumber.Name]; }
		set { SetField(Schema.ContainerNumber, value); }
	}

	public ZString CheckDigit
	{
		get { return this[Schema.CheckDigit.Name]; }
		set { SetField(Schema.CheckDigit, value); }
	}

	public ZInt NoOfContainers
	{
		get { return GetFieldAsZInt(Schema.NoOfContainers.Name); }
		set { SetField(Schema.NoOfContainers, value); }
	}

	public ZInt NoOfTeus
	{
		get { return GetFieldAsZInt(Schema.NoOfTeus.Name); }
		set { SetField(Schema.NoOfTeus, value); }
	}

	public ZDecimal TotalTareWeightInMT
	{
		get { return GetFieldAsZDecimal(Schema.TotalTareWeightInMT.Name, Schema.TotalTareWeightInMT.Length); }
		set { SetField(Schema.TotalTareWeightInMT, value); }
	}

	public ZDecimal CargoWeightInKG
	{
		get { return GetFieldAsZDecimal(Schema.CargoWeightInKG.Name, Schema.CargoWeightInKG.Length); }
		set { SetField(Schema.CargoWeightInKG, value); }
	}

	public ZDecimal GrossWeightInKG
	{
		get { return GetFieldAsZDecimal(Schema.GrossWeightInKG.Name, Schema.GrossWeightInKG.Length); }
		set { SetField(Schema.GrossWeightInKG, value); }
	}

	public ZDecimal CargoVolumeInM3
	{
		get { return GetFieldAsZDecimal(Schema.CargoVolumeInM3.Name, Schema.CargoVolumeInM3.Length); }
		set { SetField(Schema.CargoVolumeInM3, value); }
	}

	public ZInt TotalQuantity
	{
		get { return GetFieldAsZInt(Schema.TotalQuantity.Name); }
		set { SetField(Schema.TotalQuantity, value); }
	}

	public ZDecimal FreightTonne
	{
		get { return GetFieldAsZDecimal(Schema.FreightTonne.Name, Schema.FreightTonne.Length); }
		set { SetField(Schema.FreightTonne, value); }
	}

	public ZInt NoOfPallets
	{
		get { return GetFieldAsZInt(Schema.NoOfPallets.Name); }
		set { SetField(Schema.NoOfPallets, value); }
	}

	public ZString SlacIndicator
	{
		get { return this[Schema.SlacIndicator.Name]; }
		set { SetField(Schema.SlacIndicator, value); }
	}

	public ZString ContractCarriageCondition
	{
		get { return this[Schema.ContractCarriageCondition.Name]; }
		set { SetField(Schema.ContractCarriageCondition, value); }
	}

	public ZString Remarks
	{
		get { return this[Schema.Remarks.Name]; }
		set { SetField(Schema.Remarks, value); }
	}

	#endregion

	public static class Schema
	{
		public static readonly FlatFileFieldProperty RecordIdentifier = new FlatFileFieldProperty(0, 3);
		public static readonly FlatFileFieldProperty BillOfLadingNo = new FlatFileFieldProperty(1, 20);
		public static readonly FlatFileFieldProperty PartneringLineCode = new FlatFileFieldProperty(2, 6);
		public static readonly FlatFileFieldProperty PartneringAgentCode = new FlatFileFieldProperty(3, 6);
		public static readonly FlatFileFieldProperty PortCodeOfOrigin = new FlatFileFieldProperty(4, 5);
		public static readonly FlatFileFieldProperty PortCodeOfLoading = new FlatFileFieldProperty(5, 5);
		public static readonly FlatFileFieldProperty PortCodeOfDischarge = new FlatFileFieldProperty(6, 5);
		public static readonly FlatFileFieldProperty PortCodeOfDestination = new FlatFileFieldProperty(7, 5);
		public static readonly FlatFileFieldProperty DateOfLoading = new FlatFileFieldProperty(8, 11);
		public static readonly FlatFileFieldProperty ManifestRegistrationNumber = new FlatFileFieldProperty(9, 8);
		public static readonly FlatFileFieldProperty TradeCode = new FlatFileFieldProperty(10, 1);
		public static readonly FlatFileFieldProperty TransShipmentMode = new FlatFileFieldProperty(11, 1);
		public static readonly FlatFileFieldProperty BillOfLadingOwnerName = new FlatFileFieldProperty(12, 30);
		public static readonly FlatFileFieldProperty BillOfLadingOwnerAddress = new FlatFileFieldProperty(13, 240);
		public static readonly FlatFileFieldProperty CargoCode = new FlatFileFieldProperty(14, 1);
		public static readonly FlatFileFieldProperty ConsolidatedCargoIndicator = new FlatFileFieldProperty(15, 1);
		public static readonly FlatFileFieldProperty StorageRequestCode = new FlatFileFieldProperty(16, 1);
		public static readonly FlatFileFieldProperty ContainerServiceType = new FlatFileFieldProperty(17, 7);
		public static readonly FlatFileFieldProperty CountryOfOrigin = new FlatFileFieldProperty(18, 2);
		public static readonly FlatFileFieldProperty OriginalConsigneeName = new FlatFileFieldProperty(19, 30);
		public static readonly FlatFileFieldProperty OriginalConsigneeAddress = new FlatFileFieldProperty(20, 240);
		public static readonly FlatFileFieldProperty OriginalVesselName = new FlatFileFieldProperty(21, 30);
		public static readonly FlatFileFieldProperty OriginalVoyageNumber = new FlatFileFieldProperty(22, 10);
		public static readonly FlatFileFieldProperty OriginalBOLNumber = new FlatFileFieldProperty(23, 20);
		public static readonly FlatFileFieldProperty OriginalShipperName = new FlatFileFieldProperty(24, 30);
		public static readonly FlatFileFieldProperty OriginalShipperAddress = new FlatFileFieldProperty(25, 240);
		public static readonly FlatFileFieldProperty ShipperName = new FlatFileFieldProperty(26, 30);
		public static readonly FlatFileFieldProperty ShipperAddress = new FlatFileFieldProperty(27, 240);
		public static readonly FlatFileFieldProperty ShipperCountryCode = new FlatFileFieldProperty(28, 2);
		public static readonly FlatFileFieldProperty ConsigneeCode = new FlatFileFieldProperty(29, 5);
		public static readonly FlatFileFieldProperty ConsigneeName = new FlatFileFieldProperty(30, 48);
		public static readonly FlatFileFieldProperty ConsigneeAddress = new FlatFileFieldProperty(31, 240);
		public static readonly FlatFileFieldProperty NotifyCode1 = new FlatFileFieldProperty(32, 6);
		public static readonly FlatFileFieldProperty NotifyName1 = new FlatFileFieldProperty(33, 48);
		public static readonly FlatFileFieldProperty NotifyAddress1 = new FlatFileFieldProperty(34, 240);
		public static readonly FlatFileFieldProperty NotifyCode2 = new FlatFileFieldProperty(35, 6);
		public static readonly FlatFileFieldProperty NotifyName2 = new FlatFileFieldProperty(36, 48);
		public static readonly FlatFileFieldProperty NotifyAddress2 = new FlatFileFieldProperty(37, 240);
		public static readonly FlatFileFieldProperty NotifyCode3 = new FlatFileFieldProperty(38, 6);
		public static readonly FlatFileFieldProperty NotifyName3 = new FlatFileFieldProperty(39, 48);
		public static readonly FlatFileFieldProperty NotifyAddress3 = new FlatFileFieldProperty(40, 240);
		public static readonly FlatFileFieldProperty MarksAndNumbers = new FlatFileFieldProperty(41, 200);
		public static readonly FlatFileFieldProperty CommodityCode = new FlatFileFieldProperty(42, 10);
		public static readonly FlatFileFieldProperty CommodityDescription = new FlatFileFieldProperty(43, 100);
		public static readonly FlatFileFieldProperty Packages = new FlatFileFieldProperty(44, 9);
		public static readonly FlatFileFieldProperty PackagesType = new FlatFileFieldProperty(45, 30);
		public static readonly FlatFileFieldProperty PackagesTypeCode = new FlatFileFieldProperty(46, 3);
		public static readonly FlatFileFieldProperty ContainerNumber = new FlatFileFieldProperty(47, 10);
		public static readonly FlatFileFieldProperty CheckDigit = new FlatFileFieldProperty(48, 1);
		public static readonly FlatFileFieldProperty NoOfContainers = new FlatFileFieldProperty(49, 3);
		public static readonly FlatFileFieldProperty NoOfTeus = new FlatFileFieldProperty(50, 3);
		public static readonly FlatFileFieldProperty TotalTareWeightInMT = new FlatFileFieldProperty(51, 1);
		public static readonly FlatFileFieldProperty CargoWeightInKG = new FlatFileFieldProperty(52, 3);
		public static readonly FlatFileFieldProperty GrossWeightInKG = new FlatFileFieldProperty(53, 3);
		public static readonly FlatFileFieldProperty CargoVolumeInM3 = new FlatFileFieldProperty(54, 3);
		public static readonly FlatFileFieldProperty TotalQuantity = new FlatFileFieldProperty(55, 9);
		public static readonly FlatFileFieldProperty FreightTonne = new FlatFileFieldProperty(56, 3);
		public static readonly FlatFileFieldProperty NoOfPallets = new FlatFileFieldProperty(57, 4);
		public static readonly FlatFileFieldProperty SlacIndicator = new FlatFileFieldProperty(58, 1);
		public static readonly FlatFileFieldProperty ContractCarriageCondition = new FlatFileFieldProperty(59, 3);
		public static readonly FlatFileFieldProperty Remarks = new FlatFileFieldProperty(60, 200);
	}

	const int numberOfFields = 61;
}
