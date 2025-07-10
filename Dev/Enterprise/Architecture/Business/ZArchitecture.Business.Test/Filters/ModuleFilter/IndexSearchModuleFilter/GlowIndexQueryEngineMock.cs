using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using GlowIndexQueryService.Business;
using Moq;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class GlowIndexQueryEngineMock : IDisposable
	{
		public GlowIndexQueryEngineMock(GlowIndexQueryResultCollection results) : this()
		{
			this.results = results;
		}

		public GlowIndexQueryEngineMock(SearchFieldCollection fields) : this()
		{
			searchFields = fields;
		}

		public GlowIndexQueryEngineMock(SearchFieldCollection fields, GlowIndexQueryResultCollection results) : this()
		{
			searchFields = fields;
			this.results = results;
		}

		public GlowIndexQueryEngineMock(Action<Mock<IGlowIndexQueryEngine>> glowQueryEngineMockSetupAction = null, Action<Mock<IGlowRegistry>> registryMockSetupAction = null)
		{
			glowIndexQueryEngineMock = SubstituteGlowIndexQueryEngine(glowQueryEngineMockSetupAction);
			indexSearchRegistryMock = SubstituteIndexSearchRegistry(registryMockSetupAction);
		}

		public SearchFieldCollection SearchFields
		{
			get => searchFields ??= GetTestSearchFields();
		}
		SearchFieldCollection searchFields;

		public GlowIndexQueryResultCollection Results
		{
			get => results ??= GetIndexQueryResultCollection();
		}
		GlowIndexQueryResultCollection results;

		readonly IDisposable glowIndexQueryEngineMock;
		readonly IDisposable indexSearchRegistryMock;

		GlowIndexQueryResultCollection GetIndexQueryResultCollection()
		{
			var ret = new GlowIndexQueryResultCollection();
			ret.Status = GlowIndexQueryStatus.Success;
			ret.Results.Add(new GlowIndexQueryResult(ZGuid.BrettsGuid.ToString(), "Dummy"));
			return ret;
		}

		public static SearchFieldCollection GetTestSearchFields()
		{
			var field1 = SearchField.Create("CODE", "Code");
			var field2 = SearchField.Create("NAME", "Full Name");

			var ret = new SearchFieldCollection("IGlbStaff", new SearchField[] { field1, field2 });
			return ret;
		}

		IDisposable SubstituteGlowIndexQueryEngine(Action<Mock<IGlowIndexQueryEngine>> action)
		{
			var mock = new Mock<IGlowIndexQueryEngine>();
			if (action != null)
			{
				action(mock);
			}
			else
			{
				_ = mock.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>())).Returns(() => Results);
				_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(() => SearchFields);
				_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IGlbStaff" });
			}
			return ObjectFactory.Substitute(mock.Object);
		}

		IDisposable SubstituteIndexSearchRegistry(Action<Mock<IGlowRegistry>> action)
		{
			var registryMock = new Mock<IGlowRegistry>();
			if (action != null)
			{
				action(registryMock);
			}
			else
			{
				_ = registryMock.Setup(e => e.IsGlowIndexSearchAllowedForModule(It.IsAny<string>())).Returns(true);
				_ = registryMock.Setup(e => e.MaximumNumberOfModuleFiltersSearchResults).Returns(50);
			}
			return ObjectFactory.Substitute(registryMock.Object);
		}

		public void Dispose()
		{
			IndexSearchFilterHelper.ResetIndexQueryEngine();
			GlowModuleToCW1ModuleConverter.ClearEntityTypesCache();
			glowIndexQueryEngineMock?.Dispose();
			indexSearchRegistryMock?.Dispose();
		}
	}
}
