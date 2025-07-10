using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmDocumentDelivery))]
	sealed class StmDocumentDeliveryTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => stmDocumentDelivery;

		protected override BusinessObject GetNewBusinessObject() => stmDocumentDelivery;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => stmDocumentDelivery;

		protected override void SetUp()
		{
			base.SetUp();
			stmDocumentDelivery = Factory.NewWithValidTestData<StmDocumentDelivery>();
			stmDocumentDelivery.SDL_Instructions = @"{}";
		}

		public void TestSerializableDeliveryInstructions()
		{
			SetUp();

			var serializableDeliveryInstructions = SerializableDeliveryInstructions.DeserializeDeliveryInstructions(stmDocumentDelivery);
			AssertNotNull(serializableDeliveryInstructions);
			CombineAssertions("SerializableDeliveryInstructions: null", () =>
			{
				AssertNullOrEmpty("Language", serializableDeliveryInstructions.Language);
				AssertEquals("IsDraft", false, serializableDeliveryInstructions.IsDraft);
				AssertEquals("PrintQueuePK", ZGuid.Empty, serializableDeliveryInstructions.PrintQueuePK);
				AssertEquals("NumberOfCopies", 0, serializableDeliveryInstructions.NumberOfCopies);
				AssertEquals("PrintMultiDocPack", false, serializableDeliveryInstructions.PrintMultiDocPack);
				AssertEquals("AutoDeliverMultiDocPack", false, serializableDeliveryInstructions.AutoDeliverMultiDocPack);
				AssertNullOrEmpty("SpecifiedPageRanges", serializableDeliveryInstructions.SpecifiedPageRanges);
				AssertNullOrEmpty("CoverNote", serializableDeliveryInstructions.CoverNote);
				AssertEquals("Recipients", true, serializableDeliveryInstructions.Recipients.IsNullOrEmpty());
				AssertEquals("DocumentsToBeDelivered", true, serializableDeliveryInstructions.DocumentsToBeDelivered.IsNullOrEmpty());
				AssertEquals("EDocsToBeDelivered", true, serializableDeliveryInstructions.EDocsToBeDelivered.IsNullOrEmpty());
			});

			stmDocumentDelivery.SDL_Instructions = @"
{
    ""Language"": ""zh-CN"",
    ""SpecifiedPageRanges"": ""SpecifiedPageRanges"",
    ""NumberOfCopies"": ""2"",
    ""PrintQueuePK"": ""0880e018-318e-4885-bb86-f8bd69a51590"",
    ""IsDraft"": true,
    ""CoverNote"": ""CoverNote"",
    ""AutoDeliverMultiDocPack"": true,
    ""PrintMultiDocPack"": true,
    ""Recipients"": [
        {
            ""OrgHeaderPK"": ""3de09de4-c351-48a4-afea-07bcb61339d7"",
            ""DeliveryMethod"": ""EML"",
            ""AttachmentType"": ""PDF"",
            ""DeliveryAddress"": ""DeliveryAddress@test.com"",
            ""Salutation"": ""Salutation"",
            ""ContactName"": ""ContactName"",
            ""SendFrom"": ""SendFrom"",
            ""EmailCarbonCopyRecipientsAsString"": ""EmailCarbonCopyRecipientsAsString"",
            ""EmailBlindCarbonCopyRecipientsAsString"": ""EmailBlindCarbonCopyRecipientsAsString"",
            ""EmailSubjectMacro"": ""EmailSubjectMacro""
        }
    ],
    ""DocumentsToBeDelivered"": [
        {
            ""Index"": ""1"",
            ""PrintQueuePK"": ""c47f56c7-6d63-4ba5-aeec-75cdadbf0403"",
            ""Copies"": ""1"",
            ""Identifier"": ""Identifier""
        }
    ],
    ""EDocsToBeDelivered"": [
        {
            ""Index"": ""1"",
            ""Identifier"": ""Identifier""
        }
    ]
}";

			var serializableDeliveryInstructions2 = SerializableDeliveryInstructions.DeserializeDeliveryInstructions(stmDocumentDelivery);
			AssertNotNull(serializableDeliveryInstructions2);
			CombineAssertions("SerializableDeliveryInstructions: not null", () =>
			{
				AssertEquals("Language", "zh-CN", serializableDeliveryInstructions2.Language);
				AssertEquals("IsDraft", true, serializableDeliveryInstructions2.IsDraft);
				AssertEquals("PrintQueuePK", "0880e018-318e-4885-bb86-f8bd69a51590", serializableDeliveryInstructions2.PrintQueuePK.ToString());
				AssertEquals("NumberOfCopies", 2, serializableDeliveryInstructions2.NumberOfCopies);
				AssertEquals("PrintMultiDocPack", true, serializableDeliveryInstructions2.PrintMultiDocPack);
				AssertEquals("AutoDeliverMultiDocPack", true, serializableDeliveryInstructions2.AutoDeliverMultiDocPack);
				AssertEquals("SpecifiedPageRanges", "SpecifiedPageRanges", serializableDeliveryInstructions2.SpecifiedPageRanges);
				AssertEquals("CoverNote", "CoverNote", serializableDeliveryInstructions2.CoverNote);
			});

			AssertEquals("Recipients", 1, serializableDeliveryInstructions2.Recipients.Count());
			CombineAssertions("Recipients: not null", () =>
			{
				var recipient = serializableDeliveryInstructions2.Recipients.First();
				AssertEquals("OrgHeaderPK", "3de09de4-c351-48a4-afea-07bcb61339d7", recipient.OrgHeaderPK.ToString());
				AssertEquals("DeliveryMethod", "EML", recipient.DeliveryMethod);
				AssertEquals("AttachmentType", "PDF", recipient.AttachmentType);
				AssertEquals("DeliveryAddress", "DeliveryAddress@test.com", recipient.DeliveryAddress);
				AssertEquals("Salutation", "Salutation", recipient.Salutation);
				AssertEquals("ContactName", "ContactName", recipient.ContactName);
				AssertEquals("SendFrom", "SendFrom", recipient.SendFrom);
				AssertEquals("EmailCarbonCopyRecipientsAsString", "EmailCarbonCopyRecipientsAsString", recipient.EmailCarbonCopyRecipientsAsString);
				AssertEquals("EmailBlindCarbonCopyRecipientsAsString", "EmailBlindCarbonCopyRecipientsAsString", recipient.EmailBlindCarbonCopyRecipientsAsString);
				AssertEquals("EmailSubjectMacro", "EmailSubjectMacro", recipient.EmailSubjectMacro);
			});

			AssertEquals("DocumentsToBeDelivered", 1, serializableDeliveryInstructions2.DocumentsToBeDelivered.Count());
			CombineAssertions("DocumentsToBeDelivered: not null", () =>
			{
				var documentToBeDelivered = serializableDeliveryInstructions2.DocumentsToBeDelivered.First();
				AssertEquals("Index", 1, documentToBeDelivered.Index.ToZInt());
				AssertEquals("PrintQueuePK", "c47f56c7-6d63-4ba5-aeec-75cdadbf0403", documentToBeDelivered.PrintQueuePK.ToString());
				AssertEquals("Copies", 1, documentToBeDelivered.Copies);
				AssertEquals("Identifier", "Identifier", documentToBeDelivered.Identifier);
			});

			AssertEquals("EDocsToBeDelivered", 1, serializableDeliveryInstructions2.EDocsToBeDelivered.Count());
			CombineAssertions("EDocsToBeDelivered: not null", () =>
			{
				var eDocToBeDelivered = serializableDeliveryInstructions2.EDocsToBeDelivered.First();
				AssertEquals("Index", 1, eDocToBeDelivered.Index.ToZInt());
				AssertEquals("Identifier", "Identifier", eDocToBeDelivered.Identifier);
			});
		}

		StmDocumentDelivery stmDocumentDelivery;
	}
}
