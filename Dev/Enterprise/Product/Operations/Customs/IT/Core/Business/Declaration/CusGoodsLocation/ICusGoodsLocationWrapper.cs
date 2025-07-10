using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public interface ICusGoodsLocationWrapper
{
	EU.Business.CusGoodsLocationAddress Address { get; }

	IReadOnlyCollection<ZString> GetCustomOfficeCodes();
}
