using System.Collections.Generic;

namespace CargoWise.Common.Collections
{
	/// <summary>
	/// Performs a Merge Sort on a list and returns true if the order of the list changed.
	/// </summary>
	static class MergeSort
	{
		public static bool Sort<T>(List<T> items, IComparer<T> comparer)
		{
			T[] tmp = new T[items.Count];
			return Sort(items, tmp, 0, items.Count, comparer);
		}

		static bool Sort<T>(List<T> items, T[] tmp, int startElement, int endElement, IComparer<T> comparer)
		{
			if (endElement - startElement <= 1)
			{
				return false;
			}

			int midElement = (startElement + endElement) / 2;

			return Sort(items, tmp, startElement, midElement, comparer)
				| Sort(items, tmp, midElement, endElement, comparer)
				| MergeResults(items, tmp, startElement, midElement, endElement, comparer);
		}

		static bool MergeResults<T>(List<T> items, T[] tmp, int startElement, int midElement, int endElement, IComparer<T> comparer)
		{
			bool orderChanged = false;
			int l = 0, r = 0, i = 0;
			int ms = midElement - startElement;
			int em = endElement - midElement;
			while (l < ms && r < em)
			{
				if (comparer.Compare(items[startElement + l], (items[midElement + r])) <= 0)
				{
					tmp[i] = items[startElement + l];
					l++;
				}
				else
				{
					tmp[i] = items[midElement + r];
					r++;
					orderChanged = true;
				}
				i++;
			}

			while (r < em)
			{
				tmp[i] = items[midElement + r]; i++; r++;
			}

			while (l < ms)
			{
				tmp[i] = items[startElement + l]; i++; l++;
			}

			for (int k = 0; k < endElement - startElement; k++)
			{
				items[startElement + k] = tmp[k];
			}

			return orderChanged;
		}
	}
}
