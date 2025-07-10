using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie818;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tms;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE818ProviderTest : Business.Testing.DataProviderTestCase<IE818Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE818Provider(null));
		}

		public void TestMrnNumber()
		{
			AssertEquals("20GB41000000001870745", Provider.MrnNumber);
		}

		public void TestMrnNumberSequenceNumber()
		{
			AssertEquals("1", Provider.MrnNumberSequenceNumber);
		}

		public void TestExciseMovementEad()
		{
			CombineAssertions(() =>
			{
				var exciseMovementEad = Provider.ExciseMovementEad;
				AssertEquals("Ead", "20GB41000000001870745", exciseMovementEad.AdministrativeReferenceCode);
				AssertEquals("Sequence", "1", exciseMovementEad.SequenceNumber);
			});
		}

		public void TestGlobalConclusionOfReceipt()
		{
			AssertEquals("GlobalConclusionOfReceipt", "21", Provider.GlobalConclusionOfReceipt);
		}

		public void TestReportOfReceipts()
		{
			var provider = Provider;
			message.Body.AcceptedOrRejectedReportOfReceiptExport.BodyReportOfReceiptExport = new System.Collections.ObjectModel.Collection<BodyReportOfReceiptExportType>
			{
				new BodyReportOfReceiptExportType
				{
					BodyRecordUniqueReference = "1"
				}
			};
			AssertEquals("1 Record", 1, provider.ReportOfReceipts.Count);
		}

		public void TestReportOfReceipts_NoRecords()
		{
			var reportOfReceipts = Provider.ReportOfReceipts;
			AssertEquals("No Records", false, reportOfReceipts.Any());
		}

		protected override IEnumerable<Expression<Func<IE818Provider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ExciseMovementEad;
			yield return x => x.ReportOfReceipts;
		}

		protected override IE818Provider GetProvider()
		{
			message = new Ie818Type
			{
				Header = new HeaderType
				{
					MessageSender = "NDEA.GB",
					MessageRecipient = "NDEA.GB",
					DateOfPreparation = new DateTime(2022, 09, 01),
					TimeOfPreparation = new DateTime(2022, 09, 01, 15, 30, 05),
					MessageIdentifier = "e3e6f99d-2bc3-4c11-a20f-c7aa391f8573",
				},
				Body = new BodyType
				{
					AcceptedOrRejectedReportOfReceiptExport = new AcceptedOrRejectedReportOfReceiptExportType
					{
						ExciseMovement = new ExciseMovementType
						{
							AdministrativeReferenceCode = "20GB41000000001870745",
							SequenceNumber = "1"
						},
						ReportOfReceiptExport = new ReportOfReceiptExportType
						{
							GlobalConclusionOfReceipt = GlobalConclusionOfReceipt.Item21
						}
					}
				}
			};
			return new IE818Provider(message);
		}
		Ie818Type message;
	}
}
