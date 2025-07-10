using System;
using Enterprise.Licensing;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestsSubclassesOf(typeof(GlowOnlyModule))]
	public abstract class GlowOnlyModuleTest<T> : ZModuleBasherTest
		where T : GlowOnlyModule, new()
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = new T())
			{
				AssertEquals(GetModuleID(), module.ID);
			}
		}

		#endregion

		#region TestFilterBusinessObject

		public void TestFilterBusinessObject()
		{
			using (var module = new T())
			{
				AssertEquals(true, module.FilterBusinessObject.GetType().IsAssignableFrom(ExpectedFilterBusinessObjectType));
			}
		}

		protected abstract Type ExpectedFilterBusinessObjectType { get; }

		#endregion

		#region TestFilterControl

		[RequiresSTA]
		public void TestFilterControl()
		{
			using (var module = new T())
			{
				using (var controlForTest = (IDisposable)module.GetNewFilterControlForGrid())
				{
					AssertEquals(true, controlForTest.GetType().IsAssignableFrom(ExpectedFilterControlType));
				}
			}
		}

		protected abstract Type ExpectedFilterControlType { get; }

		#endregion

		#region TestGridCollection

		public void TestGridCollection()
		{
			using (var module = new T())
			{
				AssertEquals(true, module.GridCollection.GetType().IsAssignableFrom(ExpectedCollectionType));
			}
		}

		protected abstract Type ExpectedCollectionType { get; }

		#endregion

		#region TestLicenseCheckpoint

		public void TestLicenseCheckpoint()
		{
			using (var module = new T())
			{
				AssertEquals(module.LicenceCheckPoint, ExpectedLicenceCheckPoint);
			}
		}

		protected abstract LicenceCheckpoint ExpectedLicenceCheckPoint { get; }

		#endregion

		#region TestSecurityCheckpoint

		public void TestSecurityCheckpoint()
		{
			using (var module = new T())
			{
				AssertEquals(ExpectedSecurityCheckPoint, module.SecurityCheckpoint);
			}
		}

		protected abstract SecurityCheckpoint ExpectedSecurityCheckPoint { get; }

		#endregion

		#region TestSupportsWorkflow

		public void TestSupportsWorkflow()
		{
			using (var module = new T())
			{
				AssertEquals(ExpectedSupportsWorkflow, module.SupportsWorkflow);
			}
		}

		protected virtual bool ExpectedSupportsWorkflow => false;

		#endregion

		#region TestAllowView

		public void TestAllowView()
		{
			using (var module = new T())
			{
				AssertEquals(ExpectedAllowView, module.AllowView);
			}
		}

		protected virtual bool ExpectedAllowView => true;

		#endregion

		#region TestAllowEdit

		public void TestAllowEdit()
		{
			using (var module = new T())
			{
				AssertEquals(ExpectedAllowEdit, module.AllowEdit);
			}
		}

		protected virtual bool ExpectedAllowEdit => true;

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			using (var module = new T())
			{
				AssertEquals(ExpectedAllowNew, module.AllowNew);
			}
		}

		protected virtual bool ExpectedAllowNew => false;

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var module = new T())
			{
				AssertEquals(ExpectedAllowDelete, module.AllowDelete);
			}
		}

		protected virtual bool ExpectedAllowDelete => false;

		#endregion
	}
}
