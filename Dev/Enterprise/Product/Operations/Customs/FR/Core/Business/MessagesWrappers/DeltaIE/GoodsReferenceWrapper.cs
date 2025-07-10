using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GoodsReferenceWrapper : CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces.IGoodsReference
	{
		GoodsReferenceWrapper(ZString mergeLineNumber)
		{
			this.mergeLineNumber = mergeLineNumber;
		}
		readonly ZString mergeLineNumber;

		public string DeclarationGoodsItemNumber => mergeLineNumber;

		public static GoodsReferenceWrapper New(ZString mergeLineNumber) => new GoodsReferenceWrapper(mergeLineNumber);
	}
}
