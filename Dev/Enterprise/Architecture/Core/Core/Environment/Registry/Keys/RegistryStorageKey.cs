using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryStorageKey
	{
		public RegistryStorageKey(Guid companyPK, Guid branchPK, Guid departmentPK)
			: this(RegistryCacheKey.GetOwnerPK(companyPK, branchPK), departmentPK)
		{
		}

		public RegistryStorageKey(Guid ownerPK, Guid departmentPK)
		{
			this.ownerPK = ownerPK;
			this.departmentPK = departmentPK;
		}

		public string Key
		{
			get
			{
				if (key == null)
				{
					key = Convert.ToBase64String(ownerPK.ToByteArray()) + Convert.ToBase64String(departmentPK.ToByteArray());
				}
				return key;
			}
		}

		string key;
		readonly Guid ownerPK;
		readonly Guid departmentPK;
	}
}
