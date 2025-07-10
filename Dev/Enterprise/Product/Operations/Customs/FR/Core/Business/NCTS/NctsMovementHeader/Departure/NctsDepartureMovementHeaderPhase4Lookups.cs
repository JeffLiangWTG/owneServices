using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Business.CusAuthorisationHeaderCollection.FilterConstants;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsDepartureMovementHeaderPhase4Lookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Lookups, IFRNctsDepartureMovementHeaderLookups
	{
		public NctsDepartureMovementHeaderPhase4Lookups(NctsDepartureMovementHeader parent) : base(parent)
		{
		}

		public CusAuthorisationHeaderCollection AuthorizedLocationOfGoodsCodeList
		{
			get
			{
				ZString type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
				ZString country = Core.Constants.CountryCodes.France;
				var holder = Parent.Header?.Consignor?.Organisation?.PK ?? ZGuid.Empty;

				var filtered = new CusAuthorisationHeaderCollectionFiltered(Factory, type, holder);

				filtered.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(AuthorisationHolder, "Property", holder, false));
				filtered.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(AuthorisationType, "Property", type, false));
				filtered.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Country, "Property", country, false));

				return filtered;
			}
		}

		public CodeDescriptionPairList PaymentDestinationList => UniversalReferenceDataHelper.PaymentDestinationList(Parent.Factory, (Integration.Customs.FR.IHarbourJob)Parent);

		public ZString BarrierPort => Parent.Header.PortOfDispatch;

		public RefUNLOCOCollection PortOfPresentationList => null;
	}
}
