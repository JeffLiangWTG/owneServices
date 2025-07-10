using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public abstract class CustomsReferenceCollectionCreatorFrom
	{
		public CustomsReferenceCollectionCreatorFrom(UniversalDataObjectWriterHelper helper, BusinessObject bizObj, IDataWritingManager writeManager, string dataContext)
		{
			this.helper = helper;
			this.bizObj = bizObj;
			this.dataContext = dataContext;
			this.writeManager = writeManager;
		}
		protected readonly UniversalDataObjectWriterHelper helper;
		protected readonly string dataContext;
		protected readonly BusinessObject bizObj;
		protected readonly IDataWritingManager writeManager;

		protected UniversalCustoms.CustomsReference Create(BusinessObject bizOBj)
		{
			return CreateCustomsReference(bizOBj);
		}

		protected abstract UniversalCustoms.CustomsReference CreateCustomsReference(BusinessObject bizObj);

		public List<UniversalCustoms.CustomsReference> CreateCollection()
		{
			List<UniversalCustoms.CustomsReference> result = new List<UniversalCustoms.CustomsReference>();
			if (bizObj != null)
			{
				ZGuid bizObjPK = bizObj.PK;
				if (bizObjPK.IsValid)
				{
					var tablePrefix = bizObj.TablePrefix;
					result = GetCustomReferences(tablePrefix, bizObjPK);
				}
			}
			return result;
		}

		protected abstract List<UniversalCustoms.CustomsReference> GetCustomReferences(ZString tablePrefix, ZGuid bizObjPK);
	}
}
