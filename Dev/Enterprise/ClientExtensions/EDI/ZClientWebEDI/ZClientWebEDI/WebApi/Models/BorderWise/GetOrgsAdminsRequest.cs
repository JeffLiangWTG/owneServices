using System;
using System.Collections.Generic;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise
{
	public class GetOrgsAdminsRequest
	{
		public string ApiKey { get; set; }
		public List<Guid> OrgPks { get; set; }
	}
}
