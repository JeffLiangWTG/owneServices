using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsReferenceCollectionCreatorFromCusAuthorizationUsage : CustomsReferenceCollectionCreatorFrom
	{
		public CustomsReferenceCollectionCreatorFromCusAuthorizationUsage(UniversalDataObjectWriterHelper helper, BusinessObject bizObj, IDataWritingManager writeManager, string dataContext)
			: base(helper, bizObj, writeManager, dataContext)
		{
		}

		protected override UniversalCustoms.CustomsReference CreateCustomsReference(BusinessObject bizObj)
		{
			UniversalCustoms.CustomsReference result = null;
			var cusAuthorizationUsage = bizObj as CusAuthorizationUsage;

			if (cusAuthorizationUsage != null)
			{
				var countryCode = cusAuthorizationUsage.Instruction?.JobDeclaration.CountryCode ?? cusAuthorizationUsage.InvoiceLine?.Declaration?.CountryCode ?? ZString.Empty;
				var provider = Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(countryCode);
				var authorizationTypeList = provider?.GetAuthorisationTypeList(cusAuthorizationUsage.Factory) ?? new ZArchitecture.Core.CodeDescriptionPairList();

				result = new UniversalCustoms.CustomsReference()
				{
					Type = new UniversalShipment.CodeDescriptionPair() { Code = CusReferenceTypeCodes.AUT, Description = CusReferenceTypeDescriptions.AUT },
					SubType = ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair35Char>(cusAuthorizationUsage.AGC_Code, authorizationTypeList),
					Reference = cusAuthorizationUsage.EffectiveReferenceNumber,
					Owner = new OrganizationDataObjectWriter(writeManager, AddressTypes.Owner).GetDataObject(cusAuthorizationUsage.Owner?.MainAddress)
				};
			}

			return result;
		}

		protected override List<UniversalCustoms.CustomsReference> GetCustomReferences(ZString tablePrefix, ZGuid bizObjPK)
		{
			var query = new ZQuery(CusAuthorizationUsageSchema.AGC_ParentID, bizObjPK);
			query.AddToFilter(CusAuthorizationUsageSchema.AGC_ParentTableCode, tablePrefix);
			return helper.Load<CusAuthorizationUsage>(query).Select(x => Create(x)).ToList();
		}
	}
}
