using System.Collections;
using CargoWise.Common.Testing;

namespace Enterprise.eHubMessaging.Business
{
	public class XPathQuery
	{
		string xpath;
		ArrayList compiledXPath;
		int matchIndex;
		int matchCount;
		int[] depthLookup;
		bool matchState;
		int treeDepth = -1;
		int key;

		public XPathQuery()
		{
		}

		public XPathQuery(string xpath)
		{
			this.xpath = xpath;
			Compile();
		}

		public XPathQuery(string xpath, int depth)
		{
			this.xpath = xpath;
			this.treeDepth = depth - 1;
			Compile();
		}

		public XPathQuery Clone()
		{
			XPathQuery clone = new XPathQuery();
			clone.xpath = xpath;
			clone.compiledXPath = compiledXPath;
			clone.depthLookup = depthLookup;
			clone.treeDepth = treeDepth;
			return clone;
		}

		internal int Key
		{
			set { this.key = value; }
			get { return this.key; }
		}

		void Compile()
		{
			compiledXPath = new ArrayList();
			QueryBuilder builder = new QueryBuilder();

			builder.Build(xpath, compiledXPath, this.treeDepth);
			int lookupLength = ((BaseAxisQuery)compiledXPath[compiledXPath.Count - 2]).Depth + 1;

			depthLookup = new int[lookupLength];

			for (int i = 0; i < compiledXPath.Count - 1; ++i)
			{
				if (depthLookup[((BaseAxisQuery)compiledXPath[i]).Depth] == 0)
				{
					depthLookup[((BaseAxisQuery)compiledXPath[i]).Depth] = i;
				}
			}
		}

		public string XPath
		{
			get { return xpath; }
			set { xpath = value; }
		}

		[SuppressWeaklyTypedCollectionMessageAttribute]
		public ArrayList GetXPathQueries
		{
			get { return compiledXPath; }
		}

		public override string ToString()
		{
			return xpath;
		}

		internal bool Match()
		{
			return matchState;
		}

		internal void SetMatchState(XPathReader reader)
		{
			if (matchIndex < 1)
			{
				return;
			}

			int queryCount = compiledXPath.Count - 1;
			int queryDepth = ((BaseAxisQuery)compiledXPath[matchIndex - 1]).Depth;

			if (matchCount == queryCount && queryDepth == reader.Depth)
			{
				matchState = true;
			}
		}

		internal bool IsAttributeQuery()
		{
			if (compiledXPath[matchIndex] is AttributeQuery)
			{
				return true;
			}
			return false;
		}

		internal void ResetMatching(XPathReader reader)
		{
			matchState = false;

			int count = compiledXPath.Count;

			if (reader.Depth < ((BaseAxisQuery)compiledXPath[matchIndex]).Depth)
			{
				matchIndex = depthLookup[reader.Depth];
				matchCount = matchIndex + 1;
			}

			if (matchCount == count - 1 && matchIndex > 0)
			{
				--matchCount;
				--matchIndex;
			}
		}

		internal void Advance(XPathReader reader)
		{
			ResetMatching(reader);

			if ((Query)compiledXPath[matchIndex] is DescendantQuery)
			{
				if (((Query)compiledXPath[matchIndex + 1]).MatchNode(reader))
				{
					matchIndex = matchIndex + 2;
					matchCount = matchIndex;

					for (int i = matchCount; i < compiledXPath.Count - 1; ++i)
					{
						((BaseAxisQuery)compiledXPath[matchIndex]).Depth += reader.Depth - 1;
					}
				}
			}
			else
			{
				while (reader.Depth == ((BaseAxisQuery)compiledXPath[matchIndex]).Depth)
				{
					if (((Query)compiledXPath[matchIndex]).MatchNode(reader))
					{
						++matchIndex;
						matchCount = matchIndex;
					}
					else
					{
						break;
					}
				}

				SetMatchState(reader);
			}
		}

		internal void AdvanceUntil(XPathReader reader)
		{
			Advance(reader);

			if (compiledXPath[matchIndex] is AttributeQuery)
			{
				reader.ProcessAttribute = reader.Depth + 1; // the attribute depth should be current element plus one
			}
		}
	}
}
