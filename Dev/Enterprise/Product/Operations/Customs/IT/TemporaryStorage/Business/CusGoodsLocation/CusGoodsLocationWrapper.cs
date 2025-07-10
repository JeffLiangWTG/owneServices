using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class CusGoodsLocationWrapper : ICusGoodsLocationWrapper
{
	public CusGoodsLocationWrapper(CusGoodsLocation goodsLocation)
	{
		this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
	}
	readonly CusGoodsLocation goodsLocation;

	EU.Business.CusGoodsLocationAddress ICusGoodsLocationWrapper.Address => goodsLocation.Address;

	IReadOnlyCollection<ZString> ICusGoodsLocationWrapper.GetCustomOfficeCodes()
	{
		if (goodsLocation.Parent is TemporaryStorageHeader header && !header.PresentationCustomsOffice.IsEmpty)
		{
			return new[] { header.PresentationCustomsOffice };
		}

		return Array.Empty<ZString>();
	}
}
