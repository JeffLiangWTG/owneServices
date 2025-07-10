using System;
using System.Xml;

namespace Enterprise.Licensing.Testing
{
	sealed class LicenceSegmentDummy : LicenceSegment
	{
		public string StringAttributeValue(XmlNode node, string attributeName)
		{
			return base.GetStringAttributeValueSafely(node, attributeName);
		}

		public Guid GuidAttributeValue(XmlNode node, string attributeName)
		{
			return base.GetGuidAttributeValueSafely(node, attributeName);
		}

		public bool BoolAttributeValue(XmlNode node, string attributeName)
		{
			return base.GetBoolAttributeValueSafely(node, attributeName);
		}

		internal override void AddToLicenceNode(XmlNode licenceNode)
		{
			throw new NotImplementedException();
		}

		internal override void LoadFromLicenceNode(XmlNode licenceNode)
		{
			throw new NotImplementedException();
		}
	}
}
