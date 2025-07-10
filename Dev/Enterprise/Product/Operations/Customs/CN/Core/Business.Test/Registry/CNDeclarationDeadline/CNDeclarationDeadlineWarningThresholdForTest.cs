using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNDeclarationDeadlineWarningThresholdForTest : CNDeclarationDeadlineWarningThreshold
	{
		public CNDeclarationDeadlineWarningThresholdForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void WriteElementsForTest(XmlWriter writer)
		{
			writer.WriteStartDocument(true);
			writer.WriteStartElement("TEST");
			WriteElements(writer);
			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		public void ReadElementsForTest(XmlReaderWrapper wrapper) => ReadElements(wrapper);
	}
}
