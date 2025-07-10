using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAExternalModuleColumnsAndFiltersProvider.EqualAndBlankModuleStatusFilter))]
	sealed class EqualModuleStatusFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CAExternalModuleColumnsAndFiltersProvider.EqualAndBlankModuleStatusFilter(
				ACIMessageStatusFilterId,
					(NoResString)ACIMessageStatusFilterId,
					(c, v) => new ZQuery(),
					() => new MessageStatusList((NoResString)"House Bill or Supplementary"));
		}

		const string ACIMessageStatusFilterId = "ACI Message Status";
	}
}
