using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class ProductionBatchCollection : CusCodeDataCollection<ProductionBatch>
	{
		public ProductionBatchCollection(BusinessObject parent) : base(parent, Constants.CusCodeDataTypes.Codes.CIQ)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, CusCodeDataSchema.CY_Code, SQLComparisonOperator.Equal, Constants.CusCodeDataCode.BatchNumber);
			return result;
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			var invoiceLine = Master as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.Validation.ValidateManufactureDatesAsString();
			}
		}
	}
}
