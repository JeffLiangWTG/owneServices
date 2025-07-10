using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.EInvoicing.KoreaSouth;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.eInvoicing.KoreaSouth.Testing
{
	sealed class XmlToAdditionalInfoConverterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDescriptionTextFromXML()
		{
			var xml = File.ReadAllText(GetTestFilePath("TaxInvoice.xml"));

			AssertEquals(0, GetDescriptionTextAfterConversion("").Count());

			AssertEquals(0, GetDescriptionTextAfterConversion("<DescriptionText/>").Count());
			AssertEquals(0, GetDescriptionTextAfterConversion("<DescriptionText/><DescriptionText/>").Count());

			AssertEquals(0, GetDescriptionTextAfterConversion("<DescriptionText></DescriptionText>").Count());
			AssertEquals(0, GetDescriptionTextAfterConversion("<DescriptionText></DescriptionText><DescriptionText>   </DescriptionText>").Count());

			var oneLineOfDescriptionText = "<DescriptionText>AR INVOICE DESC 1</DescriptionText>";
			var twoLinesOfDescriptionText = @"<DescriptionText>AR INVOICE DESC 1</DescriptionText>
<DescriptionText>AR INVOICE DESC 2</DescriptionText>";
			var threeLinesOfDescriptionText = @"<DescriptionText>AR INVOICE DESC 1</DescriptionText>
<DescriptionText>AR INVOICE DESC 2</DescriptionText>
<DescriptionText>AR INVOICE DESC 3</DescriptionText>";

			AssertContainsExactElementsInExactOrder(new List<string> { "AR INVOICE DESC 1" }, GetDescriptionTextAfterConversion(oneLineOfDescriptionText));
			AssertContainsExactElementsInExactOrder(new List<string> { "AR INVOICE DESC 1", "AR INVOICE DESC 2" }, GetDescriptionTextAfterConversion(twoLinesOfDescriptionText));
			AssertContainsExactElementsInExactOrder(new List<string> { "AR INVOICE DESC 1", "AR INVOICE DESC 2", "AR INVOICE DESC 3" }, GetDescriptionTextAfterConversion(threeLinesOfDescriptionText));

			IEnumerable<ZString> GetDescriptionTextAfterConversion(string newDescriptionTextXML)
			{
				var oldDescriptionTextXML = "<DescriptionText>AR INVOICE</DescriptionText>";
				var newXml = xml.Replace(oldDescriptionTextXML, newDescriptionTextXML);

				var xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(newXml);

				var additionalInfo = new XmlToAdditionalInfoConverter().Convert(xmlDocument);

				return additionalInfo.DescriptionText;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetIssueIDFromXML()
		{
			var xml = File.ReadAllText(GetTestFilePath("TaxInvoice.xml"));
			var xmlDocument = new XmlDocument();

			xmlDocument.LoadXml(xml);
			AssertEquals("202206201234567800000001", XmlToAdditionalInfoConverter.GetIssueIDFromXML(xmlDocument));

			xmlDocument.LoadXml(xml.Replace("<IssueID>202206201234567800000001</IssueID>", "<IssueID/>"));
			AssertNullOrEmpty(XmlToAdditionalInfoConverter.GetIssueIDFromXML(xmlDocument));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetOriginalIssueIDFromXML()
		{
			var converter = new XmlToAdditionalInfoConverter();
			var xml = File.ReadAllText(GetTestFilePath("TaxInvoice.xml"));
			var xmlDocument = new XmlDocument();

			AssertNotContains("Precondition", "OriginalIssueID", xml);
			xmlDocument.LoadXml(xml);
			AssertNullOrEmpty(converter.Convert(xmlDocument).OriginalIssueID);

			xmlDocument.LoadXml(xml.Replace("<IssueID>202206201234567800000001</IssueID>", "<OriginalIssueID>202206201234567800000002</OriginalIssueID>"));
			AssertEquals("202206201234567800000002", converter.Convert(xmlDocument).OriginalIssueID);

			xmlDocument.LoadXml(xml.Replace("<IssueID>202206201234567800000001</IssueID>", "<OriginalIssueID/>"));
			AssertNullOrEmpty(converter.Convert(xmlDocument).OriginalIssueID);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetIssueDateTimeFromXML()
		{
			var xml = File.ReadAllText(GetTestFilePath("TaxInvoice.xml"));
			var xmlDocument = new XmlDocument();

			xmlDocument.LoadXml(xml);
			AssertEquals("20220620", XmlToAdditionalInfoConverter.GetIssueDateTimeFromXML(xmlDocument));

			xmlDocument.LoadXml(xml.Replace("<IssueDateTime>20220620</IssueDateTime>", "<IssueDateTime/>"));
			AssertNullOrEmpty(XmlToAdditionalInfoConverter.GetIssueDateTimeFromXML(xmlDocument));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvert()
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(File.ReadAllText(GetTestFilePath("TaxInvoice.xml")));

			var additionalInfo = new XmlToAdditionalInfoConverter().Convert(xmlDoc);
			AssertNotNull(additionalInfo);

			AssertEquals("202206201234567800000001", additionalInfo.IssueID);
			AssertEquals("20220620", additionalInfo.IssueDateTime);
			AssertEquals("04", additionalInfo.AmendStatusCode);
			AssertEquals("0101", additionalInfo.FullTypeCode);
			AssertEquals("02", additionalInfo.PaymentStatus);

			AssertEquals("AR INVOICE", additionalInfo.DescriptionText.Single());

			AssertEquals("40", additionalInfo.SpecifiedPaymentMeansTypeCode);
			AssertEquals("13186419753", additionalInfo.SpecifiedPaymentMeansPaidAmount);

			AssertEquals("11987654321", additionalInfo.SpecifiedMonetarySummationChargeTotalAmount);
			AssertEquals("1198765432", additionalInfo.SpecifiedMonetarySummationTaxTotalAmount);
			AssertEquals("13186419753", additionalInfo.SpecifiedMonetarySummationGrandTotalAmount);

			AssertEquals("2345678901", additionalInfo.InvoiceeID);
			AssertEquals("KOWLOON CANTON RAILWAY CORP", additionalInfo.InvoiceeNameText);
			AssertEquals("1234567890123456789012345678901234567890", additionalInfo.InvoiceeSpecifiedPersonNameText);
			AssertEquals("HO TUNG LAU STORES KCR WORKSHOP\r\nHO TUNG LAU, FOTAN, SHATIN, HONG KONG\r\nHong Kong", additionalInfo.InvoiceeSpecifiedAddressLineOneText);
			AssertEquals("12345678", additionalInfo.InvoiceeTypeCode);
			AssertEquals("1234567890", additionalInfo.InvoiceeClassificationCode);
			AssertEquals("jim.jim@cargowise.com", additionalInfo.InvoiceePrimaryDefinedContactURICommunication);
			AssertEquals("2nd.2nd@cargowise.com", additionalInfo.InvoiceeSecondaryDefinedContactURICommunication);

			AssertEquals("0987654321", additionalInfo.InvoicerID);
			AssertEquals("4 TRACK ADVENTURES", additionalInfo.InvoicerNameText);
			AssertEquals("12345678901234567890123456789012345678901234567890", additionalInfo.InvoicerSpecifiedPersonNameText);
			AssertEquals("P.O. BOX 540\r\nKUMEU\r\nAUCKLAND TEST\r\nNew Zealand", additionalInfo.InvoicerSpecifiedAddressLineOneText);
			AssertEquals("87654321", additionalInfo.InvoicerTypeCode);
			AssertEquals("77654321", additionalInfo.InvoicerClassificationCode);
			AssertEquals("joe.toe@cargowise.com", additionalInfo.InvoicerDefinedContactURICommunication);

			AssertEquals(2, additionalInfo.Lines.Count);

			AssertEquals("First Line", additionalInfo.Lines[0].DescriptionText);
			AssertEquals("1", additionalInfo.Lines[0].InvoiceAmount);
			AssertEquals("0", additionalInfo.Lines[0].CalculatedAmount);
			AssertEquals("First Line Name", additionalInfo.Lines[0].NameText);
			AssertEquals("20240126", additionalInfo.Lines[0].PurchaseExpiryDateTime);

			AssertEquals("Second Line", additionalInfo.Lines[1].DescriptionText);
			AssertEquals("20", additionalInfo.Lines[1].InvoiceAmount);
			AssertEquals("2", additionalInfo.Lines[1].CalculatedAmount);
			AssertEquals("Second Line Name", additionalInfo.Lines[1].NameText);
			AssertEquals("20240127", additionalInfo.Lines[1].PurchaseExpiryDateTime);
		}

		#region ConvertToAdditionalInfo

		public void TestConvertToAdditionalInfo_ReturnNull()
		{
			AssertNull(new XmlToAdditionalInfoConverter().Convert(null));
		}

		public void TestConvertToAdditionalInfo_ReturnEmpty()
		{
			var additionalInfo = new XmlToAdditionalInfoConverter().Convert(new XmlDocument());
			AssertNotNull(additionalInfo);

			AssertNullOrEmpty(additionalInfo.IssueID);
			AssertNullOrEmpty(additionalInfo.IssueDateTime);
			AssertNullOrEmpty(additionalInfo.AmendStatusCode);

			AssertNull(additionalInfo.DescriptionText);

			AssertNullOrEmpty(additionalInfo.SpecifiedPaymentMeansTypeCode);
			AssertNullOrEmpty(additionalInfo.SpecifiedPaymentMeansPaidAmount);

			AssertNullOrEmpty(additionalInfo.SpecifiedMonetarySummationChargeTotalAmount);
			AssertNullOrEmpty(additionalInfo.SpecifiedMonetarySummationTaxTotalAmount);
			AssertNullOrEmpty(additionalInfo.SpecifiedMonetarySummationGrandTotalAmount);

			AssertNullOrEmpty(additionalInfo.InvoiceeID);
			AssertNullOrEmpty(additionalInfo.InvoiceeNameText);
			AssertNullOrEmpty(additionalInfo.InvoiceeSpecifiedPersonNameText);
			AssertNullOrEmpty(additionalInfo.InvoiceeSpecifiedAddressLineOneText);
			AssertNullOrEmpty(additionalInfo.InvoiceeTypeCode);
			AssertNullOrEmpty(additionalInfo.InvoiceeClassificationCode);
			AssertNullOrEmpty(additionalInfo.InvoiceePrimaryDefinedContactURICommunication);
			AssertNullOrEmpty(additionalInfo.InvoiceeSecondaryDefinedContactURICommunication);

			AssertNullOrEmpty(additionalInfo.InvoicerID);
			AssertNullOrEmpty(additionalInfo.InvoicerNameText);
			AssertNullOrEmpty(additionalInfo.InvoicerSpecifiedPersonNameText);
			AssertNullOrEmpty(additionalInfo.InvoicerSpecifiedAddressLineOneText);
			AssertNullOrEmpty(additionalInfo.InvoicerTypeCode);
			AssertNullOrEmpty(additionalInfo.InvoicerClassificationCode);
			AssertNullOrEmpty(additionalInfo.InvoicerDefinedContactURICommunication);

			AssertEquals(false, additionalInfo.Lines.Any());
		}

		#endregion

		string GetTestFilePath(string testFileName) => BaseSourcePath + $@"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\EInvoicing\KoreaSouth\{testFileName}";
	}
}
