namespace CargoWise.EntityFramework
{
	public class CacheStalenessPolicy
	{
		public static CacheStalenessPolicy NeverStale => new CacheStalenessPolicy(0);
		public static CacheStalenessPolicy StaleOnFactorySave => new CacheStalenessPolicy(1);
		public static CacheStalenessPolicy StaleBeforeFactorySavingTransaction => new CacheStalenessPolicy(2);
		public static CacheStalenessPolicy StaleWhenDataTableChanges(string tableName, BusinessObjectFactory factory) => new CacheStalenessPolicy(3, tableName, factory);

		CacheStalenessPolicy(int policyID, string tableName = null, BusinessObjectFactory factory = null)
		{
			PolicyID = policyID;
			TableName = tableName;
			Factory = factory;
		}

		int PolicyID { get; }

		internal string TableName { get; }
		internal BusinessObjectFactory Factory { get; }

		#region Object Overrides

		public override bool Equals(object obj)
		{
			return (CacheStalenessPolicy)obj == this;
		}

		public override int GetHashCode()
		{
			return PolicyID.GetHashCode();
		}

		public static bool operator ==(CacheStalenessPolicy lhs, CacheStalenessPolicy rhs)
		{
			var lhsIsNull = object.ReferenceEquals(lhs, null);
			var rhsIsNull = object.ReferenceEquals(rhs, null);

			if (lhsIsNull)
			{
				return rhsIsNull;
			}
			else if (rhsIsNull)
			{
				return false;
			}
			else
			{
				return lhs.PolicyID == rhs.PolicyID && lhs.TableName == rhs.TableName;
			}
		}

		public static bool operator !=(CacheStalenessPolicy lhs, CacheStalenessPolicy rhs)
		{
			return !(lhs == rhs);
		}

		#endregion
	}

	public static class CacheStalenessPolicyExtensions
	{
		public static bool IsStaleWhenDataTableChanges(this CacheStalenessPolicy policy)
		{
			return !string.IsNullOrEmpty(policy?.TableName);
		}
	}
}
