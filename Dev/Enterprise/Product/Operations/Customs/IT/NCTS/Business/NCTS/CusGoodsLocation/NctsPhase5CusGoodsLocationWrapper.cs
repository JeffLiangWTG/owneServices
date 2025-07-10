using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsPhase5CusGoodsLocationWrapper : ICusGoodsLocationWrapper
{
	public NctsPhase5CusGoodsLocationWrapper(CusGoodsLocation goodsLocation)
	{
		this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
	}

	readonly CusGoodsLocation goodsLocation;

	#region ICusGoodsLocationWrapper

	IReadOnlyCollection<ZString> ICusGoodsLocationWrapper.GetCustomOfficeCodes()
		=> goodsLocation.Header
			?.MovementHeader?.CustomsOffices
			?.Where(c => c.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture && !c.CY_Data.IsEmpty)
			.Select(c => c.CY_Data)
			.ToArray() ?? Array.Empty<ZString>();

	EU.Business.CusGoodsLocationAddress ICusGoodsLocationWrapper.Address => goodsLocation.Address;

	#endregion
}
