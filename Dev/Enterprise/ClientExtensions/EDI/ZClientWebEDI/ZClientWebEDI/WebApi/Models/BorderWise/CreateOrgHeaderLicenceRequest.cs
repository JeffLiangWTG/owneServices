using System;
using System.Collections.Generic;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise
{
	public class CreateOrgHeaderLicenceRequest
	{
		public string ApiKey { get; set; }
		public IEnumerable<Guid> OrgHeaderPks { get; set; }
	}
}
