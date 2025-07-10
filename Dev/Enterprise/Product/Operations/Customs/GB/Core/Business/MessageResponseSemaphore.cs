using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Semaphores.Common;

namespace Enterprise.Customs.GB.Business
{
	public class MessageResponseSemaphore : ISemaphoreType
	{
		public MessageResponseSemaphore() : this(null) { }
		public MessageResponseSemaphore(JobDeclaration declaration)
		{
			LockInfo = string.Join(":", LockInfoPrefix, declaration?.PK.ToString() ?? "0");
		}

		public const string LockInfoPrefix = "MessageResponseSemaphore";

		public string LockInfo { get; private set; }

		public string Category => "MSG";

		public int MaxConcurrentHandles => 1;
	}
}
