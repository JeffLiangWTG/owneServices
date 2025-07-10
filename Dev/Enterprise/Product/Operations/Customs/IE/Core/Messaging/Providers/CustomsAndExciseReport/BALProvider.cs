using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class BALProvider : IXlsxProvider
	{
		public BALProvider(BALMessage jsonObject)
		{
			this.jsonObject = jsonObject;
		}
		readonly BALMessage jsonObject;

		public ZString Timestamp => jsonObject.Timestamp;

		[XlsxField(2, "Total")]
		public ZString Total => jsonObject.Total;

		[XlsxField(3, "Cash")]
		public ZString Cash => jsonObject.Cash;

		[XlsxField(4, "Deferred")]
		public ZString Deferred => jsonObject.Deferred;
	}
}
