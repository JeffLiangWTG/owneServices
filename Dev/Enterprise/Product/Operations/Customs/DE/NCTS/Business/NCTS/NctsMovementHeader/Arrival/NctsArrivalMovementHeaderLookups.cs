using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsArrivalMovementHeaderLookups : EU.NCTS.Business.NctsArrivalMovementHeaderLookups
	{
		public NctsArrivalMovementHeaderLookups(NctsArrivalMovementHeader parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList PortsOfUnloadingList
		{
			get
			{
				var parent = Parent;
				var header = parent.Header;
				var officeCode = parent.DestinationCustomsOfficeCodeForArrival;
				var destinationTraderPk = header.DestinationTrader.Organisation?.PK ?? ZGuid.Empty;
				return CusAuthorizationHelper.GetCachedRuleValues(Factory
					, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit
					, destinationTraderPk
					, Customs.Business.CusAuthorisationRuleTypeList.Codes.Location
					, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities
					, RefCusCodeListAttributeTypes.Codes.CustomsOffice
					, officeCode);
			}
		}

		protected override CodeDescriptionPairList NctsTransitStatusListCore =>
			Factory.GetCachedValue("DE.NctsArrivalMovementHeaderLookups.NctsTransitStatusList",
				() =>
				{
					var lookupsNctsTransitStatusList = new CodeDescriptionPairList(base.NctsTransitStatusListCore);
					lookupsNctsTransitStatusList.AddPairIfNotExist(Enterprise.Customs.DE.NCTS.Business.NctsTransitStatusList.Codes.DeclarationDataRequested, Enterprise.Customs.DE.NCTS.Business.NctsTransitStatusList.Descriptions.DeclarationDataRequested);
					lookupsNctsTransitStatusList.AddPairIfNotExist(Enterprise.Customs.DE.NCTS.Business.NctsTransitStatusList.Codes.ControlResultCaptured, Enterprise.Customs.DE.NCTS.Business.NctsTransitStatusList.Descriptions.ControlResultCaptured);
					return lookupsNctsTransitStatusList;
				});
	}
}
