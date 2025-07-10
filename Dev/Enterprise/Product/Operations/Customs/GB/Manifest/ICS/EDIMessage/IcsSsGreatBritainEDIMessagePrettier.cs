using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using static System.FormattableString;

namespace Enterprise.Customs.GB.ICS;

public abstract class IcsSsGreatBritainEDIMessagePrettier
{
	public abstract ZString MakeHumanReadable();

	protected ZString ToKeyValuePairSection(IEnumerable<(ZString key, ZString value)> pairs)
	{
		var content = pairs
			.Where(p => !p.value.IsEmpty)
			.Select(p => new ZString(ToStrongIfNotEmpty(p.key + ": ") + p.value).Trim(' ', ':'))
			.Where(x => !x.IsEmpty)
			.JoinAsString(BR);

		return ToPIfNotEmpty(content);
	}

	protected ZString ToH3IfNotEmpty(ZString h3)
	{
		return !h3.IsEmpty
			? (ZString)Invariant($"<H3>{h3}</H3>")
			: ZString.Empty;
	}

	protected ZString ToPIfNotEmpty(ZString p)
	{
		return !p.IsEmpty
			? (ZString)Invariant($"<p>{p}</p>")
			: ZString.Empty;
	}

	protected ZString ToStrongIfNotEmpty(ZString strong)
	{
		return !strong.IsEmpty
			? (ZString)Invariant($"<strong>{strong}</strong>")
			: ZString.Empty;
	}

	protected const string BR = "<br>";
}
