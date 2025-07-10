using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	[TestedType(typeof(ED818Provider))]
	class ED818ProviderTest : InboundDataProviderTestCase<IED818, ED818Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED818Provider(null));
		}

		public void TestMessageGroup()
		{
			AssertEquals(EmcsMessageSubTypeList.Codes.Eme, dataProvider.MessageGroup);
		}

		public void TestMessageIdentifier()
		{
			AssertEquals("0072260102", dataProvider.MessageIdentifier);
		}

		public void TestExciseMovement()
		{
			AssertSame("Cached", dataProvider.ExciseMovement, dataProvider.ExciseMovement);
		}

		public void TestAdministrativeReferenceCode()
		{
			AssertEquals("Excise Movement", "20DE41000000001870745", dataProvider.ExciseMovement.AdministrativeReferenceCode);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Sequence", "1", dataProvider.ExciseMovement.SequenceNumber);
		}

		public void TestGlobalConclusionOfReceipt()
		{
			AssertEquals("GlobalConclusionOfReceipt", "21", dataProvider.GlobalConclusionOfReceipt);
		}

		public void TestReportOfReceipts()
		{
			message.Body.AcceptedOrRejectedReportOfReceipt.BodyReportOfReceipt = new ED818DBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceipt[]
			{
				new ED818DBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceipt
				{
					BodyRecordUniqueReference = "1"
				}
			};
			CombineAssertions(() =>
			{
				var reportOfReceipts = dataProvider.ReportOfReceipts;
				AssertEquals("1 Record", 1, reportOfReceipts.Count);
				AssertSame("Cached", reportOfReceipts, dataProvider.ReportOfReceipts);
			});
		}

		public void TestReportOfReceipts_NoRecords()
		{
			var reportOfReceipts = dataProvider.ReportOfReceipts;
			AssertEquals("No Records", false, reportOfReceipts.Any());
		}

		protected override void SetUp()
		{
			base.SetUp();

			message = new ED818D
			{
				Header = new ED818DHeader
				{
					MessageGroup = ED818DHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102",
				},
				Body = new ED818DBody
				{
					AcceptedOrRejectedReportOfReceipt = new ED818DBodyAcceptedOrRejectedReportOfReceipt
					{
						ExciseMovement = new ED818DBodyAcceptedOrRejectedReportOfReceiptExciseMovement
						{
							AdministrativeReferenceCode = "20DE41000000001870745",
							SequenceNumber = "1"
						},
						ReportOfReceipt = new ED818DBodyAcceptedOrRejectedReportOfReceiptReportOfReceipt
						{
							GlobalConclusionOfReceipt = ED818DBodyAcceptedOrRejectedReportOfReceiptReportOfReceiptGlobalConclusionOfReceipt.Item21
						}
					}
				}
			};
			dataProvider = new ED818Provider(message);
		}
		ED818D message;
		IED818 dataProvider;

		protected override ED818Provider GetProvider() => (ED818Provider)dataProvider;
	}
}
