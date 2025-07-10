using System;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Business.Testing;

public class BOLLineTest : ManifestLineTest.ManifestLineTesting
{
	public void TestProperties()
	{
		BOLLine line = (BOLLine)GetNewManifestLine();
		line.BillOfLadingNo = Pad("BillOfLadingNo");
		line.PartneringLineCode = Pad("PartneringLineCode");
		line.PartneringAgentCode = Pad("PartneringAgentCode");
		line.PortCodeOfOrigin = Pad("PortCodeOfOrigin");
		line.PortCodeOfLoading = Pad("PortCodeOfLoading");
		line.PortCodeOfDischarge = Pad("PortCodeOfDischarge");
		line.PortCodeOfDestination = Pad("PortCodeOfDestination");
		line.DateOfLoading = new ZDateTime(2008, 8, 8, 8, 8, 8);
		line.ManifestRegistrationNumber = Pad("ManifestRegistrationNumber");
		line.TradeCode = Pad("TradeCode");
		line.TransShipmentMode = Pad("TransShipmentMode");
		line.BillOfLadingOwnerName = Pad("BillOfLadingOwnerName");
		line.BillOfLadingOwnerAddress = Pad("BillOfLadingOwnerAddress");
		line.CargoCode = Pad("CargoCode");
		line.ConsolidatedCargoIndicator = Pad("ConsolidatedCargoIndicator");
		line.StorageRequestCode = Pad("StorageRequestCode");
		line.ContainerServiceType = Pad("ContainerServiceType");
		line.CountryOfOrigin = Pad("CountryOfOrigin");
		line.OriginalConsigneeName = Pad("OriginalConsigneeName");
		line.OriginalConsigneeAddress = Pad("OriginalConsigneeAddress");
		line.OriginalVesselName = Pad("OriginalVesselName");
		line.OriginalVoyageNumber = Pad("OriginalVoyageNumber");
		line.OriginalBOLNumber = Pad("OriginalBOLNumber");
		line.OriginalShipperName = Pad("OriginalShipperName");
		line.OriginalShipperAddress = Pad("OriginalShipperAddress");
		line.ShipperName = Pad("ShipperName");
		line.ShipperAddress = Pad("ShipperAddress");
		line.ShipperCountryCode = Pad("ShipperCountryCode");
		line.ConsigneeCode = Pad("ConsigneeCode");
		line.ConsigneeName = Pad("ConsigneeName");
		line.ConsigneeAddress = Pad("ConsigneeAddress");
		line.NotifyCode1 = Pad("NotifyCode1");
		line.NotifyName1 = Pad("NotifyName1");
		line.NotifyAddress1 = Pad("NotifyAddress1");
		line.NotifyCode2 = Pad("NotifyCode2");
		line.NotifyName2 = Pad("NotifyName2");
		line.NotifyAddress2 = Pad("NotifyAddress2");
		line.NotifyCode3 = Pad("NotifyCode3");
		line.NotifyName3 = Pad("NotifyName3");
		line.NotifyAddress3 = Pad("NotifyAddress3");
		line.MarksAndNumbers = Pad("MarksAndNumbers");
		line.CommodityCode = Pad("CommodityCode");
		line.CommodityDescription = Pad("CommodityDescription");
		line.Packages = 123456789;
		line.PackagesType = Pad("PackagesType");
		line.PackagesTypeCode = Pad("PackagesTypeCode");
		line.ContainerNumber = Pad("ContainerNumber");
		line.CheckDigit = Pad("CheckDigit");
		line.NoOfContainers = 123456789;
		line.NoOfTeus = 123456789;
		line.TotalTareWeightInMT = 123465.123456;
		line.CargoWeightInKG = 123465.123456;
		line.GrossWeightInKG = 123465.123456;
		line.CargoVolumeInM3 = 123465.123456;
		line.TotalQuantity = 123123132;
		line.FreightTonne = 123123.13123;
		line.NoOfPallets = 123456789;
		line.SlacIndicator = Pad("SlacIndicator");
		line.ContractCarriageCondition = Pad("ContractCarriageCondition");
		line.Remarks = Pad("Remarks");
		AssertEquals("BOL", line.Identifier);
		AssertEquals("BillOfLadingNo      ", line.BillOfLadingNo);
		AssertEquals("Partne", line.PartneringLineCode);
		AssertEquals("Partne", line.PartneringAgentCode);
		AssertEquals("PortC", line.PortCodeOfOrigin);
		AssertEquals("PortC", line.PortCodeOfLoading);
		AssertEquals("PortC", line.PortCodeOfDischarge);
		AssertEquals("PortC", line.PortCodeOfDestination);
		AssertEquals("08-Aug-2008", line.GetField(BOLLine.Schema.DateOfLoading.Name));
		AssertEquals("Manifest", line.ManifestRegistrationNumber);
		AssertEquals("T", line.TradeCode);
		AssertEquals("T", line.TransShipmentMode);
		AssertEquals("BillOfLadingOwnerName         ", line.BillOfLadingOwnerName);
		AssertEquals("BillOfLadingOwnerAddress                ", line.BillOfLadingOwnerAddress);
		AssertEquals("C", line.CargoCode);
		AssertEquals("C", line.ConsolidatedCargoIndicator);
		AssertEquals("S", line.StorageRequestCode);
		AssertEquals("Contain", line.ContainerServiceType);
		AssertEquals("Co", line.CountryOfOrigin);
		AssertEquals("OriginalConsigneeName         ", line.OriginalConsigneeName);
		AssertEquals("OriginalConsigneeAddress                ", line.OriginalConsigneeAddress);
		AssertEquals("OriginalVesselName            ", line.OriginalVesselName);
		AssertEquals("OriginalVo", line.OriginalVoyageNumber);
		AssertEquals("OriginalBOLNumber   ", line.OriginalBOLNumber);
		AssertEquals("OriginalShipperName           ", line.OriginalShipperName);
		AssertEquals("OriginalShipperAddress                  ", line.OriginalShipperAddress);
		AssertEquals("ShipperName                   ", line.ShipperName);
		AssertEquals("ShipperAddress                          ", line.ShipperAddress);
		AssertEquals("Sh", line.ShipperCountryCode);
		AssertEquals("Consi", line.ConsigneeCode);
		AssertEquals("ConsigneeName                           ", line.ConsigneeName);
		AssertEquals("ConsigneeAddress                        ", line.ConsigneeAddress);
		AssertEquals("Notify", line.NotifyCode1);
		AssertEquals("NotifyName1                             ", line.NotifyName1);
		AssertEquals("NotifyAddress1                          ", line.NotifyAddress1);
		AssertEquals("Notify", line.NotifyCode2);
		AssertEquals("NotifyName2                             ", line.NotifyName2);
		AssertEquals("NotifyAddress2                          ", line.NotifyAddress2);
		AssertEquals("Notify", line.NotifyCode3);
		AssertEquals("NotifyName3                             ", line.NotifyName3);
		AssertEquals("NotifyAddress3                          ", line.NotifyAddress3);
		AssertEquals("MarksAndNumbers                         ", line.MarksAndNumbers);
		AssertEquals("CommodityC", line.CommodityCode);
		AssertEquals("CommodityDescription                    ", line.CommodityDescription);
		AssertEquals(123456789, line.Packages);
		AssertEquals("PackagesType                  ", line.PackagesType);
		AssertEquals("Pac", line.PackagesTypeCode);
		AssertEquals("ContainerN", line.ContainerNumber);
		AssertEquals("C", line.CheckDigit);
		AssertEquals(123, line.NoOfContainers);
		AssertEquals(123, line.NoOfTeus);
		AssertEquals(new ZDecimal(123465.1), line.TotalTareWeightInMT);
		AssertEquals(new ZDecimal(123465.123), line.CargoWeightInKG);
		AssertEquals(new ZDecimal(123465.123), line.GrossWeightInKG);
		AssertEquals(new ZDecimal(123465.123), line.CargoVolumeInM3);
		AssertEquals(123123132, line.TotalQuantity);
		AssertEquals(new ZDecimal(123123.131), line.FreightTonne);
		AssertEquals(1234, line.NoOfPallets);
		AssertEquals("S", line.SlacIndicator);
		AssertEquals("Con", line.ContractCarriageCondition);
		AssertEquals("Remarks                                 ", line.Remarks);
	}

	#region override
	protected override ManifestLine GetNewManifestLine()
	{
		return new BOLLine();
	}

	protected override int FieldCount
	{
		get
		{
			return 61;
		}
	}

	protected override Type ExpectedType
	{
		get
		{
			return typeof(BOLLine);
		}
	}
	#endregion
}
