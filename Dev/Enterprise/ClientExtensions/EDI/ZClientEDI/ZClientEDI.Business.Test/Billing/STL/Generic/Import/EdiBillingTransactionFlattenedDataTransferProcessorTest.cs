using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business.Test;
using Moq;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class EdiBillingTransactionFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestImport_Validation()
		{
			var transaction1 = new EdiBillingTransactionFlattened(Factory);
			transaction1.Category = "";
			transaction1.PriceItemCode = "P01";
			transaction1.BillableCount = 5;
			transaction1.ReportingSource = "ABC";
			transaction1.ClientNumber = DatabaseId;
			transaction1.Reference1 = "REF1";

			var transaction2 = new EdiBillingTransactionFlattened(Factory);
			transaction2.Category = "ABC";
			transaction2.PriceItemCode = "P01";
			transaction2.BillableCount = 5;
			transaction2.ReportingSource = "ABC";
			transaction2.ClientNumber = "";
			transaction2.Reference1 = "REF1";

			var transaction3 = new EdiBillingTransactionFlattened(Factory);
			transaction3.Category = "ABC";
			transaction3.PriceItemCode = "P0X";
			transaction3.BillableCount = 5;
			transaction3.ReportingSource = "ABC";
			transaction3.ClientNumber = DatabaseId;
			transaction3.Reference1 = "REF1";

			var transaction4 = new EdiBillingTransactionFlattened(Factory);
			transaction4.Category = "ABC";
			transaction4.PriceItemCode = "P01";
			transaction4.BillableCount = 5;
			transaction4.ReportingSource = "ABC";
			transaction4.ClientNumber = DatabaseId;
			transaction4.ServiceOccuredUTC = ZDateTime.Today;
			transaction4.Reference1 = "REF1";

			var flattenedCollection = new EdiBillingTransactionFlattenedCollection(Factory)
			{
				transaction1,
				transaction2,
				transaction3,
				transaction4
			};

			var collectionInfo = new EdiBillingTransactionImportInfo(flattenedCollection);
			var processor = new EdiBillingTransactionFlattenedDataTransferProcessorForTest(collectionInfo);
			processor.Import();

			var errors = new StringBuilder();
			foreach (EdiBillingTransactionFlattened tx in flattenedCollection)
			{
				foreach (var e in tx.GetErrors())
				{
					errors.AppendLine($"{tx.LineNumber} : {e.Message}");
				}
			}
			AssertEquals(@"1 : Error - Category: Please enter a Category.
1 : Error - ServiceOccuredUTC: Please enter a Service Occurred UTC.
2 : Error - ServiceOccuredUTC: Please enter a Service Occurred UTC.
2 : Error - ClientNumber: Please enter a Client Number.
3 : Error - PriceItemCode: Enter a valid Price Item Code.
3 : Error - ServiceOccuredUTC: Please enter a Service Occurred UTC.
", errors.ToString());
		}

		public void TestImport()
		{
			var flattenedCollection = new EdiBillingTransactionFlattenedCollection(Factory);
			for (int i = 0; i < 278; i++)
			{
				var transaction1 = new EdiBillingTransactionFlattened(Factory);
				transaction1.Category = "ABC";
				transaction1.PriceItemCode = "P01";
				transaction1.BillableCount = i + 1;
				transaction1.ReportingSource = "ABC";
				transaction1.ClientNumber = DatabaseId;
				transaction1.ServiceOccuredUTC = ZDateTime.Today.AddDays(-1).AddMinutes(i);
				transaction1.Reference1 = "REF1";
				flattenedCollection.Add(transaction1);
			}

			var messages = new List<IeHubMessage>();
			var pks = new List<Guid>();
			var statuses = new Dictionary<Guid, OutboundMessageStatus>();

			var outbox = new Mock<IMessageOutbox>();
			outbox.Setup(m => m.AddMessage(It.IsAny<IeHubMessage>()))
				.Callback<IeHubMessage>(x => messages.Add(x));

			var adapter = new Mock<IeHubAdapter>();
			adapter.Setup(m => m.Outbox).Returns(outbox.Object);
			adapter.Setup(m => m.SendMessages());
			adapter.Setup(m => m.GetOutboundMessageStatuses(It.IsAny<Guid[]>()))
				.Callback<Guid[]>(x => pks.AddRange(x))
				.Returns(() =>
				{
					statuses = pks.ToDictionary(k => k, v => OutboundMessageStatus.Sent);

					if (statuses.ContainsKey(messages[55].TrackingID))
					{
						statuses.Remove(messages[55].TrackingID);
					}

					if (statuses.ContainsKey(messages[101].TrackingID))
					{
						statuses[messages[101].TrackingID] = OutboundMessageStatus.Failed;
					}

					if (statuses.ContainsKey(messages[203].TrackingID))
					{
						statuses[messages[203].TrackingID] = OutboundMessageStatus.Pending;
					}

					return statuses;
				});

			var collectionInfo = new EdiBillingTransactionImportInfo(flattenedCollection);
			var processor = new EdiBillingTransactionFlattenedDataTransferProcessorForTest(collectionInfo);
			processor.Adapter = adapter.Object;
			processor.Import();

			var notifications = string.Join("\r\n", flattenedCollection.Where(x => x.HasErrors)
				.Select(x => $"{x.GetErrors().First().Message}"));
			AssertEquals("", notifications);
		}
		protected override void SetUp()
		{
			base.SetUp();

			//ABC/PL0  ABC/PL1
			UsageBillingSettingsTest.SetupValidTestRegistry();
			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "AUS", "SYD");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2", "AU2", "MEL");
			lic1.Database.LD_Product = "ABC";
			var priceList = BillingTestHelper.CreatePriceList(lic1);
			priceList.L6_SystemCode = "PL0";
			BillingTestHelper.AddPriceItem(priceList, new UsageCodeKey("PL0", "P01"), "TRA", 100);
			BillingTestHelper.AddPriceItem(priceList, new UsageCodeKey("PL0", "P02"), "TRA", 200);
			Factory.Save();
			DatabaseId = lic1.Database.DatabaseId;
		}

		string DatabaseId;

		class EdiBillingTransactionFlattenedDataTransferProcessorForTest : EdiBillingTransactionFlattenedDataTransferProcessor
		{
			public EdiBillingTransactionFlattenedDataTransferProcessorForTest(EdiBillingTransactionImportInfo flattenedCollectionInfo) : base(flattenedCollectionInfo)
			{
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			protected override void Sleep(int millisecondsTimeout)
			{
			}

			protected override IeHubAdapter GetNewAdapter() => Adapter;

			public IeHubAdapter Adapter { get; set; }
		}
	}
}
