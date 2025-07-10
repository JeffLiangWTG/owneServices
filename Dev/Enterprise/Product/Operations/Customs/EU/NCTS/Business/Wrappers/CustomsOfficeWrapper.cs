using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CustomsOfficeWrapper : ICustomsOffice
	{
		public CustomsOfficeWrapper(EU.Business.ICustomsOffice customsOffice)
		{
			this.customsOffice = Argument.NotNull(customsOffice, nameof(customsOffice));
		}

		public ZString ArrivalTime => WrapperHelper.GetLongDateAndTime(customsOffice.ArrivalTime);

		public ZString ReferenceNumber => customsOffice.OfficeCode;

		readonly EU.Business.ICustomsOffice customsOffice;
	}
}
