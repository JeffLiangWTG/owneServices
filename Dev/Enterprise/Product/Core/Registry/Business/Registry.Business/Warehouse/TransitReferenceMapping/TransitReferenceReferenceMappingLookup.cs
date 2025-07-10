using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Warehouse
{
	public class TransitReferenceMappingLookups : ZLookups
	{
		public TransitReferenceMappingLookups(TransitReferenceMapping parent) : base(parent)
		{
		}
		new TransitReferenceMapping Parent => (TransitReferenceMapping)base.Parent;

		public CodeDescriptionPairList SourceReferenceCategoryList => GetReferenceCategoryList();

		public CodeDescriptionPairList TargetReferenceCategoryList => GetReferenceCategoryList();

		public CodeDescriptionPairList SourceReferenceTypeList => GetReferenceTypeList(Parent.SourceCategory);

		public CodeDescriptionPairList TargetReferenceTypeList => GetReferenceTypeList(Parent.TargetCategory);

		public CodeDescriptionPairList DirectionList => GetDirectionList();

		static CodeDescriptionPairList GetReferenceCategoryList()
		{
			var categoryList = new CodeDescriptionPairList();
			categoryList.AddPair(TransitWarehouseReferenceCategories.Codes.AdditionalReference, TransitWarehouseReferenceCategories.Descriptions.AdditionalReference);
			categoryList.AddPair(TransitWarehouseReferenceCategories.Codes.CustomsReference, TransitWarehouseReferenceCategories.Descriptions.CustomsReference);
			categoryList.AddPair(TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehouseReferenceCategories.Descriptions.PortReference);

			return categoryList;
		}

		static CodeDescriptionPairList GetDirectionList()
		{
			var directionList = new CodeDescriptionPairList();
			directionList.AddPair(TransitWarehouseConsignmentDirections.Codes.Import, TransitWarehouseConsignmentDirections.Descriptions.Import);
			directionList.AddPair(TransitWarehouseConsignmentDirections.Codes.Export, TransitWarehouseConsignmentDirections.Descriptions.Export);
			directionList.AddPair(TransitWarehouseConsignmentDirections.Codes.Domestic, TransitWarehouseConsignmentDirections.Descriptions.Domestic);

			return directionList;
		}

		static CodeDescriptionPairList GetReferenceTypeList(ZString category)
		{
			var result = new CodeDescriptionPairList();
			switch (category)
			{
				case TransitWarehouseReferenceCategories.Codes.AdditionalReference:
					var warehouseRegistry = WarehouseDataRegistry.Instance;
					var codeDescriptionPairs = warehouseRegistry.AdditionalReferenceType.Value.GetCodeDescriptionPairList();
					foreach (CodeDescriptionPair pair in codeDescriptionPairs)
					{
						result.AddPair(pair.Code, pair.Description);
					}
					break;
				case TransitWarehouseReferenceCategories.Codes.CustomsReference:
					result.AddPair(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, TransitWarehouseCustomsReferenceTypes.Descriptions.CustomsNumber);
					result.AddPair(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, TransitWarehouseCustomsReferenceTypes.Descriptions.CustomsReleaseNumber);
					break;
				case TransitWarehouseReferenceCategories.Codes.PortReference:
					result.AddPair(TransitWarehousePortReferenceTypes.Codes.PortAuthority, TransitWarehousePortReferenceTypes.Descriptions.PortAuthority);
					break;
			}

			return result;
		}
	}
}
