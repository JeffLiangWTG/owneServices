using System;
using System.Runtime;

namespace CargoWise.Common
{
	public class GCWrapperResult
	{
		public long MemoryBeforeCollect { get; internal set; }
		public long MemoryAfterCollect { get; internal set; }
		public int GenerationOfObject { get; internal set; }
	}

	/// <summary>
	/// Encapsulates access to GC.Collect for useful scenarios:
	/// Use ReclaimMemory() when finished using a memory-intesive object.
	/// Use MemoryFailPoint before starting a memory intensive operation.
	/// </summary>
	public static class GCWrapper
	{
		/// <summary>
		/// Disposes an object if IDisposable, sets the object reference to null and reclaims memory via garbage collection.
		/// </summary>
		/// <param name="obj"></param>
		public static GCWrapperResult ReclaimMemory<T>(ref T obj) where T : class
		{
			IDisposable disposable = obj as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
			disposable = null;

			int generation;
			int genLookup;
			generation = genLookup = GC.GetGeneration(obj);
			obj = null;
			if (genLookup > reclaimCollectSpacing.Length)
			{
				genLookup = reclaimCollectSpacing.Length - 1;
			}
			GCWrapperResult result = null;
			bool shouldCollect;
			lock (syncRoot)
			{
				if (shouldCollect = DateTime.UtcNow.Subtract(lastCollect[genLookup]) > reclaimCollectSpacing[genLookup])
				{
					UpdateLastCollect(genLookup);
				}
			}
			if (shouldCollect)
			{
				result = new GCWrapperResult { GenerationOfObject = generation, MemoryBeforeCollect = GC.GetTotalMemory(false) };
				GC.Collect(generation); // This is the GCWrapper
				result.MemoryAfterCollect = GC.GetTotalMemory(false);
			}
			return result;
		}

		static void UpdateLastCollect(int generation)
		{
			for (int i = generation; i >= 0; i--)
			{
				lastCollect[i] = DateTime.UtcNow;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Array used as readonly only but still .NET 4 so immutable collections are not available")]
		static readonly TimeSpan[] reclaimCollectSpacing = new TimeSpan[] { new TimeSpan(0, 0, 1), new TimeSpan(0, 0, 10), new TimeSpan(0, 0, 30) };

		[WTG.StaticAnalysis.Annotation.ThreadSafe(WTG.StaticAnalysis.Annotation.ThreadSafeAttribute.Mechanism.Lock)]
		static DateTime[] lastCollect = new DateTime[] { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };

		static readonly object syncRoot = new object();

#if DEBUG
		internal static void ResetLastCollectionTimesForTest()
		{
			lastCollect = new DateTime[] { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
		}
#endif

		/// <summary>
		/// Creates a new MemoryFailPoint of the given size, calls GC.Collect if there is insufficient memory and tries again.
		/// Throws an InsufficientMemoryException if there is still insufficient memory after the GC.Collect.
		/// </summary>
		/// <param name="sizeInMegabytes"></param>
		/// <returns></returns>
		public static MemoryFailPoint MemoryFailPoint(int sizeInMegabytes)
		{
			MemoryFailPoint memoryFailPoint = null;
			try
			{
				memoryFailPoint = new MemoryFailPoint(sizeInMegabytes);
			}
			catch (InsufficientMemoryException)
			{
				GC.Collect(); // This is the GCWrapper
				UpdateLastCollect(reclaimCollectSpacing.Length - 1);
				memoryFailPoint = new MemoryFailPoint(sizeInMegabytes);
			}

			return memoryFailPoint;
		}

		public static int BytesToMegabytes(int bytes)
		{
			return (bytes / 1048576) + 1;
		}
	}
}
