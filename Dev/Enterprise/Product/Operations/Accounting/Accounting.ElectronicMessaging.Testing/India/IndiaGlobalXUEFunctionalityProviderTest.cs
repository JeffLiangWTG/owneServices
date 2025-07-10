using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.India.Testing
{
	[TestedType(typeof(IndiaGlobalXUEFunctionalityProvider))]
	class IndiaGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new IndiaEInvoicingObjectFactory();

		public override void TestIsEventMessageProcessSupported()
		{
			AssertIsEventMessageProcessSupported("IAK", "", true);
			AssertIsEventMessageProcessSupported("IAK", "GEN", true);
			AssertIsEventMessageProcessSupported("IRJ", "", true);
			AssertIsEventMessageProcessSupported("IRJ", "GEN", true);
			AssertIsEventMessageProcessSupported("***", "___", false);
		}

		public override void TestGetEventMessageProcessor()
		{
			var objects = GetEventMessageProcessorObjects(ReadXmlFromEmbeddedResource("OldStyleXUE.xml"));

			AssertGetEventMessageProcessor("IAK", "", objects, typeof(EInvoicingEventMessageINProcessor));
			AssertGetEventMessageProcessor("IAK", "GEN", objects, typeof(EInvoicingEventMessageINProcessor));
			AssertGetEventMessageProcessor("IRJ", "", objects, typeof(EInvoicingEventMessageINProcessor));
			AssertGetEventMessageProcessor("IRJ", "GEN", objects, typeof(EInvoicingEventMessageINProcessor));
		}

		public void TestGetEventMessageProcessor_NewStyleXUEWithBackwardCompatibility()
		{
			var objects = GetEventMessageProcessorObjects(ReadXmlFromEmbeddedResource("NewStyleXUEWithBackwardCompatibility.xml"));

			AssertGetEventMessageProcessor("IAK", "", objects, typeof(GlobalEInvoicingEventMessageProcessor));
			AssertGetEventMessageProcessor("IAK", "GEN", objects, typeof(GlobalEInvoicingEventMessageProcessor));
			AssertGetEventMessageProcessor("IRJ", "", objects, typeof(GlobalEInvoicingEventMessageProcessor));
			AssertGetEventMessageProcessor("IRJ", "GEN", objects, typeof(GlobalEInvoicingEventMessageProcessor));
		}

		public void TestGetEventMessageProcessor_NewStyleXUE()
		{
			var objects = GetEventMessageProcessorObjects(ReadXmlFromEmbeddedResource("NewStyleXUE.xml"));

			AssertGetEventMessageProcessor("IAK", "", objects, typeof(GlobalEInvoicingEventMessageProcessor));
			AssertGetEventMessageProcessor("IAK", "GEN", objects, typeof(GlobalEInvoicingEventMessageProcessor));
			AssertGetEventMessageProcessor("IRJ", "", objects, typeof(GlobalEInvoicingEventMessageProcessor));
			AssertGetEventMessageProcessor("IRJ", "GEN", objects, typeof(GlobalEInvoicingEventMessageProcessor));
		}

		#region Backward Compatibility Test Cases

		public void TestOldStyleXUEMessage_IsProcessedSuccessfully_WithDynamicProcessor()
			=> TestXUEMessageIsProcessedSuccessfully("OldStyleXUE.xml");

		public void TestNewStyleXUEMessage_WithBackwardCompatibility_IsProcessedSuccessfully_WithDynamicProcessor()
			=> TestXUEMessageIsProcessedSuccessfully("NewStyleXUEWithBackwardCompatibility.xml");

		public void TestNewStyleXUEMessage_IsProcessedSuccessfully_WithDynamicProcessor()
			=> TestXUEMessageIsProcessedSuccessfully("NewStyleXUE.xml");

		public void TestNewStyleXUEMessage_WithBackwardCompatibility_IsProcessedSuccessfully_WithNewProcessor()
			=> TestXUEMessageIsProcessedSuccessfully("NewStyleXUEWithBackwardCompatibility.xml", d => new GlobalEInvoicingEventMessageProcessor(d, CountryEInvoicingObjectFactory));

		public void TestNewStyleXUEMessage_WithBackwardCompatibility_IsProcessedSuccessfully_WithOldProcessor()
			=> TestXUEMessageIsProcessedSuccessfully("NewStyleXUEWithBackwardCompatibility.xml", d => new EInvoicingEventMessageINProcessor(d.Logger, d.Message, d.UniversalEvent, d.InvoiceBatch));

		void TestXUEMessageIsProcessedSuccessfully(string embeddedResourceName, Func<EventMessageProcessorData, EInvoicingEventMessageProcessor> factory = null)
		{
			// Arrange
			var (logger, ediMessage, universalEvent, batch, invoice) = CreateAndSaveTestObjects(embeddedResourceName);

			var query = new ZQuery();
			query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, SQLComparisonOperator.Equal, invoice.PK);
			var authRecords = Factory.Load<AccTransactionHeaderAuthorisationRecord>(query);
			AssertEquals("Precondition: Should be no auth records related to the invoice", 0, authRecords.Length);

			var processorData = new EventMessageProcessorData("IAK", "GEN", logger, ediMessage, universalEvent, batch);
			factory = factory ?? ((d) => GetGlobalXUEFunctionalityProvider.GetEventMessageProcessor(d));
			var processor = factory(processorData);

			// Act
			processor.Process();

			// Assert
			authRecords = Factory.Load<AccTransactionHeaderAuthorisationRecord>(query);
			AssertEquals("Should be one auth record created", 1, authRecords.Length);
			var authRecord = authRecords[0];

			CombineAssertions("Expected data for auth record", () =>
			{
				AssertEquals("AHF_IDType", "GVT", authRecord.AHF_IDType);
				AssertEquals("AHF_Counter", "112010000002315", authRecord.AHF_Counter);
				AssertEquals("AHF_DateTime", "2020-08-05T15:18:00.0000000+05:30", authRecord.AHF_DateTime.ToISO8601String());
				AssertEquals("AHF_Number", "11f8ef701fe294d4a14aad0b12457e62775d0fdc41a0acf05b74fbb2ddc47acb", authRecord.AHF_Number);
				AssertEquals("AHF_AuthorisationData", ZBlob.FromAscii("eyJhbGciOiJSUzI1NiIsImtpZCI6IjExNUY0NDI2NjE3QTc5MzhCRTFCQTA2REJFRTkxQTQyNzU4NEVEQUIiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJFVjlFSm1GNmVUaS1HNkJ0dnVrYVFuV0U3YXMifQ.eyJkYXRhIjoie1wiQWNrTm9cIjoxMTIwMTAwMDAwMDIzMTUsXCJBY2tEdFwiOlwiMjAyMC0wOC0wNSAxNToxODowMFwiLFwiSXJuXCI6XCIxMWY4ZWY3MDFmZTI5NGQ0YTE0YWFkMGIxMjQ1N2U2Mjc3NWQwZmRjNDFhMGFjZjA1Yjc0ZmJiMmRkYzQ3YWNiXCIsXCJWZXJzaW9uXCI6XCIxLjAxXCIsXCJUcmFuRHRsc1wiOntcIlRheFNjaFwiOlwiR1NUXCIsXCJTdXBUeXBcIjpcIkIyQlwiLFwiUmVnUmV2XCI6XCJOXCIsXCJJZ3N0T25JbnRyYVwiOlwiWVwifSxcIkRvY0R0bHNcIjp7XCJUeXBcIjpcIklOVlwiLFwiTm9cIjpcIkRELTIwMjAwODA0LTlcIixcIkR0XCI6XCIwNC8wOC8yMDIwXCJ9LFwiU2VsbGVyRHRsc1wiOntcIkdzdGluXCI6XCIzN0FSWlBUNDM4NFExTVRcIixcIkxnbE5tXCI6XCIgQUJDIGNvbXBhbnkgcHZ0IGx0ZFwiLFwiVHJkTm1cIjpcInZpa2FzXCIsXCJBZGRyMVwiOlwiVEUgaFwiLFwiQWRkcjJcIjpcImFiY1wiLFwiTG9jXCI6XCJCYW5nYWxvcmVcIixcIlBpblwiOjUxNTMxMSxcIlN0Y2RcIjpcIjM3XCIsXCJQaFwiOlwiOTczODk3MTk3MFwiLFwiRW1cIjpcInZpa2FzQGdtYWlsLmNvbVwifSxcIkJ1eWVyRHRsc1wiOntcIkdzdGluXCI6XCIxMUFBQUNUMzkwNEYxWlpcIixcIkxnbE5tXCI6XCJYWVogY29tcGFueSBwdnQgbHRkXCIsXCJQb3NcIjpcIjM3XCIsXCJBZGRyMVwiOlwiN3RoIGJsb2NrLCBrdXZlbXB1IGxheW91dFwiLFwiTG9jXCI6XCJHQU5ESElOQUdBUlwiLFwiUGluXCI6NzM3MTAxLFwiU3RjZFwiOlwiMTFcIn0sXCJEaXNwRHRsc1wiOntcIk5tXCI6XCJuYW1lIG9mIHRoZSBjb21wYW55IGZyb20gd2hpY2ggZ29vZHMgZGlzcGF0Y2hlZFwiLFwiQWRkcjFcIjpcImFkZHJlc3NcIixcIkFkZHIyXCI6XCJCYW5nYWxvcmVcIixcIkxvY1wiOlwia3prXCIsXCJQaW5cIjo2OTA1MTMsXCJTdGNkXCI6XCIzMlwifSxcIlNoaXBEdGxzXCI6e1wiR3N0aW5cIjpcIjMyRElVUFAxMTc1RzFaMVwiLFwiTGdsTm1cIjpcInNoaXAgdHJhZGVcIixcIlRyZE5tXCI6XCJ2aWthc1wiLFwiQWRkcjFcIjpcInNoaXAgYiBub1wiLFwiQWRkcjJcIjpcIkJhbmdhbG9yZVwiLFwiTG9jXCI6XCJCYW5nYWxvcmVcIixcIlBpblwiOjY5MDUxMyxcIlN0Y2RcIjpcIjMyXCJ9LFwiSXRlbUxpc3RcIjpbe1wiSXRlbU5vXCI6MSxcIlNsTm9cIjpcIjFcIixcIklzU2VydmNcIjpcIk5cIixcIlByZERlc2NcIjpcIlN0ZWVsXCIsXCJIc25DZFwiOlwiMTAwMVwiLFwiUXR5XCI6MTAsXCJVbml0XCI6XCJCQUdcIixcIlVuaXRQcmljZVwiOjIwMC4wMCxcIlRvdEFtdFwiOjIwMDAuMDAsXCJEaXNjb3VudFwiOjEwLFwiQXNzQW10XCI6MTk5MC4wMCxcIkdzdFJ0XCI6MTIuMDAsXCJJZ3N0QW10XCI6MjM4LjgsXCJDZ3N0QW10XCI6MCxcIlNnc3RBbXRcIjowLFwiQ2VzUnRcIjo1LFwiQ2VzQW10XCI6OTkuNSxcIkNlc05vbkFkdmxBbXRcIjoxMCxcIlN0YXRlQ2VzUnRcIjoxMixcIlN0YXRlQ2VzQW10XCI6MjM4LjgwLFwiU3RhdGVDZXNOb25BZHZsQW10XCI6NSxcIk90aENocmdcIjoxMCxcIlRvdEl0ZW1WYWxcIjoyNTkyLjF9LHtcIkl0ZW1Ob1wiOjIsXCJTbE5vXCI6XCIyXCIsXCJJc1NlcnZjXCI6XCJOXCIsXCJQcmREZXNjXCI6XCJTdGVlbFwiLFwiSHNuQ2RcIjpcIjEwMDFcIixcIlF0eVwiOjEwLFwiVW5pdFwiOlwiQkFHXCIsXCJVbml0UHJpY2VcIjoyMDAuMDAsXCJUb3RBbXRcIjoyMDAwLjAwLFwiRGlzY291bnRcIjoxMCxcIkFzc0FtdFwiOjE5OTAuMDAsXCJHc3RSdFwiOjEyLjAwLFwiSWdzdEFtdFwiOjIzOC44LFwiQ2dzdEFtdFwiOjAsXCJTZ3N0QW10XCI6MCxcIkNlc1J0XCI6NSxcIkNlc0FtdFwiOjk5LjUsXCJDZXNOb25BZHZsQW10XCI6MTAsXCJTdGF0ZUNlc1J0XCI6MTIsXCJTdGF0ZUNlc0FtdFwiOjIzOC44MCxcIlN0YXRlQ2VzTm9uQWR2bEFtdFwiOjUsXCJPdGhDaHJnXCI6MTAsXCJUb3RJdGVtVmFsXCI6MjU5Mi4xfSx7XCJJdGVtTm9cIjozLFwiU2xOb1wiOlwiM1wiLFwiSXNTZXJ2Y1wiOlwiTlwiLFwiUHJkRGVzY1wiOlwiU3RlZWxcIixcIkhzbkNkXCI6XCIxMDAxXCIsXCJRdHlcIjoxMCxcIlVuaXRcIjpcIkJBR1wiLFwiVW5pdFByaWNlXCI6MjAwLjAwLFwiVG90QW10XCI6MjAwMC4wMCxcIkRpc2NvdW50XCI6MTAsXCJBc3NBbXRcIjoxOTkwLjAwLFwiR3N0UnRcIjoxMi4wMCxcIklnc3RBbXRcIjoyMzguOCxcIkNnc3RBbXRcIjowLFwiU2dzdEFtdFwiOjAsXCJDZXNSdFwiOjUsXCJDZXNBbXRcIjo5OS41LFwiQ2VzTm9uQWR2bEFtdFwiOjEwLFwiU3RhdGVDZXNSdFwiOjEyLFwiU3RhdGVDZXNBbXRcIjoyMzguODAsXCJTdGF0ZUNlc05vbkFkdmxBbXRcIjo1LFwiT3RoQ2hyZ1wiOjEwLFwiVG90SXRlbVZhbFwiOjI1OTIuMX1dLFwiVmFsRHRsc1wiOntcIkFzc1ZhbFwiOjU5NzAuMCxcIkNnc3RWYWxcIjowLFwiU2dzdFZhbFwiOjAsXCJJZ3N0VmFsXCI6NzE2LjQsXCJDZXNWYWxcIjozMjguNSxcIlN0Q2VzVmFsXCI6NzMxLjQsXCJSbmRPZmZBbXRcIjowLFwiVG90SW52VmFsXCI6Nzc3Ni4zfSxcIlJlZkR0bHNcIjp7XCJJbnZSbVwiOlwiMTIzXCIsXCJQcmVjRG9jRHRsc1wiOlt7XCJJbnZOb1wiOlwiQUJDXCIsXCJJbnZEdFwiOlwiMDIvMDIvMjAyMFwiLFwiT3RoUmVmTm9cIjpcIjEyQVwifV0sXCJDb250ckR0bHNcIjpbe1wiUmVjQWR2UmVmclwiOlwiMTIzXCIsXCJSZWNBZHZEdFwiOlwiMTIvMDIvMjAyMFwiLFwiVGVuZFJlZnJcIjpcImFiY1wiLFwiQ29udHJSZWZyXCI6XCJhYmNcIixcIkV4dFJlZnJcIjpcImFiY1wiLFwiUHJvalJlZnJcIjpcImFiY1wiLFwiUE9SZWZyXCI6XCJhYmNcIixcIlBPUmVmRHRcIjpcIjEyLzAyLzIwMjBcIn1dfSxcIkFkZGxEb2NEdGxzXCI6W3tcIlVybFwiOlwiaHR0cHM6Ly9laW52LWFwaXNhbmRib3gubmljLmluL2dzdGNvcmVfdGVzdC92MS4wMS9pbnZvaWNlXCIsXCJEb2NzXCI6XCJ2aWthc1wiLFwiSW5mb1wiOlwidmlrYXNcIn1dLFwiRXdiRHRsc1wiOntcIlRyYW5zSWRcIjpcIjEyQVdHUFY3MTA3QjFaMVwiLFwiVHJhbnNOYW1lXCI6XCJYWVogRVhQT1JUU1wiLFwiVHJhbnNNb2RlXCI6XCIxXCIsXCJEaXN0YW5jZVwiOjEwMCxcIlRyYW5zRG9jTm9cIjpcIkRPQzAxXCIsXCJUcmFuc0RvY0R0XCI6XCIwNC8wOC8yMDIwXCIsXCJWZWhOb1wiOlwia2ExMjM0NTZcIixcIlZlaFR5cGVcIjpcIlJcIn19IiwiaXNzIjoiTklDIn0.oesnTXdXgOEeRjYr6bRQ-_Ks-bnIpwtj7Zx8phzfjL6vsfuGqBokILz6ai0NHFKRxiX_bTLrgrWmwXyBdEFmt88myf4n-NP5JvwqFx4OIf0gYMFTKGLx4AQsxwXER836FDxyS33K_7Erkm7_yHsITR5sBkYrZYOWimYl5cgh4EFN2mEq0B8oIp9pSXAU2RGvuirV6Rnl902sWj1Zv_2UK8e9C7cS7maeuFvEgAHrwBjxqLVvRGDz93oRVgQcavhdNTmBr8LQo2yRQkwtZKCMY_NGDsIoJx3orAKEUE7D1RbAM6xh-uxGOlqxur50826y0sk6OuG2WB9K5g5gumxIpg"), authRecord.AHF_AuthorisationData);
				AssertEquals("AHF_VerificationUrl", "eyJhbGciOiJSUzI1NiIsImtpZCI6IjExNUY0NDI2NjE3QTc5MzhCRTFCQTA2REJFRTkxQTQyNzU4NEVEQUIiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJFVjlFSm1GNmVUaS1HNkJ0dnVrYVFuV0U3YXMifQ.eyJkYXRhIjoie1wiU2VsbGVyR3N0aW5cIjpcIjM3QVJaUFQ0Mzg0UTFNVFwiLFwiQnV5ZXJHc3RpblwiOlwiMTFBQUFDVDM5MDRGMVpaXCIsXCJEb2NOb1wiOlwiREQtMjAyMDA4MDQtOVwiLFwiRG9jVHlwXCI6XCJJTlZcIixcIkRvY0R0XCI6XCIwNC8wOC8yMDIwXCIsXCJUb3RJbnZWYWxcIjo3Nzc2LjMsXCJJdGVtQ250XCI6MyxcIk1haW5Ic25Db2RlXCI6XCIxMDAxXCIsXCJJcm5cIjpcIjExZjhlZjcwMWZlMjk0ZDRhMTRhYWQwYjEyNDU3ZTYyNzc1ZDBmZGM0MWEwYWNmMDViNzRmYmIyZGRjNDdhY2JcIn0iLCJpc3MiOiJOSUMifQ.fya8oD85f2_K8pDWSf8N94_T24O1lA9OPpIuUwk14el_r1lhL13OFxGkklhiewSMUom8DvO9JKu4jjz2l5farRTJhiBWJ43EtEky2SLzRhJf23JYW_6PyLErYL2RTzv2PlZ75eXIBZzPkxc2erCx61T50oHmExLgl1Q6HclvgiQUAVxysq1VFv96zEZVH8I0xDNqjdvqdtsW74ZHqzpV28kDIvuyV4Z5j3bR39GE6YKMetext_x3bJ4Wt4F1z3DOzfUjuKGdEjP0fTSwNg1RpiDoH4wcaMP7RJgtbQYXn4j3YoppCEw916AmbihiT2gSODPn04vhCbBecI7oOZvxpw", authRecord.AHF_VerificationUrl);
				AssertEquals("AHF_PublicKey", new ZBlob(Convert.FromBase64String("MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArxd93uLDs8HTPqcSPpxZrf0Dc29r3iPp0a8filjAyeX4RAH6lWm9qFt26CcE8ESYtmo1sVtswvs7VH4Bjg/FDlRpd+MnAlXuxChij8/vjyAwE71ucMrmZhxM8rOSfPML8fniZ8trr3I4R2o4xWh6no/xTUtZ02/yUEXbphw3DEuefzHEQnEF+quGji9pvGnPO6Krmnri9H4WPY0ysPQQQd82bUZCk9XdhSZcW/am8wBulYokITRMVHlbRXqu1pOFmQMO5oSpyZU3pXbsx+OxIOc4EDX0WMa9aH4+snt18WAXVGwF2B4fmBk7AtmkFzrTmbpmyVqA3KO2IjzMZPw0hQIDAQAB")), authRecord.AHF_PublicKey);
			});

			AssertEquals("There should be no processing errors", "", logger.ToString());
		}

		public void TestNewStyleXUEMessage_FailsProcessing_WithOldProcessor()
		{
			// Arrange
			var (logger, ediMessage, universalEvent, batch, invoice) = CreateAndSaveTestObjects("NewStyleXUE.xml");

			var query = new ZQuery();
			query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, SQLComparisonOperator.Equal, invoice.PK);
			var authRecords = Factory.Load<AccTransactionHeaderAuthorisationRecord>(query);
			AssertEquals("Precondition: Should be no auth records related to the invoice", 0, authRecords.Length);
			var processor = new EInvoicingEventMessageINProcessor(logger, ediMessage, universalEvent, batch);

			// Act
			processor.Process();

			// Assert
			authRecords = Factory.Load<AccTransactionHeaderAuthorisationRecord>(query);
			AssertEquals("Should be one no auth record created", 0, authRecords.Length);

			var expectedError = "India Invoice Response was not processed due to context collection not having Response Message Context for invoice batch 1 in Eagle Datamation International.";
			AssertEquals("Error - " + expectedError, logger.ToString());
			AssertEquals(expectedError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		(
			IXmlSessionTracker logger,
			IEDIMessage ediMessage,
			Event universalEvent,
			AccEInvoicingBatch batch,
			InvoicingBase invoice
		) CreateAndSaveTestObjects(string embeddedResourceName)
		{
			var objectCreator = new TestObjectCreator(Factory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1000", objectCreator.USD, 1.0m, 10m, 1m, 10m, 1m);
			var (logger, ediMessage, universalEvent, batch) = GetEventMessageProcessorObjects(ReadXmlFromEmbeddedResource(embeddedResourceName));
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(invoice);
			pivot.AIP_AIB = batch.PK;
			Factory.Save();
			return (logger, ediMessage, universalEvent, batch, invoice);
		}

		string ReadXmlFromEmbeddedResource(string embeddedResourceName)
			=> EInvoicingTestHelper.GetEmbeddedResourceAsUtf8String(embeddedResourceName, "Enterprise.Accounting.ElectronicMessaging.Testing.India.TestMessages.");

		#endregion
	}
}
