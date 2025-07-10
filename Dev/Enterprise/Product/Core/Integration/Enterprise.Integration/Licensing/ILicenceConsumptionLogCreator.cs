using System;
using CargoWise.EntityFramework;

namespace Enterprise.Integration.Licensing
{
	public interface ILicenceConsumptionLogCreator
	{
		void CreateLog(ILicenceCheckpoint licenceCheckpoint);
		void CreateLog(ILicenceCheckpoint licenceCheckpoint, bool runInForeground, BusinessObjectFactory factory = null);
		void CreateLog(ILicenceCheckpoint licenceCheckpoint, DateTime utcNow);
		void CreateLog(ILicenceCheckpoint licenceCheckpoint, string deviceID, string deviceDetails, int keyStrokes);
	}
}
