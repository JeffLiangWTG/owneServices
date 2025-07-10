using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ServiceManager.Shared;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CustomsOfficeOfTransitWrapper : ICustomsOfficeOfTransit
	{
		CustomsOfficeOfTransitWrapper(NctsEuOfficeCode customsOffice)
		{
			this.customsOffice = Argument.NotNull(customsOffice, nameof(customsOffice));
		}

		readonly NctsEuOfficeCode customsOffice;

		public static CustomsOfficeOfTransitWrapper New(NctsEuOfficeCode customsOffice) => customsOffice == null ? null : new CustomsOfficeOfTransitWrapper(customsOffice);

		public DateTime? ArrivalDateAndTimeEstimated => arrivalDateAndTimeEstimated ?? (arrivalDateAndTimeEstimated = customsOffice.CY_Date.ToNullableDateTime());
		DateTime? arrivalDateAndTimeEstimated;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = customsOffice.CY_Data);
		string referenceNumber;
	}
}
