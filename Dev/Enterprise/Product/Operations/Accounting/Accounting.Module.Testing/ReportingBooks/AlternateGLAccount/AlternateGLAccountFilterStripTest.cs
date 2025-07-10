using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AlternateGLAccountFilterStrip))]
	public class AlternateGLAccountFilterStripTest : TestCaseWithFactory
	{
		public void TestAlternateGLAccountFilterStripPercentNumber()
		{ 
			using var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.AlternateGLAccounts);
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Percent Number");
				var filter = (AlternateGLAccountModuleFilter)strip.CurrentModuleFilter;

				using var moduleForm = (ZForm)module.ShowPopup();
				{
					var filterControl = moduleForm.FindSingle<ZFilterStripCommonControl>();
					filterControl.AddFilterStrip(strip);
					var findBox = filterControl.FindSingle<AlternateGLAccountZGuidFindBox>();

					AssertNotNull("Percent Number:AlternateGLAccountZGuidFindBox should not be null", findBox);
				}

				AssertNotNull("Percent Number:AlternateGLAccountModuleFilter should not be null", filter);
			}
		}

		public void TestAlternateGLAccountFilterStripConsolidationNumber()
		{
			using var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.AlternateGLAccounts);
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Consolidate Number");
				var filter = (AlternateGLAccountModuleFilter)strip.CurrentModuleFilter;

				using var moduleForm = (ZForm)module.ShowPopup();
				{
					var filterControl = moduleForm.FindSingle<ZFilterStripCommonControl>();
					_ = filterControl.AddFilterStrip(strip);
					var findBox = filterControl.FindSingle<AlternateGLAccountZGuidFindBox>();

					AssertNotNull("Consolidate Number:AlternateGLAccountZGuidFindBox should not be null", findBox);
				}

				AssertNotNull("Consolidate Number:AlternateGLAccountModuleFilter should not be null", filter);
			}
		}

		public void TestAlternateGLAccountFilterStripAlternateNumber()
		{
			using var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.AlternateGLAccounts);
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Alternate Number");
				var filter = (AlternateGLAccountModuleFilter)strip.CurrentModuleFilter;

				using var moduleForm = (ZForm)module.ShowPopup();
				{
					var filterControl = moduleForm.FindSingle<ZFilterStripCommonControl>();
					_ = filterControl.AddFilterStrip(strip);
					var findBox = filterControl.FindSingle<AlternateGLAccountZGuidFindBox>();

					AssertNotNull("Alternate Number:AlternateGLAccountZGuidFindBox should not be null", findBox);
				}

				AssertNotNull("Alternate Number:AlternateGLAccountModuleFilter should not be null", filter);
			}
		}

		public void TestAlternateGLAccountFilterStripTotalReferenceNumber()
		{
			using var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.AlternateGLAccounts);
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Total Reference Number");
				var filter = (AlternateGLAccountModuleFilter)strip.CurrentModuleFilter;

				using var moduleForm = (ZForm)module.ShowPopup();
				{
					var filterControl = moduleForm.FindSingle<ZFilterStripCommonControl>();
					_ = filterControl.AddFilterStrip(strip);
					var findBox = filterControl.FindSingle<AlternateGLAccountZGuidFindBox>();

					AssertNotNull("Total Reference Number:AlternateGLAccountZGuidFindBox should not be null", findBox);
				}

				AssertNotNull("Total Reference Number:AlternateGLAccountModuleFilter should not be null", filter);
			}
		}
	}
}
