using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.VisualBoards.Business
{
	public sealed class VisualBoardLicensedComponent : ILicensedComponent, IDisposable
	{
		public VisualBoardLicensedComponent()
		{
			checkpoint = GetBMLicencePoint();
		}

		IDisposable ILicensedComponent.LicensedComponentManager => licensedComponentManager ?? (licensedComponentManager = new LicensedComponentManager(this));
		LicensedComponentManager licensedComponentManager;
		readonly ILicenceCheckpoint checkpoint;

		public void Dispose()
		{
			((ILicensedComponent)this).LicensedComponentManager.Dispose();
		}

		public void Login()
		{
			checkpoint.Login(this);
		}

		static LicenceCheckpoint GetBMLicencePoint()
		{
			return Env.Licence.BufferManagement;
		}

#if DEBUG

		public static void ResetBMLicencing_ForTest()
		{
			// This is needed because Env.Licencing is static. And we don't reset static variables between runs but we do clean the db...
			GetBMLicencePoint().SetLastConsumptionLogCreatedForTest(ZDateTime.Empty);
		}

#endif
	}
}
