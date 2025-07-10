using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryAuditColumnCacheKey : RegistryCacheKey
	{
		internal RegistryAuditColumnCacheKey(string name, string auditColumnName, Guid companyPK, Guid branchPK, Guid departmentPK) : base(name, companyPK, branchPK, departmentPK, false)
		{
			AuditColumnName = auditColumnName;
		}

		public readonly string AuditColumnName;

		protected override string GetKey()
		{
			return base.GetKey() + AuditColumnName;
		}
	}
}
