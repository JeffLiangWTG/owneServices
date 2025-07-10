using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(FriendlyCodeWithPointers))]
	public abstract class FriendlyCodeWithPointerAbstractTest<T, TMetaData> : TestCaseWithFactory
		where T : FriendlyCodeWithPointers
		where TMetaData : class
	{
		protected abstract string RejectionMessageXml { get; }
		protected abstract string RequestMessageXml { get; }

		public virtual T[] GetResponses(TMetaData metaData, XElement requestXml) => System.Array.Empty<T>();

		public virtual void TestEndToEnd()
		{
			var requestXml = GetRequestMessageXml();
			var metaData = GetRejectionMessageXml();
			var helpers = GetResponses(metaData, requestXml);

			AssertEquals(7, helpers.Length);

			var pointerHelper = helpers.First(c => c.Code == "DMS10001");

			CombineAssertions("DMS10001", () =>
			{
				AssertEquals("ActualXPath", "/*[local-name()='Declaration']/*[local-name()='GoodsShipment']/*[local-name()='GovernmentAgencyGoodsItem'][4]/*[local-name()='Commodity']/*[local-name()='GoodsMeasure']/*[local-name()='NetNetWeightMeasure']", pointerHelper.ActualXPath);  // GovernmentAgencyGoodItem[SequenceNumeric=4] will always be the same as GovernmentAgencyGoodItem[4]. 
				AssertEquals("FinalFieldName", "Net net weight", pointerHelper.FinalFieldName);
				AssertEquals("FinalFieldValue", "100", pointerHelper.FinalFieldValue);
				AssertEquals("LineNumber", "4", pointerHelper.LineNumber);
				AssertEquals("PseudoXpath", "Declaration/GoodsShipment/GovernmentAgencyGoodsItem[4]/Commodity/GoodsMeasure/NetNetWeightMeasure", pointerHelper.PseudoXpath);
				AssertEquals("TagIdsPath", "42A/67A[1]/68A[4]/23A/65A/128[1]", pointerHelper.TagIdsPath);
			});

			pointerHelper = helpers.First(c => c.Code == "DMS12056");

			CombineAssertions("DMS12056", () =>
			{
				AssertEquals("ActualXPath", "/*[local-name()='Declaration']/*[local-name()='GoodsShipment']/*[local-name()='GovernmentAgencyGoodsItem']/*[local-name()='AdditionalDocument']/*[local-name()='CategoryCode']", pointerHelper.ActualXPath);
				AssertEquals("FinalFieldName", "Document category, coded", pointerHelper.FinalFieldName);
				AssertEquals("FinalFieldValue", "absent", pointerHelper.FinalFieldValue);
				AssertEquals("LineNumber", "1", pointerHelper.LineNumber);
				AssertEquals("PseudoXpath", "Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument/CategoryCode", pointerHelper.PseudoXpath);
				AssertEquals("TagIdsPath", "42A/67A[1]/68A[1]/02A/D031[1]", pointerHelper.TagIdsPath);
			});

			pointerHelper = helpers.First(c => c.Code == "CDS10001");

			CombineAssertions("CDS10001", () =>
			{
				AssertEquals("ActualXPath", "/*[local-name()='Declaration']/*[local-name()='GoodsShipment']/*[local-name()='GovernmentAgencyGoodsItem']/*[local-name()='StatisticalValueAmount']", pointerHelper.ActualXPath);
				AssertEquals("FinalFieldName", "Statistical value", pointerHelper.FinalFieldName);
				AssertEquals("FinalFieldValue", "10", pointerHelper.FinalFieldValue);
				AssertEquals("LineNumber", "1", pointerHelper.LineNumber);
				AssertEquals("PseudoXpath", "Declaration/GoodsShipment/GovernmentAgencyGoodsItem/StatisticalValueAmount", pointerHelper.PseudoXpath);
				AssertEquals("TagIdsPath", "42A/67A/68A[1]/114[1]", pointerHelper.TagIdsPath);
			});

			pointerHelper = helpers.First(c => c.Code == "CDS12056");

			CombineAssertions("CDS12056", () =>
			{
				AssertEquals("ActualXPath", "/*[local-name()='Declaration']/*[local-name()='GoodsShipment']/*[local-name()='GovernmentAgencyGoodsItem']/*[local-name()='AdditionalDocument']/*[local-name()='TypeCode']", pointerHelper.ActualXPath);
				AssertEquals("FinalFieldName", "Additional document type, coded", pointerHelper.FinalFieldName);
				AssertEquals("FinalFieldValue", "absent", pointerHelper.FinalFieldValue);
				AssertEquals("LineNumber - Should be empty as there are more than one goods items.", "", pointerHelper.LineNumber);
				AssertEquals("PseudoXpath", "Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument/TypeCode", pointerHelper.PseudoXpath);
				AssertEquals("TagIdsPath", "42A/67A/68A/02A/D006[1]", pointerHelper.TagIdsPath);
			});

			pointerHelper = helpers.First(c => c.Code == "CDS10020");

			CombineAssertions("CDS10020", () =>
			{
				AssertEquals("ActualXPath", "/*[local-name()='Declaration']/*[local-name()='GoodsShipment']/*[local-name()='GovernmentAgencyGoodsItem']/*[local-name()='AdditionalDocument'][4]/*[local-name()='ID']", pointerHelper.ActualXPath);
				AssertEquals("FinalFieldName", "Additional document reference number", pointerHelper.FinalFieldName);
				AssertEquals("FinalFieldValue", "absent", pointerHelper.FinalFieldValue);
				AssertEquals("LineNumber", "1", pointerHelper.LineNumber);
				AssertEquals("PseudoXpath", "Declaration/GoodsShipment/GovernmentAgencyGoodsItem/AdditionalDocument[4]/ID", pointerHelper.PseudoXpath);
				AssertEquals("TagIdsPath", "42A/67A[1]/68A[1]/02A[4]/D005[1]", pointerHelper.TagIdsPath);
			});

			pointerHelper = helpers.First(c => c.Code == "CDS12070");
			CombineAssertions("CDS12070", () =>
			{
				AssertEquals("ActualXPath", "/*[local-name()='Declaration']/*[local-name()='AdditionalDocument'][4]/*[local-name()='CategoryCode']", pointerHelper.ActualXPath);
				AssertEquals("FinalFieldName", "Document category, coded", pointerHelper.FinalFieldName);
				AssertEquals("FinalFieldValue", "absent", pointerHelper.FinalFieldValue);
				AssertEquals("LineNumber", "", pointerHelper.LineNumber);
				AssertEquals("PseudoXpath", "Declaration/AdditionalDocument[4]/CategoryCode", pointerHelper.PseudoXpath);
				AssertEquals("TagIdsPath", "42A/02A[4]/D031[1]", pointerHelper.TagIdsPath);
			});
		}

		public ZString GetRejectionMessageXmlContent()
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream(RejectionMessageXml))
			using (var streamReader = new StreamReader(stream))
			{
				return streamReader.ReadToEnd();
			}
		}

		public TMetaData GetRejectionMessageXml()
		{
			return XmlObjectSerializer.Deserialize<TMetaData>(GetRejectionMessageXmlContent());
		}

		public ZString GetRequestMessageXmlContent()
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream(RequestMessageXml))
			using (var streamReader = new StreamReader(stream))
			{
				return streamReader.ReadToEnd();
			}
		}

		public XElement GetRequestMessageXml()
		{
			return XElement.Parse(GetRequestMessageXmlContent(), LoadOptions.None);
		}
	}
}
