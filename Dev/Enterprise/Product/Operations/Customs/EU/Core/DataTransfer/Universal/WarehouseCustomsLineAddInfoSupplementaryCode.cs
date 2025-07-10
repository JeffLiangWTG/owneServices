using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class WarehouseCustomsLineAddInfoSupplementaryCode : IWarehouseCustomsLineAddInfo
	{
		public WarehouseCustomsLineAddInfoSupplementaryCode(CustomsReference supplementaryCode)
		{
			this.supplementaryCode = Argument.NotNull(supplementaryCode, nameof(supplementaryCode));
		}

		readonly CustomsReference supplementaryCode;

		public ZString Type => BondedWarehousingHelper.Constants.SupplementaryCodeAddInfo.Type;

		public ZString AddInfoData
		{
			get
			{
				if (!addInfoData.HasValue)
				{
					addInfoData = AddInfoParser.Serialise(new Dictionary<ZString, ZString>
					{
						{ BondedWarehousingHelper.Constants.SupplementaryCodeAddInfo.AddInfoKeys.Code, supplementaryCode.SubType?.Code?.ToString() },
						{ BondedWarehousingHelper.Constants.SupplementaryCodeAddInfo.AddInfoKeys.Order, supplementaryCode?.Order?.ToString() }
					});
				}
				return addInfoData.Value;
			}
		}
		ZString? addInfoData;

		public ZString NAddInfoData => ZString.Empty;
	}
}
