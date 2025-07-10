using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCommonBookedMoveCollection : DocumentWrapperCollection<DocCommonBookedMove>
	{
		public DocCommonBookedMoveCollection(CommonCartage cartage)
			: base(cartage.Factory)
		{
			foreach (CommonBookedCtgMove move in cartage.LooseBookedMoves)
			{
				Add(DocCommonBookedMove.New(move, Factory));
			}
		}

		public DocCommonBookedMoveCollection(CommonContainer container, CommonCartage cartage)
			: base(cartage.Factory)
		{
			foreach (CommonBookedCtgMove move in cartage.GetBookedMoves(container))
			{
				Add(DocCommonBookedMove.New(move, Factory));
			}
		}
	}
}
