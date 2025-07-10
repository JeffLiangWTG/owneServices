using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// A simple set of mappings between the position of a DataRow in a DataView and the position
	/// of a BusinessObject in an ActiveBusinessObjectCollection.
	/// </summary>
	[DebuggerTypeProxy(typeof(DataViewBusinessObjectMapping_DebuggerTypeProxy))]
	internal class DataViewBusinessObjectMapping
	{
		public int this[int dataViewIndex]
		{
			get
			{
				int result = -1;
				if (IsDataViewIndexWithinRange(dataViewIndex))
				{
					result = indexes[dataViewIndex - realStartIndex];
				}
				return result;
			}
			set
			{
				EnsureDataViewIndexWithinRange(dataViewIndex);
				indexes[dataViewIndex - realStartIndex] = value;
			}
		}

		#region Implementation

		int startIndex;
		int endIndex;

		int realStartIndex;
		int[] indexes;

		internal int StartIndex
		{ get { return startIndex; } }

		internal int EndIndex
		{ get { return endIndex; } }

		bool IsDataViewIndexWithinRange(int dataViewIndex)
		{ return indexes != null && dataViewIndex >= startIndex && dataViewIndex <= endIndex; }

		void EnsureDataViewIndexWithinRange(int dataViewIndex)
		{
			if (indexes == null)
			{
				startIndex = dataViewIndex;
				endIndex = dataViewIndex;
				indexes = new int[dataViewIndex < 100 ? dataViewIndex + 100 : 200];
				realStartIndex = dataViewIndex < 100 ? 0 : dataViewIndex - 100;
			}
			else
			{
				if (dataViewIndex < startIndex)
				{
					if (dataViewIndex < realStartIndex)
					{
						int newRealStartIndex = dataViewIndex - 100 < 0 ? 0 : dataViewIndex - 100;
						PrependNewArrayItems(realStartIndex - newRealStartIndex);
					}
					startIndex = dataViewIndex;
				}
				if (dataViewIndex > endIndex)
				{
					if (indexes.Length < dataViewIndex - realStartIndex + 1)
					{
						int oldLength = indexes.Length;
						Array.Resize(ref indexes, dataViewIndex - realStartIndex + 100);
						for (int i = oldLength; i < indexes.Length; i++)
						{
							indexes[i] = -1;
						}
					}
					endIndex = dataViewIndex;
				}
			}
		}

		void PrependNewArrayItems(int numberToPrepend)
		{
			int[] newIndexes = new int[numberToPrepend + indexes.Length];
			Array.Copy(indexes, 0, newIndexes, numberToPrepend, indexes.Length);
			for (int i = 0; i < numberToPrepend; i++)
			{
				newIndexes[i] = -1;
			}
			indexes = newIndexes;
			realStartIndex -= numberToPrepend;
		}

		public string[] Mappings
		{
			get
			{
				var result = new List<string>();
				for (int i = StartIndex; i <= EndIndex; i++)
				{
					result.Add(i + "->" + this[i]);
				}
				return result.ToArray();
			}
		}

		public override string ToString()
		{
			return Mappings.Aggregate((x, y) => { return x + ", " + y; });
		}

		#endregion
	}
}
