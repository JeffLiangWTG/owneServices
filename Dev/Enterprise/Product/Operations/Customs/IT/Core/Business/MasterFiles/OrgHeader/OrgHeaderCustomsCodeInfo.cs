using System;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class OrgHeaderCustomsCodeInfo
{
	public OrgHeaderCustomsCodeInfo(ZString idCountryCode, ZString id)
	{
		IdCountryCode = idCountryCode;
		Id = id;
	}

	public ZString IdCountryCode { get; }
	public ZString Id { get; }
	public ZString FullId => FormattableString.Invariant($"{IdCountryCode}{Id}");
}
