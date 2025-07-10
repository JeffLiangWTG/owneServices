using System.Xml.Linq;

namespace Enterprise.DataTransfer.Native.Utils
{
	public static class XElementExtension
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML attribute name")]
		public static void SetDefaultNameSpace(this XElement root, XNamespace ns, bool removeVersion = false)
		{
			var prev = root.Name.Namespace;
			foreach (var e in root.DescendantsAndSelf())
			{
				if (e.Name.Namespace == prev)
				{
					e.Name = ns + e.Name.LocalName;
				}
				if (removeVersion)
				{
					e.Attribute("version")?.Remove();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML attribute name")]
		public static void RemoveNameSpace(this XElement root, XNamespace ns = null)
		{
			foreach (var e in root.DescendantsAndSelf())
			{
				var nsAttribute = e.Attribute("xmlns");
				nsAttribute?.Remove();
				if (e.Name.Namespace == (ns ?? e.Name.Namespace))
				{
					e.Name = e.Name.LocalName;
				}
			}
		}
	}
}