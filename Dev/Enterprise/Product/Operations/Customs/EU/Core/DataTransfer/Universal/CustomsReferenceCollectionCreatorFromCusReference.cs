using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsReferenceCollectionCreatorFromCusReference : CustomsReferenceCollectionCreatorFrom
	{
		public CustomsReferenceCollectionCreatorFromCusReference(UniversalDataObjectWriterHelper helper, BusinessObject bizObj, IDataWritingManager writeManager, string dataContext)
			: base(helper, bizObj, writeManager, dataContext)
		{
		}

		protected override UniversalCustoms.CustomsReference CreateCustomsReference(BusinessObject bizObj)
		{
			var cusReference = bizObj as CusReference;
			return cusReference != null ? new UniversalCustoms.CustomsReference()
			{
				Type = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair>(cusReference.CFR_Type, new CusReferenceTypeList()),
				SubType = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair35Char>(cusReference.CFR_Code, ((CommonCusReferenceLookups)cusReference.Lookups).CodeList),
				Reference = cusReference.CFR_Reference,
				Owner = new OrganizationDataObjectWriter(writeManager, AddressTypes.Owner).GetDataObject(cusReference.Owner)
			} : new UniversalCustoms.CustomsReference();
		}

		protected override List<UniversalCustoms.CustomsReference> GetCustomReferences(ZString tablePrefix, ZGuid bizObjPK)
		{
			var result = new List<UniversalCustoms.CustomsReference>();
			var supportedTypes = helper.GetSupportedCusReferenceCFR_TypesFor(tablePrefix, dataContext);
			if (supportedTypes != null && supportedTypes.Length > 0)
			{
				var query = new ZQuery(CusReferenceSchema.CFR_ParentID, bizObjPK);
				query.AddToFilter(CusReferenceSchema.CFR_ParentTableCode, tablePrefix);
				query.AddToFilter(CusReferenceSchema.CFR_Type, supportedTypes);
				var cusReferences = helper.Load<CusReference>(query);
				result = new List<UniversalCustoms.CustomsReference>(cusReferences.Select(x => Create(x)));
			}

			return result;
		}
	}
}
