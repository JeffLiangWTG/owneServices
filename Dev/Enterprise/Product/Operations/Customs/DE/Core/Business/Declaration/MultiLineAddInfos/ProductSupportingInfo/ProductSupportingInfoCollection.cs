using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public class ProductSupportingInfoCollection : CusSupportingInfoCollection<ProductSupportingInfo>
	{
		public ProductSupportingInfoCollection(CusEntryInstruction entryInstruction)
			: base(entryInstruction, ProductSupportingInfo.CusSupportingInfoType)
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}

		const int MaxCountForValidation = 999;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, CusSupportingInfoSchema.CSI_Type, SQLComparisonOperator.Equal, ProductSupportingInfo.CusSupportingInfoType);
			return result;
		}
	}
}
