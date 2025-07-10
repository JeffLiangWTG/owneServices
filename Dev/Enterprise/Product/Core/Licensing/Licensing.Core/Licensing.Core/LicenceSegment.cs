using System;
using System.Xml;

namespace Enterprise.Licensing
{
	public abstract class LicenceSegment
	{
#if DEBUG
		internal abstract void AddToLicenceNode(XmlNode licenceNode);
#endif
		internal abstract void LoadFromLicenceNode(XmlNode licenceNode);

		protected string GetStringAttributeValueSafely(XmlNode node, string attributeName)
		{
			return GetStringAttributeValueSafely(node, attributeName, String.Empty);
		}

		protected string GetStringAttributeValueSafely(XmlNode node, string attributeName, string defaultValue)
		{
			XmlAttribute anAttribute = node != null ? node.Attributes[attributeName] : null;
			return (anAttribute != null) ? anAttribute.Value : defaultValue;
		}

		protected Guid GetGuidAttributeValueSafely(XmlNode node, string attributeName)
		{
			string value = GetStringAttributeValueSafely(node, attributeName);
			return (value.Length == 0) ? Guid.Empty : new Guid(value);
		}

		protected bool GetBoolAttributeValueSafely(XmlNode node, string attributeName)
		{
			string value = GetStringAttributeValueSafely(node, attributeName);
			return !(value.Length == 0) && bool.Parse(value);
		}
	}
}
