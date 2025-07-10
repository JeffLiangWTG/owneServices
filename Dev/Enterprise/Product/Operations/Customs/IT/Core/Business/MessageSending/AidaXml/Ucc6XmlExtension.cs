using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml;

public static class Ucc6XmlExtension
{
	public static ZString RemoveLastCharSafe(this ZString value)
	{
		if (value.IsEmpty)
		{
			return ZString.Empty;
		}
		return value.Remove(value.Length - 1, 1);
	}

	public static int? NullIfZero(this ZInt value)
	{
		return value.IsEmpty
			? null
			: (int?)value;
	}

	public static decimal? NullIfZero(this ZDecimal value)
	{
		return value.IsEmpty
			? null
			: (decimal?)value;
	}

	public static IReadOnlyCollection<IAdditionalInformation> ToAdditionalInformationWrapperCollection(this IEnumerable<AdditionalInfo> additionalInfoCollection)
	{
		if (additionalInfoCollection is null)
		{
			return null;
		}

		if (!additionalInfoCollection.Any())
		{
			return new IAdditionalInformation[] { new NoneOfAboveAdditionalInformationWrapper() }.ToCollection();
		}

		return additionalInfoCollection.Select(x => new AdditionalInformationWrapper(x)).ToCollection();
	}

	public static IEoriTrader ToTraderOrEmpty(this JobDocAddress docAddress)
	{
		if (docAddress is null || docAddress.IsEmpty)
		{
			return null;
		}

		return new TraderWrapper(docAddress);
	}

	public static int ToInt(this ZBool boolValue)
	{
		return boolValue ? 1 : 0;
	}
}
