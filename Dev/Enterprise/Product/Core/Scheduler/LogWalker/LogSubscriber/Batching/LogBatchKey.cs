using System;

namespace Enterprise.LogWalker
{
	public class LogBatchKey : IEquatable<LogBatchKey>
	{
		public LogBatchKey(int retries, object key)
		{
			this.retries = retries;
			this.key = key ?? throw new ArgumentNullException(nameof(key));
		}

		readonly object key;
		readonly int retries;

		public int Retries => retries;
		public object Key => key;

		#region IEquatable

		public override bool Equals(object obj) => obj is LogBatchKey other && Equals(other);

		public override int GetHashCode()
		{
			if (retries > 0)
			{
				return base.GetHashCode();
			}
			else
			{
				return key.GetHashCode();
			}
		}

		public bool Equals(LogBatchKey other)
		{
			if (retries > 0)
			{
				return ReferenceEquals(other, this);
			}
			else
			{
				return key.Equals(other.key);
			}
		}

		#endregion
	}
}
