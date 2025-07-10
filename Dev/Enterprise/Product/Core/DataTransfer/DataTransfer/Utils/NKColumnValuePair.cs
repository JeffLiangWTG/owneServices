using System;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business
{
	public struct NKColumnValuePair
	{
		public NKColumnValuePair(Type bizType, SchemaColumn naturalKeyColumn, string value)
		{
			this.BizType = bizType;
			this.NKColumn = naturalKeyColumn;
			this.Value = value;
		}

		public readonly Type BizType;
		public readonly SchemaColumn NKColumn;
		public readonly string Value;

		public static NKColumnValuePair New(string naturalKeyValue, ForeignKeyType foreignKeyType)
		{
			NKColumnValuePair result;
			naturalKeyValue = naturalKeyValue.Trim();

			switch (foreignKeyType)
			{
				case ForeignKeyType.Contact:
					result = new NKColumnValuePair(typeof(OrgContact), OrgContactSchema.OC_ContactName, naturalKeyValue);
					break;

				case ForeignKeyType.WebUrl:
					result = new NKColumnValuePair(typeof(OrgWebURL), OrgWebURLSchema.PU_URL, naturalKeyValue);
					break;

				case ForeignKeyType.OrganisationMatchWithFullName:
					result = new NKColumnValuePair(typeof(OrgHeader), OrgHeaderSchema.OH_FullName, naturalKeyValue);
					break;

				case ForeignKeyType.OrganisationMatchWithCode:
					result = new NKColumnValuePair(typeof(OrgHeader), OrgHeaderSchema.OH_Code, naturalKeyValue);
					break;

				case ForeignKeyType.RefServiceLevelNK:
					result = new NKColumnValuePair(typeof(RefServiceLevel), RefServiceLevelSchema.RS_Code, naturalKeyValue);
					break;

				case ForeignKeyType.IntZoneNK:
					result = new NKColumnValuePair(typeof(RefZoneHeader), RefZoneHeaderSchema.FZ_Code, naturalKeyValue);
					break;

				case ForeignKeyType.CurrencyNK:
					result = new NKColumnValuePair(typeof(RefCurrency), RefCurrencySchema.RX_Code, naturalKeyValue);
					break;

				case ForeignKeyType.OrgAddressNK:
					result = new NKColumnValuePair(typeof(OrgAddress), OrgAddressSchema.OA_Code, naturalKeyValue);
					break;

				case ForeignKeyType.PortNK:
					result = new NKColumnValuePair(typeof(RefUNLOCO), RefUNLOCOSchema.RL_PortName, naturalKeyValue);
					break;

				case ForeignKeyType.CountryNK:
					result = new NKColumnValuePair(typeof(RefCountry), RefCountrySchema.RN_Code, naturalKeyValue);
					break;

				case ForeignKeyType.ContainerCodeNK:
					result = new NKColumnValuePair(typeof(RefContainer), RefContainerSchema.RC_Code, naturalKeyValue);
					break;

				case ForeignKeyType.VesselNameNK:
					result = new NKColumnValuePair(typeof(RefVessel), RefVesselSchema.RV_Code, naturalKeyValue);
					break;

				case ForeignKeyType.ChargeCodeNK:
					result = new NKColumnValuePair(typeof(AccChargeCode), AccChargeCodeSchema.AC_Code, naturalKeyValue);
					break;

				case ForeignKeyType.PackTypeCodeNK:
					result = new NKColumnValuePair(ObjectFactory.GetType<Enterprise.Freight.Integration.IPackLine>(), JobPackLinesSchema.JL_F3_NKPackType, naturalKeyValue);
					break;

				case ForeignKeyType.IncoTermNK:
					result = new NKColumnValuePair(ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>(), JobShipmentSchema.JS_INCO, naturalKeyValue);
					break;

				case ForeignKeyType.EventCodeNK:
					result = new NKColumnValuePair(typeof(StmALog), StmALogSchema.SL_SE_NKEvent, naturalKeyValue);
					break;

				// TODO: Change to Code
				case ForeignKeyType.WarehouseNK:
					result = new NKColumnValuePair(ObjectFactory.GetType<Enterprise.Warehouse.Integration.IWhsWarehouse>(), WhsWarehouseSchema.WW_WarehouseCode, naturalKeyValue);
					break;

				default:
					throw new Exception("Unknown NK column '" + foreignKeyType + "'");
			}

			return result;
		}
	}
}
