using System;
using System.Collections.Generic;
using Enterprise.Integration.Licensing;

namespace Enterprise.ZArchitecture.Core
{
	public class LicensedComponentManager : IDisposable
	{
		List<ILicenceCheckpoint> checkpoints;
		readonly ILicensedComponent licensedComponent;

		public LicensedComponentManager(ILicensedComponent licensedComponent)
		{
			this.licensedComponent = licensedComponent;
		}

		public void Dispose()
		{
			if (checkpoints != null)
			{
				while (checkpoints.Count > 0)
				{
					checkpoints[0].Logout(licensedComponent);
				}
			}
		}

		internal int CheckpointCount => checkpoints?.Count ?? 0;

		internal bool AddLicenceCheckpointLoggedIn(ILicenceCheckpoint checkpoint)
		{
			if (checkpoints == null)
			{
				checkpoints = new List<ILicenceCheckpoint>();
			}
			else if (checkpoints.Contains(checkpoint))
			{
				return false;
			}

			checkpoints.Add(checkpoint);
			return true;
		}

		public bool ContainsCheckpoint(ILicenceCheckpoint checkpoint) => checkpoints?.Contains(checkpoint) ?? false;
		internal bool RemoveLicenceCheckpointLoggedIn(ILicenceCheckpoint checkpoint) => checkpoints?.Remove(checkpoint) ?? false;
	}
}
