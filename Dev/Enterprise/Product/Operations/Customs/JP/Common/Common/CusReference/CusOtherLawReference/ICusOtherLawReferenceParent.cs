using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common
{
	public interface ICusOtherLawReferenceParent
	{
		ZBool IsOtherLawReferenceRequired { get; }

		ZString MessageType { get; }

		IEnumerable<ZString> GetTariffAttributesByKey(ZString key);
	}
}
