using System;
using System.Collections.Generic;

namespace Enterprise.Client.EDI.Web.Admin
{
	class UpdatePasswordRequest
	{
		public string ApiKey { get; set; }
		public byte[] PasswordHash { get; set; }
		public byte[] PasswordSalt { get; set; }
		public int PasswordHashIterations { get; set; }
		public List<Guid> ContactPks { get; set; }
	}
}
