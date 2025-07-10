using System;
using System.Xml;

namespace Enterprise.DataTransfer.Xml
{
	public class RootNamespaceRenamingXmlReader : XmlReaderDelegator
	{
		public RootNamespaceRenamingXmlReader(XmlReader inner, string rootNamespace) : base(inner)
		{
			this.RootNamespace = inner.NameTable.Get(rootNamespace);
			if (this.RootNamespace == null)
			{
				this.RootNamespace = inner.NameTable.Add(rootNamespace);
			}
		}

		public readonly string RootNamespace;

		public override string NamespaceURI
		{
			get
			{
				string result = base.NamespaceURI;

				if (NodeType == XmlNodeType.Element && !ReadStartElementFirstCalled)
				{
					result = RootNamespace;
				}

				return result;
			}
		}

		public override bool IsStartElement(string localname, string ns)
		{
			bool result;

			if (!ReadStartElementFirstCalled)
			{
				MoveToContent();
				result = Inner.IsStartElement(localname, base.NamespaceURI);
			}
			else
			{
				result = Inner.IsStartElement(localname, ns);
			}

			return result;
		}

		public override void ReadStartElement(string localname, string ns)
		{
			if (!ReadStartElementFirstCalled)
			{
				MoveToContent();
				Inner.ReadStartElement(localname, base.NamespaceURI);
				ReadStartElementFirstCalled = true;
			}
			else
			{
				Inner.ReadStartElement(localname, ns);
			}
		}

		public override void ReadStartElement(string name)
		{
			if (!ReadStartElementFirstCalled)
			{
				MoveToContent();
				Inner.ReadStartElement(name, base.NamespaceURI);
				ReadStartElementFirstCalled = true;
			}
			else
			{
				Inner.ReadStartElement(name);
			}
		}

		public override string ReadOuterXml()
		{
			if (!ReadStartElementFirstCalled)
			{
				throw new NotImplementedException("This is not implemented because it requires special handling");
			}
			else
			{
				return base.ReadOuterXml();
			}
		}

		bool ReadStartElementFirstCalled;
	}
}
