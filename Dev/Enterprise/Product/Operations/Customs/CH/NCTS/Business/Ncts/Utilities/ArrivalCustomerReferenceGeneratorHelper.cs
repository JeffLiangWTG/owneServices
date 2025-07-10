using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public static class ArrivalCustomerReferenceGeneratorHelper
{
	public static ZString GenerateArrivalCustomerReferenceNumber(BusinessObjectFactory factory, ZString authorizationNumber)
	{
		var arrivalCustomerReferenceFormat = CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.Value;
		var referenceFormat = arrivalCustomerReferenceFormat.GetFormatByAuthorizationLocationCode(authorizationNumber);

		if (referenceFormat == null)
		{
			return ZString.Empty;
		}

		var prefix = referenceFormat.Prefix;
		var suffix = referenceFormat.Suffix;
		var year = ZDateTime.Now.Year.ToString();

		ZString sequenceNumber = Env.NumberFountains.GetNctsLocalReferenceNumberFountain($"{GlbCompany.CurrentCompany.PK}{referenceFormat.AuthorizationLocationCode}{(referenceFormat.IsRestartOnNewYear ? year : "0000")}").GetNext(factory).ToString();

		if (!referenceFormat.IsRemoveLeadingZeros)
		{
			sequenceNumber = sequenceNumber.PadLeft(referenceFormat.SequenceNumberLength, '0');
		}

		switch (referenceFormat.YearOption)
		{
			case CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix2:
				sequenceNumber = prefix + year.Substring(2, 2) + sequenceNumber + suffix;
				break;
			case CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix4:
				sequenceNumber = prefix + year + sequenceNumber + suffix;
				break;
			case CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Suffix2:
				sequenceNumber = prefix  + sequenceNumber + suffix + year.Substring(2,2);
				break;
			case CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Suffix4:
				sequenceNumber = prefix + sequenceNumber + suffix + year;
				break;
			default:
				sequenceNumber = prefix + sequenceNumber + suffix;
				break;
		}
		return sequenceNumber;
	}
}
