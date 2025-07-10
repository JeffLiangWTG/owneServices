using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldColumnCollection : BusinessObjectCollection<StmSystemDefinedFieldColumn>
	{
		public StmSystemDefinedFieldColumnCollection(StmSystemDefinedField parentField, BusinessObjectFactory factory) : base(factory)
		{
			this.ParentField = parentField;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			StmSystemDefinedFieldColumn newFieldColumn = (StmSystemDefinedFieldColumn)child;

			newFieldColumn.S1_Order = ParentField.S1_Order;
			newFieldColumn.S1_BusinessContext = ParentField.S1_BusinessContext;

			ZShort largestOrderColumn = 0;

			foreach (StmSystemDefinedFieldColumn otherFieldColumn in this)
			{
				if (otherFieldColumn.S1_OrderColumn > largestOrderColumn)
				{
					largestOrderColumn = otherFieldColumn.S1_OrderColumn;
				}
			}

			newFieldColumn.S1_OrderColumn = largestOrderColumn + 1;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(StmSystemDefinedFieldSchema.S1_BusinessContext, ParentField.S1_BusinessContext);
			result.AddToFilter(StmSystemDefinedFieldSchema.S1_Order, ParentField.S1_Order);
			result.AddToFilter(StmSystemDefinedFieldSchema.S1_OrderColumn, SQLComparisonOperator.NotEqual, ZShort.Zero);
			return result;
		}

		internal readonly StmSystemDefinedField ParentField;
	}
}
