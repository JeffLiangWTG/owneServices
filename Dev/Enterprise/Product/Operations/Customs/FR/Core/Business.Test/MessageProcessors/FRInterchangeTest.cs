using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	[TestedType(typeof(FRInterchange))]
	public class FRInterchangeTest : EDIInterchangeTest
	{
		public void TestInterchangeNumberReplacement()
		{
			var strBuilder = new StringBuilder();
			var writer = XmlTextWriter.Create(strBuilder, new XmlWriterSettings() { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
			writer.WriteStartElement("Node");
			writer.WriteElementString("InterchangeNumber", EDIInterchange.InterchangeNumberPlaceHolder);
			writer.Flush();
			writer.Close();

			var ediInterchange = Factory.New<FRInterchange>();
			ediInterchange.EI_HeaderText = $"header+{strBuilder}";
			ediInterchange.EI_BodyText = $"body+{strBuilder}";
			ediInterchange.EI_InterchangeNum = "100";

			Factory.Save();

			AssertEquals("header+<Node><InterchangeNumber>100</InterchangeNumber></Node>", ediInterchange.EI_HeaderText);
			AssertEquals("body+<Node><InterchangeNumber>100</InterchangeNumber></Node>", ediInterchange.EI_BodyText);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<FRInterchange>();
		}
	}
}
