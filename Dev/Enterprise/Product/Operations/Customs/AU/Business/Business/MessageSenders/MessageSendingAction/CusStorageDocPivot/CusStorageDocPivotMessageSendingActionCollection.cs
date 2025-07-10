using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusStorageDocPivotMessageSendingActionCollection : NonPersistentBusinessObjectCollection<CusStorageDocPivotMessageSendingAction>
	{
		public CusStorageDocPivotMessageSendingActionCollection(QuarantineColsHeader colsHeader) : base(colsHeader.Factory)
		{
			PopulateElements(colsHeader.EDocPivotCollection);
		}

		void PopulateElements(CusStorageDocPivotCollection docPivots)
		{
			foreach (CusStorageDocPivot docPivot in docPivots)
			{
				Add(new CusStorageDocPivotMessageSendingAction(docPivot));
			}
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException("Users cannot add a new member");
	}
}
