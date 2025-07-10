using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusStorageDocPivotEDIMessageCollection : BusinessObjectCollectionView<EDIMessage>
	{
		public CusStorageDocPivotEDIMessageCollection(CusStorageDocPivot docPivot, EDIMessageCollection completeCollection)
			: base(completeCollection)
		{
			pivot = Argument.NotNull(docPivot, nameof(docPivot));
			Rebuild();
		}
		readonly CusStorageDocPivot pivot;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return pivot?.MessagePivots.FindRelated(element.PK) != null;
		}
	}
}
