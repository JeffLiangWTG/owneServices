using System;
using System.IO;
using System.Xml.Schema;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class DECustomsDataTest : TestCase
	{
		public void TestXSDValidation()
		{
			AssertExceptionThrown<XmlSchemaValidationException>("Missing Alll Nodes", () => new DECustomsDataProvider<SCSTAB>(cusstaEmbeddedResourcePath, new StringReader("<DECustomsData></DECustomsData>")));
		}

		public void TestLogbookTime()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<XmlSchemaValidationException>("Invalid Date in LogbookTime Node", () => new DECustomsDataProvider<SCSTAB>(cusstaEmbeddedResourcePath, new StringReader("<DECustomsData><LogbookTime>2019-11-31 25:29:00+2:00</LogbookTime></DECustomsData>")));
				NUnit.Framework.Assert.That(deCustomsData.LogbookTime.ToUtcDateTime(), Is.EqualTo(new DateTime(2019, 11, 20, 13, 29, 00).AddTicks(2347040)), "Valid Date in UTC");
				NUnit.Framework.Assert.That(deCustomsData.LogbookTime.ToISO8601String(), Is.EqualTo("2019-11-20T15:29:00.2347040+02:00"), "Valid Date in LogbookTime Output");
			});
		}

		[ExpectNoExceptions]
		public void TestMessage()
		{
			NUnit.Framework.Assert.That(deCustomsData.Message.MetaData.MessageIdentifier, Is.EqualTo("CUSSTA58750000000510579070519161955"), "Message is extracted correctly");
		}

		[ExpectNoExceptions]
		public void TestAttachedDocuments()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(deCustomsData.AttachedDocuments.Count, Is.EqualTo(0), "Missing AttachedDocument Node");
				using (var streamReader = File.OpenText(Path.Combine(BaseSourcePath, TestExtensions.MessagingTestDirectory, @"Providers\TestFiles\CustomsDataWithAttachedDocumentMessage.txt")))
				{
					deCustomsData = new DECustomsDataProvider<SCSTAB>(cusstaEmbeddedResourcePath, streamReader);
					NUnit.Framework.Assert.That(deCustomsData.AttachedDocuments.Count, Is.EqualTo(2), "Multiple Documents -> Count");
					AssertAttachedDocument("Multiple Documents -> File 1", deCustomsData.AttachedDocuments[0], "CUSSTA58750000000510579070519161955.pdf", "CAU", "Report CUSSTA 1", "MkE5UlZFQUMtQVpQMlU0RjItSkhKRVNDUkMtWllSSFpOUEYtTFREODQ2TFYtMzlRRDZGTk0tM1Y3VzZNRDgtUTdVMjk5VTI=");
					AssertAttachedDocument("Multiple Documents -> File 2", deCustomsData.AttachedDocuments[1], "CUSSTA58750000000510579070519161956.pdf", "CAU", "Report CUSSTA 2", "MkE5UlZFQUMtQVpQMlU0RjItSkhKRVNDUkMtWllSSFpOUEYtTFREODQ2TFYtMzlRRDZGTk0tM1Y3VzZNRDgtUTdVMjk5VTI=");
				}
			});
		}

		[ExpectNoExceptions]
		void AssertAttachedDocument(string testCase, AttachedDocument document, string fileName, string typeCode, string typeDescription, string imageData)
		{
			NUnit.Framework.Assert.That(document.FileName, Is.EqualTo(fileName).Using(CustomComparers.TypeComparison), testCase + " -> FileName");
			NUnit.Framework.Assert.That(document.Type.Code, Is.EqualTo(typeCode).Using(CustomComparers.TypeComparison), testCase + " -> Type.Code");
			NUnit.Framework.Assert.That(document.Type.Description, Is.EqualTo(typeDescription).Using(CustomComparers.TypeComparison), testCase + " -> Type.Description");
			NUnit.Framework.Assert.That(document.ImageData.ConvertToByteArrayAndCloseStream(), Is.EqualTo(Convert.FromBase64String(imageData)), testCase + " -> ImageData");
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusstaEmbeddedResourcePath = typeof(SCSTAB).GetEmbeddedResourcePath();
			using (var streamReader = File.OpenText(Path.Combine(BaseSourcePath, TestExtensions.MessagingTestDirectory, @"Providers\TestFiles\CustomsData.txt")))
			{
				deCustomsData = new DECustomsDataProvider<SCSTAB>(cusstaEmbeddedResourcePath, streamReader);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			deCustomsData?.Dispose();
		}
		DECustomsDataProvider<SCSTAB> deCustomsData;
		string cusstaEmbeddedResourcePath;
	}
}
