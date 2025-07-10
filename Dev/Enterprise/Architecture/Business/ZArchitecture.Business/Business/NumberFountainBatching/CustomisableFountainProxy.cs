using System;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Environment
{
	class CustomisableFountainProxy : INumberFountainProxy
	{
		public CustomisableFountainProxy(INumberFountainProxy proxy)
		{
			this.proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
		}

		public bool EnsureConsistentSequence => proxy.EnsureConsistentSequence;

		public string Prefix => proxy.Prefix;

		public string Name => proxy.Name;

		public long GetNext(IDbConnected dbConnected)
		{
			return proxy.GetNext(dbConnected);
		}

		public long[] GetNexts(IDbConnected dbConnected, int amount)
		{
			return proxy.GetNexts(dbConnected, amount);
		}

		public string GetNextFormatted(IDbConnected dbConnected)
		{
			return proxy.GetNextFormatted(dbConnected);
		}

		public string[] GetNextsFormatted(IDbConnected dbConnected, int amount)
		{
			return proxy.GetNextsFormatted(dbConnected, amount);
		}

		public void SetNext(IDbConnected dbConnected, long nextValue)
		{
			proxy.SetNext(dbConnected, nextValue);
		}

		public void SetValues(IDbConnected dbConnected, long minValue, long nextValue, long maxValue)
		{
			proxy.SetValues(dbConnected, minValue, nextValue, maxValue);
		}

		public void GetMinAndMaxValues(IDbConnected dbConnected, out long minValue, out long maxValue)
		{
			proxy.GetMinAndMaxValues(dbConnected, out minValue, out maxValue);
		}

		public long PeekPreliminary(IDbConnected dbConnected)
		{
			return proxy.PeekPreliminary(dbConnected);
		}

		public long PeekPreliminaryOrDefault(IDbConnected dbConnected, long defaultValue)
		{
			return proxy.PeekPreliminaryOrDefault(dbConnected, defaultValue);
		}

		public string PeekPreliminaryFormatted(IDbConnected dbConnected)
		{
			return proxy.PeekPreliminaryFormatted(dbConnected);
		}

		public void FixFountain(IDbConnected dbConnected, string uniqueIndexViolated)
		{
			proxy.FixFountain(dbConnected, uniqueIndexViolated);
		}

		public void FixFountain(IDbConnected dbConnected, DbCommand commandToFindMaxValueInDatabase)
		{
			proxy.FixFountain(dbConnected, commandToFindMaxValueInDatabase);
		}

		#region IEquatable

		public override bool Equals(object other) => other is CustomisableFountainProxy oProxy && oProxy.Equals(this);
		public bool Equals(INumberFountainProxy other) => other is CustomisableFountainProxy oProxy && oProxy.Equals(this);
		public bool Equals(CustomisableFountainProxy other) => other.proxy.Equals(proxy);
		public static bool operator ==(CustomisableFountainProxy x, CustomisableFountainProxy y) => ReferenceEquals(x, y) || (!ReferenceEquals(x, null) && !ReferenceEquals(y, null) && x.Equals(y));
		public static bool operator !=(CustomisableFountainProxy x, CustomisableFountainProxy y) => !(x == y);
		public override int GetHashCode()
		{
			unchecked
			{
				return proxy.GetHashCode() + 1;
			}
		}

		#endregion

		#region Implementation

		readonly INumberFountainProxy proxy;

		#endregion
	}
}
