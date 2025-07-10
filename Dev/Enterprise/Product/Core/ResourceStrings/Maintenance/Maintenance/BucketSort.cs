using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.IO;

namespace Enterprise.ResourceStrings.Maintenance
{
	/// <summary>
	/// Sort a very large amount of data without having to store it all in memory
	/// </summary>
	public class BucketSort<T> : IDisposable where T : class
	{
		public BucketSort(ISerializer serializer, IComparer<T> comparer, int bucketSize)
		{
			this.serializer = serializer;
			this.comparer = comparer;
			this.bucketSize = bucketSize;
			currentList = new SortedDictionary<T, T>(comparer);
		}

		public void Add(T item)
		{
			currentList.Add(item, item);
			if (currentList.Count >= bucketSize)
			{
				CreateBucket();
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			CreateBucket();
			while (fileList.Count > 1)
			{
				string mergedFile = Guid.NewGuid().ToString();
				Merge(fileList[0], fileList[1], mergedFile);
				fileList.RemoveAt(0);
				fileList.RemoveAt(0);
				fileList.Add(mergedFile);
			}
			using var reader = new StreamReader(Path.Combine(tempDirectory.DirectoryName, fileList[0]));
			T item;
			while ((item = serializer.Read(reader)) != null)
			{
				yield return item;
			}
		}

		void CreateBucket()
		{
			if (currentList.Count == 0)
			{
				return;
			}

			using (var writer = new StreamWriter(Path.Combine(tempDirectory.DirectoryName, fileList.Count.ToString())))
			{
				foreach (var value in currentList.Values)
				{
					serializer.Write(value, writer);
				}
			}
			fileList.Add(fileList.Count.ToString());
			currentList = new SortedDictionary<T, T>(comparer);
		}

		void Merge(string fileA, string fileB, string outputFile)
		{
			T itemA = null;
			T itemB = null;
			using (var readerA = new StreamReader(Path.Combine(tempDirectory.DirectoryName, fileA)))
			using (var readerB = new StreamReader(Path.Combine(tempDirectory.DirectoryName, fileB)))
			using (var writer = new StreamWriter(Path.Combine(tempDirectory.DirectoryName, outputFile)))
			{
				do
				{
					itemA ??= serializer.Read(readerA);
					itemB ??= serializer.Read(readerB);
					if (itemA == null && itemB == null)
					{
						break;
					}
					else if (itemA == null)
					{
						serializer.Write(itemB, writer);
						itemB = null;
					}
					else if (itemB == null)
					{
						serializer.Write(itemA, writer);
						itemA = null;
					}
					else if (comparer.Compare(itemA, itemB) < 0)
					{
						serializer.Write(itemA, writer);
						itemA = null;
					}
					else
					{
						serializer.Write(itemB, writer);
						itemB = null;
					}
				}
				while (true);
			}
			File.Delete(Path.Combine(tempDirectory.DirectoryName, fileA));
			File.Delete(Path.Combine(tempDirectory.DirectoryName, fileB));
		}

		readonly TempDirectory tempDirectory = new();
		readonly ISerializer serializer;
		readonly IComparer<T> comparer;
		readonly int bucketSize;
		readonly List<string> fileList = new();
		SortedDictionary<T, T> currentList;

		public void Dispose()
		{
			tempDirectory.Dispose();
		}

		public interface ISerializer
		{
			T Read(TextReader reader);
			void Write(T item, TextWriter writer);
		}
	}
}
