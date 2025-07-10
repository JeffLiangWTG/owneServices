using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class DatesPlacesProvider : IDatesPlaces
	{
		public DatesPlacesProvider(RF415MessageSendingObject messageSendingObject, DateTime preparationDateAndTime)
		{
			this.messageSendingObject = messageSendingObject;
			Date = preparationDateAndTime;
		}

		readonly RF415MessageSendingObject messageSendingObject;

		public DateTime Date { get; }

		public string OfficeOfDebt => messageSendingObject.OfficeOfDebt;

		public string OfficeOfResponsibility => messageSendingObject.OfficeOfResponsibility;

		public IRF415GoodsLocation LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods, () => new RF415GoodsLocationProvider(messageSendingObject.Bill.CusGoodsLocation));
		public CachedValue<RF415GoodsLocationProvider> locationOfGoods;
	}
}
