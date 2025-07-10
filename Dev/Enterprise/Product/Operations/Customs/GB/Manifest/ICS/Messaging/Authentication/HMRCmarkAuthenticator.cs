using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	public class HMRCmarkAuthenticator
	{
		/// <summary>
		/// Returns the provided SOAP Message with a SHA1 token in the BinarySecurityToken tag.
		/// </summary>
		public string SignSoapMessage(string soapMessage)
		{
			var messageXml = new XmlDocument();
			messageXml.LoadXml(soapMessage);

			// The SHA needs be derived from the exact text of the message.
			// In addition, the Namespaces in the Envelope tag must be repeated in the body tag before signing.
			// Loading into a DOM and retrieving the node is not the same. The DOM tends to add other namespaces and adjust the formatting.
			// But the DOM can be useful for locating the tags.

			var bodyNode = messageXml.SelectSingleNode("/*[local-name()='Envelope']/*[local-name()='Body']");
			var bodyXml = SnipTagXmlFromMessage(soapMessage, bodyNode.Name);
			bodyXml = EnsureBodyHasEnvelopeNamespaces(messageXml, bodyNode, bodyXml);
			var token = CanonicalizeAndCalculateSHA1(bodyXml);

			var securityTokenNode = messageXml.SelectSingleNode("/*[local-name()='Envelope']/*[local-name()='Header']/*[local-name()='Security']/*[local-name()='BinarySecurityToken']");
			var signedSoapMessage = UpdateSecurityToken(soapMessage, securityTokenNode.Name, token);
			return signedSoapMessage;
		}

		/// <summary>
		/// Generates a SHA1 token from a Body XML.
		/// The XML needs to include the opening and closing Body tags and all namespaces.
		/// </summary>
		public string CanonicalizeAndCalculateSHA1(string xmlText)
		{
			var transform = new XmlDsigC14NTransform(true);
			var bytes = new UTF8Encoding().GetBytes(xmlText);
			var memStream = new MemoryStream(bytes, 0, bytes.Length);
			transform.LoadInput(memStream);

			var stream = (Stream)transform.GetOutput(typeof(Stream));
			var hash = SHA1.Create().ComputeHash(stream);
			return Convert.ToBase64String(hash);
		}

		string EnsureBodyHasEnvelopeNamespaces(XmlDocument doc, XmlNode bodyNode, string bodyXml)
		{
			var bodyTagEndIndex = bodyXml.IndexOf(">");
			var bodyTag = bodyXml.Substring(0, bodyTagEndIndex);
			var bodyContent = bodyXml.Substring(bodyTagEndIndex);

			var bodyAttributes = bodyNode.Attributes;
			foreach (XmlAttribute attrib in doc.DocumentElement.Attributes)
			{
				if (attrib.Prefix == "xmlns" && bodyAttributes.GetNamedItem(attrib.Name) == null)
				{
					bodyTag += $@" {attrib.Name}=""{attrib.Value}""";
				}
			}

			return bodyTag + bodyContent;
		}

		string UpdateSecurityToken(string soapMessage, string securityTokenTagName, string token)
		{
			var securityTokenXmlUnsigned = SnipTagXmlFromMessage(soapMessage, securityTokenTagName);

			var tokenTagEndIndex = securityTokenXmlUnsigned.IndexOf(">");
			var tokenTag = securityTokenXmlUnsigned.Substring(0, tokenTagEndIndex + 1);
			var securityTokenXmlSigned = string.Concat(tokenTag, token, $"</{securityTokenTagName}>");

			return soapMessage.Replace(securityTokenXmlUnsigned, securityTokenXmlSigned);
		}

		string SnipTagXmlFromMessage(string soapMessage, string bodyTagName)
		{
			var bodyStartIndex = soapMessage.IndexOf($"<{bodyTagName}");
			var bodyEndIndex = soapMessage.IndexOf($"</{bodyTagName}>");

			return soapMessage.Substring(bodyStartIndex, bodyEndIndex - bodyStartIndex + bodyTagName.Length + 3);
		}
	}
}
