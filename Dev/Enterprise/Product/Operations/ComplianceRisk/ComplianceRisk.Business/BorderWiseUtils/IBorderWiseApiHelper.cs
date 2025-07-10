using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.ComplianceRisk.Business
{
	public interface IBorderWiseApiHelper
	{
		Task<ICollection<ComplianceCheckResponseModel>> ComplianceCheckAsync(IEnumerable<ComplianceCheckRequestModel> complianceCheckRequests, CancellationToken cancellationToken);

		Task<SupportedCountriesCheckResponseModel> SupportedCountriesCheckAsync(CancellationToken cancellationToken);
	}
}
