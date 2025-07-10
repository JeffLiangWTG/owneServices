using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISeaCargoReportHeader : ICargoReportHeader
	{
		ZString HouseBill { get; }
		ZString ParentBill { get; }
		ZString OceanBill { get; }
		ZString Voyage { get; }
		ZString LloydsNumber { get; }
		OrgHeader NotifyParty { get; }
		ZString OriginCountry { get; }
		bool IsConsolidation { get; }
		bool IsBureau { get; }
		ZString PrincipalID { get; }
		BusinessObject BusinessObject { get; }
		ISeaCargoReportLine[] Lines { get; }
		ISeaCargoReportLine[] DatabaseLines { get; }
	}
}
