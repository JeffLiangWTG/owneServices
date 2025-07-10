using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	/// <summary>
	/// Setups up Environment to use Branch temporarily
	/// </summary>
	public class WebLoginBranch : IDisposable
	{
		public WebLoginBranch(IBranch branch)
		{
			if (branch != Env.CurrentBranch)
			{
				userContextChange = Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, branch.PK, Env.CurrentDepartment.PK));
			}
		}

		readonly IDisposable userContextChange;

		#region IDisposable Members

		public void Dispose()
		{
			if (userContextChange != null)
			{
				userContextChange.Dispose();
			}
		}

		#endregion
	}
}
