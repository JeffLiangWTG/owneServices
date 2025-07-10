using System;
using System.Collections.Specialized;
using System.Text;

namespace CargoWise.EntityFramework
{
	public class StringCollectionX : StringCollection
	{
		public StringCollectionX() : base()
		{
		}

		public StringCollectionX(params string[] strings)
		{
			AddRange(strings);
		}

		public StringCollectionX(StringCollection collection)
		{
			AddRange(collection);
		}

		public void AddRange(StringCollection collection)
		{
			foreach (string value in collection)
			{
				Add(value);
			}
		}

		public override string ToString()
		{
			int length = 0;
			for (int i = 0; i < Count; i++)
			{
				string value = this[i];
				if (!string.IsNullOrEmpty(value))
				{
					length += value.Length;
				}
			}

			StringBuilder result = new StringBuilder(length);

			for (int i = 0; i < Count; i++)
			{
				if (i > 0)
				{
					result.Append(Environment.NewLine);
				}
				result.Append(this[i]);
			}

			return result.ToString();
		}

		public string[] ToArray()
		{
			string[] array = new string[Count];
			CopyTo(array, 0);
			return array;
		}
	}
}
