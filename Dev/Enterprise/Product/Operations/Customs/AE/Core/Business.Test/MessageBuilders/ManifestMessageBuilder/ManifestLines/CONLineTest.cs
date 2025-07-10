using System;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Business.Testing;

public class CONLineTest : ManifestLineTest.ManifestLineTesting
{
	public void TestProperties()
	{
		CONLine line = (CONLine)GetNewManifestLine();
		line.SerialNumber = Pad("SerialNumber");
		line.MarksAndNumbers = Pad("MarksAndNumbers");
		line.CargoDescription = Pad("CargoDescription");
		line.UsedOrNewIndicator = Pad("UsedOrNewIndicator");
		line.CommodityCode = Pad("CommodityCode");
		line.ConsignmentPackages = 123456789;
		line.PackageType = Pad("PackageType");
		line.PackageTypeCode = Pad("PackageTypeCode");
		line.NoOfPallets = 123456789;
		line.ConsignmentWeightInKG = 132.123123;
		line.ConsignmentVolumeInM3 = 123.123137;
		line.DangerousGoodsIndicator = Pad("DangerousGoodsIndicator");
		line.IMOClassNumber = Pad("IMOClassNumber");
		line.UnNumberOfDangerousGoods = Pad("UnNumberOfDangerousGoods");
		line.FlashPoint = Pad("FlashPoint");
		line.UnitOfTemperature1 = Pad("UnitOfTemperature1");
		line.StorageRequestedForDangerousGoods = Pad("StorageRequestedForDangerousGoods");
		line.RefrigerationRequired = Pad("RefrigerationRequired");
		line.MinimumTemperatureOfRefregeration = Pad("MinimumTemperatureOfRefregeration");
		line.MaximumTemperatureOfRefregeration = Pad("MaximumTemperatureOfRefregeration");
		line.UnitOfTemperature2 = Pad("UnitOfTemperature2");
		AssertEquals("CON", line.Identifier);
		AssertEquals("Serial", line.SerialNumber);
		AssertEquals("MarksAndNumbers                         ", line.MarksAndNumbers);
		AssertEquals("CargoDescription                        ", line.CargoDescription);
		AssertEquals("U", line.UsedOrNewIndicator);
		AssertEquals("CommodityC", line.CommodityCode);
		AssertEquals(123456789, line.ConsignmentPackages);
		AssertEquals("PackageType                   ", line.PackageType);
		AssertEquals("Pac", line.PackageTypeCode);
		AssertEquals(1234, line.NoOfPallets);
		AssertEquals(new ZDecimal(132.123), line.ConsignmentWeightInKG);
		AssertEquals(new ZDecimal(123.123), line.ConsignmentVolumeInM3);
		AssertEquals("D", line.DangerousGoodsIndicator);
		AssertEquals("IMO", line.IMOClassNumber);
		AssertEquals("UnNum", line.UnNumberOfDangerousGoods);
		AssertEquals("FlashPo", line.FlashPoint);
		AssertEquals("U", line.UnitOfTemperature1);
		AssertEquals("S", line.StorageRequestedForDangerousGoods);
		AssertEquals("R", line.RefrigerationRequired);
		AssertEquals("Minimum", line.MinimumTemperatureOfRefregeration);
		AssertEquals("Maximum", line.MaximumTemperatureOfRefregeration);
		AssertEquals("U", line.UnitOfTemperature2);
	}

	#region Override
	protected override ManifestLine GetNewManifestLine()
	{
		return new CONLine();
	}

	protected override int FieldCount
	{
		get
		{
			return 22;
		}
	}

	protected override Type ExpectedType
	{
		get
		{
			return typeof(CONLine);
		}
	}
	#endregion
}
