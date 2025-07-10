using System.IO;
using System.Xml.Linq;
using Microsoft.XmlDiffPatch;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class XmlDifferences
	{
		public static XmlDifferences Compare(string expectedXml, string actualXml)
		{
			var expectedDoc = XDocument.Load(new StringReader(expectedXml));
			var actualDoc = XDocument.Load(new StringReader(actualXml));

			var differences = new XDocument();
			using (var expectedReader = expectedDoc.CreateReader())
			using (var actualReader = actualDoc.CreateReader())
			using (var differencesWriter = differences.CreateWriter())
			{
				var xmlDiff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreWhitespace);
				xmlDiff.Compare(actualReader, expectedReader, differencesWriter);
			}
			return new XmlDifferences(differences);
		}

		XmlDifferences(XDocument xmlDiff)
		{
			Result = xmlDiff;
		}

		public XDocument Result { get; }

		public bool AreEquals => !Result.Root.HasElements;

		public override string ToString() => Result.ToString();
	}
}
