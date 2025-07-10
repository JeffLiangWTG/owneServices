using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.JAS.Business.Matching.Testing
{
	public abstract class PreMatchedDataLineTest : BasePreMatchedDataLineTest
	{
		public void TestToString()
		{
			string generated = Line.ToString();
			string expected = NettingCodes + ExpectedNormalTransactionString;
			AssertEquals(expected, generated);
		}

		public void TestMultipleMawbsToString()
		{
			string generated = GenerateTransactionWithMultipleMawbsV3_3i2(Ledger);
			string expected = NettingCodes + ExpectedTransactionWithMultipleMawbsV3_3i2;
			AssertEquals(expected, generated);
		}

		public void TestMultipleHawbsToString()
		{
			string generated = GenerateTransactionWithMultipleHawbsV3_3i4(Ledger);
			string expected = NettingCodes + ExpectedTransactionWithMultipleHawbsV3_3i4;
			AssertEquals(expected, generated);
		}

		public void TestCategoryOorTToString()
		{
			string generated = GenerateTransactionWithCategoryOorTV3_3i6(Ledger);
			string expected = NettingCodes + ExpectedTransactionWithCategoryOorTV3_3i6;
			AssertEquals(expected, generated);
		}

		public void TestCategorySToString()
		{
			string generated = GenerateTransactionWithCategorySV3_3i7(Ledger, true);
			string expected = NettingCodes + ExpectedTransactionWithCategorySV3_3i7;
			AssertEquals(expected, generated);
		}

		public void TestCategorySButNoContainerNumToString()
		{
			string generated = GenerateTransactionWithCategorySV3_3i7(Ledger, false);
			string expected = NettingCodes + ExpectedTransactionWithCategorySButNoContainerNum;
			AssertEquals(expected, generated);
		}

		public void TestDirectConsolToString()
		{
			string generated = GenerateTransactionWithDirectConsolV3_3i9(Ledger);
			string expected = NettingCodes + ExpectedTransactionWithDirectConsolV3_3i9;
			AssertEquals(expected, generated);
		}

		public void TestCoLoadConsolToString()
		{
			string generated = GenerateTransactionWithCoLoadConsolV3_3i10(Ledger);
			string expected = NettingCodes + ExpectedTransactionWithCoLoadConsolV3_3i10;
			AssertEquals(expected, generated);
		}

		public void TestCoLoadConsolButNoVoyageNumberToString()
		{
			string generated = GenerateTransactionWithCoLoadConsolButNoVoyageNumber(Ledger);
			string expected = NettingCodes + ExpectedTransactionWithCoLoadConsolButNoVoyageNumber;
			AssertEquals(expected, generated);
		}

		#region Implementation
		protected abstract string ExpectedNormalTransactionString { get; }

		protected abstract string ExpectedTransactionWithMultipleMawbsV3_3i2 { get; }

		protected abstract string ExpectedTransactionWithMultipleHawbsV3_3i4 { get; }

		protected abstract string ExpectedTransactionWithCategoryOorTV3_3i6 { get; }

		protected abstract string ExpectedTransactionWithCategorySV3_3i7 { get; }

		protected abstract string ExpectedTransactionWithCategorySButNoContainerNum { get; }

		protected abstract string ExpectedTransactionWithDirectConsolV3_3i9 { get; }

		protected abstract string ExpectedTransactionWithCoLoadConsolV3_3i10 { get; }

		protected abstract string ExpectedTransactionWithCoLoadConsolButNoVoyageNumber { get; }

		protected abstract string NettingCodes { get; }

		#region Generate Transactions
		protected string GenerateTransactionWithMultipleMawbsV3_3i2(ZString ledger)
		{
			CommonShipment shipment = Generator.GenerateShipment("HB101", Constants.TransportModes.Air, "AUSYD", "AUMEL");
			shipment.Consols.Add(Generator.GenerateConsol("MB101", Constants.TransportModes.Air));
			shipment.Consols.Add(Generator.GenerateConsol("MB102", Constants.TransportModes.Air));
			shipment.Consols.Add(Generator.GenerateConsol("MB103", Constants.TransportModes.Air));
			shipment.Consols.Add(Generator.GenerateConsol("MB104", Constants.TransportModes.Air));
			JobHeader job = Generator.GenerateJob(shipment.PK, "JH101");
			AccTransactionHeader trans = Generator.GenerateAccTransactionHeader(job.PK, ledger, TransactionTypes.CreditNote, DueDate, InvoiceDate, "TRAN101", "Description for 101", AUDCode, 101, CounterpartOrg.PK);
			Factory.Save();
			AccTransactionHeaderWithJobInfo transaction = GetRecordByTransactionNum(trans.AH_TransactionNum);
			PreMatchedDataLine line = NewPreMatchedDataLine(transaction);
			return line.ToString();
		}

		protected string GenerateTransactionWithMultipleHawbsV3_3i4(ZString ledger)
		{
			JASForwardingConsol consol = Generator.GenerateConsol("MB201", Constants.AgentType.Agent, "AUSYD", "AUMEL", Core.Constants.TransportModes.Sea, "QF201", new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 1));
			consol.Shipments.Add(Generator.GenerateShipment("HB201", Constants.TransportModes.Sea, "AUSYD", "AUMEL"));
			consol.Shipments.Add(Generator.GenerateShipment("HB202", Constants.TransportModes.Other, "AUMEL", "IDJKT"));
			consol.Shipments.Add(Generator.GenerateShipment("HB203", Constants.TransportModes.SeaAir, "IDJKT", "AUSYD"));
			AccTransactionHeader trans = Generator.GenerateAccTransactionHeader(consol.JK_UniqueConsignRef, ledger, TransactionTypes.Invoice, DueDate, InvoiceDate, "TRAN201", "Description for 201", AUDCode, 201, CounterpartOrg.PK);
			Factory.Save();
			AccTransactionHeaderWithJobInfo transaction = GetRecordByTransactionNum(trans.AH_TransactionNum);
			PreMatchedDataLine line = NewPreMatchedDataLine(transaction);
			return line.ToString();
		}

		protected string GenerateTransactionWithCategoryOorTV3_3i6(ZString ledger)
		{
			CommonShipment shipment = Generator.GenerateShipment("HB301", Constants.TransportModes.Road, "AUSYD", "AUBNE");
			shipment.Consols.Add(Generator.GenerateConsol("MB301", Constants.TransportModes.Air));
			JobHeader job = Generator.GenerateJob(shipment.PK, "J301");
			AccTransactionHeader trans = Generator.GenerateAccTransactionHeader(job.PK, ledger, TransactionTypes.Invoice, DueDate, InvoiceDate, "TRAN301", "Description for 301", AUDCode, 301, CounterpartOrg.PK);
			Factory.Save();
			AccTransactionHeaderWithJobInfo transaction = GetRecordByTransactionNum(trans.AH_TransactionNum);
			PreMatchedDataLine line = NewPreMatchedDataLine(transaction);
			return line.ToString();
		}

		protected string GenerateTransactionWithCategorySV3_3i7(ZString ledger, bool hasContainer)
		{
			CommonShipment shipment = Generator.GenerateShipment("HB401", Constants.TransportModes.SeaAir, "AUSYD", "AUBNE");
			JASForwardingConsol consol = Generator.GenerateConsol("MB401", Constants.TransportModes.Air);
			shipment.Consols.Add(consol);
			JobHeader job = Generator.GenerateJob(shipment.PK, "J401");
			if (hasContainer)
			{
				CommonContainer container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CN401";
			}

			AccTransactionHeader trans = Generator.GenerateAccTransactionHeader(job.PK, ledger, TransactionTypes.CreditNote, DueDate, InvoiceDate, "TRAN401", "Description for 401", AUDCode, 401, CounterpartOrg.PK);
			Factory.Save();
			AccTransactionHeaderWithJobInfo transaction = GetRecordByTransactionNum(trans.AH_TransactionNum);
			PreMatchedDataLine line = NewPreMatchedDataLine(transaction);
			return line.ToString();
		}

		protected string GenerateTransactionWithDirectConsolV3_3i9(ZString ledger)
		{
			JASForwardingConsol consol = Generator.GenerateConsol("MB501", Constants.AgentType.Direct, "AUSYD", Core.Constants.TransportModes.Air);
			CommonShipment shipment = Generator.GenerateShipment("HB501", Constants.TransportModes.Air, "AUSYD", "AUBNE");
			shipment.Consols.Add(consol);
			JobHeader job = Generator.GenerateJob(shipment.PK, "J501");
			AccTransactionHeader trans = Generator.GenerateAccTransactionHeader(job.PK, ledger, TransactionTypes.AdjustmentNote, DueDate, InvoiceDate, "TRAN501", "Description for 501", AUDCode, 501, 50.1M, 551.1, 1, CounterpartOrg.PK);
			Factory.Save();
			AccTransactionHeaderWithJobInfo transaction = GetRecordByTransactionNum(trans.AH_TransactionNum);
			PreMatchedDataLine line = NewPreMatchedDataLine(transaction);
			return line.ToString();
		}

		protected string GenerateTransactionWithCoLoadConsolV3_3i10(ZString ledger)
		{
			CommonShipment shipment = Generator.GenerateShipment("HB601", Constants.TransportModes.Air, "AUSYD", "AUBNE");
			shipment.Consols.Add(Generator.GenerateConsol("MB601", Constants.AgentType.CoLoad, "AUSYD", "AUBNE", Core.Constants.TransportModes.Air, "JV2222", new ZDateTime(2004, 2, 2), new ZDateTime(2004, 2, 2)));
			JobHeader job = Generator.GenerateJob(shipment.PK, "J601");
			AccTransactionHeader trans = Generator.GenerateAccTransactionHeader(job.PK, ledger, TransactionTypes.Invoice, DueDate, InvoiceDate, "TRAN601", "Description for 601", AUDCode, 601, 60.1M, 661.1M, 0.9M, CounterpartOrg.PK);
			Factory.Save();
			AccTransactionHeaderWithJobInfo transaction = GetRecordByTransactionNum(trans.AH_TransactionNum);
			PreMatchedDataLine line = NewPreMatchedDataLine(transaction);
			return line.ToString();
		}

		protected string GenerateTransactionWithCoLoadConsolButNoVoyageNumber(ZString ledger)
		{
			CommonShipment shipment = Generator.GenerateShipment("HB701", Constants.TransportModes.Air, "AUSYD", "AUBNE");
			shipment.Consols.Add(Generator.GenerateConsol("MB701", Constants.AgentType.CoLoad, "AUSYD", Core.Constants.TransportModes.Sea));
			JobHeader job = Generator.GenerateJob(shipment.PK, "J701");
			AccTransactionHeader trans = Generator.GenerateAccTransactionHeader(job.PK, ledger, TransactionTypes.Invoice, DueDate, InvoiceDate, "TRAN701", "Description for 701", AUDCode, 701, -70.1M, 630.9M, 0.5M, CounterpartOrg.PK);
			Factory.Save();
			AccTransactionHeaderWithJobInfo transaction = GetRecordByTransactionNum(trans.AH_TransactionNum);
			PreMatchedDataLine line = NewPreMatchedDataLine(transaction);
			return line.ToString();
		}

		#endregion
		protected override void SetUp()
		{
			base.SetUp();
			JASDataRegistry.Instance.UseJobInvoiceNumberAsPreMatchingInvoiceRef = false;
		}
		#endregion
	}
}
