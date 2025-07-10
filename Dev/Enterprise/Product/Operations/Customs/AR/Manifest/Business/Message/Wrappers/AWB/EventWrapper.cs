using System;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class EventWrapper : IEvent
	{
		internal EventWrapper(BusinessObjectFactory factory, ZString location, ZDateTime eventDate)
		{
			airportInfo = ARHelperClass.AirportInfo(factory, location);
			this.eventDate = eventDate.ToDateTime();
		}
		readonly (ZString code, ZString name) airportInfo;
		readonly DateTime eventDate;

		DateTime IEvent.Date => ARHelperClass.SafeDateTime(eventDate);

		string IEvent.LocationCode => airportInfo.code;

		string IEvent.LocationName => airportInfo.name;
	}
}
