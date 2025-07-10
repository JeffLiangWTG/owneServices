using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business
{
	public static class ArrayToTextConverter
	{
		public static ZString ConvertToCommaSeparatedText(IEnumerable<BusinessObject> collection, ZString fieldName)
		{
			return JoinCollectionValues(", ", collection, fieldName);
		}

		public static ZString ConvertToCommaSeparatedMultilineText(IEnumerable<BusinessObject> collection, ZString fieldName)
		{
			return JoinCollectionValues(", " + System.Environment.NewLine, collection, fieldName);
		}

		public static ZString ConvertToCommaSeparatedText(params ZString[] values)
		{
			return JoinArrayValues(", ", values);
		}

		public static ZString ConvertToCommaSeparatedMultilineText(params ZString[] values)
		{
			return JoinArrayValues(", " + System.Environment.NewLine, values);
		}

		#region Implementation

		static ZString JoinCollectionValues(string separator, IEnumerable<BusinessObject> collection, ZString fieldName)
		{
			List<ZString> items = new List<ZString>();

			foreach (BusinessObject bizO in collection)
			{
				string value = ZPropertyAccessor.Get(bizO, fieldName).ToString();
				if (!string.IsNullOrEmpty(value))
				{
					items.Add(value);
				}
			}

			return ZString.Join(separator, items.ToArray());
		}

		static ZString JoinArrayValues(string separator, params ZString[] values)
		{
			List<ZString> items = new List<ZString>();

			foreach (ZString value in values)
			{
				if (!value.IsEmpty)
				{
					items.Add(value);
				}
			}

			return ZString.Join(separator, items.ToArray());
		}

		#endregion
	}
}
