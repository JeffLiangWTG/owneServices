using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(NctsDefaultPrincipalRegistryItem))]
	class NctsDefaultPrincipalRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<NctsDefaultPrincipal>
	{
		protected override StronglyTypedRegistryItem<NctsDefaultPrincipal, NctsDefaultPrincipal> GetNewRegistryItem()
		{
			var nctsDefaultPrincipal = new NctsDefaultPrincipal(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			nctsDefaultPrincipal.LeaveBlank = true;
			return new NctsDefaultPrincipalRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, nctsDefaultPrincipal);
		}
	}
}
