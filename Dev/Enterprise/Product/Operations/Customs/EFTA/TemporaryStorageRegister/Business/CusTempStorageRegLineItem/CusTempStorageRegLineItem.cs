using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineItem : AutoCusTempStorageRegLineItem, Integration.Customs.TemporaryStorage.ICusTempStorageRegLineItem
{
	public CusTempStorageRegLineItem(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : AutoCusTempStorageRegLineItem.Schema
	{
		public const string FormattedTariff = "FormattedTariff";
	}

	[ResourceStringData("D6A44DDC-02FE-44E8-B2A3-0AC7E6FE27CF", Caption = "TSD Item Number", MediumCaption = "TSD Item No.", ShortCaption = "TSD It. No.", FullDescription = "Goods Item Number in TSD")]
	[ReadOnly(true)]
	public override ZInt SRI_GoodsItemNumber { get => base.SRI_GoodsItemNumber; set => base.SRI_GoodsItemNumber = value; }

	[ResourceStringData("5DF74D0E-6861-47F4-9A31-1AF3BD279268", Caption = "Commodity Code", MediumCaption = "Commodity", ShortCaption = "Cmdty.", FullDescription = "Tariff Commodity Code for the Goods Item")]
	public ZString FormattedTariff => TariffFormatter.DisplayFormat(SRI_Tariff);

	public ZPropertyInfo FormattedTariffInfo => GetZPropertyInfo(Schema.FormattedTariff);

	[ResourceStringData("399CBA77-8A0D-4321-B46E-5445DD953CC6", Caption = "CUS Code", MediumCaption = "CUS Code", ShortCaption = "CUS Code", FullDescription = "CUS Code for chemical substances")]
	[ReadOnly(true)]
	public override ZString SRI_CusC4Number { get => base.SRI_CusC4Number; set => base.SRI_CusC4Number = value; }

	[ResourceStringData("342C017F-0D78-48AC-923A-9AB31B0AB6E7", Caption = "Goods Description", MediumCaption = "Goods Description", ShortCaption = "Description", FullDescription = "Goods Description Text")]
	[ReadOnly(true)]
	public override ZString SRI_GoodsDescription { get => base.SRI_GoodsDescription; set => base.SRI_GoodsDescription = value; }

	TariffFormatter TariffFormatter => tariffFormatter ??= GetNewTariffFormatter();
	TariffFormatter tariffFormatter;

	protected virtual TariffFormatter GetNewTariffFormatter() => new();

	public Type GetStorageRegLineItemPivotType() => GetStorageRegLineItemPivotTypeCore();

	protected virtual Type GetStorageRegLineItemPivotTypeCore() => typeof(CusTempStorageRegLineItemPivot);
}
