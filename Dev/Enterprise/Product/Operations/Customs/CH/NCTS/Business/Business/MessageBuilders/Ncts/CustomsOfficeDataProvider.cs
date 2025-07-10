using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CustomsOfficeDataProvider : ICustomsOffice
{
	public static IEnumerable<CustomsOfficeDataProvider> NewCollection(NctsEuOfficeCodeCollection officeCodes, ZString typeOfOffice)
	{
		return officeCodes?
			.Where(officeCode => officeCode.CY_Code == typeOfOffice)
			.OrderBy(officeCode => officeCode.CY_SystemCreateTimeUtc)
			.Cast<NctsEuOfficeCode>()
			.Select((officeCode, index) => new CustomsOfficeDataProvider(officeCode, index + 1));
	}

	public static CustomsOfficeDataProvider New(NctsEuOfficeCodeCollection officeCodes, ZString typeOfOffice)
	{
		var office = (NctsEuOfficeCode)officeCodes?.Where(c => c.CY_Code == typeOfOffice).FirstOrDefault();
		return office == null ? null : new CustomsOfficeDataProvider(office, 0);
	}

	CustomsOfficeDataProvider(NctsEuOfficeCode customsOffice, int sequenceNumber)
	{
		this.customsOffice = customsOffice;
		SequenceNumber = sequenceNumber;
	}
	readonly NctsEuOfficeCode customsOffice;

	public int SequenceNumber { get; }

	public string ReferenceNumber => customsOffice.CY_Data;

	public int? DaysFromActivationToArrival => customsOffice.EstimatedNumberOfDays.IsEmpty ? null : (int?)ZInt.ParseSafe(customsOffice.EstimatedNumberOfDays, 0);
}
