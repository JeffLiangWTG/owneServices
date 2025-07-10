using System.Xml.Linq;
using CargoWise.Common;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	abstract class XmlBuilder
	{
		protected XmlBuilder(XNamespace xNamespace)
		{
			Argument.NotNull(xNamespace, nameof(xNamespace));
			XNamespace = xNamespace;
		}

		protected XName GetTagName(string tagName) => XNamespace.GetName(tagName);

		XNamespace XNamespace { get; }
	}
}
