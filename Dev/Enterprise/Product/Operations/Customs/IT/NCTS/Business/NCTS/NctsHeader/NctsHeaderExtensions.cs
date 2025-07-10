using System;
using CargoWise.Common;

namespace Enterprise.Customs.IT.NCTS.Business;

public static class NctsHeaderExtensions
{
	public static NctsHeader CheckNotNullAndDepartureType(this NctsHeader nctsHeader)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));
		if (!nctsHeader.IsDepartureMovement)
		{
			throw new ArgumentException($"{nameof(nctsHeader)} is not a departure movement");
		}
		return nctsHeader;
	}
}
