using System;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class ReferenceFilesRegistry
	{
		public ReferenceFilesRegistry(RawDataRegistry rawRegistry)
		{
			RawRegistry = rawRegistry;
		}

		public ReadOnlyCodeDescriptionPairList EquipmentGroup
		{
			get { return (ReadOnlyCodeDescriptionPairList)RawRegistry.EquipmentGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.EquipmentGroup.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}
		readonly RawDataRegistry RawRegistry;
	}
}
