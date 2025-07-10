using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public static class ApplicationReferenceHelper
{
	public static ZString GetNew(ZString node, ZString subscriber, ZString customsOffice) => FormattableString.Invariant($"{node}:{subscriber}:{customsOffice}");

	public static ZString GetNew(ICustomsEntryApplicationReference customsEntryApplicationReference)
	{
		Argument.NotNull(customsEntryApplicationReference, nameof(customsEntryApplicationReference));
		return GetNew(customsEntryApplicationReference.Node, customsEntryApplicationReference.Subscriber, customsEntryApplicationReference.CustomsOffice);
	}
}
