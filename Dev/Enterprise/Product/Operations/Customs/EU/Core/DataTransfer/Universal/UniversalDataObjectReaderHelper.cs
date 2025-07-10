using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.EU.DataTransfer.Universal;

public class UniversalDataObjectReaderHelper : Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper
{
	public UniversalDataObjectReaderHelper(UniversalObjectFactory factory, ZString targetCountryCode, ZString sourceCountryCode)
		: base(factory, targetCountryCode, sourceCountryCode)
	{
	}

	#region DV1Details Link Dictionary

	public void RegisterDv1DetailsPK(ZInt link, ZGuid pk)
	{
		Dv1DetailsDictionary[link] = pk;
	}

	public ZGuid? GetDv1DetailsPK(ZInt? link)
	{
		ZGuid? result = null;
		if (link.HasValue && Dv1DetailsDictionary.ContainsKey(link.Value))
		{
			result = Dv1DetailsDictionary.GetValueSafe(link.Value);
		}
		return result;
	}

	Dictionary<ZInt, ZGuid> Dv1DetailsDictionary { get { return dv1DetailsDictionary ?? (dv1DetailsDictionary = new Dictionary<ZInt, ZGuid>()); } }
	Dictionary<ZInt, ZGuid> dv1DetailsDictionary;

	#endregion
}
