using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageLineCollectionTo<T, AssociatedObjectT> : ManyToManyBusinessObjectCollection<T, AssociatedObjectT>
		where T : CusTempStorageLine
		where AssociatedObjectT : CusTempStorageLine
	{
		public CusTempStorageLineCollectionTo(AssociatedObjectT fromLine) : base(fromLine)
		{
		}

		protected AssociatedObjectT FromLine => (AssociatedObjectT)fAssociatedObject;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var cusTempStorageLine = (T)child;
			using (cusTempStorageLine.SuspendSettingHasChanges())
			{
				cusTempStorageLine.TSL_STH = FromLine.TSL_STH;
			}

			base.SetDefaultsForNewChild(child);
		}

		protected override Type TypeOfRelationshipBusinessObject => typeof(CusTempStorageLinePivot);

		protected override SchemaGuidColumn PivotTableFKToAssociatedBusinessObject => CusTempStorageLinePivotSchema.SLR_TSL_FromLine;

		protected override SchemaGuidColumn PivotTableFKToCollectionBusinessObjects => CusTempStorageLinePivotSchema.SLR_TSL_ToLine;
	}
}
