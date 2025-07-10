using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class CustomsOfficeOfTransitWrapper : ICustomsOfficeOfTransit
{
	public CustomsOfficeOfTransitWrapper(NctsEuOfficeCode customsOffice)
	{
		this.customsOffice = Argument.NotNull(customsOffice, nameof(customsOffice));
		lazyArrivalDateAndTimeEstimated = new Lazy<DateTime?>(GetArrivalDateAndTimeEstimated);
	}

	string ICustomsOfficeOfTransit.ReferenceNumber => customsOffice.CY_Data;

	DateTime? ICustomsOfficeOfTransit.ArrivalDateAndTimeEstimated => lazyArrivalDateAndTimeEstimated.Value;
	readonly Lazy<DateTime?> lazyArrivalDateAndTimeEstimated;

	DateTime? GetArrivalDateAndTimeEstimated()
	{
		var arrivalDateAndTimeEstimated = customsOffice.CY_Date;
		if (arrivalDateAndTimeEstimated.IsEmpty || !arrivalDateAndTimeEstimated.IsValid)
		{
			return null;
		}
		return arrivalDateAndTimeEstimated.ToDateTime();
	}

	readonly NctsEuOfficeCode customsOffice;
}
