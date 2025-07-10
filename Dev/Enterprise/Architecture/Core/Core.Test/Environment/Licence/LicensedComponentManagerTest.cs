using System;
using Enterprise.Integration.Licensing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class LicensedComponentManagerTest : TestCase
	{
		public void TestEverything()
		{
			MockLicenceCheckpoint checkpoint1 = new MockLicenceCheckpoint("checkpoint1");
			MockLicenceCheckpoint checkpoint2 = new MockLicenceCheckpoint("checkpoint2");
			MockLicensedComponent licensedComponent = new MockLicensedComponent();

			AssertEquals("CheckpointCount", 0, licensedComponent.LicensedComponentManager.CheckpointCount);

			AssertEquals(false, licensedComponent.LicensedComponentManager.RemoveLicenceCheckpointLoggedIn(checkpoint1));
			AssertEquals("CheckpointCount", 0, licensedComponent.LicensedComponentManager.CheckpointCount);
			AssertEquals("ContainsCheckpoint(checkpoint1)", false, licensedComponent.LicensedComponentManager.ContainsCheckpoint(checkpoint1));

			AssertEquals(true, licensedComponent.LicensedComponentManager.AddLicenceCheckpointLoggedIn(checkpoint1));
			AssertEquals(false, licensedComponent.LicensedComponentManager.AddLicenceCheckpointLoggedIn(checkpoint1));
			AssertEquals("CheckpointCount", 1, licensedComponent.LicensedComponentManager.CheckpointCount);
			AssertEquals("ContainsCheckpoint(checkpoint1)", true, licensedComponent.LicensedComponentManager.ContainsCheckpoint(checkpoint1));
			AssertEquals("ContainsCheckpoint(checkpoint2)", false, licensedComponent.LicensedComponentManager.ContainsCheckpoint(checkpoint2));

			AssertEquals(true, licensedComponent.LicensedComponentManager.AddLicenceCheckpointLoggedIn(checkpoint2));
			AssertEquals("CheckpointCount", 2, licensedComponent.LicensedComponentManager.CheckpointCount);
			AssertEquals("ContainsCheckpoint(checkpoint1)", true, licensedComponent.LicensedComponentManager.ContainsCheckpoint(checkpoint1));
			AssertEquals("ContainsCheckpoint(checkpoint2)", true, licensedComponent.LicensedComponentManager.ContainsCheckpoint(checkpoint2));

			AssertEquals(true, licensedComponent.LicensedComponentManager.RemoveLicenceCheckpointLoggedIn(checkpoint2));
			AssertEquals(false, licensedComponent.LicensedComponentManager.RemoveLicenceCheckpointLoggedIn(checkpoint2));
			AssertEquals("CheckpointCount", 1, licensedComponent.LicensedComponentManager.CheckpointCount);
			AssertEquals("ContainsCheckpoint(checkpoint1)", true, licensedComponent.LicensedComponentManager.ContainsCheckpoint(checkpoint1));
			AssertEquals("ContainsCheckpoint(checkpoint2)", false, licensedComponent.LicensedComponentManager.ContainsCheckpoint(checkpoint2));

			AssertEquals(true, licensedComponent.LicensedComponentManager.AddLicenceCheckpointLoggedIn(checkpoint2));
			licensedComponent.LicensedComponentManager.Dispose();
			AssertEquals("CheckpointCount", 0, licensedComponent.LicensedComponentManager.CheckpointCount);
			AssertEquals("ContainsCheckpoint(checkpoint1)", false, licensedComponent.LicensedComponentManager.ContainsCheckpoint(checkpoint1));
			AssertEquals("ContainsCheckpoint(checkpoint2)", false, licensedComponent.LicensedComponentManager.ContainsCheckpoint(checkpoint2));
		}

		#region MockLicenceCheckpoint

		class MockLicenceCheckpoint : ILicenceCheckpoint
		{
			public MockLicenceCheckpoint(string name)
			{
				this.name = name;
			}

			#region ILicenceCheckpoint Members

			string ILicenceCheckpoint.Name
			{
				get { return name; }
			}
			readonly string name;

			string ILicenceCheckpoint.DisplayName
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			ILicenceCheckpoint ILicenceCheckpoint.ParentCheckpoint
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			string ILicenceCheckpoint.LastReasonForNotAllowing
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			ModuleLicenceType ILicenceCheckpoint.LicenceType
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			LicenceLoginResponse ILicenceCheckpoint.Login(ILicensedComponent licensedComponent)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			void ILicenceCheckpoint.Logout(ILicensedComponent licensedComponent)
			{
				((LicensedComponentManager)licensedComponent.LicensedComponentManager).RemoveLicenceCheckpointLoggedIn(this);
			}

			#endregion

			public string LicenceMissingMessage
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public ILicenceCheckpointUserContext ParentUserContext
			{
				get { throw new NotImplementedException(); }
			}
		}

		#endregion

		#region MockLicensedComponent

		class MockLicensedComponent : ILicensedComponent
		{
			LicensedComponentManager licensedComponentManager;

			public MockLicensedComponent()
			{
			}

			public LicensedComponentManager LicensedComponentManager
			{
				get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
			}

			IDisposable ILicensedComponent.LicensedComponentManager
			{
				get { return licensedComponentManager ?? (licensedComponentManager = new LicensedComponentManager(this)); }
			}
		}

		#endregion
	}
}
