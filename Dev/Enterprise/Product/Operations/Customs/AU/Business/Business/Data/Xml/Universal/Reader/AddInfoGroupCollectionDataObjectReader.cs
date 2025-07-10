using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AddInfoGroupCollectionDataObjectReader : DataTransfer.Universal.AddInfoGroupCollectionDataObjectReader
	{
		internal AddInfoGroupCollectionDataObjectReader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, ZString[] addInfoTypesThatShouldNotBeImported = null, string dataContext = "")
			: base(logger, helper, addInfoTypesThatShouldNotBeImported, dataContext)
		{
		}

		public override IColumnIndexer[] ReadIntoDataRows(ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase, UniversalCustoms.IAddInfoGroupCollectionParent addInfoGroupCollectionParent)
		{
			var result = new List<IColumnIndexer>();
			var parentResult = base.ReadIntoDataRows(parentPK, parentTableCode, isParentInDatabase, addInfoGroupCollectionParent);
			if (parentResult != null)
			{
				result.AddRange(parentResult);
			}

			var addInfoGroupCollection = addInfoGroupCollectionParent.AddInfoGroupCollection;
			if (addInfoGroupCollection != null)
			{
				foreach (var groupElement in addInfoGroupCollection)
				{
					var row = new QuarantineLineDataObjectReader(groupElement, logger, helper, parentPK).ReadIntoDataRow();
					if (row != null)
					{
						result.AddRange(row);
					}
				}
			}
			return result.ToArray();
		}
	}
}
