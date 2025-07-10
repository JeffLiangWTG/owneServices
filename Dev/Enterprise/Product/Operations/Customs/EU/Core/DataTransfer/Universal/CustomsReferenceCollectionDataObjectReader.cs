using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsReferenceCollectionDataObjectReader : Customs.DataTransfer.Universal.CustomsReferenceCollectionDataObjectReader, Integration.Customs.EU.ICustomsReferenceCollectionDataObjectReader
	{
		public CustomsReferenceCollectionDataObjectReader(IXmlImportLogger logger, ICustomsReferenceCollectionReaderHelper helper, string dataContext = "")
			: base(logger, helper, dataContext)
		{
		}

		protected override IEnumerable<IColumnIndexer> ReadIntoCusReference(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, List<UniversalCustoms.CustomsReference> customsReferenceCollection, ZString[] supportedTypes)
		{
			var newRows = new List<IColumnIndexer>();
			if (supportedTypes != null && supportedTypes.Length > 0)
			{
				var query = new ZQuery(CusReferenceSchema.CFR_ParentID, parentPK);
				query.AddToFilter(CusReferenceSchema.CFR_ParentTableCode, parentTableCode);
				query.AddToFilter(CusReferenceSchema.CFR_Type, supportedTypes);
				query.FetchOnlyFromLocalCache = isParentInDatabase.HasValue && !isParentInDatabase.Value;
				var existingCusReferences = helper.Factory.Load<CusReference>(query);
				existingCusReferences.DeleteAll(true);

				foreach (var customsReference in customsReferenceCollection.Where(x => supportedTypes.Contains(x.Type.Code ?? ZString.Empty)))
				{
					var row = new CustomsReferenceDataObjectReader(customsReference, logger, parentPK, parentTableCode, helper.Factory).ReadIntoDataRowCusReference();
					if (row != null)
					{
						newRows.Add(row);
					}
				}
			}
			return newRows;
		}

		protected override IEnumerable<IColumnIndexer> ReadIntoCusAuthorizationUsage(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, List<UniversalCustoms.CustomsReference> customsReferenceCollection)
		{
			var newRows = new List<IColumnIndexer>();
			var query = new ZQuery(CusAuthorizationUsageSchema.AGC_ParentID, parentPK);
			query.AddToFilter(CusAuthorizationUsageSchema.AGC_ParentTableCode, parentTableCode);
			query.FetchOnlyFromLocalCache = isParentInDatabase.HasValue && !isParentInDatabase.Value;
			var existingCusAuthorizationUsages = helper.Factory.Load<CusAuthorizationUsage>(query);
			existingCusAuthorizationUsages.DeleteAll(true);

			foreach (var customsReference in customsReferenceCollection.Where(x => (x.Type.Code ?? ZString.Empty) == CusReferenceTypeCodes.AUT))
			{
				var row = new CusAuthorizationUsageDataObjectReader(customsReference, logger, parentPK, parentTableCode, helper.Factory).ReadIntoDataRowCusReference();
				if (row != null)
				{
					newRows.Add(row);
				}
			}
			return newRows;
		}
	}
}
