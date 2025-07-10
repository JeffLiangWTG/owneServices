using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	[System.Diagnostics.DebuggerDisplay("Count = {nextKey}")]
	public sealed partial class HyperlinkActionCollection
	{
		public HyperlinkActionCollection()
		{
			keyLookup = new SortedDictionary<string, IHyperlinkAction>();
			linkLookup = new Dictionary<LogHyperlink, IHyperlinkAction>();
		}

		public void Clear()
		{
			keyLookup.Clear();
			linkLookup.Clear();
			nextKey = 0;
		}

		public IHyperlinkAction this[string key]
		{
			get
			{
				IHyperlinkAction result;
				keyLookup.TryGetValue(key, out result);
				return result;
			}
		}

		public const string Key = "KEY";
		public const int KeyLength = 4;

		public int Count
		{
			get { return keyLookup.Count; }
		}

		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "Successfully running code")]
		public IHyperlinkAction this[LogHyperlink hyperlink]
		{
			get
			{
				if (hyperlink == null)
				{
					throw new ArgumentNullException(nameof(hyperlink));
				}

				IHyperlinkAction result;

				if (!linkLookup.TryGetValue(hyperlink, out result))
				{
					result = HyperlinkActionFactory.New(hyperlink, string.Format(CultureInfo.InvariantCulture, Key + "{0:" + new String('0', KeyLength) + "}", nextKey++)); // Lookup key not shown to the user, unless they do a copy paste of the text
					linkLookup.Add(hyperlink, result);
					keyLookup.Add(result.Key, result);
				}

				return result;
			}
		}

		public static bool IsHyperlinkActionCollectionLink(LinkClickedEventArgs e)
		{
			return e.LinkText.Contains("#" + Key) && !Uri.IsWellFormedUriString(e.LinkText, UriKind.RelativeOrAbsolute);
		}

		int nextKey;
		readonly IDictionary<string, IHyperlinkAction> keyLookup;
		readonly IDictionary<LogHyperlink, IHyperlinkAction> linkLookup;
	}
}

#region Test
#if DEBUG

#region Display Proxy

namespace Enterprise.ZArchitecture.GUI
{
	[System.Diagnostics.DebuggerTypeProxy(typeof(_DisplayProxy))]
	partial class HyperlinkActionCollection
	{
		sealed class _DisplayProxy
		{
			public _DisplayProxy(HyperlinkActionCollection parent)
			{
				this.parent = parent;
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
			public IHyperlinkAction[] Content
			{
				get
				{
					var result = new IHyperlinkAction[parent.keyLookup.Count];
					parent.keyLookup.Values.CopyTo(result, 0);
					return result;
				}
			}

			readonly HyperlinkActionCollection parent;
		}
	}
}

#endregion

#endif
#endregion
