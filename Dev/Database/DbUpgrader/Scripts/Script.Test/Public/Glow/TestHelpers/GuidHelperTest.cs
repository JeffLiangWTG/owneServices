using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.TestHelpers
{
	class GuidHelper
	{
		readonly Dictionary<string, Guid> items;

		public GuidHelper()
		{
			items = new Dictionary<string, Guid>();
		}

		public Guid this[string key]
		{
			get { return items[key]; }
			set { items[key] = value; }
		}

		public void AssertIsNewGuid(object value)
		{
			TestCase.AssertType(typeof(Guid), value);
			TestCase.AssertNotEquals(Guid.Empty, value);
			TestCase.AssertCollectionNotContains(value, items.Values);
		}

		public bool ContainsValue(object value)
		{
			return items.Values.Any(v => v.Equals(value));
		}

		public string Fill(string template)
		{
			var builder = new StringBuilder(template);

			foreach (var item in items)
			{
				builder.Replace("@" + item.Key, item.Value.ToString());
			}

			return builder.ToString();
		}

		public string Key(object value)
		{
			var item = items.FirstOrDefault(i => i.Value.Equals(value));

			if (item.Equals(default(KeyValuePair<string, Guid>)))
			{
				throw new ArgumentOutOfRangeException(nameof(value));
			}

			return item.Key;
		}
	}
}
