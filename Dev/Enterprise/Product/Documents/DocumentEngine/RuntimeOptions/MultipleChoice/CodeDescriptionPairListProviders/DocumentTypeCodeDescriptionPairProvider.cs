using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class DocumentTypeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			ZQuery query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.Equal, Enterprise.Core.Constants.ReferenceTypes.All);
			if (!DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				query.AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics);
				query.AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship);
			}

			query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_IsActive, SQLComparisonOperator.Equal, "Y");
			query.OrderBy = RefDocTypeSchema.RT_Desc.Name;
			BusinessObjectFactory factory = new BusinessObjectFactory();
			RefDocType[] refdoc = factory.Load<RefDocType>(query);

			foreach (RefDocType docType in refdoc)
			{
				result.AddPair(docType.RT_DocType, docType.RT_DescMultilingual);
			}
			return result;
		}
	}
}
