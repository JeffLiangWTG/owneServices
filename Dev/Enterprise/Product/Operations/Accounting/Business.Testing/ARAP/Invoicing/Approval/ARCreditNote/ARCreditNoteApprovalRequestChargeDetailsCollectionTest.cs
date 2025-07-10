using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalRequestChargeDetailsCollection))]
	public class ARCreditNoteApprovalRequestChargeDetailsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ARCreditNoteApprovalRequestChargeDetailsCollection>
	{
		#region Serialization

		public void TestEmptyCollectionSerialization()
		{
			using (MemoryStream writerStream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(writerStream, Encoding.Unicode))
			{
				var collection = GetCollectionToTest();
				AssertEquals("Precondition: collection.Count", 0, collection.Count);
				var serializer = ZXmlSerializer.New(collection.GetType());
				serializer.Serialize(writer, collection);
				writer.Flush();
				var xml = Encoding.Unicode.GetString(writerStream.ToArray());
				var expectedXML = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfChargeDetails />";
				this.AssertXMLEqualsByDiff("Resulting XML", expectedXML, xml);

				using (MemoryStream readerStream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
				using (XmlTextReader reader = new XmlTextReader(readerStream))
				{
					var deserializedCollection = (ARCreditNoteApprovalRequestChargeDetailsCollection)serializer.Deserialize(reader);
					AssertEquals("collection.Count", 0, deserializedCollection.Count);
				}
			}
		}

		public void TestCollectionSerialization()
		{
			using (MemoryStream writerStream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(writerStream, Encoding.Unicode))
			{
				var collection = GetCollectionToTest();
				var element = (ARCreditNoteApprovalRequestChargeDetails)GetNewElementToAddToTheCollection();
				element.JobNumber = "10";
				element.ChargeCode = "FRT";
				element.Branch = "BRN";
				element.Department = "DEP";
				element.Description = "description1";
				var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
				element.AccInvMsgPK = taxMessage.PK;
				element.TaxDate = new ZDate(2020, 3, 13);

				collection.Add(element);
				element = (ARCreditNoteApprovalRequestChargeDetails)GetNewElementToAddToTheCollection();
				element.JobNumber = "20";
				element.ChargeCode = "BAF";
				element.Branch = "SYD";
				element.Department = "FEA";
				collection.Add(element);

				var serializer = ZXmlSerializer.New(collection.GetType());
				serializer.Serialize(writer, collection);
				writer.Flush();
				var xml = Encoding.Unicode.GetString(writerStream.ToArray());
				var expectedXML = $"\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfChargeDetails><ChargeDetails><JobNumber>10</JobNumber><ChargeCode>FRT</ChargeCode><Branch>BRN</Branch><Department>DEP</Department><Description>description1</Description><AccInvMsgPK>{taxMessage.PK}</AccInvMsgPK><TaxDate>13-Mar-20</TaxDate><SellAccount /><SellCurrency /><OSSellAmount>0</OSSellAmount><LocalSellAmount>0</LocalSellAmount><InvoiceType /><OSTaxAmount>0</OSTaxAmount><LocalTaxAmount>0</LocalTaxAmount><ExchangeRate>0</ExchangeRate><TaxCode /><SupplyType /></ChargeDetails><ChargeDetails><JobNumber>20</JobNumber><ChargeCode>BAF</ChargeCode><Branch>SYD</Branch><Department>FEA</Department><Description /><AccInvMsgPK>00000000-0000-0000-0000-000000000000</AccInvMsgPK><TaxDate /><SellAccount /><SellCurrency /><OSSellAmount>0</OSSellAmount><LocalSellAmount>0</LocalSellAmount><InvoiceType /><OSTaxAmount>0</OSTaxAmount><LocalTaxAmount>0</LocalTaxAmount><ExchangeRate>0</ExchangeRate><TaxCode /><SupplyType /></ChargeDetails></ArrayOfChargeDetails>";
				this.AssertXMLEqualsByDiff("Resulting XML", expectedXML, xml);

				using (MemoryStream readerStream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
				using (XmlTextReader reader = new XmlTextReader(readerStream))
				{
					var deserializedCollection = (ARCreditNoteApprovalRequestChargeDetailsCollection)serializer.Deserialize(reader);
					AssertEquals("collection.Count", 2, deserializedCollection.Count);
					var deserializedElement = deserializedCollection[0];
					AssertEquals("JobNumber", "10", deserializedElement.JobNumber);
					AssertEquals("ChargeCode", "FRT", deserializedElement.ChargeCode);
					AssertEquals("Branch", "BRN", deserializedElement.Branch);
					AssertEquals("Department", "DEP", deserializedElement.Department);

					deserializedElement = deserializedCollection[1];
					AssertEquals("JobNumber", "20", deserializedElement.JobNumber);
					AssertEquals("ChargeCode", "BAF", deserializedElement.ChargeCode);
					AssertEquals("Branch", "SYD", deserializedElement.Branch);
					AssertEquals("Department", "FEA", deserializedElement.Department);
				}
			}
		}

		#endregion

		protected override ARCreditNoteApprovalRequestChargeDetailsCollection GetCollectionToTest()
		{
			return new ARCreditNoteApprovalRequestChargeDetailsCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ARCreditNoteApprovalRequestChargeDetails(Factory);
		}
	}
}
