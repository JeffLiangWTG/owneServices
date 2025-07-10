using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class CusGoodsLocationWrapper : ICusGoodsLocationWrapper
{
	public CusGoodsLocationWrapper(CusGoodsLocation goodsLocation)
	{
		this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
	}

	readonly CusGoodsLocation goodsLocation;

	#region ICusGoodsLocationWrapper

	EU.Business.CusGoodsLocationAddress ICusGoodsLocationWrapper.Address => goodsLocation.Address;

	IReadOnlyCollection<ZString> ICusGoodsLocationWrapper.GetCustomOfficeCodes()
	{
		if (goodsLocation.Parent is JobDeclaration declaration && !declaration.JE_CustomsOffice.IsEmpty)
		{
			return new[] { declaration.JE_CustomsOffice };
		}

		return Array.Empty<ZString>();
	}

	#endregion
}
