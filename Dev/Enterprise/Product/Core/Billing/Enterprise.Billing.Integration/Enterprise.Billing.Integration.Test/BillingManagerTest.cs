using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Billing.Integration.Test
{
	class BillingManagerTest : TestCaseWithFactory
	{
		public void TestAddAndGetTransactions()
		{
			AssertEquals("Assertion: no Error Report is generated", 0, ErrorReporter.TotalErrorCount);
			var transactions = new List<BillingTransaction>();
			for (int i = 0; i < 10; i++)
			{
				transactions.Add(new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI" + i,
					Reference1 = "Item " + i,
					Reference2 = "A" + (char)30 + "B", // test for treatment of invalid xml chars
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = new DateTime(2019, 12, 12, 12, 12, 12)
				});
			}
			SimulatePriceItemCodeConstantFail("TST", "PI4");
			new BillingManager().AddTransactions(transactions, Db.Connection);

			SimulatePriceItemCodeConstantFail("TST", "PI7");
			var items = new BillingManager().GetTransactions(Factory, 10);
			var priceItemCodes = items.Select(BillingManager.GetPriceItemCode);
			AssertContainsExactElementsInAnyOrder(
				new[] { "PI0", "PI1", "PI2", "PI3", "PI4", "PI5", "PI6", "PI7", "PI8", "PI9" },
				priceItemCodes);
		}

		public void TestGetTransactions_OrderedByDate()
		{
			var transactions = new List<BillingTransaction>();
			for (int i = 0; i < 4; i++)
			{
				transactions.Add(new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI" + i,
					Reference1 = "Item " + i,
					Reference2 = "A" + (char)30 + "B", // test for treatment of invalid xml chars
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow
				});
				new BillingManager().AddTransactions(transactions, Db.Connection);
				Thread.Sleep(100);
				transactions.Clear();
			}
			var items = new BillingManager().GetTransactions(Factory, 1000).ToArray();
			AssertEquals(4, items.Length);
			Assert((ZDateTime)items[0]["SUD_PostedTimeUtc"] < (ZDateTime)items[1]["SUD_PostedTimeUtc"]);
			Assert((ZDateTime)items[1]["SUD_PostedTimeUtc"] < (ZDateTime)items[2]["SUD_PostedTimeUtc"]);
			Assert((ZDateTime)items[2]["SUD_PostedTimeUtc"] < (ZDateTime)items[3]["SUD_PostedTimeUtc"]);
		}

		public void TestAddTransactionsWhenRefFieldsFailLengthRestrictions()
		{
			const string tooLongReferenceField = "12345678901234567890123456789012345678901234567890111";

			var transactions = new List<BillingTransaction>
			{
				new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI2",
					Reference1 = tooLongReferenceField,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow,
					Version = 1
				},
				new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI2",
					Reference2 = tooLongReferenceField,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow,
					Version = 2
				},
				new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI2",
					Reference3 = tooLongReferenceField,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow,
					Version = 3
				},
				new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI2",
					Reference4 = tooLongReferenceField,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow,
					Version = 4
				},
				new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI2",
					Reference5 = tooLongReferenceField,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow,
					Version = 5
				},
				new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI2",
					Reference1 = tooLongReferenceField,
					Reference2 = tooLongReferenceField,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow,
					Version = 6
				},
				new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI2",
					Reference1 = "1234567890",
					Reference2 = tooLongReferenceField,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow,
					Version = 7
				},
				new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI2",
					Reference1 = null,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow,
					Version = 8
				},
				new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI2",
					Reference1 = string.Empty,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow,
					Version = 9
				}
			};
			AssertNoExceptionThrown(() => new BillingManager().AddTransactions(transactions, Db.Connection));

			var orderedTransactions = transactions.OrderBy(txn => txn.Version);
			var ref1Field = orderedTransactions.Skip(0).Take(1).FirstOrDefault()?.Reference1;
			var ref2Field = orderedTransactions.Skip(1).Take(1).FirstOrDefault()?.Reference2;
			var ref3Field = orderedTransactions.Skip(2).Take(1).FirstOrDefault()?.Reference3;
			var ref4Field = orderedTransactions.Skip(3).Take(1).FirstOrDefault()?.Reference4;
			var ref5Field = orderedTransactions.Skip(4).Take(1).FirstOrDefault()?.Reference5;
			var ref6_1Field = orderedTransactions.Skip(5).Take(1).FirstOrDefault()?.Reference1;
			var ref6_2Field = orderedTransactions.Skip(5).Take(1).FirstOrDefault()?.Reference2;
			var ref7_1Field = orderedTransactions.Skip(6).Take(1).FirstOrDefault()?.Reference1;
			var ref7_2Field = orderedTransactions.Skip(6).Take(1).FirstOrDefault()?.Reference2;
			var ref8_1Field = orderedTransactions.Skip(7).Take(1).FirstOrDefault()?.Reference1;
			var ref9_1Field = orderedTransactions.Skip(8).Take(1).FirstOrDefault()?.Reference1;

			AssertEquals("Reference1 is truncated", 50, ref1Field.Length);
			AssertEquals("Reference2 is truncated", 50, ref2Field.Length);
			AssertEquals("Reference3 is truncated", 50, ref3Field.Length);
			AssertEquals("Reference4 is truncated", 50, ref4Field.Length);
			AssertEquals("Reference5 is truncated", 50, ref5Field.Length);
			AssertEquals("Reference1 is truncated", 50, ref6_1Field.Length);
			AssertEquals("Reference2 is truncated", 50, ref6_2Field.Length);
			AssertEquals("Reference1 is unchanged", 10, ref7_1Field.Length);
			AssertEquals("Reference2 is truncated", 50, ref7_2Field.Length);
			AssertEquals("Reference1 is mandatory", 1, ref8_1Field.Length);
			AssertEquals("Reference1 is mandatory", 1, ref9_1Field.Length);
		}

		public void TestAddAndGetTransactionsWhenManyFailed()
		{
			var transactions = new List<BillingTransaction>();
			var passedTransactionTime = DateTime.UtcNow;
			var failedTransactionTime = passedTransactionTime.AddSeconds(-1.0);
			for (int i = 0; i < 1000; i++)
			{
				transactions.Add(new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = BillingManager.InvalidCategory,
					PriceItemCode = "PI_",
					Reference1 = "Item " + i,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = failedTransactionTime
				});
			}
			for (int i = 0; i < 10; i++)
			{
				transactions.Add(new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "TST",
					PriceItemCode = "PI" + i,
					Reference1 = "Item " + i,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = passedTransactionTime
				});
			}
			new BillingManager().AddTransactions(transactions, Db.Connection);

			Assert("Get transactions should only load one batch of records at a time.", !new BillingManager().GetTransactions(Factory, 1000).Any());
			AssertEquals("NotFailedCategory [" + BillingManager.InvalidCategory + "]", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
			Assert("Transactions were found", new BillingManager().GetTransactions(Factory, 10).IsCountMoreThan(0));
		}

		public void TestMarkAsFailed()
		{
			var transactions = new List<BillingTransaction>();
			for (int i = 0; i < 10; i++)
			{
				transactions.Add(new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "CAT",
					PriceItemCode = "PI" + i,
					Reference1 = "Item " + i,
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = DateTime.UtcNow
				});
			}
			new BillingManager().AddTransactions(transactions, Db.Connection);
			var items = new BillingManager().GetTransactions(Factory, 10);
			var itemToFail = items.First(item => BillingManager.GetPriceItemCode(item) == "PI6");
			BillingManager.MarkAsFailed(itemToFail.PK);
			items = new BillingManager().GetTransactions(new BusinessObjectFactory(), 10);
			var priceItemCodes = items.Select(BillingManager.GetPriceItemCode);
			AssertContainsExactElementsInAnyOrder(
				new[] { "PI0", "PI1", "PI2", "PI3", "PI4", "PI5", "PI7", "PI8", "PI9" },
				priceItemCodes);
		}

		public void TestGetTransactionStreamAndXml()
		{
			new BillingManager().AddTransactions(new List<BillingTransaction>
			{
				new BillingTransaction
				{
					BillableCount = 1,
					ClientID = "ABCDEFXYZ",
					Category = "CAT",
					PriceItemCode = "PIC",
					Reference1 = "REFERENCE",
					ReportingSource = BillingManager.ReportingSource,
					ServiceOccuredUTC = new DateTime(2015, 5, 8, 10, 3, 0),
					AdditionalRefs = "MyData"
				}
			}, Db.Connection);
			var items = new BillingManager().GetTransactions(Factory, 10).ToList();
			AssertEquals(1, items.Count);
			var transactionXml = BillingManager.GetTransactionXml(items[0]);
			AssertXMLEquals(@"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.4"">
  <BillableCount>1</BillableCount>
  <Category>CAT</Category>
  <ClientID>ABCDEFXYZ</ClientID>
  <PriceItemCode>PIC</PriceItemCode>
  <Reference1>REFERENCE</Reference1>
  <ReportingSource>ENT</ReportingSource>
  <ServiceOccuredUTC>2015-05-08T10:03:00</ServiceOccuredUTC>
  <Version>0</Version>
  <AdditionalRefs>MyData</AdditionalRefs>
</BillingTransaction>", transactionXml);
			using (var stream = new MemoryStream(BillingManager.GetTransactionData(items[0])))
			using (var reader = new StreamReader(stream, new UTF8Encoding(false)))
			{
				AssertXMLEquals(transactionXml, reader.ReadToEnd());
			}
		}

		public void TestNoLocalSchemaValidation()
		{
			var transaction = new BillingTransaction
			{
				BillableCount = 1,
				ServiceOccuredUTC = new DateTime(2015, 5, 11, 14, 40, 0),
				Category = "CAT",
				ClientID = " ",
				PriceItemCode = "PIC",
				ReportingSource = "\t ",
				Reference1 = "\r\n"
			};
			new BillingManager().AddTransactions(new List<BillingTransaction> { transaction }, Db.Connection);
			AssertEquals("Transactions were found", 1, new BillingManager().GetTransactions(Factory, 10).Count());
		}

		public void TestNotFailedTransactionsWithIncorrectCodesAreReported()
		{
			var itemWithNoPriceItemCode = CreateBillingTransactionItem("TST", BillingManager.NoPriceItemCode, false, "Some XML");
			Factory.Save();
			Assert(!new BillingManager().GetTransactions(Factory, 10).Any());
			AssertEquals("NotFailedPriceItemCode [" + BillingManager.NoPriceItemCode + "]", ErrorReporter.LastKeyReported);
			AssertEquals("XML: [Some XML]", ErrorReporter.LastMessageReported);
			Assert("Item was marked as failed", (ZBool)itemWithNoPriceItemCode[StmUsageDataSchema.Constants.SUD_Fail]);

			var itemWithInvalidPriceItemCode = CreateBillingTransactionItem("TST", BillingManager.InvalidPriceItemCode, false, "Some Other XML");
			Factory.Save();
			Assert(!new BillingManager().GetTransactions(Factory, 10).Any());
			AssertEquals("NotFailedPriceItemCode [" + BillingManager.InvalidPriceItemCode + "]", ErrorReporter.LastKeyReported);
			AssertEquals("XML: [Some Other XML]", ErrorReporter.LastMessageReported);
			Assert("Item was marked as failed", (ZBool)itemWithInvalidPriceItemCode[StmUsageDataSchema.Constants.SUD_Fail]);

			ErrorReporter.Clear();
		}

		public void TestNotFailedTransactionsWithIncorrectCategoriesAreReported()
		{
			var itemWithNoPriceItemCode = CreateBillingTransactionItem(BillingManager.NoCategory, "TST", false, "Some XML");
			Factory.Save();
			Assert(!new BillingManager().GetTransactions(Factory, 10).Any());
			AssertEquals("NotFailedCategory [" + BillingManager.NoCategory + "]", ErrorReporter.LastKeyReported);
			AssertEquals("XML: [Some XML]", ErrorReporter.LastMessageReported);
			Assert("Item was marked as failed", (ZBool)itemWithNoPriceItemCode[StmUsageDataSchema.Constants.SUD_Fail]);

			var itemWithInvalidPriceItemCode = CreateBillingTransactionItem(BillingManager.InvalidCategory, "TST", false, "Some Other XML");
			Factory.Save();
			Assert(!new BillingManager().GetTransactions(Factory, 10).Any());
			AssertEquals("NotFailedCategory [" + BillingManager.InvalidCategory + "]", ErrorReporter.LastKeyReported);
			AssertEquals("XML: [Some Other XML]", ErrorReporter.LastMessageReported);
			Assert("Item was marked as failed", (ZBool)itemWithInvalidPriceItemCode[StmUsageDataSchema.Constants.SUD_Fail]);

			ErrorReporter.Clear();
		}

		public void TestEncryptDecryptTransaction()
		{
			var transaction = new BillingTransaction
			{
				BillableCount = 1,
				ClientID = "ABCDEFXYZ",
				PriceItemCode = "TST",
				Reference1 = "REF 1",
				Reference2 = "REF 2",
				Reference3 = "REF 3",
				Reference4 = "REF 4",
				ReportingSource = BillingManager.ReportingSource,
				ServiceOccuredUTC = DateTime.UtcNow,
				ClientNumber = "CN",
				ClientStaffCode = "CSC",
				AdditionalRefs = "DATA"
			};
			AssertEquals(
				transaction.ToString(),
				BillingManager.DecryptTransaction(BillingManager.EncryptTransaction(transaction), BillingManager.CurrentSchemaVersion).ToString());
		}

		[ExpectNoExceptions]
		public void TestCanDecryptOldTransaction()
		{
			const string data = "MwIcTPirrgrS/WLNZOr/5UlY3BmkxwGimk55Wg+SHMPMIakZgYSo1CyhpNdHekJTsb452VRM9pvRnk2bCtVArpu6BVfx33tHVOJ8CExrMgQ4m9SNt48r8Fkun3RoSPhH+UekQL/3LpBvdLzJy4Ow5BqpzK9lf+Geo9pzqUsIaKBzazewbsw8+7xHOaZBXuYLvgueJdMQBtWTwQzFBmRCPvKMii0k0qPr45LamwJRpzrxbSL7uqIKdzvw+yRx/Hpy9y+02rJxlJ6/8NFOX0XfQASQWmBF/l9e98GVJljkE0y9kUAKCZXUwZcUd/2ln6cAQZZ37NqnegYAGEleJcYnN9lyjICltvbjbvdfp25/+ovfr6i6dsXjDOOY/vv25qZy/M3ScdCKnoqWqlzjhcAZqSV+/P71503ZV64g37iaPCWN/FNVTCzlsPDRwS2o2z8ST5J+f9xHKNOc8fpJurwmVOQYI2moOM0auHc37JhNOl0rwSIvd2FWPHhaviEf1CbljiXSnp5rb86yrUr+6gsb3liZU6iI8xghO+Sex5bhRv0=";
			BillingManager.DecryptTransaction(data, "");
		}

		[ExpectNoExceptions]
		public void TestCanDecryptTransactionV12WhenNoSchemaSpecified()
		{
			const string data = "RhYf7r5Z5GJqqHM0eeiq+o0+VCXuu1fnLd+JbjGIEZ5s8PpizWkg5XeEC05TxXgL1e6ZqhWQ3lfHRscVF2K6nWdn1eDNfvyKs8GgPndBN7HGeg2CsSIfkas7NMF5AREmGSbPqDrIjvjX9k7KAVxj3aVp4ZoPwao9ax9WBhBLpDA06m/XI6OHIE5hl7wChPa7YRGJpuLPO7fORn7OYPxlI1/KdEDqtDF4G/qH2xpBW72PgN7s05fY2wa4AiEhAF1JjHic2ooe6gBRJES8RePMP6DuSkvoH6k0ZB3W0rxKapoaRrZh3C8YrtNHjdLn3zsE5lv+PNc973CVg1SgpS9IqXWCOXq1YhIgpxLzyIcI1sr/WjT0SNnpzcLjcRYkGYuonWKlZykSZBmrFJmv25a9lELG1BjCZ1uxvLwk//CFCaY5fT2kiMBpIIMPyQucJ1zipTAzVtsn/FvF/gVs0ffzdGqqubWAGFQfAMS/CyDSQyi5PKlg+9nYrVXEzkoFk9E0PS/43UHvmeyRTayK9XLn1v/y+IBxXP7BUXYjsFoRbkyB5C8ms9uFf4E5bYS8yH7n6Yhdm/WHYbv424mfe5a2GUU3N7aCZb4k8E40Lf2hWTlkmQG4/6zZsJ6iNwcOEMHZ";
			BillingManager.DecryptTransaction(data, "");
		}

		[ExpectNoExceptions]
		public void TestCanDecryptTransactionV12WhenSchemaSpecified()
		{
			const string data = "RhYf7r5Z5GJqqHM0eeiq+o0+VCXuu1fnLd+JbjGIEZ5s8PpizWkg5XeEC05TxXgL1e6ZqhWQ3lfHRscVF2K6nWdn1eDNfvyKs8GgPndBN7HGeg2CsSIfkas7NMF5AREmGSbPqDrIjvjX9k7KAVxj3aVp4ZoPwao9ax9WBhBLpDA06m/XI6OHIE5hl7wChPa7YRGJpuLPO7fORn7OYPxlI1/KdEDqtDF4G/qH2xpBW72PgN7s05fY2wa4AiEhAF1JjHic2ooe6gBRJES8RePMP6DuSkvoH6k0ZB3W0rxKapoaRrZh3C8YrtNHjdLn3zsE5lv+PNc973CVg1SgpS9IqXWCOXq1YhIgpxLzyIcI1sr/WjT0SNnpzcLjcRYkGYuonWKlZykSZBmrFJmv25a9lELG1BjCZ1uxvLwk//CFCaY5fT2kiMBpIIMPyQucJ1zipTAzVtsn/FvF/gVs0ffzdGqqubWAGFQfAMS/CyDSQyi5PKlg+9nYrVXEzkoFk9E0PS/43UHvmeyRTayK9XLn1v/y+IBxXP7BUXYjsFoRbkyB5C8ms9uFf4E5bYS8yH7n6Yhdm/WHYbv424mfe5a2GUU3N7aCZb4k8E40Lf2hWTlkmQG4/6zZsJ6iNwcOEMHZ";
			BillingManager.DecryptTransaction(data, "1.2");
		}

		[ExpectNoExceptions]
		public void TestCanDecryptTransactionV14WhenNoSchemaSpecified()
		{
			const string data = "r/DexUF2gPT+r5Hu612M6qSBA63LeFtm3JnX4hXn99Xnqj8qNJxxoUQn88dQZDGwtnHniYSPd0MEff3Cfp5EE6Ox0sbE1IYqGBZ3iOlIZjJ+L/gfHHDQeqMDDlEtnf+BAZredkgIqMjExSKEyjlHLuepe2EEUMUWN8DT5SlAgha3gBxXKQMInmPsaozo+86Z6XY9mBcAaAeYmgKn6oLleayQy53QTw1a+48YeD5xFSnoLaXpXvWOJFW9hIaIWaV+znJEnWmAo9NKEN++lvDIR9VAaaILarCcsxoTLNyCljJTrPVQgRhe/pl8HuMGj2hbS5xD4NwRB8eMIF6KiTFW1CQcmqBno3amgOeaC/jabnyeMRLkMQzKFdWcgnpdsV9JRWDPuCFOLtTCmViVp/RmR6FWq2oqbJrNzoNRwqgey4yKoUlCAmUpA/VoqK2DNTfshx84UU5PNaY3xBi5ttXYur4WDeadx08PvRNi0gJ+MjYThQSuGjaWYZDaCaf89i6rDDm4dQ5cmAeojHZVHUZeDB4ZkFUNzsWUoMBgR7BXcqpsJ+0LIM6Zu4TG3Sj0mZ8B1/BIqS9IIRB3Z3rXdRlmO0W/Zu0m8DmFcOC77dDPK+ivlMIFRt7Ne5Mhb5ttXB++picWeq7pHjACMnTRUgG48Q==";
			BillingManager.DecryptTransaction(data, "");
		}

		public void TestAddAndGetUsageTransactionXml()
		{
			AssertEquals("Assertion: no Error Report is generated", 0, ErrorReporter.TotalErrorCount);
			var additionalRefs = "{  \"FeatureCode\": \"ATR\",  \"Module\": \"Rating\",  \"FeatureDescription\": \"AutoRate\",  \"EnterpriseCode\": \"WUT\",  \"OrganisationName\": \"YOUR AUSTRALIA CORPORATION\",  \"ServerCode\": \"BC0\",  \"CompanyCode\": \"DAU\",  \"CompanyName\": \"Your Australia Corp\",  \"BranchCode\": \"A01\",  \"Environment\": \"TST\",  \"RatesSearchResult\": {    \"CargoSphere\": {      \"IsAllowed\": true    },    \"Cargoguide\": {      \"IsAllowed\": true    },    \"CW1\": {      \"IsEnabled\": true,      \"IsAllowed\": true,      \"TotalRates\": 4,      \"TotalCharges\": 9    }  },  \"IsRateSelector\": true,  \"TriggerSource\": \"Menu\",  \"ContainerMode\": \"LSE\",  \"TransportMode\": \"AIR\",  \"Mode\": \"Revenue\",  \"JobType\": \"Shipment\"}";
			var transactions = new List<UsageTransaction>();
			transactions.Add(new UsageTransaction
			{
				UsageCount = 7,
				AdditionalRefs = additionalRefs,
				ServiceOccuredUTC = new DateTime(2023, 12, 9, 0, 17, 24, DateTimeKind.Utc)
			});
			new BillingManager().AddTransactions(transactions, Db.Connection);

			var items = new BillingManager().GetTransactions(Factory, 10);
			AssertEquals(1, items.Count());
			var item = items.Single();
			AssertEquals("Wrong category", "USG", item["SUD_Category"]);
			var transactionXml = BillingManager.GetTransactionXml(item);
			AssertXMLEquals($@"<?xml version=""1.0"" encoding=""utf-8""?>
<UsageTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/#Usage_2.1"">
  <UsageCount>7</UsageCount>
  <ServiceOccuredUTC>2023-12-09T00:17:24Z</ServiceOccuredUTC>
  <AdditionalRefs>{additionalRefs}</AdditionalRefs>
</UsageTransaction>", transactionXml);
		}

		void SimulatePriceItemCodeConstantFail(string category, string priceItemCode)
		{
			for (int i = 0; i < 30; i++)
			{
				CreateBillingTransactionItem(category, priceItemCode, true, "Some Encrypted Data. Shouldn't be used.");
			}
			Factory.Save();
		}

		BusinessObject CreateBillingTransactionItem(string category, string priceItemCode, bool failed, string data)
		{
			var result = Factory.New(BillingManager.StmUsageDataType);
			result[StmUsageDataSchema.Constants.SUD_Schema] = BillingManager.CurrentSchemaVersion;
			result[StmUsageDataSchema.Constants.SUD_Category] = category;
			result[StmUsageDataSchema.Constants.SUD_Code] = priceItemCode;
			result[StmUsageDataSchema.Constants.SUD_Data] = BillingDataEncryptor.Encrypt(BillingManager.Encoding.GetBytes(data));
			result[StmUsageDataSchema.Constants.SUD_Fail] = failed;
			return result;
		}
	}
}
