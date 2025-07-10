using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class AccQueryClaimModuleTest : ZModuleBasherTest
	{
		#region Properties

		public void TestGetNewGridCollection()
		{
			using (IQueryClaimForTest module = GetModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is AccQueryClaimCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (IQueryClaimForTest module = GetModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is AccQueryClaimFilterBusinessObject);
			}
		}

		#endregion

		protected abstract IQueryClaimForTest GetModuleForTest();
		protected abstract AccQueryClaimModule GetModuleInstance();

		#region AccQueryClaimModuleForTest

		public interface IQueryClaimForTest : IDisposable
		{
			IFilterControl NewFilterControl { get; }
			IBusinessObjectCollection NewGridCollection { get; }
			FilterBusinessObject NewFilterBusinessObject { get; }
		}

		#endregion

		public void TestHasNoDeleteMenuItemText()
		{
			using (AccQueryClaimModule module = GetModuleInstance())
			{
				var deleteMenuItem = module.FormActionMenu.FindByText("Cancel") as ZMenuItem;
				AssertNull(deleteMenuItem);
			}
		}
	}
}
