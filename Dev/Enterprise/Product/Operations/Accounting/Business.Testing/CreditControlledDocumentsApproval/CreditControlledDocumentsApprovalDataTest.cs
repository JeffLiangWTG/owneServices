using System;
using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class CreditControlledDocumentsApprovalDataTest : TestCase
	{
		public void TestSerialization()
		{
			var data = new CreditControlledDocumentsApprovalData();
			data.MenuItemPK = new ZGuid("B33EEE90-37AF-43A3-8452-FDF895DB64B7");
			data.ApproveAllDocuments = true;

			var serializer = ZXmlSerializer.New(typeof(CreditControlledDocumentsApprovalData));

			using (var sw = new StringWriter())
			using (var writer = XmlWriter.Create(sw))
			{
				serializer.Serialize(writer, data);
				writer.Flush();
				this.AssertXMLEqualsByDiff(@"<?xml version=""1.0"" encoding=""utf-16""?><CreditControlledDocumentsApprovalData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><MenuItemPK>b33eee90-37af-43a3-8452-fdf895db64b7</MenuItemPK><ApproveAllDocuments>true</ApproveAllDocuments><RejectionReason></RejectionReason></CreditControlledDocumentsApprovalData>", sw.ToString());
			}
		}

		public void TestDeserialization()
		{
			var serializer = ZXmlSerializer.New(typeof(CreditControlledDocumentsApprovalData));

			string xml = @"<?xml version=""1.0"" encoding=""utf-16""?><CreditControlledDocumentsApprovalData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><MenuItemPK>b33eee90-37af-43a3-8452-fdf895db64b7</MenuItemPK><ApproveAllDocuments>true</ApproveAllDocuments></CreditControlledDocumentsApprovalData>";

			using (var reader = new XmlTextReader(new StringReader(xml)))
			{
				var approvalData = (CreditControlledDocumentsApprovalData)serializer.Deserialize(reader);

				AssertEquals("approvalData.MenuItemPK", new ZGuid("B33EEE90-37AF-43A3-8452-FDF895DB64B7"), approvalData.MenuItemPK);
				AssertEquals("approvalData.ApproveAllDocuments", true, approvalData.ApproveAllDocuments);
			}
			//xsi:nil=""true""
			xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
						<CreditControlledDocumentsApprovalData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
							xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xsi:nil=""true"" />";

			using (var reader = new XmlTextReader(new StringReader(xml)))
			{
				var approvalData = (CreditControlledDocumentsApprovalData)serializer.Deserialize(reader);
				AssertNull(approvalData);
			}
			//without xsi:nil=""true""
			xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
						<CreditControlledDocumentsApprovalData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
								xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" />";

			using (var reader = new XmlTextReader(new StringReader(xml)))
			{
				var approvalData = (CreditControlledDocumentsApprovalData)serializer.Deserialize(reader);
				AssertEquals("approvalData.MenuItemPK", ZGuid.Empty, approvalData.MenuItemPK);
				AssertEquals("approvalData.ApproveAllDocuments", false, approvalData.ApproveAllDocuments);
			}

			//bad data
			xml = @"g902la59agkl2";

			using (var reader = new XmlTextReader(new StringReader(xml)))
			{
				try
				{
					var approvalData = (CreditControlledDocumentsApprovalData)serializer.Deserialize(reader);
				}
				catch (Exception ex)
				{
					Assert(ex is InvalidOperationException);
				}
			}
		}

		public void TestCanDeserialize()
		{
			var serialiser = ZXmlSerializer.New(typeof(CreditControlledDocumentsApprovalData));

			var objWithNewLine = new CreditControlledDocumentsApprovalData() { RejectionReason = "ABC\u000ADEF" };
			using (var ms = new MemoryStream())
			{
				serialiser.Serialize(ms, objWithNewLine);
				ms.Position = 0L;
				using (var xmlReader = XmlReader.Create(ms))
				{
					Assert("New lines can be deserialised ", serialiser.CanDeserialize(xmlReader));
				}
				ms.Position = 0L;
				var deserialisedNewLine = (CreditControlledDocumentsApprovalData)serialiser.Deserialize(ms);
				AssertEquals("New line should round trip", objWithNewLine.RejectionReason, deserialisedNewLine.RejectionReason);
			}

			var xmlWithExplicitNewLine = @"<?xml version=""1.0""?>
<CreditControlledDocumentsApprovalData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <MenuItemPK>00000000-0000-0000-0000-000000000000</MenuItemPK>
  <ApproveAllDocuments>false</ApproveAllDocuments>
  <RejectionReason>ABC&#xA;DEF</RejectionReason>
</CreditControlledDocumentsApprovalData>";
			using (var reader = new StringReader(xmlWithExplicitNewLine))
			using (var xmlReader = XmlReader.Create(reader))
			{
				Assert("New line as '&#xA;' can be deserialised", serialiser.CanDeserialize(xmlReader));
			}
			using (var reader = new StringReader(xmlWithExplicitNewLine))
			{
				var deserialisedNewLine = (CreditControlledDocumentsApprovalData)serialiser.Deserialize(reader);
				AssertEquals("New line is equivalent to '&#xA;' XML character sequence", objWithNewLine.RejectionReason, deserialisedNewLine.RejectionReason);
			}
		}
	}
}
