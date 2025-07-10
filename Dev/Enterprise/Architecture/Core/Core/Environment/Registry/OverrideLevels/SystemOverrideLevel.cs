using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels
{
	public class SystemOverrideLevel : OverrideLevelWithDepartments
	{
		public SystemOverrideLevel(bool hideInactiveChildren = false)
			: base(
				description: Res.GetString("D85F09B8-AEE6-4117-B1BC-417C9BAD26BF", "System"),
				pathRelativeToParent: string.Empty,
				companyPK: Guid.Empty,
				branchPK: Guid.Empty,
				departmentStorageFlag: RegistryStorageFlags.SystemDepartment,
				hideInactiveChildren: hideInactiveChildren)
		{
		}

		public override bool CanSetValueOf(IRegistryItem item)
		{
			return item.Storage.HasFlag(RegistryStorageFlags.System);
		}

		public override IOverrideLevel Parent
		{
			get { return new DefaultOverrideLevel(); }
		}

		protected override IEnumerable<IOverrideLevel> OtherChildren { get { return Enumerable.Empty<IOverrideLevel>(); } }
	}
}
