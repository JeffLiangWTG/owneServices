using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestsSubclassesOf(typeof(ZEmbeddedModule), typeof(TestExcludeZFilterGridModulesAllHaveModuleBashersAttribute), ExcludeClientDlls = true)]
	public abstract class ZEmbeddedModuleBasherTest : BasherTest
	{
		public void TestReportModuleInTheReportModuleList()
		{
			if (ModuleToBashType.SelectRecursive(t => t.BaseType != null ? new [] { t.BaseType } : Array.Empty<Type>()).Any(t => t.Name == "ZReportModule"))
			{
				AssertCollectionContains("Please add this Module ID to Enterprise.ZArchitecture.Modules.ReportModules.GetReportModules()", ModuleID, ReportModules.GetReportModules());
			}
			else
			{
				Assert(true);
			}
		}

		[RequiresSTA]
		public void TestUserDefinedFilters_ShouldGenerateValidQueries()
		{
			AssertUserDefinedFilters_ShouldGenerateValidQueries();
		}

		protected virtual void AssertUserDefinedFilters_ShouldGenerateValidQueries()
		{
			using (var moduleForUserDefinedFilters = (ZEmbeddedModule)ZModule.GetZModule(ModuleID))
			{
				try
				{
					var control = moduleForUserDefinedFilters.EmbeddedControl as StripControl;

					if (control != null)
					{
						var filterBizo = control.FilterBusinessObject;
						AssertEquals("Only ZFilterModules should support User-Defined filters... SAD!", false, filterBizo.SupportsUserDefinedFilters);
					}
				}
				catch (ModuleGuiNotSupportedException)
				{
				}
			}

			Assert(true);
		}

		#region Implementation

		protected abstract ModuleIdentifier GetModuleID();

		public ModuleIdentifier ModuleID
		{
			get { return GetModuleID(); }
		}

		public virtual Type ModuleToBashType
		{
			get
			{
				using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
				{
					return module.GetType();
				}
			}
		}

		ZEmbeddedModule module;

		public override Form GetFormToBash()
		{
			if (module != null)
			{
				module.Dispose();
			}

			var id = GetModuleID();

			module = (ZEmbeddedModule)ZModuleFactory.Instance.Create(id);

			if (module == null)
			{
				Fail("Unable to get module for ID : " + id.ToString());
			}
			if (!BashType.IsInstanceOfType(module))
			{
				//allow module to be of BashType.BaseType if BashType.Name is same as BaseType.Name + ForTest
				//allow module to be test extraction accessor
				if (BashType.Name != BashType.BaseType?.Name + "ForTest" || !BashType.BaseType.IsInstanceOfType(module))
				{
					throw new InvalidOperationException("BashType must be consistent with the module");
				}
			}

			PrepareModuleForBashing(module);

			var embeddedControl = module.EmbeddedControl;
			var form = new ZForm();
			form.CaptionRenderingEnabled = true;
			form.Controls.Add(embeddedControl);

			return form;
		}

		protected virtual void PrepareModuleForBashing(ZEmbeddedModule module)
		{
		}

		protected virtual string GetIsSystemDefinedDefaultProperty()
		{
			return "System"; // Filter option
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (module != null)
			{
				module.Dispose();
			}
		}

		#endregion
	}
}
