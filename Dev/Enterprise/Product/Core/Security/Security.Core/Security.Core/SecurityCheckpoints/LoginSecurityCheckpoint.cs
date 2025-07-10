using System;
using CargoWise.Application;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Security.Core.ResString;

namespace Enterprise.Security
{
	public class LoginSecurityCheckpoint : SecurityCheckpoint
	{
		public LoginSecurityCheckpoint(IUserContext userContext)
			: base((NoResString)"Login", ResString.GetMultilingualString("eabe2831-bad2-42b4-aa34-ba0fed7e9251", "Login Branches and Departments"), null,
			ObjectFactory.Get<IZSecurityFactory>().NewSecurityInstance(null,
				userContext.User,
				userContext.Branch != null ? userContext.Branch.PK : Guid.Empty,
				userContext.Department != null ? userContext.Department.PK : Guid.Empty,
				userContext.Company != null ? userContext.Company.PK : Guid.Empty))
		{
			this.user = userContext.User;
		}

		readonly IUser user;

		public override bool IsAllowed
		{
			get { return (user != null && !user.IsOperational) || base.IsAllowed; }
		}
	}
}
