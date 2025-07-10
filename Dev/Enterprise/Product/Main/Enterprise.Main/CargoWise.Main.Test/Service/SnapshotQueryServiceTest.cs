using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Navigation;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using GlowIndexQueryService.Business;
using Moq;

namespace CargoWise.Main.Service.Testing;

sealed class SnapshotQueryServiceTest : TestCaseWithFactory
{
	public void TestQuerySnapShotResult_WhenModuleFilterIdIsInvalid()
	{
		var snapshot = new Snapshot()
		{
			Id = Guid.NewGuid(),
			Order = 0,
			ModuleFilter = new SnapshotModuleFilter()
			{
				ModuleFilterId = Guid.Empty,
				ModuleFilterName = "Nonexistent ModuleFilterId",
				ModuleId = null
			}
		};

		AssertExceptionThrown(typeof(InvalidOperationException), () => SnapshotQueryService.Instance.QuerySnapShotResult(snapshot));
	}

	public void TestQuerySnapShotResultShouldThrowInvalidOperationExceptionWhenIndexSearchIsFalse()
	{
		using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.WorkItem))
		{
			var filterBusinessObject = module.FilterBusinessObject;
			var filter = filterBusinessObject.AddFilterStrip<ModuleTextFilter>("Summary");
			filter.Property = "SSS";
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			var actual = filterBusinessObject.GetActiveFiltersQueries().Select(m => m.ToUrlComponent());
			var stm = filterBusinessObject.SaveLayout("test");
			filterBusinessObject.Factory.Save();

			var moduleId = new ModuleList().GetRegisteredIdentifierByTableName(stm.S9_ModuleID);
			var snapshot = new Snapshot()
			{
				Id = Guid.NewGuid(),
				Order = 0,
				ModuleFilter = new SnapshotModuleFilter()
				{
					ModuleFilterId = stm.PK.ToGuid(),
					ModuleFilterName = stm.S9_FilterName,
					ModuleId = moduleId,
				}
			};

			AssertExceptionThrown(typeof(InvalidOperationException), () => SnapshotQueryService.Instance.QuerySnapShotResult(snapshot));
		}
	}

	public void TestQuerySnapShotResultShouldGenerateCorrectGlowQueries()
	{
		using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.WorkItem))
		using (GetIndexSearchRegistryMock())
		using (GetGlowIndexQueryEngineForTest())
		{
			var filterBusinessObject = module.FilterBusinessObject;
			filterBusinessObject.SearchType = SearchType.Index;

			var filter = filterBusinessObject.AddFilterStrip<ModuleTextFilter>("CODE");
			filter.Property = "CCC";
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			filter = filterBusinessObject.AddFilterStrip<ModuleTextFilter>("NAME");
			filter.Property = "NNN";
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

			var actual = filterBusinessObject.GetActiveFiltersQueries().Select(m => m.ToUrlComponent());
			var stm = filterBusinessObject.SaveLayout("test");
			filterBusinessObject.Factory.Save();

			var moduleId = new ModuleList().GetRegisteredIdentifierByTableName(stm.S9_ModuleID);
			var snapshot = new Snapshot()
			{
				Id = Guid.NewGuid(),
				Order = 0,
				ModuleFilter = new SnapshotModuleFilter()
				{
					ModuleFilterId = stm.PK.ToGuid(),
					ModuleFilterName = stm.S9_FilterName,
					ModuleId = moduleId,
				}
			};

			AssertEquals(50, SnapshotQueryService.Instance.QuerySnapShotResult(snapshot));
			AssertEquals("((startswith(CODE,'CCC')) and (startswith(NAME,'NNN')))", lastGlowQueries.Select(m => m.ToUrlComponent()).First());
		}
	}

	SearchFieldCollection GetSearchFieldCollection()
	{
		var field1 = SearchField.Create("CODE", "Code");
		var field2 = SearchField.Create("NAME", "Full Name");
		var ret = new SearchFieldCollection("IWorkItem", [field1, field2]);
		return ret;
	}

	IDisposable GetGlowIndexQueryEngineForTest()
	{
		var mock = new Mock<IGlowIndexQueryEngine>();
		mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection);
		mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IWorkItem" });
		mock.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>()))
			.Returns(GetIndexQueryResultCollection)
			.Callback((GlowIndexQueryParam e) => lastGlowQueries = e.GlowQueries);
		return ObjectFactory.Substitute(mock.Object);
	}

	IDisposable GetIndexSearchRegistryMock()
	{
		var registryMock = new Mock<IGlowRegistry>();
		registryMock.Setup(e => e.IsGlowIndexSearchAllowedForModule(It.IsAny<string>())).Returns(true);
		registryMock.Setup(e => e.MaximumNumberOfModuleFiltersSearchResults).Returns(50);
		return ObjectFactory.Substitute(registryMock.Object);
	}

	GlowIndexQueryResultCollection GetIndexQueryResultCollection()
	{
		var ret = new GlowIndexQueryResultCollection();
		ret.Status = GlowIndexQueryStatus.Success;
		ret.Results.Add(new GlowIndexQueryResult(ZGuid.BrettsGuid.ToString(), "Dummy"));
		ret.MaximumResults = 50;
		return ret;
	}

	IEnumerable<IGlowQuery> lastGlowQueries;
}

