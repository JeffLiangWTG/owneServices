using System;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise
{
	public class OrgHeaderLicenceDetail
	{
		public Guid OrgHeaderPk { get; set; }
		public int CompanyNumber { get; set; }
		public int DatabaseNumber { get; set; }
		public bool IsFirstLicenceDatabase { get; set; }
		public bool HasInactiveLicenceDatabase { get; set; }
	}
}
