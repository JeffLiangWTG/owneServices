using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
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
			message.Body.AcceptedOrRejectedReportOfReceipt.BodyReportOfReceipt = new ED818CBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceipt[]
			{
				new ED818CBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceipt
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

			message = new ED818C
			{
				Header = new ED818CHeader
				{
					MessageGroup = ED818CHeaderMessageGroup.EME,
					MessageIdentifier = "0072260102",
				},
				Body = new ED818CBody
				{
					AcceptedOrRejectedReportOfReceipt = new ED818CBodyAcceptedOrRejectedReportOfReceipt
					{
						ExciseMovementEad = new ED818CBodyAcceptedOrRejectedReportOfReceiptExciseMovementEad
						{
							AdministrativeReferenceCode = "20DE41000000001870745",
							SequenceNumber = "1"
						},
						ReportOfReceipt = new ED818CBodyAcceptedOrRejectedReportOfReceiptReportOfReceipt
						{
							GlobalConclusionOfReceipt = ED818CBodyAcceptedOrRejectedReportOfReceiptReportOfReceiptGlobalConclusionOfReceipt.Item21
						}
					}
				}
			};
			dataProvider = new ED818Provider(message);
		}
		ED818C message;
		IED818 dataProvider;

		protected override ED818Provider GetProvider() => (ED818Provider)dataProvider;
	}
}
