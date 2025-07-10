using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	public abstract class ZFilterStripGridModuleTestCase : ZFilterGridModuleTest
	{
		public const string DefaultLayoutNameValue = "System Default Layout";

		#region Overrides

		protected sealed override void TestFilterBusinessObjectTypeCore()
		{
			// Test method redundant for FilterStrips. Functionality now tested in the following test:
			// ZFilterStripGridModuleTestCase.TestModuleUsedInFilterStripHasCorrespondingWebModule()
			Assert(true);
		}

		protected sealed override void TestFilterBusinessObjectCore()
		{
			// Test method redundant for FilterStrips. Functionality now tested in the following test:
			// ZFilterStripGridModule.TestModuleUsedInFilterStripHasCorrespondingWebModule()"
			Assert(true);
		}

		protected sealed override void TestFilterBusinessObjectDefaultsCore()
		{
			// Test method redundant for FilterStrips. Defaults are no longer used.
			Assert(true);
		}

		public sealed override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return null;
		}

		#endregion

		public void TestAuditFilters()
		{
			List<ModuleFilter> auditFilters = new List<ModuleFilter>();
			FilterStripGridModule.OverrideFilterStripBizO((FilterStripBusinessObject)FilterStripGridModule.CreateNewFilterBusinessObject());
			foreach (ModuleFilter filter in FilterStripGridModule.FilterStripBizO.ModuleFilters)
			{
				if (filter.Category == FilterCategories.AuditInformation)
				{
					if (filter.IsPublishedOnWeb)
					{
						auditFilters.Add(filter);
					}
				}
			}

			Dictionary<string, string> expectedAuditFilters = GetExpectedAuditFilters();
			AssertEquals("Audit filters count", expectedAuditFilters.Count, auditFilters.Count);

			foreach (ModuleFilter auditFilter in auditFilters)
			{
				Assert(expectedAuditFilters.ContainsKey(auditFilter.Code));
				AssertEquals(auditFilter.Description, expectedAuditFilters[auditFilter.Code]);
			}
		}

		protected virtual Dictionary<string, string> GetExpectedAuditFilters()
		{
			return new Dictionary<string, string>();
		}

		#region TestModuleUsedInFilterStripHasCorrespondingWebModule

		public void TestModuleUsedInFilterStripHasCorrespondingWebModule()
		{
			bool errorFound = false;
			StringBuilder error = new StringBuilder();

			foreach (ModuleFilter moduleFilter in GetModuleFiltersFromModule(FilterStripGridModule))
			{
				if (moduleFilter.IsPublishedOnWeb && moduleFilter is IModuleFilterWithModuleID)
				{
					var id = ((IModuleFilterWithModuleID)moduleFilter).ModuleId;
					if (id != null && WebModuleIDs.GetWebModuleIDFromModuleID(id) == WebModuleIDs.NotAssigned)
					{
						if (!errorFound)
						{
							error.AppendLine("\r\n\r\nModule: " + FilterStripGridModule.GetType().Name);
							errorFound = true;
						}

						error.AppendLine(string.Format("   FilterName: {0}", moduleFilter.Description));
						error.AppendLine(string.Format("   Module ID:  {0}", id.Name));
					}
				}
			}

			if (errorFound)
			{
				Fail("The following ModuleIDs are used by ModuleFilters that have IsPublishedOnWeb = true, " +
					"however there are no corresponding WebModuleIDs (and likely no web modules created):" + error.ToString());
			}
			Assert("To avoid Empty Test when there are no errors detected", true);
		}

		#endregion

		#region TestLoadEvents

		[ExpectNoExceptions]
		public void TestLoadEvents()
		{
			AssertNull(FilterStripGridModule.FilterStripBizO);
			if (TestPage is ZTestPage)
			{
				((ZTestPage)TestPage).OnLoad();
				((ZTestPage)TestPage).CallOnLoadComplete();
			}
			else
			{
				CallTestPageLoadEvents(TestPage);
			}
			Assert("Module should handle page's events when page's dataSource is null", true);
		}

		protected virtual void CallTestPageLoadEvents(ZPage page)
		{
			Fail("Please implement similar test on your module");
		}

		#endregion

		protected abstract ColumnDetailsForTest[] ExpectedColumnDetails { get; }

		public void TestGridColumnDetails()
		{
			bool initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				ZFilterStripGridModule module = (ZFilterStripGridModule)TestZWebModule;
				DataGridColumn[] actualColumns = module.GridColumnFields;

				AssertEquals("All grid columns must have their details tested. ExpectedColumnDetails.Length should equal GridColumnFields.Length", ExpectedColumnDetails.Length, actualColumns.Length);

				var sbExpected = new StringBuilder();
				foreach (ColumnDetailsForTest columnDetails in ExpectedColumnDetails)
				{
					if (columnDetails != null)
					{
						sbExpected.Append(columnDetails.ColumnIndex).Append("-").AppendLine(columnDetails.HeaderText);
					}
				}

				var sbActual = new StringBuilder();
				for (int i = 0; i < actualColumns.Length; i++)
				{
					sbActual.Append(i).Append("-").AppendLine(actualColumns[i].HeaderText);
				}

				AssertMultilineEquals("All actual columns should be in same order as expected", sbExpected.ToString(), sbActual.ToString(), '\n');
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		#region Implementation

		ModuleFilterCollection GetModuleFiltersFromModule(ZFilterStripGridModule module)
		{
			MethodInfo getNewFilterStripMethod = module.GetType().GetMethod("GetNewFilterStripBusinessObject", BindingFlags.Instance | BindingFlags.NonPublic);
			FilterStripBusinessObject filterStrip = (FilterStripBusinessObject)getNewFilterStripMethod.Invoke(module, null);
			PropertyInfo moduleFiltersProperty = typeof(FilterStripBusinessObject).GetProperty("ModuleFilters", BindingFlags.Instance | BindingFlags.Public);

			return (ModuleFilterCollection)moduleFiltersProperty.GetValue(filterStrip, null);
		}

		#endregion

		protected ZFilterStripGridModule FilterStripGridModule
		{
			get { return FilterGridModule as ZFilterStripGridModule; }
		}

		protected virtual FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get { return null; }
		}

		protected virtual string ExpectedDefaultLayoutName
		{
			get { return DefaultLayoutNameValue; }
		}

		public void TestDefaultFilterStripLayout()
		{
			AssertEquals(ExpectedDefaultLayoutRegistryItem, FilterStripGridModule.DefaultLayoutRegistryItemForTest);
			if (ExpectedDefaultLayoutRegistryItem != null)
			{
				AssertEquals(ExpectedDefaultLayoutRegistryItem.Value, FilterStripGridModule.DefaultLayoutNameForTest);
			}
			AssertEquals(ExpectedDefaultLayoutName, FilterStripGridModule.DefaultLayoutNameForTest);
		}

		protected virtual string ExpectedFilterStripLayoutContext
		{
			get { return FilterStripGridModule.FilterStripLayoutContextForTest; }
		}

		public void TestFilterStripLayoutContext()
		{
			AssertEquals(ExpectedFilterStripLayoutContext, FilterStripGridModule.FilterStripLayoutContextForTest);
		}

		public void TestDefaultLayoutRegistryItemIsNotNull()
		{
			AssertNotNull("DefaultLayoutRegistryItem property needs to be overriden.", FilterStripGridModule.DefaultLayoutRegistryItemForTest);
		}
	}
}
