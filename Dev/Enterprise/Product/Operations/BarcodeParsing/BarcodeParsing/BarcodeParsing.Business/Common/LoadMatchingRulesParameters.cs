using CargoWise.Types;

namespace Enterprise.BarcodeParsing.Business
{
	public record LoadMatchingRulesParameters
	{
		public ZString ModuleCode;
		public ZGuid BuyerPK;
		public ZGuid SupplierPK;
		public ZGuid RelatedEntityPK;
		public bool IsBuyerRequiredForRelatedEntity;
		public bool IsGS1;

		public LoadMatchingRulesParameters(
			ZString moduleCode,
			ZGuid buyerPK,
			ZGuid supplierPK,
			ZGuid relatedEntityPK,
			bool isBuyerRequiredForRelatedEntity,
			bool isGS1)
		{
			ModuleCode = moduleCode;
			BuyerPK = buyerPK;
			SupplierPK = supplierPK;
			RelatedEntityPK = relatedEntityPK;
			IsBuyerRequiredForRelatedEntity = isBuyerRequiredForRelatedEntity;
			IsGS1 = isGS1;
		}
	}
}
