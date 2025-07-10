using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;

namespace Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort
{
	public sealed class DocRollUpGroup<TID, TDocLineList>
		where TID : IZType
		where TDocLineList : ISortableDocLineList
	{
		public DocRollUpGroup(TDocLineList collection, bool needsRollUp, TID id)
			{
				Collection = collection;
				NeedsRollUp = needsRollUp;
				ID = id;
			}

			public TID ID { get; }

			public bool NeedsRollUp { get; }

			public TDocLineList Collection { get; }
		}
}
