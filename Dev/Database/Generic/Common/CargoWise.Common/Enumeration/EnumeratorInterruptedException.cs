using System;
using System.Threading;
using CargoWise.Common.Collections;

namespace CargoWise.Common
{
	[Serializable]
	public class EnumeratorInterruptedException : Exception
	{
		public EnumeratorInterruptedException()
		{
		}

		public EnumeratorInterruptedException(string message)
			: base(message)
		{
		}

		public EnumeratorInterruptedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected EnumeratorInterruptedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public static IDisposable InterruptEnumeratorOnThread(Thread thread)
		{
			return new Semaphore(interruptingThreads, thread);
		}

		public static IDisposable AllowEnumerationInterruptionOnCurrentThread()
		{
			return new Semaphore(threadsAllowingInterruption, Thread.CurrentThread);
		}

		public static void ThrowIfInterruptingOnCurrentThread()
		{
			lock (mutex)
			{
				if (interruptingThreads.ContainsKey(Thread.CurrentThread))
				{
					throw new EnumeratorInterruptedException();
				}
			}
		}

		#region Semaphore

		sealed class Semaphore : IDisposable
		{
			internal Semaphore(WeakReferencedKeyDictionary<Thread, int> dictionary, Thread thread)
			{
				Argument.NotNull(dictionary, nameof(dictionary)); // Suggested By ReviewBot 
				this.dictionary = dictionary;
				this.thread = thread;
				lock (mutex)
				{
					object value = dictionary[thread];
					dictionary[thread] = ((int)value == default(int)) ? 1 : ((int)value + 1);
				}
			}

			#region IDisposable Members

			public void Dispose()
			{
				lock (mutex)
				{
					int value = dictionary[thread];
					dictionary[thread] = --value;
					if (value == 0)
					{
						dictionary.Remove(thread);
					}
				}
			}

			#endregion

			readonly WeakReferencedKeyDictionary<Thread, int> dictionary;
			readonly Thread thread;
		}

		#endregion

		static readonly object mutex = new object();

		[WTG.StaticAnalysis.Annotation.ThreadSafe(WTG.StaticAnalysis.Annotation.ThreadSafeAttribute.Mechanism.Lock)]
		static readonly WeakReferencedKeyDictionary<Thread, int> interruptingThreads = new WeakReferencedKeyDictionary<Thread, int>();

		[WTG.StaticAnalysis.Annotation.ThreadSafe(WTG.StaticAnalysis.Annotation.ThreadSafeAttribute.Mechanism.Lock)]
		static readonly WeakReferencedKeyDictionary<Thread, int> threadsAllowingInterruption = new WeakReferencedKeyDictionary<Thread, int>();
	}
}
