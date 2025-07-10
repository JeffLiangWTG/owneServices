using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public interface IEdiCommissionAgreementCustomization : IBusiness, ICommissionAgreementRelated<EdiCommissionAgreementCustomization>
	{
	}
}
