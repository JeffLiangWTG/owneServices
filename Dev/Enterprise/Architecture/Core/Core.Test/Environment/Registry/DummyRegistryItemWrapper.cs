using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DummyRegistryItemWrapper : RegistryItemWrapper
	{
		public int CountOnOverrideDefaultChangedCalled { get; private set; }

		public DummyRegistryItemWrapper(IRegistryItem inner)
			: base(inner)
		{
			CountOnOverrideDefaultChangedCalled = 0;
		}

		protected override void OnOverrideDefaultChanged(Guid companyPK, Guid branchPK, Guid departmentPK, bool state)
		{
			base.OnOverrideDefaultChanged(companyPK, branchPK, departmentPK, state);
			CountOnOverrideDefaultChangedCalled++;
		}
	}
}
