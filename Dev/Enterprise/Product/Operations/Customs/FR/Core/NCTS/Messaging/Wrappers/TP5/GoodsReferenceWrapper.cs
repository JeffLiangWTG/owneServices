using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class GoodsReferenceWrapper : IGoodsReference
	{
		public GoodsReferenceWrapper(string itemNumber)
		{
			this.itemNumber = itemNumber;
		}
		readonly string itemNumber;

		public string DeclarationGoodsItemNumber => itemNumber;

		public static GoodsReferenceWrapper New(string itemNumber) => string.IsNullOrEmpty(itemNumber) ? null : new GoodsReferenceWrapper(itemNumber);
	}
}
