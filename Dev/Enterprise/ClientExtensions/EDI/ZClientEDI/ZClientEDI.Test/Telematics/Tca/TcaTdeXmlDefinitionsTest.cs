using Enterprise.Client.EDI.Telematics.Tca;
using NUnit.Framework;

namespace ZClientEDI.Test.Telematics.Tca
{
	public abstract class TcaXmlTestCase : TestCase
	{
		protected void CheckXml<T>(string xml, T message, string schema)
		{
			// Arrange
			// Act
			var serialized = TcaXmlSerializer.SerializeToTelematicsRimData(message, schema);

			// Assert
			this.AssertXMLEqualsIgnoreChildOrder($"{typeof(T).Name} failed validation", xml, serialized);
		}
	}
}
