using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class RF415DatesPlacesTypeProvider : IRF415DatesPlacesType
	{
		public RF415DatesPlacesTypeProvider(RF415MessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly RF415MessageSendingObject sendingObject;

		public string Date => ZDate.Today.ToDateTime().ToString();

		public string OfficeOfDept => sendingObject.OfficeOfDebt;

		public string OfficeOfResponsibility => sendingObject.OfficeOfResponsibility;

		public IGoodsLocation LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods, () => {
			if (sendingObject.Bill != null)
			{
				return new GoodsLocationProvider(sendingObject.Bill.CusGoodsLocation);
			}

			return null;
		});
		CachedValue<IGoodsLocation> locationOfGoods;
	}
}
