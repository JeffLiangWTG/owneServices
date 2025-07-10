using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public class InvoiceLineChargeDefinitionProvider
	{
		public InvoiceLineChargeDefinitionProvider(WarehouseCustomsAddInfo addInfo)
		{
			this.addInfo = Argument.NotNull(addInfo, nameof(addInfo));
		}
		readonly WarehouseCustomsAddInfo addInfo;

		Dictionary<ZString, ZString> AddInfoDictionary => addInfoDictionary ?? (addInfoDictionary = AddInfoParser.CreateDictionaryWithAddInfoString(addInfo.B7_AddInfoData));
		Dictionary<ZString, ZString> addInfoDictionary;

		public ZDecimal Amount => ZDecimal.ParseSafe(AddInfoDictionary.GetValueSafe(EU.Business.BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.Amount), ZDecimal.Zero);

		public ZString ChargeType => AddInfoDictionary.GetValueSafe(EU.Business.BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.ChargeType);

		public ZString Currency => AddInfoDictionary.GetValueSafe(EU.Business.BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.Currency);

		public ZBool IsDutiable
		{
			get
			{
				ZBool.TryParse(AddInfoDictionary.GetValueSafe(EU.Business.BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.IsDutiable), out var result);
				return result;
			}
		}

		public ZBool IsGSTApplicable
		{
			get
			{
				ZBool.TryParse(AddInfoDictionary.GetValueSafe(EU.Business.BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.IsGSTApplicable), out var result);
				return result;
			}
		}

		public ZBool IsIncludedInITOT
		{
			get
			{
				ZBool.TryParse(AddInfoDictionary.GetValueSafe(EU.Business.BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.IsIncludedInITOT), out var result);
				return result;
			}
		}

		public ZBool IsStatisticalValueApplicable
		{
			get
			{
				ZBool.TryParse(AddInfoDictionary.GetValueSafe(EU.Business.BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.IsStatisticalValueApplicable), out var result);
				return result;
			}
		}
	}
}
