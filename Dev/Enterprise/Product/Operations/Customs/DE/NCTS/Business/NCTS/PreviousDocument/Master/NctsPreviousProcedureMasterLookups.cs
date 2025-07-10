using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsPreviousProcedureMasterLookups : ZLookups
	{
		public NctsPreviousProcedureMasterLookups(NctsPreviousProcedureMaster parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ProcedureList => Factory.GetCachedValue<NctsPreviousProcedureList>();

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList
		{
			get
			{
				var attributeFilterList = new List<RefCusCodeListAttributeFilter>() { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.MainCustomsOffice, JoinCondition.And, false, null, "True") };// param value
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Germany, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice }, ZDateTime.Now, attributeFilterList, false);
			}
		}

		public CodeDescriptionPairList AuthorizationNumberList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var nctsHeader = Parent.Parent.NctsHeader;
				var consignor = nctsHeader.Consignor;
				var principal = nctsHeader.Principal;
				var transactionDate = ZDate.Today;
				var procedure = Parent.CSI_Procedure;
				if (procedure == NctsPreviousProcedureList.Codes._9DEY)
				{
					result = CusAuthorizationHelper.GetCachedAuthorizationNumbers(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.InwardProcessing }, new ZGuid[] { consignor.OrganisationPK, principal.OrganisationPK }, transactionDate);
				}
				else if (procedure == NctsPreviousProcedureList.Codes._9DEZ)
				{
					result = CusAuthorizationHelper.GetCachedAuthorizationNumbersForAddresses(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP },
						new ZGuid[] { consignor.E2_OA_Address, principal.E2_OA_Address }, transactionDate);
				}
				return result;
			}
		}

		protected new NctsPreviousProcedureMaster Parent => (NctsPreviousProcedureMaster)base.Parent;
	}
}
