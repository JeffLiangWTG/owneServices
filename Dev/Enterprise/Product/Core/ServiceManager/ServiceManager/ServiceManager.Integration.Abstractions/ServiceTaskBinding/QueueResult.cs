using System;

namespace ServiceManager.Integration.Abstractions
{
	public record QueueResult
	{
		public QueueResult(int queueSize, TimeSpan age)
		{
			if (queueSize < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(queueSize));
			}
			if (queueSize == 0 && age != TimeSpan.Zero)
			{
				throw new ArgumentException(null, nameof(age));
			}
			QueueSize = queueSize;
			MaximumItemAge = age;
		}

		QueueResult(int queueSize)
		{
			QueueSize = queueSize;
			MaximumItemAge = TimeSpan.Zero;
		}

		public int QueueSize { get; }
		public TimeSpan MaximumItemAge { get; }
		public static QueueResult Error => new QueueResult(-1);
		public static QueueResult Zero => new QueueResult(0);
	}
}
