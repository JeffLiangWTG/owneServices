using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ProcessHeaderTemplateTextFilter))]
	public class ProcessHeaderTemplateTextFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTemplateTextFilter()
		{
			foreach (var type in new Type[] { typeof(ProcessHeaderFilterBusinessObject), typeof(BMFilterRuleFilterBusinessObject) })
			{
				foreach (var filterControlId in new string[] { "JustAnIdentifier", "TagRule" })
				{
					foreach (var templateListCode in new string[] { TemplateFilterOptions.Codes.Template, TemplateFilterOptions.Codes.All, TemplateFilterOptions.Codes.NonTemplate })
					{
						CheckTemplateTextFilter(type, filterControlId, templateListCode);
					}
				}
			}
		}

		void CheckTemplateTextFilter(Type type, string filterControlId, string templateListCode)
		{
			var input = "INPUT: " + type + " " + filterControlId + " " + templateListCode;

			var obj = Activator.CreateInstance(type);
			if (obj.GetType() == typeof(BMFilterRuleFilterBusinessObject))
			{
				((IBMFilterRuleFilterBusinessObject)obj).FilterControlIdentifier = filterControlId;
			}

			var collection = new ModuleFilterCollection();
			((ProcessHeaderFilterBusinessObject)obj).AddProcessHeaderFiltersToModuleFilterCollection(collection, new ZSqlParameterCollection());
			var filter = (ProcessHeaderTemplateTextFilter)collection[ProcessHeader.ModuleFilterConstants.Template];
			filter.Property = templateListCode;

			if (type == typeof(BMFilterRuleFilterBusinessObject) && ((IBMFilterRuleFilterBusinessObject)obj).FilterControlIdentifier == "TagRule")
			{
				AssertHasWarning(input, filter.PropertyInfo, TagRuleValidation.FilterTemplateMessage);
			}
			else
			{
				AssertNoErrors(input, filter.PropertyInfo);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProcessHeaderTemplateTextFilter(new ProcessHeaderFilterBusinessObject());
		}
	}
}
