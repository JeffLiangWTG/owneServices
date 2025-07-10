using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestsSubclassesOf(typeof(ZPopupModule), typeof(TestExcludeZFilterGridModulesAllHaveModuleBashersAttribute), ExcludeClientDlls = true)]
	public abstract class ZPopupModuleBasherTest : BasherTest
	{
		[RequiresSTA]
		public void TestUserDefinedFilters_ShouldBeDisabled()
		{
			try
			{
				using (var module = (ZPopupModule)ZModule.GetZModule(ModuleID))
				using (var form = (Form)module.ShowPopup())
				{
					if (form != null)
					{
						var controls = form.FindAll<StripControl>();

						foreach (var control in controls)
						{
							var filterBizo = control.FilterBusinessObject;
							AssertEquals("Only ZFilterModules should support User-Defined filters... SAD!", false, filterBizo.SupportsUserDefinedFilters);
						}
					}
				}
			}
			catch (ModuleGuiNotSupportedException)
			{
			}

			Assert(true);
		}

		#region Implementation

		protected abstract ModuleIdentifier GetModuleID();

		public ModuleIdentifier ModuleID => GetModuleID();

		public Type ModuleToBashType => GetModuleToBashType();

		protected virtual Type GetModuleToBashType()
		{
			using (var moduleForType = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				return moduleForType.GetType();
			}
		}

		ZPopupModule moduleToBash;

		public override Form GetFormToBash()
		{
			moduleToBash?.Dispose();

			var id = GetModuleID();

			moduleToBash = (ZPopupModule)ZModuleFactory.Instance.Create(id);

			if (moduleToBash == null)
			{
				Fail("Unable to get module for ID : " + id);
			}
			if (!BashType.IsInstanceOfType(moduleToBash))
			{
				//allow module to be of BashType.BaseType if BashType.Name is same as BaseType.Name + ForTest
				//allow module to be test extraction accessor
				if (BashType.Name != BashType.BaseType?.Name + "ForTest" || !BashType.BaseType.IsInstanceOfType(moduleToBash))
				{
					throw new InvalidOperationException("BashType must be consistent with the moduleToBash");
				}
			}

			return GetFormToBashCore(moduleToBash);
		}

		protected virtual Form GetFormToBashCore(ZPopupModule module)
		{
			return (Form)moduleToBash.ShowPopup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			moduleToBash?.Dispose();
		}

		#endregion
	}
}
