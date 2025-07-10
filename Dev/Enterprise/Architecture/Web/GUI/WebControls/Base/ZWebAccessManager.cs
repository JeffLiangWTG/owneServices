using System.Collections.Generic;
using Enterprise.Integration.Licensing;

namespace Enterprise.ZArchitecture.Web.GUI
{
	public abstract class ZWebAccessManager : IWebAccessManager
	{
		#region Constructors

		public ZWebAccessManager(ZGlobal appInstance)
		{
			this.appInstance = appInstance;
		}

		#endregion

		#region Properties

		public ZGlobal AppInstance
		{
			get
			{
				return appInstance;
			}
		}

		#endregion

		#region Methods

		public ILicenceCheckpoint[] LicenceCheckpoints(string pageRelativePage)
		{
			return GetLicenceCheckpoints(pageRelativePage);
		}

		public bool IsReportsPage(string pageRelativePath)
		{
			return IsReportsPageCore(pageRelativePath);
		}

		#endregion

		#region Implementation

		protected virtual bool IsReportsPageCore(string pageRelativePath)
		{
			return false;
		}

		protected ILicenceCheckpoint[] GetLicenceCheckpoints(string pageRelativePath)
		{
			List<ILicenceCheckpoint> result = GetLicenceCheckpointsCore(pageRelativePath);
			return result.ToArray();
		}

		protected virtual List<ILicenceCheckpoint> GetLicenceCheckpointsCore(string pageRelativePath)
		{
			List<ILicenceCheckpoint> result = new List<ILicenceCheckpoint>();
			return result;
		}

		readonly ZGlobal appInstance;

		#endregion
	}
}
