using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class BMSGUITestHelper : BMSTestHelper
	{
		#region Helper Methods

		public static BMComponentAcceptabilityBand CreateAcceptabilityBandForTag(BMComponent component, int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax, int cautionMax, string name, TagMagnitude tag, string sql = "", string type = AcceptabilityBandTypes.Codes.SQL)
		{
			var band = CreateAcceptabilityBand(component, cautionMin, goodMin, excellentMin, excellentMax, goodMax, cautionMax, name, sql, type);

			FilterStripsTestHelper.AddFilterStrips(band.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.TagMagnitude,
				FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = tag.PK,
			});

			return band;
		}

		#endregion

		#region Filters

		/// <summary>
		/// Use this method to create a filter layout that is not persisted, and just used for copying between filter layouts, such as to simulate what happens when users customise filter strips applied to a board section.
		/// </summary>
		/// <param name="factory">The factory in which the filter will be constructed.</param>
		/// <param name="filterName">The value to use for <see cref="StmModuleFilter.S9_FilterName"/>.</param>
		/// <returns>An <see cref="StmModuleFilter"/> row which cannot be saved to the db due to its missing <see cref="StmModuleFilter.S9_ParentID"/> and <see cref="StmModuleFilter.S9_ParentTableCode"/> values.</returns>
		public static StmModuleFilter CreateTemporaryBMFilterRuleModuleFilter(BusinessObjectFactory factory, string filterName)
		{
			var filter = factory.New<StmModuleFilter>();
			filter.S9_FilterName = filterName;
			filter.S9_ModuleID = ModuleIDs.BMFilterRule.Name;
			filter.S9_FilterType = StmModuleFilterTypes.Codes.FilterRule;

			return filter;
		}

		public static IDisposable SubstituteWithMockFilter(BMComponentLink link, out Mock<DummyModuleFilter> mockFilter)
		{
			const string filterName = "MockModuleFilter";
			var mockModuleFilter = new Mock<DummyModuleFilter>(new ZString(filterName), DummyBizoSchema.PK) { CallBase = true };

			var bmFilterRuleFilterBusinessObjectMock = new Mock<BMFilterRuleFilterBusinessObject>() { CallBase = true };
			bmFilterRuleFilterBusinessObjectMock
				.Protected()
				.Setup<ModuleFilterCollection>("GetModuleFiltersCore")
				.Returns(() =>
				{
					var moduleFilterCollection = new ModuleFilterCollection();
					moduleFilterCollection.AddFilter(mockModuleFilter.Object);
					return moduleFilterCollection;
				});

			FilterStripsTestHelper.AddFilterStrip<DummyModuleFilter>(link.FilterRule, filterName);

			mockModuleFilter.Object.IsActive = true;
			mockFilter = mockModuleFilter;

			return ObjectFactory.Substitute("BMFilterRule_FilterStripBusinessObject", bmFilterRuleFilterBusinessObjectMock.Object);
		}

		public static IDisposable SubstituteWithMockFilter(BMComponentLink link, Func<ZQuery> mockFilterGetQueryMethod)
		{
			var disposable = SubstituteWithMockFilter(link, out var mockModuleFilter);

			mockModuleFilter.Object.IsActive = true;
			mockModuleFilter
				.Protected()
				.Setup<ZQuery>("GetQuery")
				.Returns(() => mockFilterGetQueryMethod.Invoke());

			return disposable;
		}

		#endregion
	}
}
