using System.Collections.Generic;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise
{
	public class CreateOrgHeaderLicenceResult
	{
		public IEnumerable<OrgHeaderLicenceDetail> EdiProdLicences { get; set; }
		public string ErrorMessage { get; set; }
	}
}
