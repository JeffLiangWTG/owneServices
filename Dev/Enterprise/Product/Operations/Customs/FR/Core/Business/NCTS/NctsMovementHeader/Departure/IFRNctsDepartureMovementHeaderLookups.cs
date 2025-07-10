using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public interface IFRNctsDepartureMovementHeaderLookups : EU.NCTS.Business.INctsDepartureMovementHeaderLookups
	{
		CodeDescriptionPairList PaymentDestinationList { get; }

		CusAuthorisationHeaderCollection AuthorizedLocationOfGoodsCodeList { get; }

		ZString BarrierPort { get; }

		RefUNLOCOCollection PortOfPresentationList { get; }
	}
}
