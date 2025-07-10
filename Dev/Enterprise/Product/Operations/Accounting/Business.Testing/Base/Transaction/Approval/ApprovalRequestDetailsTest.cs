using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	public abstract class ApprovalRequestDetailsTest<DetailsType> : NonPersistentBusinessObjectTestCase where DetailsType : ApprovalRequestDetails
	{
		public virtual void TestOpertorEqual()
		{
			ApprovalRequestDetails a = null;
			ApprovalRequestDetails b = null;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a = (ApprovalRequestDetails)GetNewBusinessObject();
			b = null;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			a = null;
			b = (ApprovalRequestDetails)GetNewBusinessObject();
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			a = (ApprovalRequestDetails)GetNewBusinessObject();
			b = (ApprovalRequestDetails)GetNewBusinessObject();
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			a.MaxAmountToApprove = 1M;
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			b.MaxAmountToApprove = 1M;
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);
		}

		#region Decimal Places

		public void TestZDecimalsHaveCorrectDecimalPlaces()
		{
			var postingRequest = (ApprovalRequestDetails)GetNewBusinessObject();

			var localList = new List<string>
			{
				nameof(postingRequest.MaxAmountToApprove),
			};

			var tester = new DecimalPlacesAttributeTester(postingRequest);
			tester.CheckLocalCurrency(localList, nameof(postingRequest.LocalDecimals));
		}

		public void TestDecimalPlacesAttributeApplyToAllZDecimalProperties()
		{
			var properties = typeof(ApprovalRequestDetails).GetProperties().Where(x => x.PropertyType == typeof(ZDecimal)).ToList();
			Assert(properties.All(x => Attribute.IsDefined(x, typeof(DecimalPlacesAttribute))));
		}

		#endregion

		public void TestCopyFrom()
		{
			var postingRequest = (ApprovalRequestDetails)GetNewBusinessObject();

			postingRequest.MaxAmountToApprove = 11M;

			var newPostingRequest = (ApprovalRequestDetails)GetNewBusinessObject();

			newPostingRequest.CopyFrom(postingRequest);
			AssertEquals("MaxAmountToApprove", 11M, newPostingRequest.MaxAmountToApprove);
		}

		public abstract void TestIsPostingActionTheSame();

		#region Serialization

		[TestDate(2014, 11, 12)]
		public void TestEmptyObjectSerialization()
		{
			using (MemoryStream writerStream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(writerStream, Encoding.Unicode))
			{
				var collection = (ApprovalRequestDetails)GetNewBusinessObject();
				var serializer = ZXmlSerializer.New(collection.GetType());

				serializer.Serialize(writer, collection);
				writer.Flush();
				var xml = Encoding.Unicode.GetString(writerStream.ToArray());
				var expectedXML = GetExpectedEmptyXML();
				this.AssertXMLEqualsByDiff("Resulting XML", expectedXML, xml);

				using (MemoryStream readerStream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
				using (XmlTextReader reader = new XmlTextReader(readerStream))
				{
					AssertNoExceptionThrown(() => serializer.Deserialize(reader));
				}
			}
		}

		[TestDate(2014, 11, 12)]
		public virtual void TestReadXML()
		{
			var xml = GetExpectedXML(true);

			using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				var serializer = ZXmlSerializer.New(typeof(DetailsType));
				var details = (DetailsType)serializer.Deserialize(reader);
				AssertFullyPopulatedBusinessObjectForXMLTest(details);
			}
		}

		[TestDate(2014, 11, 12)]
		public virtual void TestWriteXML()
		{
			var details = GetNewFullyPopulatedBusinessObjectForXMLTest();

			using (MemoryStream stream = new MemoryStream())
			using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.Unicode))
			{
				var serializer = ZXmlSerializer.New(typeof(DetailsType));
				serializer.Serialize(writer, details);
				writer.Flush();
				var xml = Encoding.Unicode.GetString(stream.ToArray());
				xml = PrepareRealXMLForComparison(xml, details);
				var expectedXML = GetExpectedXML(false);
				this.AssertXMLEqualsByDiff("Resulting XML", expectedXML, xml);
			}
		}

		[TestDate(2014, 11, 12)]
		public virtual void TestReadLegacyXMLToCheckBacwardCompatibility()
		{
			var xml = GetLegacyXML();

			using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				var serializer = ZXmlSerializer.New(typeof(DetailsType));
				var details = (DetailsType)serializer.Deserialize(reader);
				AssertFullyPopulatedBusinessObjectForXMLTest(details, false);
			}
		}

		protected abstract string GetExpectedXML(bool isReadTest);
		protected abstract string GetExpectedEmptyXML();
		protected abstract string GetLegacyXML();
		protected virtual string PrepareRealXMLForComparison(string xml, DetailsType details)
		{
			return xml;
		}

		protected virtual DetailsType GetNewFullyPopulatedBusinessObjectForXMLTest()
		{
			var charge = (DetailsType)GetNewBusinessObject();

			charge.MaxAmountToApprove = 100;
			charge.Description = "HeaderDesc";
			charge.InvoiceTerm = Enterprise.Core.Constants.InvoiceTerms.FromCustomsClearanceDate;
			charge.InvoiceTermDays = 4;

			return charge;
		}

		protected virtual void AssertFullyPopulatedBusinessObjectForXMLTest(DetailsType details, bool withNewFields = true)
		{
			AssertEquals("MaxAmountToApprove", 100M, details.MaxAmountToApprove);

			AssertEquals(withNewFields ? "HeaderDesc" : string.Empty, details.Description);
			AssertEquals(withNewFields ? Core.Constants.InvoiceTerms.FromCustomsClearanceDate : string.Empty, details.InvoiceTerm);
			AssertEquals(withNewFields ? (ZByte)4 : (ZByte)0, details.InvoiceTermDays);
		}

		#endregion

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
