using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(NctsDefaultTraderAtDestinationRegistryItem))]
	sealed class NctsDefaultTraderAtDestinationRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<NctsDefaultTraderAtDestination>
	{
		protected override StronglyTypedRegistryItem<NctsDefaultTraderAtDestination, NctsDefaultTraderAtDestination> GetNewRegistryItem()
		{
			var nctsDefaultTraderAtDestination = new NctsDefaultTraderAtDestination(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			nctsDefaultTraderAtDestination.LeaveBlank = true;
			return new NctsDefaultTraderAtDestinationRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, nctsDefaultTraderAtDestination);
		}
	}
}
