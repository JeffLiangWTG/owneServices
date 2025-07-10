
namespace CargoWise.Data
{
	public interface IConnectionPooling
	{
		bool IsPooling { get; }
		int MaxPoolSize { get; }
		int MinPoolSize { get; }
		int LoadBalanceTimeout { get; }
	}

	public class NoConnectionPooling : IConnectionPooling
	{
		#region IConnectionPooling Members

		public bool IsPooling
		{
			get
			{
				return false;
			}
		}

		public int MaxPoolSize
		{
			get
			{
				return 0;
			}
		}

		public int MinPoolSize
		{
			get
			{
				return 0;
			}
		}

		public int LoadBalanceTimeout
		{
			get
			{
				return 0;
			}
		}

		#endregion
	}

	public class DefaultConnectionPooling : IConnectionPooling
	{
		#region IConnectionPooling Members

		public bool IsPooling
		{
			get { return true; }
		}

		public virtual int LoadBalanceTimeout
		{
			get
			{
				return 0;
			}
		}

		public virtual int MaxPoolSize
		{
			get { return 100; }
		}

		public virtual int MinPoolSize
		{
			get
			{
				return 0;
			}
		}

		#endregion
	}
}
