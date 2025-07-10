using System.Collections;
using System.IO;
using System.Xml;

namespace Enterprise.eHubMessaging.Business
{
	internal enum ReadMethods
	{
		Read,
		ReadUntil,
		MoveToAttribute,
		MoveToElement,
		None
	}

	public class XPathReader : XmlReader
	{
		readonly XmlReader reader;
		readonly XPathCollection xpathCollection;
		int processAttribute = -1;
		ReadMethods readMethod = ReadMethods.None;

		XPathReader()
		{
		}

		public XPathReader(XmlReader reader, XPathCollection xc)
			: this()
		{
			xpathCollection = xc;
			xc.SetReader = this;
			this.reader = reader;
		}

		public XPathReader(string url, string xpath)
			: this()
		{
			this.reader = new XmlTextReader(url);
			xpathCollection = new XPathCollection();
			xpathCollection.Add(xpath);
		}

		public XPathReader(TextReader reader, string xpath)
			: this()
		{
			this.reader = new XmlTextReader(reader);
			xpathCollection = new XPathCollection();
			xpathCollection.Add(xpath);
		}

		public XPathReader(string url, XPathCollection xc)
			: this(new XmlTextReader(url), xc)
		{
		}

		public bool Match(int queryIndex)
		{
			if (xpathCollection[queryIndex] != null)
			{
				return (xpathCollection[queryIndex].Match());
			}
			else
			{
				return false;
			}
		}

		public bool Match(string xpathQuery)
		{
			return true;
		}

		public bool Match(XPathQuery xpathExpr)
		{
			if (xpathCollection.Contains(xpathExpr) && xpathExpr.Match())
			{
				return true;
			}

			return false;
		}

		public bool MatchesAny(ArrayList queryList)
		{
			return (xpathCollection.MatchesAny(queryList, this.reader.Depth));
		}

		public bool ReadUntilMatch()
		{
			while (true)
			{
				if (this.processAttribute > 0)
				{
					if (MoveToNextAttribute())
					{
						if (xpathCollection.MatchAnyQuery())
						{
							return true;
						}
					}
					else
					{
						this.processAttribute = -1;
					}
				}
				else if (this.reader.Read())
				{
					xpathCollection.AdvanceUntil(this);
					if (xpathCollection.MatchAnyQuery())
					{
						return true;
					}
				}
				else
				{
					return false;
				}
			}
		}

		public override bool Read()
		{
			this.readMethod = ReadMethods.Read;

			bool ret = true;
			ret = this.reader.Read();

			if (ret)
			{
				xpathCollection.Advance(this);
			}
			return ret;
		}

		public override bool MoveToAttribute(string name)
		{
			this.readMethod = ReadMethods.MoveToAttribute;

			bool ret = false;

			ret = this.reader.MoveToAttribute(name);

			if (ret)
			{
				xpathCollection.Advance(this);
			}
			return ret;
		}

		public override bool MoveToAttribute(string name, string ns)
		{
			bool ret = false;
			ret = this.reader.MoveToAttribute(name, ns);

			if (ret)
			{
				xpathCollection.Advance(this);
			}
			return ret;
		}

		public override void MoveToAttribute(int i)
		{
			this.readMethod = ReadMethods.MoveToAttribute;
			this.reader.MoveToAttribute(i);
			xpathCollection.Advance(this);
		}

		public override bool MoveToFirstAttribute()
		{
			bool ret = false;

			ret = this.reader.MoveToFirstAttribute();

			if (ret)
			{
				xpathCollection.Advance(this);
			}
			return ret;
		}

		public override bool MoveToNextAttribute()
		{
			bool ret = false;

			ret = this.reader.MoveToNextAttribute();

			if (ret)
			{
				xpathCollection.Advance(this);
			}
			return ret;
		}

		public override bool MoveToElement()
		{
			bool ret = false;

			readMethod = ReadMethods.MoveToElement;

			ret = this.reader.MoveToElement();

			if (ret)
			{
				xpathCollection.Advance(this);
			}

			return ret;
		}

		public override XmlNodeType NodeType
		{
			get { return (this.reader.NodeType); }
		}

		public override string Name
		{
			get { return (this.reader.Name); }
		}

		public override string LocalName
		{
			get { return (this.reader.LocalName); }
		}

		public override string NamespaceURI
		{
			get { return (this.reader.NamespaceURI); }
		}

		public override string Prefix
		{
			get { return (this.reader.Prefix); }
		}

		public override bool HasValue
		{
			get { return (this.reader.HasValue); }
		}

		public override string Value
		{
			get { return (this.reader.Value); }
		}

		public override int Depth
		{
			get { return (this.reader.Depth); }
		}

		public override string BaseURI
		{
			get { return (this.reader.BaseURI); }
		}

		public override bool IsEmptyElement
		{
			get { return (this.reader.IsEmptyElement); }
		}

		public override bool IsDefault
		{
			get { return (this.reader.IsDefault); }
		}

		public override char QuoteChar
		{
			get { return (this.reader.QuoteChar); }
		}

		public override XmlSpace XmlSpace
		{
			get { return (this.reader.XmlSpace); }
		}

		public override string XmlLang
		{
			get { return (this.reader.XmlLang); }
		}

		public override int AttributeCount
		{
			get { return (this.reader.AttributeCount); }
		}

		public override string GetAttribute(string name)
		{
			return this.reader.GetAttribute(name);
		}

		public override string GetAttribute(string name, string namespaceURI)
		{
			return this.reader.GetAttribute(name, namespaceURI);
		}

		public override string GetAttribute(int i)
		{
			return this.reader.GetAttribute(i);
		}

		public override string this[int i]
		{
			get { return this.reader[i]; }
		}

		public override string this[string name]
		{
			get { return this.reader[name]; }
		}

		public override string this[string name, string namespaceURI]
		{
			get { return this.reader[name, namespaceURI]; }
		}

		public override bool CanResolveEntity
		{
			get { return this.reader.CanResolveEntity; }
		}

		public override bool EOF
		{
			get { return this.reader.EOF; }
		}

		public override void Close()
		{
			this.reader.Close();
		}

		public override ReadState ReadState
		{
			get { return this.reader.ReadState; }
		}

		public override string ReadString()
		{
			return this.reader.ReadString();
		}

		public override XmlNameTable NameTable
		{
			get { return this.reader.NameTable; }
		}

		public override string LookupNamespace(string prefix)
		{
			return this.reader.LookupNamespace(prefix);
		}
		public override void ResolveEntity()
		{
			this.reader.ResolveEntity();
		}

		public override bool ReadAttributeValue()
		{
			return this.reader.ReadAttributeValue();
		}

		public override string ReadInnerXml()
		{
			return this.reader.ReadInnerXml();
		}

		public override string ReadOuterXml()
		{
			return this.reader.ReadOuterXml();
		}

		internal XmlReader BaseReader
		{
			get { return this.reader; }
		}

		internal int ProcessAttribute
		{
			get { return this.processAttribute; }
			set { this.processAttribute = value; }
		}

		internal ReadMethods ReadMethod
		{
			get { return readMethod; }
		}

		internal bool MapPrefixWithNamespace(string prefix)
		{
			XmlNamespaceManager nsMgr = xpathCollection.NamespaceManager;

			if (nsMgr != null && nsMgr.LookupNamespace(prefix) == this.NamespaceURI)
			{
				return true;
			}

			return false;
		}
	}
}
