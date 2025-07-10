using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Poland.Testing
{
	public class PolandEInvoicingAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		public void TestNullTransactions_Returns_Null()
		{
			var (bizoBatch, branch, factory, logger) = CreateObjectsForTest();

			var universalBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			universalBatch.TransactionCollection = null;

			var result = new PolandEInvoicingAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(bizoBatch, branch, universalBatch, factory, logger);

			AssertNull("Null transactions collection should not throw and should return a null collection", result);
		}

		public void TestNoTransactions_Returns_Null()
		{
			var (bizoBatch, branch, factory, logger) = CreateObjectsForTest();

			var universalBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			universalBatch.TransactionCollection.Clear();

			var result = new PolandEInvoicingAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(bizoBatch, branch, universalBatch, factory, logger);

			AssertNull("Empty transactions collection should return a null collection", result);
		}

		public void TestNoRelatedShipments_Returns_Null()
		{
			var (bizoBatch, branch, factory, logger) = CreateObjectsForTest();

			var universalBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			universalBatch.TransactionCollection.Add(CreateTransaction("AR", "1234", TransactionType.INV, "ABCORG"));
			universalBatch.TransactionCollection.Add(CreateTransaction("AP", "2345", TransactionType.CRD, "DEFORG"));

			var result = new PolandEInvoicingAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(bizoBatch, branch, universalBatch, factory, logger);

			AssertNull("No related shipments should return a null collection", result);
		}

		public void TestTransactionWithShipment_Returns_DataItem_ContainingOrderNumbersAndGoodsDescription()
		{
			var (bizoBatch, branch, factory, logger) = CreateObjectsForTest();

			var universalBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			universalBatch.TransactionCollection.Add(
				CreateTransaction("AR", "1234", TransactionType.INV, "ABCORG")
					.WithShipment("The description of the goods", new[] { "ordNum12" })
			);

			var result = new PolandEInvoicingAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(bizoBatch, branch, universalBatch, factory, logger);

			CombineAssertions(() =>
			{
				AssertEquals("Related shipment should return an additional data item", 1, result.Count);
				AssertEquals("Key should encode transaction natural key", "AR:INV:ABCORG:1234║ShipmentDetails", result[0].Key);
				AssertEquals(
					  "Value should encode shipment details as JSON",
						@"{""GoodsDescriptions"":[""The description of the goods""],""OrderNumbers"":[""ordNum12""]}",
						result[0].Value
				);
			});
		}

		public void TestManyTransactionsWithShipment_Returns_DataItemPerTransaction_ContainingOrderNumbersAndGoodsDescription()
		{
			var (bizoBatch, branch, factory, logger) = CreateObjectsForTest();

			var universalBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			universalBatch.TransactionCollection.Add(
				CreateTransaction("AR", "1234", TransactionType.INV, "ABCORG")
					.WithShipment("The description of the goods", new[] { "ordNum12" })
			);
			universalBatch.TransactionCollection.Add(
				CreateTransaction("AR", "2345", TransactionType.INV, "DEFORG")
					.WithShipment("Different description of goods", new[] { "someNum999", "2second", "TheThird" })
			);
			universalBatch.TransactionCollection.Add(
				CreateTransaction("AP", "00001", TransactionType.CRD, "ABCORG")
					.WithShipment("Things", Array.Empty<string>())
			);
			universalBatch.TransactionCollection.Add(
				CreateTransaction("AP", "00011", TransactionType.INV, "")
					.WithShipment("", new[] { "766666.13a" })
			);
			universalBatch.TransactionCollection.Add(
				CreateTransaction("AP", "00111", TransactionType.INV, "ABCORG")
					.WithShipment("", Array.Empty<string>())
			);

			var result = new PolandEInvoicingAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(bizoBatch, branch, universalBatch, factory, logger);

			CombineAssertions(() =>
			{
				AssertEquals("Related shipment should return an additional data item per transaction", 4, result.Count);

				AssertEquals("[0] Key should encode transaction natural key", "AR:INV:ABCORG:1234║ShipmentDetails", result[0].Key);
				AssertEquals(
					  "[0] Value should encode shipment details as JSON",
						@"{""GoodsDescriptions"":[""The description of the goods""],""OrderNumbers"":[""ordNum12""]}",
						result[0].Value
				);

				AssertEquals("[1] Key should encode transaction natural key", "AR:INV:DEFORG:2345║ShipmentDetails", result[1].Key);
				AssertEquals(
					  "[1] Value should encode shipment details as JSON",
						@"{""GoodsDescriptions"":[""Different description of goods""],""OrderNumbers"":[""someNum999"",""2second"",""TheThird""]}",
						result[1].Value
				);

				AssertEquals("[2] Key should encode transaction natural key", "AP:CRD:ABCORG:00001║ShipmentDetails", result[2].Key);
				AssertEquals(
					  "[2] Value should encode shipment details as JSON",
						@"{""GoodsDescriptions"":[""Things""],""OrderNumbers"":[]}",
						result[2].Value
				);

				AssertEquals("[3] Key should encode transaction natural key", "AP:INV::00011║ShipmentDetails", result[3].Key);
				AssertEquals(
					  "[3] Value should encode shipment details as JSON",
						@"{""GoodsDescriptions"":[],""OrderNumbers"":[""766666.13a""]}",
						result[3].Value
				);

				AssertExceptionThrown<ArgumentOutOfRangeException>("Related shipment with no additional data should not be returned to save bytes on wire", () => _ = result[4]);
			});
		}

		public void TestManyTransactionsTransactionWithManyShipments_Returns_DataItemPerTransaction_ContainingOrderNumbersAndGoodsDescriptionArrays()
		{
			var (bizoBatch, branch, factory, logger) = CreateObjectsForTest();

			var universalBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			universalBatch.TransactionCollection.Add(
				CreateTransaction("AR", "1234", TransactionType.INV, "ABCORG")
					.WithShipment("Goods", new[] { "5000", "5001" })
					.WithShipment("More Goods", new[] { "5993", "5993a" })
			);
			universalBatch.TransactionCollection.Add(
				CreateTransaction("AP", "01111", TransactionType.INV, "ORG007")
					.WithShipment("Spy Equipment", new[] { "5000", "5001" })
					.WithShipment("REDACTED", new[] { "5993" })
			);
			universalBatch.TransactionCollection.Add(
				CreateTransaction("AP", "11111", TransactionType.INV, "ABCORG")
					.WithShipment("Goods", new[] { "5000", "5001" })
					.WithShipment("More Goods", new[] { "5993", "5993a" })
			);

			var result = new PolandEInvoicingAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(bizoBatch, branch, universalBatch, factory, logger);

			CombineAssertions(() =>
			{
				AssertEquals("Related shipment should return an additional data item per transaction", 3, result.Count);

				AssertEquals("[0] Key should encode transaction natural key", "AR:INV:ABCORG:1234║ShipmentDetails", result[0].Key);
				AssertEquals(
					  "[0] Value should encode shipment details as JSON",
						@"{""GoodsDescriptions"":[""Goods"",""More Goods""],""OrderNumbers"":[""5000"",""5001"",""5993"",""5993a""]}",
						result[0].Value
				);

				AssertEquals("[1] Key should encode transaction natural key", "AP:INV:ORG007:01111║ShipmentDetails", result[1].Key);
				AssertEquals(
					  "[1] Value should encode shipment details as JSON",
						@"{""GoodsDescriptions"":[""Spy Equipment"",""REDACTED""],""OrderNumbers"":[""5000"",""5001"",""5993""]}",
						result[1].Value
				);

				AssertEquals("[2] Key should encode transaction natural key", "AP:INV:ABCORG:11111║ShipmentDetails", result[2].Key);
				AssertEquals(
					  "[2] Value should encode shipment details as JSON",
						@"{""GoodsDescriptions"":[""Goods"",""More Goods""],""OrderNumbers"":[""5000"",""5001"",""5993"",""5993a""]}",
						result[2].Value
				);
			});
		}

		public void TestDuplicateData_IsRemoved_InOrderNumbersAndGoodsDescriptionArrays()
		{
			var (bizoBatch, branch, factory, logger) = CreateObjectsForTest();

			var universalBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			universalBatch.TransactionCollection.Add(
				CreateTransaction("AR", "1234", TransactionType.INV, "ABCORG")
					.WithShipment("Car parts", new[] { "ordNum12", "number999" })
					.WithShipment("Car parts", new[] { "32" })
					.WithShipment("Computer parts", new[] { "ordNum12", "32" })
			);

			var result = new PolandEInvoicingAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(bizoBatch, branch, universalBatch, factory, logger);

			CombineAssertions(() =>
			{
				AssertEquals("Related shipment should return an additional data item per transaction", 1, result.Count);

				AssertEquals("Key should encode transaction natural key", "AR:INV:ABCORG:1234║ShipmentDetails", result[0].Key);
				AssertEquals("Value should encode shipment details as JSON; duplicate values are removed",
						@"{""GoodsDescriptions"":[""Car parts"",""Computer parts""],""OrderNumbers"":[""ordNum12"",""number999"",""32""]}",
						result[0].Value
				);
			});
		}

		public void TestWhenGoodsDescriptionIsNull()
		{
			var (bizoBatch, branch, factory, logger) = CreateObjectsForTest();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.GoodsDescription = null;
			var orderNumberCollection = new DataObjectList<OrderNumber>() { new OrderNumber() { OrderReference = "1", Sequence = 1 } };
			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing.SetOrderNumberCollection(() => orderNumberCollection);

			var transaction = CreateTransaction("AR", "1234", TransactionType.INV, "ABCORG");
			transaction.SetShipmentCollection(() => new List<UniversalShipment>() { shipment });
			var universalBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			universalBatch.TransactionCollection.Add(transaction);

			var result = new PolandEInvoicingAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(bizoBatch, branch, universalBatch, factory, logger);

			CombineAssertions(() =>
			{
				AssertEquals("Related shipment should return an additional data item", 1, result.Count);
				AssertEquals("Key should encode transaction natural key", "AR:INV:ABCORG:1234║ShipmentDetails", result[0].Key);
				AssertEquals("Value should encode shipment details as JSON; no exceptions from nulls",
						@"{""GoodsDescriptions"":[],""OrderNumbers"":[""1""]}",
						result[0].Value
				);
			});
		}

		public void TestWhenLocalProcessingIsNull()
		{
			var (bizoBatch, branch, factory, logger) = CreateObjectsForTest();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.GoodsDescription = "Goods";
			AssertNull("Precondition", shipment.LocalProcessing);

			var transaction = CreateTransaction("AR", "1234", TransactionType.INV, "ABCORG");
			transaction.SetShipmentCollection(() => new List<UniversalShipment>() { shipment });
			var universalBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			universalBatch.TransactionCollection.Add(transaction);

			var result = new PolandEInvoicingAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(bizoBatch, branch, universalBatch, factory, logger);

			CombineAssertions(() =>
			{
				AssertEquals("Related shipment should return an additional data item", 1, result.Count);
				AssertEquals("Key should encode transaction natural key", "AR:INV:ABCORG:1234║ShipmentDetails", result[0].Key);
				AssertEquals("Value should encode shipment details as JSON; no exceptions from nulls",
						@"{""GoodsDescriptions"":[""Goods""],""OrderNumbers"":[]}",
						result[0].Value
				);
			});
		}

		public void TestWhenOrderNumberCollectionIsNull()
		{
			var (bizoBatch, branch, factory, logger) = CreateObjectsForTest();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.GoodsDescription = "Goods";
			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			AssertNull("Precondition: OrderNumberCollection must be null", shipment.LocalProcessing.OrderNumberCollection);

			var transaction = CreateTransaction("AR", "1234", TransactionType.INV, "ABCORG");
			transaction.SetShipmentCollection(() => new List<UniversalShipment>() { shipment });
			var universalBatch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			universalBatch.TransactionCollection.Add(transaction);

			var result = new PolandEInvoicingAdditionalDataItemsProvider().GetAdditionalHeaderDataItems(bizoBatch, branch, universalBatch, factory, logger);

			CombineAssertions(() =>
			{
				AssertEquals("Related shipment should return an additional data item", 1, result.Count);
				AssertEquals("Key should encode transaction natural key", "AR:INV:ABCORG:1234║ShipmentDetails", result[0].Key);
				AssertEquals("Value should encode shipment details as JSON; no exceptions from nulls",
						@"{""GoodsDescriptions"":[""Goods""],""OrderNumbers"":[]}",
						result[0].Value
				);
			});
		}

		#region Implementation

		UniversalTransaction CreateTransaction(string ledger, string number, TransactionType type, string orgCode)
		{
			var result = new UniversalTransaction(DefaultDataObjectWriterStrategy.TestInstance);
			result.Ledger = ledger;
			result.TransactionType = type;
			result.Number = number;
			result.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			result.OrganizationAddress.OrganizationCode = orgCode;
			return result;
		}

		(AccEInvoicingBatch bizoBatch, GlbBranch branch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings) CreateObjectsForTest()
			=> (Factory.New<AccEInvoicingBatch>(), GlbBranch.CurrentBranch, new PolandEInvoicingObjectFactory(), new Logger());

		#endregion
	}

	static class PolandEInvoicingAdditionalDataItemsProviderTestHelper
	{
		public static UniversalTransaction WithShipment(this UniversalTransaction transaction, string goodsDescription, IEnumerable<string> orderNumbers)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.GoodsDestination = "Some destination which should not be mapped";
			shipment.GoodsDescription = goodsDescription;
			var orderNumberCollection = new DataObjectList<OrderNumber>(orderNumbers.Select((x, i) => new OrderNumber() { OrderReference = x, Sequence = (short)i }));
			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing.SetOrderNumberCollection(() => orderNumberCollection);

			var newCollection = (transaction.ShipmentCollection ?? new List<UniversalShipment>()).Concat(new[] { shipment }).ToList();
			transaction.SetShipmentCollection(() => newCollection);
			return transaction;
		}
	}
}
