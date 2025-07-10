using System;
using System.Xml;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public abstract class LicenceSegment
	{
		protected LicenceSegment() { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public static string GetStringAttributeValueSafely(XmlNode node, string attributeName)
		{
			return GetStringAttributeValueSafely(node, attributeName, String.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public static string GetStringAttributeValueSafely(XmlNode node, string attributeName, string defaultValue)
		{
			XmlAttribute anAttribute = node != null ? node.Attributes[attributeName] : null;
			return (anAttribute != null) ? anAttribute.Value : defaultValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public static Guid GetGuidAttributeValueSafely(XmlNode node, string attributeName)
		{
			string val = GetStringAttributeValueSafely(node, attributeName);
			return (val.Length == 0) ? Guid.Empty : new Guid(val);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public static bool GetBoolAttributeValueSafely(XmlNode node, string attributeName)
		{
			string val = GetStringAttributeValueSafely(node, attributeName);
			return !(val.Length == 0) && bool.Parse(val);
		}
	}
}

