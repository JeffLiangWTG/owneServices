using System.Xml;

namespace Enterprise.DataTransfer.Xml
{
	public class RootElementRenamingXmlWriter : XmlWriterDelegator
	{
		public RootElementRenamingXmlWriter(XmlWriter inner, string rootElementName) : base(inner)
		{
			this.RootElementName = rootElementName;
		}

		public readonly string RootElementName;

		public override void WriteStartElement(string prefix, string localName, string ns)
		{
			if (!fWriteStartElementFirstCalled)
			{
				Inner.WriteStartElement(prefix, RootElementName, ns);
				fWriteStartElementFirstCalled = true;
			}
			else
			{
				Inner.WriteStartElement(prefix, localName, null);
			}
		}

		#region Implementation

		bool fWriteStartElementFirstCalled;

		#endregion
	}
}
