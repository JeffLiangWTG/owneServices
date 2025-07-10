using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class AECustomsRegistry
	{
		public AECustomsRegistry(RawDataRegistry rawRegistry)
		{
			RawRegistry = rawRegistry;
		}

		public string CourierID
		{
			get { return (string)RawRegistry.AECourierID.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.AECourierID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		readonly RawDataRegistry RawRegistry;
	}
}
