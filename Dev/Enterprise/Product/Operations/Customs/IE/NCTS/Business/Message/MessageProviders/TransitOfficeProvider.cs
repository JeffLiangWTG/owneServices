using System;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class TransitOfficeProvider : ITransitOffice
	{
		readonly ICustomsOffice office;

		public TransitOfficeProvider(ICustomsOffice office)
		{
			this.office = office;
		}

		public DateTime ETA => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(office.ArrivalTime, removeMillisecond: true);

		public string OfficeCode => office.OfficeCode;
	}
}
