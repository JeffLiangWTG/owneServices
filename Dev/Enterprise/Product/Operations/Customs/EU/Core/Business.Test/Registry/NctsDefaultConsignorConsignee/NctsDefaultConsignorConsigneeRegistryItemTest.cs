using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(NctsDefaultConsignorConsigneeRegistryItem))]
	class NctsDefaultConsignorConsigneeRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<NctsDefaultConsignorConsignee>
	{
		protected override StronglyTypedRegistryItem<NctsDefaultConsignorConsignee, NctsDefaultConsignorConsignee> GetNewRegistryItem()
		{
			var nctsDefaultConsignorConsignee = new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			nctsDefaultConsignorConsignee.LeaveBlank = true;
			return new NctsDefaultConsignorConsigneeRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, nctsDefaultConsignorConsignee);
		}
	}
}
